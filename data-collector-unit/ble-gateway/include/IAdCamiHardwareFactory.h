//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_IAdCamiHardwareFactory_h
#define AdCamiDaemon_IAdCamiHardwareFactory_h

#include "IAdCamiBluetooth.h"

namespace AdCamiHardware {

/**
 */
class IAdCamiHardwareFactory {
public:
    enum EnumHardwareState {
        OK				= 0,
        BLUETOOTH_ERROR	= -1
    };
    
    virtual IAdCamiHardwareFactory::EnumHardwareState Init() = 0;
    virtual void Destroy() = 0;
    virtual IAdCamiBluetooth* Bluetooth() const = 0;
};
    
} //namespace
#endif
