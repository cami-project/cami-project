cmake_minimum_required(VERSION 3.5)
project(adcamid)
set(CMAKE_PROJECT_NAME "adcamid")
set(MAJOR_VERSION "0")
set(MINOR_VERSION "1")
set(PATCH_VERSION "1")

#set(CMAKE_VERBOSE_MAKEFILE on)
#set(CMAKE_BUILD_TYPE Debug)
set(CMAKE_CXX_COMPILER c++)
set(CMAKE_CXX_STANDARD 11)

set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS} -Wall -O0 -g -DDEBUG -DNDEBUG -DSQLITE_ENABLE_JSON1 \
-D__FILENAME__='\"$(subst ${CMAKE_SOURCE_DIR}/,,$(basename $(abspath $<)))\"'")
set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS} -Wall -O3 -DNDEBUG -DVERBOSE -DSQLITE_ENABLE_JSON1 \
-D__FILENAME__='\"$(subst ${CMAKE_SOURCE_DIR}/,,$(basename $(abspath $<)))\"'")
set(CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/build")

# Search for libmicrohttpd
find_package(MHD)
if (MHD_FOUND)
    include_directories(${MHD_INCLUDE_DIR})
    set(requiredlibs ${requiredlibs} ${MHD_LIBRARIES})
else (MHD_FOUND)
    message(FATAL_ERROR "Could not find the LIBMICROHTTPD library and development files.")
endif (MHD_FOUND)

# Search for libcurl
find_package(CURL)
if (CURL_FOUND)
    include_directories(${CURL_INCLUDE_DIR})
    set(requiredlibs ${requiredlibs} ${CURL_LIBRARIES})
else (CURL_FOUND)
    message(FATAL_ERROR "Could not find the CURL library and development files.")
endif (CURL_FOUND)

find_package(DBus)
if (DBUS_FOUND)
    include_directories(${DBUS_INCLUDE_DIRS})
    set(requiredlibs ${requiredlibs} ${DBUS_LIBRARIES})
else (DBUS_FOUND)
    message(FATAL_ERROR "Could not find the DBUS library and development files.")
endif (DBUS_FOUND)

find_package(GLIB)
if (GLIB_FOUND)
    include_directories(${GLIB_INCLUDE_DIRS})
    set(requiredlibs ${requiredlibs} ${GLIB_LIBRARIES} ${GIO_LIBRARIES})
else (GLIB_FOUND)
    message(FATAL_ERROR "Could not find the GLIB library and development files.")
endif (GLIB_FOUND)

find_package(DBusGlib)
if (DBUS_GLIB_FOUND)
    include_directories(${DBUS_GLIB_INCLUDE_DIRS})
    set(requiredlibs ${requiredlibs} ${DBUS_GLIB_LIBRARIES})
else (DBUS_GLIB_FOUND)
    message(FATAL_ERROR "Could not find the DBUS-GLIB library and development files.")
endif (DBUS_GLIB_FOUND)

find_package(GTest)
if (GTEST_FOUND)
    include_directories(${GTEST_INCLUDE_DIRS})
    set(requiredlibs ${requiredlibs} ${GTEST_LIBRARIES})
else (GTEST_FOUND)
    message(FATAL_ERROR "Could not find the GTEST library and development files.")
endif (GTEST_FOUND)

include_directories("/usr/include")

set(INCLUDE_DIR include)
set(SOURCE_DIR src)
set(TEST_DIR test)

set(SOURCE_FILES
        # ###########################
        # Header files
        # ###########################
        ${INCLUDE_DIR}/AdCamiActionsServer.h
        ${INCLUDE_DIR}/AdCamiBluetooth5.h
        ${INCLUDE_DIR}/AdCamiBluetoothDefinitions.h
        ${INCLUDE_DIR}/AdCamiBluetoothDevice.h
        ${INCLUDE_DIR}/AdCamiCommon.h
        ${INCLUDE_DIR}/AdCamiConfiguration.h
        ${INCLUDE_DIR}/AdCamiEvent.h
        ${INCLUDE_DIR}/AdCamiEventBloodPressureMeasurement.h
        ${INCLUDE_DIR}/AdCamiEventFactory.h
        ${INCLUDE_DIR}/AdCamiEventsStorage.h
        ${INCLUDE_DIR}/AdCamiEventWeightMeasurement.h
        ${INCLUDE_DIR}/AdCamiHttpClient.h
        ${INCLUDE_DIR}/AdCamiHttpCommon.h
        ${INCLUDE_DIR}/AdCamiHttpData.h
        ${INCLUDE_DIR}/AdCamiHttpHeaders.h
        ${INCLUDE_DIR}/AdCamiHttpServer.h
        ${INCLUDE_DIR}/AdCamiJsonConverter.h
        ${INCLUDE_DIR}/AdCamiMeasurement.h
        ${INCLUDE_DIR}/AdCamiMonitor.h
        ${INCLUDE_DIR}/AdCamiUrl.h
        ${INCLUDE_DIR}/AdCamiUtilities.h
        ${INCLUDE_DIR}/IAdCamiBluetooth.h
        ${INCLUDE_DIR}/IAdCamiEventMeasurement.h
        ${INCLUDE_DIR}/IAdCamiHardwareFactory.h
        ${INCLUDE_DIR}/dbus/dbus_helper_5.h
        ${INCLUDE_DIR}/dbus/dbus_helper_defs.h
        ${INCLUDE_DIR}/sqlite/sqlite3.h
        # ###########################
        # Source files
        # ###########################
        ${SOURCE_DIR}/AdCamiBluetooth5.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiConfiguration.cpp
        ${SOURCE_DIR}/AdCamiEventsStorage.cpp
        ${SOURCE_DIR}/AdCamiHttpClient.cpp
        ${SOURCE_DIR}/AdCamiHttpCommon.cpp
        ${SOURCE_DIR}/AdCamiHttpData.cpp
        ${SOURCE_DIR}/AdCamiHttpHeaders.cpp
        ${SOURCE_DIR}/AdCamiHttpServer.cpp
        ${SOURCE_DIR}/AdCamiJsonConverter.cpp
        ${SOURCE_DIR}/AdCamiMonitor.cpp
        ${SOURCE_DIR}/AdCamiUrl.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp
        ${SOURCE_DIR}/sqlite/sqlite3.c)

include_directories(${INCLUDE_DIR})
add_executable(adcamid ${SOURCE_DIR}/AdCamiBleDaemon.cpp ${SOURCE_FILES})
target_link_libraries(adcamid
        curl dl json microhttpd pthread
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0)

# ###########################
# Bluetooth tests
# ###########################
add_executable(test-adproximity
        ${TEST_DIR}/TestAdCamiProximity.cpp
        ${SOURCE_DIR}/AdCamiBluetooth5.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-adproximity
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

add_executable(test-discovery
        ${TEST_DIR}/TestDiscovery.cpp
        ${SOURCE_DIR}/AdCamiBluetooth5.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-discovery
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

add_executable(test-pairdevice
        ${TEST_DIR}/TestPairDevice.cpp
        ${SOURCE_DIR}/AdCamiBluetooth5.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-pairdevice
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

add_executable(test-connectdevice
        ${TEST_DIR}/TestConnectDevice.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-connectdevice
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

add_executable(test-deviceinformation
        ${TEST_DIR}/TestDeviceInformation.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-deviceinformation
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

add_executable(test-deviceproperties
        ${TEST_DIR}/TestDeviceProperties.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-deviceproperties
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

add_executable(test-syncnotifications
        ${TEST_DIR}/TestSyncNotifications.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiJsonConverter.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-syncnotifications json
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

#add_executable(test-asyncnotifications
#        ${TEST_DIR}/TestAsyncNotifications.cpp
#        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
#        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
#        ${SOURCE_DIR}/AdCamiUtilities.cpp
#        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
#target_link_libraries(test-asyncnotifications
#        ${DBUS_LIBRARIES}
#        ${DBUS_GLIB_LIBRARIES}
#        ${GLIB_LIBRARIES}
#        -lglib-2.0 -lgobject-2.0 -lgio-2.0
#        pthread)

add_executable(test-asyncdiscovery
        ${TEST_DIR}/TestAsyncDiscovery.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetooth5.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-asyncdiscovery
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

add_executable(test-discoverreaddevice
        ${TEST_DIR}/TestDiscoverReadDevice.cpp
        ${SOURCE_DIR}/AdCamiBluetooth5.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDefinitions.cpp
        ${SOURCE_DIR}/AdCamiBluetoothDevice.cpp
        ${SOURCE_DIR}/AdCamiJsonConverter.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp
        ${SOURCE_DIR}/dbus/dbus_helper_5.cpp)
target_link_libraries(test-discoverreaddevice
        json
        ${DBUS_LIBRARIES}
        ${DBUS_GLIB_LIBRARIES}
        ${GLIB_LIBRARIES}
        -lglib-2.0 -lgobject-2.0 -lgio-2.0
        pthread)

# ###########################
# Unit Tests configuration
# ###########################
enable_testing()

add_executable(test_adcamiconfiguration
        ${TEST_DIR}/unit/TestAdCamiConfiguration.cpp
        ${SOURCE_DIR}/AdCamiConfiguration.cpp)
target_link_libraries(test_adcamiconfiguration GTest::GTest GTest::Main json)

add_executable(test_adcamihttpclient
        ${TEST_DIR}/unit/TestAdCamiHttpClient.cpp
        ${SOURCE_DIR}/AdCamiHttpClient.cpp
        ${SOURCE_DIR}/AdCamiHttpCommon.cpp
        ${SOURCE_DIR}/AdCamiHttpData.cpp
        ${SOURCE_DIR}/AdCamiHttpHeaders.cpp
        ${SOURCE_DIR}/AdCamiUrl.cpp
        ${SOURCE_DIR}/AdCamiUtilities.cpp)
target_link_libraries(test_adcamihttpclient GTest::GTest GTest::Main curl)

#add_executable(test_adcamimeasurements
#        ${TEST_DIR}/unit/TestAdCamiMeasurements.cpp)
#target_link_libraries(test_adcamimeasurements GTest::GTest GTest::Main)
#
#add_test(AllTests test_adcamiconfiguration test_adcamimeasurements)
add_test(AllTests test_adcamiconfiguration test_adcamihttpclient)

# ###########################
# Debian package generator
# ###########################
install(TARGETS adcamid DESTINATION /usr/sbin)
install(FILES scripts/adcamid
        DESTINATION /etc/init.d
        PERMISSIONS OWNER_READ OWNER_WRITE OWNER_EXECUTE GROUP_READ GROUP_EXECUTE WORLD_READ WORLD_EXECUTE)
install(FILES scripts/adcamid.service DESTINATION /lib/systemd/system)
install(DIRECTORY build/${CMAKE_BUILD_TYPE}/etc_adcamid_test/ DESTINATION /etc/adcamid)

set(CPACK_GENERATOR "DEB")
set(CPACK_PACKAGE_DESCRIPTION "AdCAMI daemon")
set(CPACK_PACKAGE_DESCRIPTION_SUMMARY
        "Daemon for communication with A&D Bluetooth medical devices")
set(CPACK_DEBIAN_PACKAGE_MAINTAINER "Jorge Miranda")
set(CPACK_PACKAGE_CONTACT "jmiranda@aliviate.dk")
set(CPACK_PACKAGE_VENDOR "Aliviate")
set(CPACK_PACKAGE_VERSION_MAJOR "${MAJOR_VERSION}")
set(CPACK_PACKAGE_VERSION_MINOR "${MINOR_VERSION}")
set(CPACK_PACKAGE_VERSION_PATCH "${PATCH_VERSION}")
set(CPACK_DEBIAN_ARCHITECTURE ${CMAKE_SYSTEM_PROCESSOR})
if (CMAKE_BUILD_TYPE STREQUAL "Release")
    set(CPACK_PACKAGE_FILE_NAME
            "${CMAKE_PROJECT_NAME}_${MAJOR_VERSION}.${MINOR_VERSION}.${CPACK_PACKAGE_VERSION_PATCH}_${CPACK_DEBIAN_ARCHITECTURE}")
    set(CPACK_DEBIAN_PACKAGE_SECTION "network")
else (CMAKE_BUILD_TYPE STREQUAL "Release")
    set(CPACK_PACKAGE_FILE_NAME
            "${CMAKE_PROJECT_NAME}-dbg_${MAJOR_VERSION}.${MINOR_VERSION}.${CPACK_PACKAGE_VERSION_PATCH}_${CPACK_DEBIAN_ARCHITECTURE}")
    set(CPACK_DEBIAN_PACKAGE_SECTION "debug")
endif (CMAKE_BUILD_TYPE STREQUAL "Release")
set(CPACK_DEBIAN_PACKAGE_DEPENDS "libcurl3, libcurl3-gnutls, libmicrohttpd10, libglib2.0-0, bluez (>= 5.43)")
set(CPACK_DEBIAN_PACKAGE_CONTROL_EXTRA "${CMAKE_CURRENT_SOURCE_DIR}/scripts/postinst")
include(CPack)
