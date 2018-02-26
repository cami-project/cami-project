//
//  Created by Jorge Miguel Miranda on 29/01/14.
//

#include "AdCamiHttpHeaders.h"

using EnumHttpHeader = AdCamiCommunications::AdCamiHttpHeaders::EnumHttpHeader;

namespace AdCamiCommunications {

const unsigned int AdCamiHttpHeaders::_kAcceptEncoding = 15;
const unsigned int AdCamiHttpHeaders::_kAuthorizationLength = 13;
const unsigned int AdCamiHttpHeaders::_kContentLengthLength = 14;
const unsigned int AdCamiHttpHeaders::_kContentTypeLength = 12;
const unsigned int AdCamiHttpHeaders::_kHttpLength = 8;
const unsigned int AdCamiHttpHeaders::_kResponseStatusCodeLength = 3;

const map <EnumHttpHeader, AdCamiHttpHeaders::_HttpHeaderInfo> AdCamiHttpHeaders::_kHeaderInfo = {
        {EnumHttpHeader::AcceptEncoding, _HttpHeaderInfo("Accept-Encoding", _kAcceptEncoding)},
        {EnumHttpHeader::Authorization,  _HttpHeaderInfo("Authorization", _kAuthorizationLength)},
        {EnumHttpHeader::ContentLength,  _HttpHeaderInfo("Content-Length", _kContentLengthLength)},
        {EnumHttpHeader::ContentType,    _HttpHeaderInfo("Content-Type", _kContentTypeLength)}
};

AdCamiHttpHeaders::~AdCamiHttpHeaders() {
    this->_fields.clear();
}

string AdCamiHttpHeaders::GetHeaderAndValue(EnumHttpHeader field) {
    string header;
    auto it = _kHeaderInfo.find(field);

    if (it != _kHeaderInfo.end()) {
        header = it->second.AsString;
    }

    return header + ": " + this->GetValue(field);
}

void AdCamiHttpHeaders::SetValue(const char *field) {
    string fieldString(field);
    int separator = fieldString.find_first_of(": ");
    EnumHttpHeader fieldHash = AdCamiUtilities::StringHash<EnumHttpHeader>(fieldString.substr(0, separator).c_str());

    /* Remove end-of-line characters. */
    fieldString = AdCamiUtilities::StringTrimEnd(fieldString, "\r\n");

    /* The +2 on the substr function is to skip the colon and the space between the header and the value of it. */
    switch (fieldHash) {
        case EnumHttpHeader::AcceptEncoding:
            this->_fields[EnumHttpHeader::AcceptEncoding] = new string(fieldString.substr(_kAcceptEncoding + 2));
        case EnumHttpHeader::Authorization:
            this->_fields[EnumHttpHeader::Authorization] = new string(fieldString.substr(_kAuthorizationLength + 2));
        case EnumHttpHeader::HttpVersion10:
        case EnumHttpHeader::HttpVersion11:
            this->_fields[EnumHttpHeader::HttpVersion] = new string(fieldString.substr(0, _kHttpLength));
            this->_fields[EnumHttpHeader::ResponseStatusCode] = new string(
                    fieldString.substr(9, _kResponseStatusCodeLength));
            this->_fields[EnumHttpHeader::ResponseStatusMessage] = new string(fieldString.substr(13));
            break;
        case EnumHttpHeader::ContentType:
            this->_fields[EnumHttpHeader::ContentType] = new string(fieldString.substr(_kContentTypeLength + 2));
            break;
        case EnumHttpHeader::ContentLength:
            this->_fields[EnumHttpHeader::ContentLength] = new string(fieldString.substr(_kContentLengthLength + 2));
            break;
        default:
            break;
    }
}

void AdCamiHttpHeaders::SetValue(const EnumHttpHeader header, const string &value) {
    switch (header) {
        case EnumHttpHeader::HttpVersion:
        case EnumHttpHeader::HttpVersion10:
        case EnumHttpHeader::HttpVersion11:
            this->_fields[EnumHttpHeader::HttpVersion] = new string(value);
            break;
        default:
            this->_fields[header] = new string(value);
            break;
    }
}

} // namespace
