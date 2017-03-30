//
//  AdCamiUrl.cpp
//  AdCamiBleDaemon
//
//  Created by Jorge Miguel Miranda on 08/11/13.
//
//

#include <cstring>
#include "AdCamiUrl.h"
#include "AdCamiUtilities.h"

namespace AdCamiCommunications {

AdCamiUrl::AdCamiUrl() : std::string(), _numberUrlTokens(0) {
    this->_CountTokens();
}

AdCamiUrl::AdCamiUrl(const char* url) : std::string(url), _numberUrlTokens(0) {
	this->_CountTokens();
}

AdCamiUrl::AdCamiUrl(const std::string& url) : std::string(url), _numberUrlTokens(0) {
    this->_CountTokens();
}

AdCamiUrl::~AdCamiUrl() {}

void AdCamiUrl::_CountTokens() {
    for (unsigned int i = 0; i < this->length(); i++) {
        this->_numberUrlTokens += (this->at(i) == '/' ? 1 : 0);
    }
}

bool operator== (const AdCamiUrl& lhs, const AdCamiUrl& rhs) {
    unsigned int lhs_len = lhs.length(), rhs_len = rhs.length();
    unsigned int lhs_counter = 0, rhs_counter = 0;
    
    /* If the number of tokens is different, the URLs are not equal. */
    if (lhs._numberUrlTokens != rhs._numberUrlTokens) return false;
        
    /* Compare the strings. */
    for (; lhs_counter < lhs_len && rhs_counter < rhs_len; lhs_counter++, rhs_counter++) {
        /* If an asterisk is found, advance on the right string until it ends or
         * another character is found. */
        if (lhs[lhs_counter] == '*') {
            while (rhs_counter < rhs_len && rhs[rhs_counter] != '/') {
                rhs_counter++;
            }
        }
        /* Characters are different, therefore the string is different. */
        else if (lhs[lhs_counter] != rhs[rhs_counter])
            return false;
    }

    return (lhs_counter == lhs_len && rhs_counter == rhs_len);
}

bool operator!= (const AdCamiUrl& lhs, const AdCamiUrl& rhs) {
	return (lhs != rhs);
}

bool operator< (const char* lhs, const AdCamiUrl& rhs) {
	return AdCamiUrl(lhs) < rhs;
}

bool operator< (const AdCamiUrl& lhs, const AdCamiUrl& rhs) {
    unsigned int lhs_len = lhs.length(), rhs_len = rhs.length();
    unsigned int lhs_counter = 0, rhs_counter = 0;
    
    if (lhs._numberUrlTokens > rhs._numberUrlTokens) return false;
    
    /* Compare the strings. */
    for (; lhs_counter < lhs_len && rhs_counter < rhs_len; lhs_counter++, rhs_counter++) {
        /* If an asterisk is found, advance on the right string until it ends or
         * another character is found. */
        if (lhs[lhs_counter] == '*') {
            while (rhs_counter < rhs_len && rhs[rhs_counter] != '/') {
                rhs_counter++;
            }
        }
        /* If the character of the left-string is minor than the one of the
         * right-string, the string is less. */
        else if (lhs[lhs_counter] < rhs[rhs_counter])
            return true;
    }

    /* It is known that the left-string is lesser than the right-string. Therefore,
     * if the right-string wasn't fully compared, then it is assured that the left
     * is lesser, otherwise it is greater. */
    return true;//rhs_counter < rhs_len ? true : false;
}

string operator+ (const char* lhs, const string& rhs) {
    return string(lhs) + rhs;
}

} //namespace