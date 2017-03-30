//
// Created by Jorge Miguel Miranda on 28/12/2016.
//
#include <chrono>
#include <thread>
#include "AdCamiBluetoothDevice.h"
#include "AdCamiUtilities.h"

using AdCamiHardware::AdCamiBluetoothDevice;

int main(int argc, char **argv) {
    AdCamiBluetoothError error;
    std::string address = "5C:31:3E:00:57:A4"; //blood pressure
//    std::string address = "B4:99:4C:5A:B1:17"; //scale
    AdCamiBluetoothDevice device(address);

    PRINT_DEBUG("Trying to connect to device " << address << "...");

    if ((error = device.Connect()) != AdCamiBluetoothError::BT_OK) {
        PRINT_LOG("Error connecting to device = " << error);
        exit(EXIT_FAILURE);
    }
    else {
        PRINT_LOG("Connected with the device " << address);
    }

    std::this_thread::sleep_for(std::chrono::milliseconds(10000));

    if ((error = device.Disconnect()) != AdCamiBluetoothError::BT_OK) {
        PRINT_LOG("Error disconnecting from the device = " << error);
        exit(EXIT_FAILURE);
    }
    else {
        PRINT_LOG("Disconnected from the device " << address);
    }

    exit(EXIT_SUCCESS);
}

