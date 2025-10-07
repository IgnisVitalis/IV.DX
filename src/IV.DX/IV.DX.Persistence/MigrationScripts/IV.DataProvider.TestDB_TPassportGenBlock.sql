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
-- Table structure for table `TPassportGenBlock`
--

DROP TABLE IF EXISTS `TPassportGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TPassportGenBlock` (
  `ID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SerialNumber` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `TPassportObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID` (`ID`),
  UNIQUE KEY `TPassportObjectID_unique` (`TPassportObjectID`),
  KEY `FK_TPassportGenBlock_TPassportObject_0000_idx` (`TPassportObjectID`),
  CONSTRAINT `FK_TPassportGenBlock_TPassportObject_0000` FOREIGN KEY (`TPassportObjectID`) REFERENCES `TPassportObject` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `TPassportGenBlock`
--

LOCK TABLES `TPassportGenBlock` WRITE;
/*!40000 ALTER TABLE `TPassportGenBlock` DISABLE KEYS */;
INSERT INTO `TPassportGenBlock` VALUES ('365ee65a-eb05-4865-8988-1d6ed3cbff5c','1f852be3-f851-4807-807f-cfd45a0b4093','6bcc2af44aa3','1f852be3-f851-4807-807f-cfd45a0b4093'),('9582877a-b115-41f4-8a0d-98f5b994ce70','459276b0-69db-43da-bf9d-c2683c4b4d39','4a82ceaa1cff','459276b0-69db-43da-bf9d-c2683c4b4d39'),('c13efbf5-348a-4551-806a-206dfc44a861','bd56ffdd-0d30-4d9f-b879-7875162fc7b6','6ca8ae455e82','bd56ffdd-0d30-4d9f-b879-7875162fc7b6');
/*!40000 ALTER TABLE `TPassportGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:36
