#AdCAMI
A Bluetooth Low Energy (BLE) gateway for [A&D medical devices](http://www.andonline.com/medical/). At the moment only,
the white versions of the weight scale A&D UC-352BLE and the blood pressure monitor A&D UA-651BLE are supported.

**DISCLAMER:** the gateway is tested to work properly with out of the box vanilla devices. Pairing with already
used/paired devices will lead to unknown results.

##Development

### Requirements
To build the project the following dependencies need to be installed (refer to the next section for automatic
installation of these):

* [libmicrohttpd](https://www.gnu.org/software/libmicrohttpd/) (>= 0.9.44)
* [libcurl](https://curl.haxx.se/libcurl/) (>= 7.47)
* [libjson](https://libjson.sourceforge.io) (>= 7.6.1)
* [BlueZ](https://git.kernel.org/cgit/bluetooth/bluez.git/) (>= 5.43)
* [SQLite](https://www.sqlite.org)
* [Google Test](https://github.com/google/googletest) (*optional*)
* [Python](https://www.python.org) (>= 3.5, *optional*)

### Project's folder structure
 * **build** contains the executable binaries and CMake search files;
 * **contrib** contains non-essential files for the project, such as downloaded files of libraries to be installed. This
    folder is created by the executing either the script `prepare_project.sh` or `prepare_deployment.sh`;
 * **include** contains all header files (.h);
 * **scripts** contains configuration files for systemd, namely **adcamid** and **adcamid.service**, and other script
    files used to configure and/or create necessary files to execute the daemon; 
 * **src** contains all source code files (.cpp);
 * **test** contains test source code files.

### Prepare development environment
To prepare the development environment execute the script `prepare_project.sh` on the root folder of the project. This
script does the following:

1. Installs all the dependencies necessary to build the project (these are listed on section Requirements);

  * the dependencies **libmicrohttpd** and **libcurl** are installed from the operating system repository (for example,
    using apt-get);
  * **libjson**, **BlueZ** and **Google Test** are downloaded and built on the host machine;
  * **SQLite** is already embedded on the project, using the amalgamation version.
  
2. Creates folders necessary for development, namely:

  * **build/Debug** where the debug (i.e. **with** debug symbols) version of the daemon is placed;
  * **build/Debug/etc_adcamid_test** is a test copy of the *etc* folder with necessary files to run the daemon;
  * **build/Release** where the release (i.e. **without** debug symbols) version of the daemon is placed;
  
3. Creates test files on **build/Debug/etc_adcamid_test**, namely:
 
  * **adcamid.conf**, the configuration file of the daemon;
  * **events.db**, the database where devices and events are registered;
  * **server.key**, key file used by the secure HTTP server;
  * **server.pem**, PEM file used by the secure HTTP server.
 
If the files or folders already exists, running the script again will not override them. 

### Build
Both debug and release versions of the daemon executable can be built. On the respective directory, inside **build**
directory, execute `cmake -DCMAKE_BUILD_TYPE=Debug ../..` for debug, or `cmake -DCMAKE_BUILD_TYPE=Release ../..` for 
release. For building the executable, execute `make adcamid`.

### Install
To install the daemon, execute, as root or with `sudo`, the script `prepare_deployment.sh` on the root folder of the
project. The script installs and configures the following files:

 * the *release* version of executable `adcamid` (this must be previously compiled);
 * the init.d file *adcamid*, present on the folder *scripts*;
 * the systemd file *adcamid.service*, present on the folder *scripts*.

## Usage

### Start the daemon
Once adcamid is installed, it can be started via systemd interface. To start the daemons execute, as root or with root
privileges, `service adcamid start`; to stop execute `service adcamid stop`. The daemon is also configured to start when
the system boots.

It is provided a REST API for interacting with the daemon. A sample Python application to interact with the gateway is
provided on `test/TestAdCamiInteractive.py`. The following options are presented:

 1. **Discover and pair Bluetooth devices** makes a discovery of nearby Bluetooth devices (both BR/EDR and BLE) and
    allows to pair with a specific device. If no device's address is specified, only the discovery is performed; 
 2. **Get events** retrieves all events that the gateway registered. These events can be: device, when one is "seen"
    by the gateway; or measurement, when biometric measurements are collected from a device; 
 3. **Get paired devices** retrieves all Bluetooth devices paired with the gateway;
 4. **Read from device** reads measurement notifications from a Bluetooth device;
 5. **Add devices** registers a Bluetooth device on the gateway. This will not be enabled to receive measurement
    notifications from a device and will not be paired;
 6. **Delete devices** deletes a device from the gateway. All events that occurred will still be kept on the gateway;
 7. **Enable notifications** enables receiving measurement notification from a Bluetooth device;
 8. **Disable notifications** disables receiving measurement notification from a Bluetooth device;
 9. **Set credentials** configures the credentials to access a remote web service;
 10. **Set gateway name** configures the name of the gateway;
 11. **Set remote endpoint** configures the endpoint of a remote web service, where measurements will be sent. The
    endpoint must be a URL and if a port different than 80 is used, it must be indicated - for example,
    *http://remote-endpoint.com:8081/event*;
 12. **Exit** terminates the application.
 
Besides sending requests, the application is also able to receive measurement notifications from the daemon. To do so, 
configure the gateway remote endpoint using option 11.


