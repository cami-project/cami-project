//
// Created by Jorge Miguel Miranda on 28/01/2017.
//

#ifndef ADCAMID_IADCAMIEVENTMEASUREMENT_H
#define ADCAMID_IADCAMIEVENTMEASUREMENT_H

#include <memory>
#include "AdCamiMeasurement.h"

using AdCamiData::AdCamiMeasurement;
using EnumMeasurementType = AdCamiData::AdCamiMeasurement<double>::EnumMeasurementType;

namespace AdCamiData {

template<typename T = double>
class IAdCamiEventMeasurement {
public:
    virtual const map <string, AdCamiMeasurement<T>> Measurements() const = 0;

    virtual const AdCamiMeasurement<T> GetMeasurement(const EnumMeasurementType &type) const = 0;

    virtual void SetMeasurement(const AdCamiMeasurement<T> &measurement) = 0;

    operator std::string() const {
        string str;

        for (auto &measurement : this->Measurements()) {
            str += std::get<0>(measurement) + ": " +
                   string(std::get<1>(measurement)) + ", ";
        }

        return str;
    }

    friend std::ostream &operator<<(std::ostream &os, const IAdCamiEventMeasurement<T> &event) {
        for (auto &measurement : event.Measurements()) {
            os << std::get<0>(measurement) << ": " << std::get<1>(measurement) << ", ";
        }

        return os;
    }
};

} //namespace

#endif //ADCAMID_IADCAMIEVENTMEASUREMENT_H
