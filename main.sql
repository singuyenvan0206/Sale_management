CREATE DATABASE  IF NOT EXISTS `main` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `main`;
-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: localhost    Database: main
-- ------------------------------------------------------
-- Server version	8.0.45

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
-- Table structure for table `accounts`
--

DROP TABLE IF EXISTS `accounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accounts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(255) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Role` varchar(20) NOT NULL DEFAULT 'Cashier',
  `employeeName` varchar(100) DEFAULT NULL,
  `LastLoginDate` datetime DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Username` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `categories`
--

DROP TABLE IF EXISTS `categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `TaxPercent` decimal(5,2) NOT NULL DEFAULT '0.00',
  `Description` text,
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Address` text,
  `CustomerType` varchar(50) DEFAULT 'Regular',
  `Points` int NOT NULL DEFAULT '0',
  `UpdatedDate` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `TotalSpent` decimal(15,2) DEFAULT '0.00',
  `DateOfBirth` date DEFAULT NULL,
  `Gender` varchar(10) DEFAULT NULL,
  `LastPurchaseDate` datetime DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=301 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `customervoucherusage`
--

DROP TABLE IF EXISTS `customervoucherusage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customervoucherusage` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CustomerId` int NOT NULL,
  `VoucherId` int NOT NULL,
  `InvoiceId` int DEFAULT NULL,
  `DiscountAmount` decimal(12,2) NOT NULL,
  `UsedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `InvoiceId` (`InvoiceId`),
  KEY `idx_customer` (`CustomerId`),
  KEY `idx_voucher` (`VoucherId`),
  KEY `idx_used_date` (`UsedDate`),
  CONSTRAINT `customervoucherusage_ibfk_1` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `customervoucherusage_ibfk_2` FOREIGN KEY (`VoucherId`) REFERENCES `vouchers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `customervoucherusage_ibfk_3` FOREIGN KEY (`InvoiceId`) REFERENCES `invoices` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `featureflags`
--

DROP TABLE IF EXISTS `featureflags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `featureflags` (
  `Key` varchar(100) NOT NULL,
  `IsEnabled` tinyint(1) NOT NULL DEFAULT '1',
  `UpdatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invoiceitems`
--

DROP TABLE IF EXISTS `invoiceitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoiceitems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InvoiceId` int NOT NULL,
  `ProductId` int NOT NULL,
  `ProductName` varchar(255) DEFAULT NULL,
  `ProductCode` varchar(50) DEFAULT NULL,
  `EmployeeId` int NOT NULL,
  `UnitPrice` decimal(12,2) NOT NULL,
  `Quantity` int NOT NULL,
  `LineTotal` decimal(12,2) NOT NULL,
  `DiscountPercent` decimal(5,2) DEFAULT '0.00',
  `DiscountAmount` decimal(12,2) DEFAULT '0.00',
  PRIMARY KEY (`Id`),
  KEY `InvoiceId` (`InvoiceId`),
  KEY `ProductId` (`ProductId`),
  KEY `EmployeeId` (`EmployeeId`),
  CONSTRAINT `invoiceitems_ibfk_1` FOREIGN KEY (`InvoiceId`) REFERENCES `invoices` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `invoiceitems_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`),
  CONSTRAINT `invoiceitems_ibfk_3` FOREIGN KEY (`EmployeeId`) REFERENCES `accounts` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invoices`
--

DROP TABLE IF EXISTS `invoices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoices` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InvoiceNumber` varchar(50) DEFAULT NULL,
  `CustomerId` int NOT NULL,
  `EmployeeId` int NOT NULL,
  `Subtotal` decimal(12,2) NOT NULL,
  `TaxPercent` decimal(5,2) NOT NULL DEFAULT '0.00',
  `TaxAmount` decimal(12,2) NOT NULL DEFAULT '0.00',
  `DiscountAmount` decimal(15,2) NOT NULL DEFAULT '0.00',
  `Total` decimal(12,2) NOT NULL,
  `Paid` decimal(12,2) NOT NULL DEFAULT '0.00',
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `VoucherId` int DEFAULT NULL,
  `VoucherDiscount` decimal(15,2) DEFAULT '0.00',
  `TierDiscount` decimal(15,2) DEFAULT '0.00',
  `PaymentMethod` varchar(50) DEFAULT 'Cash',
  `PaymentStatus` varchar(20) DEFAULT 'Paid',
  `ChangeAmount` decimal(15,2) DEFAULT '0.00',
  `Notes` text,
  `Status` varchar(20) DEFAULT 'Completed',
  `UpdatedDate` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `CompletedDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `InvoiceNumber` (`InvoiceNumber`),
  KEY `CustomerId` (`CustomerId`),
  KEY `EmployeeId` (`EmployeeId`),
  KEY `VoucherId` (`VoucherId`),
  CONSTRAINT `invoices_ibfk_1` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`),
  CONSTRAINT `invoices_ibfk_2` FOREIGN KEY (`EmployeeId`) REFERENCES `accounts` (`Id`),
  CONSTRAINT `invoices_ibfk_3` FOREIGN KEY (`VoucherId`) REFERENCES `vouchers` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Code` varchar(50) DEFAULT NULL,
  `CategoryId` int DEFAULT NULL,
  `SalePrice` decimal(10,2) NOT NULL,
  `PromoDiscountPercent` decimal(5,2) NOT NULL DEFAULT '0.00',
  `PromoStartDate` datetime DEFAULT NULL,
  `PromoEndDate` datetime DEFAULT NULL,
  `PurchasePrice` decimal(10,2) DEFAULT '0.00',
  `PurchaseUnit` varchar(50) DEFAULT 'Cái',
  `ImportQuantity` int DEFAULT '0',
  `StockQuantity` int NOT NULL DEFAULT '0',
  `Description` text,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `UpdatedDate` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `SupplierId` int DEFAULT '0',
  `MinStockLevel` int DEFAULT '10',
  `MaxStockLevel` int DEFAULT '1000',
  `IsActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Code` (`Code`),
  KEY `CategoryId` (`CategoryId`),
  CONSTRAINT `products_ibfk_1` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=201 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `promotions`
--

DROP TABLE IF EXISTS `promotions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promotions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Type` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'FlashSale, BOGO, Combo',
  `StartDate` datetime NOT NULL,
  `EndDate` datetime NOT NULL,
  `DiscountPercent` decimal(5,2) DEFAULT '0.00',
  `DiscountAmount` decimal(12,2) DEFAULT '0.00',
  `RequiredProductId` int DEFAULT NULL,
  `RequiredQuantity` int DEFAULT '0',
  `RewardProductId` int DEFAULT NULL,
  `RewardQuantity` int DEFAULT '0',
  `TargetCategoryId` int DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `RequiredProductId` (`RequiredProductId`),
  KEY `RewardProductId` (`RewardProductId`),
  KEY `TargetCategoryId` (`TargetCategoryId`),
  CONSTRAINT `promotions_ibfk_1` FOREIGN KEY (`RequiredProductId`) REFERENCES `products` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `promotions_ibfk_2` FOREIGN KEY (`RewardProductId`) REFERENCES `products` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `promotions_ibfk_3` FOREIGN KEY (`TargetCategoryId`) REFERENCES `categories` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `stockmovements`
--

DROP TABLE IF EXISTS `stockmovements`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stockmovements` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `MovementType` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Import, Sale, Adjustment, Return, Transfer',
  `Quantity` int NOT NULL COMMENT 'Positive for increase, negative for decrease',
  `PreviousStock` int NOT NULL,
  `NewStock` int NOT NULL,
  `ReferenceType` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Invoice, PurchaseOrder, Adjustment, etc.',
  `ReferenceId` int DEFAULT NULL,
  `Notes` text COLLATE utf8mb4_unicode_ci,
  `EmployeeId` int DEFAULT NULL,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `EmployeeId` (`EmployeeId`),
  KEY `idx_product` (`ProductId`),
  KEY `idx_created_date` (`CreatedDate`),
  KEY `idx_movement_type` (`MovementType`),
  CONSTRAINT `stockmovements_ibfk_1` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `stockmovements_ibfk_2` FOREIGN KEY (`EmployeeId`) REFERENCES `accounts` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `suppliers`
--

DROP TABLE IF EXISTS `suppliers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `suppliers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `ContactName` varchar(255) DEFAULT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Address` text,
  `TaxCode` varchar(50) DEFAULT NULL,
  `Website` varchar(255) DEFAULT NULL,
  `Notes` text,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `vouchers`
--

DROP TABLE IF EXISTS `vouchers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vouchers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(255) DEFAULT NULL,
  `DiscountType` varchar(20) NOT NULL,
  `DiscountValue` decimal(12,2) NOT NULL,
  `MinInvoiceAmount` decimal(12,2) NOT NULL DEFAULT '0.00',
  `StartDate` datetime NOT NULL,
  `EndDate` datetime NOT NULL,
  `UsageLimit` int NOT NULL DEFAULT '0',
  `UsedCount` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `MaxDiscountAmount` decimal(12,2) DEFAULT NULL,
  `UsageLimitPerCustomer` int DEFAULT '0',
  `ApplicableCategories` text,
  `ApplicableProducts` text,
  `ApplicableCustomerTypes` varchar(255) DEFAULT NULL,
  `CreatedBy` int DEFAULT NULL,
  `CreatedDate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-04-13 14:47:02
