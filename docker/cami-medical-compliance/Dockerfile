# Use a base image that has ansible set up.
FROM williamyeh/ansible:ubuntu16.04

# Add playbooks to the Docker image. This requires that the build context is the
# project root folder. Run build like:
# `docker build -f docker/cami-store/Dockerfile .`
COPY . /cami-project
WORKDIR /cami-project

# Run Ansible to configure the Docker image
RUN ansible-playbook ansible/medical_compliance_provision.yml -c local

COPY docker/cami-medical-compliance/docker-entrypoint.sh /usr/local/bin/
ENTRYPOINT ["docker-entrypoint.sh"]

EXPOSE 8000
