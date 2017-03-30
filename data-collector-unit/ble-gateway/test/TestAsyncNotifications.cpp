//
// Created by Jorge Miguel Miranda on 25/01/2017.
//

#include <iostream>
#include "AdCamiBluetoothDevice.h"
#include "AdCamiUtilities.h"

using AdCamiHardware::AdCamiBluetoothDevice;
//using AdCamiHardware::AdCamiMeasurement;
//using AdCamiData::AdCamiMeasurementCollection;

//void _NotificationReceived(AdCamiMeasurement &measurement) const {
//    PRINT_DEBUG("Measurement = " << mesurement)
//}

int main(int argc, char **argv) {
//    string address = "5C:31:3E:00:57:A4"; //blood pressure
    string address = "B4:99:4C:5A:B1:17"; //weight scale
    AdCamiBluetoothDevice device(address);
//    AdCamiMeasurementCollection<double> measurements;
    AdCamiBluetoothError res;

    if ((res = device.Init()) != AdCamiBluetoothError::BT_OK) {
        PRINT_LOG("Problem initializing device with address " << address << " [error = " << res << "]");
        exit(res);
    }

//    if ((res = device.Connect()) != AdCamiBluetoothError::BT_OK) {
//        PRINT_LOG("Problem connecting to " << address << " [error = " << res << "]");
//        exit(res);
//    } else {
//        PRINT_LOG("Connected to " << address);
//    }

    if ((res = device.StartNotifications()) != BT_OK) {
        PRINT_LOG("Problem starting notifications from " << address << " [error = " << res << "]");
    } else {
        for (auto measurement : measurements) {
            PRINT_DEBUG("value = " << measurement.Value() << " " << measurement.Unit())
        }
    }

    std::cout << "Press any key to stop..." << std::endl;
    std::cin.get();

    if ((res = device.StopNotifications()) != BT_OK) {
        PRINT_LOG("Problem stopping notifications from " << address << " [error = " << res << "]");
    } else {
        for (auto measurement : measurements) {
            PRINT_DEBUG("value = " << measurement.Value() << " " << measurement.Unit())
        }
    }

    //    if ((res = device.Disconnect()) != AdCamiBluetoothError::BT_OK) {
//        PRINT_LOG("Problem disconnecting to " << address << " [error = " << res << "]");
//        exit(res);
//    } else {
//        PRINT_LOG("Connected to " << address);
//    }

    return 0;
}
