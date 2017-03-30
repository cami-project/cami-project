//
// Created by Jorge Miguel Miranda on 29/12/2016.
//

#include "AdCamiBluetoothDevice.h"

using AdCamiHardware::AdCamiBluetoothDevice;

int main(int argc, char **argv) {
//    string address = "5C:31:3E:00:57:A4"; //blood pressure
    string address = "B4:99:4C:5A:B1:17"; //weight scale
    AdCamiBluetoothDevice device(address);
    string uuid, manufacturer, model, serial, hwRevision, fwRevision, swRevision;
    uint16_t appearance;
    int batteryLevel;
    AdCamiBluetoothError error;

    if ((error = device.Connect()) != AdCamiBluetoothError::BT_OK) {
        PRINT_LOG("Problem connecting to " << address << " [error = " << error << "]");
        exit(EXIT_FAILURE);
    }


    auto _ReadCharacteristic = [&device, &uuid](const EnumGattCharacteristic &characteristic,
                                         const string &description,
                                         string *info) -> void {
        device.GetCharacteristic(characteristic, info, &uuid);
        std::cout << description << " = " << *info << " (" << uuid << ")" <<std::endl;
    };

    device.GetCharacteristic(Appearance, &appearance);
    std::cout << "Appearance = 0x" << std::hex << appearance << std::dec << std::endl;
    _ReadCharacteristic(ManufacturerName, "Manufacturer Name", &manufacturer);
    _ReadCharacteristic(ModelNumber, "Model Number", &model);
    _ReadCharacteristic(SerialNumber, "Serial Number", &serial);
    _ReadCharacteristic(HardwareRevision, "Hardware Revision", &hwRevision);
    _ReadCharacteristic(FirmwareRevision, "Software Revision", &fwRevision);
    _ReadCharacteristic(SoftwareRevision, "Firmware Revision", &swRevision);
    device.GetCharacteristic(BatteryLevel, &batteryLevel);
    std::cout << "Battery Level = " << batteryLevel << "%" << std::endl;

    if ((error = device.Disconnect()) != AdCamiBluetoothError::BT_OK) {
        PRINT_LOG("Problem disconnecting from " << address << " [error = " << error << "]");
        exit(EXIT_FAILURE);
    }

    exit(EXIT_SUCCESS);
}
