DROP DATABASE IF EXISTS cami;

CREATE DATABASE cami CHARACTER SET utf8;

USE cami;

CREATE TABLE physical_exercise_logs (
  id INT(10),
  exercise_id INT(10)
);
