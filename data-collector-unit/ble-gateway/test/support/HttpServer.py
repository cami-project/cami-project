from http.server import BaseHTTPRequestHandler, HTTPServer


class HttpServer(HTTPServer):
    def __init__(self, port=80):
        HTTPServer.__init__(self, ('', port), self._HttpRequestHandler)
        self.execute_after_request = None
        self.__requests = {}

    def add_request_handler(self, request, method, handler):
        self.__requests[(request, method)] = handler
        return

    def get_request_handler(self, request, method):
        return self.__requests[(request, method)]

    @property
    def execute_after_request(self):
        return self.__execute_after_request

    @execute_after_request.setter
    def execute_after_request(self, method):
        self.__execute_after_request = method

    def run(self):
        HTTPServer.serve_forever(self)

    def shutdown(self):
        HTTPServer.shutdown(self)
        return

    # Create custom HTTPRequestHandler class
    class _HttpRequestHandler(BaseHTTPRequestHandler):
        def do_GET(self):
            try:
                request_clbk = self.server.get_request_handler(self.path, "GET")
                response_body = request_clbk()
                response_code = 200
            except KeyError:
                response_code = 400
                response_body = '{"error":"bad request"}'

            self.send_response(response_code, response_body.encode("utf-8"))
            self.send_header("Content-type", "application/json")
            self.end_headers()
            self._log_message("""\trequest = {} {}
	response code = {}
	response data = {}""".format(self.command, self.path,
                                 response_code,
                                 ("(none)" if response_body == "" else response_body)))
            # Execute after request handler
            if self.server.execute_after_request is not None:
                self.server.execute_after_request()
            return

        def do_POST(self):
            try:
                request_clbk = self.server.get_request_handler(self.path, "POST")
                # Read POST body data
                request_length = int(self.headers['Content-Length'])
                request_type = self.headers['Content-Type']
                received_data = self.rfile.read(request_length).decode("utf-8")
                # Execute callback
                clbk_return = request_clbk(received_data)
                response_body = (clbk_return if clbk_return is not None else "")
                response_code = 200
            except KeyError as err:
                print("err = " + str(err))
                response_code = 400
                request_type = "(none)"
                response_body = '{"error":"bad request"}'
                received_data = ""

            self.send_response(response_code, response_body.encode("utf-8"))
            self.send_header("Content-type", "application/json")
            self.end_headers()
            self._log_message("""\trequest = {} {}
	request data type = {}
	request data = {}
	response code = {}
	response data = {}""".format(self.command, self.path,
                                 request_type,
                                 received_data,
                                 response_code,
                                 ("(none)" if response_body == "" else response_body)))
            # Execute after request handler
            if self.server.execute_after_request is not None:
                self.server.execute_after_request()
            return

        def log_message(self, message, *args):
            return

        def _log_message(self, message=None):
            print("\n\nSERVER [{} - {}]\n{}\n".format(self.client_address[0],
                                                      self.log_date_time_string(),
                                                      message))
