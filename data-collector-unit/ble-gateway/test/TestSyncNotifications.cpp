//
// Created by Jorge Miguel Miranda on 09/01/2017.
//

#include "AdCamiBluetoothDevice.h"
#include "AdCamiEvent.h"
#include "AdCamiEventWeightMeasurement.h"
#include "AdCamiJsonConverter.h"
#include "AdCamiUtilities.h"

using AdCamiData::AdCamiEvent;
using AdCamiData::AdCamiEventWeightMeasurement;
using AdCamiHardware::AdCamiBluetoothDevice;

int main(int argc, char **argv) {
    string address = "5C:31:3E:00:57:A4"; //blood pressure
//    string address = "B4:99:4C:5A:B1:17"; //weight scale
    AdCamiBluetoothDevice device(address);
    vector<AdCamiEvent*> measurements;
    size_t notificationsTimeout = 10; //seconds
    AdCamiBluetoothError error;

//    if ((error = device.Init()) != AdCamiBluetoothError::BT_OK) {
//        PRINT_LOG("Problem initializing device with address " << address << " [error = " << error << "]");
//        exit(EXIT_FAILURE);
//    }

    if ((error = device.Connect()) != AdCamiBluetoothError::BT_OK) {
        PRINT_LOG("Problem connecting to " << address << " [error = " << error << "]");
        exit(EXIT_FAILURE);
    } else {
        PRINT_LOG("Connected to " << address);
    }

    if ((error = device.ReadMeasurementNotifications(&measurements, notificationsTimeout)) != BT_OK) {
        PRINT_LOG("Problem getting notifications from " << address << " [error = " << error << "]");
    } else {
        string json;
        PRINT_DEBUG(AdCamiCommunications::AdCamiJsonConverter().ToJson(measurements, "", &json))
    }

    exit(EXIT_SUCCESS);
}
