<?php
  /**
  * _______ _______  ______  _____  __   _  _____  _     _ _______
  * |  |  | |_____| |_____/ |     | | \  | |   __| |     | |_____|
  * |  |  | |     | |    \_ |_____| |  \_| |____\| |_____| |     |
  *
  * Theme Name:   cami-web
  * Author:       rtud
  * Company:      Maronqua
  * Company URI:  http://maronqua.ro
  * File Name:    _functions-overrides.php
  * Notes:        File needs to be required at the end of theme's main functions.php
  */



// Styles: Append the global styles overrides
// -----------------------------------------------------------------
function mq_global_styles_overrides() {
  wp_register_style( 'mq-bundle-styles-overrides', get_stylesheet_directory_uri() . '/css/styles-overrides-bundle.min.css', '', date('U', filemtime(get_stylesheet_directory() . '/css/styles-overrides-bundle.min.css')), '');
  wp_enqueue_style( 'mq-bundle-styles-overrides' );
}
add_action('wp_print_styles', 'mq_global_styles_overrides');


// Styles: Append the global styles overrides
// -- classes will be used to provide increase in styles specificity
// -- this will insure priority of overrides over existing ones
// -----------------------------------------------------------------
add_filter('body_class', 'mq_global_body_classes');
function mq_global_body_classes($classes) {

  $body_classes_overrides = array(
    'mq',
    'maronqua',
    'styles-overrides',
    'styles-overrides-bundle',
    // create page specific class from the title
    get_the_title() ? 'page_' . sanitize_title(get_the_title()) : ''
  );

  return array_merge($classes, $body_classes_overrides);
}

