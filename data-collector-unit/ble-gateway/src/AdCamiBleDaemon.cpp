#include <csignal>
#include <cstdlib>
#include <fstream>
#include <iostream>
#include <sys/stat.h>
#include "AdCamiActionsServer.h"
#include "AdCamiBluetooth5.h"
#include "AdCamiCommon.h"
#include "AdCamiReadMeasurementsStrategy.h"
#include "AdCamiCommon.h"
#include "AdCamiConfiguration.h"
#include "AdCamiHttpServer.h"
#include "AdCamiLogging.h"

using AdCamiCommunications::AdCamiHttpServer;
using AdCamiCommunications::AdCamiRequest;
using AdCamiHardware::AdCamiBluetooth5;

static const int kAdCamiHttpPort = 60773;
static const int kAdCamiHttpSecurePort = 60774;
//static const int kAdCamiRemoteEndPointPort = 61773;

/* Global variable for HTTP server. */
static bool run = true;

/* Function to be executed when daemon receives an interrupt signal. */
static void _InterruptHandlers(int sig) {
    run = false;
}

static void _InterruptPipe(int sig) {}

void _SavePid() {
    unlink(PID_FILE);
    FILE *fp = fopen(PID_FILE, "w");
    if (fp == nullptr) {
        Log<MessageType::Error>::ToMessages("Error opening pid file for daemon");
        exit(EXIT_FAILURE);
    }

    pid_t pid = getpid();
    /* Write PID to file. */
    fprintf(fp, "%ld\n", (long) pid);
    Log<MessageType::Info>::ToMessages("Daemon created with PID " + std::to_string(static_cast<long>(pid)));
    fclose(fp);
}

int main(int argc, char **argv) {
    AdCamiHttpServer server(kAdCamiHttpPort);
    AdCamiHttpServer serverSecure(kAdCamiHttpSecurePort, EnumHttpServerConfiguration::Secure);
    AdCamiBluetooth5 *bluetooth = nullptr;
    AdCamiConfiguration configuration(AdCamiCommon::kAdCamiConfigurationFile);
    AdCamiBluetoothError error;
    /* Signal interrupts */
    struct sigaction sigHupAction = {0}, sigTermAction = {0}, sigPipeAction = {0};

    /* Load configuration file. */
    configuration.Load();

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
        Log<MessageType::Info>::ToMessages("Couldn't find directory " +
                                           string(AdCamiCommon::kAdCamiConfigurationDir) + ". Terminating.");
        exit(EXIT_FAILURE);
    }

    /* Initialize Bluetooth object. */
    bluetooth = new AdCamiBluetooth5(configuration.GetBluetoothAdapter(),
                                     new AdCamiReadMeasurementsStrategy());
    if ((error = bluetooth->Init()) != AdCamiBluetoothError::BT_OK) {
        switch (error) {
            case BT_ERROR_ADAPTER_NOT_FOUND:
                Log<MessageType::Error>::ToMessages("Couldn't find any Bluetooth adapter. Terminating.");
                break;
            default:
                Log<MessageType::Error>::ToMessages("Problem opening Bluetooth communication [error = " +
                                                    std::to_string(error) + "]. Terminating.");
                break;
        }
        exit(EXIT_FAILURE);
    }

    /* HTTP server(s) requests */
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

    /* Initialize signal interrupts. */
    sigemptyset(&sigHupAction.sa_mask);
    sigHupAction.sa_handler = _InterruptHandlers;
    sigemptyset(&sigTermAction.sa_mask);
    sigTermAction.sa_handler = _InterruptHandlers;
    sigemptyset(&sigPipeAction.sa_mask);
    sigPipeAction.sa_handler = _InterruptPipe;
    sigPipeAction.sa_flags = SA_RESTART;

    if (0 != sigaction(SIGHUP, &sigHupAction, nullptr))
        Log<MessageType::Error>::ToMessages("Failed to install SIGHUP handler: " + string(strerror(errno)));
    if (0 != sigaction(SIGINT, &sigTermAction, nullptr))
        Log<MessageType::Error>::ToMessages("Failed to install SIGINT handler: " + string(strerror(errno)));
    if (0 != sigaction(SIGTERM, &sigTermAction, nullptr))
        Log<MessageType::Error>::ToMessages("Failed to install SIGTERM handler: " + string(strerror(errno)));
    if (0 != sigaction(SIGPIPE, &sigPipeAction, nullptr))
        Log<MessageType::Error>::ToMessages("Failed to install SIGPIPE handler: " + string(strerror(errno)));

    /* Set discovery filter and start background Bluetooth discovery thread. */
    if ((error = bluetooth->SetDiscoveryFilter(AdCamiCommon::kAllowedBluetoothUuids)) != BT_OK) {
        Log<MessageType::Error>::ToMessages(
                "Problem setting discovery filter [error = " + std::to_string(error) + "].");
    }

    if ((error = bluetooth->StartDiscovery()) != BT_OK) {
        Log<MessageType::Error>::ToMessages("Problem starting discovery [error = " +
                                            std::to_string(error) +
                                            "]. Terminating.");
        exit(EXIT_FAILURE);
    } else {
        Log<MessageType::Info>::ToMessages("Bluetooth discovery started.");
    }

    /* Start HTTP server. */
    server.AddSyncRequestAction(requestActions);
    if (server.Start() != AdCamiHttpServer::Running) {
        Log<MessageType::Error>::ToMessages("Could not start HTTP server on port " +
                                            std::to_string(server.GetPort()) + ". Terminating.");
        exit(EXIT_FAILURE);
    } else {
        Log<MessageType::Info>::ToMessages("HTTP server started on port " + std::to_string(server.GetPort()) + ".");
    }

    /* Start secure HTTP server. */
    serverSecure.AddSyncRequestAction(requestActions);
    serverSecure.SetCertificate(AdCamiCommon::kAdCamiCertificatePemFile,
                                AdCamiCommon::kAdCamiCertificateKeyFile);
    if (serverSecure.Start() != AdCamiHttpServer::Running) {
        Log<MessageType::Error>::ToMessages("Could not start secure HTTP server on port " +
                                            std::to_string(server.GetPort()) + ". Terminating.");
        exit(EXIT_FAILURE);
    } else {
        Log<MessageType::Info>::ToMessages("Secure HTTP server started on port " +
                                           std::to_string(server.GetPort()) + ".");
    }

    _SavePid();

    while (run) { sleep(10); }

    /* Stop HTTP server. */
    server.Stop();
    Log<MessageType::Info>::ToMessages("HTTP server stopped.");
    serverSecure.Stop();
    Log<MessageType::Info>::ToMessages("Secure HTTP server stopped.");

    /* Stop Bluetooth discovery thread. */
    if ((error = bluetooth->StopDiscovery()) != BT_OK) {
        Log<MessageType::Error>::ToMessages("Problem stopping discovery [error = " + std::to_string(error) + "]");
        exit(EXIT_FAILURE);
    } else {
        Log<MessageType::Info>::ToMessages("Bluetooth discovery stopped.");
    }

    exit(EXIT_SUCCESS);
}
