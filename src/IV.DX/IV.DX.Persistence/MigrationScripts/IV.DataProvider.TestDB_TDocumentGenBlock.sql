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
-- Table structure for table `TDocumentGenBlock`
--

DROP TABLE IF EXISTS `TDocumentGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TDocumentGenBlock` (
  `ID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `TDocumentObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID` (`ID`),
  UNIQUE KEY `TDocumentObjectID_unique` (`TDocumentObjectID`),
  KEY `FK_TDocumentGenObject_TDocumentObject_0000_idx` (`TDocumentObjectID`),
  CONSTRAINT `FK_TDocumentGenBlock_TDocumentObject_0000` FOREIGN KEY (`TDocumentObjectID`) REFERENCES `TDocumentObject` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `TDocumentGenBlock`
--

LOCK TABLES `TDocumentGenBlock` WRITE;
/*!40000 ALTER TABLE `TDocumentGenBlock` DISABLE KEYS */;
INSERT INTO `TDocumentGenBlock` VALUES ('15ce8ddc-ad58-4403-a90b-8bd94ffb65c1','ce7a2422-7df4-426a-b1fe-2a2090443246','document1','ce7a2422-7df4-426a-b1fe-2a2090443246'),('21e94f39-48e6-4c5a-97d0-3b5092fd1fff','02e1591c-375b-466c-b8fb-0bed19220707','document4','02e1591c-375b-466c-b8fb-0bed19220707'),('348e16f5-e24e-4a10-80c7-e6a69542cf68','a844e32e-fcf3-4f7e-b138-19685347a150','document2','a844e32e-fcf3-4f7e-b138-19685347a150'),('c0fa7b4c-b0a6-43f3-aadb-03101885d5c9','ccb9da2b-12ea-41c0-96d1-a774a3f4b22b','document3','ccb9da2b-12ea-41c0-96d1-a774a3f4b22b'),('e3875c37-21ea-4699-8ac9-de4644b21e2e','c2caacbe-f9c8-4409-8c65-535a3b530a3d','document6','c2caacbe-f9c8-4409-8c65-535a3b530a3d'),('e56651b1-707f-4632-8aeb-fb646aaf3aa5','6a7c4e0d-1163-41ca-8a1a-e25fe8797100','document5','6a7c4e0d-1163-41ca-8a1a-e25fe8797100');
/*!40000 ALTER TABLE `TDocumentGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:42
