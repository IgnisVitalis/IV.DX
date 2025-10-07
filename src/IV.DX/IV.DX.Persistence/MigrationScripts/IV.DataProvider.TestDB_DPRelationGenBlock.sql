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
-- Table structure for table `DPRelationGenBlock`
--

DROP TABLE IF EXISTS `DPRelationGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DPRelationGenBlock` (
  `ID` char(36) NOT NULL,
  `ObjectID` char(36) NOT NULL,
  `DPRelationObjectID` char(36) DEFAULT NULL,
  `ObjectNameLeft` varchar(100) NOT NULL,
  `RelationNameLeft` varchar(100) NOT NULL,
  `ObjectNameRight` varchar(100) NOT NULL,
  `RelationNameRight` varchar(100) NOT NULL,
  `RelationTable` varchar(100) DEFAULT NULL,
  `RelationType` int NOT NULL,
  `RevertedRelationObjectID` char(36) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  UNIQUE KEY `ObjectID_UNIQUE` (`ObjectID`),
  UNIQUE KEY `ObjectNameLeft_RelationNameRight_UNIQUE` (`ObjectNameLeft`,`RelationNameRight`),
  UNIQUE KEY `ObjectNameRight_RelationNameLeft_UNIQUE` (`ObjectNameRight`,`RelationNameLeft`),
  KEY `FK_DPRelationGenBlock_DPRelationObject_0000_idx` (`DPRelationObjectID`),
  KEY `FK_DPRelationGenBlock_DPRelationTypeEnum_0000_idx` (`RelationType`),
  CONSTRAINT `FK_DPRelationGenBlock_DPRelationObject_0000` FOREIGN KEY (`DPRelationObjectID`) REFERENCES `DPRelationObject` (`ID`),
  CONSTRAINT `FK_DPRelationGenBlock_DPRelationTypeEnum_0000` FOREIGN KEY (`RelationType`) REFERENCES `DPRelationTypeEnum` (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DPRelationGenBlock`
--

LOCK TABLES `DPRelationGenBlock` WRITE;
/*!40000 ALTER TABLE `DPRelationGenBlock` DISABLE KEYS */;
INSERT INTO `DPRelationGenBlock` VALUES ('0bb4b170-9935-40ad-b143-66b42dc28e29','f15f71da-ab49-4937-911a-58170f32da30','f15f71da-ab49-4937-911a-58170f32da30','TUserObject','User','TPassportObject','Passport',NULL,1,NULL),('159eeff6-1bbb-4589-9a51-096f297e955b','0ac26de8-ceeb-457f-b6fb-9cfa52b869eb','0ac26de8-ceeb-457f-b6fb-9cfa52b869eb','TPositionObject','Position','TUserObject','User','TPositionObject',8,NULL),('3644a125-dc3a-4bd8-9b42-39ee33e8f911','8ab5ba94-ca7c-47ef-99a8-dfb9c020af92','8ab5ba94-ca7c-47ef-99a8-dfb9c020af92','TUserObject','User','TPositionObject','Position','TPositionObject',8,NULL),('63a551ea-a013-4e02-9da5-73ad8be7f4f6','992bfb48-016f-41c7-a9a5-413256388d8a','992bfb48-016f-41c7-a9a5-413256388d8a','TDeviceObject','Devices','TUserObject','User',NULL,4,NULL),('6826a7ea-e2c1-463d-a3c9-22efbe118207','e799893d-0943-4902-86aa-9a21747cf764','e799893d-0943-4902-86aa-9a21747cf764','TUserObject','User','TDocumentObject','Documents',NULL,5,NULL),('6da17907-5654-493f-9346-99501dc97e46','51ca8d14-fe24-4653-a163-74743f76d156','51ca8d14-fe24-4653-a163-74743f76d156','TPassportObject','Passport','TUserObject','User',NULL,2,NULL),('88407e07-3123-40c3-9d7b-d897f5927b1e','93b257ed-21f5-421c-8c0d-48cb15b02943','93b257ed-21f5-421c-8c0d-48cb15b02943','TBookObject','Books','TUserObject','Users','Relation_TUserObject_TBookObject_0',7,NULL),('9e54b118-f8b5-4e67-b6f5-0adc81ad278f','46313bf6-7911-451f-9228-47a0c37daed2','46313bf6-7911-451f-9228-47a0c37daed2','TDocumentObject','Documents','TUserObject','User',NULL,6,NULL),('c679329f-c595-43ea-a866-536d7c80add3','ef5e9942-c1bb-4637-97a1-b95b2f843a50','ef5e9942-c1bb-4637-97a1-b95b2f843a50','TUserObject','Users','TBookObject','Books','Relation_TUserObject_TBookObject_0',7,NULL),('c74741b4-8c58-4271-bc6a-de92b763bcd9','3e9be76e-a2d2-4ff4-9c93-8a4df4846066','3e9be76e-a2d2-4ff4-9c93-8a4df4846066','TUserObject','User','TDeviceObject','Devices',NULL,3,NULL),('d119a60e-93b7-4dc7-859b-48838e7962cd','9995faae-0c64-4179-8562-d41c5269848d','9995faae-0c64-4179-8562-d41c5269848d','TPassportObject','User','TUserObject','Passport',NULL,2,NULL),('e91b8765-3eb0-4131-947d-22c9c9168efa','ca62700d-107d-4257-928a-871ff9fbdff2','ca62700d-107d-4257-928a-871ff9fbdff2','TPassportObject','RelTableRightRelation','TUserObject','RelTableLeftRelation',NULL,2,NULL);
/*!40000 ALTER TABLE `DPRelationGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:52
