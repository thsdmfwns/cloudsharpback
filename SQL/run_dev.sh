docker build .. --file ./Dockerfile --no-cache -t cloudsharp_db
docker run \
-d \
--network cloudsharp \
--restart=always \
--name cs_db \
-e MYSQL_ROOT_HOSTS=% \
-e MYSQL_ROOT_PASSWORD="cloudsharp" \
-e MYSQL_USER="cloudsharp" \
-e MYSQL_PASSWORD="cloudsharp" \
cloudsharp_db