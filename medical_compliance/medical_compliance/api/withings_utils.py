import json
import logging

from django.conf import settings

logger = logging.getLogger("medical_compliance")

class WithingsUser(object):
    def __init__(self, userid, oauth_token, oauth_token_secret):
        self.userid = userid
        self.oauth_token = oauth_token
        self.oauth_token_secret = oauth_token_secret

def get_withings_user(userid):
    result = [user for user in settings.WITHINGS_USERS if user['userid'] == int(userid)]

    logger.debug("[medical-compliance] settings.WITHINGS_USERS: %s" % (settings.WITHINGS_USERS))
    logger.debug("[medical-compliance] Looked up userid: %s" % (userid))
    logger.debug("[medical-compliance] Look up result: %s" % (result))

    if len(result) == 0:
        raise Exception('Withings user with userid %s could not be found' % (userid))
    
    return WithingsUser(**result[0])

