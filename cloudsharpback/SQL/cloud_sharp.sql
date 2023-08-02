CREATE DATABASE cloud_sharp;

USE cloud_sharp;

-- member: table
CREATE TABLE `member` (
                          `member_id` bigint unsigned NOT NULL AUTO_INCREMENT,
                          `id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
                          `password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
                          `nickname` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
                          `role_id` bigint unsigned NOT NULL,
                          `email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
                          `directory` binary(16) NOT NULL,
                          `profile_image` varchar(255) DEFAULT NULL,
                          UNIQUE KEY `member_id` (`member_id`),
                          UNIQUE KEY `id` (`id`) USING BTREE,
                          KEY `role_id` (`role_id`),
                          CONSTRAINT `member_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- No native definition for element: role_id (index)

-- password_store_directory: table
CREATE TABLE `password_store_directory` (
                                            `password_directory_id` bigint unsigned NOT NULL AUTO_INCREMENT,
                                            `name` varchar(256) NOT NULL COMMENT 'directory_name',
                                            `comment` varchar(256) DEFAULT NULL,
                                            `icon` varchar(256) DEFAULT NULL,
                                            `last_edited_time` bigint unsigned NOT NULL,
                                            `created_time` bigint unsigned NOT NULL COMMENT '                              ',
                                            `member_id` bigint unsigned NOT NULL,
                                            PRIMARY KEY (`password_directory_id`),
                                            KEY `member_id` (`member_id`),
                                            CONSTRAINT `member_id` FOREIGN KEY (`member_id`) REFERENCES `member` (`member_id`)
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- password_store_keys: table
CREATE TABLE `password_store_keys` (
                                       `password_store_key_id` bigint unsigned NOT NULL AUTO_INCREMENT,
                                       `owner_id` bigint unsigned NOT NULL,
                                       `public_key` varchar(1024) NOT NULL,
                                       `private_key` varchar(1024) NOT NULL,
                                       `encrypt_algorithm` int NOT NULL,
                                       `created_time` bigint unsigned NOT NULL,
                                       `name` varchar(256) NOT NULL,
                                       `comment` varchar(256) DEFAULT NULL,
                                       PRIMARY KEY (`password_store_key_id`),
                                       KEY `password_store_keys_owner_id` (`owner_id`),
                                       CONSTRAINT `password_store_keys_owner_id` FOREIGN KEY (`owner_id`) REFERENCES `member` (`member_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- password_store_value: table
CREATE TABLE `password_store_value` (
                                        `password_store_value_id` bigint unsigned NOT NULL AUTO_INCREMENT,
                                        `directory_id` bigint unsigned NOT NULL,
                                        `encrypt_key_id` bigint unsigned NOT NULL,
                                        `value_id` varchar(1024) DEFAULT NULL,
                                        `value_password` varchar(1024) NOT NULL COMMENT '키로 암호화 ',
                                        `created_time` bigint unsigned NOT NULL,
                                        `last_edited_time` bigint NOT NULL,
                                        PRIMARY KEY (`password_store_value_id`),
                                        KEY `password_store_value_directory_id` (`directory_id`),
                                        KEY `password_store_value_encrypt_key_id` (`encrypt_key_id`),
                                        CONSTRAINT `password_store_value_directory_id` FOREIGN KEY (`directory_id`) REFERENCES `password_store_directory` (`password_directory_id`) ON DELETE CASCADE,
                                        CONSTRAINT `password_store_value_encrypt_key_id` FOREIGN KEY (`encrypt_key_id`) REFERENCES `password_store_keys` (`password_store_key_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- role: table
CREATE TABLE `role` (
                        `role_id` bigint unsigned NOT NULL AUTO_INCREMENT,
                        `name` varchar(255) DEFAULT NULL,
                        UNIQUE KEY `role_id` (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1000 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO role(role_id, name) VALUES (1, 'user');
INSERT INTO role(role_id, name) VALUES (2, 'member');

-- share: table
CREATE TABLE `share` (
                         `share_id` bigint unsigned NOT NULL AUTO_INCREMENT,
                         `member_id` bigint unsigned NOT NULL,
                         `target` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
                         `password` varchar(255) DEFAULT NULL,
                         `expire_time` bigint unsigned DEFAULT NULL,
                         `comment` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
                         `share_time` bigint unsigned NOT NULL,
                         `share_name` varchar(255) DEFAULT NULL,
                         `token` binary(16) NOT NULL,
                         `file_size` bigint unsigned NOT NULL,
                         UNIQUE KEY `id` (`share_id`),
                         UNIQUE KEY `token` (`token`) USING BTREE,
                         KEY `member_id` (`member_id`),
                         CONSTRAINT `share_ibfk_1` FOREIGN KEY (`member_id`) REFERENCES `member` (`member_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- No native definition for element: member_id (index)

-- torrent: table
CREATE TABLE `torrent` (
                           `torrent_id` bigint unsigned NOT NULL AUTO_INCREMENT,
                           `owner_member_id` bigint unsigned NOT NULL,
                           `torrent_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
                           `download_path` varchar(255) NOT NULL,
                           UNIQUE KEY `idx` (`torrent_id`),
                           KEY `owner_member_id` (`owner_member_id`),
                           CONSTRAINT `torrent_ibfk_1` FOREIGN KEY (`owner_member_id`) REFERENCES `member` (`member_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- No native definition for element: owner_member_id (index)

