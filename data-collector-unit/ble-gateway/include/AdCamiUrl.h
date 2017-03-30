//
//  Created by Jorge Miguel Miranda on 23/11/16.
//

#ifndef AdCamiDaemon_AdCamiUrl_h
#define AdCamiDaemon_AdCamiUrl_h

#include <iostream>
#include <string>

using std::string;

namespace AdCamiCommunications {
    
/**
 */
class AdCamiUrl : public std::string {
private:
    unsigned int _numberUrlTokens;
    
    void _CountTokens();

public:
    AdCamiUrl();
    AdCamiUrl(const char* url);
    AdCamiUrl(const string& url);
    ~AdCamiUrl();
    
    friend bool operator== (const AdCamiUrl& lhs, const AdCamiUrl& rhs);
    friend bool operator!= (const AdCamiUrl& lhs, const AdCamiUrl& rhs);
    friend bool operator< (const char* lhs, const AdCamiUrl& rhs);
    friend bool operator< (const AdCamiUrl& lhs, const AdCamiUrl& rhs);
    friend string operator+ (const char* lhs, const string& rhs);
};
    
} //namespace
#endif /* defined(AdCamiDaemon_AdCamiUrl_h) */
