#include <chrono>
#include <ctime>
#include <string>
#include <tuple>
#include "gtest/gtest.h"
#include "AdCamiEvent.h"
#include "AdCamiUtilities.h"

using std::chrono::system_clock;
using std::string;
using std::tuple;
using AdCamiData::AdCamiEvent;
using EnumEventType = AdCamiData::AdCamiEvent::EnumEventType;

namespace {

class TestAdCamiEvent : public ::testing::Test {
protected:
    TestAdCamiEvent() : _dummyEvent(EnumEventType::Device, "", "AA:BB:CC:DD:EE:FF") {}

    virtual void SetUp() {
        _originalTimeStamp = std::chrono::system_clock::now();
        _dummyEvent.TimeStamp(AdCamiUtilities::GetDate(_originalTimeStamp));
    }

    virtual void TearDown() {}

    AdCamiEvent _dummyEvent;
    std::chrono::system_clock::time_point _originalTimeStamp;
};

TEST_F(TestAdCamiEvent, TimeStampDifference) {
    std::chrono::system_clock::time_point timeDiff = _originalTimeStamp + std::chrono::seconds(10);
    AdCamiEvent event(EnumEventType::Device, AdCamiUtilities::GetDate(timeDiff), "AA:BB:CC:DD:EE:FF");

    /* Assess 10 seconds difference*/
    ASSERT_TRUE((event - _dummyEvent) == 10);
}

}//namespace

int main(int argc, char **argv) {
    ::testing::InitGoogleTest(&argc, argv);

    return RUN_ALL_TESTS();
}