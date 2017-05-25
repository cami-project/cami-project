# Deploying CAMI Cloud
CAMI Cloud infrastructure is made of Docker Containers that are run from images. The images that are used for production deployment are stored and pulled from [Docker Hub](https://hub.docker.com/r/vitaminsoftware/cami-project/). Whenever the `master` branch of the project is modified, the images are automatically rebuilt by Docker Hub.

Let's assume that you just pushed some commits in the `master` branch. The steps that must be followed afterwards are described below.

## Simple procedure

### 1. Check the build status of the Docker Images
 * Go to [Docker Hub](https://hub.docker.com/r/vitaminsoftware/cami-project/) to see the build status of the project's Docker Images
* Wait for them to be successfully built

### 2. Upgrade the containers from Rancher Web UI
* Go to [CAMI's Rancher Web UI](http://138.68.92.229:8080/) and select the **CAMI** stack. There you'll find the already set-up docker containers
* Select the ones you know were modified and click the **Upgrade** button (up arrow icon) from the right of the entry row
* You'll be redirected to a new page. Scroll down to it's bottom and click Upgrade
* Wait for the upgrade spinner to stop and then click the **Finish Upgrade** button(check icon). That should be all

**NOTES**: It's **strongly recommended** to **not** touch the `cami-mysql` container, as this contains the MySQL instance and can lead to total data loss.

## Advanced actions
### Adding a new container image in Docker Hub
* Log in to [Docker Hub](https://hub.docker.com/) with your vitamin email
* Go to [CAMI on Docker Hub](https://hub.docker.com/r/vitaminsoftware/cami-project/)
* Go to **Build Settings** tab
* Add a new docker image entry
* Set the `Dockerfile location` to the container's docker file path from the project's repo
* Set the `Dockertag name` to the container's name used in the dockerfile's name
* Click **Save Changes**

### Adding a new container in Rancher
* Go to [CAMI's Rancher Web UI](http://138.68.92.229:8080/) and select the **CAMI** stack
* Click *Add Service* button
* Fill the name of the container
* Fill the `Select Image` field with: `vitaminsoftware/cami-project:<IMAGE_TAG_FROM_DOCKER_HUB>`
* Add `Port Maps` or `Service Links` if needed (like those used in the docker compose file)
* Fill an entry point if needed
* Click **Create**
* Start the container that you just created by clicking the **Start button** (play icon)