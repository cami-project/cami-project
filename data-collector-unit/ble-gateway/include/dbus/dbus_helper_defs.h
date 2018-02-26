//
//  Created by Jorge Miranda on 07/12/2016
//

#ifndef DBUS_HELPER_DEFS_H
#define DBUS_HELPER_DEFS_H

#ifdef    __cplusplus
extern "C" {
#endif

#include <dbus/dbus.h>
#include <dbus/dbus-glib.h>
#include <dbus/dbus-glib-lowlevel.h>

#define BLUEZ_SERVICE                               "org.bluez"
#define BLUEZ_SERVICE_PATH                          "/org/bluez"
#define BLUEZ_MANAGER_PATH                          "/"

/* ************* DBUS ************* */
#define DBUS_OBJECTMANAGER_INTERFACE                "org.freedesktop.DBus.ObjectManager"
#define DBUS_PROPERTIES_INTERFACE                   "org.freedesktop.DBus.Properties"

#define DBUS_METHOD_GETMANAGEDOBJECTS               "GetManagedObjects"

#define DBUS_SIGNAL_INTERFACESADDED                 "InterfacesAdded"
#define DBUS_SIGNAL_PROPERTIESCHANGED               "PropertiesChanged"


/* ************* Agent ************* */
#define BLUEZ5_AGENT_INTERFACE                      "org.bluez.Agent1"

#define BLUEZ5_AGENT_METHOD_RELEASE                 "Release"
#define BLUEZ5_AGENT_METHOD_REQUESTPINCODE          "RequestPinCode"
#define BLUEZ5_AGENT_METHOD_DISPLAYPINCODE          "DisplayPinCode"
#define BLUEZ5_AGENT_METHOD_REQUESTPASSKEY          "RequestPasskey"
#define BLUEZ5_AGENT_METHOD_DISPLAYPASSKEY          "DisplayPasskey"
#define BLUEZ5_AGENT_METHOD_REQUESTCONFIRMATION     "RequestConfirmation"
#define BLUEZ5_AGENT_METHOD_AUTHORIZESERVICE        "AuthorizeService"
#define BLUEZ5_AGENT_METHOD_CANCEL                  "Cancel"


/* ************* Agent Manager ************* */
#define BLUEZ5_AGENTMANAGER_INTERFACE               "org.bluez.AgentManager1"

#define BLUEZ5_AGENTMANAGER_METHOD_REGISTERAGENT    "RegisterAgent"

#define BLUEZ5_AGENTMANAGER_CAPABILITIES_DISPLAYONLY       "DisplayOnly"
#define BLUEZ5_AGENTMANAGER_CAPABILITIES_DISPLAYYESNO      "DisplayYesNo"
#define BLUEZ5_AGENTMANAGER_CAPABILITIES_KEYBOARDONLY      "KeyboardOnly"
#define BLUEZ5_AGENTMANAGER_CAPABILITIES_NOIO              "NoInputNoOutput"
#define BLUEZ5_AGENTMANAGER_CAPABILITIES_KEYBOARDDISPLAY   "KeyboardDisplay"


/* ************* Adapter ************* */
#define BLUEZ5_ADAPTER_INTERFACE                    "org.bluez.Adapter1"

#define BLUEZ5_ADAPTER_METHOD_SETDISCOVERYFILTER    "SetDiscoveryFilter"
#define BLUEZ5_ADAPTER_METHOD_STARTDISCOVERY        "StartDiscovery"
#define BLUEZ5_ADAPTER_METHOD_STOPDISCOVERY         "StopDiscovery"


/* ************* Device ************* */
#define BLUEZ5_DEVICE_INTERFACE                     "org.bluez.Device1"

#define BLUEZ5_DEVICE_METHOD_CONNECT                "Connect"
#define BLUEZ5_DEVICE_METHOD_CONNECTPROFILE         "ConnectProfile"
#define BLUEZ5_DEVICE_METHOD_DISCONNECT             "Disconnect"
#define BLUEZ5_DEVICE_METHOD_PAIR                   "Pair"

#define BLUEZ5_DEVICE_PROPERTY_ADDRESS              "Address"
#define BLUEZ5_DEVICE_PROPERTY_APPEARANCE           "Appearance"
#define BLUEZ5_DEVICE_PROPERTY_CONNECTED            "Connected"
#define BLUEZ5_DEVICE_PROPERTY_NAME                 "Name"
#define BLUEZ5_DEVICE_PROPERTY_PAIRED               "Paired"
#define BLUEZ5_DEVICE_PROPERTY_RSSI                 "RSSI"
#define BLUEZ5_DEVICE_PROPERTY_SERVICESRESOLVED     "ServicesResolved"
#define BLUEZ5_DEVICE_PROPERTY_UUIDS                "UUIDs"


/* ************* GATT ************* */
#define BLUEZ5_GATTCHARACTERISTIC_INTERFACE        "org.bluez.GattCharacteristic1"
#define BLUEZ5_GATTSERVICE_INTERFACE               "org.bluez.GattService1"
#define BLUEZ5_GATTCHARACTERISTICDESCRIPTOR_INTERFACE   "org.bluez.GattDescriptor1"

#define BLUEZ5_GATTCHARACTERISTIC_METHOD_READVALUE      "ReadValue"
#define BLUEZ5_GATTCHARACTERISTIC_METHOD_STARTNOTIFY    "StartNotify"
#define BLUEZ5_GATTCHARACTERISTIC_METHOD_STOPNOTIFY     "StopNotify"
#define BLUEZ5_GATTCHARACTERISTIC_METHOD_WRITEVALUE     "WriteValue"
#define BLUEZ5_GATTCHARACTERISTIC_PROPERTY_UUID         "UUID"

#define BLUEZ5_GATTCHARACTERISTICDESCRIPTOR_METHOD_READVALUE    "ReadValue"
#define BLUEZ5_GATTCHARACTERISTICDESCRIPTOR_METHOD_WRITEVALUE   "WriteValue"
#define BLUEZ5_GATTCHARACTERISTICDESCRIPTOR_PROPERTY_CHARACTERISTIC "Characteristic"
#define BLUEZ5_GATTCHARACTERISTICDESCRIPTOR_PROPERTY_FLAGS      "Flags"
#define BLUEZ5_GATTCHARACTERISTICDESCRIPTOR_PROPERTY_UUID       "UUID"
#define BLUEZ5_GATTCHARACTERISTICDESCRIPTOR_PROPERTY_VALUE      "Value"

typedef enum _dbus_ret {
    DBUS_OK = 0,
    DBUS_ERROR_CONNECTION = -1,
    DBUS_ERROR_UNKNOWN = -2,
    DBUS_ERROR_INVALID_PARAMETERS = -3,
    DBUS_ERROR_PROXY_CREATION = -4,
    DBUS_ERROR_PROXY_METHOD_CALL = -5,
    DBUS_ERROR_NO_ADAPTER = -6,
    DBUS_ERROR_NO_DEVICE_FOUND = -7,
    DBUS_ERROR_PROPERTIES_LOOKUP = -8,
    DBUS_ERROR_NOT_PAIRED = -9,
    DBUS_ERROR_UNEXPECTED_MESSAGE = -10,
    DBUS_ERROR_NO_MANAGED_OBJECTS = -11,
    DBUS_ERROR_REGISTER_AGENT = -12,
    DBUS_ERROR_NO_AGENT_XML = -13,
    DBUS_ERROR_CONNECTING_DEVICE = -14,
    DBUS_ERROR_READING_VALUE = -15,
    DBUS_ERROR_READING_PROPERTY = -16,
    DBUS_ERROR_NOTIFICATION = -17,
    DBUS_ERROR_PROPERTY_NOT_FOUND = -18
} EnumDBusResult;

#ifdef    __cplusplus
}
#endif

#endif	/* DBUS_HELPER_DEFS_H */

