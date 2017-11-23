#include "AdCamiBluetooth5.h"

namespace AdCamiHardware {

//map <string, std::thread> AdCamiBluetooth5::_devicesThreads;

AdCamiBluetooth5::AdCamiBluetooth5() :
        _bluetoothAdapterPath(nullptr),
        _discoveryTask(new DiscoveryTask()), _discoveryCallback(nullptr),
        _filterDevices(true), _storeDiscoveredDevices(false), _discoveredDevices(nullptr) {}

AdCamiBluetooth5::~AdCamiBluetooth5() {
    /* Free the Adapter Path */
    if (this->_bluetoothAdapterPath) {
        free(this->_bluetoothAdapterPath);
    }

    delete this->_discoveryTask;
    delete this->_discoveredDevices;
}

AdCamiBluetoothError AdCamiBluetooth5::Init() {
    /* Check if we called Init before */
    if (this->_bluetoothAdapterPath != nullptr) {
        return BT_ERROR_OTHER;
    }

    /* Establish DBus Connection */
    if (DBusConnectionEstablish(G_BUS_TYPE_SYSTEM) != DBUS_OK) {
        return BT_ERROR_DBUS_NO_CONNECTION;
    }

    /* Verify if we have a Bluetooth Adapter */
    if (DBusAdapterGetObjectPath(&this->_bluetoothAdapterPath) != DBUS_OK) {
        return BT_ERROR_ADAPTER_NOT_FOUND;
    }

    return BT_OK;
}

AdCamiBluetoothError AdCamiBluetooth5::DiscoverDevices(vector <AdCamiBluetoothDevice> *devices,
                                                       const unsigned int timeout) {
    GDBusConnection *busConnection = nullptr;
    EnumDBusResult error = DBUS_OK;
    guint idInterfacesAddedSignal = -1, idPropertiesChangedSignal = -1;
    GMainLoop *mainLoop = nullptr;
    map <string, AdCamiBluetoothDevice> discoveredDevices;

    auto clean = [&]() {
        if (mainLoop != nullptr) {
            g_main_loop_unref(mainLoop);
        }
        if (idInterfacesAddedSignal > 0) {
            g_dbus_connection_signal_unsubscribe(busConnection, idInterfacesAddedSignal);
        }
        if (idPropertiesChangedSignal > 0) {
            g_dbus_connection_signal_unsubscribe(busConnection, idPropertiesChangedSignal);
        }
    };

    /* If the discovery is already running, just indicate to store devices and sleep. */
    if (this->_discoveryTask->IsRunning()) {
        this->_storeDiscoveredDevices = true;
        this->_discoveredDevices = devices;
        std::this_thread::sleep_for(std::chrono::seconds(timeout));
        this->_storeDiscoveredDevices = false;
    } else {
        /* Verify Parameter */
        if (devices == nullptr) {
            PRINT_DEBUG("BT_ERROR_BAD_PARAMETERS")
            return BT_ERROR_BAD_PARAMETERS;
        } else {
            devices->clear();
        }

        /* Get Regular DBus Connection to deal with signals */
        if (DBusGetConnection(&busConnection) != DBUS_OK) {
            PRINT_DEBUG("BT_ERROR_DBUS_NO_CONNECTION")
            clean();
            return BT_ERROR_DBUS_NO_CONNECTION;
        }

        /* Get Default Adapter if needed */
        if (this->_bluetoothAdapterPath == nullptr) {
            if (DBusAdapterGetObjectPath(&this->_bluetoothAdapterPath) != DBUS_OK) {
                PRINT_DEBUG("BT_ERROR_ADAPTER_NOT_FOUND")
                return BT_ERROR_ADAPTER_NOT_FOUND;
            }
        }

        /* Subscribe to signals. It is necessary to subscribe to InterfacesAdded, which indicates a new device
         * and to PropertiesChanged that indicates the device is already known, but some property of it changed.
         * This indicates that the device is on. It also allows that between discoveries, the device is always
         * discovered. */
        idInterfacesAddedSignal = g_dbus_connection_signal_subscribe(busConnection,
                                                                     nullptr,
                                                                     DBUS_OBJECTMANAGER_INTERFACE,
                                                                     DBUS_SIGNAL_INTERFACESADDED,
                                                                     nullptr,
                                                                     nullptr,
                                                                     G_DBUS_SIGNAL_FLAGS_NONE,
                                                                     _SyncDiscoveryInterfaceAddedClbk,
                                                                     &discoveredDevices,
                                                                     nullptr);
        idPropertiesChangedSignal = g_dbus_connection_signal_subscribe(busConnection,/* connection */
                                                                       nullptr,/* sender */
                                                                       DBUS_PROPERTIES_INTERFACE,/* iface name */
                                                                       DBUS_SIGNAL_PROPERTIESCHANGED,/* member */
                                                                       nullptr,/* object_path */
                                                                       BLUEZ5_DEVICE_INTERFACE,/* arg0 */
                                                                       G_DBUS_SIGNAL_FLAGS_MATCH_ARG0_PATH,/* flags */
                                                                       _SyncDiscoveryPropertiesChangedClbk,/* callback */
                                                                       &discoveredDevices,/* callback user data */
                                                                       nullptr); /* user data free function */

        /* Start Discovery */
        if ((error = DBusAdapterStartDiscovery(this->_bluetoothAdapterPath)) != DBUS_OK) {
            PRINT_DEBUG("BT_ERROR_START_DISCOVERY [error = " << error << "]")
            clean();
            return BT_ERROR_START_DISCOVERY;
        }

        /* Listen to Signals (Exclusive access to the Bus)*/
        mainLoop = g_main_loop_new(nullptr, FALSE);
        g_timeout_add(timeout * 1000, _StopGMainLoop, mainLoop);
        g_main_loop_run(mainLoop);

        if ((error = DBusAdapterStopDiscovery(this->_bluetoothAdapterPath)) != DBUS_OK) {
            PRINT_DEBUG("Error stopping discovery [error = " << error << "]")
            clean();
            return BT_ERROR_STOP_DISCOVERY;
        }

        for (auto device : discoveredDevices) {
            devices->push_back(std::move(device.second));
        }

        /* Free Memory */
        clean();
    }

    return BT_OK;
}

AdCamiBluetoothError AdCamiBluetooth5::GetPairedDevices(vector <AdCamiBluetoothDevice> *devices) {
//    GPtrArray *device_list = NULL;
//    GHashTable *device_properties = NULL;
//    GHashTable *adapter_properties = NULL;
//    guint index_devices = 0;
//    EnumDBusResult ret_dbus;
//    char *address = NULL;
//    char *name = NULL;
//    int sid = 0;
//    struct sockaddr_l2 addr;
//    char address_modified[BLUETOOTH_SIMPLER_MACSTRING_LENGTH + 1] = {0};
//
//    /* Take possession of DBus */
//    _freeDispatch = false;
//    mutexDBus.lock();
//
//    /* 1) Check Parameters */
//    if (devices == nullptr) {
//        _freeDispatch = true;
//        mutexDBus.unlock();
//        return BT_ERROR_BAD_PARAMETERS;
//    } else
//        devices->clear();
//
//    /* 2) Establish DBus Connection */
//    if (DBusConnectionEstablish(G_BUS_TYPE_SYSTEM) != DBUS_OK) {
//        _freeDispatch = true;
//        mutexDBus.unlock();
//        return BT_ERROR_DBUS_NO_CONNECTION;
//    }
//
//    /* 3) Get Default Adapter if needed */
//    if (this->_bluetoothAdapterPath == nullptr) {
//        if (DBusAdapterGetObjectPath(&this->_bluetoothAdapterPath) != DBUS_OK) {
//            _freeDispatch = true;
//            mutexDBus.unlock();
//            return BT_ERROR_ADAPTER_NOT_FOUND;
//        }
//    }
//
//    /* Get Adapter Properties */
//    if (dbus_adapter_get_properties(this->_bluetoothAdapterPath, &adapter_properties) != DBUS_OK) {
//        _freeDispatch = true;
//        mutexDBus.unlock();
//        return BT_ERROR_DBUS_GENERIC;
//    }
//
//    /* 4) Get List of Devices */
//    if (dbus_adapter_get_device_list(&device_list, this->_bluetoothAdapterPath, adapter_properties) != DBUS_OK) {
//        mutexDBus.unlock();
//        return BT_ERROR_DBUS_GENERIC;
//    }
//
//    /* 5) Check if there are devices in the array */
//    if (device_list->len <= 0) {
//        _freeDispatch = true;
//        mutexDBus.unlock();
//        g_ptr_array_free(device_list, TRUE);
//        return BT_ERROR_NO_PAIRED_DEVICES;
//    }
//
//    /* 6) Analyze each Bluetooth Device in the list */
//    for (; index_devices < device_list->len; index_devices++) {
//        /* 7) Get Device Properties */
//        ret_dbus = dbus_device_get_properties((char *) device_list->pdata[index_devices], &device_properties);
//
//        /* 8) Is the device Paired? */
//        if (ret_dbus == DBUS_OK &&
//            dbus_device_is_paired((char *) device_list->pdata, device_properties) == DBUS_OK) {
//            /* 9) Get Address & Name of Bluetooth Device */
//            dbus_device_get_address(nullptr, device_properties, &address);
//            dbus_device_get_name(nullptr, device_properties, &name);
//
//            /* 10) Try to connect to the Bluetooth Device */
//            sid = socket(PF_BLUETOOTH, SOCK_RAW, BTPROTO_L2CAP);
//
//            memset(&addr, 0, sizeof(addr));
//            addr.l2_family = PF_BLUETOOTH;
//            str2ba(address, &addr.l2_bdaddr);
//
//            if (connect(sid, (struct sockaddr *) &addr, sizeof(addr)) == 0) {
//                /* If we could connect, the device is around */
//                PRINT_DEBUG("[GetDevices] Connected to " << address << " (" << name << ")!");
//
//                /* Close Bluetooth Socket */
//                close(sid);
//
//                /* 11) Add to Vector */
//                if (_ParseBluetoothAddress(address, address_modified)) {
//                    AdCamiBluetoothDevice device(address_modified);
//
//                    device.SetName(name);
//                    devices->push_back(device);
//                }
//            } else {
//                PRINT_DEBUG("[GetDevices] Could not connect to " << address << " (" << name << ")!");
//            }
//
//            /* Free Device Properties */
//            g_hash_table_destroy(device_properties);
//
//            /* 12) Free Strings */
//            free(address);
//            free(name);
//        }
//    }
//
//    /* 13) Free Memory */
//    g_hash_table_destroy(adapter_properties);
//
//    _freeDispatch = true;
//    mutexDBus.unlock();

    return BT_OK;
}

AdCamiBluetoothError AdCamiBluetooth5::PairDevice(const string &deviceAddress) {
    GDBusConnection *busConnection = nullptr;
    GError *error = nullptr;
//	DBusError error_dbus;
//	bt_err_t error_bt;
//  const char* strDeviceAddress = deviceAddress.c_str();
//	int signal = 0;
    char *adapterPath, *devicePath;
    GMainLoop *main_loop = g_main_loop_new(nullptr, FALSE);

    char *agentIntrospectionXml = nullptr;
    ReadAgentIntrospectionXmlData(&agentIntrospectionXml);
    GDBusNodeInfo *agentIntrospectionData = g_dbus_node_info_new_for_xml(agentIntrospectionXml, NULL);

    static const GDBusInterfaceVTable interface_vtable = {
            PairAgentMethodCall,
            nullptr,
            nullptr
    };

//	dbus_error_init(&error_dbus);

    /* Message Handler */
//	DBusObjectPathVTable agent_table;
//	agent_table.message_function = agent_message;

    /* 1) Check Parameters */
    if (deviceAddress.empty()) {
        return BT_ERROR_BAD_PARAMETERS;
    }

    /* Take possession of DBus */
//	_freeDispatch = false;
//	mutexDBus.lock();

    /* 2) Check if device is already paired. */
//	error_bt = bluetooth_is_device_paired(device_address, _bluetoothAdapterPath);
//	switch (error_bt) {
//		case BTP_OK: {
////			_freeDispatch = true;
////			mutexDBus.unlock();
//			return BT_ERROR_ALREADY_PAIRED;
//		}
//		case BTP_ERROR_DEVICE_NOT_FOUND:
//		case BTP_ERROR_NOT_PAIRED: {
//			break;
//		}
//		/* Any other error is a problem */
//		default: {
////			_freeDispatch = true;
////			mutexDBus.unlock();
//			return BT_ERROR_OTHER;
//		}
//	}

    /* 3) If not Paired, get regular DBus Connection */
    if (DBusGetConnection(&busConnection) != DBUS_OK) {
//		_freeDispatch = true;
//		mutexDBus.unlock();
//		dbus_error_free(&error_dbus);
        return BT_ERROR_DBUS_NO_CONNECTION;
    }

    /* Get the Bluetooth adapter path. */
    if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
        return BT_ERROR_ADAPTER_NOT_FOUND;
    }
    /* Get device path based on the Bluetooth address. */
    if (DBusDeviceGetObjectPath(deviceAddress.c_str(), adapterPath, &devicePath) != DBUS_OK) {
        return BT_ERROR_DEVICE_NOT_FOUND;
    }

    /* Register agent manager. */
    if (DBusAgentManagerRegister(kAgentPairPath, BLUEZ5_AGENTMANAGER_CAPABILITIES_NOIO) != DBUS_OK) {
        return BT_ERROR_ADAPTER_NOT_FOUND;//TODO change the return
    }

    /* 4) Register Agent Object Path */
    g_dbus_connection_register_object(busConnection,//connection
                                      kAgentPairPath,//object_path
                                      agentIntrospectionData->interfaces[0],//interfaceInfo
                                      &interface_vtable,//vtable
                                      nullptr,//user_data
                                      nullptr,//free_func
                                      &error);
//	if (!dbus_connection_try_register_object_path(regular_connection, BLUETOOTH_DBUS_OBJPATH_AGENT, &agent_table, nullptr, &error_dbus)) {
//		if (!dbus_error_has_name(&error_dbus, DBUS_ERROR_OBJECT_PATH_IN_USE)) {
//			_freeDispatch = true;
//			mutexDBus.unlock();
//			dbus_error_free(&error_dbus);
//			return BT_ERROR_DBUS_GENERIC;
//		}
//	}

    /* 5) Pair the Device */
//	if (dbus_adapter_create_paired_device(deviceAddress.c_str(), BLUETOOTH_DBUS_OBJPATH_AGENT, BLUETOOTH_AGENT_CAPABILITIES_NOIO, nullptr) != DBUS_OK) {
//		_freeDispatch = true;
//		mutexDBus.unlock();
//		dbus_error_free(&error_dbus);
//		return BT_ERROR_NOT_PAIRED;
//	}
    DBusDevicePair(devicePath);

    g_timeout_add(10000, _StopGMainLoop, main_loop);
    g_main_loop_run(main_loop);

//	/* Free DBus */
//	_freeDispatch = true;
//	mutexDBus.unlock();
//
//	/* 6) Add Device to the List of Paired Devices */
//	this->_PairedDevices.push_back(btAddress);
//
//	/* 7) Signal Daemon that a new Bluetooth Device was paired. */
//	signal = DAEMON_SIG_PAIRED_DEVICE;
//	write(_Pipe[1], &signal, sizeof(signal));

    /* Free Memory */
//	dbus_error_free(&error_dbus);

    return BT_OK;
}

AdCamiBluetoothError AdCamiBluetooth5::StartDiscovery() {
//    char *adapterPath = nullptr;
    GDBusConnection *busConnection = nullptr;
    EnumDBusResult error = DBUS_OK;
    map <string, AdCamiBluetoothDevice> discoveredDevices;

    auto clean = [&]() {
//        delete[] adapterPath;
    };

    if (this->_discoveryTask->IsRunning()) {
        return BT_ERROR_DISCOVERY_RUNNING;
    }

    /* Get Regular DBus Connection to deal with signals */
    if (DBusGetConnection(&busConnection) != DBUS_OK) {
        PRINT_DEBUG("BT_ERROR_DBUS_NO_CONNECTION")
        clean();
        return BT_ERROR_DBUS_NO_CONNECTION;
    }
    this->_discoveryTask->BusConnection = busConnection;

    /* Get Default Adapter if needed */
    if (this->_bluetoothAdapterPath == nullptr) {
        //if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
        if (DBusAdapterGetObjectPath(&this->_bluetoothAdapterPath) != DBUS_OK) {
            PRINT_DEBUG("BT_ERROR_ADAPTER_NOT_FOUND")
            return BT_ERROR_ADAPTER_NOT_FOUND;
        }
    }
//    } else {
//        adapterPath = strndup(this->_bluetoothAdapterPath, strlen(this->_bluetoothAdapterPath));
//    }

    /* Subscribe to signals. It is necessary to subscribe to InterfacesAdded, which indicates a new device
     * and to PropertiesChanged that indicates the device is already known, but some property of it changed.
     * This indicates that the device is on. It also allows that between discoveries, the device is always
     * discovered. */
//    idInterfacesAddedSignal = g_dbus_connection_signal_subscribe(busConnection,
    this->_discoveryTask->InterfacesAddedCallbackId =
            g_dbus_connection_signal_subscribe(busConnection,
                                               nullptr,
                                               DBUS_OBJECTMANAGER_INTERFACE,
                                               DBUS_SIGNAL_INTERFACESADDED,
                                               nullptr,
                                               nullptr,
                                               G_DBUS_SIGNAL_FLAGS_NONE,
                                               _AsyncDiscoveryInterfaceAddedClbk,
                                               this,
                                               nullptr);
//    idPropertiesChangedSignal = g_dbus_connection_signal_subscribe(busConnection,/* connection */
    this->_discoveryTask->PropertiesChangedCallbackId =
            g_dbus_connection_signal_subscribe(busConnection,/* connection */
                                               nullptr,/* sender */
                                               DBUS_PROPERTIES_INTERFACE,/* iface name */
                                               DBUS_SIGNAL_PROPERTIESCHANGED,/* member */
                                               nullptr,/* object_path */
                                               BLUEZ5_DEVICE_INTERFACE,/* arg0 */
                                               G_DBUS_SIGNAL_FLAGS_MATCH_ARG0_PATH,/* flags */
                                               _AsyncDiscoveryPropertiesChangedClbk,/* callback */
                                               this,/* callback user data */
                                               nullptr); /* user data free function */

    /* Start Discovery */
//    if ((error = DBusAdapterStartDiscovery(adapterPath)) != DBUS_OK) {
    if ((error = DBusAdapterStartDiscovery(this->_bluetoothAdapterPath)) != DBUS_OK) {
        PRINT_DEBUG("BT_ERROR_START_DISCOVERY [error = " << error << "]")
        clean();
        return BT_ERROR_START_DISCOVERY;
    }

    /* Listen to Signals (Exclusive access to the Bus) */
    this->_discoveryTask->Loop = g_main_loop_new(nullptr, FALSE);

    this->_discoveryTask->Thread = std::thread([&]() {
        g_main_loop_run(this->_discoveryTask->Loop);
    });
    this->_discoveryTask->Thread.detach();

    return BT_OK;
}

AdCamiBluetoothError AdCamiBluetooth5::StopDiscovery() {
//    char *adapterPath = nullptr;
    GDBusConnection *busConnection = nullptr;
    EnumDBusResult error = DBUS_OK;

    auto clean = [&]() {
//        delete[] adapterPath;
    };

    /* Get Regular DBus Connection to deal with signals */
    if (DBusGetConnection(&busConnection) != DBUS_OK) {
        PRINT_DEBUG("BT_ERROR_DBUS_NO_CONNECTION")
        clean();
        return BT_ERROR_DBUS_NO_CONNECTION;
    }

    /* Get Default Adapter if needed */
    if (this->_bluetoothAdapterPath == nullptr) {
        //if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
        if (DBusAdapterGetObjectPath(&this->_bluetoothAdapterPath) != DBUS_OK) {
            PRINT_DEBUG("BT_ERROR_ADAPTER_NOT_FOUND")
            return BT_ERROR_ADAPTER_NOT_FOUND;
        }
    }

    /* Stop Discovery */
//    if ((error = DBusAdapterStopDiscovery(adapterPath)) != DBUS_OK) {
    if ((error = DBusAdapterStopDiscovery(this->_bluetoothAdapterPath)) != DBUS_OK) {
        PRINT_LOG("Error stopping discovery [error = " << error << "]")
        clean();
        return BT_ERROR_STOP_DISCOVERY;
    }
    if (this->_discoveryTask->IsRunning()) {
        g_main_loop_quit(this->_discoveryTask->Loop);
    }
    this->_discoveryTask->Clean();

    return BT_OK;
}

string AdCamiBluetooth5::_BluetoothAddressFromObjectPath(const char *objectPath) {
    const size_t kAddressLength = 17;
    string address(objectPath + (strlen(objectPath) - kAddressLength), kAddressLength);

    std::replace(address.begin(), address.end(), '_', ':');

    return address;
}

void AdCamiBluetooth5::_AsyncDiscoveryInterfaceAddedClbk(GDBusConnection *connection,
                                                         const gchar *sender_name,
                                                         const gchar *object_path,
                                                         const gchar *interface_name,
                                                         const gchar *signal_name,
                                                         GVariant *parameters,
                                                         gpointer user_data) {
    AdCamiBluetooth5 *bluetooth = static_cast<AdCamiBluetooth5 *>(user_data);
    BluetoothDeviceProperties deviceInfo;

    /* If no information for the device could be found, then stop. Most probably the interrupt was generated by an
     * interface other than org.bluez.Device1. */
    if (DBusLookupDeviceProperties(parameters, &deviceInfo) != DBUS_OK) {
        return;
    }

    std::unique_ptr <AdCamiBluetoothDevice> device(new AdCamiBluetoothDevice(deviceInfo.Address));
    device.get()->Name(deviceInfo.Name).Uuids(deviceInfo.UUIDs);

    /* If the Discovery() method was invoked, then store all found devices on list. */
    if (bluetooth->_storeDiscoveredDevices && std::find(bluetooth->_discoveredDevices->begin(),
                                                        bluetooth->_discoveredDevices->end(),
                                                        *device.get()) == bluetooth->_discoveredDevices->end()) {
        bluetooth->_discoveredDevices->push_back(*device.get());
    }

    /* Check if the device is authorized to receive notifications. */
    bool deviceAuthorized = (false || !bluetooth->_filterDevices);
    if (bluetooth->_filterDevices && bluetooth->_updateDevicesFilterClbk != nullptr) {
        bluetooth->_updateDevicesFilterClbk(&bluetooth->_devicesFilter);
        deviceAuthorized = (std::find(bluetooth->_devicesFilter.begin(),
                                      bluetooth->_devicesFilter.end(),
                                      *device.get()) != bluetooth->_devicesFilter.end());
    }

    if (bluetooth->_discoveryCallback != nullptr && deviceAuthorized) {
        bluetooth->_discoveryCallback(std::move(device));
    }
}

void AdCamiBluetooth5::_AsyncDiscoveryPropertiesChangedClbk(GDBusConnection *connection,
                                                            const gchar *sender_name,
                                                            const gchar *object_path,
                                                            const gchar *interface_name,
                                                            const gchar *signal_name,
                                                            GVariant *parameters,
                                                            gpointer user_data) {
//    PRINT_DEBUG("parameters = " << g_variant_print(parameters, TRUE));
    AdCamiBluetooth5 *bluetooth = static_cast<AdCamiBluetooth5 *>(user_data);
    string address = _BluetoothAddressFromObjectPath(object_path);

    std::unique_ptr <AdCamiBluetoothDevice> device(new AdCamiBluetoothDevice(address));
    device.get()->NameFromCache();
    device.get()->UuidsFromCache();

    /* If the Discovery() method was invoked, then store all found devices on list. */
    if (bluetooth->_storeDiscoveredDevices && std::find(bluetooth->_discoveredDevices->begin(),
                                                        bluetooth->_discoveredDevices->end(),
                                                        *device.get()) == bluetooth->_discoveredDevices->end()) {
        bluetooth->_discoveredDevices->push_back(*device.get());
    }

    /* Check if the device is authorized to receive notifications. */
    bool deviceAuthorized = (false || !bluetooth->_filterDevices);
    if (bluetooth->_filterDevices && bluetooth->_updateDevicesFilterClbk != nullptr) {
        bluetooth->_updateDevicesFilterClbk(&bluetooth->_devicesFilter);
        deviceAuthorized = (std::find(bluetooth->_devicesFilter.begin(),
                                      bluetooth->_devicesFilter.end(),
                                      *device.get()) != bluetooth->_devicesFilter.end());
    }

    if (bluetooth->_discoveryCallback != nullptr && deviceAuthorized) {
        bluetooth->_discoveryCallback(std::move(device));
    }
}

void AdCamiBluetooth5::_SyncDiscoveryInterfaceAddedClbk(GDBusConnection *connection,
                                                        const gchar *sender_name,
                                                        const gchar *object_path,
                                                        const gchar *interface_name,
                                                        const gchar *signal_name,
                                                        GVariant *parameters,
                                                        gpointer user_data) {
    auto *devicesList = static_cast<map <string, AdCamiBluetoothDevice> *>(user_data);
    BluetoothDeviceProperties deviceInfo;

    DBusLookupDeviceProperties(parameters, &deviceInfo);

    /* Check if the device was already added. If it wasn't, then add it to the list
     * of discovered devices. That might occur because other event could add it. */
    if (devicesList->find(deviceInfo.ObjectPath) == devicesList->end()) {
        AdCamiBluetoothDevice device(deviceInfo.Address);
        device.Name(deviceInfo.Name)
                .Uuids(deviceInfo.UUIDs);
//        devicesList->insert(std::pair<string, AdCamiBluetoothDevice>(string(deviceInfo.ObjectPath), device));
        devicesList->insert({string(deviceInfo.ObjectPath), std::move(device)});
    }
}

void AdCamiBluetooth5::_SyncDiscoveryPropertiesChangedClbk(GDBusConnection *connection,
                                                           const gchar *sender_name,
                                                           const gchar *object_path,
                                                           const gchar *interface_name,
                                                           const gchar *signal_name,
                                                           GVariant *parameters,
                                                           gpointer user_data) {
    auto *devicesList = static_cast<map <string, AdCamiBluetoothDevice> *>(user_data);
    string address = _BluetoothAddressFromObjectPath(object_path);
    gchar *retStr = nullptr;

    PRINT_DEBUG("parameters = " << (retStr = g_variant_print(parameters, TRUE)));
    g_free(retStr);

    /* Check if the device was already added. If it wasn't, then add it to the list
     * of discovered devices. That might occur because other event could add it. */
    if (devicesList->find(object_path) == devicesList->end()) {
        AdCamiBluetoothDevice device(address);
        device.NameFromCache();
        device.UuidsFromCache();
//        devicesList->insert(std::pair<string, AdCamiBluetoothDevice>(string(object_path), device));
        devicesList->insert({string(object_path), std::move(device)});
    }
}

gboolean AdCamiBluetooth5::_StopGMainLoop(gpointer data) {
    g_main_loop_quit(static_cast<GMainLoop *>(data));
    return FALSE;
}

} //namespace
