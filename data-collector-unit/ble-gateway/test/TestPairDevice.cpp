//
// Created by Jorge Miguel Miranda on 19/12/2016.
//
#include "AdCamiBluetooth5.h"
#include "AdCamiBluetoothDevice.h"
#include "AdCamiUtilities.h"

using AdCamiHardware::AdCamiBluetooth5;
using AdCamiHardware::AdCamiBluetoothDevice;
using AdCamiHardware::IAdCamiBluetooth;

int main(int argc, char **argv) {
    IAdCamiBluetooth *bluetooth = new AdCamiBluetooth5();
    AdCamiBluetoothError res;
    string address = "5C:31:3E:00:57:A4";
//    string address = "B4:99:4C:5A:B1:17";

    if ((res = bluetooth->Init()) != AdCamiBluetoothError::BT_OK) {
        PRINT_DEBUG("Problem opening Bluetooth communication [error = " << res << "]")
        exit(res);
    }

    if ((res = bluetooth->PairDevice(address)) != AdCamiBluetoothError::BT_OK) {
        PRINT_DEBUG("Error connecting to device = " << res)
        return res;
    } else {
        PRINT_DEBUG("Paired with the device!")
    }


    return 0;
}

