//
// Created by Jorge Miguel Miranda on 27/11/2017.
//

#include <algorithm>
#include "AdCamiReadMeasurementsStrategy.h"
#include "AdCamiEventFactory.h"
#include "AdCamiHttpClient.h"
#include "AdCamiLogging.h"
#include "AdCamiJsonConverter.h"
#include "IAdCamiEventMeasurement.h"

using AdCamiCommunications::AdCamiHttpClient;
using AdCamiCommunications::AdCamiJsonConverter;
using AdCamiData::IAdCamiEventMeasurement;
using EnumHttpClientState = AdCamiCommunications::AdCamiHttpClient::EnumHttpClientState;
using EnumStorageError = AdCamiData::AdCamiEventsStorage::EnumStorageError;

const string AdCamiReadMeasurementsStrategy::kEndpointEvents = "/events";
const string AdCamiReadMeasurementsStrategy::kEndpointNewDevice = "/device/new";

AdCamiReadMeasurementsStrategy::AdCamiReadMeasurementsStrategy() :
        _configuration(AdCamiCommon::kAdCamiConfigurationFile),
        _storage(AdCamiCommon::kAdCamiEventsDatabase) {}

AdCamiReadMeasurementsStrategy::~AdCamiReadMeasurementsStrategy() {}

void AdCamiReadMeasurementsStrategy::DiscoveryEvent(std::unique_ptr <AdCamiBluetoothDevice> &&device,
                                                    const GVariant *event) {
    AdCamiBluetoothError error;
    int16_t rssi;
    bool connected = false, servicesResolved = false;
    string json;

    /* No need to check if the device exists, since that was done on the filtering function. */
    this->_storage.GetDevice(device.get());

    if (_GetPropertyValue<int16_t>(event, "RSSI", &rssi) == DBUS_OK) {
        LogInfoToDebug("Indication (RSSI) = " + std::to_string(rssi) +
                       " => " + g_variant_print(const_cast<GVariant *>(event), TRUE));
        if (device->NotificationsEnabled() == false) {
            auto lastEvent = AdCamiEventFactory().GetEvent(EnumEventType::Device, device->Address()).release();
            auto newEvent = AdCamiEventFactory().GetEvent(EnumEventType::Device, device->Address()).release();

            if (this->_storage.GetLastEvent(*device, lastEvent) == EnumStorageError::Ok) {
                /* If the time difference is smaller than the specified difference, ignore the event. This prevents an
                 * overload of events. */
                if (*newEvent - *lastEvent < kEventTimeDifference) {
                    return;
                }
            }

            /* Store event. */
            this->_storage.AddEvent(newEvent);
            /* Create JSON string for sending to remote endpoint. */
            AdCamiJsonConverter().ToJson(*device, &json);
            /* Set endpoint where the event will be sent off. */
            this->_configuration.Load();
            this->_SendMessageToEndpoint(this->_configuration.GetRemoteEndpoints(), kEndpointEvents, json);
        } else {
            if ((error = device->Connect()) != BT_OK) {
                Log<MessageType::Error>::ToMessages("Couldn't connect to device " +
                                                    device->Address() +
                                                    " [error " + std::to_string(error) + "]");
            }
            return;
        }

    } else if (_GetPropertyValue<bool>(event, "Connected", &connected) == DBUS_OK) {
        LogInfoToDebug("Connected = " + string(connected ? "true" : "false") +
                       " => " + g_variant_print(const_cast<GVariant *>(event), TRUE));
        if (connected == false) {
            vector < AdCamiEvent * > measurements;

            auto it = std::find_if(this->_devicesInAction.begin(),
                                   this->_devicesInAction.end(),
                                   [&device](AdCamiBluetoothDevice *d) { return *device.get() == *d; });

            if (it == this->_devicesInAction.end()) {
                return;
            }

            AdCamiBluetoothDevice *deviceInAction = *it;

            if ((error = deviceInAction->StopNotifications(&measurements)) != BT_OK) {
                Log<MessageType::Error>::ToMessages("Problem stopping notifications notifications for device " +
                                                    deviceInAction->Address() +
                                                    " [error = " + std::to_string(error) + "]");
            }

            if ((error = deviceInAction->Disconnect()) != BT_OK) {
                Log<MessageType::Error>::ToMessages("Problem disconnecting from device " +
                                                    deviceInAction->Address() +
                                                    " [error = " + std::to_string(error) + "]");
            } else {
                Log<MessageType::Info>::ToMessages("Disconnected from device " + deviceInAction->Address() + ".");
            }

            this->_devicesInAction.erase(it);

            if (measurements.size() > 0) {
                Log<MessageType::Info>::ToMessages("Received " +
                                                   std::to_string(measurements.size()) +
                                                   " measurement(s) from device " + device->Address());
                for (auto measurement : measurements) {
                    Log<MessageType::Info>::ToMessages("\t" +
                                                       string(*dynamic_cast<IAdCamiEventMeasurement<double> *>(measurement)));
                }

                this->_configuration.Load();
                /* Save measurements to database. */
                this->_storage.AddEvent(measurements);
                /* Create JSON string for sending to remote endpoint. */
                AdCamiJsonConverter().ToJson(measurements, this->_configuration.GetGatewayName(), &json);
                /* Set endpoint where the event will be sent off. */
                this->_SendMessageToEndpoint(this->_configuration.GetRemoteEndpoints(), kEndpointEvents, json);
            }
            /* Delete measurement objects. */
            std::for_each(measurements.begin(), measurements.end(), [](AdCamiEvent *m) { delete m; });
        }

        return;
    } else if (_GetPropertyValue<bool>(event, "ServicesResolved", &servicesResolved) == DBUS_OK) {
        LogInfoToDebug("ServicesResolved = " + string(servicesResolved ? "true" : "false") +
                       " => " + g_variant_print(const_cast<GVariant *>(event), TRUE));
        if (servicesResolved) {
            AdCamiBluetoothDevice *deviceInAction = device.get();
            if ((error = deviceInAction->StartNotifications()) != BT_OK) {
                Log<MessageType::Error>::ToMessages("Problem starting listening for notifications for " +
                                                    deviceInAction->Address() + " [error = " +
                                                    std::to_string(error) + "]");
            }
            this->_devicesInAction.push_back(device.release());

            return;
        }
    }

}

bool AdCamiReadMeasurementsStrategy::FilterDevice(const AdCamiBluetoothDevice &device) {
    bool authorized = true;

    /* Check if the device is authorized, i.e is not on the unknown devices list. */
    auto deviceIsUnknown = std::find_if(this->_unknownDevices.begin(),
                                        this->_unknownDevices.end(),
                                        [&device](string &d) { return d == device; });


    /* Check if the device exists. If not, send a request to the remote endpoints with the
     * information of the new device, so the user can decide what to do with it. */
    EnumStorageError error = this->_storage.GetDevice(device.Address(), nullptr);
    switch (error) {
        case EnumStorageError::Ok:
            /* The device might not be on the list. This case can happen if a device was added to the database, but it
             * was never turned on/paired/connected. */
            if (deviceIsUnknown != this->_unknownDevices.end()) {
                this->_unknownDevices.erase(deviceIsUnknown);
            }
            authorized = true;
            break;
        case EnumStorageError::DeviceNotFound:
            if (deviceIsUnknown == this->_unknownDevices.end()) {
                /* Create JSON string for sending to remote endpoint. */
                string json;
                AdCamiJsonConverter().ToJson(device, &json);
                /* Set endpoint where the event will be sent off. */
                this->_configuration.Load();
                this->_SendMessageToEndpoint(this->_configuration.GetRemoteEndpoints(), kEndpointNewDevice, json);
                this->_unknownDevices.push_back(device.Address());
            }
            authorized = false;
            break;
        default:
            break;
    }

    return authorized;
}

const string AdCamiReadMeasurementsStrategy::_GetDeviceDescription(const AdCamiBluetoothDevice &device) const {
    string deviceDescription;
    vector <string> deviceUuids = device.Uuids();
    auto it = AdCamiCommon::kKnownBluetoothUuids.end();
    auto itDeviceUuid = deviceUuids.begin();

    for (; itDeviceUuid != deviceUuids.end(); ++itDeviceUuid) {
        if ((it = AdCamiCommon::kKnownBluetoothUuids.find(*itDeviceUuid)) !=
            AdCamiCommon::kKnownBluetoothUuids.end()) {
            deviceDescription += it->second + "; ";
        }
    }
    if (deviceDescription.back() == ' ') {
        deviceDescription.erase(deviceDescription.end() - 2, deviceDescription.end());
    }

    return deviceDescription;
}


template<typename T>
EnumDBusResult AdCamiReadMeasurementsStrategy::_GetPropertyValue(const GVariant *objects,
                                                                 const char *name,
                                                                 T *value) {
    GVariant *child = g_variant_get_child_value(const_cast<GVariant *>(objects), 1);
    GVariant *variant = g_variant_lookup_value(child, name, nullptr);

    /* The variant object is not a PropertiesChanged. Try to lookup for a InterfacesAdded signal, but other
     * messages (for example ManufacturerData) might reach this point. */
    if (g_variant_is_of_type(child, G_VARIANT_TYPE_DICTIONARY)) {
        child = g_variant_lookup_value(child, BLUEZ5_DEVICE_INTERFACE, G_VARIANT_TYPE_DICTIONARY);
        if (child != nullptr) {
            variant = g_variant_lookup_value(child, name, nullptr);
        }
    }

    if (variant == nullptr) {
        return DBUS_ERROR_PROPERTY_NOT_FOUND;
    }

    *value = _CastPropertyValue<T>(variant);

    return DBUS_OK;
}

template<typename T = int16_t>
T AdCamiReadMeasurementsStrategy::_CastPropertyValue(GVariant *variant) {
    return g_variant_get_int16(variant);
}

template<>
bool AdCamiReadMeasurementsStrategy::_CastPropertyValue(GVariant *variant) {
    return g_variant_get_boolean(variant);
}

void AdCamiReadMeasurementsStrategy::_SendMessageToEndpoint(const vector <AdCamiUrl> &remoteEndpoints,
                                                            const string &endpoint,
                                                            const string &message) {
    AdCamiHttpClient client;
    AdCamiHttpData sendData(AdCamiJsonConverter::MimeType, message.size(), message.c_str());
    AdCamiHttpData response;
    EnumHttpClientState clientError;
    string endpointAddressFull;

    for (AdCamiUrl endpointAddress : remoteEndpoints) {
        endpointAddressFull = endpointAddress + endpoint;
        LogInfoToDebug("Sending to server " + endpointAddressFull);
        if ((clientError = client.Post(endpointAddressFull,
                                       &sendData,
                                       &response)) != EnumHttpClientState::OK) {
            Log<MessageType::Error>::ToMessages("Error sending HTTP request to " +
                                                endpointAddressFull +
                                                " (error " + std::to_string(clientError) + ").");
            return;
        } else {
            Log<MessageType::Info>::ToMessages(
                    "Request sent to POST " + endpointAddressFull + " with status code " +
                    response.Headers.GetValue(EnumHttpHeader::ResponseStatusCode));
            LogInfoToDebug("Sent to server " +
                           endpointAddressFull +
                           " with status code " +
                           response.Headers.GetValue(EnumHttpHeader::ResponseStatusCode));
        }
    }
}