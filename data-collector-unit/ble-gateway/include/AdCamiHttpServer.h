//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiHttpServer_h
#define AdCamiDaemon_AdCamiHttpServer_h

#include <functional>
#include <iostream>
#include <map>
#include <tuple>
#include <vector>
#include <microhttpd.h>
#include "AdCamiHttpCommon.h"
#include "AdCamiHttpData.h"
#include "AdCamiUrl.h"

using AdCamiCommunications::AdCamiHttpData;
using AdCamiCommunications::AdCamiUrl;
using EnumActionResult = AdCamiCommunications::AdCamiHttpCommon::EnumActionResult;
using EnumHttpHeader = AdCamiCommunications::AdCamiHttpHeaders::EnumHttpHeader;
using EnumHttpMethod = AdCamiCommunications::AdCamiHttpCommon::EnumHttpMethod;
using EnumHttpStatusCode = AdCamiCommunications::AdCamiHttpCommon::EnumHttpStatusCode;
using std::map;
using std::pair;
using std::tuple;
using std::vector;

namespace AdCamiCommunications {
/**
 * Callback invoked when a HTTP request is received.
 */
using AdCamiRequestCallback = std::function<AdCamiHttpCommon::EnumHttpStatusCode(const AdCamiUrl &url,
                                                                                 const AdCamiHttpData &requestData,
                                                                                 AdCamiHttpData *response,
                                                                                 void *data)>;

/**
 * Type definition used to register the requests on the server.
 */
using _request_t = tuple<
        const string,
        const EnumHttpMethod,
        AdCamiCommunications::AdCamiRequestCallback,
        void *>;

class AdCamiRequest : public _request_t {
public:
    AdCamiRequest() : _request_t("", EnumHttpMethod::GET, nullptr, nullptr) {}

    AdCamiRequest(const string &url,
                  const EnumHttpMethod &method,
                  AdCamiCommunications::AdCamiRequestCallback clbk) : _request_t(url, method, clbk, nullptr) {}

    AdCamiRequest(const string &url,
                  const EnumHttpMethod &method,
                  AdCamiCommunications::AdCamiRequestCallback clbk,
                  void *arg) : _request_t(url, method, clbk, arg) {}

    ~AdCamiRequest() {}
};

/**
 *
 *
 * Useful page with libmicrohttpd doxygen:
 * http://ftp.heanet.ie/disk1/www.gnu.org/software/libmicrohttpd/doxygen/dc/d0c/microhttpd_8h.html
 */
class AdCamiHttpServer {
public:
    static const size_t kHttpServerDefaultPort = 80;
    static const size_t kHttpServerDefaultSecurePort = 443;

    /**
     * Enumeration with values used to configure how the server must execute.
     */
    enum EnumConfiguration : int {
        InternalSelect = MHD_USE_SELECT_INTERNALLY,
        ExternalSelect = MHD_USE_DEBUG,
        Secure = MHD_USE_SSL
    };
    /**
     * Enumeration with the possible server states. It is used by as return value by methods
     * Start() and Stop().
     */
    enum EnumServerState {
        Error = -1,
        Running = 0,
        Stopped = 1
    };

    AdCamiHttpServer(const unsigned int port = kHttpServerDefaultPort,
                     const EnumConfiguration mode = InternalSelect);

    ~AdCamiHttpServer();

    /**
     * Register a method to be invoked when a HTTP request is received.
     * @param url HTTP URL representing the request
     * @param method HTTP method (GET, POST, PUT)
     * @param callback method to be invoked when the request is received
     */
    void AddSyncRequestAction(const AdCamiUrl &url,
                              const EnumHttpMethod method,
                              AdCamiRequestCallback callback,
                              void *callbackArg = nullptr);

    /**
	 * Register a vector of methods to be invoked when a HTTP request is received.
     * @param requests the vector with the requests to add
     */
    void AddSyncRequestAction(const vector <AdCamiRequest> &requests);

    /**
     * Puts the server in waiting mode to receive requests. This method must only be invoked
     * when the server is using an external select().
     */
    void AttendRequest();

    /**
	 * Gets the port where the server is listening.
     * @return the port where the server is listening
     */
    inline const unsigned int &GetPort() const { return this->_port; };

    /**
     * Gets the descriptors of the server. These can be used when the server is using an external select().
     * Otherwise they must not be used.
     * @param rfds read file descriptor
     * @param wfds write file descriptor
     * @param efds exception file descriptor
     * @param maxfd maximum file descriptor
     * @param tv time struct that specifies waiting time
     */
    void GetDescriptors(fd_set **rfds, fd_set **wfds, fd_set **efds, int **maxfd, struct timeval **tv);

    /**
	 * Sets the certificates that will be used by the server when executed in secure mode. It is not
     * necessary to invoke this if hte server will not run in secure mode.
     * @param certificate path to the certificate file
     * @param key path to the key file
	 */
    void SetCertificate(const string &certificate, const string &key);

    /**
	 * Starts the HTTP server.
     * @return state of the server, i.e. if it is running, stopped or other state.
     */
    AdCamiHttpServer::EnumServerState Start();

    /**
	 * Stops the HTTP server.
     * @return state of the server, i.e. if it is running, stopped or other state.
     */
    AdCamiHttpServer::EnumServerState Stop();

private:
    class Request {
    public:
        /**
         * Data received on the request.
         */
        AdCamiHttpData *Data;

        /**
         * Default constructor.
         */
        Request() : Data(new AdCamiHttpData()) {}
    };

    /**
     *
     */
    unsigned int _port;
    /**
     */
    struct MHD_Daemon *_daemon;
    /**
     *
     */
    EnumConfiguration _configuration;
    /**
     *
     */
    map<tuple<AdCamiUrl, EnumHttpMethod>, const AdCamiRequest *> _requests;

    char *_certificatePemFile;
    char *_certificateFilePath;
    char *_keyPemFile;
    char *_keyFilePath;

    fd_set *_rfds;
    fd_set *_wfds;
    fd_set *_efds;
    int *_maxfd;
    struct timeval *_tv;

    /**
	 *
     */
    static int _AnswerRequest(void *cls, struct MHD_Connection *connection,
                              const char *url, const char *method,
                              const char *version, const char *upload_data,
                              size_t *upload_data_size, void **con_cls);

    /**
	 * Retrieve the action method that must be called when a request is received.
     * @param url URL of the request
     * @param method HTTP method associated with the URL (ex. GET, POST, PUT)
     * @return a callback function
     */
    AdCamiRequestCallback _GetAction(const char *url, const EnumHttpMethod method, void **callbackArg);

    /**
	 * Retrieve the content a file. This function is used to retrieve the contents of key
     * and certificate files.
     * @param filename the path to the certificate file
     * @return content of the certificate file
     */
    char *_LoadFile(const char *filename, char *data);

    /**
     * Updates the file descriptors. The descriptors can be used to read and write data,
     * if an external select is used.
     */
    void _UpdateDescriptors();
};

} //namespace

using EnumHttpServerConfiguration = AdCamiCommunications::AdCamiHttpServer::EnumConfiguration;

#endif
