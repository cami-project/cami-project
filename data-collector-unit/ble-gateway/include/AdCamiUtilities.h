//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiUtilities_h
#define AdCamiDaemon_AdCamiUtilities_h

#include <chrono>
#include <cstdint>
#include <ctime>
#include <iomanip>
#include <iostream>
#include <iterator>
#include <sstream>
#include <stdexcept>
#include <string>
#include <vector>
#include "AdCamiCommon.h"

using std::chrono::duration_cast;
using std::chrono::milliseconds;
using std::chrono::system_clock;
using std::ostream;
using std::string;

/* Debug macro that prints a message to the screen. This macro is only defined
 * if the flag DEBUG is defined on compile time. */
#if defined(DEBUG) || defined(VERBOSE)
#define PRINT_DEBUG(message) \
std::cout << "[" << __FILENAME__ << "::" << __FUNCTION__ << ":" << std::dec << __LINE__ << "] " << \
message << std::endl;
#else
#define PRINT_DEBUG(message)
#endif

/* Macro that prints log messages to a log file defined on LOG_FILE.  */
#define PRINT_LOG(message) \
std::clog << "[" \
          << AdCamiUtilities::GetDate(system_clock::now()) << "] " \
          << message << std::endl;

/* Macro that prints error messages to a log file defined on ERROR_FILE.  */
#define PRINT_ERROR(message) \
std::cerr << message << std::endl;

/* Abstract unsigned char as type byte. */
typedef std::uint8_t byte;

namespace AdCamiUtilities {

/* Alias for a buffer. */
template<typename T = byte, size_t N = 16>
class AdCamiBuffer : public std::vector<T> {
public:
    AdCamiBuffer() : std::vector<T>() {}

    AdCamiBuffer(const size_t &size) : std::vector<T>(size, 0x00) {}

    AdCamiBuffer(const size_t &size, const T &value) : std::vector<T>(size, value) {}

    AdCamiBuffer(const size_t size, const T *list) : std::vector<T>(size, 0x00) {
        this->assign(list, list + size);
    }

    AdCamiBuffer(const size_t size, const std::initializer_list <T> &list) : std::vector<T>(size, 0x00) {
        this->assign(list);
    }

    AdCamiBuffer(const std::initializer_list <T> &list) : std::vector<T>(list) {}

    AdCamiBuffer(const AdCamiBuffer &buffer) : std::vector<T>(buffer) {}

    operator std::string() const {
        std::string str;

        for (size_t i = 0; i < this->size(); i++) {
            str += this[i];
        }

        return str;
    }

    friend std::string operator+(std::string &&str, const AdCamiBuffer<T> buffer) {
        size_t size = buffer.size();

        for (size_t i = 0; i < size; i++) {
            str += static_cast<char>(buffer[i]);
        }

        return str;
    }

    friend ostream &operator<<(ostream &out, const AdCamiBuffer<T> buffer) {
        for (auto v : buffer) {
            out << "0x" << std::hex << static_cast<int>(v) << " ";
        }

        return out;
    }
};

/**
 *
 * @param t
 * @return
 * @note This code is borrowed from http://stackoverflow.com/questions/34963738/c11-get-current-date-and-time-as-string
 */
std::string GetDate(std::chrono::system_clock::time_point t);

/**
 * Calculates a hash value for a string.
 * @param str string to calculate hash
 * @param h position of the string from where the hash will start being calculated
 * @return a hash value
 */
template<typename R = unsigned int, typename T = const char *>
constexpr R StringHash(/*const char**/T str, int h = 0) {
    return static_cast<R>(!str[h] ? 5381 : (StringHash(str, h + 1) * 33) ^ str[h]);
}

/**
 * Removes specified characters from the end of the string.
 * @param s string to be trimmed
 * @param characters array of characters to search on the end of the string to
 *	remove. If no characters are specified, the function only searches for space
 *	characters
 * @return the trimmed string
 */
std::string &StringTrimEnd(std::string &s, const char *characters);

/**
 *
 */
void ToHexString(const char *stream, unsigned int size, std::string &hexString);

/**
 * Converts a hexadecimal string to bytes.
 * @param input string to convert
 * @param output converted string. This array must have half of the length of
 *  input. The function does not verify the limits.
 * @return the number of converted hexadecimal numbers. Each value of
 *  the string uses two digits, therefore the total length of the converted
 *  string is its length divided by 2.
 */
int FromHexString(const std::string &input, byte *output);

template<typename T>
std::string IntToHexString(T i) {
    std::stringstream stream;
    stream << std::setfill('0') << std::setw(sizeof(T) * 2)
           << std::hex << i;
    return stream.str();
}

template<typename T = string>
void CastFromByte(const byte *src, T *dst) {
    *dst = reinterpret_cast<const char *>(src);
}

template<>
void CastFromByte(const byte *src, int *dst);

template<>
void CastFromByte(const byte *src, uint16_t *dst);

} //namespace

#endif
