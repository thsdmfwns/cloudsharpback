#!/bin/bash

docker build .. --file ./Dockerfile -t cs_E2E_test
docker run \
--network cloudsharp \
--name cs_E2E_test \
--rm \
-e MYSQL_SERVER="cs_db" \
-e MYSQL_PORT=3306 \
-e MYSQL_USER="cloudsharp" \
-e MYSQL_PASSWORD="cloudsharp" \
-e FILTER="$1" \
-e REDIS_SERVER="redis-stack" \
-e REDIS_PASSWORD="mypassword" \
-v cloudsharp:/var/cloud-sharp \
cs_E2E_test
docker rmi cs_E2E_test