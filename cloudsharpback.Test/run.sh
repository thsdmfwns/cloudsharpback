#!/bin/bash

docker build .. --file ./Dockerfile -t cs_test
docker run \
--network cloudsharp \
--name cs_test \
--rm \
-e MYSQL_SERVER="cs_db" \
-e MYSQL_PORT=3306 \
-e MYSQL_USER="root" \
-e MYSQL_PASSWdeaORD=3279 \
-e FILTER="$1" \
cs_test
docker rmi cs_test