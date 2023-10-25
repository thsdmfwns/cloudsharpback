#!/bin/bash

source /tools/checkEnv.sh 

if [ $validate_result != 0 ]; then
    echo "There are enviornment value problems"
    exit 1
fi

exec dotnet cloudsharpback.dll