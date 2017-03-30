//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiCommon_h
#define AdCamiDaemon_AdCamiCommon_h

#define DEBUG_FILE "/var/log/adcamid/debug.log"
#define ERROR_FILE "/var/log/adcamid/error.log"
#define LOG_FILE "/var/log/adcamid/messages.log"
#define PID_FILE "/var/run/adcamid.pid"

namespace AdCamiCommon {

#ifdef DEBUG
constexpr const char* kAdCamiConfigurationDir = "./etc_adcamid_test";
constexpr const char* kAdCamiConfigurationFile = "./etc_adcamid_test/adcamid.conf";
constexpr const char* kAdCamiEventsDatabase = "./etc_adcamid_test/events.db";
constexpr const char* kAdCamiCertificatePemFile = "./etc_adcamid_test/server.pem";
constexpr const char* kAdCamiCertificateKeyFile = "./etc_adcamid_test/server.key";
#else
constexpr const char* kAdCamiConfigurationDir = "/etc/adcamid";
constexpr const char* kAdCamiConfigurationFile = "/etc/adcamid/adcamid.conf";
constexpr const char* kAdCamiEventsDatabase = "/etc/adcamid/events.db";
constexpr const char* kAdCamiCertificatePemFile = "/etc/adcamid/server.pem";
constexpr const char* kAdCamiCertificateKeyFile = "/etc/adcamid/server.key";
#endif



}


#endif
