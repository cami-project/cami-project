//
// Created by Jorge Miguel Miranda on 05/12/2016.
//

#ifndef ADCAMID_ADCAMICONFIGURATION_H
#define ADCAMID_ADCAMICONFIGURATION_H

#include <string>
#include <tuple>
#include "libjson/libjson.h"

using std::string;

class AdCamiConfiguration {
private:
    enum EnumConfigurationError : int {
        ConfigurationFileNotFound = -4,
        InvalidJsonFormat = -3,
        JsonMemberNotFound = -2,
        Error = -1,
        Ok = 0
    };

    string _configurationFilePath;
    bool _isFileLoaded;
    std::tuple<string, bool> _gatewayName;
    std::tuple<string, bool> _remoteEndpoint;
    std::tuple<string, string, bool> _credentials;

    AdCamiConfiguration::EnumConfigurationError _GetMember(const JSONNode &node,
                                                           const string &member,
                                                           JSONNode::const_iterator *value) const;

    AdCamiConfiguration::EnumConfigurationError _ParseJson(const string &json);

    /**
     * Sets the configuration values based on a strategy.
     * @param configuration
     */
//    void _SetConfiguration(const AdCamiConfiguration &configuration);

    void _ToJson(string *json);

public:
    AdCamiConfiguration(const string &filePath) :
            _configurationFilePath(filePath),
            _isFileLoaded(false),
            _gatewayName(std::make_tuple("", false)),
            _remoteEndpoint(std::make_tuple("", false)),
            _credentials(std::make_tuple("", "", false)) {}

    AdCamiConfiguration(const char *filePath) :
            _configurationFilePath(std::string(filePath)),
            _isFileLoaded(false),
            _gatewayName(std::make_tuple("", false)),
            _remoteEndpoint(std::make_tuple("", false)),
            _credentials(std::make_tuple("", "", false)) {}

    AdCamiConfiguration(const AdCamiConfiguration &configuration) :
            _configurationFilePath(configuration._configurationFilePath, false),
            _isFileLoaded(configuration._isFileLoaded),
            _gatewayName(std::make_tuple(std::get<0>(configuration._gatewayName),
                                         std::get<1>(configuration._gatewayName))),
            _remoteEndpoint(std::make_tuple(std::get<0>(configuration._remoteEndpoint),
                                            std::get<1>(configuration._remoteEndpoint))),
            _credentials(std::make_tuple(std::get<0>(configuration._credentials),
                                         std::get<1>(configuration._credentials),
                                         std::get<2>(configuration._credentials))) {}

    inline void GetCredentials(string &username, string &password) const {
        username = std::get<0>(this->_credentials);
        password = std::get<1>(this->_credentials);
    }

    inline const string &GetGatewayName() const { return std::get<0>(this->_gatewayName); }

    inline const string &GetRemoteEndpoint() const { return std::get<0>(this->_remoteEndpoint); }

    inline void SetCredentials(const string &username, const string &password) {
        std::get<0>(this->_credentials) = username;
        std::get<1>(this->_credentials) = password;
        std::get<2>(this->_credentials) = true;
    }

    inline void SetGatewayName(const string &name) {
        std::get<0>(this->_gatewayName) = name;
        std::get<1>(this->_gatewayName) = true;
    }

    inline void SetRemoteEndpoint(const string &endpoint) {
        std::get<0>(this->_remoteEndpoint) = endpoint;
        std::get<1>(this->_remoteEndpoint) = true;
    }

    EnumConfigurationError Load();

    EnumConfigurationError Save();
};

#endif //ADCAMID_ADCAMICONFIGURATION_H
