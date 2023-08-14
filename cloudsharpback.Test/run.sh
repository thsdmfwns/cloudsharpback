docker build .. --file ./Dockerfile -t cs_test
docker run --network cloudsharp \
--name cs_test \
--rm \
-e CS_VOLUME_PATH=/var/cloud-sharp/ \
-e MYSQL_SERVER=cs_db \
-e MYSQL_PORT=3306 \
-e MYSQL_USER=root \
-e MYSQL_PASSWORD=3279 \
cs_test