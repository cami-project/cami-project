#!/bin/bash

CONTRIB_DIR=contrib
DEPENDENCIES=(libcurl3 libcurl3-gnutls libmicrohttpd10 libglib2.0-0)
BLUEZ_DEPENDENCIES=(
    g++-5 cmake autoconf libtool elfutils libelf-dev libdw-dev libudev-dev libical-dev
    libreadline-dev libsbc-dev libspeexdsp-dev libglib2.0-0 libglib2.0-dev dbus
    libdbus-1-3 libdbus-1-dev libdbus-glib-1-dev debhelper dh-autoreconf flex bison
    libcap-ng-dev dh-systemd check devscripts cups
)

function print_green () {
    if [ "$1" == '-n' ]; then
        echo "$(tput setaf 2)$2 $(tput sgr0)"
    else
        echo -n "$(tput setaf 2)$1 $(tput sgr0)"
    fi
}

function install_packages () {
    declare -a deps=("${!1}")
    for dep in ${deps[@]}; do
        print_green "> Installing "$dep"..."
        if [ "$(dpkg-query -W -f='${db:Status-Status}' $dep >/dev/null 2>&1)" != 'installed' ]; then
            apt-get -y install $dep >/dev/null 2>&1
        fi
        print_green -n "done!"
    done
}

function install_tools {
    command -v unzip >/dev/null 2>&1 || { echo "Installing unzip..."; sudo apt-get install unzip >/dev/null 2>&1; }
}

function install_bluez {
    BLUEZ_VERSION=5.47
    BLUEZ_FILE=bluez-"$BLUEZ_VERSION".tar.xz
    BLUEZ_FILE_URL=http://www.kernel.org/pub/linux/bluetooth/bluez-"$BLUEZ_VERSION".tar.xz
    BLUEZ_DIR="$CONTRIB_DIR"/bluez-"$BLUEZ_VERSION"
    INITIAL_PATH=$(pwd)

    if [ "$(dpkg-query -W -f='${db:Status-Status}' bluez)" = 'installed' ]; then
        print_green -n "> Installing BlueZ $BLUEZ_VERSION... done!"
        return
    fi

    install_packages BLUEZ_DEPENDENCIES[@]

    print_green "> Installing BlueZ $BLUEZ_VERSION..."
    if [ ! -d "$BLUEZ_DIR" ]; then
        mkdir -p "$CONTRIB_DIR"
        mkdir -p "$BLUEZ_DIR"
        wget -O "$CONTRIB_DIR"/"$BLUEZ_FILE" "$BLUEZ_FILE_URL"
        tar -xvf "$CONTRIB_DIR"/"$BLUEZ_FILE" -C "$CONTRIB_DIR"
    fi

    cd "$BLUEZ_DIR"
    sh configure --prefix=/usr
    make
    if [[ $EUID -ne 0 ]]; then
        sudo make install
    else
        make install
    fi
    cd "$INITIAL_PATH"

    print_green -n "done!"
}

function install_libjson {
    print_green "> Installing libjson..."

    if [ -f /usr/lib/libjson.a ]; then
        print_green -n "done!"
        return
    fi

    LIBJSON_URL=https://downloads.sourceforge.net/project/libjson/libjson_7.6.1.zip
    LIBJSON_PATH_URL=https://sourceforge.net/p/libjson/patches/_discuss/thread/0dcc16ec/15c5/attachment/libjson_7.6.1-fix-makefile.patch
    LIBJSON_FILE_NAME=libjson_7.6.1.zip
    LIBJSON_PATCH_FILE_NAME=libjson_7.6.1-fix-makefile.patch
    LIBJSON_DIR="$CONTRIB_DIR"/libjson
    LIBJSON_OPTIONS_FILE="$LIBJSON_DIR"/JSONOptions.h
    INITIAL_PATH=$(pwd)

    if [ ! -d "$LIBJSON_DIR" ]; then
        mkdir -p "$CONTRIB_DIR"
        mkdir -p "$LIBJSON_DIR"
        wget -O "$CONTRIB_DIR"/"$LIBJSON_FILE_NAME" "$LIBJSON_URL"
        unzip "$CONTRIB_DIR"/"$LIBJSON_FILE_NAME" -d "$CONTRIB_DIR"
        wget -O "$CONTRIB_DIR"/"$LIBJSON_PATCH_FILE_NAME" "$LIBJSON_PATH_URL"
    fi
    sed '14 s/^/\/\//' -i "$LIBJSON_OPTIONS_FILE"
    cd "$CONTRIB_DIR"
    patch -Np0 < "$LIBJSON_PATCH_FILE_NAME"
    cd ../"$LIBJSON_DIR"
    make >/dev/null 2>&1
    sudo make install
    cd "$INITIAL_PATH"

    print_green -n "done!"
}

function install_dependencies {
    install_packages DEPENDENCIES[@]
    install_libjson
    install_bluez
}

function create_configuration_files {
    print_green "> Preparing environment structure..."

    ETC_ADCAMID_TEST=/etc/adcamid
    ADCAMID_CONFIG_FILE=$ETC_ADCAMID_TEST/adcamid.conf
    EVENTS_DB_FILE=$ETC_ADCAMID_TEST/events.db
    SERVER_KEY_FILE=$ETC_ADCAMID_TEST/server.key
    SERVER_PEM_FILE=$ETC_ADCAMID_TEST/server.pem
    LOGS_FOLDER=/var/log/adcamid

    mkdir -p "$ETC_ADCAMID_TEST"

    # Write configuration file
    if [ ! -f $ADCAMID_CONFIG_FILE ]; then
        cat >"$ADCAMID_CONFIG_FILE" <<EOL
{
        "bluetoothAdapter": "hci0",
        "remoteEndpoints" : [],
        "gatewayName" : "",
        "opentele" : {
                "username" : "",
                "password" : ""
        },
        "readMeasurementsTimeout": 30
}
EOL
    fi

    # Generate certificates
    if [ ! -f $SERVER_KEY_FILE ]; then
        openssl genrsa -out $SERVER_KEY_FILE 1024
    fi
    if [ ! -f $SERVER_PEM_FILE ]; then
        openssl req -days 365 -out $SERVER_PEM_FILE -new -x509 -key $SERVER_KEY_FILE
    fi

    # Generate database
    if [ ! -f $EVENTS_DB_FILE ]; then
        sqlite3 $EVENTS_DB_FILE < ./scripts/sql/eventsdb_create.sql
    fi

    # Create folders for logs
    sudo mkdir -p "$LOGS_FOLDER"

    print_green -n "done!"
}

function install {
    cp build/Release/adcamid /usr/sbin
    cp scripts/adcamid /etc/init.d
    cp scripts/adcamid.service /lib/systemd/system
    # Configure service to start at boot
    sudo systemctl enable adcamid
    sudo update-rc.d adcamid enable
}

if [[ $1 ]]; then
    $1
else
    # Install necessary tools that might be missing from the system
    install_tools
    # Install project library dependencies
    install_dependencies
    # Prepare environment folders and files
    create_configuration_files
    # Install daemon
    install
fi
