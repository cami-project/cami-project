//
// Created by Jorge Miguel Miranda on 06/12/2016.
//

#include "gtest/gtest.h"
#include <string>
#include "AdCamiConfiguration.h"

namespace {

static constexpr char kConfigurationFile[] = "./etc_adcamid_test/adcamid.conf";
static constexpr char kDefaultGatewayName[] = "test-gateway-name";
static constexpr char kDefaultRemoteEndpoint[] = "https://testremoteendpoint.com";
static constexpr char kDefaultUsername[] = "test_opentele_default_username";
static constexpr char kDefaultPassword[] = "test_opentele_default_password";

class TestAdCamiConfiguration : public ::testing::Test {
protected:
    TestAdCamiConfiguration() : _configuration(kConfigurationFile), _defaultConfiguration(kConfigurationFile) {}

    virtual void SetUp() {
        _defaultConfiguration.SetGatewayName(kDefaultGatewayName);
        _defaultConfiguration.SetRemoteEndpoint(kDefaultRemoteEndpoint);
        _defaultConfiguration.SetCredentials(kDefaultUsername, kDefaultPassword);
        _defaultConfiguration.Save();
    }

    /* This function resets the configuration file to the default values. */
    virtual void TearDown() {
        _configuration.Load();
        _configuration.SetGatewayName(kDefaultGatewayName);
        _configuration.SetRemoteEndpoint(kDefaultRemoteEndpoint);
        _defaultConfiguration.SetCredentials(kDefaultUsername, kDefaultPassword);
        _configuration.Save();
    }

    AdCamiConfiguration _configuration;
    AdCamiConfiguration _defaultConfiguration;
};

TEST_F(TestAdCamiConfiguration, LoadConfiguration) {
    string username, password;

    _configuration.Load();
    _configuration.GetCredentials(username, password);

    ASSERT_STREQ(kDefaultGatewayName,
            _configuration.GetGatewayName().c_str());
    ASSERT_STREQ(kDefaultRemoteEndpoint,
            _configuration.GetRemoteEndpoint().c_str());
    ASSERT_STREQ(kDefaultUsername,
            username.c_str());
    ASSERT_STREQ(kDefaultPassword,
            password.c_str());
}

TEST_F(TestAdCamiConfiguration, LoadSetAllAndSaveConfiguration) {
    constexpr char kGatewayName[] = "unit-test-LoadSetAllAndSaveConfiguration-gatewayname";
    constexpr char kRemoteEndpoint[] = "https://unit-test-LoadSetAllAndSaveConfiguration.com";
    constexpr char kUsername[] = "unit-test-LoadSetAllAndSaveConfiguration-username";
    constexpr char kPassword[] = "unit-test-LoadSetAllAndSaveConfiguration-password";

    /* Load configuration file, set all properties and save the configuration. */
    _configuration.Load();
    _configuration.SetGatewayName(kGatewayName);
    _configuration.SetRemoteEndpoint(kRemoteEndpoint);
    _configuration.SetCredentials(kUsername, kPassword);
    _configuration.Save();

    /* Load configuration file and test values*/
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    string username, password;

    configurationAfterSave.Load();
    configurationAfterSave.GetCredentials(username, password);

    ASSERT_STREQ(kGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_STREQ(kRemoteEndpoint, configurationAfterSave.GetRemoteEndpoint().c_str());
    ASSERT_STREQ(kUsername, username.c_str());
    ASSERT_STREQ(kPassword, password.c_str());
}

TEST_F(TestAdCamiConfiguration, LoadSetGatewayAndSaveConfiguration) {
    constexpr char kGatewayName[] = "unit-test-LoadSetGatewayAndSaveConfiguration";

    _configuration.Load();
    _configuration.SetGatewayName(kGatewayName);
    _configuration.Save();

    /* Load configuration file and test values*/
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    ASSERT_STREQ(kGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_STREQ(kDefaultRemoteEndpoint, configurationAfterSave.GetRemoteEndpoint().c_str());
}

TEST_F(TestAdCamiConfiguration, LoadSetEndpointAndSaveConfiguration) {
    constexpr char kRemoteEndpoint[] = "unit-test-LoadSetEndpointAndSaveConfiguration";

    _configuration.Load();
    _configuration.SetRemoteEndpoint(kRemoteEndpoint);
    _configuration.Save();

    /* Load configuration file and test values*/
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_STREQ(kRemoteEndpoint, configurationAfterSave.GetRemoteEndpoint().c_str());
}

TEST_F(TestAdCamiConfiguration, LoadSetCredentialsAndSaveConfiguration) {
    constexpr char kUsername[] = "unit-test-LoadSetCredentialsAndSaveConfiguration-username";
    constexpr char kPassword[] = "unit-test-LoadSetCredentialsAndSaveConfiguration-password";
    string username, password;

    _configuration.Load();
    _configuration.SetCredentials(kUsername, kPassword);
    _configuration.Save();

    /* Load configuration file and test values*/
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    configurationAfterSave.GetCredentials(username, password);

    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_STREQ(kDefaultRemoteEndpoint, configurationAfterSave.GetRemoteEndpoint().c_str());
    ASSERT_STREQ(kUsername, username.c_str());
    ASSERT_STREQ(kPassword, password.c_str());
}

TEST_F(TestAdCamiConfiguration, SetGatewayNameAndSaveConfiguration) {
    constexpr char kGatewayName[] = "unit-test-SetGatewayNameAndSaveConfiguration";

    _configuration.SetGatewayName(kGatewayName);
    _configuration.Save();

    /* Load configuration file and test values*/
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    ASSERT_STREQ(kGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_STREQ(kDefaultRemoteEndpoint, configurationAfterSave.GetRemoteEndpoint().c_str());
}

TEST_F(TestAdCamiConfiguration, SetEndpointAndSaveConfiguration) {
    constexpr char kRemoteEndpoint[] = "http://unit-test-SetEndpointAndSaveConfiguration.com";

    _configuration.SetRemoteEndpoint(kRemoteEndpoint);
    _configuration.Save();

    /* Load configuration file and test values*/
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_STREQ(kRemoteEndpoint, configurationAfterSave.GetRemoteEndpoint().c_str());
}

TEST_F(TestAdCamiConfiguration, SetCredentialsAndSaveConfiguration) {
    constexpr char kUsername[] = "SetCredentialsAndSaveConfiguration-username";
    constexpr char kPassword[] = "SetCredentialsAndSaveConfiguration-password";
    string username, password;

    _configuration.SetCredentials(kUsername, kPassword);
    _configuration.Save();

    /* Load configuration file and test values. */
    AdCamiConfiguration configurationAfterSave(kConfigurationFile);
    configurationAfterSave.Load();
    configurationAfterSave.GetCredentials(username, password);

    ASSERT_STREQ(kUsername, username.c_str());
    ASSERT_STREQ(kPassword, password.c_str());
    ASSERT_STREQ(kDefaultGatewayName, configurationAfterSave.GetGatewayName().c_str());
    ASSERT_STREQ(kDefaultRemoteEndpoint, configurationAfterSave.GetRemoteEndpoint().c_str());
}

}//namespace

int main(int argc, char **argv) {
    ::testing::InitGoogleTest(&argc, argv);

    return RUN_ALL_TESTS();
}
