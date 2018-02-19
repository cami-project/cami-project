//
// Created by Jorge Miguel Miranda on 27/11/2017.
//

#ifndef ADCAMID_IADCAMIBLUETOOTHDISCOVERYSTRATEGY_H
#define ADCAMID_IADCAMIBLUETOOTHDISCOVERYSTRATEGY_H

#include "AdCamiBluetoothDevice.h"

using AdCamiHardware::AdCamiBluetoothDevice;

class IAdCamiBluetoothDiscoveryStrategy {
public:
    virtual void DiscoveryEvent(std::unique_ptr<AdCamiBluetoothDevice> &&device, const GVariant *event) = 0;

    virtual bool FilterDevice(const AdCamiBluetoothDevice &device) = 0;
};

#endif //ADCAMID_IADCAMIBLUETOOTHDISCOVERYSTRATEGY_H
