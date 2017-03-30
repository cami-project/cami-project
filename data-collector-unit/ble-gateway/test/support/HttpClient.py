import http.client, ssl
import sys, time


class HttpClientHandler(object):
    def __init__(self, request_url, method, pre_handler=None, post_handler=None):
        self.__request_url = request_url
        self.__method = method
        self.__pre = pre_handler
        self.__post = post_handler

    @property
    def request_url(self):
        return self.__request_url

    @property
    def method(self):
        return self.__method

    @property
    def pre_handler(self):
        return self.__pre

    @property
    def post_handler(self):
        return self.__post


class HttpClient(object):
    def __init__(self, server_address, is_secure=False):
        self.__server_address = server_address
        self.__is_secure = is_secure

    def request(self, handler):
        if self.__is_secure:
            conn = http.client.HTTPSConnection(self.__server_address, context=ssl._create_unverified_context())
        else:
            conn = http.client.HTTPConnection(self.__server_address)
        try:
            # Send request to server
            (response, response_body, headers) = self.__do_request(conn, handler)
            if response is not None:
                # Print log message
                self.__log_message("""\trequest = {} {}
    response headers = {}
    response status = {} {}
    response data = {}""".format(handler.method, handler.request_url,
                                 str(response.headers),
                                 response.status, response.reason,
                                 ("(none)" if response_body == "" else response_body)))
                return response, response_body
            else:
                print("There was a problem making the request {0}...".format(handler.request_url))
        except http.client.HTTPException as err:
            print("Connection error: {}".format(err))
        except ConnectionRefusedError:
            print("Connection refused! Check that the gateway daemon is running.")
        except Exception as err:
            print("Unknown error: {0}".format(err))
        finally:
            conn.close()

        return None

    def __do_request(self, connection, handler):
        try:
            if handler.method is "GET":
                connection.request(handler.method, handler.request_url)
            else:
                request_body = handler.pre_handler() if handler.pre_handler is not None else None
                headers = {"Content-type": "application/json",
                           "Accept": "application/json"}
                connection.request(handler.method, handler.request_url, request_body, headers)
        except Exception as err:
            print("connection exception = {0}".format(err))

        response = connection.getresponse()
        response_body = response.read().decode(encoding='UTF-8')
        headers = response.getheaders()

        if response.status == 200:
            if handler.post_handler is not None:
                handler.post_handler(response_body)
        return response, response_body, headers

    def __log_message(self, message=None):
        print("\n\nCLIENT [{} - {}]\n{}\n".format(self.__server_address,
                                                  time.strftime("%d/%b/%Y %H:%M:%S"),
                                                  message))
