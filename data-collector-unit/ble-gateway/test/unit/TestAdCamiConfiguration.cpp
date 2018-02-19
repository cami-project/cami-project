//
// Created by Jorge Miguel Miranda on 06/12/2016.
//

#include "gtest/gtest.h"
#include <string>
#include <vector>
#include "AdCamiConfiguration.h"
#include "AdCamiUrl.h"

using std::string;
using std::vector;
using AdCamiCommunications::AdCamiUrl;

namespace {

static constexpr char kConfigurationFile[] = "./etc_adcamid_test/adcamid.conf";
static constexpr char kDefaultBluetoothAdapter[] = "hci0";
static constexpr char kDefaultGatewayName[] = "test-gateway-name";
static const vector<AdCamiUrl> kDefaultRemoteEndpoints = { "https://testremoteendpoint.com" };
static constexpr char kDefaultUsername[] = "test_opentele_default_username";
static constexpr char kDefaultPassword[] = "test_opentele_default_password";
static const unsigned int kDefaultReadMeasurementsTimeout = 30;

class TestAdCamiConfiguration : public ::testing::Test {
protected:
    TestAdCamiConfiguration() : _configuration(kConfigurationFile), _defaultConfiguration(kConfigurationFile) {}

    virtual void SetUp() {
        _defaultConfiguration.SetBluetoothAdapter(kDefaultBluetoothAdapter);
        _defaultConfiguration.SetCredentials(kDefaultUsername, kDefaultPassword);
        _defaultConfiguration.SetGatewayName(kDefaultGatewayName);
        _defaultConfiguration.SetReadMeasurementsTimeout(kDefaultReadMeasurementsTimeout);
        _defaultConfiguration.SetRemoteEndpoints(kDefaultRemoteEndpoints);
        _defaultConfiguration.Save();
    }

    /* This function resets the configuration file to the default values. */
    virtual void TearDown() {
        _configuration.Load();
        _configuration.SetBluetoothAdapter(kDefaultBluetoothAdapter);
        _configuration.SetCredentials(kDefaultUsername, kDefaultPassword);
        _configuration.SetGatewayName(kDefaultGatewayName);
        _configuration.SetReadMeasurementsTimeout(kDefaultReadMeasurementsTimeout);
        _configuration.SetRemoteEndpoints(kDefaultRemoteEndpoints);
        _configuration.Save();
    }

    AdCamiConfiguration _configuration;
    AdCamiConfiguration _defaultConfiguration;
};

TEST_F(TestAdCamiConfiguration, LoadConfiguration) {
    string username, password;

    _configuration.Load();

    ASSERT_STREQ(kDefaultBluetoothAdapter, _configuration.GetBluetoothAdapter().c_str());

    _configuration.GetCredentials(username, password);
    ASSERT_STREQ(kDefaultUsername, username.c_str());
    ASSERT_STREQ(kDefaultPassword, password.c_str());

    ASSERT_STREQ(kDefaultGatewayName, _configuration.GetGatewayName().c_str());

    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), _configuration.GetRemoteEndpoints()[i].c_str());
    }

    ASSERT_EQ(kDefaultReadMeasurementsTimeout, _configuration.GetReadMeasurementsTimeout());
}

TEST_F(TestAdCamiConfiguration, LoadSetAllAndSaveConfiguration) {
    constexpr char kBluetoothAdapter[] = "unit-test-bluetooth-adapter";
    constexpr char kGatewayName[] = "unit-test-LoadSetAllAndSaveConfiguration-gatewayname";
    const vector<AdCamiUrl> kRemoteEndpoint = { "https://unit-test-LoadSetAllAndSaveConfiguration.com" };
    constexpr char kUsername[] = "unit-test-LoadSetAllAndSaveConfiguration-username";
    constexpr char kPassword[] = "unit-test-LoadSetAllAndSaveConfiguration-password";
    const unsigned int kReadMeasurementsTimeout = 45;

    /* Load configuration file, set all properties and save the new configuration to file. */
    _configuration.Load();
    _configuration.SetBluetoothAdapter(kBluetoothAdapter);
    _configuration.SetGatewayName(kGatewayName);
    _configuration.SetRemoteEndpoints(kRemoteEndpoint);
    _configuration.SetCredentials(kUsername, kPassword);
    _configuration.SetReadMeasurementsTimeout(kReadMeasurementsTimeout);
    _configuration.Save();

    /* Load configuration file and test values*/
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    string username, password;

    configurationAfterSave.Load();

    ASSERT_STREQ(kBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());

    configurationAfterSave.GetCredentials(username, password);
    ASSERT_STREQ(kUsername, username.c_str());
    ASSERT_STREQ(kPassword, password.c_str());

    ASSERT_STREQ(kGatewayName, configurationAfterSave.GetGatewayName().c_str());

    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kRemoteEndpoint[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }

    ASSERT_EQ(kReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());
}

TEST_F(TestAdCamiConfiguration, LoadSetBluetoothAdapterAndSaveConfiguration) {
    constexpr char kBluetoothAdapter[] = "unit-test-bluetooth-adapter";

    /* Load file, set gateway name and save new configuration to file. */
    _configuration.Load();
    _configuration.SetBluetoothAdapter(kBluetoothAdapter);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    ASSERT_STREQ(kBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());
    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());
    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }
    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());
}

TEST_F(TestAdCamiConfiguration, LoadSetGatewayAndSaveConfiguration) {
    constexpr char kGatewayName[] = "unit-test-LoadSetGatewayAndSaveConfiguration";

    /* Load file, set gateway name and save new configuration to file. */
    _configuration.Load();
    _configuration.SetGatewayName(kGatewayName);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();

    ASSERT_STREQ(kDefaultBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());

    ASSERT_STREQ(kGatewayName, configurationAfterSave.GetGatewayName().c_str());

    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }

    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());
}

TEST_F(TestAdCamiConfiguration, LoadSetEndpointAndSaveConfiguration) {
    const vector<AdCamiUrl> kRemoteEndpoint = { "unit-test-LoadSetEndpointAndSaveConfiguration" };

    /* Load file, set endpoint and save new configuration to file. */
    _configuration.Load();
    _configuration.SetRemoteEndpoints(kRemoteEndpoint);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();

    ASSERT_STREQ(kRemoteEndpoint[0].c_str(), configurationAfterSave.GetRemoteEndpoints()[0].c_str());

    ASSERT_STREQ(kDefaultBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());
    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());
}

TEST_F(TestAdCamiConfiguration, LoadSetCredentialsAndSaveConfiguration) {
    constexpr char kUsername[] = "unit-test-LoadSetCredentialsAndSaveConfiguration-username";
    constexpr char kPassword[] = "unit-test-LoadSetCredentialsAndSaveConfiguration-password";
    string username, password;

    /* Load file, set credentials and save new configuration to file. */
    _configuration.Load();
    _configuration.SetCredentials(kUsername, kPassword);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    configurationAfterSave.GetCredentials(username, password);

    ASSERT_STREQ(kUsername, username.c_str());
    ASSERT_STREQ(kPassword, password.c_str());

    ASSERT_STREQ(kDefaultBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());
    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());
    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }
}

TEST_F(TestAdCamiConfiguration, SetBluetoothAdapterAndSaveConfiguration) {
    constexpr char kBluetoothAdapter[] = "unit-test-SetGatewayNameAndSaveConfiguration";

    /* Set gateway name and save new configuration to file. */
    _configuration.SetBluetoothAdapter(kBluetoothAdapter);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();

    ASSERT_STREQ(kBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());

    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());

    string username, password;
    configurationAfterSave.GetCredentials(username, password);
    ASSERT_STREQ(kDefaultUsername, username.c_str());
    ASSERT_STREQ(kDefaultPassword, password.c_str());

    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());

    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }
}

TEST_F(TestAdCamiConfiguration, SetGatewayNameAndSaveConfiguration) {
    constexpr char kGatewayName[] = "unit-test-SetGatewayNameAndSaveConfiguration";

    /* Set gateway name and save new configuration to file. */
    _configuration.SetGatewayName(kGatewayName);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();

    ASSERT_STREQ(kGatewayName, configurationAfterSave.GetGatewayName().c_str());

    ASSERT_STREQ(kDefaultBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());

    string username, password;
    configurationAfterSave.GetCredentials(username, password);
    ASSERT_STREQ(kDefaultUsername, username.c_str());
    ASSERT_STREQ(kDefaultPassword, password.c_str());

    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());

    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }
}

TEST_F(TestAdCamiConfiguration, SetEndpointAndSaveConfiguration) {
    const vector<AdCamiUrl> kRemoteEndpoints = {
        "http://unit-test-SetEndpointAndSaveConfiguration-1.com",
        "http://unit-test-SetEndpointAndSaveConfiguration-2.com"
    };

    /* Set endpoints and save new configuration to file. */
    _configuration.SetRemoteEndpoints(kRemoteEndpoints);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();

    string username, password;
    configurationAfterSave.GetCredentials(username, password);
    ASSERT_STREQ(kDefaultUsername, username.c_str());
    ASSERT_STREQ(kDefaultPassword, password.c_str());

    ASSERT_STREQ(kDefaultBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());

    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());

    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());

    for (unsigned int i = 0; i < configurationAfterSave.GetRemoteEndpoints().size(); i++) {
        ASSERT_STREQ(kRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }
}

TEST_F(TestAdCamiConfiguration, SetCredentialsAndSaveConfiguration) {
    constexpr char kUsername[] = "SetCredentialsAndSaveConfiguration-username";
    constexpr char kPassword[] = "SetCredentialsAndSaveConfiguration-password";
    string username, password;

    /* Set credentails and save new configuration to file. */
    _configuration.SetCredentials(kUsername, kPassword);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();

    configurationAfterSave.GetCredentials(username, password);
    ASSERT_STREQ(kUsername, username.c_str());
    ASSERT_STREQ(kPassword, password.c_str());

    ASSERT_STREQ(kDefaultBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());

    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());

    ASSERT_EQ(kDefaultReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());

    for (unsigned int i = 0; i < configurationAfterSave.GetRemoteEndpoints().size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }
}

TEST_F(TestAdCamiConfiguration, SetReadMeasurementsTimeoutAndSaveConfiguration) {
    const unsigned int kReadMeasurementsTimeout = 50;

    /* Set gateway name and save new configuration to file. */
    _configuration.SetReadMeasurementsTimeout(kReadMeasurementsTimeout);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();

    ASSERT_EQ(kReadMeasurementsTimeout, configurationAfterSave.GetReadMeasurementsTimeout());

    ASSERT_STREQ(kDefaultBluetoothAdapter, configurationAfterSave.GetBluetoothAdapter().c_str());

    string username, password;
    configurationAfterSave.GetCredentials(username, password);
    ASSERT_STREQ(kDefaultUsername, username.c_str());
    ASSERT_STREQ(kDefaultPassword, password.c_str());

    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());

    for (unsigned int i = 0; i < kDefaultRemoteEndpoints.size(); i++) {
        ASSERT_STREQ(kDefaultRemoteEndpoints[i].c_str(), configurationAfterSave.GetRemoteEndpoints()[i].c_str());
    }
}

}//namespace

int main(int argc, char **argv) {
    ::testing::InitGoogleTest(&argc, argv);

    return RUN_ALL_TESTS();
}
