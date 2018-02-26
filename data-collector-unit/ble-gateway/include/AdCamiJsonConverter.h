//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiJsonConverter_h
#define AdCamiDaemon_AdCamiJsonConverter_h

#include <iostream>
#include <vector>
#include "AdCamiBluetoothDevice.h"
#include "AdCamiConfiguration.h"
#include "AdCamiEvent.h"
#include "AdCamiEventBloodPressureMeasurement.h"
#include "AdCamiEventWeightMeasurement.h"
#include "libjson/libjson.h"

using AdCamiHardware::AdCamiBluetoothDevice;
using AdCamiData::AdCamiEvent;
using AdCamiData::AdCamiEventBloodPressureMeasurement;
using AdCamiData::AdCamiEventWeightMeasurement;
using std::string;
using std::vector;
using EnumEventType = AdCamiData::AdCamiEvent::EnumEventType;

namespace AdCamiCommunications {

/**
 */
class AdCamiJsonConverter {
public:
    enum EnumState : int {
        Ok = 0,
        InvalidArgument = -1,
        AttributeNotFound = -2,
        JsonMemberNotFound = -3,
        JsonMemberTypeDiffers = -4,
        InvalidJsonString = -5
    };

    static const std::string MimeType;

    AdCamiJsonConverter();

    /**
     *
     */
    string *Error(const string &message, string *json);

    /**
     * Get the value from a JSON array object.
     * @tparam T type of the value. Only int, float, string and bool are supported.
     * @param json JSON string to be searched
     * @param name name of the object to look for
     * @param value object where the JSON value must be stored
     * @return AdCamiJsonConverter::EnumState::Ok if the object was found, or
     *  AdCamiJsonConverter::EnumState::JsonMemberNotFound if the object does not exist
     */
    template<typename T>
    EnumState GetObjectValue(const string &json, const string &name, T *value) {
        if (libjson::is_valid(json) == false) {
            PRINT_DEBUG("Invalid JSON string.")
            return AdCamiJsonConverter::EnumState::InvalidJsonString;
        }

        JSONNode node = libjson::parse(json);
        JSONNode::const_iterator it = node.begin();
        if (_GetMember(node, name, &it) == EnumState::Ok) {
            if (this->_ObjectType<T>(it, value)) {
                PRINT_DEBUG("'" << name << "' object found with value '" << *value << "'")
                return AdCamiJsonConverter::EnumState::Ok;
            }
            else {
                return AdCamiJsonConverter::EnumState::JsonMemberTypeDiffers;
            }
        } else {
            PRINT_DEBUG("'" << name << "' object not found")
            return AdCamiJsonConverter::EnumState::JsonMemberNotFound;
        }
    }

    /**
     * Get the values from a JSON array object.
     * @tparam T type of the values of the array. Only int, float, string and bool are supported.
     * @param json JSON string to be searched
     * @param name name of the object to look for
     * @param values vector where the JSON array values must be stored
     * @return AdCamiJsonConverter::EnumState::Ok if the object was found, or
     *  AdCamiJsonConverter::EnumState::JsonMemberNotFound if the object does not exist
     */
    template<typename T>
    EnumState GetObjectValue(const string &json, const string &name, vector <T> *values) {
        if (libjson::is_valid(json) == false) {
            PRINT_DEBUG("Invalid JSON string.")
            return AdCamiJsonConverter::EnumState::InvalidJsonString;
        }

        JSONNode node = libjson::parse(json);
        JSONNode::const_iterator it = node.begin();
        if (_GetMember(node, name, &it) == EnumState::Ok) {
            if (it->type() != JSON_ARRAY) {
                PRINT_DEBUG("'" << name << "' is not a JSON array of values.")
                return EnumState::JsonMemberNotFound;
            }
            for (auto v : *it) {
                T value;
                this->_ObjectType<T>(v, &value);
                values->push_back(value);
            }

            return AdCamiJsonConverter::EnumState::Ok;
        } else {
            PRINT_DEBUG("'" << name << "' object not found")
            return AdCamiJsonConverter::EnumState::JsonMemberNotFound;
        }
    }

    string &ToJson(const AdCamiBluetoothDevice &device, string *json);

    string &ToJson(const vector <AdCamiBluetoothDevice> &device, string *json);

    string &ToJson(const AdCamiEvent &event, string *json);

    string &ToJson(const AdCamiEvent *event, string *json);

    string &ToJson(const vector <AdCamiEvent> &event, const string &gatewayName, string *json);

    string &ToJson(const vector <AdCamiEvent*> &event, const string &gatewayName, string *json);

    string &ToJson(const AdCamiEventBloodPressureMeasurement &event, string *json);

    string &ToJson(const vector <AdCamiEventBloodPressureMeasurement> &event, const string &gatewayName, string *json);

    string &ToJson(const AdCamiEventWeightMeasurement &event, string *json);

    string &ToJson(const vector <AdCamiEventWeightMeasurement> &event, const string &gatewayName, string *json);

    string &ErrorMessage(const string &message, string *json);

private:
    using JSONNodeType = char;

    JSONNode::const_iterator _GetMember(const string &member, const JSONNode &node);

    /**
     * Searches on a parsed JSON string for a member.
     * @param node JSON node where the object will be searched
     * @param member name of the object's member to search on the JSON string
     * @param value iterator that points to the found object
     * @return Ok if the member is found, JsonValueNotFound otherwise
     */
    AdCamiJsonConverter::EnumState _GetMember(const JSONNode &node,
                                              const string &member,
                                              JSONNode::const_iterator *value) const;

    /**
     * Searches on a parsed JSON string for a member with a specific type. If a member is
     * found but its type the differ, the function will return that the member was not found.
     * @param node JSON node where the object will be searched
     * @param member name of the object's member to search on the JSON string
     * @param nodeType type of the node to be searchd for
     * @param value iterator that points to the found object
     * @return Ok if the member is found, JsonMemberNotFound if the member wasn't found or
     *  JsonMemberTypeDiffers if a member with the name is found but its type differs from
     *  the requested
     */
    AdCamiJsonConverter::EnumState _GetMember(const JSONNode &node,
                                              const string &member,
                                              const JSONNodeType &nodeType,
                                              JSONNode::const_iterator *value) const;

    /**
     * Searches on a parsed JSON array for an object with a specified member name.
     * @param node a JSON_ARRAY node where the object will be searched
     * @param member name of the object's member to search on the JSON string
     * @param value iterator that points to the found object
     * @return Ok if the member is found, JsonMemberNotFound if the member wasn't found, or
     *  JsonMemberTypeDiffers if the node argument is not a JSON array
     */
    AdCamiJsonConverter::EnumState _GetMemberFromArray(const JSONNode &node,
                                                       const string &member,
                                                       JSONNode::const_iterator *value);

    template<typename T>
    bool _ObjectType(JSONNode::const_iterator &it, T *value) {
        *value = it->as_string();
        return it->type() == JSON_STRING;
    }

    template<typename T>
    bool _ObjectType(JSONNode &node, T *value) {
        *value = node.as_string();
        return node.type() == JSON_STRING;
    }

    template<typename T>
    bool _ObjectType(JSONNode::const_iterator &it, AdCamiUrl *value) {
        *value = it->as_string();
        return it->type() == JSON_STRING;
    }

    template<typename T>
    bool _ObjectType(JSONNode &node, AdCamiUrl *value) {
        *value = node.as_string();
        return node.type() == JSON_STRING;
    }

    template<typename T>
    bool _ObjectType(JSONNode::const_iterator &it, int *value) {
        *value = it->as_int();
        return it->type() == JSON_NUMBER;
    }

    template<typename T>
    bool _ObjectType(JSONNode &node, int *value) {
        *value = node.as_int();
        return node.type() == JSON_NUMBER;
    }

    template<typename T>
    bool _ObjectType(JSONNode::const_iterator &it, float *value) {
        *value = it->as_float();
        return it->type() == JSON_NUMBER;
    }

    template<typename T>
    bool _ObjectType(JSONNode &node, float *value) {
        *value = node.as_float();
        return node.type() == JSON_NUMBER;
    }

    template<typename T>
    bool _ObjectType(JSONNode::const_iterator &it, bool *value) {
        *value = it->as_bool();
        return it->type() == JSON_BOOL;
    }

    template<typename T>
    bool _ObjectType(JSONNode &node, bool *value) {
        *value = node.as_bool();
        return node.type() == JSON_BOOL;
    }
};

} //namespace
#endif
