import json

from django.conf import settings

class WithingsUser(object):
    def __init__(self, userid, oauth_token, oauth_token_secret):
        self.userid = userid
        self.oauth_token = oauth_token
        self.oauth_token_secret = oauth_token_secret

def get_withings_user(userid):
    result = [user for user in settings.WITHINGS_USERS if user['userid'] == userid]

    if len(result) == 0:
        raise Exception('Withings user with userid %s could not be found' % (userid))
    
    return WithingsUser(**result[0])

