//
// Created by Jorge Miguel Miranda on 05/01/2017.
//
#include "AdCamiBluetoothDefinitions.h"

const GattHandle kGattHandles[] = {
        GattHandle(Appearance, 0x0001, 0x0004),
        GattHandle(ManufacturerName, 0x0018, 0x0019),
        GattHandle(ModelNumber, 0x0018, 0x001b),
        GattHandle(SerialNumber, 0x0018, 0x001d),
        GattHandle(HardwareRevision, 0x0018, 0x001f),
        GattHandle(FirmwareRevision, 0x0018, 0x0021),
        GattHandle(SoftwareRevision, 0x0018, 0x0023),
        GattHandle(SystemId, 0x0018, 0x0025),
        GattHandle(BatteryLevel, 0x0029, 0x002a),
        GattHandle(BloodPressureMeasurement, 0x0010, 0x0011),
        GattHandle(WeightMeasurement, 0x0010, 0x0011),
        GattHandle(Invalid, 0x0000, 0x0000)
};

template<>
string GattHandle::CharacteristicUuid() {
    return AdCamiUtilities::IntToHexString<uint16_t>(this->_characteristicUuid);
}

template<>
const char *GattHandle::CharacteristicUuid() {
    return strdup(AdCamiUtilities::IntToHexString<uint16_t>(this->_characteristicUuid).c_str());
}

template<>
string GattHandle::AttributeHandle() {
    return AdCamiUtilities::IntToHexString<uint16_t>(this->_attributeHandle);
}

template<>
const char *GattHandle::AttributeHandle() {
    return strdup(AdCamiUtilities::IntToHexString<uint16_t>(this->_attributeHandle).c_str());
}

template<>
string GattHandle::CharacteristicHandle() {
    return AdCamiUtilities::IntToHexString<uint16_t>(this->_characteristicHandle);
}

template<>
const char *GattHandle::CharacteristicHandle() {
    return strdup(AdCamiUtilities::IntToHexString<uint16_t>(this->_characteristicHandle).c_str());
}

GattHandle GetGattHandle(const EnumGattCharacteristic &characteristic) {
    GattHandle handle;
    int i = 0;

    for (handle = kGattHandles[i];
         handle != characteristic &&
         handle != EnumGattCharacteristic::Invalid;
         handle = kGattHandles[i++]);

    return handle;
}

EnumGattCharacteristic GetCharacteristicFromUuid(const string &uuid) {
    string shortUuid = uuid.substr(0, uuid.find_first_of("-"));
    uint16_t shortUuidInt = strtoul(shortUuid.c_str(), nullptr, 16);

    return static_cast<EnumGattCharacteristic>(shortUuidInt);
}




