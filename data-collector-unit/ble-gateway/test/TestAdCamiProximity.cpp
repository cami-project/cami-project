#include <csignal>
#include <cstdlib>
#include <fstream>
#include <iostream>
#include <sys/stat.h>
#include "AdCamiBluetooth5.h"
#include "AdCamiUtilities.h"

using AdCamiHardware::AdCamiBluetooth5;
using AdCamiHardware::AdCamiBluetoothDevice;

AdCamiBluetooth5 *bluetooth = nullptr;
static bool run = true;

/* Function to be executed when daemon receives an interrupt signal. */
static void _InterruptHandlers(int sig) {
    run = false;
}

static void _InterruptPipe(int sig) {}

void _SavePid() {
    unlink(PID_FILE);
    FILE *fp = fopen(PID_FILE, "w");
    if (fp == NULL) {
        PRINT_LOG("Error opening PID file for daemon");
        exit(EXIT_FAILURE);
    }

    pid_t pid = getpid();
    /* Write PID to file */
    fprintf(fp, "%ld\n", (long) pid);
    PRINT_LOG("Daemon created with PID " << static_cast<long>(pid));
    fclose(fp);
}

int main(int argc, char **argv) {
    AdCamiBluetoothError error;
    vector <AdCamiBluetoothDevice> availableDevices;

    auto DiscoveryCallback = [&availableDevices](std::unique_ptr <AdCamiBluetoothDevice> arg) -> void {
        AdCamiBluetoothDevice *device = arg.get();

        PRINT_LOG("Device " << device->Address() << " seen!");
        if (find(availableDevices.begin(),
                 availableDevices.end(),
                 *device) != availableDevices.end()) {
            availableDevices.push_back(*device);
        }
    };

    /* Redirect output of std::cout to DEBUG_FILE file. */
    std::ofstream outDebugStream("/var/log/adproximityd/debug.log");
    std::cout.rdbuf(outDebugStream.rdbuf());
    /* Redirect output of std::cerr to ERROR_FILE file. */
    std::ofstream outErrorStream("/var/log/adproximityd/error.log");
    std::cerr.rdbuf(outErrorStream.rdbuf());
    /* Redirect output of std::clog to LOG_FILE file. */
    std::ofstream outLogStream("/var/log/adproximityd/messages.log");
    std::clog.rdbuf(outLogStream.rdbuf());

    /* Start Bluetooth discovery thread. */
    bluetooth = new AdCamiBluetooth5();
    if ((error = bluetooth->Init()) != AdCamiBluetoothError::BT_OK) {
        switch (error) {
            case BT_ERROR_ADAPTER_NOT_FOUND:
                PRINT_LOG("Couldn't find any Bluetooth adapter. Exiting...");
                break;
            default:
                PRINT_LOG("Problem opening Bluetooth communication [error = " << error << "]. Exiting...");
                break;
        }
        exit(EXIT_FAILURE);
    }

    /* Initialize signal interrupts. */
    struct sigaction sigHupAction = {0}, sigTermAction = {0}, sigPipeAction = {0};

    sigemptyset(&sigHupAction.sa_mask);
    sigHupAction.sa_handler = _InterruptHandlers;
    sigemptyset(&sigTermAction.sa_mask);
    sigTermAction.sa_handler = _InterruptHandlers;
    sigemptyset(&sigPipeAction.sa_mask);
    sigPipeAction.sa_handler = _InterruptPipe;
    sigPipeAction.sa_flags = SA_RESTART;

    if (0 != sigaction(SIGHUP, &sigHupAction, nullptr))
        PRINT_LOG("Failed to install SIGHUP handler: " << strerror(errno));
    if (0 != sigaction(SIGINT, &sigTermAction, nullptr))
        PRINT_LOG("Failed to install SIGINT handler: " << strerror(errno));
    if (0 != sigaction(SIGTERM, &sigTermAction, nullptr))
        PRINT_LOG("Failed to install SIGTERM handler: " << strerror(errno));
    if (0 != sigaction(SIGPIPE, &sigPipeAction, nullptr))
        PRINT_LOG("Failed to install SIGPIPE handler: " << strerror(errno));

    /* Set filter flag, discovery callback and start background Bluetooth discovery thread. */
    bluetooth->FilterDevices(false);
    bluetooth->SetDiscoveryCallback(DiscoveryCallback);

    if ((error = bluetooth->StartDiscovery()) != BT_OK) {
        PRINT_LOG("Problem starting discovery [error = " << error << "]. Exiting...");
        exit(EXIT_FAILURE);
    } else {
        PRINT_LOG("Bluetooth discovery started.");
    }

    _SavePid();

    while (run) { sleep(10); }

    /* Stop Bluetooth discovery thread. */
    if ((error = bluetooth->StopDiscovery()) != BT_OK) {
        PRINT_LOG("Problem stopping discovery [error = " << error << "]");
        exit(EXIT_FAILURE);
    } else {
        PRINT_LOG("Bluetooth discovery stopped.");
    }

    exit(EXIT_SUCCESS);
}
