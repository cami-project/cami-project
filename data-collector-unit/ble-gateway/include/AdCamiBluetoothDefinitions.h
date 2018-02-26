//
// Created by Jorge Miguel Miranda on 29/12/2016.
//

#ifndef ADCAMIBLUETOOTHDEFINITIONS_H
#define ADCAMIBLUETOOTHDEFINITIONS_H

#include <array>
#include <cstring>
#include <string>
#include <utility>
#include "AdCamiUtilities.h"

using std::array;
using std::pair;

const string kDefaultBluetoothAdapter = "hci0";

/* Bluetooth Error Codes */
enum AdCamiBluetoothError : int {
    BT_OK = 0,
    BT_ERROR_OTHER = -1,
    BT_ERROR_ADAPTER_NOT_FOUND = -2,
    BT_ERROR_DEVICE_NOT_FOUND = -3,
    BT_ERROR_NO_PAIRED_DEVICES = -4,
    BT_ERROR_TIMEOUT = -5,
    BT_ERROR_HCI_CONNECTION = -6,
    BT_ERROR_NO_SCANNED_DEVICES = -7,
    BT_ERROR_DBUS_NO_CONNECTION = -8,
    BT_ERROR_DBUS_PROXY_CREATION = -9,
    BT_ERROR_DBUS_PROXY_METHOD = -10,
    BT_ERROR_DBUS_BAD_MESSAGE = -11,
//    BT_ERROR_DBUS_GENERIC = -12,
    BT_ERROR_DBUS_OPATH_IN_USE = -13,
    BT_ERROR_BAD_PARAMETERS = -14,
    BT_ERROR_ALREADY_PAIRED = -15,
    BT_ERROR_NOT_PAIRED = -16,
    BT_ERROR_NO_AGENT_XML = -17,
    BT_ERROR_NO_CHARACTERISTIC = -18,
    BT_ERROR_INVALID_MEASUREMENT = -19,
    BT_ERROR_INVALID_DEVICETYPE = -20,
    BT_ERROR_START_NOTIFICATIONS = -21,
    BT_ERROR_STOP_NOTIFICATIONS = -22,
    BT_ERROR_READING_NOTIFICATIONS = -23,
    BT_ERROR_DISCOVERY_RUNNING = -24,
    BT_ERROR_START_DISCOVERY = -25,
    BT_ERROR_STOP_DISCOVERY = -26,
    BT_ERROR_SET_DISCOVERY_FILTER = -27
};

enum EnumBluetoothDeviceType : uint32_t {
    BloodPressure = 0x00001810,
    WeightScale = 0x0000181d,
    UnknownDevice = 0x0000ffff
};

/**
 *
 */
enum EnumGattCharacteristic : uint32_t {
    Appearance = 0x00002a01,
    BatteryLevel = 0x00002a19,
    ManufacturerName = 0x00002a29,
    ModelNumber = 0x00002a24,
    SerialNumber = 0x00002a25,
    HardwareRevision = 0x00002a27,
    FirmwareRevision = 0x00002a26,
    SoftwareRevision = 0x00002a28,
    SystemId = 0x00002a23,
    BloodPressureMeasurement = 0x00002a35,
    WeightMeasurement = 0x00002a9d,
    Invalid = 0x00000000,
};

class GattHandle {
private:
    uint16_t _characteristicUuid;
    uint16_t _attributeHandle;
    uint16_t _characteristicHandle;

public:
    GattHandle() {}

    GattHandle(const uint16_t &characteristicUuid,
               const uint16_t &attributeHandle,
               const uint16_t &characteristicHandle) :
            _characteristicUuid(characteristicUuid),
            _attributeHandle(attributeHandle),
            _characteristicHandle(characteristicHandle) {}

    template<typename T = uint16_t>
    T CharacteristicUuid() { return this->_characteristicUuid; }

    template<typename T = uint16_t>
    T AttributeHandle() { return this->_attributeHandle; }

    template<typename T = uint16_t>
    T CharacteristicHandle() { return this->_characteristicHandle; }

    operator EnumGattCharacteristic() const {
        return static_cast<EnumGattCharacteristic>(this->_characteristicUuid);
    }
};

template<> string GattHandle::CharacteristicUuid();
template<> const char *GattHandle::CharacteristicUuid();
template<> string GattHandle::AttributeHandle();
template<> const char *GattHandle::AttributeHandle();
template<> string GattHandle::CharacteristicHandle();
template<> const char *GattHandle::CharacteristicHandle();

/**
 *
 * @param characteristic
 * @return
 */
GattHandle GetGattHandle(const EnumGattCharacteristic &characteristic);

/**
 *
 * @param uuid
 * @return
 */
EnumGattCharacteristic GetCharacteristicFromUuid(const string &uuid);

#endif //ADCAMIBLUETOOTHDEFINITIONS_H
