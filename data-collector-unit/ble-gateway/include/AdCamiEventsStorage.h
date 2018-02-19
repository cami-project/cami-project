//
//  Created by Jorge Miguel Miranda on 2/12/16.
//

#ifndef AdCamiDaemon_AdCamiEventsStorage_h
#define AdCamiDaemon_AdCamiEventsStorage_h

#include "sqlite/sqlite3.h"
#include <iostream>
#include <string>
#include <vector>
#include "AdCamiBluetoothDevice.h"
#include "AdCamiEventBloodPressureMeasurement.h"
#include "AdCamiEventWeightMeasurement.h"
#include "AdCamiUtilities.h"

using AdCamiData::AdCamiEventBloodPressureMeasurement;
using AdCamiHardware::AdCamiBluetoothDevice;
using std::string;
using std::vector;
using EnumEventType = AdCamiData::AdCamiEvent::EnumEventType;

namespace AdCamiData {

class AdCamiEventsStorage {
public:
    enum EnumStorageError : int {
        RecordAlreadyExists = -19,
        RecordNotFound = -18,
        DatabaseFileNotFound = -10,
        DeviceNotFound = -5,
        DeviceNotInserted = -4,
        EventNotFound = -3,
        EventNotInserted = -2,
        Error = -1,
        Ok = 0,
//        DeviceInserted = 1,
//        EventInserted = 2,
        Open = 10,
        Closed = 11
    };

    enum EnumDeviceFilter : int {
        All = 0x0,
        Paired = 0x1,
        Unpaired = 0x2,
        NotificationsEnabled = 0x4,
        NotificationsDisabled = 0x8
    };

    AdCamiEventsStorage(const string &dbPath);

    AdCamiEventsStorage(const char *dbPath);

    virtual ~AdCamiEventsStorage();

    AdCamiEventsStorage::EnumStorageError AddDevice(const AdCamiBluetoothDevice &device);

    AdCamiEventsStorage::EnumStorageError AddDevice(const vector <AdCamiBluetoothDevice> &devices);

    AdCamiEventsStorage::EnumStorageError AddEvent(const AdCamiEvent &event);

    AdCamiEventsStorage::EnumStorageError AddEvent(const AdCamiEvent *event);

    //TODO pass as reference
    AdCamiEventsStorage::EnumStorageError AddEvent(const vector <AdCamiEvent> events);

    //TODO pass as reference
    AdCamiEventsStorage::EnumStorageError AddEvent(const vector<AdCamiEvent *> events);

    AdCamiEventsStorage::EnumStorageError DeleteDevice(const AdCamiBluetoothDevice &device);

    AdCamiEventsStorage::EnumStorageError DeleteDevice(const vector <AdCamiBluetoothDevice> &devices);

    /**
     * Get a device from the database.
     * @param address the address fo the device to retrieve
     * @param device a pointer to a device object where the result will be stored. If this is a null pointer,
     *  nothing is stored, even if the device exists.
     * @return Ok if the device exists; DeviceNotFound if the device doesn't exist; other error otherwise
     */
    AdCamiEventsStorage::EnumStorageError GetDevice(const string &address,
                                                    AdCamiBluetoothDevice *device);

    /**
     * Get a device from the database. If the device exists, the device given as argument is updated with the
     * properties on the database.
     * @param device a pointer to a device object where the result will be stored. If this is a null pointer,
     *  nothing is stored, even if the device exists.
     * @return Ok if the device exists; DeviceNotFound if the device doesn't exist; other error otherwise
     */
    AdCamiEventsStorage::EnumStorageError GetDevice(AdCamiBluetoothDevice *device);

    /**
     * Get a list of device from the database that respect the criteria defined on filter.
     * @param devices a pointer to a list where the devices will be stored. If this is a null pointer,
     * @param filter criteria to filter the devices to search. By default it searches all devices
     * @return Ok if the query executes successfully; other error otherwise
     */
    AdCamiEventsStorage::EnumStorageError GetDevices(vector <AdCamiBluetoothDevice> *devices,
                                                     const EnumDeviceFilter &filter = All);

    /**
     * Gets that last event from the database, for a specific device .
     * @param device an object representing the device whose last event will be retrieved
     * @param event a pointer where the last event will be stored. If this is a null pointer, nothing is stored,
     *  even if the event exists.
     * @return Ok if the event exists; EventNotFound if the device doesn't exist; other error otherwise
     */
    AdCamiEventsStorage::EnumStorageError GetLastEvent(const AdCamiBluetoothDevice &device, AdCamiEvent *event);

    AdCamiEventsStorage::EnumStorageError GetEvents(vector <AdCamiEvent> *events);

    AdCamiEventsStorage::EnumStorageError GetEvents(vector <AdCamiEventBloodPressureMeasurement> *events);

    AdCamiEventsStorage::EnumStorageError GetEvents(vector <AdCamiEventWeightMeasurement> *events);

    AdCamiEventsStorage::EnumStorageError UpdateDevice(const AdCamiBluetoothDevice &device);

private:
    /* Definition of the type used for SQLite's query callback. */
    typedef int (*_QueryClbk)(void *arg, int argc, char **argv, char **colname);

    sqlite3 *_db;
    char *_dbPath;
    AdCamiEventsStorage::EnumStorageError _dbState;

    /**
     */
    static int _ClbkGetDeviceEvents(void *arg, int argc, char **argv, char **colname);

    static int _ClbkGetDevice(void *arg, int argc, char **argv, char **colname);

    static int _ClbkGetDeviceEvent(void *arg, int argc, char **argv, char **colname);

    static int _ClbkGetDevices(void *arg, int argc, char **argv, char **colname);

    static int _ClbkGetBloodPressureEvents(void *arg, int argc, char **argv, char **colname);

    static int _ClbkGetWeightEvents(void *arg, int argc, char **argv, char **colname);

    /**
     */
    AdCamiEventsStorage::EnumStorageError _Open();

    inline bool _RecordExists(const string &query) {
        struct sqlite3_stmt *stmt;
        int res;

        if (sqlite3_prepare_v2(this->_db, query.c_str(), -1, &stmt, nullptr) == SQLITE_OK) {
            res = sqlite3_step(stmt);
        }
        sqlite3_finalize(stmt);

        return res == SQLITE_ROW;
    }

    /**
     */
    inline AdCamiEventsStorage::EnumStorageError
    _ExecuteQuery(const string &query, _QueryClbk clbk, void *clbkArgument) {
        char *error = nullptr;

        this->_Open();

        int res = sqlite3_exec(this->_db, query.c_str(), clbk, clbkArgument, &error);

        if (res != SQLITE_OK) {
            int errorCode = sqlite3_errcode(this->_db);
            PRINT_DEBUG("Database error: " << error << " [errorCode " << errorCode << "]")
            sqlite3_free(error);
            switch (errorCode) {
                /* On an INSERT query, the user already exists on the table. */
                case SQLITE_CONSTRAINT: {
                    return EnumStorageError::RecordAlreadyExists;
                }
                default: {
                    return EnumStorageError::RecordNotFound;
                }
            }
        }

        return EnumStorageError::Ok;
    }
};

} //namespace
#endif
