import MySQLdb
import pika
import logging

from django.conf import settings

# Get an instance of a logger
logger = logging.getLogger(__name__)

class Healthchecker:
    def __init__(self):
        # Read the DB settings from the django project settings file
        db_settings = getattr(settings, "DATABASES", None)['default']
        self.user = db_settings['USER']
        self.password = db_settings['PASSWORD']
        self.server = db_settings['HOST']
        self.database = db_settings['NAME']

        _mysql = self.check_mysql()
        _message_queue = self.check_message_queue()

        self.mysql = 'ok' if _mysql else 'failed'
        self.message_queue = 'ok' if _message_queue else 'failed'
        self.status = 'ok' if (_mysql and _message_queue) else 'failed'

    def check_mysql(self):
        """ 
        Try to connect to the database and run a dummy querry.

        If the result is empty or an exception is thrown, then 
        the connection failed.
        """
        try:
            db = MySQLdb.connect(self.server, self.user, self.password, self.database)

            cursor = db.cursor()
            cursor.execute("SELECT VERSION()")

            results = cursor.fetchone()

            if results:
                return True
            else:
                return False
        except MySQLdb.Error, e:
            logger.error("ERROR %d IN MYSQL CONNECTION: %s" % (e.args[0], e.args[1]))
        return False

    def check_message_queue(self):
        class Helper:
            """
            Helper class to close a connection and memorize a return value
            """

            def __init__(self):
                self.ret = True

            def close_connection(self, conn, ret):
                self.ret = ret
                conn.close()
                if ret is False:
                    logger.error("ERROR in RabbitMQ connection: Connection timed out")

        helper = Helper()
        
        """
        Try to connect to RabbitMQ server with pika

        If the connection will timeout or an exception will be thrown, 
        then the connection failed

        NOTES: workaround needed - the built-in connection timeout parameters 
        of pika don't work, so we had to use the callbacks of SelectConnection
        """
        try:
            # Connect to RabbitMQ server
            parameters = pika.URLParameters(settings.BROKER_URL)

            # Connection ok
            # Using on_connect callback of SelectConnection to close the
            # connection(and the ioloop implicitly) and set the return
            # value to True
            connection = pika.SelectConnection(parameters, lambda conn: helper.close_connection(conn, True))

            # Connection timeout
            # Using on_timeout callback of SelectConnection to close 
            # connection and set the return value to False
            connection.ioloop.add_timeout(settings.RABBITMQ_CHECK_TIMEOUT, lambda: helper.close_connection(connection, False))

            # Start the ioloop
            connection.ioloop.start()

        # Connection error
        except pika.exceptions.AMQPConnectionError, e:
            logger.error("ERROR in RabbitMQ connection")
            return False

        return helper.ret