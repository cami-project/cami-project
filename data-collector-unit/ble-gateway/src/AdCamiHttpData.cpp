//
//  Created by Jorge Miguel Miranda on 19/11/13.
//

#include "AdCamiHttpData.h"

namespace AdCamiCommunications {

AdCamiHttpData::AdCamiHttpData() : _mimeType(""), _size(0), _data(nullptr) {}

AdCamiHttpData::AdCamiHttpData(const string &mimetype, unsigned int size, const void *data) :
        _mimeType(mimetype), _size(size) {
    this->_data = static_cast<byte *>(malloc(size));
    memcpy(this->_data, data, size);
    this->Headers.SetValue(EnumHttpHeader::ContentType, this->_mimeType);
}

AdCamiHttpData::~AdCamiHttpData() {
    free((void *) this->_data);
}

void AdCamiHttpData::SetData(const byte *data, const size_t &size, bool grow) {
    /* Only grow the array if there is no data already on it. */
    if (grow == true && this->_size > 0) {
        byte *tmp = static_cast<byte *>(realloc(this->_data, this->_size + size));
        if (tmp != nullptr) {
            this->_data = tmp;
            memcpy(this->_data + this->_size, data, size);
            this->_size += size;
        }
    } else {
        /* If internal data array's size and the new one aren't equal, then
         * resize it. */
        if (this->_size != size) {
            free(this->_data);
            this->_data = static_cast<byte *>(malloc(size * sizeof(byte)));
        } else if (this->_data == nullptr) {
            this->_data = static_cast<byte *>(malloc(size * sizeof(byte)));
        }
        /* Copy data to the data pointer. */
        if (data != nullptr && size > 0) {
            this->_data = static_cast<byte *>(memcpy(this->_data, data, size));
            this->_size = size;
        } else {
            this->_size = 0;
        }
    }
}

AdCamiHttpData &AdCamiHttpData::operator=(const AdCamiHttpData &data) {
    this->_mimeType = data._mimeType;
    this->SetData(data._data, data._size);
    this->_size = data._size;

    return *this;
}

ostream &operator<<(ostream &os, const AdCamiHttpData &response) {
    for (unsigned int i = 0; i < response.GetSize(); i++) {
        os << ((byte *) response._data)[i];
    }

    return os;
}

} //namespace
