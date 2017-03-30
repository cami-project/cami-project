#include "dbus/dbus_helper_5.h"

//using NotificationCallback = (*)(Notification)

static GDBusConnection *bus_connection = nullptr;

static NotificationsTask *notificationsTask = nullptr;

/* Signatures for private functions. */
EnumDBusResult DBusLookupAdapterPath(GVariant *objects, char **adapterPath);

EnumDBusResult DBusLookupDevicePath(GVariant *objects, const char *address, char **path);

char *ay_to_string(const GVariant *variant, size_t *length, GError **error);

byte *ay_to_byte(const GVariant *variant, size_t *length, GError **error);

void _DevicePairedClbk(GObject *source_object, GAsyncResult *res, gpointer user_data);

void _NotificationsPropertiesChanged(GDBusProxy *proxy,
                                     GVariant *changedProperties,
                                     const gchar *const *invalidated_properties,
                                     gpointer userData);

gboolean _StopGMainLoop(gpointer data);


EnumDBusResult DBusConnectionEstablish(GBusType type) {
    GError *error = nullptr;

    /* Establish connection to Bus, if needed */
    if (bus_connection == nullptr) {
        bus_connection = g_bus_get_sync(type, nullptr, &error);

        if (bus_connection == nullptr) {
            g_error_free(error);
            return DBUS_ERROR_CONNECTION;
        }
    }

    return DBUS_OK;
}

EnumDBusResult DBusGetConnection(GDBusConnection **connection) {
    EnumDBusResult res = DBUS_OK;

    if (bus_connection == nullptr) {
        res = DBusConnectionEstablish(G_BUS_TYPE_SYSTEM);
    }
    *connection = bus_connection;

    return res;
}

EnumDBusResult DBusGetManagedObjects(GVariant **objects) {
    GDBusProxy *proxyManager = nullptr;
    GError *error = nullptr;

    proxyManager = g_dbus_proxy_new_sync(bus_connection,
                                         G_DBUS_PROXY_FLAGS_NONE,
                                         nullptr,
                                         BLUEZ_SERVICE,
                                         BLUEZ_MANAGER_PATH,
                                         DBUS_OBJECTMANAGER_INTERFACE,
                                         nullptr,
                                         &error);

    if (proxyManager == nullptr) {
        return DBUS_ERROR_PROXY_CREATION;
    }

    *objects = g_dbus_proxy_call_sync(proxyManager,
                                      DBUS_METHOD_GETMANAGEDOBJECTS,
                                      nullptr,
                                      G_DBUS_CALL_FLAGS_NONE,
                                      -1,
                                      nullptr,
                                      &error);

    return (*objects == nullptr ? DBUS_ERROR_NO_MANAGED_OBJECTS : DBUS_OK);
}

EnumDBusResult DBusAdapterGetObjectPath(char **adapterPath) {
    GVariant *objects = nullptr;

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Get managed objects. BlueZ 5 follows the DBus "standard" of having a ObjectManager
     * with all objects, interfaces and properties [1]. The HCI adapter must be searched on
     * these objects.
     * [1] https://dbus.freedesktop.org/doc/dbus-specification.html#standard-interfaces-objectmanager */
    if (DBusGetManagedObjects(&objects) != DBUS_OK) {
        return DBUS_ERROR_NO_MANAGED_OBJECTS;
    }

    /* Retrieve the HCI Bluetooth interface. The first one available is chosen. */
    if (DBusLookupAdapterPath(objects, adapterPath) != DBUS_OK) {
        return DBUS_ERROR_NO_ADAPTER;
    }

    /* If we didn't find any adapter, just return that information */
    return (*adapterPath == nullptr) ? DBUS_ERROR_NO_ADAPTER : DBUS_OK;
}

EnumDBusResult DBusAdapterStartDiscovery(const char *adapterPath) {
    GDBusProxy *proxy_adapter = nullptr;
    GError *error = nullptr;
    char *adapter = nullptr;
    EnumDBusResult retDbus = DBUS_OK;

    /* Check for DBus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check if we need to get the Default Adapter Path */
    if (adapterPath == nullptr) {
        if ((retDbus = DBusAdapterGetObjectPath(&adapter)) != DBUS_OK) {
            delete[] adapter;
            return retDbus;
        }
        return DBUS_ERROR_CONNECTION;
    }
    /* If the adapter path was given as parameter, use it. */
    else {
        adapter = strdup(adapterPath);
    }

    /* Create Adapter Proxy */
    proxy_adapter = g_dbus_proxy_new_sync(bus_connection,
                                          G_DBUS_PROXY_FLAGS_NONE,
                                          nullptr,
                                          BLUEZ_SERVICE,
                                          adapter,
                                          BLUEZ5_ADAPTER_INTERFACE,
                                          nullptr,
                                          &error);
    if (proxy_adapter == nullptr) {
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Invoke StartDiscovery Method */
    GVariant *values = g_dbus_proxy_call_sync(proxy_adapter,
                                              BLUEZ5_ADAPTER_METHOD_STARTDISCOVERY,
                                              nullptr,
                                              G_DBUS_CALL_FLAGS_NONE,
                                              -1,
                                              nullptr,
                                              &error);
    if (values == nullptr) {
        free(adapter);
        g_object_unref(proxy_adapter);

        return DBUS_ERROR_PROXY_METHOD_CALL;
    }

    /* When we arrive here, everything went well */
    free(adapter);
    g_object_unref(proxy_adapter);

    return DBUS_OK;
}

EnumDBusResult DBusAdapterStopDiscovery(const char *adapterPath) {
    GDBusProxy *proxy_adapter = nullptr;
    GError *error = nullptr;
    char *adapter = nullptr;
    EnumDBusResult retDbus = DBUS_OK;

    /* Check for DBus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check if we need to get the Default Adapter Path */
    if (adapterPath == nullptr) {
        if ((retDbus = DBusAdapterGetObjectPath(&adapter)) != DBUS_OK) {
            delete[] adapter;
            return retDbus;
        }
    }
        /* If the adapter path was given to us, use it */
    else {
        adapter = strdup(adapterPath);
    }

    /* Create Adapter Proxy */
    proxy_adapter = g_dbus_proxy_new_sync(bus_connection,
                                          G_DBUS_PROXY_FLAGS_NONE,
                                          nullptr,
                                          BLUEZ_SERVICE,
                                          adapter,
                                          BLUEZ5_ADAPTER_INTERFACE,
                                          nullptr,
                                          &error);

    if (proxy_adapter == nullptr) {
        /* NOTE: free() is used instead of delete[], because the adapter pointer is initialized with malloc() on
         * the strdup() function. */
        free(adapter);
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Invoke StopDiscovery Method */
    GVariant *values = g_dbus_proxy_call_sync(proxy_adapter,
                                              BLUEZ5_ADAPTER_METHOD_STOPDISCOVERY,
                                              nullptr,
                                              G_DBUS_CALL_FLAGS_NONE,
                                              -1,
                                              nullptr,
                                              &error);

    if (values == nullptr) {
        g_object_unref(proxy_adapter);
        return DBUS_ERROR_PROXY_METHOD_CALL;
    }

    /* NOTE: free() is used instead of delete[], because the adapter pointer is initialized with malloc() on
     * the strdup() function. */
    free(adapter);
    g_object_unref(proxy_adapter);

    return DBUS_OK;
}

EnumDBusResult DBusAgentManagerRegister(const char *agentPath, const char *capabilities) {
//    DBusGProxy* proxy_adapter           = nullptr;
    GDBusProxy *proxy_adapter = nullptr;
//    gboolean retval = FALSE;
    GError *error = nullptr;
//    char *adapter_path_found = nullptr, *adapter_path = nullptr;
//    EnumDBusResult retval_dbus;

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check other parameters */
    if (agentPath == nullptr || capabilities == nullptr) {
        return DBUS_ERROR_INVALID_PARAMETERS;
    }

//    /* Check if we need to find the Adapter Path */
//    if (adapter_path == nullptr) {
//        if ((retval_dbus = DBusAdapterGetObjectPath(&adapter_path_found)) != DBUS_OK) {
//            delete[] adapter_path_found;
//            return retval_dbus;
//        }
//    } else {
//        adapter_path_found = strdup(adapter_path);
//    }

    /* Create the Adapter proxy */
//    proxy_adapter = dbus_g_proxy_new_for_name(bus_connection,
//											  DBUS_SERVICE_ADAPTER,
//											  adapter_path_found,
//											  DBUS_IFACE_ADAPTER );
    proxy_adapter = g_dbus_proxy_new_sync(bus_connection,
                                          G_DBUS_PROXY_FLAGS_NONE,
                                          nullptr,
                                          BLUEZ_SERVICE,
                                          BLUEZ_SERVICE_PATH,//adapter_path_found,
                                          BLUEZ5_AGENTMANAGER_INTERFACE,
                                          nullptr,
                                          &error);

    /* Check for Proxy Creation errors */
    if (proxy_adapter == nullptr) {
//        free(adapter_path_found);
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Call CreatePairedDevice Method */
    g_dbus_proxy_call_sync(proxy_adapter,
                           BLUEZ5_AGENTMANAGER_METHOD_REGISTERAGENT,
                           g_variant_new("(os)",
                                         agentPath,
                                         capabilities),
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);
//							   G_TYPE_STRING, device_address,
//							   DBUS_TYPE_G_OBJECT_PATH, agent_path,
//							   G_TYPE_STRING, capabilities,
//							   G_TYPE_INVALID,
//							   DBUS_TYPE_G_OBJECT_PATH, nullptr,
//							   G_TYPE_INVALID );

    /* The method RegisterAgent returns void, therefore the call must be validated by the error. */
    if (error != nullptr) {
//        free(adapter_path_found);
        g_object_unref(proxy_adapter);
        return DBUS_ERROR_REGISTER_AGENT;
    }

    /* Free Memory */
//    free(adapter_path_found);
    g_object_unref(proxy_adapter);

    return DBUS_OK;
}

EnumDBusResult DBusDeviceConnect(const char *devicePath) {
    GDBusProxy *proxyDevice = nullptr;
    GError *error = nullptr;
    bool isConnected = false;

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check other parameters */
    if (devicePath == nullptr) {
        return DBUS_ERROR_INVALID_PARAMETERS;
    }

    /* Check if the device is connected. Return immediately as there is no need to connect again...*/
    if (DBusDeviceConnectedProperty(devicePath, &isConnected) == DBUS_OK && isConnected == true) {
        return DBUS_OK;
    }

    /* Create the Adapter proxy */
    proxyDevice = g_dbus_proxy_new_sync(bus_connection,
                                        G_DBUS_PROXY_FLAGS_NONE,
                                        nullptr,
                                        BLUEZ_SERVICE,
                                        devicePath,
                                        BLUEZ5_DEVICE_INTERFACE,
                                        nullptr,
                                        &error);

    /* Check for Proxy Creation errors */
    if (proxyDevice == nullptr) {
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Invoke Connect method */
    g_dbus_proxy_call_sync(proxyDevice,
                           BLUEZ5_DEVICE_METHOD_CONNECT,
                           nullptr,
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);

    if (error != nullptr) {
        g_object_unref(proxyDevice);
        return DBUS_ERROR_CONNECTING_DEVICE;
    }

    /* Free Memory */
    g_object_unref(proxyDevice);

    return DBUS_OK;
}

EnumDBusResult DBusDeviceConnectProfile(const char *devicePath, const char *uuid) {
    GDBusConnection *connection = nullptr;
    GDBusProxy *proxyDevice = nullptr;
    GError *error = nullptr;
    char *devicePathFound = nullptr;

    /* Check for Bus Connection */
    DBusGetConnection(&connection);
    if (connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check other parameters */
    if (devicePath == nullptr) {
        return DBUS_ERROR_INVALID_PARAMETERS;
    }

    /* Create the Adapter proxy */
    proxyDevice = g_dbus_proxy_new_sync(connection,
                                        G_DBUS_PROXY_FLAGS_NONE,
                                        nullptr,
                                        BLUEZ_SERVICE,
                                        devicePath,
                                        BLUEZ5_DEVICE_INTERFACE,
                                        nullptr,
                                        &error);

    /* Check for Proxy Creation errors */
    if (proxyDevice == nullptr) {
        free(devicePathFound);
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Invoke connect profile method */
    g_dbus_proxy_call_sync(proxyDevice,
                           BLUEZ5_DEVICE_METHOD_CONNECTPROFILE,
                           g_variant_new("(s)", uuid),
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);

    if (error != nullptr) {
        delete[] devicePathFound;
        g_object_unref(proxyDevice);
        return DBUS_ERROR_CONNECTING_DEVICE;
    }

    /* Free Memory */
    delete[] devicePathFound;
    g_object_unref(proxyDevice);

    return DBUS_OK;
}

EnumDBusResult DBusDeviceDisconnect(const char *devicePath) {
    GDBusProxy *proxyDevice = nullptr;
    GError *error = nullptr;
    bool isConnected = false;

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check other parameters */
    if (devicePath == nullptr) {
        return DBUS_ERROR_INVALID_PARAMETERS;
    }

    /* Check if the device is disconnected. Return immediately as there is no need to disconnect again...*/
    if (DBusDeviceConnectedProperty(devicePath, &isConnected) == DBUS_OK && isConnected == false) {
        return DBUS_OK;
    }

    /* Create the Adapter proxy */
    proxyDevice = g_dbus_proxy_new_sync(bus_connection,
                                        G_DBUS_PROXY_FLAGS_NONE,
                                        nullptr,
                                        BLUEZ_SERVICE,
                                        devicePath,
                                        BLUEZ5_DEVICE_INTERFACE,
                                        nullptr,
                                        &error);

    /* Check for Proxy Creation errors */
    if (proxyDevice == nullptr) {
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Invoke Pair method */
    g_dbus_proxy_call_sync(proxyDevice,
                           BLUEZ5_DEVICE_METHOD_DISCONNECT,
                           nullptr,
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);

    if (error != nullptr) {
        g_object_unref(proxyDevice);
        return DBUS_ERROR_CONNECTING_DEVICE;
    }

    /* Free Memory */
    g_object_unref(proxyDevice);

    return DBUS_OK;
}

EnumDBusResult DBusDeviceGetObjectPath(const char *deviceAddress, const char *adapterPath, char **devicePath) {
    GVariant *objects = nullptr;
    if (DBusGetManagedObjects(&objects) != DBUS_OK) {
        return DBUS_ERROR_NO_MANAGED_OBJECTS;
    }

    /* Lookup for the device path based on the given address. */
    if (DBusLookupDevicePath(objects, deviceAddress, devicePath) != DBUS_OK) {
        return DBUS_ERROR_NO_DEVICE_FOUND;
    }

    return DBUS_OK;
}

EnumDBusResult DBusGetInterfaceProperty(const char *objectPath,
                                        const char *interface,
                                        const char *propertyName,
                                        GVariant **value) {
    GDBusProxy *proxy = nullptr;
    GError *error = nullptr;

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check other parameters */
    if (objectPath == nullptr) {
        return DBUS_ERROR_INVALID_PARAMETERS;
    }

    /* Create the Adapter proxy */
    proxy = g_dbus_proxy_new_sync(bus_connection,
                                  G_DBUS_PROXY_FLAGS_NONE,
                                  nullptr,
                                  BLUEZ_SERVICE,
                                  objectPath,
                                  interface,
                                  nullptr,
                                  &error);

    /* Check for Proxy Creation errors */
    if (proxy == nullptr) {
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Invoke Pair method */
    *value = g_dbus_proxy_get_cached_property(proxy,
                                              propertyName);

    if (*value == nullptr) {
        g_object_unref(proxy);
        return DBUS_ERROR_CONNECTING_DEVICE;
    }

    /* Free Memory */
    g_object_unref(proxy);

    return DBUS_OK;
}

EnumDBusResult DBusDeviceGetProperty(const char *devicePath, const char *propertyName, GVariant **value) {
    return DBusGetInterfaceProperty(devicePath, BLUEZ5_DEVICE_INTERFACE, propertyName, value);
}

EnumDBusResult DBusDeviceAddressProperty(const char *devicePath, string *address) {
    GVariant *value = nullptr;
    EnumDBusResult res;

    if ((res = DBusDeviceGetProperty(devicePath, BLUEZ5_DEVICE_PROPERTY_ADDRESS, &value)) != DBUS_OK) {
        return res;
    }

    if (value != nullptr) {
        *address = string(g_variant_get_string(value, nullptr));
        return DBUS_OK;
    } else {
        return DBUS_ERROR_READING_PROPERTY;
    }

}

EnumDBusResult DBusDeviceAppearanceProperty(const char *devicePath, uint16_t *appearance) {
    GVariant *value = nullptr;
    EnumDBusResult res;

    if ((res = DBusDeviceGetProperty(devicePath, BLUEZ5_DEVICE_PROPERTY_APPEARANCE, &value)) != DBUS_OK) {
        return res;
    }

    if (value != nullptr) {
        *appearance = static_cast<uint16_t>(g_variant_get_uint16(value));
        return DBUS_OK;
    } else {
        return DBUS_ERROR_READING_PROPERTY;
    }

}

EnumDBusResult DBusDeviceConnectedProperty(const char *devicePath, bool *connected) {
    GVariant *value = nullptr;
    EnumDBusResult res;

    if ((res = DBusDeviceGetProperty(devicePath, BLUEZ5_DEVICE_PROPERTY_CONNECTED, &value)) != DBUS_OK) {
        return res;
    }

    if (value) {
        *connected = g_variant_get_boolean(value);
        return DBUS_OK;
    } else {
        return DBUS_ERROR_READING_PROPERTY;
    }

}

EnumDBusResult DBusDeviceNameProperty(const char *devicePath, string *name) {
    GVariant *value = nullptr;
    EnumDBusResult res;

    if ((res = DBusDeviceGetProperty(devicePath, BLUEZ5_DEVICE_PROPERTY_NAME, &value)) != DBUS_OK) {
        return res;
    }

    if (value != nullptr) {
        *name = string(g_variant_get_string(value, nullptr));
        return DBUS_OK;
    } else {
        return DBUS_ERROR_READING_PROPERTY;
    }

}

EnumDBusResult DBusDevicePairedProperty(const char *devicePath, bool *paired) {
    GVariant *value = nullptr;
    EnumDBusResult res;

    if ((res = DBusDeviceGetProperty(devicePath, BLUEZ5_DEVICE_PROPERTY_PAIRED, &value)) != DBUS_OK) {
        return res;
    }

    if (value != nullptr) {
        *paired = g_variant_get_boolean(value);
        return DBUS_OK;
    } else {
        return DBUS_ERROR_READING_PROPERTY;
    }

}

EnumDBusResult DBusDeviceServicesResolvedProperty(const char *devicePath, bool *servicesResolved) {
    GVariant *value = nullptr;
    EnumDBusResult res;

    if ((res = DBusDeviceGetProperty(devicePath, BLUEZ5_DEVICE_PROPERTY_SERVICESRESOLVED, &value)) != DBUS_OK) {
        return res;
    }

    if (value != nullptr) {
        *servicesResolved = g_variant_get_boolean(value);
        return DBUS_OK;
    } else {
        return DBUS_ERROR_READING_PROPERTY;
    }

}

EnumDBusResult DBusDeviceUuidsProperty(const char *devicePath, vector <string> *uuids) {
    GVariant *value = nullptr;
    GVariantIter it;
    gchar *uuid = nullptr;
    EnumDBusResult res;

    if ((res = DBusDeviceGetProperty(devicePath, BLUEZ5_DEVICE_PROPERTY_UUIDS, &value)) != DBUS_OK) {
        return res;
    }
    if (value != nullptr) {
        g_variant_iter_init(&it, value);
        while ((g_variant_iter_next(&it, "s", &uuid))) {
            uuids->push_back(string(uuid));
        }
        return DBUS_OK;
    } else {
        return DBUS_ERROR_READING_PROPERTY;
    }


}

EnumDBusResult DBusDeviceReadNotifications(const char *devicePath,
                                           const char *serviceHandle,
                                           const char *characteristicHandle,
                                           NotificationsCollection *notifications,
                                           unsigned int timeout) {
    GDBusProxy *proxyBus = nullptr, *proxyGatt = nullptr;
    GError *error = nullptr;
    char *deviceCharObjPath = nullptr;
    auto _CreateCharacteristicObjectPath = [&]() -> void {
        const char *kService = "/service";
        const char *kCharacteristic = "/char";
        const size_t kHandleLength = 4;
        size_t length = strlen(devicePath) +
                        strlen(kService) +
                        strlen(kCharacteristic) +
                        kHandleLength * 2;
        deviceCharObjPath = new char[length + 1];

        strncpy(deviceCharObjPath, devicePath, strlen(devicePath) + 1);
        strncat(deviceCharObjPath, kService, strlen(kService) + 1);
        strncat(deviceCharObjPath, serviceHandle, kHandleLength);
        strncat(deviceCharObjPath, kCharacteristic, strlen(kCharacteristic) + 1);
        strncat(deviceCharObjPath, characteristicHandle, kHandleLength);
        deviceCharObjPath[length] = '\0';
    };

    /* Append service and characteristic. */
    _CreateCharacteristicObjectPath();

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        delete[] deviceCharObjPath;
        return DBUS_ERROR_CONNECTION;
    }

    proxyBus = g_dbus_proxy_new_sync(bus_connection,
                                     G_DBUS_PROXY_FLAGS_NONE,
                                     nullptr,
                                     BLUEZ_SERVICE,
                                     deviceCharObjPath,
                                     DBUS_PROPERTIES_INTERFACE,
                                     nullptr,
                                     &error);

    /* Check for Proxy Creation errors */
    if (proxyBus == nullptr) {
        delete[] deviceCharObjPath;
        return DBUS_ERROR_PROXY_CREATION;
    }

    proxyGatt = g_dbus_proxy_new_sync(bus_connection,
                                      G_DBUS_PROXY_FLAGS_DO_NOT_CONNECT_SIGNALS,
                                      nullptr,
                                      BLUEZ_SERVICE,
                                      deviceCharObjPath,
                                      BLUEZ5_GATTCHARACTERISTIC_INTERFACE,
                                      nullptr,
                                      &error);

    gulong idPropertiesChanged = g_signal_connect(proxyGatt,
                                                  "g-properties-changed",
                                                  G_CALLBACK(_NotificationsPropertiesChanged),
                                                  notifications);

    /* Invoke StartNotify method */
    g_dbus_proxy_call_sync(proxyGatt,
                           BLUEZ5_GATTCHARACTERISTIC_METHOD_STARTNOTIFY,
                           nullptr,
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);

    if (error != nullptr) {
        g_object_unref(proxyGatt);
        delete[] deviceCharObjPath;
        return DBUS_ERROR_NOTIFICATION;
    }

    GMainLoop *main_loop = g_main_loop_new(nullptr, FALSE);
    if (timeout > 0) {
        g_timeout_add(timeout * 1000, _StopGMainLoop, main_loop);
    }
    g_main_loop_run(main_loop);

    /* Invoke StopNotify method */
    g_dbus_proxy_call_sync(proxyGatt,
                           BLUEZ5_GATTCHARACTERISTIC_METHOD_STOPNOTIFY,
                           nullptr,
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);

    if (error != nullptr) {
        g_object_unref(proxyGatt);
        delete[] deviceCharObjPath;
        return DBUS_ERROR_NOTIFICATION;
    }

    /* Free Memory */
    g_signal_handler_disconnect(proxyGatt, idPropertiesChanged);
    g_object_unref(proxyBus);
    g_object_unref(proxyGatt);
    delete[] deviceCharObjPath;

    return DBUS_OK;
}

EnumDBusResult DBusDeviceStartNotify(const char *devicePath,
                                     const char *serviceHandle,
                                     const char *characteristicHandle) {
    GDBusProxy *proxyBus = nullptr, *proxyGatt = nullptr;
    GError *error = nullptr;
    char *deviceCharObjPath = nullptr;
    auto _CreateCharacteristicObjectPath = [&]() -> void {
        const char *kService = "/service";
        const char *kCharacteristic = "/char";
        const size_t kHandleLength = 4;
        size_t length = strlen(devicePath) +
                        strlen(kService) +
                        strlen(kCharacteristic) +
                        kHandleLength * 2;
        deviceCharObjPath = new char[length];

        strncpy(deviceCharObjPath, devicePath, strlen(devicePath));
        strncat(deviceCharObjPath, kService, strlen(kService));
        strncat(deviceCharObjPath, serviceHandle, kHandleLength);
        strncat(deviceCharObjPath, kCharacteristic, strlen(kCharacteristic));
        strncat(deviceCharObjPath, characteristicHandle, kHandleLength);
    };

    /* Append service and characteristic. */
    _CreateCharacteristicObjectPath();

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        delete[] deviceCharObjPath;
        return DBUS_ERROR_CONNECTION;
    }

    proxyBus = g_dbus_proxy_new_sync(bus_connection,
                                     G_DBUS_PROXY_FLAGS_NONE,
                                     nullptr,
                                     BLUEZ_SERVICE,
                                     deviceCharObjPath,
                                     DBUS_PROPERTIES_INTERFACE,
                                     nullptr,
                                     &error);

    /* Check for Proxy Creation errors */
    if (proxyBus == nullptr) {
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Instance notifications task, if it doesn't exist. Only one instance allowed! */
    if (notificationsTask == nullptr) {
        notificationsTask = new NotificationsTask();
    }

//    proxyGatt = g_dbus_proxy_new_sync(bus_connection,
    notificationsTask->NotificationsProxy = g_dbus_proxy_new_sync(bus_connection,
                                                                  G_DBUS_PROXY_FLAGS_DO_NOT_CONNECT_SIGNALS,
                                                                  nullptr,
                                                                  BLUEZ_SERVICE,
                                                                  deviceCharObjPath,
                                                                  BLUEZ5_GATTCHARACTERISTIC_INTERFACE,
                                                                  nullptr,
                                                                  &error);


    notificationsTask->CallbackId = g_signal_connect(notificationsTask->NotificationsProxy,
                                                     "g-properties-changed",
                                                     G_CALLBACK(_NotificationsPropertiesChanged),
                                                     nullptr);

    /* Invoke StartNotify method */
    g_dbus_proxy_call_sync(notificationsTask->NotificationsProxy,
                           BLUEZ5_GATTCHARACTERISTIC_METHOD_STARTNOTIFY,
                           nullptr,
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);

    if (error != nullptr) {
        PRINT_DEBUG("DBUS_ERROR_NOTIFICATION = " << error->message)
        return DBUS_ERROR_NOTIFICATION;
    }

    notificationsTask->Loop = g_main_loop_new(nullptr, FALSE);
    notificationsTask->Thread = std::thread([]() {
        g_main_loop_run(notificationsTask->Loop);
    });
    notificationsTask->Thread.detach();

    if (error != nullptr) {
        delete[] deviceCharObjPath;
        g_object_unref(proxyBus);
        g_object_unref(proxyGatt);
        return DBUS_ERROR_NOTIFICATION;
    }

    /* Free Memory */
    delete[] deviceCharObjPath;
    g_object_unref(proxyBus);
//    g_object_unref(proxyGatt);

    return DBUS_OK;
}

EnumDBusResult DBusDeviceStopNotify(const char *devicePath,
                                    const char *serviceHandle,
                                    const char *characteristicHandle) {
//    GDBusProxy *proxyBus = nullptr, *proxyGatt = nullptr;
    GError *error = nullptr;
//    char *deviceCharObjPath = nullptr;
//    auto _CreateCharacteristicObjectPath = [&]() -> void {
//        const char *kService = "/service";
//        const char *kCharacteristic = "/char";
//        const size_t kHandleLength = 4;
//        size_t length = strlen(devicePath) +
//                        strlen(kService) +
//                        strlen(kCharacteristic) +
//                        kHandleLength * 2;
//        deviceCharObjPath = new char[length];
//
//        strncpy(deviceCharObjPath, devicePath, strlen(devicePath));
//        strncat(deviceCharObjPath, kService, strlen(kService));
//        strncat(deviceCharObjPath, serviceHandle, kHandleLength);
//        strncat(deviceCharObjPath, kCharacteristic, strlen(kCharacteristic));
//        strncat(deviceCharObjPath, characteristicHandle, kHandleLength);
//    };
//
//    /* Append service and characteristic. */
//    _CreateCharacteristicObjectPath();

    /* Check for Bus Connection */
//    if (bus_connection == nullptr) {
//        return DBUS_ERROR_CONNECTION;
//    }

//    proxyBus = g_dbus_proxy_new_sync(bus_connection,
//                                     G_DBUS_PROXY_FLAGS_NONE,
//                                     nullptr,
//                                     BLUEZ_SERVICE,
//                                     deviceCharObjPath,
//                                     DBUS_PROPERTIES_INTERFACE,
//                                     nullptr,
//                                     &error);
//
//    /* Check for Proxy Creation errors */
//    if (proxyBus == nullptr) {
//        return DBUS_ERROR_PROXY_CREATION;
//    }
//
//    proxyGatt = g_dbus_proxy_new_sync(bus_connection,
//                                      G_DBUS_PROXY_FLAGS_DO_NOT_CONNECT_SIGNALS,
//                                      nullptr,
//                                      BLUEZ_SERVICE,
//                                      deviceCharObjPath,
//                                      BLUEZ5_GATTCHARACTERISTIC_INTERFACE,
//                                      nullptr,
//                                      &error);

    /* Invoke StopNotify method */
//    g_dbus_proxy_call_sync(proxyGatt,
    g_dbus_proxy_call_sync(notificationsTask->NotificationsProxy,
                           BLUEZ5_GATTCHARACTERISTIC_METHOD_STOPNOTIFY,
                           nullptr,
                           G_DBUS_CALL_FLAGS_NONE,
                           -1,
                           nullptr,
                           &error);

    if (error != nullptr) {
//        g_object_unref(proxyBus);
//        g_object_unref(proxyGatt);
        return DBUS_ERROR_NOTIFICATION;
    }

    /* Stop loop thread. */
    g_main_loop_quit(notificationsTask->Loop);
    g_signal_handler_disconnect(notificationsTask->NotificationsProxy,
                                notificationsTask->CallbackId);

    /* Free Memory */
//    g_object_unref(proxyBus);
//    g_object_unref(proxyGatt);
    delete notificationsTask;

    return DBUS_OK;
}

EnumDBusResult DBusDevicePair(const char *devicePath) {
    GDBusProxy *proxy_adapter = nullptr;
//    gboolean retval = FALSE;
    GError *error = nullptr;
    //char *adapter_path_found = nullptr;
    char *adapterPath, *devicePathFound = nullptr;
//    EnumDBusResult retval_dbus;

    /* Check for Bus Connection */
    if (bus_connection == nullptr) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check other parameters */
//    if (device_address == nullptr || agent_path == nullptr || capabilities == nullptr) {
//        return DBUS_ERROR_INVALID_PARAMETERS;
//    }

    if (DBusAdapterGetObjectPath(&adapterPath) != DBUS_OK) {
        return DBUS_ERROR_NO_ADAPTER;
    }

    /* Check if we need to find the Adapter Path */
//    if (devicePath == nullptr) {
//        if ((retval_dbus = DBusDeviceGetObjectPath(device_address, adapterPath, &devicePathFound)) != DBUS_OK) {//DBusAdapterGetObjectPath(&adapter_path_found)) != DBUS_OK) {
//            free(devicePathFound);
//            return DBUS_ERROR_NO_DEVICE_FOUND;
//        }
//    } else {
//        devicePathFound = strdup(devicePath);
//    }

    /* Create the Adapter proxy */
//    proxy_adapter = dbus_g_proxy_new_for_name(bus_connection,
//											  DBUS_SERVICE_ADAPTER,
//											  adapter_path_found,
//											  DBUS_IFACE_ADAPTER );
    proxy_adapter = g_dbus_proxy_new_sync(bus_connection,
                                          G_DBUS_PROXY_FLAGS_NONE,
                                          nullptr,
                                          BLUEZ_SERVICE,
                                          devicePath,
                                          BLUEZ5_DEVICE_INTERFACE,
                                          nullptr,
                                          &error);

    /* Check for Proxy Creation errors */
    if (proxy_adapter == nullptr) {
        free(devicePathFound);
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Call CreatePairedDevice Method */
//    retval = dbus_g_proxy_call(proxy_adapter, DBUS_METHOD_ADAPTER_CREATEPAIREDDEVICE, &error,
//							   G_TYPE_STRING, device_address,
//							   DBUS_TYPE_G_OBJECT_PATH, agent_path,
//							   G_TYPE_STRING, capabilities,
//							   G_TYPE_INVALID,
//							   DBUS_TYPE_G_OBJECT_PATH, nullptr,
//							   G_TYPE_INVALID );
    /* Invoke Pair method */
//    GVariant *values = g_dbus_proxy_call_sync(proxy_adapter,
//                                              BLUEZ5_DEVICE_METHOD_PAIR,
//                                              nullptr,
//                                              G_DBUS_CALL_FLAGS_NONE,
//                                              -1,
//                                              nullptr,
//                                              &error);
    g_dbus_proxy_call(proxy_adapter,
                      BLUEZ5_DEVICE_METHOD_PAIR,
                      nullptr,
                      G_DBUS_CALL_FLAGS_NONE,
                      -1,
                      nullptr,
                      _DevicePairedClbk,
                      nullptr);

//    if (error == nullptr) {
//        delete[] devicePathFound;
//        g_object_unref(proxy_adapter);
//        return DBUS_ERROR_NOT_PAIRED;
//    }

    /* Free Memory */
    delete[] devicePathFound;
    g_object_unref(proxy_adapter);

    return DBUS_OK;
}

EnumDBusResult DBusGattGetProperty(const char *servicePath, const char *propertyName, GVariant **value) {
    return DBusGetInterfaceProperty(servicePath, BLUEZ5_DEVICE_INTERFACE, propertyName, value);
}

EnumDBusResult DBusGattCharacteristicReadValue(const char *devicePath,
                                               const char *serviceHandle,
                                               const char *characteristicHandle,
                                               byte **value,
                                               string *uuid) {
    GDBusConnection *connection = nullptr;
    GDBusProxy *proxyDevice = nullptr;
    GError *error = nullptr;
    char *deviceCharObjPath = nullptr;
    auto _CreateCharacteristicObjectPath = [&]() -> void {
        const char *kService = "/service";
        const char *kCharacteristic = "/char";
        const size_t kHandleLength = 4;
        size_t length = strlen(devicePath) +
                        strlen(kService) +
                        strlen(kCharacteristic) +
                        kHandleLength * 2;
        deviceCharObjPath = new char[length];

        strncpy(deviceCharObjPath, devicePath, strlen(devicePath));
        strncat(deviceCharObjPath, kService, strlen(kService));
        strncat(deviceCharObjPath, serviceHandle, kHandleLength);
        strncat(deviceCharObjPath, kCharacteristic, strlen(kCharacteristic));
        strncat(deviceCharObjPath, characteristicHandle, kHandleLength);
    };

    /* Check for Bus Connection */
    EnumDBusResult r = DBusGetConnection(&connection);
    if (connection == nullptr || g_dbus_connection_is_closed(connection) || r != DBUS_OK) {
        return DBUS_ERROR_CONNECTION;
    }

    /* Check if Device Path is set */
    if (devicePath == nullptr) {
        return DBUS_ERROR_INVALID_PARAMETERS;
    }

    /* Append service and characteristic. */
    _CreateCharacteristicObjectPath();

    /* Create Device Proxy */
    proxyDevice = g_dbus_proxy_new_sync(connection,
                                        G_DBUS_PROXY_FLAGS_NONE,
                                        nullptr,
                                        BLUEZ_SERVICE,
                                        deviceCharObjPath,
                                        BLUEZ5_GATTCHARACTERISTIC_INTERFACE,
                                        nullptr,
                                        &error);

    if (proxyDevice == nullptr) {
        delete[] deviceCharObjPath;
        return DBUS_ERROR_PROXY_CREATION;
    }

    /* Call ReadValue Method */
    GVariantBuilder *builder = g_variant_builder_new(G_VARIANT_TYPE("a{sv}"));
    GVariant *dict = g_variant_builder_end(builder);
    g_variant_builder_unref(builder);

    GVariant *variant = g_dbus_proxy_call_sync(proxyDevice,
                                               BLUEZ5_GATTCHARACTERISTIC_METHOD_READVALUE,
                                               g_variant_new_tuple(&dict, 1),
                                               G_DBUS_CALL_FLAGS_NONE,
                                               -1,
                                               nullptr,
                                               &error);

    if (variant == nullptr) {
        delete[] deviceCharObjPath;
        g_object_unref(proxyDevice);
        return DBUS_ERROR_READING_VALUE;
    }

//    GVariant *array = nullptr;
//    g_variant_get(variant, "(@ay)", &array);
//    *value = ay_to_byte(array, nullptr, nullptr);

    /* Read UUID for the value. */
    if (uuid != nullptr) {
        if (DBusGetInterfaceProperty(deviceCharObjPath,
                                     BLUEZ5_GATTCHARACTERISTIC_INTERFACE,
                                     BLUEZ5_GATTCHARACTERISTIC_PROPERTY_UUID,
                                     &variant) != DBUS_OK) {
            delete[] deviceCharObjPath;
            g_object_unref(proxyDevice);
            return DBUS_ERROR_READING_PROPERTY;
        } else {
            if (variant != nullptr) {
                *uuid = string(g_variant_get_string(variant, nullptr));
            } else {
                PRINT_DEBUG("variant is null!")
            };
        }
    }

    delete[] deviceCharObjPath;
    g_object_unref(proxyDevice);

    return DBUS_OK;
}

void _DevicePairedClbk(GObject *source_object, GAsyncResult *res, gpointer user_data) {
    PRINT_DEBUG("Device paired!");
}

void _NotificationsPropertiesChanged(GDBusProxy *proxy,
                                     GVariant *changedProperties,
                                     const gchar *const *invalidated_properties,
                                     gpointer userData) {
    PRINT_DEBUG("changedProperties = " << g_variant_print(changedProperties, TRUE));

    /* Nothing to do... */
    if (userData == nullptr) {
        return;
    }

    NotificationsCollection *notifications = reinterpret_cast<NotificationsCollection *>(userData);

    const gchar *ifaceName = g_dbus_proxy_get_interface_name(proxy);
    /* If the interface that received the interrupt is not GattCharacteristic1, then stop. This should never
     * happen, since the callback is associated with the proxy for the GattCharacteristic1 interface. */
    if (strncmp(ifaceName, BLUEZ5_GATTCHARACTERISTIC_INTERFACE, strlen(BLUEZ5_GATTCHARACTERISTIC_INTERFACE)) != 0) {
        return;
    }
    /* If the changed properties are not for a value, ignore it. */
    GVariant *array = nullptr;
    if ((array = g_variant_lookup_value(changedProperties, "Value", nullptr)) == nullptr) {
        return;
    }

    /* Get values form the notifications array. */
    size_t length;
    const byte *v = reinterpret_cast<const byte *>(g_variant_get_fixed_array(array, &length, sizeof(byte)));

    /* Get the measurement's UUID. */
    const gchar *deviceObjectPath = g_dbus_proxy_get_object_path(proxy);
    string uuid;
    if (DBusGetInterfaceProperty(deviceObjectPath,
                                 BLUEZ5_GATTCHARACTERISTIC_INTERFACE,
                                 BLUEZ5_GATTCHARACTERISTIC_PROPERTY_UUID, &array) == DBUS_OK) {
        uuid = string(g_variant_get_string(array, nullptr));
    }

    notifications->emplace(notifications->end(), v, length, uuid);
}

gboolean _StopGMainLoop(gpointer data) {
    g_main_loop_quit(static_cast<GMainLoop *>(data));
    return FALSE;
}

void PairAgentMethodCall(GDBusConnection *connection,
                         const gchar *sender,
                         const gchar *object_path,
                         const gchar *interface_name,
                         const gchar *method_name,
                         GVariant *parameters,
                         GDBusMethodInvocation *invocation,
                         gpointer user_data) {
    auto IsMethod = [&](const char *method) -> bool {
        if (method_name == nullptr || method == nullptr || !strncmp(method_name, "", 1) || !strncmp(method, "", 1))
            return false;
        else
            return strncmp(method_name, method, strlen(method_name)) == 0;
    };

    /* If the request is not from the Agent interface, ignore it. */
    if (strncmp(interface_name, BLUEZ5_AGENT_INTERFACE, strlen(BLUEZ5_AGENT_INTERFACE)) != 0) {
        return;
    }

    if (IsMethod(BLUEZ5_AGENT_METHOD_RELEASE)) {
        //return agent_method_release(connection, message, data);
        // TODO change this method
        return;
    } else {
        return;
    }
//    else if (IsMethod(BLUEZ5_AGENT_METHOD_REQUESTPINCODE))
//        return DBUS_HANDLER_RESULT_HANDLED;
//    else if (IsMethod(BLUEZ5_AGENT_METHOD_DISPLAYPINCODE))
//        return DBUS_HANDLER_RESULT_HANDLED;
//    else if (IsMethod(BLUEZ5_AGENT_METHOD_REQUESTPASSKEY))
//        return DBUS_HANDLER_RESULT_HANDLED;
//    else if (IsMethod(BLUEZ5_AGENT_METHOD_DISPLAYPASSKEY))
//        return DBUS_HANDLER_RESULT_HANDLED;
//    else if (IsMethod(BLUEZ5_AGENT_METHOD_REQUESTCONFIRMATION))
//        return DBUS_HANDLER_RESULT_HANDLED;
//    else if (IsMethod(BLUEZ5_AGENT_METHOD_AUTHORIZESERVICE))
//        return DBUS_HANDLER_RESULT_HANDLED;
//    else if (IsMethod(BLUEZ5_AGENT_METHOD_CANCEL))
//        return DBUS_HANDLER_RESULT_HANDLED;
//
//    return DBUS_HANDLER_RESULT_NOT_YET_HANDLED;
}


void ReadAgentIntrospectionXmlData(char **introspectionData) {
    std::ifstream stream(kAgentIntrospectionFile);

    if (stream.is_open()) {
        stream.seekg(0, std::ios::end);
        size_t size = stream.tellg();
        std::string str(size, ' ');
        stream.seekg(0);
        stream.read(&str[0], size);

        size_t length = str.length();
        *introspectionData = new char[length + 1];
        strncpy(*introspectionData, str.c_str(), length);
    } else {
        *introspectionData = nullptr;
    }
}

EnumDBusResult DBusLookupAdapterPath(GVariant *objects, char **adapterPath) {
    GVariant *ifaces;
    GVariantIter i;
    const char *path;

//    PRINT_DEBUG("objects = " << g_variant_print(objects, TRUE));
    g_variant_iter_init(&i, g_variant_get_child_value(objects, 0));

    while ((g_variant_iter_next(&i, "{&o*}", &path, &ifaces))) {
//        PRINT_DEBUG("ifaces = " << g_variant_print(ifaces, TRUE))
        if (g_variant_lookup_value(ifaces, BLUEZ5_ADAPTER_INTERFACE, G_VARIANT_TYPE_DICTIONARY)) {
            size_t length = strlen(path);
            *adapterPath = new char[length + 1];
            memcpy(*adapterPath, path, length);
            (*adapterPath)[length] = '\0';
        }
    }

    g_variant_unref(ifaces);

    return DBUS_OK;
}

EnumDBusResult DBusLookupDevicePath(GVariant *objects, const char *address, char **path) {
    GVariant *ifaces = nullptr, *device = nullptr;
    GVariantIter i;
    const char *devicePath, *deviceAddress;
    bool deviceFound = false;

//    PRINT_DEBUG("objects = " << g_variant_print(objects, TRUE));
    g_variant_iter_init(&i, g_variant_get_child_value(objects, 0));
    while ((g_variant_iter_next(&i, "{&o*}", &devicePath, &ifaces)) && deviceFound == false) {
//        PRINT_DEBUG("ifaces = " << g_variant_print(ifaces, TRUE))
        if ((device = g_variant_lookup_value(ifaces, BLUEZ5_DEVICE_INTERFACE, G_VARIANT_TYPE_DICTIONARY)) != nullptr) {
//            PRINT_DEBUG("device = " << g_variant_print(device, TRUE))
            device = g_variant_lookup_value(device, BLUEZ5_DEVICE_PROPERTY_ADDRESS, G_VARIANT_TYPE_STRING);
            deviceAddress = g_variant_get_string(device, nullptr);
            if (strncmp(deviceAddress, address, strlen(deviceAddress)) == 0) {
                size_t length = strlen(devicePath);
                *path = new char[length + 1];
                memcpy(*path, devicePath, length);
                (*path)[length] = '\0';
                g_variant_unref(device);
                deviceFound = true;
            }
        }
    }

    if (ifaces != nullptr)
        g_variant_unref(ifaces);
    if (device != nullptr)
        g_variant_unref(device);

    return deviceFound ? DBUS_OK : DBUS_ERROR_NO_DEVICE_FOUND;
}

EnumDBusResult DBusLookupDeviceProperties(GVariant *objects, BluetoothDeviceProperties *device) {
    GVariant *varDevice = nullptr, *varProperties = nullptr;
    const char *path;

    auto _GetStringFromVariant = [](const GVariant *variant, const char *name) -> const char * {
        gsize length;
        GVariant *varValue = g_variant_lookup_value(const_cast<GVariant *>(variant), name, G_VARIANT_TYPE_STRING);
        return (varValue == nullptr) ? nullptr : g_variant_dup_string(varValue, &length);
    };

    auto _GetBoolFromVariant = [](const GVariant *variant, const char *name) -> bool {
        GVariant *varValue = g_variant_lookup_value(const_cast<GVariant *>(variant), name, G_VARIANT_TYPE_BOOLEAN);
        return g_variant_get_boolean(varValue);
    };

    auto _GetListStringsFromVariant = [](const GVariant *variant, const char *name) -> vector <string> {
        gchar *str;
        GVariantIter it;
        vector <string> list;
        GVariant *value = g_variant_lookup_value(const_cast<GVariant *>(variant), name, G_VARIANT_TYPE_STRING_ARRAY);

        if (value != nullptr) {
            g_variant_iter_init(&it, value);
            while ((g_variant_iter_next(&it, "s", &str))) {
                if (str != nullptr) {
                    list.push_back(string(str));
                }
            }
        }

        return list;
    };

    /* Retrieve object_path and all device interfaces + properties. The message complies with the signal
     * org.freedesktop.DBus.ObjectManager.InterfacesAdded. More information here:
     * https://dbus.freedesktop.org/doc/dbus-specification.html#standard-interfaces-properties */
    g_variant_get(objects, "(&o*)", &path, &varProperties);
    varDevice = g_variant_lookup_value(varProperties, BLUEZ5_DEVICE_INTERFACE, G_VARIANT_TYPE_DICTIONARY);

    /* If the object path of the interrupt is not device (for example /org/bluez/hci0/dev_00_11_22_33_44_55), then it
     * won't have value to retrieve. Therefore, return. */
    if (std::regex_match(path, std::regex("^\\/org\\/bluez\\/hci[0-9]\\/dev(_[0-9A-Fa-f]{2}){6}$")) == false) {
        device = nullptr;
        if (varDevice != nullptr) {
            g_variant_unref(varDevice);
        }
        if (varProperties != nullptr) {
            g_variant_unref(varProperties);
        }

        return DBUS_ERROR_NO_DEVICE_FOUND;
    }

    /* Example structure for a newly added Bluetooth device:
     *
     * {'Address': <'4A:58:36:33:82:3A'>,
     *  'Name': <'Blood Pressure'>,
     *  'Alias': <'Blood Pressure'>,
     *  'Paired': <false>,
     *  'Trusted': <false>,
     *  'Blocked': <false>,
     *  'LegacyPairing': <false>,
     *  'RSSI': <int16 -59>,
     *  'Connected': <false>,
     *  'UUIDs': <['0000180a-0000-1000-8000-00805f9b34fb', '00001810-0000-1000-8000-00805f9b34fb']>,
     *  'Adapter': <objectpath '/org/bluez/hci0'>
     * } */

    device->ObjectPath = string(path);
    device->Address = string(_GetStringFromVariant(varDevice, BLUEZ5_DEVICE_PROPERTY_ADDRESS));
    const char *str = _GetStringFromVariant(varDevice, BLUEZ5_DEVICE_PROPERTY_NAME);
    device->Name = string((str == nullptr ? "(unknown)" : str));
    device->Paired = _GetBoolFromVariant(varDevice, BLUEZ5_DEVICE_PROPERTY_PAIRED);
    device->UUIDs = _GetListStringsFromVariant(varDevice, BLUEZ5_DEVICE_PROPERTY_UUIDS);

    g_variant_unref(varDevice);
    g_variant_unref(varProperties);

    return DBUS_OK;
}

char *ay_to_string(const GVariant *variant, size_t *length, GError **error) {
    gsize len;
    const char *data;

    data = static_cast<const char *>(g_variant_get_fixed_array(const_cast<GVariant *>(variant), &len, sizeof(char)));
    if (len == 0) {
        return nullptr;
    }

    /* Make sure there are no embedded NULs */
    if (memchr(data, '\0', len) != nullptr) {
        g_set_error_literal(error, G_DBUS_ERROR, G_DBUS_ERROR_INVALID_ARGS,
                            "String is shorter than claimed");
        return nullptr;
    }

    if (length != nullptr) {
        *length = len;
    }

    return g_strndup(data, len);
}

byte *ay_to_byte(const GVariant *variant, size_t *length, GError **error) {
    return reinterpret_cast<byte *>(ay_to_string(variant, length, error));
}