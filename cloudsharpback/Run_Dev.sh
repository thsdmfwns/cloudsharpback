docker build .. --file ./Dockerfile --no-cache -t dev

docker run \
-d \
--network cloudsharp \
--rm \
--name cloudsharp_db \
-e MYSQL_USER="cloudsharp" \
-e MYSQL_PASSWORD="cloudsharp" \
dev

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
]

End