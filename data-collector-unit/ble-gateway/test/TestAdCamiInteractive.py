import json
import sys
import threading
from support.HttpClient import HttpClient, HttpClientHandler
from support.HttpServer import HttpServer

SERVER_PORTS = [61773, 61774]
ADCAMID_ADDRESS = "localhost:60773"


# #########################################
# Client requests
# #########################################
def add_devices():
    addresses = input("Insert addresses (separated by comma): ")
    addresses = [addr.strip() for addr in addresses.split(',')]
    json_str = json.dumps({"devices": addresses})
    print("json_str = " + json_str)
    return json_str


def delete_devices():
    addresses = input("Insert addresses (separated by comma): ")
    addresses = [addr.strip() for addr in addresses.split(',')]
    json_str = json.dumps({"devices": addresses})
    print("json_str = " + json_str)
    return json_str


def disable_notifications():
    addresses = input("Insert addresses (separated by comma): ")
    addresses = [addr.strip() for addr in addresses.split(',')]
    json_str = json.dumps({"devices": addresses})
    print("json_str = " + json_str)
    return json_str


def discover_devices():
    # TODO check if the address format is correct
    address = input("Address (empty for all): ")
    timeout = input("Discovery timeout (seconds): ")
    return '{"address":"' + address + '", "timeout":' + timeout + '}'


def enable_notifications():
    addresses = input("Insert addresses (separated by comma): ")
    addresses = [addr.strip() for addr in addresses.split(',')]
    json_str = json.dumps({"devices": addresses})
    return json_str


def get_trusted_devices(data):
    print("trusted devices = " + data)
    return


def list_events(data):
    print(data)
    return


def list_devices(data):
    print(data)
    return


def print_measurement(data):
    print(data)
    return


def read_device():
    # TODO check if the address format is correct
    address = input("Address: ")
    timeout = input("Discovery timeout (seconds): ")
    return '{"address":"' + address + '", "timeout":' + timeout + '}'


def set_credentials():
    username = input("Username: ")
    password = input("Password: ")
    return '{"username":"' + username + '", "password":"' + password + '"}'


def set_gateway_name():
    name = str(input("Gateway name: "))
    return '{"name":"' + name + '"}'


def set_remote_endpoint():
    endpoints = input("Remote endpoint(s) (separated by comma): ")
    endpoints = [endp.strip() for endp in endpoints.split(',')]
    return json.dumps({"endpoint": endpoints})


# #########################################
# Server requests
# #########################################
def request_events(post_data):
    print("request_events post_data = " + post_data)
    return


def request_new_device(post_data):
    print("request_events post_data = " + post_data)
    return


def print_menu():
    print()
    for key, value in sorted(requests.items()):
        print("{}. {}".format(key, value[0]))
    print("0. Exit")
    print("Option: ", end="")
    return


# def server_signal_handler(num, stack):
#     print_menu()
#     return


# Start HTTP server on a separate thread
http_servers = {}
for port in SERVER_PORTS:
    http_server = HttpServer(port)
    http_server.add_request_handler("/events", "POST", request_events)
    http_server.add_request_handler("/device/new", "POST", request_new_device)
    http_server.execute_after_request = print_menu
    http_server_thread = threading.Thread(target=http_server.run)
    http_server_thread.start()
    http_servers[port] = http_server
    print("Listening on port {}".format(port))

# Use another address for the gateway, if specified as application argument
gateway_address = (sys.argv[1] if len(sys.argv) > 1 else ADCAMID_ADDRESS)
http_client = HttpClient(gateway_address)

# Initialize client requests.
requests = {
    1: ("Discover and pair Bluetooth devices", HttpClientHandler("/discover", "PUT",
                                                                 pre_handler=discover_devices,
                                                                 post_handler=list_devices)),
    2: ("Get events", HttpClientHandler("/events", "GET",
                                        post_handler=list_events)),
    3: ("Get paired devices", HttpClientHandler("/device/list", "GET",
                                                post_handler=get_trusted_devices)),
    4: ("Read from device", HttpClientHandler("/device/read", "PUT",
                                              pre_handler=read_device,
                                              post_handler=print_measurement)),
    5: ("Add devices", HttpClientHandler("/device", "POST",
                                         pre_handler=add_devices)),
    6: ("Delete devices", HttpClientHandler("/device", "DELETE",
                                            pre_handler=delete_devices)),
    7: ("Enable notifications", HttpClientHandler("/device/enable", "PUT",
                                                  pre_handler=enable_notifications)),
    8: ("Disable notifications", HttpClientHandler("/device/disable", "PUT",
                                                   pre_handler=disable_notifications)),
    9: ("Set credentials", HttpClientHandler("/management/credentials", "PUT", set_credentials)),
    10: ("Set gateway name", HttpClientHandler("/management/gateway", "PUT", set_gateway_name)),
    11: ("Set remote endpoint", HttpClientHandler("/management/endpoint", "PUT", set_remote_endpoint)),

}

print("Gateway's address is " + gateway_address)
while True:
    print_menu()
    try:
        option = int(input())
        if option != 0 and option <= len(requests):
            handler = requests[option]
            http_client.request(handler[1])
        elif option > len(requests):
            print('\nInvalid option "{}"'.format(option))
        else:
            for port, http_server in http_servers.items():
                http_server.shutdown()
            exit()
    except ValueError:
        print("\nInvalid insertion.")
    except KeyboardInterrupt:
        print("Termination forced by the user!")
        for port, http_server in http_servers.items():
            http_server.shutdown()
        exit()
