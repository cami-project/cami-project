#include "gtest/gtest.h"
#include "AdCamiHttpClient.h"

using AdCamiCommunications::AdCamiHttpClient;
using AdCamiCommunications::AdCamiHttpData;
using AdCamiCommunications::AdCamiHttpHeaders;
using AdCamiCommunications::AdCamiUrl;
/* Type alias for EnumHttpHeader. */
using EnumHttpHeader = AdCamiCommunications::AdCamiHttpHeaders::EnumHttpHeader;
using EnumHttpClientState = AdCamiCommunications::AdCamiHttpClient::EnumHttpClientState;

namespace {

const static AdCamiUrl TEST_SERVER("https://private-059efc-adcamihttpclient.apiary-mock.com");
const static AdCamiUrl TEST_SERVER_REQUEST_AUTH("/auth");
const static AdCamiUrl TEST_SERVER_REQUEST_FALSE("/false");
const static AdCamiUrl TEST_OPENTELE_SERVER("https://opentele.aliviate.dk:4287/opentele-citizen-server/patient");

AdCamiHttpClient client(80);
AdCamiHttpData response_ok;
AdCamiHttpData response_fail;
AdCamiHttpData response_opentele;
    
TEST(HttpClientTest, HttpClientRequest) {
    ASSERT_STREQ("{ \"id\":100 }", response_ok.GetDataAsString().c_str());
}
    
TEST(HttpClientTest, HttpClientHeaders_Ok) {
    ASSERT_STREQ("HTTP/1.1", response_ok.Headers.GetHeaderValue(EnumHttpHeader::HttpVersion).c_str());
    ASSERT_STREQ("200", response_ok.Headers.GetHeaderValue(EnumHttpHeader::ResponseStatusCode).c_str());
    ASSERT_STREQ("OK", response_ok.Headers.GetHeaderValue(EnumHttpHeader::ResponseStatusMessage).c_str());
    ASSERT_STREQ("application/json", response_ok.Headers.GetHeaderValue(EnumHttpHeader::ContentType).c_str());
    ASSERT_STREQ("12", response_ok.Headers.GetHeaderValue(EnumHttpHeader::ContentLength).c_str());
}
    
TEST(HttpClientTest, HttpClientHeaders_NotFound) {
    ASSERT_STREQ("HTTP/1.1", response_fail.Headers.GetHeaderValue(EnumHttpHeader::HttpVersion).c_str());
    ASSERT_STREQ("404", response_fail.Headers.GetHeaderValue(EnumHttpHeader::ResponseStatusCode).c_str());
    ASSERT_STREQ("Not Found", response_fail.Headers.GetHeaderValue(EnumHttpHeader::ResponseStatusMessage).c_str());
    ASSERT_STRNE("application/json", response_fail.Headers.GetHeaderValue(EnumHttpHeader::ContentType).c_str());
}

TEST(HttpClientTest, HttpClientHeaders_OpenTele) {
    std::cout << "response = " << response_opentele.GetDataAsString() << std::endl;
    ASSERT_STREQ("HTTP/1.1", response_opentele.Headers.GetHeaderValue(EnumHttpHeader::HttpVersion).c_str());
    ASSERT_STREQ("200", response_opentele.Headers.GetHeaderValue(EnumHttpHeader::ResponseStatusCode).c_str());
    ASSERT_STREQ("OK", response_opentele.Headers.GetHeaderValue(EnumHttpHeader::ResponseStatusMessage).c_str());
    ASSERT_STREQ("application/json;charset=UTF-8", response_opentele.Headers.GetHeaderValue(EnumHttpHeader::ContentType).c_str());
}

}

int main(int argc, char **argv) {
    ::testing::InitGoogleTest(&argc, argv);
    
    if (client.Get(TEST_SERVER + TEST_SERVER_REQUEST_AUTH, &response_ok) != EnumHttpClientState::OK) {
		return -1;
	}
	
    if (client.Get(TEST_SERVER + TEST_SERVER_REQUEST_FALSE, &response_fail) != EnumHttpClientState::OK) {
		return -1;
    }

    client.Insecure().SetUsername("NancyAnn").SetPassword("abcD1234");
    if (client.Get(TEST_OPENTELE_SERVER, &response_opentele) != EnumHttpClientState::OK) {
        return -1;
    }
    else {
        std::cout << "response = " << response_opentele.GetDataAsString() << std::endl;
    }
	
    return RUN_ALL_TESTS();
}