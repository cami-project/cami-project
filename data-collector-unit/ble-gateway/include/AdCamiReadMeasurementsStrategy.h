//
// Created by Jorge Miguel Miranda on 27/11/2017.
//

#ifndef ADCAMID_ADCAMIREADMEASUREMENTSSTRATEGY_H
#define ADCAMID_ADCAMIREADMEASUREMENTSSTRATEGY_H

#include <string>
#include <vector>
#include "AdCamiBluetoothDevice.h"
#include "AdCamiCommon.h"
#include "AdCamiConfiguration.h"
#include "AdCamiEventsStorage.h"
#include "AdCamiUrl.h"
#include "IAdCamiBluetoothDiscoveryStrategy.h"

using std::string;
using std::vector;
using AdCamiCommunications::AdCamiUrl;
using AdCamiData::AdCamiEventsStorage;
using AdCamiHardware::AdCamiBluetoothDevice;

class AdCamiReadMeasurementsStrategy : public IAdCamiBluetoothDiscoveryStrategy {
public:
    AdCamiReadMeasurementsStrategy();

    ~AdCamiReadMeasurementsStrategy();

    void DiscoveryEvent(std::unique_ptr<AdCamiBluetoothDevice> &&device, const GVariant *event);

    bool FilterDevice(const AdCamiBluetoothDevice &device);

private:
    static const unsigned int kEventTimeDifference = 60;//seconds
    static const string kEndpointEvents;
    static const string kEndpointNewDevice;

    AdCamiConfiguration _configuration;
    AdCamiEventsStorage _storage;
    vector <string> _unknownDevices;
    vector<AdCamiBluetoothDevice*> _devicesInAction;

    const string _GetDeviceDescription(const AdCamiBluetoothDevice &device) const;

    template<typename T>
    EnumDBusResult _GetPropertyValue(const GVariant *objects, const char *name, T *value);

    template<typename T = int16_t>
    T _CastPropertyValue(GVariant *variant);

    bool _CastPropertyValue(GVariant *variant);

    void _SendMessageToEndpoint(const vector <AdCamiUrl> &remoteEndpoints,
                                const string &endpoint,
                                const string &message);
};

#endif //ADCAMID_ADCAMIREADMEASUREMENTSSTRATEGY_H
