//
// Created by Jorge Miguel Miranda on 19/12/2016.
//
#include <memory>
#include <vector>
#include "AdCamiBluetooth5.h"
#include "AdCamiBluetoothDevice.h"
#include "AdCamiEvent.h"
#include "AdCamiJsonConverter.h"
#include "AdCamiUtilities.h"

using AdCamiCommunications::AdCamiJsonConverter;
using AdCamiData::AdCamiEvent;
using AdCamiHardware::AdCamiBluetooth5;
using AdCamiHardware::AdCamiBluetoothDevice;
using AdCamiHardware::IAdCamiBluetooth;
using std::vector;

int main(int argc, char **argv) {
    AdCamiBluetooth5 *bluetooth = new AdCamiBluetooth5();
    AdCamiBluetoothError error;
    vector <AdCamiBluetoothDevice> devices;
    vector <string> kAllowedDevices = {
            "5C:31:3E:00:57:A4",
            "B4:99:4C:5A:B1:17"
    };

    auto DiscoveryCallback = [&](std::unique_ptr <AdCamiBluetoothDevice> arg) {
        AdCamiBluetoothDevice *device = arg.get();
        vector < AdCamiEvent * > measurements;
        unsigned int timeout = 30;

        if (std::find(kAllowedDevices.begin(), kAllowedDevices.end(), device->Address()) == kAllowedDevices.end()) {
            return;
        }

        if (/*device->Init() != BT_OK ||*/ device->Connect() != BT_OK) {
            return;
        }

        PRINT_DEBUG("Connected to device " << device->Address() << "!")
        if ((error = device->ReadMeasurementNotifications(&measurements, timeout)) != BT_OK) {
            PRINT_LOG("Problem getting notifications from " << device->Address() << " [error = " << error << "]");
        } else {
            string json;
            PRINT_DEBUG(AdCamiCommunications::AdCamiJsonConverter().ToJson(measurements, "", &json))
        }
    };

    bluetooth->SetDiscoveryCallback(DiscoveryCallback);
    if ((error = bluetooth->Init()) != AdCamiBluetoothError::BT_OK) {
        PRINT_DEBUG("Problem opening Bluetooth communication [error = " << error << "]")
        exit(EXIT_FAILURE);
    }

    if ((error = bluetooth->StartDiscovery()) != BT_OK) {
        PRINT_DEBUG("Problem starting discovery [error = " << error << "]")
        exit(EXIT_FAILURE);
    } else {
        PRINT_DEBUG("Discovery started!")
    }

    std::cout << "Press any key to stop..." << std::endl;
    std::cin.get();

    if (bluetooth->StopDiscovery() != BT_OK) {
        PRINT_DEBUG("Problem stopping discovery [error = " << error << "]")
        exit(EXIT_FAILURE);
    } else {
        PRINT_DEBUG("Discovery stopped!")
    }

    exit(EXIT_SUCCESS);
}
