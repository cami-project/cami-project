#ifndef AdCamiDaemon_AdCamiActions_h
#define AdCamiDaemon_AdCamiActions_h

#include <unistd.h>
#include <chrono>
#include <cstdint>
#include <random>
#include <thread>
#include <vector>
#include "AdCamiBluetooth5.h"
#include "AdCamiCommon.h"
#include "AdCamiConfiguration.h"
#include "AdCamiJsonConverter.h"
#include "AdCamiHttpCommon.h"
#include "AdCamiHttpData.h"
#include "AdCamiUrl.h"
#include "AdCamiEventsStorage.h"
#include "AdCamiJsonConverter.h"
#include "AdCamiUtilities.h"
#include "IAdCamiBluetooth.h"
#include "IAdCamiEventMeasurement.h"

using AdCamiCommunications::AdCamiHttpData;
using AdCamiCommunications::AdCamiJsonConverter;
using AdCamiCommunications::AdCamiUrl;
using AdCamiData::AdCamiEventBloodPressureMeasurement;
using AdCamiData::AdCamiEventsStorage;
using AdCamiData::AdCamiEventWeightMeasurement;
using AdCamiData::IAdCamiEventMeasurement;
using AdCamiHardware::AdCamiBluetooth5;
using AdCamiHardware::AdCamiBluetoothDevice;
using AdCamiHardware::IAdCamiBluetooth;
using EnumDeviceFilter = AdCamiData::AdCamiEventsStorage::EnumDeviceFilter;
using EnumHttpStatusCode = AdCamiCommunications::AdCamiHttpCommon::EnumHttpStatusCode;
using std::string;

/**
 * Class with all the actions that are executed when a request arrives.
 */
class AdCamiActionsServer {
public:
    /**
     * Add Bluetooth devices authorized to automatically connect and read.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return the resulting responseData object with all available devices
     */
    static EnumHttpStatusCode AddDevices(const AdCamiUrl &url,
                                         const AdCamiHttpData &requestData,
                                         AdCamiHttpData *responseData,
                                         void *data = nullptr);

    /**
     * Deletes Bluetooth devices from the database.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return
     */
    static EnumHttpStatusCode DeleteDevices(const AdCamiUrl &url,
                                            const AdCamiHttpData &requestData,
                                            AdCamiHttpData *responseData,
                                            void *data = nullptr);

    /**
     * Disables devices to receive and send Bluetooth notifications.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return
     */
    static EnumHttpStatusCode DisableDevices(const AdCamiUrl &url,
                                             const AdCamiHttpData &requestData,
                                             AdCamiHttpData *responseData,
                                             void *data = nullptr);

    /**
     * Discovers Bluetooth devices and pairs according to the users indications.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return
     */
    static EnumHttpStatusCode DiscoverAndPair(const AdCamiUrl &url,
                                              const AdCamiHttpData &requestData,
                                              AdCamiHttpData *responseData,
                                              void *data = nullptr);

    /**
     * Enables devices to receive and send Bluetooth notifications.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return
     */
    static EnumHttpStatusCode EnableDevices(const AdCamiUrl &url,
                                            const AdCamiHttpData &requestData,
                                            AdCamiHttpData *responseData,
                                            void *data = nullptr);

    /**
     * Function invoked when a client wants to retrieve all events.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return the resulting responseData object with the data read from the
     *	Bluetooth device
     */
    static EnumHttpStatusCode GetEvents(const AdCamiUrl &url,
                                        const AdCamiHttpData &requestData,
                                        AdCamiHttpData *responseData,
                                        void *data = nullptr);

    /**
     * Retrieve all trusted devices.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return the resulting responseData object with the data read from the
     *	Bluetooth device
     */
    static EnumHttpStatusCode GetPairedDevices(const AdCamiUrl &url,
                                               const AdCamiHttpData &requestData,
                                               AdCamiHttpData *responseData,
                                               void *data = nullptr);

    /**
     * Function to read measurement from a device.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return a HTTP code indicating the success of the reading
     */
    static EnumHttpStatusCode ReadDevice(const AdCamiUrl &url,
                                         const AdCamiHttpData &requestData,
                                         AdCamiHttpData *responseData,
                                         void *data = nullptr);

    /**
     * Function invoked when a user wants to get a measurement from a Bluetooth
	 * device.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return the resulting responseData object with the data read from the
	 *	Bluetooth device
     */
    static EnumHttpStatusCode SetCredentials(const AdCamiUrl &url,
                                             const AdCamiHttpData &requestData,
                                             AdCamiHttpData *responseData,
                                             void *data = nullptr);

    /**
     * Function invoked when a user wants to get a measurement from a Bluetooth
	 * device.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return the resulting responseData object with the data read from the
	 *	Bluetooth device
     */
    static EnumHttpStatusCode SetEndpoint(const AdCamiUrl &url,
                                          const AdCamiHttpData &requestData,
                                          AdCamiHttpData *responseData,
                                          void *data = nullptr);

/**
     * Set the name of the gateway.
     * @param url URL of the request
     * @param requestData additional data sent on the request
     * @param responseData data to be sent to the client in response to this request
     * @return Ok in case the command was successfully performed, Failed otherwise
     */
    static EnumHttpStatusCode SetGatewayName(const AdCamiUrl &url,
                                             const AdCamiHttpData &requestData,
                                             AdCamiHttpData *responseData,
                                             void *data = nullptr);
};

/**
 * This function creates a JSON error message, fills responseData with this message and, for convenience,
 * returns the HTTP status code given as function's parameters.
 * @param code
 * @param message
 * @param responseData
 * @return
 * */
EnumHttpStatusCode _FillResponseErrorMessage(const EnumHttpStatusCode &code,
                                             const string &message,
                                             AdCamiHttpData *responseData) {
    AdCamiJsonConverter converter;
    string json;

    PRINT_LOG(message)
    converter.Error(message, &json);
    responseData->SetData(json);
    responseData->SetMimeType(AdCamiJsonConverter::MimeType);

    return code;
};

/**
 * Check the parsed JSON value according to 1) a condition that defines if the value is valid and
 * 2) possible errors found during the parsing of the JSON payload. The function returns an HTTP
 * code that can be used to immediately return the error.
 * @param jsonError error returned by the JSON factory
 * @param fieldName name of the field to is being checked
 * @param invalidValueCondition a boolean condition that verifies if the value is valid
 * @param responseData data to be sent to the client with the error response
 * @return
 * */
EnumHttpStatusCode _CheckJsonValue(const AdCamiJsonConverter::EnumState &jsonError,
                                   const string &fieldName,
                                   const bool invalidValueCondition,
                                   AdCamiHttpData *responseData) {
    switch (jsonError) {
        case AdCamiJsonConverter::Ok: {
            if (invalidValueCondition) {
                return _FillResponseErrorMessage(EnumHttpStatusCode::Code400,
                                                 "Malformed JSON: invalid value for '" + fieldName + "'.",
                                                 responseData);
            }
            break;
        }
        case AdCamiJsonConverter::JsonMemberNotFound: {
            return _FillResponseErrorMessage(EnumHttpStatusCode::Code400,
                                             "Malformed JSON: could not find '" + fieldName + "'.",
                                             responseData);
        }
        case AdCamiJsonConverter::JsonMemberTypeDiffers: {
            return _FillResponseErrorMessage(EnumHttpStatusCode::Code400,
                                             "Malformed JSON: wrong type for value '" + fieldName + "'.",
                                             responseData);
        }
        default:
            break;
    }

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::AddDevices(const AdCamiUrl &url,
                                                   const AdCamiHttpData &requestData,
                                                   AdCamiHttpData *responseData,
                                                   void *data) {
    vector <string> devicesAddresses;
    vector <AdCamiBluetoothDevice> devices;
    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    AdCamiJsonConverter converter;
    string errorJsonMessage;

    if (converter.GetObjectValue<string>(requestData.GetDataAsString(),
                                         "devices",
                                         &devicesAddresses) == AdCamiJsonConverter::Ok) {
        for (string addr : devicesAddresses) {
            devices.push_back(AdCamiBluetoothDevice(addr)
                                      .Paired(false)
                                      .NotificationsEnabled(false));
        }
        storage.AddDevice(devices);
    } else {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code400,
                                         "Malformed JSON data: could not find 'devices'.",
                                         responseData);
    }

    return EnumHttpStatusCode::Code201;
}

EnumHttpStatusCode AdCamiActionsServer::DeleteDevices(const AdCamiUrl &url,
                                                      const AdCamiHttpData &requestData,
                                                      AdCamiHttpData *responseData,
                                                      void *data) {
    vector <string> devicesAddresses;
    vector <AdCamiBluetoothDevice> devices;
    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    AdCamiJsonConverter converter;
    string errorJsonMessage;

    if (converter.GetObjectValue<string>(requestData.GetDataAsString(),
                                         "devices",
                                         &devicesAddresses) == AdCamiJsonConverter::Ok) {
        for (string addr : devicesAddresses) {
            devices.push_back(AdCamiBluetoothDevice(addr));
        }
        storage.DeleteDevice(devices);
    } else {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code400,
                                         "Malformed JSON data: could not find 'devices'.",
                                         responseData);
    }

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::DiscoverAndPair(const AdCamiUrl &url,
                                                        const AdCamiHttpData &requestData,
                                                        AdCamiHttpData *responseData,
                                                        void *data) {
    IAdCamiBluetooth *bluetooth = static_cast<AdCamiBluetooth5 *>(data);
    vector <AdCamiBluetoothDevice> devices;
    AdCamiJsonConverter converter;
    string address, pinCode, jsonDevices, errorJsonMessage;
    int timeout = 60; //seconds
    AdCamiBluetoothError error;
    AdCamiJsonConverter::EnumState jsonError;
    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    EnumHttpStatusCode code;

    /* If the request has no payload, then try to pair with all found devices. */
    jsonError = converter.GetObjectValue<string>(requestData.GetDataAsString(),
                                                 "address",
                                                 &address);

    jsonError = converter.GetObjectValue<int>(requestData.GetDataAsString(),
                                              "timeout",
                                              &timeout);
    if ((code = _CheckJsonValue(jsonError, "timeout", timeout <= 0,
                                responseData)) != EnumHttpStatusCode::Code200) {
        return code;
    }

    /* Parse address from JSON payload. If it is not found, then continue, as a device might not
     * need a pin code or hint. */
    converter.GetObjectValue<string>(requestData.GetDataAsString(),
                                     "pincode",
                                     &pinCode);

    /* Start devices discovery. */
    if ((error = bluetooth->DiscoverDevices(&devices, static_cast<unsigned int>(timeout))) != BT_OK) {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code500,
                                         "Problem discovering devices [error = " + std::to_string(error) + "]",
                                         responseData);
    }

    string strDevices;
    for (auto d : devices) {
        PRINT_DEBUG(d);
        strDevices += d.Address() + ", ";
    }
    PRINT_LOG("Found " << devices.size() << " devices: " << strDevices)

    /* Pair devices. */
    AdCamiBluetoothDevice deviceToPair(address);
    auto it = devices.begin();
    if (!address.empty() && (it = find(devices.begin(), devices.end(), deviceToPair)) != devices.end()) {
        if (!it->PairedFromCache() && it->Connect() == BT_OK) {
            std::this_thread::sleep_for(std::chrono::milliseconds(5000));
            it->Disconnect();
        }
    }

    /* Store devices on the database. */
    AdCamiEventsStorage::EnumStorageError storageError;
    for (auto device : devices) {
        storageError = storage.GetDevice(device.Address(), &device);
        device.RefreshCacheProperties();

        if (storageError == AdCamiEventsStorage::DeviceNotFound) {
            storageError = storage.AddDevice(device);
        } else {
            storageError = storage.UpdateDevice(device);
        }

        if (storageError == AdCamiEventsStorage::Ok) {
            storage.AddEvent(new AdCamiEvent(AdCamiEvent::Device,
                                             AdCamiUtilities::GetDate(std::chrono::system_clock::now()),
                                             device.Address()));
        }
    }

    /* Create JSON string with discovery results. */
    converter.ToJson(devices, &jsonDevices);
    responseData->SetData(jsonDevices);


    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::DisableDevices(const AdCamiUrl &url,
                                                       const AdCamiHttpData &requestData,
                                                       AdCamiHttpData *responseData,
                                                       void *data) {
    vector <string> devicesAddresses;
    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    AdCamiJsonConverter converter;
    string invalidAddresses = "{ \"invalid\": [";
    string devicesDisabled, devicesNotDisabled;

    if (converter.GetObjectValue<string>(requestData.GetDataAsString(),
                                         "devices",
                                         &devicesAddresses) == AdCamiJsonConverter::Ok) {
        for (string addr : devicesAddresses) {
            AdCamiBluetoothDevice device(addr);
            if (storage.GetDevice(addr, &device) == AdCamiEventsStorage::Ok) {
                device.NotificationsEnabled(false);
                storage.UpdateDevice(device);
                if (devicesDisabled.compare("(none)") == 0)
                    devicesDisabled.clear();
                devicesDisabled.append(addr + ", ");
            } else {
                invalidAddresses.append("\"" + addr + "\",");
                devicesNotDisabled.append(addr + ", ");
            }
        }

        /* Remove last comma. */
        if (invalidAddresses.back() == ',') {
            invalidAddresses.erase(invalidAddresses.end() - 1);
        }
        invalidAddresses.append("] }");
        responseData->SetData(invalidAddresses);
    } else {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code400,
                                         "Malformed JSON data: could not find 'devices'.",
                                         responseData);
    }

    if (devicesDisabled.empty())
        devicesDisabled.append("(none)");
    if (devicesNotDisabled.empty())
        devicesNotDisabled.append("(none)");
    PRINT_LOG("\tDevices disabled = " << devicesDisabled << "");
    PRINT_LOG("\tDevices not disabled = " << devicesNotDisabled << "");

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::EnableDevices(const AdCamiUrl &url,
                                                      const AdCamiHttpData &requestData,
                                                      AdCamiHttpData *responseData,
                                                      void *data) {
    vector <string> devicesAddresses;
    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    AdCamiJsonConverter converter;
    string invalidAddresses = "{ \"invalid\": [";
    string devicesEnabled, devicesNotEnabled;

    if (converter.GetObjectValue<string>(requestData.GetDataAsString(),
                                         "devices",
                                         &devicesAddresses) == AdCamiJsonConverter::Ok) {
        for (string addr : devicesAddresses) {
            AdCamiBluetoothDevice device(addr);
            if (storage.GetDevice(addr, &device) == AdCamiEventsStorage::Ok) {
                device.NotificationsEnabled(true);
                storage.UpdateDevice(device);
                devicesEnabled.append(addr + ", ");
            } else {
                invalidAddresses.append("\"" + addr + "\",");
                devicesNotEnabled.append(addr + ", ");
            }
        }

        /* Remove last comma. */
        if (invalidAddresses.back() == ',') {
            invalidAddresses.erase(invalidAddresses.end() - 1);
        }
        invalidAddresses.append("] }");
        responseData->SetData(invalidAddresses);
    } else {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code400,
                                         "Malformed JSON data: could not find 'devices'.",
                                         responseData);
    }

    if (devicesEnabled.empty())
        devicesEnabled.append("(none)");
    if (devicesNotEnabled.empty())
        devicesNotEnabled.append("(none)");
    PRINT_LOG("\tDevices enabled = " << devicesEnabled << "");
    PRINT_LOG("\tDevices not enabled = " << devicesNotEnabled << "");

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::GetEvents(const AdCamiUrl &url,
                                                  const AdCamiHttpData &requestData,
                                                  AdCamiHttpData *responseData,
                                                  void *data) {
    AdCamiConfiguration configuration(AdCamiCommon::kAdCamiConfigurationFile);
    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    AdCamiJsonConverter converter;
    string jsonDevice, jsonBloodPressure, jsonWeight;
    vector <AdCamiEvent> events;
    vector <AdCamiEventBloodPressureMeasurement> eventsBloodPressure;
    vector <AdCamiEventWeightMeasurement> eventsWeight;

    configuration.Load();

    storage.GetEvents(&events);
    storage.GetEvents(&eventsBloodPressure);
    storage.GetEvents(&eventsWeight);

    converter.ToJson(events, configuration.GetGatewayName(), &jsonDevice);
    converter.ToJson(eventsBloodPressure, configuration.GetGatewayName(), &jsonBloodPressure);
    converter.ToJson(eventsWeight, configuration.GetGatewayName(), &jsonWeight);

    responseData->SetData("{\"events\":" + jsonDevice +
                          ",\"bloodpressure\":" + jsonBloodPressure +
                          ",\"weight\":" + jsonWeight + "}"
    );
    responseData->SetMimeType(AdCamiJsonConverter::MimeType);

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::GetPairedDevices(const AdCamiUrl &url,
                                                         const AdCamiHttpData &requestData,
                                                         AdCamiHttpData *responseData,
                                                         void *data) {
    vector <AdCamiBluetoothDevice> devices;

    AdCamiEventsStorage storage(AdCamiCommon::kAdCamiEventsDatabase);
    AdCamiJsonConverter converter;
    string json;

    storage.GetDevices(&devices, EnumDeviceFilter::Paired);

    converter.ToJson(devices, &json);

    responseData->SetData(json);
    responseData->SetMimeType(AdCamiJsonConverter::MimeType);

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::ReadDevice(const AdCamiUrl &url,
                                                   const AdCamiHttpData &requestData,
                                                   AdCamiHttpData *responseData,
                                                   void *data) {
    string address, jsonMeasurements;
    vector < AdCamiEvent * > measurements;
    int timeout = 0;
    AdCamiConfiguration configuration(AdCamiCommon::kAdCamiConfigurationFile);
    AdCamiJsonConverter converter;
    AdCamiBluetoothError error;
    AdCamiJsonConverter::EnumState jsonError;
    EnumHttpStatusCode code;

    PRINT_DEBUG("requestData = " << requestData.GetDataAsString())
    jsonError = converter.GetObjectValue<string>(requestData.GetDataAsString(),
                                                 "address",
                                                 &address);
    if ((code = _CheckJsonValue(jsonError, "address", address.empty(), responseData)) != EnumHttpStatusCode::Code200) {
        return code;
    }

    jsonError = converter.GetObjectValue<int>(requestData.GetDataAsString(),
                                              "timeout",
                                              &timeout);
    if ((code = _CheckJsonValue(jsonError, "timeout", timeout <= 0, responseData)) != EnumHttpStatusCode::Code200) {
        return code;
    }

    AdCamiBluetoothDevice device(address);
    device.RefreshCacheProperties();

    /* Get reading from the device. */
    if ((error = device.Connect()) != AdCamiBluetoothError::BT_OK) {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code500,
                                         "Problem connecting to " + address + " [error = " + std::to_string(error) +
                                         "]",
                                         responseData);
    }
#ifdef DEBUG
    else {
        PRINT_DEBUG("Connected to " << address);
    }
#endif

    if ((error = device.ReadMeasurementNotifications(&measurements, timeout)) != BT_OK) {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code500,
                                         "Problem getting notifications from " + address + " [error = " +
                                         std::to_string(error) + "]",
                                         responseData);
    }
#ifdef DEBUG
    else {
//        IAdCamiEventMeasurement<double> *event = nullptr;
//        for (auto measurement : measurements) {
//            if ((event = dynamic_cast<IAdCamiEventMeasurement<double> *>(measurement)) != nullptr) {
//                for (auto m : event->Measurements())
//                    PRINT_DEBUG(std::get<0>(m) << " = " << std::get<1>(m).Value() << " " << std::get<1>(m).Unit());
//            }
//        }

        for (auto measurement : measurements) {
            PRINT_LOG("\t" << *dynamic_cast<IAdCamiEventMeasurement<double>*>(measurement));
        }
    }
#endif

    if ((error = device.Disconnect()) != AdCamiBluetoothError::BT_OK) {
        return _FillResponseErrorMessage(EnumHttpStatusCode::Code500,
                                         "Problem disconnecting fom " + address + " [error = " +
                                         std::to_string(error) + "]",
                                         responseData);
    } else {
        PRINT_DEBUG("Disconnected from " << address);
    }

    converter.ToJson(measurements, configuration.GetGatewayName(), &jsonMeasurements);
    responseData->SetData(jsonMeasurements);
    responseData->SetMimeType(AdCamiJsonConverter::MimeType);

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::SetCredentials(const AdCamiUrl &url,
                                                       const AdCamiHttpData &requestData,
                                                       AdCamiHttpData *responseData,
                                                       void *data) {
    string username, password;
    string errorJsonMessage;

    if (AdCamiJsonConverter().GetObjectValue<string>(requestData.GetDataAsString(),
                                                     "username",
                                                     &username) == AdCamiJsonConverter::Ok &&
        AdCamiJsonConverter().GetObjectValue<string>(requestData.GetDataAsString(),
                                                     "password",
                                                     &password) == AdCamiJsonConverter::Ok) {
        AdCamiConfiguration configuration(AdCamiCommon::kAdCamiConfigurationFile);
        configuration.SetCredentials(username, password);
        configuration.Save();
        PRINT_LOG("\tCredentials set successfully.");
    } else {
        AdCamiJsonConverter().Error("Malformed JSON data: could not find 'username' or 'password'.",
                                    &errorJsonMessage);
        responseData->SetData(errorJsonMessage);
        responseData->SetMimeType(AdCamiJsonConverter::MimeType);
        PRINT_LOG("\tFailed to set credentials.");

        return EnumHttpStatusCode::Code400;
    }

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::SetGatewayName(const AdCamiUrl &url,
                                                       const AdCamiHttpData &requestData,
                                                       AdCamiHttpData *responseData,
                                                       void *data) {
    string gatewayName;
    string errorJsonMessage;

    if (AdCamiJsonConverter().GetObjectValue<string>(requestData.GetDataAsString(),
                                                     "name",
                                                     &gatewayName) == AdCamiJsonConverter::Ok) {
        PRINT_DEBUG("gatewayName = " << gatewayName)

        AdCamiConfiguration configuration(AdCamiCommon::kAdCamiConfigurationFile);
        configuration.SetGatewayName(gatewayName);
        configuration.Save();
        PRINT_LOG("\tHTTP code = 200")
        PRINT_LOG("\tGateway name set to \"" << gatewayName << "\"");
    } else {
        AdCamiJsonConverter().Error("Malformed JSON data: could not find 'name'.",
                                    &errorJsonMessage);
        responseData->SetData(errorJsonMessage);
        responseData->SetMimeType(AdCamiJsonConverter::MimeType);
        PRINT_LOG("\tFailed to set gateway name to " << gatewayName);

        return EnumHttpStatusCode::Code400;
    }

    return EnumHttpStatusCode::Code200;
}

EnumHttpStatusCode AdCamiActionsServer::SetEndpoint(const AdCamiUrl &url,
                                                    const AdCamiHttpData &requestData,
                                                    AdCamiHttpData *responseData,
                                                    void *data) {
    PRINT_DEBUG("PUT /management/endpoint request received")

    vector <AdCamiUrl> remoteEndpoints;
    string errorJsonMessage;

    if (AdCamiJsonConverter().GetObjectValue<AdCamiUrl>(requestData.GetDataAsString(),
                                                        "endpoint",
                                                        &remoteEndpoints) == AdCamiJsonConverter::Ok) {
        AdCamiConfiguration configuration(AdCamiCommon::kAdCamiConfigurationFile);
        configuration.SetRemoteEndpoints(remoteEndpoints);
        configuration.Save();
    } else {
        AdCamiJsonConverter().Error("Malformed JSON data: could not find 'endpoint'.",
                                    &errorJsonMessage);
        responseData->SetData(errorJsonMessage);
        responseData->SetMimeType(AdCamiJsonConverter::MimeType);

        return EnumHttpStatusCode::Code400;
    }

    return EnumHttpStatusCode::Code200;
}

#endif
