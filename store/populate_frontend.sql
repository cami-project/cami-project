# This will be used to populate the database with initial data after migrations
# have run.

USE frontend;

INSERT INTO notification (type, severity, message, created) VALUES('blood_pressure', 'high', 'You are close to a stroke!', NOW());
INSERT INTO notification (type, severity, message, created) VALUES('blood_pressure', 'medium', 'You should be fine soon!', NOW());
INSERT INTO notification (type, severity, message, created) VALUES('weight', 'low', 'You are alright!', NOW());
