# CAMI-WEB styling overrides

![Cami App Logo](../application/ios/Cami/Image.xcassets/AppIcon.appiconset/cami-ios-app-icon-183.png)

The purpose of these overrides is to unify the look of the WordPress-based web front-end of the CAMI System with the already established iOS app.

In order to provide only aesthetic changes and not alter the website's core functionality and structure, the resulting assets of this override effort, will be enqueued inside the existing theme. The aim is to interfere as little as possible with the theme.

## Requirements

* [node.js / npm](https://nodejs.org/en/download/package-manager/)
* [ruby](https://www.ruby-lang.org/en/documentation/installation/)
* [sass](http://sass-lang.com/install)

## Development

In order to start working on the styles run the following commands in a terminl window inside the current directory:

```
$ npm install
$ grunt assmble
$ grunt dev
```

## Deploying

1. insure that you have the latest bundle compiled by running `$ grunt assemble`
2. ssh into the server using pem file from LastPass
3. run `$ ./deploy.sh` command

## Technology stack

* SASS
