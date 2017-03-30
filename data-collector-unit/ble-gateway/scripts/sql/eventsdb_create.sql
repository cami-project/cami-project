BEGIN TRANSACTION;
CREATE TABLE "EventWeightMeasurement" (
	`EventId`	INTEGER NOT NULL,
	`Value`	REAL NOT NULL,
	`Unit`	TEXT NOT NULL,
	PRIMARY KEY(`EventId`),
	FOREIGN KEY(`EventId`) REFERENCES `Event`(`Id`)
);
CREATE TABLE "EventType" (
	`Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`Description`	TEXT NOT NULL
);
CREATE TABLE "EventBloodPressureMeasurement" (
	`EventId`	INTEGER NOT NULL,
	`SystolicValue`	REAL NOT NULL,
	`DiastolicValue`	REAL NOT NULL,
	`MeanArterialPressureValue`	REAL NOT NULL,
	`PulseRateValue`	REAL NOT NULL,
	`SystolicUnit`	TEXT NOT NULL,
	`DiastolicUnit`	TEXT NOT NULL,
	`MeanArterialPressureUnit`	TEXT NOT NULL,
	PRIMARY KEY(`EventId`),
	FOREIGN KEY(`EventId`) REFERENCES `Event`(`Id`)
);
CREATE TABLE "Event" (
	`Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`EventTypeId`	INTEGER NOT NULL,
	`DeviceId`  INTEGER NOT NULL,
	`TimeStamp`	TEXT NOT NULL,
	FOREIGN KEY(`DeviceId`) REFERENCES `Device`(`Id`),
	FOREIGN KEY(`EventTypeId`) REFERENCES `EventType`(`Id`)
);
CREATE TABLE "DeviceType" (
	`Id`	INTEGER NOT NULL PRIMARY KEY,
	`Description`	TEXT NOT NULL
);
CREATE TABLE `Device` (
	`Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`DeviceTypeId`	INTEGER NOT NULL,
	`Address`	TEXT NOT NULL UNIQUE,
	`Paired`	INTEGER NOT NULL,
	`NotificationsEnabled`	INTEGER NOT NULL,
	`Deleted`	INTEGER NOT NULL,
	FOREIGN KEY(`DeviceTypeId`) REFERENCES `DeviceType`(`Id`)
);
CREATE VIEW EventBloodPressureMeasurementView AS
SELECT EBP."EventId", E."TimeStamp", D."Address",
    EBP."SystolicValue", EBP."SystolicUnit",
    EBP."DiastolicValue", EBP."DiastolicUnit",
    EBP."MeanArterialPressureValue", EBP."MeanArterialPressureUnit", EBP."PulseRateValue"
FROM "EventBloodPressureMeasurement" as EBP
INNER JOIN "Event" AS E ON EBP."EventId" = E."Id"
INNER JOIN "Device" AS D ON E."DeviceId" = D."Id";

CREATE VIEW EventWeightMeasurementView AS
SELECT EW."EventId", E."TimeStamp", D."Address",
    EW."Value", EW."Unit"
FROM "EventWeightMeasurement" as EW
INNER JOIN "Event" AS E ON EW."EventId" = E."Id"
INNER JOIN "Device" AS D ON E."DeviceId" = D."Id";

CREATE VIEW EventDeviceView AS
SELECT E."Id", E."TimeStamp", D."Address"
FROM "Event" AS E
INNER JOIN "Device" AS D ON E."DeviceId" = D."Id"
WHERE E."EventTypeId" IN (SELECT "Id" FROM "EventType" WHERE "Description" like "Device");
COMMIT;

INSERT INTO main."EventType"("Description")
SELECT "Device" as "Description"
UNION ALL SELECT "BloodPressure"
UNION ALL SELECT "Weight";

INSERT INTO main."DeviceType"("Id","Description")
SELECT 65535 as "Id", "Unknown" as "Description"
UNION ALL SELECT 6160, "BloodPressure"
UNION ALL SELECT 6173, "WeightScale";
