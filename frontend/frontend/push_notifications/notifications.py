import logging

# Local imports
import apns
import frontend.settings


logger = logging.getLogger("frontend.push_notifications.notifications")

def send_message(devices, message, **kwargs):
    for device in devices:
        if device['type'] == "APNS":
            apns.apns_send_message(
                device['registration_id'],
                message,
                None,
                None,
                **kwargs
            )
            logger.debug("[frontend] Sent notification to device (%s): \"%s\"" % (
                str(device),
                message
            ))
        else:
            logger.debug("[frontend] Failed sending notification to device (%s)." % str(device))