docker build .. --file ./Dockerfile -t dev

docker run \
-d \
--network cloudsharp \
--rm \
--name cs_backend \
-e MYSQL_SERVER="cs_db" \
-e MYSQL_PORT=3306 \
-e MYSQL_USER="cloudsharp" \
-e MYSQL_PASSWORD="cloudsharp" \
-e REDIS_SERVER="redis-stack" \
-e REDIS_PASSWORD="mypassword" \
-v cloudsharp:/var/cloud-sharp \
dev

docker network connect bridge cs_backend

<<"End"
[
  {
    "Key" : "CS_VOLUME_PATH",
    "Pattern" : "^(/[^/ ]*)+/?$",
    "Default" : "/var/cloud-sharp"
  },
  {
    "Key" : "MYSQL_SERVER",
    "Pattern" : null,
    "Default" : "cs_db"
  },
  {
    "Key" : "MYSQL_PORT",
    "Pattern" : "^[0-9]{1,5}$",
    "Default" : "3306"
  },
  {
    "Key" : "MYSQL_USER",
    "Pattern" : null,
    "Default" : null
  },
  {
    "Key" : "MYSQL_PASSWORD",
    "Pattern" : null,
    "Default" : null
  }
  {
      "Key" : "REDIS_SERVER",
      "Pattern" : null,
      "Default" : "cs_redis"
    },
    {
      "Key" : "REDIS_PORT",
      "Pattern" : null,
      "Default" : 6379
    },
    {
      "Key" : "REDIS_PASSWORD",
      "Pattern" : null,
      "Default" : null
    }
]

End