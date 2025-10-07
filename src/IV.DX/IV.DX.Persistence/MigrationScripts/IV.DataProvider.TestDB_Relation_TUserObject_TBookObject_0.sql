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
-- Table structure for table `Relation_TUserObject_TBookObject_0`
--

DROP TABLE IF EXISTS `Relation_TUserObject_TBookObject_0`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Relation_TUserObject_TBookObject_0` (
  `Users` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Books` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Users`,`Books`),
  KEY `FK_Relation_TUserObject_TBookObject_0_TBookObject` (`Books`),
  CONSTRAINT `FK_Relation_TUserObject_TBookObject_0_TBookObject` FOREIGN KEY (`Books`) REFERENCES `TBookObject` (`ID`),
  CONSTRAINT `FK_Relation_TUserObject_TBookObject_0_TUserObject` FOREIGN KEY (`Users`) REFERENCES `TUserObject` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Relation_TUserObject_TBookObject_0`
--

LOCK TABLES `Relation_TUserObject_TBookObject_0` WRITE;
/*!40000 ALTER TABLE `Relation_TUserObject_TBookObject_0` DISABLE KEYS */;
INSERT INTO `Relation_TUserObject_TBookObject_0` VALUES ('8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3','1b51edff-1d99-4043-9a69-209996729b69'),('dfb7bb88-30d9-46d7-9885-6ca8ae455e82','1b51edff-1d99-4043-9a69-209996729b69'),('dfb7bb88-30d9-46d7-9885-6ca8ae455e82','4782b530-6343-4d11-846a-65127cf71f3b');
/*!40000 ALTER TABLE `Relation_TUserObject_TBookObject_0` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-02 19:00:32
