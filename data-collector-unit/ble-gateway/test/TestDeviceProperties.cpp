//
// Created by Jorge Miguel Miranda on 23/01/2017.
//

#include "AdCamiBluetoothDevice.h"

using AdCamiHardware::AdCamiBluetoothDevice;

int main(int argc, char **argv) {
//    string address = "5C:31:3E:00:57:A4"; //blood pressure
    string address = "B4:99:4C:5A:B1:17"; //weight scale
    AdCamiBluetoothDevice device(address);

    PRINT_LOG("connected = " << (device.ConnectedFromCache() ? "true" : "false"))
    PRINT_LOG("name = " << device.NameFromCache())
    PRINT_LOG("paired = " << (device.PairedFromCache() ? "true" : "false"))
    PRINT_LOG("services resolved = " << (device.ServicesResolvedFromCache() ? "true" : "false"))
    std::cout << "uuids = [";
    for (auto uuid : device.UuidsFromCache()) {
        std::cout << uuid << ", ";
    }
    std::cout << "]" << std::endl;

    exit(EXIT_SUCCESS);
}
