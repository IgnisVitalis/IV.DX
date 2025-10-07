CREATE DATABASE  IF NOT EXISTS `IV.DataProvider.TestDB` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `IV.DataProvider.TestDB`;
-- MySQL dump 10.13  Distrib 8.0.26, for Win64 (x86_64)
--
-- Host: 159.89.98.54    Database: IV.DataProvider.TestDB
-- ------------------------------------------------------
-- Server version	8.0.21

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `TDeviceObject`
--

DROP TABLE IF EXISTS `TDeviceObject`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TDeviceObject` (
  `ID` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `User` char(36) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID` (`ID`),
  KEY `FK_TDeviceObject_User` (`User`),
  CONSTRAINT `FK_TDeviceObject_User` FOREIGN KEY (`User`) REFERENCES `TUserObject` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `TDeviceObject`
--

LOCK TABLES `TDeviceObject` WRITE;
/*!40000 ALTER TABLE `TDeviceObject` DISABLE KEYS */;
INSERT INTO `TDeviceObject` VALUES ('1c16f974-8e52-408b-9cac-acbb548864fa','60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff'),('24d8f6ff-b411-4acc-8a35-5e958ce7f070','60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff'),('53ced1ab-2582-4aee-b2bc-50e676eebde3','8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3'),('a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7','8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3'),('36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b','dfb7bb88-30d9-46d7-9885-6ca8ae455e82'),('58a98dbf-ce5d-43d1-adb2-670dea20c7bf','dfb7bb88-30d9-46d7-9885-6ca8ae455e82');
/*!40000 ALTER TABLE `TDeviceObject` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:31
