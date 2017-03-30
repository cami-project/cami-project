//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#include <algorithm>
#include <iostream>
#include "AdCamiJsonConverter.h"
#include "AdCamiUtilities.h"

namespace AdCamiCommunications {

const string AdCamiJsonConverter::MimeType = "application/json; charset=utf-8";

AdCamiJsonConverter::AdCamiJsonConverter() {}

string *AdCamiJsonConverter::Error(const string &message, string *json) {
    JSONNode node;

    node.push_back(JSONNode("error", message));
    *json = node.write();

    return json;
}

string &AdCamiJsonConverter::ToJson(const AdCamiBluetoothDevice &device, string *json) {
    JSONNode node(JSON_NODE);
    JSONNode uuidNode(JSON_ARRAY);

    node.push_back(JSONNode("address", device.Address()));
    node.push_back(JSONNode("name", *device.NameFromCache().release()));
    node.push_back(JSONNode("paired", device.PairedFromCache()));
    for (auto uuid : device.Uuids()) {
        uuidNode.push_back(JSONNode("", uuid));
    }
    uuidNode.set_name("uuids");
    node.push_back(uuidNode);

    *json = node.write();

    return *json;
}

string &AdCamiJsonConverter::ToJson(const vector <AdCamiBluetoothDevice> &devices, string *json) {
    string jsonTmp;

    *json = "\"devices\":[";
    for (auto device : devices) {
        this->ToJson(device, &jsonTmp);
        *json += jsonTmp + ",";
    }
    if (devices.size() > 0) {
        json->erase(json->end() - 1);
    }
    *json += "]";

    return *json;
}

string &AdCamiJsonConverter::ToJson(const AdCamiEvent &event, string *json) {
    JSONNode node(JSON_NODE);

    node.push_back(JSONNode("timestamp", event.TimeStamp()));
    node.push_back(JSONNode("address", event.Address()));
    *json = node.write();

    return *json;
}

string &AdCamiJsonConverter::ToJson(const AdCamiEvent *event, string *json) {
    switch (event->Type()) {
        case EnumEventType::BloodPressure: {
            this->ToJson(*dynamic_cast<const AdCamiEventBloodPressureMeasurement *>(event), json);
            break;
        }
        case EnumEventType::Device: {
            this->ToJson(*event, json);
            break;
        }
        case EnumEventType::Weight: {
            this->ToJson(*dynamic_cast<const AdCamiEventWeightMeasurement *>(event), json);
            break;
        }
        default: {
            break;
        }
    }

    return *json;
}

string &AdCamiJsonConverter::ToJson(const vector <AdCamiEvent> &events, const string &gatewayName, string *json) {
    string jsonTmp;

    *json = "{\"gateway\":\"" + gatewayName + "\",\"events\":[";
    for (auto event : events) {
        this->ToJson(event, &jsonTmp);
        *json += jsonTmp + ",";
    }

    if (events.size() > 0) {
        json->erase(json->end() - 1);
    }
    *json += "]}";

    return *json;
}

string &AdCamiJsonConverter::ToJson(const vector<AdCamiEvent *> &events, const string &gatewayName, string *json) {
    string jsonTmp;

    *json = "{\"gateway\":\"" + gatewayName + "\",\"events\":[";
    for (auto event : events) {
        this->ToJson(event, &jsonTmp);
        *json += jsonTmp + ",";
    }

    if (events.size() > 0) {
        json->erase(json->end() - 1);
    }
    *json += "]}";

    return *json;
}

string &AdCamiJsonConverter::ToJson(const AdCamiEventBloodPressureMeasurement &event, string *json) {
    JSONNode node(JSON_NODE), nodeTmp(JSON_NODE);

    node.push_back(JSONNode("timestamp", event.TimeStamp()));
    node.push_back(JSONNode("address", event.Address()));

    nodeTmp.set_name("systolic");
    nodeTmp.push_back(JSONNode("value", event.Systolic().Value()));
    nodeTmp.push_back(JSONNode("unit", event.Systolic().Unit()));
    node.push_back(nodeTmp);

    nodeTmp.clear();
    nodeTmp.set_name("diastolic");
    nodeTmp.push_back(JSONNode("value", event.Diastolic().Value()));
    nodeTmp.push_back(JSONNode("unit", event.Diastolic().Unit()));
    node.push_back(nodeTmp);

    nodeTmp.clear();
    nodeTmp.set_name("meanarterialpressure");
    nodeTmp.push_back(JSONNode("value", event.MeanArterialPressure().Value()));
    nodeTmp.push_back(JSONNode("unit", event.MeanArterialPressure().Unit()));
    node.push_back(nodeTmp);

    nodeTmp.clear();
    nodeTmp.set_name("pulserate");
    nodeTmp.push_back(JSONNode("value", event.PulseRate().Value()));
    nodeTmp.push_back(JSONNode("unit", event.PulseRate().Unit()));
    node.push_back(nodeTmp);

    *json = node.write();

    return *json;
}

string &AdCamiJsonConverter::ToJson(const vector <AdCamiEventBloodPressureMeasurement> &events,
                                    const string &gatewayName,
                                    string *json) {
    string jsonTmp;

    *json = "{\"gateway\":\"" + gatewayName + "\",\"events\":[";
    for (auto event : events) {
        this->ToJson(event, &jsonTmp);
        *json += jsonTmp + ",";
    }

    if (events.size() > 0) {
        json->erase(json->end() - 1);
    }
    *json += "]}";

    return *json;
}

string &AdCamiJsonConverter::ToJson(const AdCamiEventWeightMeasurement &event, string *json) {
    JSONNode node(JSON_NODE), nodeTmp(JSON_NODE);

    node.push_back(JSONNode("timestamp", event.TimeStamp()));
    node.push_back(JSONNode("address", event.Address()));

    nodeTmp.set_name("weight");
    nodeTmp.push_back(JSONNode("value", event.Weight().Value()));
    nodeTmp.push_back(JSONNode("unit", event.Weight().Unit()));
    node.push_back(nodeTmp);

    *json = node.write();

    return *json;
}

string &AdCamiJsonConverter::ToJson(const vector <AdCamiEventWeightMeasurement> &events,
                                    const string &gatewayName,
                                    string *json) {
    string jsonTmp;

    *json = "{\"gateway\":\"" + gatewayName + "\",\"events\":[";
    for (auto event : events) {
        this->ToJson(event, &jsonTmp);
        *json += jsonTmp + ",";
    }
    if (events.size() > 0) {
        json->erase(json->end() - 1);
    }
    *json += "]}";

    return *json;
}

string &AdCamiJsonConverter::ErrorMessage(const string &message, string *json) {
    JSONNode node(JSON_NODE);

    node.push_back(JSONNode("error", message));
    *json = node.write();

    return *json;
}

JSONNode::const_iterator AdCamiJsonConverter::_GetMember(const string &member, const JSONNode &node) {
    JSONNode::const_iterator it = node.begin();
    string memberName, aux;

    for (; it != node.end(); ++it) {
        memberName = it->name();
        std::transform(memberName.begin(), memberName.end(), memberName.begin(), ::tolower);
        if (member.compare(memberName) == 0) {
            return it;
        }
    }

    return it;
}

AdCamiJsonConverter::EnumState AdCamiJsonConverter::_GetMember(const JSONNode &node,
                                                               const string &member,
                                                               JSONNode::const_iterator *value) const {
    string memberName;

    for (*value = node.begin(); *value != node.end(); ++(*value)) {
        memberName = (*value)->name();
        std::transform(memberName.begin(), memberName.end(), memberName.begin(), ::tolower);
        if (memberName.compare(member) == 0) {
            return EnumState::Ok;
        }
    }

    return ((*value == node.end()) ? EnumState::JsonMemberNotFound : EnumState::Ok);
}

AdCamiJsonConverter::EnumState AdCamiJsonConverter::_GetMember(const JSONNode &node,
                                                               const string &member,
                                                               const JSONNodeType &nodeType,
                                                               JSONNode::const_iterator *value) const {
    string memberName;

    for (*value = node.begin(); *value != node.end(); ++(*value)) {
        memberName = (*value)->name();
        std::transform(memberName.begin(), memberName.end(), memberName.begin(), ::tolower);
        if (memberName.compare(member) == 0) {
            if ((*value)->type() == nodeType) {
                return EnumState::Ok;
            } else {
                return EnumState::JsonMemberTypeDiffers;
            }
        }
    }

    return ((*value == node.end()) ? EnumState::JsonMemberNotFound : EnumState::Ok);
}

AdCamiJsonConverter::EnumState AdCamiJsonConverter::_GetMemberFromArray(const JSONNode &node, const string &name,
                                                                        JSONNode::const_iterator *value) {
    if (node.type() != JSON_ARRAY) {
        return EnumState::JsonMemberTypeDiffers;
    }

    for (auto object : node) {
        if ((*value = object.find(name)) != node.end()) {
            PRINT_DEBUG("object with name '" << name << "' found: " << (*value)->as_string())
            return EnumState::Ok;
        }
    }

    return EnumState::JsonMemberNotFound;
}

} //namespace
