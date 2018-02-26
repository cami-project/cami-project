//
// Created by Jorge Miguel Miranda on 19/12/2016.
//
#include <vector>
#include "AdCamiBluetooth5.h"
#include "AdCamiBluetoothDevice.h"
#include "AdCamiUtilities.h"

using AdCamiHardware::AdCamiBluetooth5;
using AdCamiHardware::AdCamiBluetoothDevice;
using AdCamiHardware::IAdCamiBluetooth;
using std::vector;

int main(int argc, char** argv) {
    AdCamiBluetooth5 *bluetooth = new AdCamiBluetooth5();
    vector<AdCamiBluetoothDevice> devices;

    auto PrintFoundDevices = [&]() {
        PRINT_DEBUG("Found " << devices.size() << " devices.")
        for (auto d : devices) {
            PRINT_DEBUG(d);
        }
    };

    AdCamiBluetoothError error = bluetooth->Init();
    if (error != AdCamiBluetoothError::BT_OK) {
        PRINT_DEBUG("Problem opening Bluetooth communication [error = " << error << "]")
        exit(EXIT_FAILURE);
    }

    bluetooth->DiscoverDevices(&devices);
    PrintFoundDevices();

    delete bluetooth;

    return 0;
}
