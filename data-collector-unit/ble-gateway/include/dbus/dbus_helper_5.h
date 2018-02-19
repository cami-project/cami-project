//
//  Created by Jorge Miranda on 07/12/2016
//

#ifndef DBUS_HELPER_H
#define DBUS_HELPER_H

#include <gio/gio.h>
#include <cstring>
#include <fstream>
#include <regex>
#include <string>
#include <thread>
#include <vector>
#include "AdCamiUtilities.h"
#include "dbus_helper_defs.h"

using std::vector;
using AdCamiUtilities::AdCamiBuffer;

static constexpr char kAgentPairPath[] = "/adcamid/btpairagent";
static constexpr char kAgentIntrospectionFile[] = "../xml/Agent1.xml";

using BluetoothDeviceProperties = struct _bluetoothDeviceProperties {
    string ObjectPath;
    string Address;
    string Name;
    bool Paired;
    vector <string> UUIDs;
};

using ValueNotification = struct _value_notification {
    _value_notification() : Value(), UUID("") {}

    _value_notification(const byte *value, const size_t &length, const string &uuid) :
            Value(length, value), UUID(uuid) {}

    AdCamiUtilities::AdCamiBuffer <byte> Value;
    string UUID;
};

using NotificationsCollection = vector<ValueNotification>;

class NotificationsTask {
public:
    GMainLoop *Loop;
    gulong CallbackId;
    std::thread Thread;
    GDBusProxy *NotificationsProxy;

    NotificationsTask() : Loop(nullptr), CallbackId(0), NotificationsProxy(nullptr) {}

    ~NotificationsTask() {
        g_object_unref(NotificationsProxy);
    }

    bool IsRunning() const {
        return this->Loop != nullptr && g_main_loop_is_running(this->Loop);
    }
};

/**
 * Closes the active connection with the Bus.
 * @return 0 in case of success, > 0 otherwise. Error can be identified with EnumDBusResult codes.
 */
//EnumDBusResult DBusConnectionClose();

/**
 * Establishes a connection to the specified Bus.
 * @param type Bus we want to connect to.
 * @return 0 in case of success, > 0 otherwise. Error can be identified with EnumDBusResult codes.
 */
EnumDBusResult DBusConnectionEstablish(GBusType type);

/**
 *
 * @param connection
 * @return
 */
EnumDBusResult DBusGetConnection(GDBusConnection **connection);

/**
 *
 * @param objects
 * @return
 */
EnumDBusResult DBusGetManagedObjects(GVariant **objects);

/**
 * Function to extract the object path of the default Bluetooth Adapter.
 * @param adapter_obj_path String where we store the Object Path. Needs to be freed by user.
 * @return 0 in case of success, > 0 otherwise. Error can be identified with EnumDBusResult codes.
 */
/* TODO use an unique_ptr<> for the adapterPath, as the pointer is allocated inside the function. This eases the deallocation after using the function */
EnumDBusResult DBusAdapterGetObjectPath(const char* adapter, char **adapterPath);

/**
 * Set a filter for discovering Bluetooth devices.
 * @param adapter_path Object Path of the Default Bluetooth Adapter. Can be NULL.
 * @return 0 in case of success, > 0 otherwise. Error can be identified with EnumDBusResult codes.
 */
EnumDBusResult DBusAdapterSetDiscoveryFilter(const char *adapter_path, const GVariant &filter);

/**
 * This function stops a Bluetooth discovery process.
 * @param adapter_path Object Path of the Default Bluetooth Adapter. Can be NULL.
 * @return 0 in case of success, > 0 otherwise. Error can be identified with EnumDBusResult codes.
 */
EnumDBusResult DBusAdapterStartDiscovery(const char *adapter_path);

/**
 * This function stops a Bluetooth discovery process.
 * @param adapter_path Object Path of the Default Bluetooth Adapter. Can be NULL.
 * @return 0 in case of success, > 0 otherwise. Error can be identified with EnumDBusResult codes.
 */
EnumDBusResult DBusAdapterStopDiscovery(const char *adapter_path);

/**
 *
 * @param agent_path
 * @param capabilities
 * @return
 */
EnumDBusResult DBusAgentManagerRegister(const char *agent_path, const char *capabilities);

/**
 * Connect to a Bluetooth device. This function can also be used to pair a device.
 * @param devicePath object path (../hci0/dev_xx_xx_xx_xx_xx_xx) of the device
 * @return DBUS_OK if it was able to connect to the device, other error value in case of failure
 */
EnumDBusResult DBusDeviceConnect(const char *devicePath);

/**
 *
 * @param devicePath
 * @param uuid
 * @return
 */
EnumDBusResult DBusDeviceConnectProfile(const char *devicePath, const char *uuid);

/**
 *
 * @param devicePath
 * @return
 */
EnumDBusResult DBusDeviceDisconnect(const char *devicePath);

/**
 * This function allows us to find the Object Path of a Device with a given address.
 * @param device_address Address of the device we want to discover the object path.
 * @param adapter_path Object path of the Bluetooth Adapter used. If NULL, we will discover the default adapter.
 * @param device_obj_path String that will store the discovered Device object path.
 * @return
 */
EnumDBusResult DBusDeviceGetObjectPath(const char *device_address, const char *adapter_path, char **device_obj_path);

EnumDBusResult DBusDeviceGetProperty(const char *devicePath, const char *property, GVariant *value);

EnumDBusResult DBusDeviceAddressProperty(const char *devicePath, string *address);

EnumDBusResult DBusDeviceAppearanceProperty(const char *devicePath, uint16_t *appearance);

/**
 *
 * @param devicePath
 * @return
 */
EnumDBusResult DBusDeviceConnectedProperty(const char *devicePath, bool *connected);

EnumDBusResult DBusDeviceNameProperty(const char *devicePath, string *name);

EnumDBusResult DBusDevicePairedProperty(const char *devicePath, bool *paired);

EnumDBusResult DBusDeviceServicesResolvedProperty(const char *devicePath, bool *servicesResolved);

/**
 *
 * @param devicePath
 * @param uuids
 * @return
 */
EnumDBusResult DBusDeviceUuidsProperty(const char *devicePath, vector <string> *uuids);

/**
 *
 * @param devicePath
 * @return
 */
EnumDBusResult DBusDevicePair(const char *devicePath);

/**
 *
 * @param devicePath
 * @return
 */
EnumDBusResult DBusDeviceReadNotifications(const char *devicePath,
                                           const char *serviceHandle,
                                           const char *characteristicHandle,
                                           NotificationsCollection *notifications,
                                           unsigned int timeout = 60);

EnumDBusResult DBusDeviceStartNotify(const char *devicePath,
                                     const char *serviceHandle,
                                     const char *characteristicHandle,
                                     NotificationsCollection *notifications);

EnumDBusResult DBusDeviceStopNotify(const char *devicePath,
                                    const char *serviceHandle,
                                    const char *characteristicHandle);

/**
 *
 * @param devicePath
 * @param serviceHandle
 * @param characteristicId
 * @param value
 * @return
 */
EnumDBusResult DBusGattCharacteristicReadValue(const char *devicePath,
                                               const char *serviceHandle,
                                               const char *characteristicId,
                                               byte **value,
                                               string *uuid);

/**
 *
 * @param devicePath
 * @param serviceHandle
 * @param characteristicHandle
 * @param descriptorHandle
 * @param value
 * @param uuid
 * @return
 */
EnumDBusResult DBusGattCharacteristicDescriptorReadValue(const char *devicePath,
                                                         const char *serviceHandle,
                                                         const char *characteristicHandle,
                                                         const char *descriptorHandle,
                                                         byte **value,
                                                         string *uuid);

/**
 *
 * @param objectPath
 * @param interface
 * @param propertyName
 * @param value
 * @return
 */
EnumDBusResult DBusGetInterfaceProperty(const char *objectPath,
                                        const char *interface,
                                        const char *propertyName,
                                        GVariant **value);

/**
 *
 */
EnumDBusResult DBusLookupDeviceProperties(GVariant *objects, BluetoothDeviceProperties *device);

/**
 * Gets the object path of a device from a GVariant structure, based on a Bluetooth address.
 * @param objects
 * @param address Bluetooth address in the format XX:XX:XX:XX:XX:XX
 * @param devicePath the device path, nullptr if the path was not found
 */
EnumDBusResult DBusLookupDevicePath(GVariant *objects, const char *address, char **devicePath);


void PairAgentMethodCall(GDBusConnection *connection,
                         const gchar *sender,
                         const gchar *object_path,
                         const gchar *interface_name,
                         const gchar *method_name,
                         GVariant *parameters,
                         GDBusMethodInvocation *invocation,
                         gpointer user_data);

void ReadAgentIntrospectionXmlData(char **introspectionData);

#endif	/* DBUS_HELPER_H */
