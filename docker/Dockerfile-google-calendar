# Use a base image that has ansible set up.
FROM williamyeh/ansible:ubuntu16.04

# Add playbooks to the Docker image. This requires that the build context is the
# project root folder. Run build like:
# `docker build -f docker/cami-store/Dockerfile .`
COPY . /cami-project
WORKDIR /cami-project

# Run Ansible to configure the Docker image
RUN ansible-playbook ansible/google_calendar.yml -c local

COPY docker/google_calendar/docker-message-worker-entrypoint.sh /usr/local/bin/
COPY docker/google_calendar/docker-message-worker-entrypoint-dev.sh /usr/local/bin/
COPY docker/google_calendar/docker-task-scheduler-entrypoint.sh /usr/local/bin/
COPY docker/google_calendar/docker-task-scheduler-entrypoint-dev.sh /usr/local/bin/

RUN chmod +x /usr/local/bin/docker-message-worker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-message-worker-entrypoint-dev.sh
RUN chmod +x /usr/local/bin/docker-task-scheduler-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-task-scheduler-entrypoint-dev.sh

RUN adduser --disabled-password --gecos '' google_calendar
RUN chown -R google_calendar /cami-project

USER google_calendar

ENTRYPOINT ["docker-message-worker-entrypoint.sh"]
