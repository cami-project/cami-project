//
//  IAdCamiBluetooth.h
//
//  Created by Jorge Miguel Miranda on 07/12/2016.
//
//

#ifndef AdCamiDaemon_IAdCamiBluetooth_h
#define AdCamiDaemon_IAdCamiBluetooth_h

#include <string>
#include <vector>
#include "AdCamiBluetoothDefinitions.h"
#include "AdCamiBluetoothDevice.h"

using std::string;
using std::vector;

namespace AdCamiHardware {

/**
 * Class with Bluetooth actions that can be performed. This acts like an interface
 * and must be implemented by other classes that interact with Bluetooth devices.
 */
class IAdCamiBluetooth {
public:
    /**
     * Virtual destructor. This needs to be implemented so that classes that
     * implement the interface have their destructor called when it is invoked
     * via a IAdCamiBluetooth inherited instance.
     */
    virtual ~IAdCamiBluetooth() {}

    /**
     * Initializes any resources needed by the Bluetooth adapter. If no resources
     * need to be initialized, this method must be implemented empty.
     * @return BT_OK in case of success, other value in case of error
     */
    virtual AdCamiBluetoothError Init() = 0;

    /**
     * Scans for Bluetooth devices in range.
     * @param devices a std::vector where the discovered devices will be returned
     * @param timeout a timeout value for the discovery function. A value of -1 makes the function
     *  scans indefinitely.
     * @return BT_OK in case of success, other value if an error occurred.
     */
    virtual AdCamiBluetoothError DiscoverDevices(vector<AdCamiBluetoothDevice> *devices,
                                                 const unsigned int timeout = 10) = 0;

    /**
     * Gets all the paired Bluetooth devices that are in range.
     * @return a std::vector with all the available Bluetooth devices
     */
    virtual AdCamiBluetoothError GetPairedDevices(vector<AdCamiBluetoothDevice> *devices) = 0;

    /**
     * Gets a mesure from a paired Bluetooth device.
     * @param macAddress MAC address of the Bluetooth device from where the measure
     *	will be get
     * @param measure pointer to a AdCamiDeviceMeasure object where the measurement
     *	will be stored
     * @return BT_OK in case of success, other value if an error occurred.
     */
//    virtual AdCamiBluetoothError ReadDevice(const string &macAddress, vector <string> *measure) = 0;

    /**
     * Pair this system with a Bluetooth device.
     * @param macAddress Address of the Bluetooth device to pair with.
     * @return BT_OK in case of success, other value if an error occurred.
     */
    virtual AdCamiBluetoothError PairDevice(const string &macAddress) = 0;
};

} //namespace
#endif
