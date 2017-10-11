#include <csignal>
#include <cstdlib>
#include <fstream>
#include <iostream>
#include <sys/stat.h>
#include "AdCamiActionsServer.h"
#include "AdCamiBluetooth5.h"
#include "AdCamiCommon.h"
#include "AdCamiConfiguration.h"
#include "AdCamiEventsStorage.h"
#include "AdCamiHttpClient.h"
#include "AdCamiHttpServer.h"
#include "AdCamiJsonConverter.h"
#include "AdCamiUrl.h"
#include "AdCamiUtilities.h"

using AdCamiCommunications::AdCamiHttpClient;
using AdCamiCommunications::AdCamiHttpServer;
using AdCamiCommunications::AdCamiJsonConverter;
using AdCamiCommunications::AdCamiRequest;
using AdCamiData::AdCamiEventsStorage;
using AdCamiHardware::AdCamiBluetooth5;
using EnumDeviceFilter = AdCamiData::AdCamiEventsStorage::EnumDeviceFilter;
using EnumHttpClientState = AdCamiCommunications::AdCamiHttpClient::EnumHttpClientState;
using EnumHttpMethod = AdCamiCommunications::AdCamiHttpCommon::EnumHttpMethod;
using EnumHttpServerConfiguration = AdCamiCommunications::AdCamiHttpServer::EnumConfiguration;

static const int kAdCamiHttpPort = 60773;
static const int kAdCamiHttpSecurePort = 60774;
static const int kAdCamiRemoteEndPointPort = 61773;
//static const AdCamiUrl kMeasurementsEventsUrl("/events");

/* Global variable for HTTP server. */
AdCamiHttpServer server(kAdCamiHttpPort);
AdCamiHttpServer server_secure(kAdCamiHttpSecurePort, EnumHttpServerConfiguration::Secure);
AdCamiBluetooth5 *bluetooth = nullptr;
static bool run = true;

/* Function to be executed when daemon receives an interrupt signal. */
static void _InterruptHandlers(int sig) {
    run = false;
}

static void _InterruptPipe(int sig) {}

void _SavePid() {
    unlink(PID_FILE);
    FILE *fp = fopen(PID_FILE, "w");
    if (fp == NULL) {
        PRINT_ERROR("Error opening pid file for daemon");
        exit(EXIT_FAILURE);
    }

    pid_t pid = getpid();
    /* Write PID to file */
    fprintf(fp, "%ld\n", (long) pid);
    PRINT_LOG("Daemon created with PID " << (long) pid)
    fclose(fp);
}

int main(int argc, char **argv) {
    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    AdCamiBluetoothError error;

    auto DiscoveryCallback = [](std::unique_ptr <AdCamiBluetoothDevice> arg) -> void {
        AdCamiBluetoothDevice *device = arg.get();
        AdCamiBluetoothError error;
        AdCamiConfiguration configFile(AdCamiCommon::kAdCamiConfigurationFile);
        vector < AdCamiEvent * > measurements;
        string json;
        unsigned int timeout = 30;//seconds

        if (device->RefreshCacheProperties() == BT_OK && device->ConnectedFromCache() == false) {
            if (device->Connect() != BT_OK) {
                return;
            }
            PRINT_DEBUG("Connected to device " << device->Address() << "!");
        }
        else if (device->ConnectedFromCache() == true) {
            PRINT_DEBUG("Already reading measurements!");
            return;
        }

        PRINT_DEBUG("Reading measurements...");
        if ((error = device->ReadMeasurementNotifications(&measurements, timeout)) != BT_OK) {
            PRINT_LOG("Problem getting notifications from " << device->Address() << " [error = " << error << "]");
        } else if (measurements.size() > 0) {
            if ((error = device->Disconnect()) != BT_OK) {
                PRINT_DEBUG("Problem disconnecting from device " << device->Address() << " [error = " << error << "]");
            } else {
                PRINT_DEBUG("Disconnected from device " << device->Address() << ".");
            }

            /* Save measurements to database. */
            AdCamiJsonConverter converter;
            AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
            storage.AddEvent(measurements);

            converter.ToJson(measurements, configFile.GetGatewayName(), &json);

            /* Load configuration file to get the remote endpoint. */
            configFile.Load();

            AdCamiHttpClient client(kAdCamiRemoteEndPointPort);
            string endpointAddress = configFile.GetRemoteEndpoint();
            AdCamiHttpData sendData(AdCamiJsonConverter::MimeType, json.size(), json.c_str());
            AdCamiHttpData response;

            PRINT_DEBUG("sending to server " << endpointAddress);
            if (client.Post(endpointAddress/* + kMeasurementsEventsUrl*/,
                            &sendData,
                            &response) != EnumHttpClientState::OK) {
                PRINT_LOG("error sending HTTP packet...");
                return;
            } else {
                PRINT_DEBUG("status code = " << response.Headers.GetValue(EnumHttpHeader::ResponseStatusCode));
                PRINT_DEBUG(response.GetDataAsString());
            }
        }

//        if ((error = device->Disconnect()) != BT_OK) {
//            PRINT_DEBUG("Problem disconnecting from device " << device->Address() << " [error = " << error << "]");
//        }
//        else {
//            PRINT_DEBUG("Disconnected from device " << device->Address() << ".");
//        }
    };

    auto UpdateBluetoothDevicesFilter = [&storage](vector <AdCamiBluetoothDevice> *devices) -> void {
        storage.GetDevices(devices,
                           static_cast<EnumDeviceFilter>(EnumDeviceFilter::Paired |
                                                         EnumDeviceFilter::NotificationsEnabled));
    };

    /* Redirect output of std::cout to DEBUG_FILE file. */
    std::ofstream outDebugStream(DEBUG_FILE);
    std::cout.rdbuf(outDebugStream.rdbuf());
    /* Redirect output of std::cerr to ERROR_FILE file. */
    std::ofstream outErrorStream(ERROR_FILE);
    std::cerr.rdbuf(outErrorStream.rdbuf());
    /* Redirect output of std::clog to LOG_FILE file. */
    std::ofstream outLogStream(LOG_FILE);
    std::clog.rdbuf(outLogStream.rdbuf());

    /* Check if configuration directory on /etc is present. */
    struct stat info;
    if (stat(AdCamiCommon::kAdCamiConfigurationDir, &info) != 0) {
        PRINT_LOG("Couldn't find directory " << AdCamiCommon::kAdCamiConfigurationDir << ".  Exiting...");
        exit(EXIT_FAILURE);
    }

    /* Start Bluetooth discovery thread. */
    bluetooth = new AdCamiBluetooth5();
    if ((error = bluetooth->Init()) != AdCamiBluetoothError::BT_OK) {
        switch (error) {
            case BT_ERROR_ADAPTER_NOT_FOUND:
                PRINT_LOG("Couldn't find any Bluetooth adapter. Exiting...");
                break;
            default:
                PRINT_LOG("Problem opening Bluetooth communication [error = " << error << "]. Exiting...");
                break;
        }
        exit(EXIT_FAILURE);
    }
    bluetooth->SetDiscoveryCallback(DiscoveryCallback);
    bluetooth->SetUpdateDevicesFilterCallback(UpdateBluetoothDevicesFilter);

    /* Create and register HTTP requests. */
    vector <AdCamiRequest> requestActions = {
            /* Set list of BLE devices that are trusted. */
            AdCamiRequest("/device", EnumHttpMethod::POST, AdCamiActionsServer::AddDevices),
            /* Delete BLE devices. */
            AdCamiRequest("/device", EnumHttpMethod::DELETE, AdCamiActionsServer::DeleteDevices),
            /* Disable BLE devices to receive and send notifications. */
            AdCamiRequest("/device/disable", EnumHttpMethod::PUT, AdCamiActionsServer::DisableDevices),
            /* Enable BLE devices to receive and send notifications. */
            AdCamiRequest("/device/enable", EnumHttpMethod::PUT, AdCamiActionsServer::EnableDevices),
            /* Get a list of trusted BLE devices. */
            AdCamiRequest("/device/list", EnumHttpMethod::GET, AdCamiActionsServer::GetPairedDevices),
            /* Read a measurement from a device. */
            AdCamiRequest("/device/read", EnumHttpMethod::PUT, AdCamiActionsServer::ReadDevice),
            /* Discover Bluetooth devices. */
            AdCamiRequest("/discover", EnumHttpMethod::PUT, AdCamiActionsServer::DiscoverAndPair, bluetooth),
            /* Get list of known events on the gateway. */
            AdCamiRequest("/events", EnumHttpMethod::GET, AdCamiActionsServer::GetEvents),
            /* Set name of the gateway. */
            AdCamiRequest("/management/credentials", EnumHttpMethod::PUT, AdCamiActionsServer::SetCredentials),
            /* Set name of the gateway. */
            AdCamiRequest("/management/gateway", EnumHttpMethod::PUT, AdCamiActionsServer::SetGatewayName),
            /* Set name the endpoint to where events must be sent. */
            AdCamiRequest("/management/endpoint", EnumHttpMethod::PUT, AdCamiActionsServer::SetEndpoint)
    };
    server.AddSyncRequestAction(requestActions);
    server_secure.AddSyncRequestAction(requestActions);

    /* Initialize signal interrupts. */
    struct sigaction sigHupAction = {0}, sigTermAction = {0}, sigPipeAction = {0};

    sigemptyset(&sigHupAction.sa_mask);
    sigHupAction.sa_handler = _InterruptHandlers;
    sigemptyset(&sigTermAction.sa_mask);
    sigTermAction.sa_handler = _InterruptHandlers;
    sigemptyset(&sigPipeAction.sa_mask);
    sigPipeAction.sa_handler = _InterruptPipe;
    sigPipeAction.sa_flags = SA_RESTART;

    if (0 != sigaction(SIGHUP, &sigHupAction, nullptr))
        PRINT_LOG("Failed to install SIGHUP handler: " << strerror(errno));
    if (0 != sigaction(SIGINT, &sigTermAction, nullptr))
        PRINT_LOG("Failed to install SIGINT handler: " << strerror(errno));
    if (0 != sigaction(SIGTERM, &sigTermAction, nullptr))
        PRINT_LOG("Failed to install SIGTERM handler: " << strerror(errno));
    if (0 != sigaction(SIGPIPE, &sigPipeAction, nullptr))
        PRINT_LOG("Failed to install SIGPIPE handler: " << strerror(errno));

    /* Start Bluetooth background discovery thread. */
    if ((error = bluetooth->StartDiscovery()) != BT_OK) {
        PRINT_LOG("Problem starting discovery [error = " << error << "]. Exiting...");
        exit(EXIT_FAILURE);
    } else {
        PRINT_LOG("Bluetooth discovery started.");
    }

    /* Start HTTP server. */
    if (server.Start() != AdCamiHttpServer::Running) {
        PRINT_LOG("Could not start HTTP server on port " << server.GetPort() << ". Exiting...");
        exit(EXIT_FAILURE);
    } else {
        PRINT_LOG("HTTP server started on port " << server.GetPort() << ".");
    }

    /* Start secure HTTP server. */
    server_secure.SetCertificate(AdCamiCommon::kAdCamiCertificatePemFile,
                                 AdCamiCommon::kAdCamiCertificateKeyFile);
    if (server_secure.Start() != AdCamiHttpServer::Running) {
        PRINT_LOG("Could not start secure HTTP server on port " << server_secure.GetPort() << ". Exiting...");
        exit(EXIT_FAILURE);
    } else {
        PRINT_LOG("Secure HTTP server started on port " << server_secure.GetPort() << ".");
    }

    _SavePid();

    while (run) { sleep(10); }

    /* Stop HTTP server. */
    server.Stop();
    PRINT_LOG("HTTP server stopped.");
    server_secure.Stop();
    PRINT_LOG("Secure HTTP server stopped.");

    /* Stop Bluetooth discovery thread. */
    if ((error = bluetooth->StopDiscovery()) != BT_OK) {
        PRINT_LOG("Problem stopping discovery [error = " << error << "]");
        exit(EXIT_FAILURE);
    } else {
        PRINT_LOG("Bluetooth discovery stopped.");
    }

    exit(EXIT_SUCCESS);
}
