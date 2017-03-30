//
//  Created by Jorge Miguel Miranda on 2/12/16.
//

#include "AdCamiEventsStorage.h"
#include <cstdlib>
#include <cstring>

namespace AdCamiData {

AdCamiEventsStorage::AdCamiEventsStorage(const string &dbPath) : _db(nullptr), _dbState(EnumStorageError::Closed) {
    size_t size = dbPath.size();
    this->_dbPath = new char[size];
    memcpy(this->_dbPath, dbPath.c_str(), size);
    this->_dbPath[size] = '\0';
}

AdCamiEventsStorage::AdCamiEventsStorage(const char *dbPath) : _db(nullptr), _dbState(EnumStorageError::Closed) {
    size_t size = strlen(dbPath);
    this->_dbPath = new char[size + 1];
    memcpy(this->_dbPath, dbPath, size);
    this->_dbPath[size] = '\0';
}

AdCamiEventsStorage::~AdCamiEventsStorage() {
    sqlite3_close(this->_db);
    delete[] this->_dbPath;
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::AddDevice(const AdCamiBluetoothDevice &device) {
    string query = "INSERT OR IGNORE INTO Device(DeviceTypeId,Address,Paired,NotificationsEnabled,Deleted) VALUES";
    query.append("(" + std::to_string(device.Type()) +
                 ",'" + device.Address() +
                 "'," + (device.PairedFromCache() ? "1" : "0") +
                 "," + (device.NotificationsEnabled() ? "1" : "0") +
                 ",0)"
    );
    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, nullptr, nullptr);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::AddDevice(const vector <AdCamiBluetoothDevice> &devices) {
    string query = "INSERT OR IGNORE INTO Device(DeviceTypeId,Address,Paired,NotificationsEnabled,Deleted)";

    switch (devices.size()) {
        case 0:
            return EnumStorageError::DeviceNotInserted;
        case 1:
            return this->AddDevice(devices.front());
        default:
            query.append(" SELECT " + std::to_string(devices.front().Type()) + " AS DeviceTypeId, '" +
                         devices.front().Address() + "' as 'Address', " +
                         (devices.front().PairedFromCache() ? "1" : "0") + " as 'Paired', " +
                         (devices.front().NotificationsEnabled() ? "1" : "0") + " as 'NotificationsEnabled', " +
                         "0"
            );
            for (auto it = devices.begin() + 1; it != devices.end(); ++it) {
                query.append(" UNION ALL SELECT " +
                             std::to_string(it->Type()) + ", '" +
                             it->Address() + "', " +
                             (it->PairedFromCache() ? "1," : "0,") +
                             (it->NotificationsEnabled() ? "1," : "0,") +
                             "0"
                );
            }
    }

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, nullptr, nullptr);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::AddEvent(const AdCamiEvent *event) {
    string query = "INSERT INTO Event(EventTypeId, DeviceId, TimeStamp) VALUES";
    query.append("(" + std::to_string(event->Type()) +
                 ",(SELECT Id FROM Device WHERE Address = '" + event->Address() +
                 "'),'" + event->TimeStamp() +
                 "')");
    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    if (_ExecuteQuery(query, nullptr, nullptr) != EnumStorageError::Ok) {
        return EnumStorageError::EventNotInserted;
    }

    unsigned int lastRowId = sqlite3_last_insert_rowid(this->_db);

    switch (event->Type()) {
        case EnumEventType::BloodPressure: {
            const AdCamiEventBloodPressureMeasurement *measurement =
                    dynamic_cast<const AdCamiEventBloodPressureMeasurement *>(event);
            query = "INSERT INTO EventBloodPressureMeasurement(EventId,SystolicValue,SystolicUnit,DiastolicValue,"
                    "DiastolicUnit,MeanArterialPressureValue,MeanArterialPressureUnit,PulseRateValue) VALUES";
            query.append("(" + std::to_string(lastRowId) +
                         "," + std::to_string(measurement->Systolic().Value()) +
                         ",'" + measurement->Systolic().Unit() +
                         "'," + std::to_string(measurement->Diastolic().Value()) +
                         ",'" + measurement->Diastolic().Unit() +
                         "'," + std::to_string(measurement->MeanArterialPressure().Value()) +
                         ",'" + measurement->MeanArterialPressure().Unit() +
                         "'," + std::to_string(measurement->PulseRate().Value()) +
                         ")");
            if (_ExecuteQuery(query, nullptr, nullptr) != EnumStorageError::Ok) {
                return EnumStorageError::EventNotInserted;
            }
            break;
        }
        case EnumEventType::Weight: {
            const AdCamiEventWeightMeasurement *measurement = dynamic_cast<const AdCamiEventWeightMeasurement *>(event);
            query = "INSERT INTO EventWeightMeasurement(EventId,Value,Unit) VALUES";
            query.append("(" + std::to_string(lastRowId) +
                         "," + std::to_string(measurement->Weight().Value()) +
                         ",'" + measurement->Weight().Unit() +
                         "')");
            if (_ExecuteQuery(query, nullptr, nullptr) != EnumStorageError::Ok) {
                return EnumStorageError::EventNotInserted;
            }
            break;
        }
        default: {
            break;
        }
    }

    return EnumStorageError::Ok;
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::AddEvent(const vector <AdCamiEvent> events) {
    string query = "INSERT OR IGNORE INTO Event(EventTypeId, DeviceId, TimeStamp)";

    switch (events.size()) {
        case 0:
            return EnumStorageError::EventNotInserted;
        case 1:
            this->AddEvent(&events.front());
        default:
            query.append(" SELECT " + std::to_string(events.front().Type()) + " AS EventTypeId, '" +
                         "(SELECT Id FROM Device WHERE Address is " + events.front().Address() + ") as 'DeviceId', " +
                         events.front().TimeStamp() + " as 'TimeStamp' ");
            for (auto it = events.begin() + 1; it != events.end(); ++it) {
                query.append(" UNION ALL SELECT " +
                             std::to_string(static_cast<unsigned int>(it->Type())) + ", " +
                             "(SELECT Id FROM Device WHERE Address is " + it->Address() + "), '" +
                             it->TimeStamp() + "'"
                );
            }
    }

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, nullptr, nullptr);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::AddEvent(const vector<AdCamiEvent *> events) {
    EnumStorageError error;

    if (events.size() == 0) {
        return EnumStorageError::EventNotInserted;
    } else {
        for (auto it = events.begin(); it != events.end(); ++it) {
            if ((error = this->AddEvent(*it)) != EnumStorageError::Ok) {
                return error;
            }
        }
    }

    return EnumStorageError::Ok;
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::DeleteDevice(const AdCamiBluetoothDevice &device) {
    string query = "UPDATE Device SET Deleted=1 WHERE Address is '" + device.Address() + "'";

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, nullptr, nullptr);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::DeleteDevice(const vector <AdCamiBluetoothDevice> &devices) {
    string query = "UPDATE Device SET Deleted=1 WHERE Address in (";

    switch (devices.size()) {
        case 0:
            return EnumStorageError::DeviceNotInserted;
        case 1:
            return this->DeleteDevice(devices.front());
        default:
            for (auto it = devices.begin(); it != devices.end(); ++it) {
                query.append("'" + it->Address() + (it != devices.end() - 1 ? "'," : "'"));
            }
            query.append(")");
    }

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, nullptr, nullptr);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::GetDevice(const string &address,
                                                                     AdCamiBluetoothDevice *device) {
    string query = "SELECT * FROM Device WHERE Address is '" + address + "'";

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    if (!_RecordExists(query)) {
        return DeviceNotFound;
    }

    return _ExecuteQuery(query, _ClbkGetDevice, device);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::GetDevices(vector <AdCamiBluetoothDevice> *devices,
                                                                      const EnumDeviceFilter &filter) {
    string query = "SELECT * FROM Device";

    if (filter != All) {
        query.append(" WHERE Deleted=0 AND ");

        switch (filter) {
            case Paired | NotificationsEnabled: {
                query.append("Paired=1 AND NotificationsEnabled=1");
                break;
            }
            case Paired:
            case Unpaired: {
                query.append(string("Paired=") + (filter == Paired ? "1" : "0"));
                break;
            }
            case NotificationsEnabled:
            case NotificationsDisabled: {
                query.append(string("NotificationsEnabled=") + (filter == NotificationsEnabled ? "1" : "0"));
                break;
            }
            default:
                break;
        }
    }

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, _ClbkGetDevices, devices);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::GetEvents(vector <AdCamiEvent> *events) {
    string query = "SELECT * FROM EventDeviceView";

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, _ClbkGetDeviceEvents, events);
}

AdCamiEventsStorage::EnumStorageError
AdCamiEventsStorage::GetEvents(vector <AdCamiEventBloodPressureMeasurement> *events) {
    string query = "SELECT * FROM EventBloodPressureMeasurementView";

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, _ClbkGetBloodPressureEvents, events);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::GetEvents(vector <AdCamiEventWeightMeasurement> *events) {
    string query = "SELECT * FROM EventWeightMeasurementView";

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, _ClbkGetWeightEvents, events);
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::UpdateDevice(const AdCamiBluetoothDevice &device) {
    string query = "UPDATE Device SET ";
    query.append("DeviceTypeId=" + std::to_string(static_cast<unsigned int>(device.Type())) +
                 ",Paired=" + (device.Paired() ? "1" : "0") +
                 ",NotificationsEnabled=" + (device.NotificationsEnabled() ? "1" : "0"));
    query.append(" WHERE Address is '" + device.Address() + "'");

    if (_Open() != EnumStorageError::Open) {
        return this->_dbState;
    }

    return _ExecuteQuery(query, nullptr, nullptr);
}

int AdCamiEventsStorage::_ClbkGetDeviceEvents(void *arg, int argc, char **argv, char **colname) {
    vector <AdCamiEvent> *events = reinterpret_cast<vector <AdCamiEvent> *>(arg);

    events->push_back(AdCamiEvent(
            static_cast<EnumEventType>(std::atoi(argv[0])), //type
            string(argv[1]), //timestamp
            string(argv[2]) //address
    ));

    return 0;
}

int AdCamiEventsStorage::_ClbkGetDevice(void *arg, int argc, char **argv, char **colname) {
    AdCamiBluetoothDevice *device = reinterpret_cast<AdCamiBluetoothDevice *>(arg);

    device->Address(string(argv[2]))
            .Type(static_cast<EnumBluetoothDeviceType>(std::strtoul(argv[1], nullptr, 10)))
            .Paired(string(argv[3]).compare("1") == 0)
            .NotificationsEnabled(string(argv[4]).compare("1") == 0);

    return 0;
}

int AdCamiEventsStorage::_ClbkGetDevices(void *arg, int argc, char **argv, char **colname) {
    vector <AdCamiBluetoothDevice> *devices =
            reinterpret_cast<vector <AdCamiBluetoothDevice> *>(arg);

    AdCamiBluetoothDevice device(argv[2]);
    device.RefreshCacheProperties();
    device.Type(static_cast<EnumBluetoothDeviceType>(std::strtoul(argv[1], nullptr, 10)))
            .NotificationsEnabled(string(argv[4]).compare("1") == 0);

    devices->push_back(device);

    return 0;
}

int AdCamiEventsStorage::_ClbkGetBloodPressureEvents(void *arg, int argc, char **argv, char **colname) {
    vector <AdCamiEventBloodPressureMeasurement> *events =
            reinterpret_cast<vector <AdCamiEventBloodPressureMeasurement> *>(arg);

    events->push_back(AdCamiEventBloodPressureMeasurement(
            string(argv[1]), //timestamp
            string(argv[2]), //address
            std::atof(argv[3]), //systolic value
            string(argv[4]), //systolic unit
            std::atof(argv[5]), //diastolic value
            string(argv[6]), //diastolic unit
            std::atof(argv[7]), //mean arterial pressure value
            string(argv[8]), //mean arterial pressure unit
            std::atof(argv[9]) //pulse rate value
    ));

    return 0;
}

int AdCamiEventsStorage::_ClbkGetWeightEvents(void *arg, int argc, char **argv, char **colname) {
    vector <AdCamiData::AdCamiEventWeightMeasurement> *events =
            reinterpret_cast<vector <AdCamiData::AdCamiEventWeightMeasurement> *>(arg);

    events->push_back(AdCamiData::AdCamiEventWeightMeasurement(
            string(argv[1]), //timestamp
            string(argv[2]), //address
            std::atof(argv[3]), //weight value
            string(argv[4]) //weight unit
    ));

    return 0;
}

AdCamiEventsStorage::EnumStorageError AdCamiEventsStorage::_Open() {
    if (this->_dbState == EnumStorageError::Open) {
        return this->_dbState;
    }

    if (sqlite3_open(this->_dbPath, &this->_db)) {
        PRINT_ERROR("Can't open database" << sqlite3_errmsg(this->_db))
        sqlite3_close(this->_db);
        this->_dbState = EnumStorageError::Closed;
    } else {
        this->_dbState = EnumStorageError::Open;
    }

    return this->_dbState;
}

} //namespace