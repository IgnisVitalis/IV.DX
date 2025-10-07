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
-- Table structure for table `DPBlockInObjectDescGenBlock`
--

DROP TABLE IF EXISTS `DPBlockInObjectDescGenBlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `DPBlockInObjectDescGenBlock` (
  `ID` char(36) NOT NULL,
  `ObjectID` char(36) NOT NULL,
  `DPBlockInObjectDescObjectID` char(36) DEFAULT NULL,
  `DPBlockInObjectTypeEnum` int NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`),
  KEY `FK_DPBlockInObjectDescGenBlock_DPBlockInObjectTypeEnum_0000_idx` (`DPBlockInObjectTypeEnum`),
  CONSTRAINT `FK_DPBlockInObjectDescGenBlock_DPBlockInObjectTypeEnum_0000` FOREIGN KEY (`DPBlockInObjectTypeEnum`) REFERENCES `DPBlockInObjectTypeEnum` (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `DPBlockInObjectDescGenBlock`
--

LOCK TABLES `DPBlockInObjectDescGenBlock` WRITE;
/*!40000 ALTER TABLE `DPBlockInObjectDescGenBlock` DISABLE KEYS */;
INSERT INTO `DPBlockInObjectDescGenBlock` VALUES ('07fb6348-a3f8-4b6c-924d-f6533316156e','db3cdce4-a71f-4129-8d1b-a1b8662ad1dd','db3cdce4-a71f-4129-8d1b-a1b8662ad1dd',1),('3109ea24-f209-4aac-9414-e7f3493aa41a','a132f7ef-5bf2-4ebf-bc51-c95f6eddd78c','a132f7ef-5bf2-4ebf-bc51-c95f6eddd78c',1),('326b0dfe-b738-4828-98f1-f9cba1e1f58c','b21e21ca-d919-49e7-b0ca-6fbc0badaefa','b21e21ca-d919-49e7-b0ca-6fbc0badaefa',1),('55a9d478-e4ef-4bb2-8743-290216b22979','7f052b48-7008-4418-84fe-e51d42e2170d','7f052b48-7008-4418-84fe-e51d42e2170d',1),('963b1a70-bff1-49c4-8360-f7c34c02b2cb','f9bf6850-49e3-4515-9299-4a9f07674b22','f9bf6850-49e3-4515-9299-4a9f07674b22',4),('d598414d-5c48-40c5-ba7a-b56a871e62b5','fc4ec5ba-6371-4824-8123-603b62df32f4','fc4ec5ba-6371-4824-8123-603b62df32f4',1),('fd309653-967e-40e1-862f-92b906689d70','72f5d23f-2fd5-44ac-886a-3da7dd3f70ea','72f5d23f-2fd5-44ac-886a-3da7dd3f70ea',1);
/*!40000 ALTER TABLE `DPBlockInObjectDescGenBlock` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:40
