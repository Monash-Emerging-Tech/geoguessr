#!/bin/bash

# This script download the appropriate js api version from the mazemap servers

# Get version number from argument, if not default to a known version
VERSION=$1
if [[ -z $VERSION ]]; then
    VERSION="2.0.103"
fi

BUNDLED_VERSION_FROM="2.0.103"
js_api_path_prefix="https://api.mazemap.com/js/v$VERSION"
if [[ $BUNDLED_VERSION_FROM == $VERSION ]]
then
    js_api_path_prefix+="/bundled"
else
    version_array=($VERSION $BUNDLED_VERSION_FROM)
    printf -v joined '%s\n' "${version_array[@]}"
    sorted=$(echo $joined | sort -V)
    highest_version=$(echo $joined | sort -V | tail -n1)
    if [[ $highest_version == $VERSION ]]
    then
        js_api_path_prefix+="/bundled"
    fi
fi


echo "Downloading MazeMap JS API version $VERSION from api.mazemap.com..."
curl "${js_api_path_prefix}/mazemap.min.js" -o mazemap.min.js
curl "${js_api_path_prefix}/mazemap.min.css" -o mazemap.min.css
echo "Finished"