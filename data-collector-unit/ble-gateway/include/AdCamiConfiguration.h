//
// Created by Jorge Miguel Miranda on 05/12/2016.
//

#ifndef ADCAMID_ADCAMICONFIGURATION_H
#define ADCAMID_ADCAMICONFIGURATION_H

#include <string>
#include <tuple>
#include <vector>
#include "libjson/libjson.h"
#include "AdCamiUrl.h"

using std::string;
using std::tuple;
using std::vector;
using AdCamiCommunications::AdCamiUrl;

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
    tuple<string, bool> _bluetoothAdapter;
    tuple<string, bool> _gatewayName;
    tuple<vector<AdCamiUrl>, bool> _remoteEndpoints;
    tuple<string, string, bool> _credentials;
    tuple<unsigned int, bool> _readMeasurementsTimeout;

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
            _bluetoothAdapter(std::make_tuple("", false)),
            _gatewayName(std::make_tuple("", false)),
            _remoteEndpoints(std::make_tuple(vector<AdCamiUrl>({}), false)),
            _credentials(std::make_tuple("", "", false)),
            _readMeasurementsTimeout(std::make_tuple(0, false)) {}

    AdCamiConfiguration(const char *filePath) :
            _configurationFilePath(std::string(filePath)),
            _isFileLoaded(false),
            _bluetoothAdapter(std::make_tuple("", false)),
            _gatewayName(std::make_tuple("", false)),
            _remoteEndpoints(std::make_tuple(vector<AdCamiUrl>({}), false)),
            _credentials(std::make_tuple("", "", false)),
            _readMeasurementsTimeout(std::make_tuple(0, false)) {}

    AdCamiConfiguration(const AdCamiConfiguration &configuration) :
            _configurationFilePath(configuration._configurationFilePath, false),
            _isFileLoaded(configuration._isFileLoaded),
            _bluetoothAdapter(std::make_tuple(std::get<0>(configuration._bluetoothAdapter),
                                              std::get<1>(configuration._bluetoothAdapter))),
            _gatewayName(std::make_tuple(std::get<0>(configuration._gatewayName),
                                         std::get<1>(configuration._gatewayName))),
            _remoteEndpoints(std::make_tuple(std::get<0>(configuration._remoteEndpoints),
                                             std::get<1>(configuration._remoteEndpoints))),
            _credentials(std::make_tuple(std::get<0>(configuration._credentials),
                                         std::get<1>(configuration._credentials),
                                         std::get<2>(configuration._credentials))),
            _readMeasurementsTimeout(
                    std::make_tuple(std::get<0>(configuration._readMeasurementsTimeout),
                                    std::get<1>(configuration._readMeasurementsTimeout))) {}

    inline const string &GetBluetoothAdapter() const {
        return std::get<0>(this->_bluetoothAdapter);
    }

    inline void GetCredentials(string &username, string &password) const {
        username = std::get<0>(this->_credentials);
        password = std::get<1>(this->_credentials);
    }

    inline const string &GetGatewayName() const {
        return std::get<0>(this->_gatewayName);
    }

    inline const unsigned int GetReadMeasurementsTimeout() const {
        return std::get<0>(this->_readMeasurementsTimeout);
    }

    inline const vector <AdCamiUrl> &GetRemoteEndpoints() const { return std::get<0>(this->_remoteEndpoints); }

    inline void SetBluetoothAdapter(const string &adapter) {
        std::get<0>(this->_bluetoothAdapter) = adapter;
        std::get<1>(this->_bluetoothAdapter) = true;
    }

    inline void SetCredentials(const string &username, const string &password) {
        std::get<0>(this->_credentials) = username;
        std::get<1>(this->_credentials) = password;
        std::get<2>(this->_credentials) = true;
    }

    inline void SetGatewayName(const string &name) {
        std::get<0>(this->_gatewayName) = name;
        std::get<1>(this->_gatewayName) = true;
    }

    inline void SetReadMeasurementsTimeout(const unsigned int &timeout) {
        std::get<0>(this->_readMeasurementsTimeout) = timeout;
        std::get<1>(this->_readMeasurementsTimeout) = true;

    }

    inline void SetRemoteEndpoints(const vector <AdCamiUrl> &endpoints) {
        std::get<0>(this->_remoteEndpoints) = static_cast<vector <AdCamiUrl>>(endpoints);
        std::get<1>(this->_remoteEndpoints) = true;
    }

    EnumConfigurationError Load();

    EnumConfigurationError Save();
};

#endif //ADCAMID_ADCAMICONFIGURATION_H
