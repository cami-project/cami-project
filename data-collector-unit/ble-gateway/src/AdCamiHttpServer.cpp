//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#include "AdCamiHttpServer.h"
#include <cstring>

namespace AdCamiCommunications {

AdCamiHttpServer::AdCamiHttpServer(const unsigned int port, const EnumConfiguration configuration) :
        _port(port), _daemon(NULL), _configuration(configuration),
        _requests(map < tuple < AdCamiUrl, EnumHttpMethod >, const AdCamiRequest*> ()),
        _certificatePemFile(nullptr), _certificateFilePath(nullptr), _keyPemFile(nullptr), _keyFilePath(nullptr),
        _rfds(new fd_set), _wfds(new fd_set), _efds(new fd_set), _maxfd(new int), _tv(new struct timeval) {

    /* If the ExternalSelect flag is not passed, then define the InternalSelect default one. */
    if (!(this->_configuration & EnumConfiguration::ExternalSelect)) {
        this->_configuration = static_cast<EnumConfiguration>(_configuration | EnumConfiguration::InternalSelect);
    }
}

AdCamiHttpServer::~AdCamiHttpServer() {
    delete[] this->_keyPemFile;
    delete[] this->_certificatePemFile;
}

void AdCamiHttpServer::AttendRequest() {
    if (!(this->_configuration & EnumConfiguration::ExternalSelect)) {
        PRINT_DEBUG("not EnumConfiguration::ExternalSelect");
        return;
    }

    if (MHD_YES != MHD_run(this->_daemon)) {
        PRINT_DEBUG("Error running MHD_run");
    }
    this->_UpdateDescriptors();
}

void AdCamiHttpServer::GetDescriptors(fd_set **rfds, fd_set **wfds, fd_set **efds, int **maxfd, struct timeval **tv) {
    this->_UpdateDescriptors();

    if (rfds != nullptr)
        *rfds = this->_rfds;
    if (wfds != nullptr)
        *wfds = this->_wfds;
    if (efds != nullptr)
        *efds = this->_efds;
    if (maxfd != nullptr)
        *maxfd = this->_maxfd;
    if (tv != nullptr)
        *tv = this->_tv;
}

void AdCamiHttpServer::SetCertificate(const string &certificate, const string &key) {
    size_t certificateLength = certificate.length();
    this->_certificateFilePath = new char[certificateLength + 1];
    std::memcpy(this->_certificateFilePath, certificate.c_str(), certificateLength);
    this->_certificateFilePath[certificateLength] = '\0';

    size_t keyLength = key.length();
    this->_keyFilePath = new char[keyLength + 1];
    std::memcpy(this->_keyFilePath, key.c_str(), keyLength);
    this->_keyFilePath[keyLength] = '\0';
}

AdCamiHttpServer::EnumServerState AdCamiHttpServer::Start() {
    /* Server must run with SSL. */
    if (this->_configuration & EnumConfiguration::Secure) {

        if (this->_certificateFilePath == nullptr || this->_keyFilePath == nullptr) {
            PRINT_LOG("The path(s) for the certificate and/or key PEM file(s) are not specified.");
            return AdCamiHttpServer::Error;
        }

        this->_certificatePemFile = this->_LoadFile(this->_certificateFilePath, this->_certificatePemFile);
        this->_keyPemFile = this->_LoadFile(this->_keyFilePath, this->_keyPemFile);
        if (this->_certificatePemFile == nullptr || this->_keyPemFile == nullptr) {
            PRINT_LOG("Could not find the certificate and/or key PEM file(s). Check if they exist.")
            return AdCamiHttpServer::Error;
        }

        /* Start libmicrohttpd. The 'this' object is passed as argument so it can be used
         * by the _AnswerRequest to search the request address. */
        this->_daemon = MHD_start_daemon(this->_configuration, this->_port,
                                         nullptr, nullptr, &_AnswerRequest, this,
                                         MHD_OPTION_HTTPS_MEM_KEY, this->_keyPemFile,
                                         MHD_OPTION_HTTPS_MEM_CERT, this->_certificatePemFile,
                                         MHD_OPTION_END);
    } else {
        this->_daemon = MHD_start_daemon(this->_configuration, this->_port,
                                         nullptr, nullptr, &_AnswerRequest, this,
                                         MHD_OPTION_END);
    }

    return (nullptr == this->_daemon) ? AdCamiHttpServer::Error : AdCamiHttpServer::Running;
}

AdCamiHttpServer::EnumServerState AdCamiHttpServer::Stop() {
    MHD_stop_daemon(this->_daemon);

    return AdCamiHttpServer::Stopped;
}

void AdCamiHttpServer::AddSyncRequestAction(const AdCamiUrl &url,
                                            const EnumHttpMethod method,
                                            AdCamiRequestCallback callback,
                                            void *callbackArg) {
    tuple <AdCamiUrl, EnumHttpMethod> key(url, method);
    AdCamiRequest *value = new AdCamiRequest(url, method, callback, callbackArg);

    this->_requests.insert(pair <
                           tuple < AdCamiUrl, EnumHttpMethod > ,
                           const AdCamiRequest*> (key, value));
}

/**
 * Register a vector of methods to be invoked when a HTTP request is received.
 * @param requests the vector with the requests to add
 */
void AdCamiHttpServer::AddSyncRequestAction(const vector <AdCamiRequest> &requests) {
    tuple <AdCamiUrl, EnumHttpMethod> key;

    for (auto it = requests.begin(); it != requests.end(); ++it) {
        key = std::make_tuple(std::get<0>(*it), std::get<1>(*it));

        this->_requests.insert(pair <
                               tuple < AdCamiUrl, EnumHttpMethod > ,
                                       const AdCamiRequest* > (key, &(*it)));
    }
}

int AdCamiHttpServer::_AnswerRequest(void *cls, struct MHD_Connection *connection,
                                     const char *url, const char *method,
                                     const char *version, const char *upload_data,
                                     size_t *upload_data_size, void **con_cls) {

    struct MHD_Response *response = nullptr;
    Request *request = static_cast<Request *>(*con_cls);
    unsigned int statusCode = MHD_HTTP_OK;
    /* HTTP objects that contain, respectively, the data received on the request
     * and the data that it is sent as request response. */
    AdCamiHttpData *requestData = (*con_cls == NULL ? NULL :
                                   static_cast<Request *>(*con_cls)->Data);
    AdCamiHttpData *responseData = new AdCamiHttpData("", 0, nullptr);
    /* cls function parameter has the this object so that _GetAction function
     * can be called. */
    void *callbackArg;
    AdCamiRequestCallback handler = static_cast<AdCamiHttpServer *>(cls)->_GetAction(
            url, AdCamiHttpCommon::GetHttpMethod(method), &callbackArg);
    EnumHttpStatusCode handlerResult = EnumHttpStatusCode::Code500;

    /* In case the URL is invalid, return "404 not found request". */
    if (!handler) {
        PRINT_LOG("Request " << method << " " << url << " received.");
        PRINT_LOG("\tHTTP code = 404");
        statusCode = MHD_HTTP_NOT_FOUND;
    }

    PRINT_DEBUG("---- new request: " << url << ", " << method);
    if (request == nullptr && (strcmp(method, MHD_HTTP_METHOD_POST) == 0 ||
                               strcmp(method, MHD_HTTP_METHOD_PUT) == 0 ||
                               strcmp(method, MHD_HTTP_METHOD_DELETE) == 0)) {
        request = new Request();
        *con_cls = request;
#ifdef DEBUG
        const char* encoding = MHD_lookup_connection_value(connection,
                                                           MHD_HEADER_KIND,
                                                           MHD_HTTP_HEADER_CONTENT_TYPE);
        PRINT_DEBUG("encoding = " << (encoding == nullptr ? "NULL" : encoding))
#endif
        return MHD_YES;
    }

    /* Handle data if it has payload. This can occur if the request is a POST, PUT or DELETE.
     * The "POST" processor was previously created. */
    if (strcmp(method, MHD_HTTP_METHOD_POST) == 0 ||
        strcmp(method, MHD_HTTP_METHOD_PUT) == 0 ||
        strcmp(method, MHD_HTTP_METHOD_DELETE) == 0) {
        if (*upload_data_size != 0) {
            /* Set data. */
            request->Data->SetData(reinterpret_cast<const byte *>(upload_data),
                                   *upload_data_size,
                                   true);
            /* Clear counter in case POST processor needs to be invoked. By
             * resetting upload_data_size to 0, it is assumed that there isn't
             * more data to process. */
            *upload_data_size = 0;
            return MHD_YES;
        }
    }

    /* Retrieve URL callback and execute it. The handler is again verified because
     * if a 400 error is gonna be sent there isn't any handler to be executed.
     * The error message is only sent on the end of this function. That way all
     * messages, error or not, are sent on the same point of the function. */
    if (handler != nullptr) {
        PRINT_LOG("Request "<< method << " " << url << " received.");
        handlerResult = handler(AdCamiUrl(url), *requestData, responseData, callbackArg);
        statusCode = GetHttpStatusCodeInt(handlerResult);
        PRINT_LOG("\tHTTP code = " << statusCode);
    }

    /* Create response and fill headers. */
    response = MHD_create_response_from_buffer(responseData->GetSize(),
                                               const_cast<void *>(responseData->GetData()),
                                               MHD_RESPMEM_MUST_FREE);
    if (response != nullptr) {
        if (responseData->GetMimeType() != "") {
            MHD_add_response_header(response,
                                    MHD_HTTP_HEADER_CONTENT_TYPE,
                                    responseData->GetMimeType().c_str());
        }
    }
    /* Queue/send response. */
    int ret = MHD_queue_response(connection, statusCode, response);

#ifdef DEBUG
    if (ret == MHD_YES)
        PRINT_DEBUG("response sent/queued")
    else if (ret == MHD_NO)
        PRINT_DEBUG("problem sending response")
    else
        PRINT_DEBUG("other error: " << errno)
#endif

    /* Clear objects. Note that MHD_destroy_response is also responsible for clearing the resources it needs.
     * Check: https://www.gnu.org/software/libmicrohttpd/manual/html_node/microhttpd_002dresponse-enqueue.html */
    MHD_destroy_response(response);

    return ret;
}

AdCamiRequestCallback AdCamiHttpServer::_GetAction(const char *url, const EnumHttpMethod method, void **callbackArg) {
    auto it = this->_requests.begin();

    for (; it != this->_requests.end() &&
           !(std::get<0>(it->first) == AdCamiUrl(url) &&
             std::get<1>(it->first) == method); ++it) { ; }

    if (it != this->_requests.end()) {
        *callbackArg = std::get<3>(*it->second);
        auto callback = std::get<2>(*it->second);
        return callback;
    } else {
        *callbackArg = nullptr;
        return nullptr;
    }
}

char *AdCamiHttpServer::_LoadFile(const char *filename, char *data) {
    FILE *fp;
    long size;

    fp = fopen(filename, "rb");
    if (!fp) {
        return nullptr;
    }
    /* Get file's size. */
    if ((0 != fseek(fp, 0, SEEK_END)) || (-1 == (size = ftell(fp)))) {
        fclose(fp);
        return nullptr;
    }
    /* Reset file pointer to the begin of file. */
    if (fseek(fp, 0, SEEK_SET) != 0) {
        fclose(fp);
        return nullptr;
    }

    data = new char[size + 1];
    data[size] = '\0';

    if (static_cast<size_t>(size) != fread(data, 1, size, fp)) {
        delete[] data;
        data = nullptr;
    }
    fclose(fp);

    return data;
}

void AdCamiHttpServer::_UpdateDescriptors() {
    MHD_UNSIGNED_LONG_LONG mhdTimeout;

    FD_ZERO(this->_rfds);
    FD_ZERO(this->_wfds);
    FD_ZERO(this->_efds);
    if (MHD_get_fdset(this->_daemon, this->_rfds, this->_wfds, this->_efds, this->_maxfd) != MHD_YES) {
        PRINT_DEBUG("Could not get the server's file descriptors");
    }

    if (MHD_get_timeout(this->_daemon, &mhdTimeout) == MHD_YES) {
        this->_tv->tv_sec = mhdTimeout / 1000;
        this->_tv->tv_usec = (mhdTimeout - (this->_tv->tv_sec * 1000)) * 1000;
    } else {
        this->_tv = nullptr;
    }
}

} //namespace
