#!/bin/bash

# Define the path to your solution file
solutionPath="Source/PsionicBomb.sln"

# Define an array of configurations
configurations=("Release v1.2" "Release v1.3" "Release v1.4" "Release v1.5")

# Loop through each configuration and build it
for config in "${configurations[@]}"; do
    echo "Building for configuration: $config"
    msbuild "$solutionPath" /p:Configuration="$config" &
done

echo "All builds completed!"

