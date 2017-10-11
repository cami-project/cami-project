#include "AdCamiBluetoothDevice.h"

namespace AdCamiHardware {

const string AdCamiBluetoothDevice::_kBluetoothBaseUuid = "-0000-1000-8000-00805F9B34FB";

AdCamiBluetoothDevice::AdCamiBluetoothDevice(const string &address) :
        _address(address), _deviceType(UnknownDevice), _notificationsEnabled(false), _paired(false),
        _deviceObjectPath(nullptr) {}

AdCamiBluetoothError AdCamiBluetoothDevice::Connect() {
    GDBusConnection *busConnection = nullptr;
    char *adapterPath, *devicePath;

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

    /* Connect to the device */
    if (DBusDeviceConnect(devicePath) != DBUS_OK) {
        DBusDeviceDisconnect(devicePath);
        return BT_ERROR_TIMEOUT;
    }

    return BT_OK;
}

AdCamiBluetoothError AdCamiBluetoothDevice::Disconnect() {
    GDBusConnection *busConnection = nullptr;
    char *adapterPath, *devicePath;

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

    /* Connect to the device */
    DBusDeviceDisconnect(devicePath);

    g_free(adapterPath);
    g_free(devicePath);

    return BT_OK;
}

bool AdCamiBluetoothDevice::ConnectedFromCache() {
    if (_GetDeviceObjectPath(&this->_deviceObjectPath) == BT_OK) {
        DBusDeviceConnectedProperty(this->_deviceObjectPath, &this->_connected);
    }

    return this->_connected;
}

string AdCamiBluetoothDevice::NameFromCache() {
    if (_GetDeviceObjectPath(&this->_deviceObjectPath) == BT_OK) {
        DBusDeviceNameProperty(this->_deviceObjectPath, &this->_name);
    }

    return this->_name;
}

std::unique_ptr <string> AdCamiBluetoothDevice::NameFromCache() const {
    char *devicePath = nullptr;
    string *name = new string();

    if (_GetDeviceObjectPath(&devicePath) == BT_OK) {
        DBusDeviceNameProperty(devicePath, name);
    }

    g_free(devicePath);

    return std::unique_ptr<string>(std::move(name));
}

bool AdCamiBluetoothDevice::PairedFromCache() {
    if (_GetDeviceObjectPath(&this->_deviceObjectPath) == BT_OK) {
        DBusDevicePairedProperty(this->_deviceObjectPath, &this->_paired);
    }

    return this->_paired;
}

bool AdCamiBluetoothDevice::PairedFromCache() const {
    char *devicePath = nullptr;
    bool paired = false;

    AdCamiBluetoothError error = _GetDeviceObjectPath(&devicePath);
    switch (error) {
        case BT_OK: {
            DBusDevicePairedProperty(devicePath, &paired);
            break;
        }
        default: {
            break;
        }

    }

    g_free(devicePath);

    return paired;
}

bool AdCamiBluetoothDevice::ServicesResolvedFromCache() {
    if (_GetDeviceObjectPath(&this->_deviceObjectPath) == BT_OK) {
        DBusDeviceServicesResolvedProperty(this->_deviceObjectPath, &this->_servicesResolved);
    }

    return this->_paired;
}

vector <string> AdCamiBluetoothDevice::UuidsFromCache() {
    if (_GetDeviceObjectPath(&this->_deviceObjectPath) == BT_OK) {
        DBusDeviceUuidsProperty(this->_deviceObjectPath, &this->_servicesUuids);
    }

    return this->_servicesUuids;
}

AdCamiBluetoothError AdCamiBluetoothDevice::RefreshCacheProperties() {
    AdCamiBluetoothError error;

    if ((error = _GetDeviceObjectPath(&this->_deviceObjectPath)) != BT_OK) {
        return error;
    }

    DBusDeviceConnectedProperty(this->_deviceObjectPath, &this->_connected);

    DBusDeviceNameProperty(this->_deviceObjectPath, &this->_name);

    DBusDevicePairedProperty(this->_deviceObjectPath, &this->_paired);

    DBusDeviceUuidsProperty(this->_deviceObjectPath, &this->_servicesUuids);

    DBusDeviceServicesResolvedProperty(this->_deviceObjectPath, &this->_servicesResolved);

    this->_deviceType = _DiscoverDeviceType();

    return BT_OK;
}

EnumBluetoothDeviceType AdCamiBluetoothDevice::_DiscoverDeviceType() const {
    const vector <EnumBluetoothDeviceType> kServicesToFind = {
            BloodPressure,
            WeightScale
    };
    auto it = kServicesToFind.begin();

    for (auto uuid : this->_servicesUuids) {
        if ((it = std::find(kServicesToFind.begin(),
                            kServicesToFind.end(),
                            GetCharacteristicFromUuid(uuid))) != kServicesToFind.end()) {
            return *it;
        }
    }

    return EnumBluetoothDeviceType::UnknownDevice;
}

AdCamiBluetoothError AdCamiBluetoothDevice::_GetDeviceObjectPath(char **devicePath, bool force) const {
    GDBusConnection *busConnection = nullptr;
    char *adapterPath;

    if (*devicePath != nullptr && force == false) {
        return BT_OK;
    }

    if (DBusGetConnection(&busConnection) != DBUS_OK) {
        return BT_ERROR_DBUS_NO_CONNECTION;
    }

    /* Get the Bluetooth adapter path. */
    if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
        free(adapterPath);
        return BT_ERROR_ADAPTER_NOT_FOUND;
    }
    /* Get device path based on the Bluetooth address. */
    if (DBusDeviceGetObjectPath(this->_address.c_str(), adapterPath, devicePath) != DBUS_OK) {
        return BT_ERROR_DEVICE_NOT_FOUND;
    }

    free(adapterPath);

    return BT_OK;
}

std::ostream &operator<<(std::ostream &os, const AdCamiBluetoothDevice &device) {
    os << "Name: " << device.Name() << ", ";
    os << "Address: " << std::hex << device.Address() << ", ";
    os << "UUIDs: [";
    for (string uuid : device.Uuids()) {
        os << uuid << (uuid == device.Uuids().back() ? "" : ", ");
    }
    os << "]";

    return os;
}

bool operator==(const AdCamiBluetoothDevice &lhs, const AdCamiBluetoothDevice &rhs) {
    return lhs._address == rhs._address;
}

} //namespace
