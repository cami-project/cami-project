//
//  Created by Jorge Miranda on 07/12/2016
//

#ifndef AdCamiDaemon_AdCamiBluetooth5_h
#define AdCamiDaemon_AdCamiBluetooth5_h

#include <algorithm>
#include <chrono>
#include <cstdio>
#include <cstdlib>
#include <functional>
#include <map>
#include <memory>
#include <thread>
/* DBus helper library */
#include "dbus/dbus_helper_5.h"
/* Bluetooth Interface */
#include "AdCamiUtilities.h"
#include "IAdCamiBluetooth.h"

using std::map;
using std::string;
using std::vector;

namespace AdCamiHardware {

class AdCamiBluetooth5 : public IAdCamiBluetooth {
public:
    using DiscoveryClbk = std::function<void(std::unique_ptr < AdCamiBluetoothDevice > )>;
    using UpdateDevicesFilterClbk = std::function<void(vector < AdCamiBluetoothDevice > *)>;

    /**
     * Default constructor
     */
    AdCamiBluetooth5();

    /**
     * Destructor
     */
    ~AdCamiBluetooth5();

    /**
     * Initializes the resources needed to interact with Bluetooth modules (i.e DBus Connection, Bluetooth Dongle/Adapter).
     * @return 0 in case of success, negative value if an error occurred.
     */
    AdCamiBluetoothError Init();

    /**
     * This method allows the system to discover Bluetooth devices that are near.
     * @param devices Vector that will be filled with information from the found Bluetooth devices.
     * @return 0 in case of success, negative value if an error occurred.
     */
    AdCamiBluetoothError DiscoverDevices(vector <AdCamiBluetoothDevice> *devices,
                                         const unsigned int timeout = 10);

    inline void FilterDevices(bool filter) {
        this->_filterDevices = filter;
    }

    /**
     * This method allows to get all devices that are paired.
     * @param devices vector with the information from the found Bluetooth devices.
     * @return 0 in case of success, negative value if an error occurred.
     */
    AdCamiBluetoothError GetPairedDevices(vector <AdCamiBluetoothDevice> *devices);

    /**
     * This method allows us to pair the system with the Bluetooth device indicated by the address.
     * It is important to note that the AdCami system has no Input/Output capabilities.
     * @param macAddress Address of the Bluetooth device we want to pair with.
     * @return 0 in case of success, negative value if an error occurred.
     */
    AdCamiBluetoothError PairDevice(const string &btAddress);

    /**
     * This method is used when another class wants to know what devices were Paired. 
     * Usually, this method is invoked after a signal of PairedDevice is sent to listeners.
     * @param fullList Set as true to extract the full List of unreported Paired Devices.
     *  False to extract only the last unreported Paired Device Event.
     * @param deviceList List we want to fill with information about unreported Paired Device
     *  Events. Can be set to NULL if fullList flag is set to false.
     * @param device String to fill with the Address of the last unreported Paired Device.
     * @return 0 in case of success, negative value if an error occurred.
     */
//    AdCamiBluetoothError GetPairedDevice(const bool fullList, vector <string> *deviceList, string *device);

    void SetDiscoveryCallback(DiscoveryClbk clbk) {
        this->_discoveryCallback = clbk;
    }

    void SetUpdateDevicesFilterCallback(UpdateDevicesFilterClbk clbk) {
        _updateDevicesFilterClbk = clbk;
    }

    AdCamiBluetoothError StartDiscovery();

    AdCamiBluetoothError StopDiscovery();

private:
    struct DiscoveryTask {
        GDBusConnection *BusConnection;
        guint PropertiesChangedCallbackId;
        guint InterfacesAddedCallbackId;
        GMainLoop *Loop;
        std::thread Thread;

        DiscoveryTask() : BusConnection(nullptr),
                          PropertiesChangedCallbackId(0),
                          InterfacesAddedCallbackId(0),
                          Loop(nullptr) {}

        ~DiscoveryTask() {
            this->Clean();
        }

        bool IsRunning() const {
            return (this->Loop != nullptr && g_main_loop_is_running(this->Loop));
        }

        void Clean() {
            if (this->Loop != nullptr) {
                g_main_loop_unref(this->Loop);
            }
            if (this->BusConnection != nullptr && this->InterfacesAddedCallbackId > 0) {
                g_dbus_connection_signal_unsubscribe(this->BusConnection,
                                                     this->InterfacesAddedCallbackId);
            }
            if (this->BusConnection != nullptr && this->PropertiesChangedCallbackId > 0) {
                g_dbus_connection_signal_unsubscribe(this->BusConnection,
                                                     this->PropertiesChangedCallbackId);
            }
        }
    };

//    static map<string, std::thread> _devicesThreads;
    /* DBus object path for the Bluetooth adapter */
    char *_bluetoothAdapterPath;
    /* Structure with variables used by the background discovery thread. */
    DiscoveryTask *_discoveryTask;
    /* Discovery callback invoked when a new device is discovered by the background discovery thread. */
    DiscoveryClbk _discoveryCallback;
    /* Flag to filter discovered devices. */
    bool _filterDevices;
    /* List with the devices that are trusted to receive notifications. */
    vector <AdCamiBluetoothDevice> _devicesFilter;
    /* Callback that is invoked to update the list of devices that are trusted. */
    UpdateDevicesFilterClbk _updateDevicesFilterClbk;
    /* Flag that indicates if the discovered devices must be stored on a list. This list is used by
     * the Discovery() function. */
    bool _storeDiscoveredDevices;
    /* List of discovered devices by the Discovery() fucntion. */
    vector<AdCamiBluetoothDevice> *_discoveredDevices;

    /**
     * Transform an object path on the format .../dev_xx_xx_xx_xx_xx_xx to a "normalized"
     * Bluetooth address.
     * @param objectPath a DBus object path string
     * @return a string with the address in standard form
     */
    static string _BluetoothAddressFromObjectPath(const char *objectPath);

    static void _AsyncDiscoveryInterfaceAddedClbk(GDBusConnection *connection,
                                                  const gchar *sender_name,
                                                  const gchar *object_path,
                                                  const gchar *interface_name,
                                                  const gchar *signal_name,
                                                  GVariant *parameters,
                                                  gpointer user_data);

    static void _AsyncDiscoveryPropertiesChangedClbk(GDBusConnection *connection,
                                                     const gchar *sender_name,
                                                     const gchar *object_path,
                                                     const gchar *interface_name,
                                                     const gchar *signal_name,
                                                     GVariant *parameters,
                                                     gpointer user_data);


    static void _SyncDiscoveryInterfaceAddedClbk(GDBusConnection *connection,
                                                 const gchar *sender_name,
                                                 const gchar *object_path,
                                                 const gchar *interface_name,
                                                 const gchar *signal_name,
                                                 GVariant *parameters,
                                                 gpointer user_data);

    static void _SyncDiscoveryPropertiesChangedClbk(GDBusConnection *connection,
                                                    const gchar *sender_name,
                                                    const gchar *object_path,
                                                    const gchar *interface_name,
                                                    const gchar *signal_name,
                                                    GVariant *parameters,
                                                    gpointer user_data);

    static gboolean _StopGMainLoop(gpointer data);
};

}

#endif	/* AdCamiDaemon_AdCamiBluetooth5_h */

