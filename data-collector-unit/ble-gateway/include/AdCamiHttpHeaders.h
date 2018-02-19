//
//  Created by Jorge Miguel Miranda on 29/01/14.
//

#ifndef ADCAMID_ADCAMIHTTPHEADERS_H
#define ADCAMID_ADCAMIHTTPHEADERS_H

#include <map>
#include <string>
#include "AdCamiUtilities.h"

using std::map;
using std::string;

namespace AdCamiCommunications {

class AdCamiHttpHeaders {
public:
    enum EnumHttpHeader {
        AcceptEncoding = AdCamiUtilities::StringHash("Accept-Encoding"),
        Authorization = AdCamiUtilities::StringHash("Authorization"),
        ContentType	= AdCamiUtilities::StringHash("Content-Type"),
        ContentLength = AdCamiUtilities::StringHash("Content-Length"),
        HttpVersion,
        HttpVersion10 = AdCamiUtilities::StringHash("HTTP/1.0"),
        HttpVersion11 = AdCamiUtilities::StringHash("HTTP/1.1"),
        ResponseStatusCode,
        ResponseStatusMessage
    };

    using iterator = std::map<EnumHttpHeader, void*>::iterator;
    using const_iterator = std::map<EnumHttpHeader, void*>::const_iterator;
    
    AdCamiHttpHeaders() : _fields(map<EnumHttpHeader, void*>()) {}
    ~AdCamiHttpHeaders();

    iterator begin() noexcept {
        return this->_fields.begin();
    }

    const_iterator cbegin() const noexcept {
        return this->_fields.cbegin();
    }

    iterator end() noexcept {
        return this->_fields.end();
    }

    const_iterator cend() const noexcept {
        return this->_fields.cend();
    }

	/**
	 * Gets the value of the field indicated by EnumHttpHeaders.
	 * By default, the return value is a string, but other types
	 * can be defined by the template argument.
	 * @param field EnumHttpHeader enumeration value of the header
	 *	to retrieve
	 * @return the value of the HTTP header field
	 */
    template<typename T = string>
    T GetValue(EnumHttpHeader field) {
        auto it = this->_fields.find(field);

        return it != this->_fields.end() ? *static_cast<T*>(it->second) : "";
    }
    
    /**
     *
     */
    string GetHeaderAndValue(EnumHttpHeader field);

    /**
     * Adds a header string with the format "<header>: <field>" to the
     * headers collection. This method is useful to parse headers that
     * where received after a request.
     * @param header a string with the header and its value
     */
    void SetValue(const char* header);

    /**
     * Adds a header and its value to the collection of headers.
     * @param header    a EnumHttpHeader with the header
     * @param value     the value to be set for the header
     */
    void SetValue(const EnumHttpHeader header, const string& value);

    bool Size() { return this->_fields.size(); }

private:
    class _HttpHeaderInfo {
    public:
        string AsString;
        unsigned int Length;
    
        _HttpHeaderInfo(const string& str, const unsigned int& length) : AsString(str), Length(length) {}
    };

    static const unsigned int _kAcceptEncoding;
    static const unsigned int _kAuthorizationLength;
    static const unsigned int _kContentTypeLength;
    static const unsigned int _kContentLengthLength;
    static const unsigned int _kHttpLength;
    static const unsigned int _kResponseStatusCodeLength;
    static const map<EnumHttpHeader, _HttpHeaderInfo> _kHeaderInfo;
    
    map<EnumHttpHeader, void*> _fields;
};
    
}

#endif //ADCAMID_ADCAMIHTTPHEADERS_H
