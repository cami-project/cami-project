# CAMI-WEB styling overrides

![Cami App Logo](../application/ios/Cami/Images.xcassets/AppIcon.appiconset/cami-ios-app-icon-183.png)

The purpose of these overrides is to unify the look of the WordPress-based web front-end of the CAMI System with the already established iOS app.

In order to provide only aesthetic changes and not alter the website's core functionality and structure, the resulting assets of this override effort, will be enqueued inside the existing theme. The aim is to interfere as little as possible with the theme.

## Requirements

* [node.js / npm](https://nodejs.org/en/download/package-manager/)
* [ruby](https://www.ruby-lang.org/en/documentation/installation/)
* [sass](http://sass-lang.com/install)

The currently used WordPress themes needs to include the following enqueuing function at the end of the `functions.php` file:

```
// Styles: Enqueue the global styles overrides
// -- we're using the file's date/time at the end of file to override caching
// --------------------------------------------------------------------------
function mq_global_styles_overrides() {
	wp_register_style('mq-bundle-styles-overrides', get_stylesheet_directory_uri() . '/css/styles-overrides-bundle.min.css', '', date('U', filemtime(get_stylesheet_directory() . '/css/styles-overrides-bundle.min.css')), '');
	wp_enqueue_style('mq-bundle-styles-overrides');
}
add_action('wp_print_styles', 'mq_global_styles_overrides');
```

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
