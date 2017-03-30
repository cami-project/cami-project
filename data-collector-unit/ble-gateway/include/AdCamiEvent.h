//
//  Created by Jorge Miguel Miranda on 30/11/16.
//

#ifndef AdCamiEvent_h
#define AdCamiEvent_h

#include <map>
#include <string>

using std::map;
using std::string;

namespace AdCamiData {

class AdCamiEvent {
public:
    enum EnumEventType : int {
        Device = 1,
        BloodPressure = 2,
        Weight = 3
    };

protected:
    EnumEventType _type;
    string _timeStamp;
    string _address;

public:
    //TODO add reference on string
    AdCamiEvent(const EnumEventType &type, const string &timeStamp, const string address) :
            _type(type), _timeStamp(timeStamp), _address(address) {}

    virtual ~AdCamiEvent() {}

    inline const string &Address() const { return this->_address; }

    inline AdCamiEvent &Address(const string &address) {
        this->_address = address;
        return *this;
    }

    inline const string &TimeStamp() const { return this->_timeStamp; }

    inline AdCamiEvent &TimeStamp(const string &timeStamp) {
        this->_timeStamp = timeStamp;
        return *this;
    }

    inline const EnumEventType &Type() const { return this->_type; }

    inline AdCamiEvent &Type(const EnumEventType &type) {
        this->_type = type;
        return *this;
    }
};

}//namespace
#endif
