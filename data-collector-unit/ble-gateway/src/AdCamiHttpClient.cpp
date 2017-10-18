//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#include "AdCamiHttpClient.h"
#include "AdCamiUtilities.h"

namespace AdCamiCommunications {

AdCamiHttpClient::AdCamiHttpClient(const int port) : _port(port) {
    this->_client = curl_easy_init();
    curl_easy_setopt(this->_client, CURLOPT_NOSIGNAL, 1);
    curl_easy_setopt(this->_client, CURLOPT_TIMEOUT, 10);
    curl_easy_setopt(this->_client, CURLOPT_FOLLOWLOCATION, 1L);
    curl_easy_setopt(this->_client, CURLOPT_WRITEFUNCTION, _ResponseDataClbk);
    curl_easy_setopt(this->_client, CURLOPT_HEADERFUNCTION, _ResponseHeaderClbk);
    curl_easy_setopt(this->_client, CURLOPT_REDIR_PROTOCOLS, CURLPROTO_HTTP | CURLPROTO_HTTPS);
#ifdef DEBUG
    curl_easy_setopt(this->_client, CURLOPT_VERBOSE, 1);
#endif
}

AdCamiHttpClient::~AdCamiHttpClient() {
    /* Clean libCURL handlers. */
    curl_easy_cleanup(this->_client);
}

AdCamiHttpClient::EnumHttpClientState AdCamiHttpClient::Get(const AdCamiUrl &url, AdCamiHttpData *response) {
    return this->Get(url, this->_port, response);
}

AdCamiHttpClient::EnumHttpClientState
AdCamiHttpClient::Get(const AdCamiUrl &url, const int port, AdCamiHttpData *response) {
    if (this->_client) {
        CURLcode res;

        /* If a different port than the default one is specified, set it. */
        if (port != _DefaultPort)
            curl_easy_setopt(this->_client, CURLOPT_PORT, port);
        /* Set where argument 'reponse' as the object to be used when _ResponseData is
         * invoked and set the URL of the request. */
        curl_easy_setopt(this->_client, CURLOPT_WRITEDATA, response);
        curl_easy_setopt(this->_client, CURLOPT_WRITEHEADER, response);
        curl_easy_setopt(this->_client, CURLOPT_URL, url.c_str());
        //curl_easy_setopt(this->_client, CURLOPT_ACCEPT_ENCODING, "application/json; charset=utf-8");
        /* Perform request */
        res = curl_easy_perform(this->_client);

        return _ErrorMessage(res, "GET request '" + url + "'");
    } else {
        return _ErrorMessage(CLIENT_NOT_INITIALIZED);
    }
}

AdCamiHttpClient::EnumHttpClientState
AdCamiHttpClient::Post(const AdCamiUrl &url, AdCamiHttpData *sendData, AdCamiHttpData *response) {
    return this->Post(url, this->_port, sendData, response);
}

AdCamiHttpClient::EnumHttpClientState
AdCamiHttpClient::Post(const AdCamiUrl &url, const int port, AdCamiHttpData *sendData, AdCamiHttpData *response) {
    if (this->_client) {
        CURLcode res;

        /* If a different port than the default one is specified, set it. */
        if (port != _DefaultPort)
            curl_easy_setopt(this->_client, CURLOPT_PORT, port);
        /* First set the URL that is about to receive our POST. */
        curl_easy_setopt(this->_client, CURLOPT_WRITEDATA, response);
        curl_easy_setopt(this->_client, CURLOPT_WRITEHEADER, response);
        curl_easy_setopt(this->_client, CURLOPT_URL, url.c_str());
        /* Now specify we want to POST data */
        curl_easy_setopt(this->_client, CURLOPT_POST, 1L);
        /* Set the expected POST size. If you want to POST large amounts of data,
         * consider CURLOPT_POSTFIELDSIZE_LARGE */
        curl_easy_setopt(this->_client, CURLOPT_HTTP_VERSION, CURL_HTTP_VERSION_1_1);
        curl_easy_setopt(this->_client, CURLOPT_POSTFIELDSIZE, sendData->GetSize());
        curl_easy_setopt(this->_client, CURLOPT_POSTFIELDS, sendData->GetData());
        if (sendData->Headers.Size() > 0) {
            curl_slist *headerlist = nullptr;
            for (auto header : sendData->Headers) {
                headerlist = curl_slist_append(headerlist, sendData->Headers.GetHeaderValue(header.first).c_str());
                curl_easy_setopt(this->_client, CURLOPT_HTTPHEADER, headerlist);
            }
        }
        /* Perform request. */
        res = curl_easy_perform(this->_client);

        return _ErrorMessage(res, "POST request '" + url + "'");
    } else {
        return _ErrorMessage(CLIENT_NOT_INITIALIZED);
    }
}

AdCamiHttpClient &AdCamiHttpClient::Insecure(const bool &insecure) {
    curl_easy_setopt(this->_client, CURLOPT_SSL_VERIFYPEER, insecure ? 0 : 1L);

    return *this;
}

AdCamiHttpClient &AdCamiHttpClient::SetPassword(const string &password) {
    curl_easy_setopt(this->_client, CURLOPT_HTTPAUTH, CURLAUTH_BASIC);
    curl_easy_setopt(this->_client, CURLOPT_PASSWORD, password.c_str());

    return *this;
}

AdCamiHttpClient &AdCamiHttpClient::SetUsername(const string &username) {
    curl_easy_setopt(this->_client, CURLOPT_HTTPAUTH, CURLAUTH_BASIC);
    curl_easy_setopt(this->_client, CURLOPT_USERNAME, username.c_str());

    return *this;
}

AdCamiHttpClient::EnumHttpClientState AdCamiHttpClient::_ErrorMessage(const int error, const std::string &exmessage) {
    PRINT_DEBUG("error = " << error)
    switch (error) {
        case CURLE_OK: {
            return AdCamiHttpClient::OK;
        }
        case CLIENT_NOT_INITIALIZED: {
            PRINT_ERROR("HTTP Client not initialized!")
            return AdCamiHttpClient::CLIENT_NOT_INITIALIZED;
        }
        case CURLE_COULDNT_CONNECT: {
            PRINT_ERROR("Couldn't connect to server! " << exmessage)
            return AdCamiHttpClient::CONNECTING_SERVER_ERROR;
        }
        default:
            return static_cast<EnumHttpClientState>(error);//AdCamiHttpClient::UNKNOWN_ERROR;
    }
}

size_t AdCamiHttpClient::_ResponseDataClbk(char *buffer, size_t size, size_t nmemb, void *userdata) {
    AdCamiHttpData *response = static_cast<AdCamiHttpData *>(userdata);

    if (response != nullptr) {
        response->SetData(buffer, size * nmemb);
    }

    return size * nmemb;
}

size_t AdCamiHttpClient::_ResponseHeaderClbk(char *buffer, size_t size, size_t nmemb, void *userdata) {
    AdCamiHttpData *response = static_cast<AdCamiHttpData *>(userdata);

    if (response != nullptr) {
        response->Headers.SetValue(buffer);
    }

    return size * nmemb;
}

} //namespace
