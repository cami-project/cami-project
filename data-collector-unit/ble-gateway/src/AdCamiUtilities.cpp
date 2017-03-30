#include "AdCamiUtilities.h"
#include <cstring>
#include <fstream>

namespace AdCamiUtilities {

std::string &StringTrimEnd(std::string &s, const char *characters) {
    size_t pos = s.rfind(characters);

    /* If the sequence was found (most cases, hope so...) remove them from. */
    if (pos != std::string::npos) {
        s.resize(pos);
    }
        /* If the sequence was not found, clear character by character. */
    else {
        /* pos can be initialized at 0, because the value of npos is -1.
         * Check http://www.cplusplus.com/reference/string/string/npos/ */
        pos = 0;
        /* Search if any of the characters is on the string. */
        for (int i = 0; characters[i]; i++) {
            if ((pos = s.rfind(characters[i])) != std::string::npos)
                s.resize(pos);
        }
    }

    return s;
}

void ToHexString(const char *stream, unsigned int size, std::string &hexString) {
    static const char *const lut = "0123456789ABCDEF";

    hexString = "";
    hexString.reserve(2 * size);

    for (size_t i = 0; i < size; ++i) {
        const unsigned char c = stream[i];
        hexString.push_back(lut[c >> 4]);
        hexString.push_back(lut[c & 15]);
    }
}

int FromHexString(const std::string &input, byte *output) {
    static const char *const lut = "0123456789ABCDEF";
    size_t len = input.length();

    if (len & 1) {
        return 0;
    }

    for (size_t i = 0; i < len; i += 2) {
        char a = input[i];
        const char *p = std::lower_bound(lut, lut + 16, a);
        if (*p != a) {
            return i / 2;
        }

        char b = input[i + 1];
        const char *q = std::lower_bound(lut, lut + 16, b);
        if (*q != b) {
            return i / 2;
        }

        output[i / 2] = (((p - lut) << 4) | (q - lut));
    }

    return len / 2;
}

std::string GetDate(std::chrono::system_clock::time_point t) {
    auto as_time_t = std::chrono::system_clock::to_time_t(t);
    struct tm tm;
    char timeBuffer[64];

    if (::gmtime_r(&as_time_t, &tm)) {
        if (std::strftime(timeBuffer, sizeof(timeBuffer), "%F %T", &tm)) {
            return std::string{timeBuffer};
        }
    }
    return "";
}

template<>
void CastFromByte(const byte *src, int *dst) {
    *dst = *src;
}

template<>
void CastFromByte(const byte *src, uint16_t *dst) {
    *dst = *src;
}

}
