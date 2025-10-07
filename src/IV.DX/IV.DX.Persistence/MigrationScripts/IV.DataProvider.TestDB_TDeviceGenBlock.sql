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
-- Table structure for table `TDeviceGenBlock`
--

DROP TABLE IF EXISTS `TDeviceGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TDeviceGenBlock` (
  `ID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Model` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `TDeviceObjectID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UUID` char(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID` (`ID`),
  UNIQUE KEY `TDeviceObjectID_unique` (`TDeviceObjectID`),
  KEY `FK_TDeviceGenBlock_TDeviceObject_0000_idx` (`TDeviceObjectID`),
  CONSTRAINT `FK_TDeviceGenBlock_TDeviceObject_0000` FOREIGN KEY (`TDeviceObjectID`) REFERENCES `TDeviceObject` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `TDeviceGenBlock`
--

LOCK TABLES `TDeviceGenBlock` WRITE;
/*!40000 ALTER TABLE `TDeviceGenBlock` DISABLE KEYS */;
INSERT INTO `TDeviceGenBlock` VALUES ('3711aaac-1062-4aee-982b-12007df360a3','24d8f6ff-b411-4acc-8a35-5e958ce7f070','Model5','24d8f6ff-b411-4acc-8a35-5e958ce7f070','9966eb62-5e20-4a49-9eb1-e54614abe807'),('70102d16-a2be-4a9f-ba40-a1146eb30a3c','a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7','Model1','a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7','70f86100-bc9c-4b88-8f5f-759cedf85972'),('7e429895-0220-41df-8688-42ec952ebd63','53ced1ab-2582-4aee-b2bc-50e676eebde3','Model2','53ced1ab-2582-4aee-b2bc-50e676eebde3','487704ea-ee63-41be-9e01-d1841dd472b8'),('93ae5489-dc76-459f-b57a-708e1190e966','1c16f974-8e52-408b-9cac-acbb548864fa','Model6','1c16f974-8e52-408b-9cac-acbb548864fa','6b9cab10-692f-4f4c-81b7-570a40d2b561'),('b2a0bdd7-5c55-4cbc-bb72-9961777d37b8','58a98dbf-ce5d-43d1-adb2-670dea20c7bf','Model3','58a98dbf-ce5d-43d1-adb2-670dea20c7bf','75d78874-5f39-4e22-bc30-ccc6743f4622'),('c789c65e-783e-421f-a18d-1594bd321964','36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b','Model4','36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b','8f030336-4861-4d9c-980a-38674fa2dcf5');
/*!40000 ALTER TABLE `TDeviceGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:51
