//
// Created by Jorge Miguel Miranda on 05/12/2016.
//

#include <algorithm>
#include <iostream>
#include <fstream>
#include "AdCamiConfiguration.h"
#include "AdCamiJsonConverter.h"


AdCamiConfiguration::EnumConfigurationError AdCamiConfiguration::Load() {
    std::ifstream file(this->_configurationFilePath);
    std::string json;

    if (file.is_open()) {
        file.seekg(0, std::ios::end);
        json.reserve(file.tellg());
        file.seekg(0, std::ios::beg);

        json.assign((std::istreambuf_iterator<char>(file)),
                    std::istreambuf_iterator<char>());

        if (this->_ParseJson(json) == EnumConfigurationError::Ok) {
            this->_isFileLoaded = true;

            return EnumConfigurationError::Ok;
        } else {
            return EnumConfigurationError::Error;
        }
    } else {
        return EnumConfigurationError::ConfigurationFileNotFound;
    }
}

AdCamiConfiguration::EnumConfigurationError AdCamiConfiguration::Save() {
    /* If the file was not previously loaded, then load it and set values
     * from current configuration. */
    if (this->_isFileLoaded == false) {
        AdCamiConfiguration configurationTmp(this->_configurationFilePath);
        configurationTmp.Load();

        if (std::get<1>(this->_gatewayName) != true)
            this->SetGatewayName(configurationTmp.GetGatewayName());
        if (std::get<1>(this->_remoteEndpoint) != true)
            this->SetRemoteEndpoint(configurationTmp.GetRemoteEndpoint());
        if (std::get<2>(this->_credentials) != true) {
            string username, password;
            configurationTmp.GetCredentials(username, password);
            this->SetCredentials(username, password);
        }
    }

    std::ofstream file(this->_configurationFilePath);
    string json;

    this->_ToJson(&json);

    if (file.is_open()) {
        file << json;
        file.close();
    } else {
        return EnumConfigurationError::ConfigurationFileNotFound;
    }

    /* Reset the values to know that they have already been saved (i.e. not modified). */
    std::get<1>(this->_gatewayName) = false;
    std::get<1>(this->_remoteEndpoint) = false;
    std::get<2>(this->_credentials) = false;

    return EnumConfigurationError::Ok;
}

AdCamiConfiguration::EnumConfigurationError AdCamiConfiguration::_GetMember(const JSONNode &node,
                                                                            const string &member,
                                                                            JSONNode::const_iterator *value) const {
    string memberName;

    for (*value = node.begin(); *value != node.end(); ++(*value)) {
        memberName = (*value)->name();
        std::transform(memberName.begin(), memberName.end(), memberName.begin(), ::tolower);
        if (memberName.compare(member) == 0) {
            return EnumConfigurationError::Ok;
        }
    }

    return ((*value == node.end()) ? EnumConfigurationError::JsonMemberNotFound
                                   : EnumConfigurationError::Ok);
}


AdCamiConfiguration::EnumConfigurationError AdCamiConfiguration::_ParseJson(const string &json) {
    if (libjson::is_valid(json) == false) {
        PRINT_DEBUG("Invalid JSON string.")
        return EnumConfigurationError::InvalidJsonFormat;
    }

    JSONNode node = libjson::parse(json);
    JSONNode::const_iterator it = node.begin();
    if (_GetMember(node, "remoteendpoint", &it) == EnumConfigurationError::Ok) {
//        PRINT_DEBUG("'remoteendpoint' attribute found = " << it->as_string())
        this->SetRemoteEndpoint(it->as_string());
    } else {
        PRINT_DEBUG("'remoteendpoint' JSON value not found")
        return EnumConfigurationError::JsonMemberNotFound;
    }

    if (_GetMember(node, "gatewayname", &it) == EnumConfigurationError::Ok) {
//        PRINT_DEBUG("'gatewayName' attribute found = " << it->as_string())
        this->SetGatewayName(it->as_string());
    } else {
        PRINT_DEBUG("'gatewayName' JSON value not found")
        return EnumConfigurationError::JsonMemberNotFound;
    }

    if (_GetMember(node, "opentele", &it) == EnumConfigurationError::Ok) {
        auto username = it->begin(), password = it->begin();
        if (_GetMember(*it, "username", &username) == EnumConfigurationError::Ok &&
            _GetMember(*it, "password", &password) == EnumConfigurationError::Ok) {
//            PRINT_DEBUG("'username' attribute found = " << username->as_string())
//            PRINT_DEBUG("'password' attribute found = " << password->as_string())
            this->SetCredentials(username->as_string(), password->as_string());
        } else {
            PRINT_DEBUG("'username' or 'password' JSON value not found")
            return EnumConfigurationError::JsonMemberNotFound;
        }
    } else {
        PRINT_DEBUG("'opentele' JSON value not found")
        return EnumConfigurationError::JsonMemberNotFound;
    }

    return EnumConfigurationError::Ok;
}

void AdCamiConfiguration::_ToJson(string *json) {
    JSONNode node(JSON_NODE), nodeOpenTele(JSON_NODE);
    string openTeleUsername, openTelePassword;

    node.push_back(JSONNode("remoteendpoint", this->GetRemoteEndpoint()));
    node.push_back(JSONNode("gatewayname", this->GetGatewayName()));

    this->GetCredentials(openTeleUsername, openTelePassword);
    nodeOpenTele.set_name("opentele");
    nodeOpenTele.push_back(JSONNode("username", openTeleUsername));
    nodeOpenTele.push_back(JSONNode("password", openTelePassword));
    node.push_back(nodeOpenTele);

    *json = node.write_formatted();
}