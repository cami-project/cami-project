#!/usr/bin/env bash

# change to project directory to use the classpath
cd /cami-smart-scheduling

runtimeOptions="-Xms256m -Xmx1536m -Dorg.optaplanner.examples.dataDir=data/"
myClasspath="target/dependency/*:target/classes"
mainClass=org.aimas.cami.scheduler.CAMIScheduler.server.Server

java ${runtimeOptions} -classpath ${myClasspath} ${mainClass} 0.0.0.0 8080