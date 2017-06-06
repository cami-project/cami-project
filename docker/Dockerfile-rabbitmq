FROM rabbitmq:3-management

# Bake configuration into image for ease of use. We'll later move this to
# runtime configuration.
ENV RABBITMQ_ERLANG_COOKIE='sa9jas9cjsa90cja9jc09-a8da'
ENV RABBITMQ_DEFAULT_VHOST=cami

# TODO(@iprunache): Find a way to create a new user without admin priviledges
# istead of using the default admin user.
ENV RABBITMQ_DEFAULT_USER=cami
ENV RABBITMQ_DEFAULT_PASS=cami
