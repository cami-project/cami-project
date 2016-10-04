import MySQLdb

from django.conf import settings

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
            If the result is empty or an exception is thrown, then the connection failed.
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
            print "ERROR %d IN CONNECTION: %s" % (e.args[0], e.args[1])
        return False

    def check_message_queue(self):
        return False