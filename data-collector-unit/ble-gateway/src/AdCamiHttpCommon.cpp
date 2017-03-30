//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#include "AdCamiHttpCommon.h"

namespace AdCamiCommunications {

namespace AdCamiHttpCommon {

EnumHttpMethod GetHttpMethod(const char* method) {
	return AdCamiUtilities::StringHash<EnumHttpMethod>(method);
}

EnumHttpMethod GetHttpMethod(string const& method) {
	return GetHttpMethod(method.c_str());
}

unsigned int GetHttpStatusCodeInt(EnumHttpStatusCode code) {
	switch (code) {
		case EnumHttpStatusCode::Code200: return 200;
		case EnumHttpStatusCode::Code201: return 201;
		case EnumHttpStatusCode::Code204: return 204;
		case EnumHttpStatusCode::Code400: return 400;
		case EnumHttpStatusCode::Code404: return 404;
		case EnumHttpStatusCode::Code500: return 500;
		default: return 0;
	}
}

unsigned int GetHttpStatusCodeInt(string const& code) {
	return GetHttpStatusCodeInt(AdCamiUtilities::StringHash<EnumHttpStatusCode>(code.c_str()));
}

EnumHttpStatusCode GetHttpStatusCode(const char* code) {
	return AdCamiUtilities::StringHash<EnumHttpStatusCode>(code);
}

EnumHttpStatusCode GetHttpStatusCode(string const& code) {
	return GetHttpStatusCode(code.c_str());
}

} //namespace AdCamiHttpCommon

} //namespace AdCamiHttpCommunications