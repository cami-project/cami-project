//
//  Created by Jorge Miguel Miranda on 11/01/16.
//

#ifndef AdCamiDaemon_AdCamiMeasurement_h
#define AdCamiDaemon_AdCamiMeasurement_h

#include <cstdlib>
#include <cstring>
#include <map>
#include <vector>
#include "AdCamiUtilities.h"

using AdCamiUtilities::AdCamiBuffer;
using std::map;
using std::string;
using std::vector;

namespace AdCamiData {
/**
 * Class that represents a measure. A measure always has a value and an unit.
 */
template<typename TValue = double>
class AdCamiMeasurement {
public:
    enum EnumMeasurementType : size_t {
        Diastolic = 0,
        MeanArterialPressure,
        PulseRate,
        Systolic,
        Weight,
        UnknownMeasurement
    };

private:
    EnumMeasurementType _measurementType;
    /**
     * Value of the measure.
     */
    TValue _value;
    /**
     * Unit of the measure.
     */
    string _unit;

public:
    AdCamiMeasurement() : _measurementType(UnknownMeasurement), _unit("") {}

    AdCamiMeasurement(const EnumMeasurementType &measurementType) :
            _measurementType(measurementType), _unit("") {}

    AdCamiMeasurement(const EnumMeasurementType &measurementType, const TValue &value, const string &unit) :
            _measurementType(measurementType), _value(value), _unit(unit) {}

    AdCamiMeasurement(const AdCamiMeasurement &measurement) :
            _measurementType(measurement.Type()), _value(measurement.Value()), _unit(measurement.Unit()) {}

    ~AdCamiMeasurement() {}

    inline EnumMeasurementType Type() const { return this->_measurementType; }

    inline AdCamiMeasurement &Type(const EnumMeasurementType &type) {
        this->_measurementType = type;
        return *this;
    }

    inline string Unit() const { return this->_unit; }

    inline AdCamiMeasurement &Unit(const string &unit) {
        this->_unit = unit;
        return *this;
    }

    inline TValue Value() const { return this->_value; }

    inline AdCamiMeasurement &Value(const TValue &value) {
        this->_value = value;
        return *this;
    }

    /**
     * Gets a stream with the attributes of the object in the format "<value>, <unit>".
     * @return a stream with the attributes of the device
     */
    friend std::ostream &operator<<(std::ostream &os, const AdCamiMeasurement<TValue> &measurement) {
        os << measurement.Value() << " " << measurement.Unit();

        return os;
    }
};

} //namespace
#endif
