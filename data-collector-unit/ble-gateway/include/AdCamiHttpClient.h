//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiHttpClient_h
#define AdCamiDaemon_AdCamiHttpClient_h

#include <iostream>
#include <curl/curl.h>
#include "AdCamiHttpData.h"
#include "AdCamiUrl.h"

using std::string;
using AdCamiCommunications::AdCamiHttpData;
using EnumHttpHeader = AdCamiCommunications::AdCamiHttpHeaders::EnumHttpHeader;

namespace AdCamiCommunications {

/**
 * This class can perform HTTP different requests to HTTP servers.
 */
class AdCamiHttpClient {
public:
    /**
     * Enumeration with the class different states and errors.
     */
    enum EnumHttpClientState : int {
        /** Action performed successfully */
		OK = 0,
        /** Response body written to an object */
		RESPONSE_WRITTEN = 1,
        /** Response header written to an object */
		DATA_WRITTEN = 2,
        /** The internal client object was not correctly initialized */
		CLIENT_NOT_INITIALIZED = -1,
		/** Unknown error */
        UNKNOWN_ERROR = -2,
        /** Response header couldn't be written to an object */
		NO_RESPONSE_HEADER_WRITTEN = -3,
        /** Response body couldn't be written to an object */
		NO_RESPONSE_DATA_WRITTEN = -4,
        /** Could not connect to server. The most probable reason is a disconnected server */
        CONNECTING_SERVER_ERROR = -5
    };
    
    /**
     * Default constructor. The constructor only initializes the object. All
     * connections are open when a request is made. This way, one client can
     * handle multiple requests to different servers.
     */
	//TODO change to unsigned int
	AdCamiHttpClient(const int port = _DefaultPort);
    /**
     * Default destructor. The destructor closes all open connections.
     */
	~AdCamiHttpClient();
    
    /**
     * Make a HTTP GET request. The request is made using HTTP's default port (80).
     * The URL address must follow the format specified on RFC 3986. For more
     * information see http://curl.haxx.se/libcurl/c/curl_easy_setopt.html#CURLOPTURL
     * @param url		HTTP request address. This address must have the server's
     *	address with the resource specified at the end (example: http://127.0.0.1:8080/resource)
     * @param response	pointer to an object where the request's answer will be written
     * @return AdCamiHttpClient::OK if the request is successfully made, or other
     *	enumeration value otherwise
     */
	EnumHttpClientState Get(const AdCamiUrl& url, AdCamiHttpData* response);
    
    /**
     * Make a HTTP GET request. The URL address must follow the format specified on
     * RFC 3986. For more information see http://curl.haxx.se/libcurl/c/curl_easy_setopt.html#CURLOPTURL
     * @param url		HTTP request address. This address must have the server's address
     *	with the resource specified at the end (example: http://127.0.0.1:8080/resource)
     * @param port		server's port where the address must be made
     * @param response	pointer to an object where the request's answer will be written
     * @return AdCamiHttpClient::OK if the request is successfully made, or other
     *	enumeration value otherwise
     */
	EnumHttpClientState Get(const AdCamiUrl& url, const int port, AdCamiHttpData* response);
    
    /**
     * Make a HTTP GET request. The request is made using HTTP's default port (80).
     * The URL address must follow the format specified on RFC 3986. For more
     * information see http://curl.haxx.se/libcurl/c/curl_easy_setopt.html#CURLOPTURL
     * @param url		HTTP request address. This address must have the server's
     *	address with the resource specified at the end (example: http://127.0.0.1:8080/resource)
     * @param sendData	pointer to an object with the data to be sent
     * @param response	pointer to an object where the request's answer will be written.
     *	If no object is specified, the response is not written
     * @return AdCamiHttpClient::OK if the request is successfully made, or other
     *	enumeration value otherwise
     */
	EnumHttpClientState Post(const AdCamiUrl& url, AdCamiHttpData* sendData, AdCamiHttpData* response = nullptr);
    
    /**
     * Make a HTTP GET request. The request is made using HTTP's default port (80).
     * The URL address must follow the format specified on RFC 3986. For more
     * information see http://curl.haxx.se/libcurl/c/curl_easy_setopt.html#CURLOPTURL
     * @param url		HTTP request address. This address must have the server's
     *	address with the resource specified at the end (example: http://127.0.0.1:8080/resource)
     * @param port		server's port where the address must be made
     * @param sendData	pointer to an object with the data to be sent
     * @param response	pointer to an object where the request's answer will be written.
     *	If no object is specified, the response is not written
     * @return AdCamiHttpClient::OK if the request is successfully made, or other
     *	enumeration value otherwise
     */
	EnumHttpClientState Post(const AdCamiUrl& url,
							 const int port,
							 AdCamiHttpData* sendData,
							 AdCamiHttpData* response = nullptr);

    AdCamiHttpClient &Insecure(const bool &insecure = true);

	AdCamiHttpClient &SetPassword(const string &password);

	AdCamiHttpClient &SetUsername(const string &username);
    
private:
	CURL* _client;
    int _port;
	
	static const int _DefaultPort = 80;
    
    /**
     * Gets HTTP client's error codes and messages. The error codes can have
     * various origins, like libCURL errors or class internal errors. Beside the
     * error handling, the function writes a message to the error log and, if
     * enabled, to debug output (std::out by default). IMPORTANT: the error log
     * must be enabled s othe error messages can be written.
     * @param error		code caught during execution os a class function
     * @param exmessage	extra message to eb appended to the defautl error message
     * @return an enumaration value with the error that occurred
     */
    EnumHttpClientState _ErrorMessage(const int error, const std::string& exmessage = "");
    
    /**
     * Write a HTTP message body on a AdCamiHttpData object, when a HTTP message is
     * received. The object is passed as argument on userdata. This function is an
     * implementation of the callback used by libCURL when the option CURLOPT_WRITEFUNCTION
     * is initialized (see http://curl.haxx.se/libcurl/c/curl_easy_setopt.html)
     * @param buffer	constains the received HTTP message
     * @param size		number of bytes received
     * @param nmemb		number of members received
     * @param userdata	a AdCamiHttpData object where the response will be stored.
     *	This argument is set with libCURL's CURLOPT_WRITEDATA option
     * @return the number of bytes actually taken care of. If that amount differs
     *	from the amount passed to your function, it'll signal an error to the
     *	library. This will abort the transfer and return CURLE_WRITE_ERROR
     */
	static size_t _ResponseDataClbk(char *buffer, size_t size, size_t nmemb, void *userdata);
    
    /**
     * Write a HTTP header on a AdCamiHttpData object, when a HTTP message is received.
     * The object is passed as argument on userdata. This function is an
     * implementation of the callback used by libCURL when the option CURLOPT_WRITEFUNCTION
     * is initialized (see http://curl.haxx.se/libcurl/c/curl_easy_setopt.html)
     * @param buffer	constains the received HTTP message
     * @param size		number of bytes received
     * @param nmemb		number of members received
     * @param userdata	a AdCamiHttpData object where the response will be stored.
     *	This argument is set with libCURL's CURLOPT_WRITEDATA option
     * @return the number of bytes actually taken care of. If that amount differs
     *	from the amount passed to your function, it'll signal an error to the
     *	library. This will abort the transfer and return CURLE_WRITE_ERROR
     */
    static size_t _ResponseHeaderClbk(char *buffer, size_t size, size_t nmemb, void *userdata);
};

} //namespace
#endif

