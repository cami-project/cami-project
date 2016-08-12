DROP DATABASE IF EXISTS frontend;

CREATE DATABASE frontend;

GRANT ALL ON frontend.* TO 'cami'@'%' IDENTIFIED BY 'cami';
