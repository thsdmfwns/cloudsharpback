#!/bin/bash

source /src/tools/checkEnv.sh ./EnvValues.json

if [ $validate_result != 0 ]; then
    echo "There are enviornment value problems"
    exit 1
fi

if  [ -z "$FILTER" ]; then
    exec dotnet test    
fi

exec dotnet test --filter "$FILTER"
