#!/bin/bash

# Color Defs
white=$(tput setaf 7)
bold=$(tput bold)
reset=$(tput sgr0)
symHammers='\xe2\x9a\x92'

# Grab the latest compiled bundle from GitHub and add it to the website theme's CSS dir
# - this will also output an activity log inside the home dir
echo -e "\n${symHammers} ${bold}${white}Grabbing latest styles overrides...${reset}\n"
wget --output-document=/var/www/html/wp-content/themes/theme/css/styles-overrides-bundle.min.css https://raw.githubusercontent.com/cami-project/cami-project/master/website/assets/styles/styles-overrides-bundle.min.css 2>&1 | tee -a $HOME/deploy.log

# Grab the latest _functions-overrides.php tpl that enables the overrides capabilities
echo -e "\n${symHammers} ${bold}${white}Grabbing latest functions overrides...${reset}\n"
wget --output-document=/var/www/html/wp-content/themes/theme/_functions-overrides.php https://raw.githubusercontent.com/cami-project/cami-project/master/website/includes/_functions-overrides.php 2>&1 | tee -a $HOME/deploy.log

# Insure spacing in-between log entries
echo -e "\n=============================================================\n" >> deploy.log
