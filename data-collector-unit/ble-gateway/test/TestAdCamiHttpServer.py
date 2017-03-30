from support.HttpClient import HttpClient, HttpClientHandler
import json, os, signal, sys, threading
import unittest

ADCAMI_ADDRESS = "localhost:60773"
ADCAMI_SECURE_ADDRESS = "localhost:60774"

# Initialize HTTP client. By default, the address of the gateway is 127.0.0.1,
# but if an IP is given as argument of the program, then use it.
# adcami_address = (sys.argv[1] if len(sys.argv) > 1 else ADCAMI_ADDRESS)
http_client = HttpClient(ADCAMI_ADDRESS)
https_client = HttpClient(ADCAMI_SECURE_ADDRESS, True)


class TestAdCamiHttpServer(unittest.TestCase):
    # Test if the request to get events on the gateway responds.
    # TODO missing test received payload
    def test_get_events(self):
        with open('test_get_events.json', 'r') as file:
            expected_response_body_json = json.loads(file.read())

        request = HttpClientHandler('/events', 'GET')
        (response, body) = http_client.request(request)
        (response_secure, body_secure) = https_client.request(request)

        self.assertEqual(response.status, 200)
        self.assertEqual(json.loads(body), expected_response_body_json)

        self.assertEqual(response_secure.status, 200)
        self.assertEqual(json.loads(body_secure), expected_response_body_json)

    # Test if the request to set BLE devices to listen to responds.
    def test_valid_set_devices(self):
        request = HttpClientHandler('/devices', 'PUT')
        (response, body) = http_client.request(request)
        (response_secure, body_secure) = https_client.request(request)

        self.assertEqual(response.status, 200)
        self.assertEqual(response_secure.status, 200)

    # Test if the request for setting the open tele credentials
    def test_valid_set_credentials(self):
        request = HttpClientHandler('/management/credentials', 'PUT',
                                    pre_handler=lambda: '{"username":"test_username","password":"test_password"}')
        (response, body) = http_client.request(request)
        (response_secure, body_secure) = https_client.request(request)

        self.assertEqual(response.status, 200)
        self.assertEqual(response_secure.status, 200)

    # Test if the request to set the gateway name responds.
    def test_valid_set_gateway_name(self):
        request = HttpClientHandler('/management/gateway', 'PUT',
                                    pre_handler=lambda: '{"name":"new gateway name"}')
        (response, body) = http_client.request(request)
        (response_secure, body_secure) = https_client.request(request)

        self.assertEqual(response.status, 200)
        self.assertEqual(response_secure.status, 200)

    # Test if the request for setting the endpoint responds.
    def test_valid_set_endpoint(self):
        request = HttpClientHandler('/management/endpoint', 'PUT',
                                    pre_handler=lambda: '{"endpoint":"https://aliviate.endpoint"}')
        (response, body) = http_client.request(request)
        (response_secure, body_secure) = https_client.request(request)

        self.assertEqual(response.status, 200)
        self.assertEqual(response_secure.status, 200)

    # Test if an invalid URL will return 404 from the server.
    def test_invalid_url_request(self):
        request = HttpClientHandler('/management/invalid', 'GET')
        (response, body) = http_client.request(request)
        (response_secure, body_secure) = https_client.request(request)

        self.assertEqual(response.status, 404)
        self.assertEqual(response_secure.status, 404)

    # Send invalid JSON payload on the discovery request
    def test_discovery_invalid_payload(self):
        url = '/discover'
        method = 'PUT'
        requests = [
            # Wrong devices argument. Must be a string.
            (HttpClientHandler(url, method,
                               pre_handler=lambda: '{"address":[], "timeout":1}'),
             "Wrong devices type"),
            # Wrong timeout argument. Must be an int.
            (HttpClientHandler(url, method,
                               pre_handler=lambda: '{"address":"aa:bb:cc:dd:ee:ff", "timeout":"1"}'),
             "Wrong timeout type"),
            # Wrong timeout argument. Must be positive.
            (HttpClientHandler(url, method,
                               pre_handler=lambda: '{"address":"aa:bb:cc:dd:ee:ff", "timeout":-1}'),
             "Wrong timeout value"),
            # Send just pincode, which is optional, while the other fields are mandatory
            (HttpClientHandler(url, method,
                               pre_handler=lambda: '{"pincode":"1234"}'),
             "Missing JSON arguments on the payload")
        ]

        for request in requests:
            (response, body) = http_client.request(request[0])
            (response_secure, body_secure) = https_client.request(request[0])

            self.assertEqual(response.status, 400, request[1])
            self.assertEqual(response_secure.status, 400, request[1])


if __name__ == '__main__':
    unittest.main()
