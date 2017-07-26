import logging

# Local imports
import apns
import frontend.settings


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
        else:
            logger.debug(
                    "[frontend] Failed sending notification to device (%s): push notification provider currently not supported",
                    str(device)
                )