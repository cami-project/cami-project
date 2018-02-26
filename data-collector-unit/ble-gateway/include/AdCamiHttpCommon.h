//
//  Created by Jorge Miguel Miranda on 23/11/26.
//

#ifndef AdCamiDaemon_AdCamiHttpCommon_h
#define AdCamiDaemon_AdCamiHttpCommon_h

#include <iostream>
#include "AdCamiUtilities.h"

using std::string;

namespace AdCamiCommunications {

    namespace AdCamiHttpCommon {

    enum class EnumActionResult : unsigned int {
        Ok,
        Failed
    };

    enum class EnumHttpMethod : unsigned int {
        DELETE = AdCamiUtilities::StringHash("DELETE"),
        GET = AdCamiUtilities::StringHash("GET"),
        PATCH = AdCamiUtilities::StringHash("PATCH"),
        POST = AdCamiUtilities::StringHash("POST"),
        PUT = AdCamiUtilities::StringHash("PUT")
    };

    enum class EnumHttpStatusCode : unsigned int {
        Code200 = AdCamiUtilities::StringHash("200"),
        Code201 = AdCamiUtilities::StringHash("201"),
        Code204 = AdCamiUtilities::StringHash("204"),
        Code400 = AdCamiUtilities::StringHash("400"),
        Code404 = AdCamiUtilities::StringHash("404"),
        Code500 = AdCamiUtilities::StringHash("500")
    };

    EnumHttpMethod GetHttpMethod(const char *method);

    EnumHttpMethod GetHttpMethod(string const &method);

    unsigned int GetHttpStatusCodeInt(EnumHttpStatusCode code);

    unsigned int GetHttpStatusCodeInt(string const &code);

    EnumHttpStatusCode GetHttpStatusCode(const char *code);

    EnumHttpStatusCode GetHttpStatusCode(string const &code);

    } //namespace AdCamiHttpCommon

} //namespace AdCamiCommunications

using EnumHttpMethod = AdCamiCommunications::AdCamiHttpCommon::EnumHttpMethod;

#endif
