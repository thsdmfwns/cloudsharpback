#!/bin/bash

/src/tools/checkEnv.sh ./EnvValues.json
result=$?

if [ $result != 0 ]; then
    echo "There are enviornment value problems"
    exit 1
fi

exec dotnet test
