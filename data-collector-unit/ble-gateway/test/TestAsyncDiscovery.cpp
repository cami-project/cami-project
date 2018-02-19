//
// Created by Jorge Miguel Miranda on 19/12/2016.
//
#include <memory>
#include <vector>
#include "AdCamiBluetooth5.h"
#include "AdCamiBluetoothDevice.h"
#include "AdCamiUtilities.h"

using AdCamiHardware::AdCamiBluetooth5;
using AdCamiHardware::AdCamiBluetoothDevice;
using AdCamiHardware::IAdCamiBluetooth;
using std::vector;

int main(int argc, char **argv) {
    AdCamiBluetooth5 *bluetooth = new AdCamiBluetooth5();
    AdCamiBluetoothError error;
    vector <AdCamiBluetoothDevice> devices;

    auto DiscoveryCallback = [](std::unique_ptr <AdCamiBluetoothDevice> device) {
        PRINT_DEBUG("clbk device = " << device.get()->Address())
    };

    bluetooth->SetDiscoveryCallback(DiscoveryCallback);
    if ((error = bluetooth->Init()) != AdCamiBluetoothError::BT_OK) {
        PRINT_DEBUG("Problem opening Bluetooth communication [error = " << error << "]")
        exit(EXIT_FAILURE);
    }

    if ((error = bluetooth->StartDiscovery()) != BT_OK) {
        PRINT_DEBUG("Problem starting discovery [error = " << error << "]")
        delete bluetooth;

        exit(EXIT_FAILURE);
    } else {
        PRINT_DEBUG("Discovery started!")
    }

    std::cout << "Press any key to stop..." << std::endl;
    std::cin.get();

    if (bluetooth->StopDiscovery() != BT_OK) {
        PRINT_DEBUG("Problem stopping discovery [error = " << error << "]")
        delete bluetooth;

        exit(EXIT_FAILURE);
    } else {
        PRINT_DEBUG("Discovery stopped!")
    }

    delete bluetooth;

    exit(EXIT_SUCCESS);
}
