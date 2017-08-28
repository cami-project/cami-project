## Using GitHub

## Commits
Commit messages should contain the issue number at the beginning of the message. The commit message itself should be an actual short sentence describing what the commit has changed, and it has to be specific.
Good example:
  - "#32 Checkbox for selecting apartments now works"

Bad example:
  - "Checkbox for selecting apartments no works" (is missing issue number)
  - "#32 Fixed checkbox" (not specific enough)
  - "Checkbox for selecting apartments no works #32" (issue number at the end)

A commit should be contained to one single atomic change. Preferably, the code should be working after each commit. A bad sign is when you find yourself comitting several hundred lines of code or cannot write a specific enough commit message.

## Branches
The master branch corresponds to code on production servers. Be careful:
  - code pushed here will automatically end up on the production servers that hosts the CAMI Cloud
  - development of new features/fixing of bugs will be based off master

Whenever you're working on an issue, you should create a separate branch off master to work on that issue, unless otherwise specified (sometimes there's a bigger feature branch and that will be taking the role of the master branch).


The name of the branch should start with the issue number and be separated by dashes. The name of the branch should resemble the name of the issue. Always make sure that you base your branch off the latest version of master, and that you keep updating the branch with master until you merge the branch.


Good examples for branch names:
  - 32-fix-checkbox-for-selecting-apartments

Bad examples for branch names:
  - fix-checkbox-for-selecting-apartments (no issue number)
  - fix-checkbox (not specific enough)
  - commits on master

## What happens when you've done working on an issue?
Create a pull request into master, and ping someone on Slack or on Github to review it (by CC-ing them via the Github interface). You can create the pull request in 2 ways through the Github interface.


Once your pull request has been reviewed, feel free to merge it to master. After merging it into master, please make sure that everything still works OK in production. Here's a checklist of things to focus on:
  - test that the core features of the system are still working 
  - test that your feature is working as expected
  - test that other features that you know have been impacted by your work are still working as expected

So, here's a short checklist of what you do after you finished an issue:
  - review the list of checkboxes once more, check them in order to make sure you've done all that you were expected to do
  - create pull request from your branch to master
  - ping someone to review it
  - merge branch into master (via the pull request)
  - clean-up the branch by deleting (and use git fetch -p to also delete the local copy of the branch)

## Github issues
We've committed an issue template. You are expected to fill in at least the first 2 fields:
  - "Why" is meant to give everyone an understanding of why you are working on that particular issue. And it also forces you to understand why you're working on that issue
  - "What" is a list of assertable sentences that can be checked off (with associated checkboxes) the developer who worked on the code is always responsible for checking the checkboxes

Here's a [good example of an issue](https://github.com/cami-project/cami-project/issues/196).

## Merging code
Code is only merged via pull requests, which causes a normal "merge".

When you pull new code from upstream, please use --rebase: [http://stackoverflow.com/questions/18930527/difference-between-git-pull-and-git-pull-rebase](http://stackoverflow.com/questions/18930527/difference-between-git-pull-and-git-pull-rebase)


Example command:
`git pull --rebase origin 32-contributing-guide`

## Merging code from master to feature branch
Sometimes you will want to get changes from master into your feature branch. To do this always use git merge:
  - Checkout your branch: `git checkout 32-contributing-guide`
  - Merge master: `git merge master`


## Communication tools (in the order of preference)
  - Slack - write in one of the CAMI channels, feel free to ping someone by using their username don't use private messages, there's something wrong if you find yourself doing that
  - Github - every comment or new issue or pull request created ends up in an e-mail. If you CC someone in Github, you will get their attention quicker
  - e-mail - only do it if really needed :-)

## Setting up Gmail filters for Github
You can create 2 filters for e-mails coming from github:
  - one for all e-mails coming from Github (call this "github")
  - one for all e-mails coming from Github containing your username (call this "github me")

We expect everyone to pay close attention to "github me", while "github" e-mails can be read once per day.

## The prefered GitHub dev flow
  - Dev flow
    1. Clone master branch locally from cami-project/cami-project.
    2. Create and document GitHub issues.
    3. When starting with issue `<issue_nr>` create a branch `#<issue_nr>-<short_issue_desc>`  in cami-project/cami-project
    4. Develop and push commits in this branch `#<issue_nr>-<short_issue_desc>`
        * Prefix commit message with `#<issue_nr>`
        * If there is an issue `#<client_issue_nr>` open in client/client_repo use `#<client_issue_nr>` as the commit prefix.
    5. When issue is done open Pull Request (PR) for code-review from branch `cami-project:#<issue_nr>-<short_issue_desc>`to branch cami-project:master. 
        - Mention colleagues using @ for a last thumbs up
        - Use Github interface to create PR
    6. When review is done close PR
