//
//  Created by Jorge Miranda on 07/12/2016
//

#ifndef AdCamiDaemon_AdCamiBluetoothDevice_h
#define AdCamiDaemon_AdCamiBluetoothDevice_h

#include <algorithm>
#include <array>
#include <chrono>
#include <iostream>
#include <memory>
#include <sstream>
#include <vector>
#include "AdCamiBluetoothDefinitions.h"
#include "AdCamiEvent.h"
#include "AdCamiEventBloodPressureMeasurement.h"
#include "AdCamiEventFactory.h"
#include "AdCamiEventWeightMeasurement.h"
#include "AdCamiUtilities.h"
#include "dbus/dbus_helper_5.h"

using AdCamiData::AdCamiEventBloodPressureMeasurement;
using AdCamiData::AdCamiEvent;
using AdCamiData::AdCamiEventFactory;
using AdCamiData::AdCamiEventWeightMeasurement;
using AdCamiUtilities::AdCamiBuffer;
using std::array;
using std::string;
using std::vector;

namespace AdCamiHardware {
/**
 * 
 */
class AdCamiBluetoothDevice {
private:
    static constexpr int _kAddressNumberOctets = 6;
    static constexpr int _kBluetoothAddressLength = 12;
    static constexpr int _kBluetoothAddressStringLength = 17;
    static const string _kBluetoothBaseUuid;

public:
    /**
     * Constructor that receives as initial parameters the name of the device,
     * its MAC address and its specialization.
     * @param name name of the device
     * @param address Bluetooth address of the device. The string must be on the format '00:11:22:33:44:55'
     */
    AdCamiBluetoothDevice(const string &address);

    /**
     * Default destructor.
     */
    ~AdCamiBluetoothDevice() {}

    /**
     * This method allows us to connect to a Bluetooth device indicated by the address.
     * It is important to note that the AdCami system has no Input/Output capabilities.
     * @param address Address of the Bluetooth device we want to pair with.
     * @return 0 in case of success, negative value if an error occurred.
     */
    AdCamiBluetoothError Connect();

    /**
     * This method allows us to connect to a Bluetooth device indicated by the address.
     * It is important to note that the AdCami system has no Input/Output capabilities.
     * @param address Address of the Bluetooth device we want to pair with.
     * @return BT_OK in case of success, other value if an error occurred.
     */
    AdCamiBluetoothError Disconnect();

    /**
     * Initializes this device's information. It reads the device's information from the cache. These values are updated
     * once a connection is made to the device (@see Connect).
     * @return BT_OK in case of success, other value if an error occurred.
     */
    AdCamiBluetoothError RefreshCacheProperties();

    /**
     * Gets the MAC address of the device as a std::string.
     * @return a std::string with the MAC address in the format "00:11:22:33:44:55"
     */
    inline string Address() const { return this->_address; }

    /**
     * Gets the name of the device.
     * @return a string with the name of the device
     */
    inline string Name() const { return this->_name; }

    /**
     * Gets the flag value indicating if the device can receive notifications.
     * @return true if the device can receive notifications, false otherwise
     */
    inline bool NotificationsEnabled() const { return this->_notificationsEnabled; }

    /**
     * Gets if the device is trusted, i.e. it can be connected and get readings.
     * @return a boolean indicating if the device is trusted or not
     */
    inline bool Paired() const { return this->_paired; }

    /**
     * Gets the device type. This value corresponds to the Bluetooth characteristic of the device's measurements
     * (e.g. 0x1810 for blood pressure, 0x181d for weight, etc.)
     * @return the Bluetooth characteristic that represents this device
     */
    inline EnumBluetoothDeviceType Type() const {
        if (this->_deviceType == UnknownDevice) {
            return this->_DiscoverDeviceType();
        } else {
            return this->_deviceType;
        }
    }

    inline EnumBluetoothDeviceType Type() {
        if (this->_deviceType == UnknownDevice) {
            this->_deviceType = _DiscoverDeviceType();
        }

        return this->_deviceType;
    }

    /**
     *
     */
    inline vector <string> Uuids() const { return this->_servicesUuids; }

    bool ConnectedFromCache();

    string NameFromCache();

    std::unique_ptr <string> NameFromCache() const;

    bool PairedFromCache();

    bool PairedFromCache() const;

    bool ServicesResolvedFromCache();

    vector <string> UuidsFromCache();

    /**
     *
     * @tparam T
     * @param characteristic
     * @param value
     * @return
     */
    template<typename T = string>
    AdCamiBluetoothError GetCharacteristic(const EnumGattCharacteristic &characteristic,
                                           T *value,
                                           string *uuid = nullptr) const {
        GDBusConnection *busConnection = nullptr;
        char *adapterPath = nullptr, *devicePath = nullptr;
        string name;
        byte *characteristicValue;
        GattHandle handle = GetGattHandle(characteristic);

        /* If not paired, get regular DBus Connection. */
        if (DBusGetConnection(&busConnection) != DBUS_OK) {
            return BT_ERROR_DBUS_NO_CONNECTION;
        }

        /* Get the Bluetooth adapter path. */
        if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
            return BT_ERROR_ADAPTER_NOT_FOUND;
        }
        /* Get device path based on the Bluetooth address. */
        if (DBusDeviceGetObjectPath(this->_address.c_str(), adapterPath, &devicePath) != DBUS_OK) {
            return BT_ERROR_DEVICE_NOT_FOUND;
        }

        if (handle != EnumGattCharacteristic::Invalid) {
            if (DBusGattCharacteristicReadValue(devicePath,
                                                handle.AttributeHandle<const char *>(),
                                                handle.CharacteristicHandle<const char *>(),
                                                &characteristicValue, uuid) != DBUS_OK) {
                PRINT_DEBUG("Could not get characteristic 0x" << handle.CharacteristicHandle<string>() <<
                                                              " from service 0x" << handle.AttributeHandle<string>());
                return BT_ERROR_NO_CHARACTERISTIC;
            } else {
                if (value != nullptr) {
                    AdCamiUtilities::CastFromByte(characteristicValue, value);
                } else {
                    value = nullptr;
                    return BT_ERROR_NO_CHARACTERISTIC;
                }
            }
        }

        return BT_OK;
    }

    template<typename T = AdCamiEvent>
    AdCamiBluetoothError ReadMeasurementNotifications(vector <T> *measurements,
                                                      const unsigned int timeout = 60) const {
        GDBusConnection *busConnection = nullptr;
        char *adapterPath = nullptr, *devicePath = nullptr;
        EnumGattCharacteristic deviceCharacteristic, measurementCharacteristic;
        NotificationsCollection notifications;

        switch (this->_deviceType) {
            case WeightScale: {
                deviceCharacteristic = WeightMeasurement;
                break;
            }
            case BloodPressure: {
                deviceCharacteristic = BloodPressureMeasurement;
                break;
            }
            case UnknownDevice:
            default: {
                return BT_ERROR_INVALID_DEVICETYPE;
            }
        }

        /* If not paired, get regular DBus Connection. */
        if (DBusGetConnection(&busConnection) != DBUS_OK) {
            return BT_ERROR_DBUS_NO_CONNECTION;
        }

        /* Get the Bluetooth adapter path. */
        if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
            return BT_ERROR_ADAPTER_NOT_FOUND;
        }

        /* Get device path based on the Bluetooth address. */
        if (DBusDeviceGetObjectPath(this->_address.c_str(), adapterPath, &devicePath) != DBUS_OK) {
            return BT_ERROR_DEVICE_NOT_FOUND;
        }

        GattHandle handle = GetGattHandle(deviceCharacteristic);
        if (DBusDeviceReadNotifications(devicePath,
                                        handle.AttributeHandle<const char *>(),
                                        handle.CharacteristicHandle<const char *>(),
                                        &notifications, timeout) == DBUS_OK) {
            if (notifications.size() > 0) {
                for (auto n : notifications) {
                    measurementCharacteristic = GetCharacteristicFromUuid(n.UUID);
                    measurements->push_back(&AdCamiEventFactory::Parse(
                            measurementCharacteristic, n.Value).release()
                            ->Address(this->_address)
                            .TimeStamp(AdCamiUtilities::GetDate(std::chrono::system_clock::now())));
                }
            }
        }

        return BT_OK;
    }

    //TODO remove template from method
    template<typename TMeasurement = double>
    AdCamiBluetoothError StartNotifications() const {
        GDBusConnection *busConnection = nullptr;
        char *adapterPath = nullptr, *devicePath = nullptr;
        EnumGattCharacteristic deviceCharacteristic, measurementCharacteristic;
        NotificationsCollection notifications;

        switch (this->_deviceType) {
            case WeightScale: {
                deviceCharacteristic = WeightMeasurement;
                break;
            }
            case BloodPressure: {
                deviceCharacteristic = BloodPressureMeasurement;
                break;
            }
            case UnknownDevice:
            default: {
                return BT_ERROR_INVALID_DEVICETYPE;
            }
        }

        /* If not paired, get regular DBus Connection. */
        if (DBusGetConnection(&busConnection) != DBUS_OK) {
            return BT_ERROR_DBUS_NO_CONNECTION;
        }

        /* Get the Bluetooth adapter path. */
        if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
            return BT_ERROR_ADAPTER_NOT_FOUND;
        }
        /* Get device path based on the Bluetooth address. */
        if (DBusDeviceGetObjectPath(this->_address.c_str(), adapterPath, &devicePath) != DBUS_OK) {
            return BT_ERROR_DEVICE_NOT_FOUND;
        }

        GattHandle handle = GetGattHandle(deviceCharacteristic);
        if (DBusDeviceStartNotify(devicePath,
                                  handle.AttributeHandle<const char *>(),
                                  handle.CharacteristicHandle<const char *>()) != DBUS_OK) {
            return BT_ERROR_START_NOTIFICATIONS;
        }

        return BT_OK;
    }

    //TODO remove template from method
    template<typename TMeasurement = double>
    AdCamiBluetoothError StopNotifications() const {
        GDBusConnection *busConnection = nullptr;
        char *adapterPath = nullptr, *devicePath = nullptr;
        EnumGattCharacteristic deviceCharacteristic;
        NotificationsCollection notifications;

        switch (this->_deviceType) {
            case WeightScale: {
                deviceCharacteristic = WeightMeasurement;
                break;
            }
            case BloodPressure: {
                deviceCharacteristic = BloodPressureMeasurement;
                break;
            }
            case UnknownDevice:
            default: {
                return BT_ERROR_INVALID_DEVICETYPE;
            }
        }

        /* If not paired, get regular DBus Connection. */
        if (DBusGetConnection(&busConnection) != DBUS_OK) {
            return BT_ERROR_DBUS_NO_CONNECTION;
        }

        /* Get the Bluetooth adapter path. */
        if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
            return BT_ERROR_ADAPTER_NOT_FOUND;
        }
        /* Get device path based on the Bluetooth address. */
        if (DBusDeviceGetObjectPath(this->_address.c_str(), adapterPath, &devicePath) != DBUS_OK) {
            return BT_ERROR_DEVICE_NOT_FOUND;
        }

        GattHandle handle = GetGattHandle(deviceCharacteristic);
        if (DBusDeviceStopNotify(devicePath,
                                 handle.AttributeHandle<const char *>(),
                                 handle.CharacteristicHandle<const char *>()) != DBUS_OK) {
            return BT_ERROR_STOP_NOTIFICATIONS;
        }

        return BT_OK;
    }

    /**
     * Sets a new address for the Bluetooth device.
     * @param address to be set on the device
     * @return this object
     */
    inline AdCamiBluetoothDevice &Address(const string &address) {
        this->_address.assign(address);
        return *this;
    }

    /**
     * Sets a new name for the Bluetooth device.
     * @param name to be set on the device
     * @return this object
     */
    inline AdCamiBluetoothDevice &Name(const string &name) {
        this->_name.assign(name);
        return *this;
    }

    /**
     * Sets a flag indicating that this device can receive notifications.
     * @param enable true/false indicating if the device can receive notifications
     * @return this object
     */
    inline AdCamiBluetoothDevice &NotificationsEnabled(const bool &enable) {
        this->_notificationsEnabled = enable;
        return *this;
    }

    /**
     * Sets if the device is trusted to be connected and read.
     * @param trusted a boolean indicating if the device is trusted (true) or not (false)
     * @return this object
     */
    inline AdCamiBluetoothDevice &Paired(const bool &paired) {
        this->_paired = paired;
        return *this;
    }

    /**
     * Sets the device type.
     * @param type an EnumBluetoothDeviceType indicting the device's type
     * @return this object
     */
    inline AdCamiBluetoothDevice &Type(const EnumBluetoothDeviceType &type) {
        this->_deviceType = type;
        return *this;
    }

    /**
     * Sets a list of services UUIDs.
     * @param uuids list of UUIDs
     * @return this object
     */
    inline AdCamiBluetoothDevice &Uuids(const vector <string> &uuids) {
        this->_servicesUuids.clear();
        this->_servicesUuids = uuids;
        return *this;
    }

    /**
     * Gets a stream with the attributes of the object in the format "name; hdp; mac".
     * @return a stream with the attributes of the device
     */
    friend std::ostream &operator<<(std::ostream &os, const AdCamiBluetoothDevice &device);

    /**
     * Compares two devices to check if they are equal. They are equal if the MAC
     * address is equal.
     * @return true if the MACs are equal, false otherwise
     */
    friend bool operator==(const AdCamiBluetoothDevice &lhs, const AdCamiBluetoothDevice &rhs);

private:
    string _address;
    EnumBluetoothDeviceType _deviceType;
    string _name;
    vector <string> _servicesUuids;
    bool _connected;
    bool _notificationsEnabled;
    bool _paired;
    bool _servicesResolved;
//    bool _trusted;
    /* DBus object path that represents this device */
    char *_deviceObjectPath;

    EnumBluetoothDeviceType _DiscoverDeviceType() const;

    AdCamiBluetoothError _GetDeviceObjectPath(char **path, bool force = false) const;
};

} //namespace
#endif
