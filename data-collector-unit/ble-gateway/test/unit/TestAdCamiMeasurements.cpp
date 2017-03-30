#include <string>
#include <tuple>
#include "gtest/gtest.h"
#include "AdCamiBluetoothDefinitions.h"
#include "AdCamiMeasurement.h"
#include "AdCamiUtilities.h"

using std::string;
using std::tuple;
using AdCamiUtilities::AdCamiBuffer;
using AdCamiData::AdCamiMeasurement;
//using AdCamiData::AdCamiMeasurementCollection;
using TestMeasurement = struct _test_measurement {
    _test_measurement(const double &value, const string &unit) : Value(value), Unit(unit) {}
    double Value;
    string Unit;
};
using TestMeasurementResult = tuple<AdCamiBuffer<byte>, TestMeasurement>;
using TestBloodPressureMeasurement = struct _test_blood_pressure_measurement {
    TestMeasurement Systolic;
    TestMeasurement Diastolic;
    TestMeasurement MeanArterialPressure;
    TestMeasurement PulseRate;
};

namespace {

TEST(TestAdCamiMeasurements, ParseWeightMeasurement) {
    const vector<TestMeasurementResult> measurements = {
        std::make_tuple(AdCamiBuffer<byte>({ 0x02, 0xb4, 0x41, 0xe1, 0x07, 0x01, 0x0a, 0x12, 0x32, 0x21 }),
                        TestMeasurement(84.1, "kg")),
        std::make_tuple(AdCamiBuffer<byte>({ 0x02, 0xd8, 0x40, 0xe1, 0x07, 0x01, 0x0c, 0x0b, 0x26, 0x07 }),
                        TestMeasurement(83.0, "kg"))
    };

    AdCamiMeasurement<double> measurement(EnumMeasurementType::Weight);
    for (TestMeasurementResult m : measurements) {
        measurement.Parse(std::get<0>(m));
        ASSERT_STREQ(std::get<1>(m).Unit.c_str(), measurement.Unit().c_str());
        /* Compare values as non-floating point, due to rounding errors when converting from uint16 to double. */
        ASSERT_EQ(static_cast<size_t>(std::get<1>(m).Value * 1000), static_cast<size_t>(measurement.Value() * 1000));
    }
}

TEST(TestAdCamiMeasurements, ParseWeightMeasurementCollection) {
    const AdCamiBuffer <byte> buffer = { 0x02, 0xb4, 0x41, 0xe1, 0x07, 0x01, 0x0a, 0x12, 0x32, 0x21 };
    AdCamiMeasurementCollection<double> collection;

    collection.ParseAndAdd(EnumBluetoothDeviceType::WeightScale, buffer);
    AdCamiMeasurement<double> measurement = collection.GetMeasurement(EnumMeasurementType::Weight);

    ASSERT_STREQ("kg", measurement.Unit().c_str());
    /* Compare values as non-floating point, due to rounding errors when converting from uint16 to double. */
    ASSERT_EQ(static_cast<size_t>(84.1 * 1000), static_cast<size_t>(measurement.Value() * 1000));
}

TEST(TestAdCamiMeasurements, ParseBloodPressureMeasurement) {
    const AdCamiBuffer <byte> kBuffer = { 0x14, 0x7f, 0x00, 0x50, 0x00, 0x61, 0x00, 0x3b, 0x00, 0x00, 0x00 };
    //0x14, 0x83, 0x00, 0x5a, 0x00, 0x6a, 0x00, 0x3d, 0x00, 0x00, 0x00 [s:131, d:90, map:106, pr:61]
    //0x14, 0x84, 0x00, 0x55, 0x00, 0x68, 0x00, 0x38, 0x00, 0x00, 0x00 [s:132, d:85, map:104, pr:56]
    //0x14, 0x90, 0x00, 0x65, 0x00, 0x7d, 0x00, 0x3b, 0x00, 0x00, 0x00 [s:144, d:101, map: 125, pr:59]
    AdCamiMeasurement<double> systolic(EnumMeasurementType::Systolic);
    AdCamiMeasurement<double> diastolic(EnumMeasurementType::Diastolic);
    AdCamiMeasurement<double> pressure(EnumMeasurementType::MeanArterialPressure);
    AdCamiMeasurement<double> pulseRate(EnumMeasurementType::PulseRate);
    auto _Assert = [&](AdCamiMeasurement<double> &measurement, const double &value, const string &unit) {
        measurement.Parse(kBuffer);
        ASSERT_EQ(value, measurement.Value());
        ASSERT_STREQ(unit.c_str(), measurement.Unit().c_str());
    };

    _Assert(systolic, 127.0, "mmHg");
    _Assert(diastolic, 80.0, "mmHg");
    _Assert(pressure, 97.0, "mmHg");
    _Assert(pulseRate, 59.0, "bpm");
}

TEST(TestAdCamiMeasurements, ParseBloodPressureMeasurementCollection) {
    const AdCamiBuffer <byte> kBuffer = { 0x14, 0x7f, 0x00, 0x50, 0x00, 0x61, 0x00, 0x3b, 0x00, 0x00, 0x00 };
    AdCamiMeasurementCollection<double> collection;
    auto _Assert = [&](const EnumMeasurementType &type, const double &value, const string &unit) {
        AdCamiMeasurement<double> &measurement = collection.GetMeasurement(type);
        ASSERT_EQ(value, measurement.Value());
        ASSERT_STREQ(unit.c_str(), measurement.Unit().c_str());
    };

    collection.ParseAndAdd(EnumBluetoothDeviceType::BloodPressure, kBuffer);
    _Assert(EnumMeasurementType::Systolic, 127.0, "mmHg");
    _Assert(EnumMeasurementType::Diastolic, 80.0, "mmHg");
    _Assert(EnumMeasurementType::MeanArterialPressure, 97.0, "mmHg");
    _Assert(EnumMeasurementType::PulseRate, 59.0, "bpm");
}

}//namespace

int main(int argc, char **argv) {
    ::testing::InitGoogleTest(&argc, argv);

    return RUN_ALL_TESTS();
}