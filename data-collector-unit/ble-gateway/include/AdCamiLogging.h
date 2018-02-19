//
// Created by Jorge Miguel Miranda on 22/11/2017.
//

#ifndef AdCamiDaemon_AdCamiLogging_h
#define AdCamiDaemon_AdCamiLogging_h

#include <chrono>
#include <iostream>
#include <sstream>

using std::chrono::high_resolution_clock;
using std::ostream;

namespace AdCamiUtilities {

enum EnumAdCamiLoggingMessageType {
    Debug,
    Error,
    Info,
    Warning
};

template<EnumAdCamiLoggingMessageType Tm = Info>
class AdCamiLogging {
public:
    static void ToMessages(const string &message) {
        std::clog << "[INFO "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }

//    static void ToDebug(const string &message) {
//        std::cout << "[INFO "
//                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
//                  << __FILENAME__
//                  << "::" << __FUNCTION__
//                  << ":" << std::dec
//                  << __LINE__ << "] "
//                  << message << std::endl;
//    }

//    static void ToDebug(char const *file, char const *function, long line, string const &message) {
//        std::cout << "[INFO "
//                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
//                  << file
//                  << "::" << function
//                  << ":" << std::dec
//                  << line << "] "
//                  << message << std::endl;
//    }

    static void ToDebug(const string &message, char const *file = __FILENAME__,
                        char const *function = __FUNCTION__, long line = __LINE__) {
#if defined(DEBUG) || defined(VERBOSE)
        std::cout << "[INFO "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now())
                  << " " << file << "::" << function << ":" << std::dec << line << "] "
                  << message << std::endl;
#endif
    }

    static void ToError(const string &message) {
        std::cerr << "[INFO "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }
};

template<>
class AdCamiLogging<Debug> {
public:
    static void ToMessages(const string &message) {
        std::clog << "[DEBUG "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }

    static void ToDebug(const string &message, char const *file = __FILENAME__,
                        char const *function = __FUNCTION__, long line = __LINE__) {
#if defined(DEBUG) || defined(VERBOSE)
        std::cout << "[DEBUG "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now())
                  << " " << file << "::" << function << ":" << std::dec << line << "] "
                  << message << std::endl;
#endif
    }

    static void ToError(const string &message) {
        std::cerr << "[DEBUG "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }
};

template<>
class AdCamiLogging<Error> {
public:
    static void ToMessages(const string &message) {
        std::clog << "[ERROR "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }

    static void ToDebug(const string &message, char const *file = __FILENAME__,
                        char const *function = __FUNCTION__, long line = __LINE__) {
#if defined(DEBUG) || defined(VERBOSE)
        std::cout << "[ERROR "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now())
                  << " " << file << "::" << function << ":" << std::dec << line << "] "
                  << message << std::endl;
#endif
    }

    static void ToError(const string &message) {
        std::cerr << "[ERROR "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }
};

template<>
class AdCamiLogging<Warning> {
public:
    static void ToMessages(const string &message) {
        std::clog << "[WARNING "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }

    static void ToDebug(const string &message, char const *file = __FILENAME__,
                        char const *function = __FUNCTION__, long line = __LINE__) {
#if defined(DEBUG) || defined(VERBOSE)
        std::cout << "[WARNING "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now())
                  << " " << file << "::" << function << ":" << std::dec << line << "] "
                  << message << std::endl;
#endif
    }

    static void ToError(const string &message) {
        std::cerr << "[WARNING "
                  << AdCamiUtilities::GetDate<EnumAdCamiTimeFormat::DateTime>(high_resolution_clock::now()) << "] "
                  << message << std::endl;
    }
};

}

using MessageType = AdCamiUtilities::EnumAdCamiLoggingMessageType;
template<MessageType T> using Log = typename AdCamiUtilities::template AdCamiLogging<T>;

//#define LogInfoToDebug(message) Log<MessageType::Info>::ToDebug(__FILE__, __FUNCTION__, __LINE__, message)
//#define LogDebugToDebug(message) Log<MessageType::Debug>::ToDebug(__FILE__, __FUNCTION__, __LINE__, message)
//#define LogErrorToDebug(message) Log<MessageType::Error>::ToDebug(__FILE__, __FUNCTION__, __LINE__, message)
//#define LogWarningToDebug(message) Log<MessageType::Warning>::ToDebug(__FILE__, __FUNCTION__, __LINE__, message)

#define LogInfoToDebug(message) Log<MessageType::Info>::ToDebug(message, __FILENAME__, __FUNCTION__, __LINE__)
#define LogDebugToDebug(message) Log<MessageType::Debug>::ToDebug(message, __FILENAME__, __FUNCTION__, __LINE__)
#define LogErrorToDebug(message) Log<MessageType::Error>::ToDebug(message, __FILENAME__, __FUNCTION__, __LINE__)
#define LogWarningToDebug(message) Log<MessageType::Warning>::ToDebug(message, __FILENAME__, __FUNCTION__, __LINE__)

#endif //AdCamiDaemon_AdCamiLogging_h
