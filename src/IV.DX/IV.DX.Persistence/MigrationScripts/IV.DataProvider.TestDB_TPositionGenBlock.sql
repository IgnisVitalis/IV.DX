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
-- Table structure for table `TPositionGenBlock`
--

DROP TABLE IF EXISTS `TPositionGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TPositionGenBlock` (
  `ID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `TPositionObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID` (`ID`),
  UNIQUE KEY `TPositionObjectID_unique` (`TPositionObjectID`),
  KEY `FK_TPositionGenBlock_TPositionObject_0000_idx` (`TPositionObjectID`),
  CONSTRAINT `FK_TPositionGenBlock_TPositionObject_0000` FOREIGN KEY (`TPositionObjectID`) REFERENCES `TPositionObject` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `TPositionGenBlock`
--

LOCK TABLES `TPositionGenBlock` WRITE;
/*!40000 ALTER TABLE `TPositionGenBlock` DISABLE KEYS */;
INSERT INTO `TPositionGenBlock` VALUES ('adaeaf9b-f976-4756-aacd-b46c5d899352','3040fe09-2ec2-4472-ae32-724f028b374e','Master','3040fe09-2ec2-4472-ae32-724f028b374e'),('b0bb2cf1-eb72-46a4-b7c1-0dba34b9468e','14c92f6a-88b7-4d72-9777-5f655f0914bf','Middle','14c92f6a-88b7-4d72-9777-5f655f0914bf'),('bdfbbf29-6432-4809-94f8-cb2cfa2bbb3d','1f852be3-f851-4807-807f-cfd45a0b4093','Junior','1f852be3-f851-4807-807f-cfd45a0b4093');
/*!40000 ALTER TABLE `TPositionGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:47
