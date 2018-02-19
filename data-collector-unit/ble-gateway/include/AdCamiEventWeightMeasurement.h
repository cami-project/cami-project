//
//  Created by Jorge Miguel Miranda on 30/11/16.
//

#ifndef AdCamiEventWeightMeasurement_h
#define AdCamiEventWeightMeasurement_h

#include <string>
#include "AdCamiEvent.h"
#include "AdCamiMeasurement.h"
#include "IAdCamiEventMeasurement.h"

using AdCamiData::AdCamiEvent;
using AdCamiData::AdCamiMeasurement;
using std::string;
using EnumMeasurementType = AdCamiData::AdCamiMeasurement<double>::EnumMeasurementType;

namespace AdCamiData {

class AdCamiEventWeightMeasurement : public AdCamiEvent, public IAdCamiEventMeasurement<double> {
private:
    AdCamiMeasurement<double> _weight;

public:
    AdCamiEventWeightMeasurement() :
            AdCamiEvent(EnumEventType::Weight, "", ""),
            _weight(EnumMeasurementType::Weight, 0.0, "") {}

    AdCamiEventWeightMeasurement(const string &timeStamp, const string &address) :
            AdCamiEvent(EnumEventType::Weight, timeStamp, address),
            _weight(EnumMeasurementType::Weight, 0.0, "") {}

    AdCamiEventWeightMeasurement(const string &timeStamp, const string &address,
                                 const double &weight, const string &unit) :
            AdCamiEvent(EnumEventType::Weight, timeStamp, address),
            _weight(EnumMeasurementType::Weight, weight, unit) {}

    inline const AdCamiMeasurement<double> &Weight() const { return this->_weight; }

    inline const AdCamiEventWeightMeasurement &Weight(const double &value, const string &unit) {
        this->_weight.Value(value);
        this->_weight.Unit(unit);
        return *this;
    }

    const map <string, AdCamiMeasurement<double>> Measurements() const {
        return map<string, AdCamiMeasurement<double>>(
                {
                        {"Weight", this->_weight}
                });
    };

    inline const AdCamiMeasurement<double> GetMeasurement(const EnumMeasurementType &type) const {
        switch (type) {
            case EnumMeasurementType::Weight: {
                return this->_weight;
            }
            default: {
                return AdCamiMeasurement<double>(EnumMeasurementType::UnknownMeasurement);
            }
        }
    }

    inline void SetMeasurement(const AdCamiMeasurement<double> &measurement) {
        switch (measurement.Type()) {
            case EnumMeasurementType::Weight: {
                this->_weight = measurement;
                break;
            }
            default: {
                break;
            }

        }
    }
};

}//namespace
#endif
