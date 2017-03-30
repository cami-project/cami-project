//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiResponse_h
#define AdCamiDaemon_AdCamiResponse_h

#include <cstdlib>
#include <cstring>
#include <iostream>
#include "AdCamiHttpHeaders.h"
#include "AdCamiUtilities.h"

using std::ostream;
using std::string;
using EnumHttpHeader = AdCamiCommunications::AdCamiHttpHeaders::EnumHttpHeader;

namespace AdCamiCommunications {

class AdCamiHttpData {
private:
    string _mimeType;
    size_t _size;
    byte *_data;

public:
    /**
     * Access the request headers. Use this attribute to modify the content of the headers.
     */
    AdCamiHttpHeaders Headers;

    AdCamiHttpData();

    AdCamiHttpData(const string &mimetype, unsigned int size, const void *data);

    ~AdCamiHttpData();

    inline void SetMimeType(const string &mimetype) {
        this->_mimeType = mimetype;
        this->Headers.SetValue(EnumHttpHeader::ContentType, this->_mimeType);
    }

    /**
     * Sets the data of the HTTP object. If it already contains some data, then
     * new one can be added by providing true on the argument grow.
     * @param data data to store
     * @param size data's size
     * @param grow if true, the new data is added to the object; false overrides
     *	the previous data (default option)
     */
    void SetData(const byte *data, const size_t &size, bool grow = false);

    /**
     *
     */
    void SetData(const string &data, bool grow = false) {
        SetData(reinterpret_cast<const byte *>(data.c_str()), static_cast<const size_t>(data.size()), grow);
    }

    /**
	 *
	 */
    inline const string &GetMimeType() const { return this->_mimeType; }

    inline const void *GetData() const { return this->_data; }

    inline string GetDataAsString() const { return string((const char *) this->_data, this->_size); }

    inline size_t GetSize() const { return this->_size; }

    friend ostream &operator<<(ostream &os, const AdCamiHttpData &response);

    AdCamiHttpData &operator=(const AdCamiHttpData &data);
};

} //namespace
#endif
