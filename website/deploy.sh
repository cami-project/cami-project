#!/bin/bash

# Color Defs
white=$(tput setaf 7)
bold=$(tput bold)
reset=$(tput sgr0)
symHammers='\xe2\x9a\x92'

# Let everyone know
echo -e "\n${symHammers} ${bold}${white}Grabbing latest styles overrides...${reset}\n"

# Grab the latest compiled bundfle from GitHub and add it to the website theme's CSS dir
# - this will also output an activity log inside the home dir
wget --output-document=/var/www/html/wp-content/themes/theme/css/test.css --output-file=$HOME/styles-overrides-fetch.log https://raw.githubusercontent.com/cami-project/cami-project/master/website/assets/styles/styles-overrides-bundle.min.css
