//
//  Created by Jorge Miguel Miranda on 30/11/16.
//

#ifndef AdCamiEventBloodPressureMeasurement_h
#define AdCamiEventBloodPressureMeasurement_h

#include <map>
#include <string>
#include <utility>
#include "AdCamiEvent.h"
#include "AdCamiMeasurement.h"
#include "IAdCamiEventMeasurement.h"

using AdCamiData::AdCamiMeasurement;
using std::map;
using std::string;
using EnumMeasurementType = AdCamiData::AdCamiMeasurement<double>::EnumMeasurementType;

namespace AdCamiData {

class AdCamiEventBloodPressureMeasurement : public AdCamiEvent, public IAdCamiEventMeasurement<double> {
private:
    AdCamiMeasurement<double> _systolic;
    AdCamiMeasurement<double> _diastolic;
    AdCamiMeasurement<double> _meanArterialPressure;
    AdCamiMeasurement<double> _pulseRate;

public:
    AdCamiEventBloodPressureMeasurement() :
            AdCamiEvent(EnumEventType::BloodPressure, "", ""),
            _systolic(EnumMeasurementType::Systolic, 0.0, ""),
            _diastolic(EnumMeasurementType::Diastolic, 0.0, ""),
            _meanArterialPressure(EnumMeasurementType::MeanArterialPressure, 0.0, ""),
            _pulseRate(EnumMeasurementType::PulseRate, 0.0, "bpm") {}

    AdCamiEventBloodPressureMeasurement(const string &timeStamp, const string &address,
                                        const double &systolicValue, const string &systolicUnit,
                                        const double &diastolicValue, const string &diastolicUnit,
                                        const double &meanArterialPressureValue, const string &meanArterialPressureUnit,
                                        const double &pulseRateValue) :
            AdCamiEvent(EnumEventType::BloodPressure, timeStamp, address),
            _systolic(EnumMeasurementType::Systolic,
                      systolicValue, systolicUnit),
            _diastolic(EnumMeasurementType::Diastolic,
                       diastolicValue, diastolicUnit),
            _meanArterialPressure(EnumMeasurementType::MeanArterialPressure,
                                  meanArterialPressureValue, meanArterialPressureUnit),
            _pulseRate(EnumMeasurementType::PulseRate,
                       pulseRateValue, "bpm") {}

    inline const AdCamiMeasurement<double> &Diastolic() const { return this->_diastolic; }

    inline const AdCamiMeasurement<double> &Systolic() const { return this->_systolic; }

    inline const AdCamiMeasurement<double> &MeanArterialPressure() const { return this->_meanArterialPressure; }

    inline const AdCamiMeasurement<double> &PulseRate() const { return this->_pulseRate; }

    inline AdCamiEventBloodPressureMeasurement &Diastolic(const double &value, const string &unit) {
        this->_diastolic.Value(value);
        this->_diastolic.Unit(unit);
        return *this;
    }

    inline AdCamiEventBloodPressureMeasurement &Systolic(const double &value, const string &unit) {
        this->_systolic.Value(value);
        this->_systolic.Unit(unit);
        return *this;
    }

    inline AdCamiEventBloodPressureMeasurement &MeanArterialPressure(const double &value, const string &unit) {
        this->_meanArterialPressure.Value(value);
        this->_meanArterialPressure.Unit(unit);
        return *this;
    }

    inline AdCamiEventBloodPressureMeasurement &PulseRate(const double &value, const string &unit) {
        this->_pulseRate.Value(value);
        this->_pulseRate.Unit(unit);
        return *this;
    }

    const map <string, AdCamiMeasurement<double>> Measurements() const {
        return map<string, AdCamiMeasurement<double>>(
                {
                        {"Systolic",               this->_systolic},
                        {"Diastolic",              this->_diastolic},
                        {"Mean Arterial Pressure", this->_meanArterialPressure},
                        {"Pulse Rate",             this->_pulseRate}
                });
    };

    inline const AdCamiMeasurement<double> &GetMeasurement(const EnumMeasurementType &type) const {
        switch (type) {
            case EnumMeasurementType::Diastolic: {
                return this->_diastolic;
            }
            case EnumMeasurementType::MeanArterialPressure: {
                return this->_meanArterialPressure;
            }
            case EnumMeasurementType::PulseRate: {
                return this->_pulseRate;
            }
            case EnumMeasurementType::Systolic: {
                return this->_systolic;
            }
            default: {
                break;
            }
        }
    }

    inline void SetMeasurement(const AdCamiMeasurement<double> &measurement) {
        switch (measurement.Type()) {
            case EnumMeasurementType::Diastolic: {
                this->_diastolic = measurement;
                break;
            }
            case EnumMeasurementType::MeanArterialPressure: {
                this->_meanArterialPressure = measurement;
                break;
            }
            case EnumMeasurementType::PulseRate: {
                this->_pulseRate = measurement;
                break;
            }
            case EnumMeasurementType::Systolic: {
                this->_systolic = measurement;
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
