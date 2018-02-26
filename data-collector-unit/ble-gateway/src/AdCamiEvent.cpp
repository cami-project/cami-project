//
// Created by Jorge Miguel Miranda on 26/10/2017.
//

#include "AdCamiCommon.h"
#include "AdCamiEvent.h"
#include "AdCamiUtilities.h"
#include <iomanip>
#include <sstream>

namespace AdCamiData {

double operator-(const AdCamiEvent &levent, const AdCamiEvent &revent) {
    std::istringstream ssl(levent._timeStamp);
    std::istringstream ssr(revent._timeStamp);
    std::tm ltime, rtime;

    ssl >> std::get_time(&ltime, AdCamiCommon::kDateTimeFormat);
    ssr >> std::get_time(&rtime, AdCamiCommon::kDateTimeFormat);

    return difftime(mktime(&ltime), mktime(&rtime));
}

}
