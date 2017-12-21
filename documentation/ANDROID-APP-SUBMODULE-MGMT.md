## Managing the cami-android-app git submodule

### Initializing submodule after cloning CAMI repo

After cloning the main CAMI repo, run the following commands to initialize the submodule and pull its existing code:
`git submodule init`
`git submodule update`

### Updating the submodule
If you want to bring in upstream changes in the submodule, you have the following two options:
  - go to the cami-androi-app directory and run the combination `git fetch`, `git merge origin/master`. This assumes
  you want to pull in changes to the master branch in the cami-android-app submodule.

  - in the CAMI main repo, run the following command: `git submodule update --remote cami-android-app`. This command again assumes
  that you want to update the checkout to the master branch of the submodule repository.
