import logging

# Local imports
import apns
import requests
from django.conf import settings


logger = logging.getLogger("frontend.push_notifications.notifications")

def send_message(devices, message, **kwargs):
    for device in devices:
        if device['type'] == "APNS":
            try:
                apns.apns_send_message(
                    device['registration_id'],
                    message,
                    None,
                    None,
                    **kwargs
                )

                logger.debug(
                    "[frontend] Sent notification to device (%s): \"%s\"",
                    str(device),
                    message
                )
            except apns.APNSError as e:
                logger.debug(
                    "[frontend] Failed sending notification to device (%s): %s",
                    str(device),
                    repr(e)
                )
            except Exception as ex:
                logger.debug(
                    "[frontend] Failed sending notification to device (%s): %s",
                    str(device),
                    repr(ex)
                )
        elif device['type'] == "GCM":
            try:
                send_pushbots_notification(device, message)
            except Exception as e:
                logger.debug(
                    "[frontend] Failed sending notification to device (%s): %s",
                    str(device),
                    repr(e)
                )
        else:
            logger.debug(
                    "[frontend] Failed sending notification to device (%s): push notification provider currently not supported",
                    str(device)
                )


def send_pushbots_notification(device, message):
    endpoint = settings.PUSH_NOTIFICATIONS_SETTINGS["PUSHBOTS_URI"] + "/push/all"

    method = 'POST'
    data = {
        "msg" : message,
        "platform" : [
            1
        ],
        "alias": device["user"],
        "payload" : {}
    }

    headers = {
        "x-pushbots-appid" : settings.PUSH_NOTIFICATIONS_SETTINGS["PUSHBOTS_APP_ID"],
        "x-pushbots-secret": settings.PUSH_NOTIFICATIONS_SETTINGS["PUSHBOTS_SECRET"],
        "Content-Type": "application/json"
    }

    r = requests.request(
        method,
        endpoint,
        headers = headers,
        json=data
    )

    if r.status_code in [200, 201]:
        logger.debug(
            "[frontend] Sent notification to device (%s): \"%s\"",
            str(device),
            message
        )

    else:
        logger.debug(
            "[frontend] " +
            "Error sending notification to device (%s). Message: \"%s\". Response: %s" % (str(device), message, r.text)
        )

