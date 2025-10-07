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
-- Table structure for table `DPObjectDescGenBlock`
--

DROP TABLE IF EXISTS `DPObjectDescGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DPObjectDescGenBlock` (
  `ID` char(36) NOT NULL,
  `ObjectID` char(36) NOT NULL,
  `DPObjectDescObjectID` char(36) DEFAULT NULL,
  `Name` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Type` int NOT NULL,
  `DisplayValue` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Kind` int NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  UNIQUE KEY `ObjectID_UNIQUE` (`ObjectID`),
  KEY `FK_DPObjectDescGenBlock_DPObjectDescObject_0000_idx` (`DPObjectDescObjectID`),
  KEY `FK_DPObjectDescGenBlock_DPObjectTypeEnum_0000_idx` (`Type`),
  KEY `FK_DPObjectDescGenBlock_DPObjectKindEnum_0000_idx` (`Kind`),
  CONSTRAINT `FK_DPObjectDescGenBlock_DPObjectDescObject_0000` FOREIGN KEY (`DPObjectDescObjectID`) REFERENCES `DPObjectDescObject` (`ID`),
  CONSTRAINT `FK_DPObjectDescGenBlock_DPObjectKindEnum_0000` FOREIGN KEY (`Kind`) REFERENCES `DPObjectKindEnum` (`Key`),
  CONSTRAINT `FK_DPObjectDescGenBlock_DPObjectTypeEnum_0000` FOREIGN KEY (`Type`) REFERENCES `DPObjectTypeEnum` (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DPObjectDescGenBlock`
--

LOCK TABLES `DPObjectDescGenBlock` WRITE;
/*!40000 ALTER TABLE `DPObjectDescGenBlock` DISABLE KEYS */;
INSERT INTO `DPObjectDescGenBlock` VALUES ('06560692-98ca-41e6-9763-331a2aed5a1f','28644277-705c-4666-8b7b-e33066e2ea27','28644277-705c-4666-8b7b-e33066e2ea27','TBookChapterBlock',1,NULL,2),('0a0245c5-65ba-4be1-8747-da4a72974053','408a680c-4012-4dc8-ad8f-2676f699734f','408a680c-4012-4dc8-ad8f-2676f699734f','TPositionGenBlock',1,NULL,2),('2218f1da-5c87-424f-843d-964fa095f480','714ee242-8821-4a4f-a28e-c623004d49a4','714ee242-8821-4a4f-a28e-c623004d49a4','TPassportGenBlock',1,NULL,2),('29f74c7a-4d82-4140-a88e-1a25a68e67ef','8e2c5365-85c0-431d-996e-fbccfe3f856a','8e2c5365-85c0-431d-996e-fbccfe3f856a','TPassportObject',2,NULL,2),('33b7bf0d-5761-49eb-9d0c-1c6b82fb9304','394566d6-93e4-446a-800d-2209898475ac','394566d6-93e4-446a-800d-2209898475ac','TPositionObject',2,NULL,2),('57a80968-3195-427b-a49d-8ba69d472f2c','37099cee-e2cd-4d86-bece-8e7a11a96da2','37099cee-e2cd-4d86-bece-8e7a11a96da2','TDeviceGenBlock',1,NULL,2),('6244f906-587d-4a2c-b3a1-54b90a110e9a','29a4d2f3-0f2a-4a60-a12e-8c4dd1af8476','29a4d2f3-0f2a-4a60-a12e-8c4dd1af8476','TDocumentGenBlock',1,NULL,2),('6e6839b6-8dc6-4e15-86b4-0767f3042f11','0ccee9e3-67cb-4692-940c-41929f9df7b0','0ccee9e3-67cb-4692-940c-41929f9df7b0','TBookGenBlock',1,NULL,2),('a18524e1-e537-4355-b860-5809727b2e3e','356aaa53-fc71-41dd-90a0-53975d938cf9','356aaa53-fc71-41dd-90a0-53975d938cf9','TDeviceObject',2,NULL,2),('a3a03877-5021-4bb9-9020-ef017b69ce48','3dae1265-e917-4b91-b4c3-f3f835281630','3dae1265-e917-4b91-b4c3-f3f835281630','TDocumentObject',2,NULL,2),('b0f45798-4fc0-48e4-b791-a31e36d16e3b','1faf325f-57bc-4ab2-bb3c-03a6ab5ae859','1faf325f-57bc-4ab2-bb3c-03a6ab5ae859','TUserObject',2,NULL,2),('b2561ad0-c5ef-40dd-b460-c7c1330b3e54','6555d7f8-27a6-495d-91e3-df0a49354032','6555d7f8-27a6-495d-91e3-df0a49354032','TBookObject',2,NULL,2),('bea190b6-4138-4775-9157-f3b15ac9d51e','515b9785-6bbc-40b6-8af6-2d862d15b60b','515b9785-6bbc-40b6-8af6-2d862d15b60b','TUserGenBlock',1,NULL,2);
/*!40000 ALTER TABLE `DPObjectDescGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:46
