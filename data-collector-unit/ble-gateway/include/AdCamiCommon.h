//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiCommon_h
#define AdCamiDaemon_AdCamiCommon_h

#include <map>
#include <string>
#include <vector>

using std::map;
using std::string;
using std::vector;

#define DEBUG_FILE "/var/log/adcamid/debug.log"
#define ERROR_FILE "/var/log/adcamid/error.log"
#define LOG_FILE "/var/log/adcamid/messages.log"
#define PID_FILE "/var/run/adcamid.pid"

namespace AdCamiCommon {

static constexpr const char *kDateTimeFormat = "%Y-%m-%d %T";

#ifdef DEBUG
constexpr const char* kAdCamiConfigurationDir = "./etc_adcamid_test";
constexpr const char* kAdCamiConfigurationFile = "./etc_adcamid_test/adcamid.conf";
constexpr const char* kAdCamiEventsDatabase = "./etc_adcamid_test/events.db";
constexpr const char* kAdCamiCertificatePemFile = "./etc_adcamid_test/server.pem";
constexpr const char* kAdCamiCertificateKeyFile = "./etc_adcamid_test/server.key";
#else
constexpr const char *kAdCamiConfigurationDir = "/etc/adcamid";
constexpr const char *kAdCamiConfigurationFile = "/etc/adcamid/adcamid.conf";
constexpr const char *kAdCamiEventsDatabase = "/etc/adcamid/events.db";
constexpr const char *kAdCamiCertificatePemFile = "/etc/adcamid/server.pem";
constexpr const char *kAdCamiCertificateKeyFile = "/etc/adcamid/server.key";
#endif

/* Conversion table between known and allowed UUIDs. */
static const map<const string, const string> kKnownBluetoothUuids = {
        {"233bf000-5a34-1b6d-975c-000d5690abe4", "A&D"},
        {"00001810-0000-1000-8000-00805f9b34fb", "Blood Pressure"},
        {"0000181d-0000-1000-8000-00805f9b34fb", "Weight Scale"}
};

/* Initialize vector with allowed UUIDs that will be filtered when discovering  */
static const vector <string> kAllowedBluetoothUuids = {
        "233bf000-5a34-1b6d-975c-000d5690abe4",
        "00001810-0000-1000-8000-00805f9b34fb",
        "0000181d-0000-1000-8000-00805f9b34fb"
};

}


#endif
