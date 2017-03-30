insert into "Device"("DeviceTypeId","Address","Paired","NotificationsEnabled","Deleted")
select 6160 as "DeviceTypeId", "ab:cd:ef:01:23:45" as "Address", 0 as "Paired", 0 as "NotificationsEnabled", 0 as "Deleted"
union all select 6173, "01:23:45:67:89:ab", 1, 0, 0
union all select 6160, "98:76:54:32:10:ab", 1, 0, 0
union all select 6173, "fe:dc:ab:01:23:45", 1, 0, 1;


insert into "Event"("EventTypeId","DeviceId","TimeStamp")
select 1 as "EventTypeId", 1 as "DeviceId", "2016-11-24 17:30:55.100" as "TimeStamp"
union all select 1, 2, "2016-11-24 17:32:35.200"
union all select 2, 1, "2016-11-24 17:40:01.300"
union all select 1, 3, "2016-11-25 10:15:09.100"
union all select 2, 2, "2016-11-25 10:16:36.120"
union all select 1, 4, "2016-11-25 14:25:19.100"
union all select 1, 1, "2016-11-25 17:09:20.150"
union all select 2, 3, "2016-11-26 16:49:30.250"
union all select 2, 4, "2016-11-26 16:52:30.250";


insert into "EventBloodPressureMeasurement"("EventId",
    "SystolicValue", "SystolicUnit",
    "DiastolicValue", "DiastolicUnit",
    "MeanArterialPressureValue", "MeanArterialPressureUnit",
    "PulseRateValue"
)
select 3 as "EventId",
    120.0 as "SystolicValue", "mmHg" as "SystolicUnit",
    80.0 as "DiastolicValue", "mmHg" as "DiastolicUnit",
    93.33 as "MeanArterialPressureValue", "mmHg" as "MeanArterialPressureUnit",
    64 as "PulseRateValue"
union all select 8, 115.0, "mmHg", 87.0, "mmHg", 96.33, "mmHg", 70;


insert into "EventWeightMeasurement"("EventId","Value","Unit")
select 5 as "EventId", 75.5 as "Value", "Kg" as "Unit"
union all select 9, 75.8, "Kg";
