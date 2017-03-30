//
// Created by Jorge Miguel Miranda on 28/01/2017.
//

#ifndef ADCAMID_ADCAMIEVENTFACTORY_H
#define ADCAMID_ADCAMIEVENTFACTORY_H

#include <memory>
#include "AdCamiBluetoothDefinitions.h"
//#include "AdCamiEvent.h"
#include "AdCamiEventBloodPressureMeasurement.h"
#include "AdCamiEventWeightMeasurement.h"
#include "AdCamiMeasurement.h"
#include "AdCamiUtilities.h"

//using AdCamiData::AdCamiEvent;
using AdCamiData::AdCamiEventBloodPressureMeasurement;
using AdCamiData::AdCamiEventWeightMeasurement;
using AdCamiData::AdCamiMeasurement;
using AdCamiUtilities::AdCamiBuffer;

namespace AdCamiData {

class AdCamiEventFactory {
private:
    using EnumMeasurementType = typename AdCamiData::AdCamiMeasurement<double>::EnumMeasurementType;

    template<typename TValue = double>
    static void _ParseGatt(const EnumMeasurementType &type,
                           const AdCamiBuffer<byte> &value,
                           AdCamiMeasurement<double> *measurement) {
        measurement->Type(type);

        switch (type) {
            case EnumMeasurementType::Diastolic: {
                /* Calculate unit */
                measurement->Unit(((value[0] & 0x0001) == 0) ? "mmHg" : "kPa");
                /* Calculate value */
                uint16_t rawValue = static_cast<uint16_t>(value[4] << 8) |
                                    static_cast<uint16_t>(value[3]);
                measurement->Value(static_cast<TValue>(rawValue));
                break;
            }
            case EnumMeasurementType::MeanArterialPressure: {
                /* Calculate unit */
                measurement->Unit(((value[0] & 0x0001) == 0) ? "mmHg" : "kPa");
                /* Calculate value */
                uint16_t rawValue = static_cast<uint16_t>(value[6] << 8) |
                                    static_cast<uint16_t>(value[5]);
                measurement->Value(static_cast<TValue>(rawValue));
                break;
            }
            case EnumMeasurementType::PulseRate: {
                /* Pulse Rate measurement is present. */
                if ((value[0] & 0x0004) != 0x0004) {
                    return;
                }
                measurement->Unit("bpm");
                uint16_t rawValue = static_cast<uint16_t>(value[8] << 8) |
                                    static_cast<uint16_t>(value[7]);
                measurement->Value(static_cast<TValue>(rawValue));
                break;
            }
            case EnumMeasurementType::Systolic: {
                /* Calculate unit */
                measurement->Unit(((value[0] & 0x0001) == 0) ? "mmHg" : "kPa");
                /* Calculate value */
                uint16_t rawValue = static_cast<uint16_t>(value[2] << 8) |
                                    static_cast<uint16_t>(value[1]);
                measurement->Value(static_cast<TValue>(rawValue));
                break;
            }
            case EnumMeasurementType::Weight: {
                size_t unit = value[0] & 0x0001;
                /* Calculate unit */
                measurement->Unit(unit == 0 ? "kg" : "lbs");
                /* Calculate value */
                uint16_t rawValue = static_cast<uint16_t>(value[2] << 8) |
                                    static_cast<uint16_t>(value[1]);
                if (unit == 0) {
                    measurement->Value(static_cast<TValue>(5 * rawValue * 0.001));
                } else {
                    measurement->Value(static_cast<TValue>(rawValue * 0.01));
                }
                break;
            }
            case EnumMeasurementType::UnknownMeasurement:
            default:
                return;
        }
    }

public:
    static std::unique_ptr <AdCamiEvent> Parse(const EnumGattCharacteristic &characteristic,
                                               const AdCamiBuffer<byte> &buffer) {
        map <EnumMeasurementType, AdCamiMeasurement<double>> measurements;
        AdCamiMeasurement<double> measurement;

        switch (characteristic) {
            case BloodPressureMeasurement: {
                std::unique_ptr <AdCamiEventBloodPressureMeasurement> event(new AdCamiEventBloodPressureMeasurement);
                _ParseGatt(EnumMeasurementType::Systolic, buffer, &measurement);
                event->SetMeasurement(measurement);
                _ParseGatt(EnumMeasurementType::Diastolic, buffer, &measurement);
                event->SetMeasurement(measurement);
                _ParseGatt(EnumMeasurementType::MeanArterialPressure, buffer, &measurement);
                event->SetMeasurement(measurement);
                _ParseGatt(EnumMeasurementType::PulseRate, buffer, &measurement);
                event->SetMeasurement(measurement);
                return std::move(event);
            }
            case WeightMeasurement: {
                std::unique_ptr <AdCamiEventWeightMeasurement> event(new AdCamiEventWeightMeasurement);
                _ParseGatt(EnumMeasurementType::Weight, buffer, &measurement);
                event->SetMeasurement(measurement);
                return std::move(event);
            }
            default: {
                break;
            }
        }

        return nullptr;
    }
};

} //namespace

#endif //ADCAMID_ADCAMIEVENTFACTORY_H
