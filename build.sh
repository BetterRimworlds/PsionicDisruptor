#!/bin/bash

# Check if inotifywait is installed
if [ -z "$(which inotifywait)" ]; then
    echo "inotifywait not installed."
    echo "Install the inotify-tools package and try again."
    exit 1
fi

# Directory to monitor for changes
dir="./Source"

# Define the path to your solution file
solutionPath="Source/PsionicBomb.sln"

# Define an array of configurations
configurations=("Release v1.2" "Release v1.3" "Release v1.4" "Release v1.5")

function build() {
    # Loop through each configuration and build it
    for config in "${configurations[@]}"; do
        echo "Building for configuration: $config"
        msbuild "$solutionPath" /p:Configuration="$config" &
    done

    echo "All builds completed!"
}

build

# Watch for changes to .cs files in the directory and subdirectories
inotifywait --recursive --monitor --format "%e %w%f" \
    --event modify,move,create,delete $dir \
    --include '\.cs$' |
    while read changed; do
        echo "Detected change in $changed"
        build
    done
