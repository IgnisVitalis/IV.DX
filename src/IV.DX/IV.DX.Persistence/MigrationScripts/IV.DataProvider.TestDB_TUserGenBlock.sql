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
-- Table structure for table `TUserGenBlock`
--

DROP TABLE IF EXISTS `TUserGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TUserGenBlock` (
  `ID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Birth` datetime NOT NULL,
  `TUserObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Surname` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID` (`ID`),
  UNIQUE KEY `TUserObjectID_unique` (`TUserObjectID`),
  KEY `FK_TUserGenBlock_TUserObject_0000_idx` (`TUserObjectID`),
  CONSTRAINT `FK_TUserGenBlock_TUserObject_0000` FOREIGN KEY (`TUserObjectID`) REFERENCES `TUserObject` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `TUserGenBlock`
--

LOCK TABLES `TUserGenBlock` WRITE;
/*!40000 ALTER TABLE `TUserGenBlock` DISABLE KEYS */;
INSERT INTO `TUserGenBlock` VALUES ('51449754-6b36-47af-a6af-240b63a589a8','60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff','Pavel','1979-12-31 22:00:00','60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff','Plamenev'),('7f54ea01-3a8d-407e-a4d2-2f2188953f5c','8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3','Victor','1989-09-04 21:00:00','8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3','Suvorov'),('9406a989-6de1-4c5f-b9f5-8f658dc3622e','dfb7bb88-30d9-46d7-9885-6ca8ae455e82','Svitlana','1993-11-16 22:00:00','dfb7bb88-30d9-46d7-9885-6ca8ae455e82','Suvorova');
/*!40000 ALTER TABLE `TUserGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:28
