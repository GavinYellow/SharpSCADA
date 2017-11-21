/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `dictionary` (
  `DictType` varchar(50) COLLATE latin1_german1_ci NOT NULL DEFAULT '',
  `Code` varchar(50) COLLATE latin1_german1_ci NOT NULL DEFAULT '',
  `Description` varchar(50) COLLATE latin1_german1_ci DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `log_alarm` (
  `StartTime` datetime DEFAULT NULL,
  `Source` varchar(50) COLLATE latin1_german1_ci DEFAULT '',
  `ConditionID` int(11) DEFAULT '0',
  `AlarmText` varchar(128) COLLATE latin1_german1_ci DEFAULT '',
  `AlarmValue` text COLLATE latin1_german1_ci,
  `Duration` int(11) DEFAULT '0',
  `Severity` int(11) DEFAULT '0',
  `SubAlarmType` int(11) DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `log_event` (
  `EventType` int(11) DEFAULT NULL,
  `Severity` int(11) DEFAULT NULL,
  `IsAcked` bit(1) DEFAULT NULL,
  `ActiveTime` datetime DEFAULT NULL,
  `Source` varchar(50) DEFAULT NULL,
  `Comment` varchar(50) DEFAULT NULL,
  `SQLCounter` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`SQLCounter`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `log_hdata` (
  `ID` int(11) NOT NULL DEFAULT '0',
  `TimeStamp` datetime NOT NULL,
  `Value` text COLLATE latin1_german1_ci
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `membership` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `UserName` varchar(50) COLLATE latin1_german1_ci NOT NULL DEFAULT '',
  `Password` varchar(50) COLLATE latin1_german1_ci NOT NULL DEFAULT '',
  `Role` int(11) NOT NULL DEFAULT '0',
  `Email` varchar(50) COLLATE latin1_german1_ci DEFAULT '',
  `Phone` varchar(50) COLLATE latin1_german1_ci DEFAULT '',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
INSERT INTO `membership` VALUES (1,'admin','c4ca4238a0b923820dcc509a6f75849b',4,NULL,NULL),(2,'op','c4ca4238a0b923820dcc509a6f75849b',1,NULL,NULL),(3,'everyone','c4ca4238a0b923820dcc509a6f75849b',1,NULL,NULL);
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `meta_condition` (
  `TypeID` int(11) NOT NULL AUTO_INCREMENT,
  `Source` varchar(50) COLLATE latin1_german1_ci NOT NULL DEFAULT '',
  `AlarmType` int(11) NOT NULL DEFAULT '0',
  `EventType` tinyint(4) NOT NULL DEFAULT '0',
  `ConditionType` tinyint(4) NOT NULL DEFAULT '0',
  `Para` text COLLATE latin1_german1_ci,
  `IsEnabled` tinyint(4) NOT NULL DEFAULT '1',
  `DeadBand` text COLLATE latin1_german1_ci,
  `Delay` int(11) NOT NULL DEFAULT '0',
  `Comment` varchar(50) COLLATE latin1_german1_ci DEFAULT '',
  PRIMARY KEY (`TypeID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `meta_driver` (
  `DriverID` int(11) NOT NULL DEFAULT '0',
  `DriverType` int(11) NOT NULL DEFAULT '0',
  `DriverName` varchar(64) COLLATE latin1_german1_ci NOT NULL DEFAULT '',
  `TimeOut` int(11) NOT NULL DEFAULT '0',
  `Server` varchar(128) COLLATE latin1_german1_ci DEFAULT '',
  `Spare1` varchar(50) COLLATE latin1_german1_ci DEFAULT '',
  `Spare2` varchar(50) COLLATE latin1_german1_ci DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
INSERT INTO `meta_driver` VALUES (1,3,'S1',1000,'127.0.0.1','{6E6170F0-FF2D-11D2-8087-00105AA8F840}','9600'),(2,5,'Modbus',1000,'127.0.0.1','','2');
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `meta_group` (
  `GroupID` int(11) NOT NULL DEFAULT '0',
  `DriverID` int(11) DEFAULT '0',
  `GroupName` varchar(20) COLLATE latin1_german1_ci DEFAULT '',
  `UpdateRate` int(11) DEFAULT '0',
  `DeadBand` text COLLATE latin1_german1_ci,
  `IsActive` tinyint(4) NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
INSERT INTO `meta_group` VALUES (20001,1,'Receiving1',300,'0',0),(20002,1,'Receiving2',0,'0',0),(20003,2,'test',1000,'0',1);
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `meta_scale` (
  `ScaleID` int(11) NOT NULL DEFAULT '0',
  `ScaleType` tinyint(4) NOT NULL DEFAULT '0',
  `EUHi` text COLLATE latin1_german1_ci,
  `EULo` text COLLATE latin1_german1_ci,
  `RawHi` text COLLATE latin1_german1_ci,
  `RawLo` text COLLATE latin1_german1_ci
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `meta_subcondition` (
  `ConditionID` int(11) NOT NULL DEFAULT '0',
  `SubAlarmType` int(11) NOT NULL DEFAULT '0',
  `Threshold` text COLLATE latin1_german1_ci,
  `Severity` tinyint(4) NOT NULL DEFAULT '0',
  `Message` varchar(250) COLLATE latin1_german1_ci DEFAULT '',
  `IsEnable` tinyint(4) NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `meta_tag` (
  `TagID` smallint(5) NOT NULL AUTO_INCREMENT,
  `TagName` varchar(512) NOT NULL,
  `DataType` tinyint(3) unsigned NOT NULL,
  `DataSize` smallint(5) NOT NULL DEFAULT '0',
  `Address` varchar(64) NOT NULL,
  `GroupID` smallint(5) NOT NULL DEFAULT '0',
  `IsActive` bit(1) NOT NULL,
  `Archive` bit(1) NOT NULL,
  `DefaultValue` blob,
  `Description` varchar(128) DEFAULT NULL,
  `Maximum` double(24,2) NOT NULL DEFAULT '0.00',
  `Minimum` double(24,2) NOT NULL DEFAULT '0.00',
  `Cycle` int(10) NOT NULL DEFAULT '0',
  `RowVersion` blob NOT NULL,
  UNIQUE KEY `TagID` (`TagID`)
) ENGINE=InnoDB AUTO_INCREMENT=159 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
INSERT INTO `meta_tag` VALUES (2,'Receiving1_AlmAck',1,1,'Channel4.Receiving1.K0008.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªÆ'),(3,'Receiving1_888',1,1,'Channel4.Receiving1.K0006.14',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªØ'),(4,'Receiving1_Conveyor3_Running',1,1,'Channel4.Receiving1.K0006.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª∞'),(5,'Receiving1_Conveyor4_Alarm',1,1,'Channel4.Receiving1.K0001.04',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª±'),(6,'Receiving1_Conveyor4_Running',1,1,'Channel4.Receiving1.K0001.03',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª≤'),(7,'Receiving1_Conveyor5_Alarm',1,1,'Channel4.Receiving1.K0008.00',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª≥'),(8,'Receiving1_Conveyor5_Running',1,1,'Channel4.Receiving1.K0007.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª¥'),(9,'Receiving1_Conveyor6_Alarm',1,1,'Channel4.Receiving1.K0008.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªµ'),(10,'Receiving1_Conveyor6_Running',1,1,'Channel4.Receiving1.K0005.14',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª∂'),(11,'Receiving1_Conveyor7_Alarm',1,1,'Channel4.Receiving1.K0006.13',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª∑'),(12,'Receiving1_Conveyor7_Running',1,1,'Channel4.Receiving1.K0006.12',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª∏'),(13,'Receiving1_Conveyor8_Running',1,1,'Channel4.Receiving1.K0001.12',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªπ'),(14,'Receiving1_Conveyor9_Alarm',1,1,'Channel4.Receiving1.K0001.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª∫'),(15,'Receiving1_Conveyor9_Running',1,1,'Channel4.Receiving1.K0001.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªª'),(16,'Receiving1_DF01SQH_Alarm',1,1,'Channel4.Receiving1.K0002.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªº'),(17,'Receiving1_DF01SQL_Alarm',1,1,'Channel4.Receiving1.K0003.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªΩ'),(18,'Receiving1_DF02SQH_Alarm',1,1,'Channel4.Receiving1.K0002.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªæ'),(19,'Receiving1_DF02SQL_Alarm',1,1,'Channel4.Receiving1.K0003.03',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªø'),(20,'Receiving1_DF03SQH_Alarm',1,1,'Channel4.Receiving1.K0002.04',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª¿'),(21,'Receiving1_DF03SQL_Alarm',1,1,'Channel4.Receiving1.K0003.04',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª¡'),(22,'Receiving1_DF04SQH_Alarm',1,1,'Channel4.Receiving1.K0002.03',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\¬'),(23,'Receiving1_DF04SQL_Alarm',1,1,'Channel4.Receiving1.K0003.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\√'),(24,'Receiving1_DF05SQH_Alarm',1,1,'Channel4.Receiving1.K0002.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\ƒ'),(25,'Receiving1_DF05SQL_Alarm',1,1,'Channel4.Receiving1.K0003.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\≈'),(26,'Receiving1_DF06SQL_Alarm',1,1,'Channel4.Receiving1.K0002.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\∆'),(27,'Receiving1_F01SQH_Alarm',1,1,'Channel4.Receiving1.K0007.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\«'),(28,'Receiving1_F02SQH_Alarm',1,1,'Channel4.Receiving1.K0007.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\»'),(29,'Receiving1_F03SQH_Alarm',1,1,'Channel4.Receiving1.K0007.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\…'),(30,'Receiving1_F04SQH_Alarm',1,1,'Channel4.Receiving1.K0007.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\ '),(31,'Receiving1_F05SQH_Alarm',1,1,'Channel4.Receiving1.K0007.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\À'),(32,'Receiving1_F06SQH_Alarm',1,1,'Channel4.Receiving1.K0007.03',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ã'),(33,'Receiving1_Fan1_Alarm',1,1,'Channel4.Receiving1.K0008.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Õ'),(34,'Receiving1_Fan1_Running',1,1,'Channel4.Receiving1.K0008.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Œ'),(35,'Receiving1_Fan2_Running',1,1,'Channel4.Receiving1.K0008.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\œ'),(36,'Receiving1_Fan3_Running',1,1,'Channel4.Receiving1.K0001.13',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\–'),(37,'Receiving1_FourWays_Left',1,1,'Channel4.Receiving1.K0006.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\—'),(38,'Receiving1_FourWays_MID',1,1,'Channel4.Receiving1.K0006.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\“'),(39,'Receiving1_FourWays_Right',1,1,'Channel4.Receiving1.K0006.15',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\”'),(40,'Receiving1_Gate1_Ols',1,1,'Channel4.Receiving1.K0000.01',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\‘'),(41,'Receiving1_Gate10_Alarm',1,1,'Channel4.Receiving1.K0003.11',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\’'),(42,'Receiving1_Gate10_Cls',1,1,'Channel4.Receiving1.K0004.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\÷'),(43,'Receiving1_Gate10_Ols',1,1,'Channel4.Receiving1.K0004.00',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\◊'),(44,'Receiving1_Gate11_Alarm',1,1,'Channel4.Receiving1.K0005.04',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\ÿ'),(45,'Receiving1_Gate11_Cls',1,1,'Channel4.Receiving1.K0001.00',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ÿ'),(46,'Receiving1_Gate11_Ols',1,1,'Channel4.Receiving1.K0005.03',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\⁄'),(47,'Receiving1_Gate12_Alarm',1,1,'Channel4.Receiving1.K0005.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\€'),(48,'Receiving1_Gate12_Cls',1,1,'Channel4.Receiving1.K0001.01',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\‹'),(49,'Receiving1_Gate12_Ols',1,1,'Channel4.Receiving1.K0005.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\›'),(50,'Receiving1_Gate13_Alarm',1,1,'Channel4.Receiving1.K0005.01',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\ﬁ'),(51,'Receiving1_Gate13_Cls',1,1,'Channel4.Receiving1.K0001.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\ﬂ'),(52,'Receiving1_Gate13_Ols',1,1,'Channel4.Receiving1.K0005.00',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\‡'),(53,'Receiving1_Gate14_Alarm',1,1,'Channel4.Receiving1.K0005.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\·'),(54,'Receiving1_Gate14_Cls',1,1,'Channel4.Receiving1.K0000.15',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\‚'),(55,'Receiving1_Gate14_Ols',1,1,'Channel4.Receiving1.K0005.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\„'),(56,'Receiving1_Gate15_Alarm',1,1,'Channel4.Receiving1.K0005.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\‰'),(57,'Receiving1_Gate15_Cls',1,1,'Channel4.Receiving1.K0000.14',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Â'),(58,'Receiving1_Gate15_Ols',1,1,'Channel4.Receiving1.K0005.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ê'),(59,'Receiving1_Gate16_Ols',1,1,'Channel4.Receiving1.K0000.11',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Á'),(60,'Receiving1_Gate17_Ols',1,1,'Channel4.Receiving1.K0000.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ë'),(61,'Receiving1_Gate18_Ols',1,1,'Channel4.Receiving1.K0000.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\È'),(62,'Receiving1_Gate19_Ols',1,1,'Channel4.Receiving1.K0000.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Í'),(63,'Receiving1_Gate20_Ols',1,1,'Channel4.Receiving1.K0000.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Î'),(64,'Receiving1_Gate21_Ols',1,1,'Channel4.Receiving1.K0000.01',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ï'),(65,'Receiving1_Gate3_Alarm',1,1,'Channel4.Receiving1.K0005.12',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ì'),(66,'Receiving1_Gate3_Cls',1,1,'Channel4.Receiving1.K0000.12',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ó'),(67,'Receiving1_Gate3_Ols',1,1,'Channel4.Receiving1.K0005.11',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª\Ô'),(68,'Receiving1_Gate4_Alarm',1,1,'Channel4.Receiving1.K0005.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª'),(69,'Receiving1_Gate4_Cls',1,1,'Channel4.Receiving1.K0000.13',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªÒ'),(70,'Receiving1_Gate4_Ols',1,1,'Channel4.Receiving1.K0005.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªÚ'),(71,'Receiving1_Gate5_Alarm',1,1,'Channel4.Receiving1.K0003.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªÛ'),(72,'Receiving1_Gate5_Cls',1,1,'Channel4.Receiving1.K0004.01',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªÙ'),(73,'Receiving1_Gate5_Ols',1,1,'Channel4.Receiving1.K0000.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªı'),(74,'Receiving1_Gate6_Alarm',1,1,'Channel4.Receiving1.K0003.08',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªˆ'),(75,'Receiving1_Gate6_Cls',1,1,'Channel4.Receiving1.K0004.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª˜'),(76,'Receiving1_Gate6_Ols',1,1,'Channel4.Receiving1.K0000.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª¯'),(77,'Receiving1_Gate7_Alarm',1,1,'Channel4.Receiving1.K0003.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª˘'),(78,'Receiving1_Gate7_Cls',1,1,'Channel4.Receiving1.K0004.04',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª˙'),(79,'Receiving1_Gate7_Ols',1,1,'Channel4.Receiving1.K0000.04',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª˚'),(80,'Receiving1_Gate8_Alarm',1,1,'Channel4.Receiving1.K0003.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª¸'),(81,'Receiving1_Gate8_Cls',1,1,'Channel4.Receiving1.K0004.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª˝'),(82,'Receiving1_Gate8_Ols',1,1,'Channel4.Receiving1.K0000.03',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ª˛'),(83,'Receiving1_Gate9_Alarm',1,1,'Channel4.Receiving1.K0003.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ªˇ'),(84,'Receiving1_Gate9_Cls',1,1,'Channel4.Receiving1.K0004.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(85,'Receiving1_Gate9_Ols',1,1,'Channel4.Receiving1.K0000.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(86,'Receiving1_Gate9_Ols4',1,1,'Channel4.Receiving1.K0000.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(87,'Receiving1_Gate9_Ols5',1,1,'Channel4.Receiving1.K0000.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(88,'Receiving1_Gate9_Ols6',1,1,'Channel4.Receiving1.K0000.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(89,'Receiving1_Gate9_Ols7',1,1,'Channel4.Receiving1.K0000.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(90,'Receiving1_Gate9_Ols8',1,1,'Channel4.Receiving1.K0000.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(91,'Receiving1_Gate9_Ols9',1,1,'Channel4.Receiving1.K0000.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(92,'Receiving1_leg1alm',1,1,'Channel4.Receiving1.K0010.00',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º	'),(93,'Receiving1_LegMotor1_Overload',8,4,'Channel4.Receiving1.R0016',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º\n'),(94,'Receiving1_LegMotor1_Running',1,1,'Channel4.Receiving1.K0006.01',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(95,'Receiving1_LegMotor2_Overload',8,4,'Channel4.Receiving1.R0024',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(96,'Receiving1_LegMotor2_Running',1,1,'Channel4.Receiving1.K0005.15',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º\r'),(97,'Receiving1_Legmotor2Speed_Speed',8,4,'Channel4.Receiving1.R0028',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(98,'Receiving1_LegMotor3_Overload',8,4,'Channel4.Receiving1.R0044',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(99,'Receiving1_LegMotor3_Running',1,1,'Channel4.Receiving1.K0006.00',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(100,'Receiving1_Legmotor3Curr_Digi',8,4,'Channel4.Receiving1.R0036',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(101,'Receiving1_Legmotor3Speed_Speed',8,4,'Channel4.Receiving1.R0048',20001,'','',NULL,'ÊèêÂçáÊú∫ÊµãÈÄü',0.00,0.00,0,'\0\0\0\0\0º'),(102,'Receiving1_LegMotor4_Overload',8,4,'Channel4.Receiving1.R0004',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(103,'Receiving1_LegMotor4_Running',1,1,'Channel4.Receiving1.K0001.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(104,'Receiving1_Legmotor4Curr_Digi',8,4,'Channel4.Receiving1.R0000',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(105,'Receiving1_LocalRemote',1,1,'Channel4.Receiving1.K0008.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(106,'Receiving1_MagicRoll1_Alarm',1,1,'Channel4.Receiving1.K0007.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(107,'Receiving1_MagicRoll1_Running',1,1,'Channel4.Receiving1.K0006.11',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(108,'Receiving1_MagicRoll2_Alarm',1,1,'Channel4.Receiving1.K0007.01',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(109,'Receiving1_MagicRoll2_Running',1,1,'Channel4.Receiving1.K0006.06',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º\Z'),(110,'Receiving1_MagicRoll3_Alarm',1,1,'Channel4.Receiving1.K0007.00',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(111,'Receiving1_MagicRoll3_Running',1,1,'Channel4.Receiving1.K0006.04',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(112,'Receiving1_MagicRoll4_Alarm',1,1,'Channel4.Receiving1.K0001.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(113,'Receiving1_MagicRoll4_Running',1,1,'Channel4.Receiving1.K0001.09',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(114,'Receiving1_Sifter1_Running',1,1,'Channel4.Receiving1.K0006.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º'),(115,'Receiving1_Sifter2_Running',1,1,'Channel4.Receiving1.K0006.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º '),(116,'Receiving1_Sifter3_Alarm',1,1,'Channel4.Receiving1.K0001.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º!'),(117,'Receiving1_Sifter3_Running',1,1,'Channel4.Receiving1.K0001.10',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º\"'),(118,'Receiving1_ThreeWays1_Left',1,1,'Channel4.Receiving1.K0001.07',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º#'),(119,'Receiving1_ThreeWays1_Right',1,1,'Channel4.Receiving1.K0001.05',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º$'),(120,'Receiving1_ThreeWays2_Left',1,1,'Channel4.Receiving1.K0006.03',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º%'),(121,'Receiving1_ThreeWays2_Right',1,1,'Channel4.Receiving1.K0006.02',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º&'),(122,'Receiving2_LegCUR102_Digi',8,4,'Channel4.Receiving1.R0020',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º\''),(123,'Receiving2_LegCUR106_Digi',8,4,'Channel4.Receiving1.R0012',20001,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º('),(124,'Receiving1_Conveyor1_Alarm',1,1,'Channel4.Receiving2.K0002.11',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º)'),(125,'Receiving1_Conveyor1_Running',1,1,'Channel4.Receiving2.K0002.10',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º*'),(126,'Receiving1_Conveyor2_Alarm',1,1,'Channel4.Receiving2.K0002.09',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º+'),(127,'Receiving1_Conveyor2_Running',1,1,'Channel4.Receiving2.K0002.04',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º,'),(128,'Receiving1_Gate1_Alarm',1,1,'Channel4.Receiving2.K0001.14',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º-'),(129,'Receiving1_Gate1_Cls',1,1,'Channel4.Receiving2.K0001.13',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º.'),(130,'Receiving1_Gate1_Ols',1,1,'Channel4.Receiving2.K0001.11',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º/'),(131,'Receiving1_Gate2_Alarm',1,1,'Channel4.Receiving2.K0001.08',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º0'),(132,'Receiving1_Gate2_Cls',1,1,'Channel4.Receiving2.K0001.09',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º1'),(133,'Receiving1_Gate2_Ols',1,1,'Channel4.Receiving2.K0001.10',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º2'),(134,'Receiving2_Airport1_Alarm',1,1,'Channel4.Receiving2.K0002.08',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º3'),(135,'Receiving2_Airport1_Running',1,1,'Channel4.Receiving2.K0001.05',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º4'),(136,'Receiving2_Airport2_Alarm',1,1,'Channel4.Receiving2.K0001.00',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º5'),(137,'Receiving2_Airport2_Running',1,1,'Channel4.Receiving2.K0001.01',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º6'),(138,'Receiving2_AlmAck',1,1,'Channel4.Receiving2.K0002.02',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º7'),(139,'Receiving2_Converyor1_Running',1,1,'Channel4.Receiving2.K0001.15',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º8'),(140,'Receiving2_Converyor2_Alarm',1,1,'Channel4.Receiving2.K0002.12',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º9'),(141,'Receiving2_Converyor2_Running',1,1,'Channel4.Receiving2.K0002.13',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º:'),(142,'Receiving2_Fan1_Alarm',1,1,'Channel4.Receiving2.K0001.02',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º;'),(143,'Receiving2_Fan1_Running',1,1,'Channel4.Receiving2.K0001.03',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º<'),(144,'Receiving2_Gate1_Alarm',1,1,'Channel4.Receiving2.K0001.04',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º='),(145,'Receiving2_Gate1_Cls',1,1,'Channel4.Receiving2.K0001.06',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º>'),(146,'Receiving2_Gate1_Ols',1,1,'Channel4.Receiving2.K0001.07',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º?'),(147,'Receiving2_Gate2_Ols',1,1,'Channel4.Receiving2.K0002.05',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0º@'),(148,'Receiving2_LegMotor1_Overload',8,4,'Channel4.Receiving2.R0008',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºA'),(149,'Receiving2_Legmotor1Speed_Speed',8,4,'Channel4.Receiving2.R0012',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºB'),(150,'Receiving2_LegMotor2_Overload',8,4,'Channel4.Receiving2.R0000',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºC'),(151,'Receiving2_LegMotor2_Running',1,1,'Channel4.Receiving2.K0002.01',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºD'),(152,'Receiving2_Legmotor2Speed_Speed',8,4,'Channel4.Receiving2.R0004',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºE'),(153,'Receiving2_LocalRemote',1,1,'Channel4.Receiving2.K0002.03',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºF'),(154,'Receiving2_MagicRoll1_Alarm',1,1,'Channel4.Receiving2.K0002.14',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºG'),(155,'Receiving2_MagicRoll1_Running',1,1,'Channel4.Receiving2.K0002.00',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºH'),(156,'Receiving2_Sifter1_Alarm',1,1,'Channel4.Receiving2.K0002.15',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºI'),(157,'Receiving2_Sifter1_Running',1,1,'Channel4.Receiving2.K0002.07',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºJ'),(158,'Receiving2_Sifter1_Running8',1,1,'Channel4.Receiving2.K0002.07',20002,'','\0',NULL,NULL,0.00,0.00,0,'\0\0\0\0\0ºK');
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `registermodule` (
  `DriverID` int(10) NOT NULL AUTO_INCREMENT,
  `AssemblyName` varchar(255) DEFAULT NULL,
  `ClassName` varchar(50) DEFAULT NULL,
  `ClassFullName` varchar(128) DEFAULT NULL,
  `Description` varchar(50) DEFAULT NULL,
  UNIQUE KEY `DriverID` (`DriverID`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
INSERT INTO `registermodule` VALUES (4,'E:\\SCADA\\dll\\OPCDriver.dll','OPCReader','OPCDriver.OPCReader','OPCÂçèËÆÆ'),(5,'E:\\SCADA\\dll\\FileDriver.dll','DataBaseReader','FileDriver.DataBaseReader','SQL Êï∞ÊçÆÂ∫ì'),(6,'E:\\SCADA\\dll\\FileDriver.dll','TagDriver','FileDriver.TagDriver','Ê†áÁ≠æÁõ¥Êé•ËØªÂÜô'),(8,'E:\\SCADA\\dll\\ModbusDriver.dll','ModbusRTUReader','ModbusDriver.ModbusRTUReader','Modbus RTUÂçèËÆÆ'),(9,'E:\\SCADA\\dll\\ModbusDriver.dll','ModbusTCPReader','ModbusDriver.ModbusTCPReader','Modbus TcpÂçèËÆÆ'),(10,'E:\\SCADA\\dll\\SiemensPLCDriver.dll','SiemensTCPReader','SiemensPLCDriver.SiemensTCPReader','S7 ‰ª•Â§™ÁΩëÂçèËÆÆ');
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `AddEventLog`(IN pStartTime 	DATETIME,
						     IN pSource		NVARCHAR(50) ,
							 IN pComment	NVARCHAR(50))
BEGIN
IF pComment<>IFNULL((SELECT Comment FROM LOG_EVENT WHERE EVENTTYPE=2 AND Source=Source ORDER BY SQLCOUNTER DESC LIMIT 1),'') THEN
	INSERT INTO LOG_EVENT(EVENTTYPE,SEVERITY,ACTIVETIME,SOURCE,COMMENT) VALUES(2,0,pStartTime,pSource,pComment);
END IF;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetAlarm`(IN pStartTime 	DATETIME,
						  IN pEndTime    DATETIME )
BEGIN
SELECT StartTime,AlarmText,AlarmValue,SubAlarmType,Severity,ConditionID,Source,Duration FROM LOG_ALARM WHERE StartTime BETWEEN pStartTime AND pEndTime ORDER BY StartTime;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetEventTime`(IN pEVENTTYPE int,
						      IN pSOURCE nvarchar(50),
							  IN pCOMMENT nvarchar(50),
							  OUT pSTARTTIME DATETIME,
							  OUT pENDTIME DATETIME)
BEGIN
DECLARE _ID INT DEFAULT 0;

SELECT SQLCOUNTER,ACTIVETIME INTO _ID,pSTARTTIME FROM  LOG_EVENT WHERE EVENTTYPE=pEVENTTYPE AND SOURCE=pSOURCE AND COMMENT=pCOMMENT ORDER BY ACTIVETIME DESC  LIMIT 1;

SET @sql = CONCAT('SELECT ACTIVETIME INTO ', pENDTIME, ' FROM LOG_EVENT WHERE EVENTTYPE = "', 
									pEVENTTYPE, '" AND SOURCE = "', pSOURCE, '" AND SQLCOUNTER> ',_ID,' ORDER BY ACTIVETIME DESC LIMIT 1');
				PREPARE stmt FROM @sql;
				EXECUTE stmt;
				DEALLOCATE PREPARE stmt;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `InitServer`(IN pTYPE int)
BEGIN

	IF pTYPE<>1 THEN
	 SELECT M.DRIVERID,DRIVERNAME,SERVER,TIMEOUT,R.AssemblyName,R.ClassFullName,Spare1,Spare2 FROM META_DRIVER M INNER JOIN RegisterModule R ON M.DRIVERTYPE=R.DriverID;
	END IF;

	SELECT COUNT(*) FROM META_TAG;

	SELECT TAGID,GROUPID,RTRIM(TAGNAME),ADDRESS,DATATYPE,DATASIZE,ARCHIVE,MAXIMUM,MINIMUM,CYCLE FROM META_TAG WHERE ISACTIVE=1;

	IF pTYPE<>1 THEN
		SELECT DRIVERID,GROUPNAME,GROUPID,UPDATERATE,DEADBAND,ISACTIVE FROM META_GROUP ;
	END IF;

	IF pTYPE=0 THEN 
		SELECT SOURCE FROM META_Condition WHERE EVENTTYPE=2;
	END IF;

	IF pTYPE<>2 THEN 
		SELECT TYPEID,SOURCE,ALARMTYPE,A.ISENABLED,CONDITIONTYPE,PARA,IFNULL(COMMENT,''),DEADBAND,DELAY,SUBALARMTYPE,Threshold,SEVERITY,
		IFNULL(MESSAGE,''),B.ISENABLE FROM META_Condition a LEFT OUTER JOIN META_SUBCONDITION b ON a.TypeID=b.ConditionID WHERE EVENTTYPE<>2;
	END IF;

	-- LEFT OUTER JOIN META_TAG c ON a.SOURCEID=c.TAGID 
	SELECT SCALEID,SCALETYPE,EUHI,EULO,RAWHI,RAWLO FROM META_SCALE;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `ReadALL`(IN pGroupID SMALLINT)
BEGIN

SELECT COUNT(*) FROM  META_TAG WHERE GROUPID=pGroupID AND IsActive=1;
SELECT TAGID,DATATYPE,IFNULL(DEFAULTVALUE,0) FROM META_TAG WHERE IsActive=1 AND GROUPID=pGroupID ORDER BY TAGID;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `ReadHData`(IN pStartTime DATETIME,
						   IN pEndTime DATETIME,
						   IN pID INT)
BEGIN

	IF pID IS NULL THEN
		SELECT ID,TIMESTAMP,VALUE,M.DATATYPE FROM LOG_HDATA L INNER JOIN META_TAG M ON L.ID=M.TAGID WHERE TIMESTAMP BETWEEN pStartTime AND pEndTime ORDER BY ID,TIMESTAMP;
	ELSE 
		SELECT TIMESTAMP,VALUE,M.DATATYPE FROM LOG_HDATA L INNER JOIN META_TAG M ON L.ID=M.TAGID WHERE ID=pID AND TIMESTAMP BETWEEN pStartTime AND pEndTime  ORDER BY TIMESTAMP;
		-- select ID,TIMESTAMP,VALUE from HDADATA WHERE TIMESTAMP BETWEEN @StartTime AND @EndTime order by TIMESTAMP
	END IF;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `ReadValueByID`(IN pID SMALLINT,
						   IN pDATATYPE TinyInt)
BEGIN

	--  ÂæÖÂÆåÂñÑ
-- IF @DATATYPE=1 
-- SELECT CAST(DEFAULTVALUE AS BIT) FROM META_TAG WHERE TAGID=@ID
-- ELSE IF @DATATYPE=3
-- SELECT CAST(DEFAULTVALUE AS TINYINT) FROM META_TAG WHERE TAGID=@ID
-- ELSE IF @DATATYPE=4
-- SELECT CAST(DEFAULTVALUE AS SMALLINT) FROM META_TAG WHERE TAGID=@ID
-- ELSE IF @DATATYPE=7
-- SELECT CAST(DEFAULTVALUE AS INT) FROM META_TAG WHERE TAGID=@ID
-- ELSE IF @DATATYPE=8
-- SELECT CAST(DEFAULTVALUE AS REAL) FROM META_TAG WHERE TAGID=@ID
-- ELSE IF @DATATYPE=11
-- SELECT CAST(DEFAULTVALUE AS VARCHAR) FROM META_TAG WHERE TAGID=@ID

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `UpdateValueByID`(IN pID SMALLINT,
						   IN pValue varchar(50))
BEGIN

	UPDATE META_TAG SET DEFAULTVALUE=pValue WHERE TAGID=pID;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `WriteHData`(IN pDATE DATETIME)
BEGIN

	-- DELETE FROM LOG_HDATA FROM LOG_HDATA L INNER JOIN META_TAG T ON T.TAGID=L.ID WHERE T.DATATYPE=11
	-- SELECT COUNT(*),COUNT(DISTINCT ID) FROM LOG_HDATA WHERE DATEDIFF(DAY,@DATE,TIMESTAMP)=0;
	-- SELECT H.ID,T.DATATYPE,C FROM( SELECT ID,COUNT(*)C FROM LOG_HDATA WHERE DATEDIFF(DAY,@DATE,TIMESTAMP)=0 GROUP BY ID)H INNER JOIN META_TAG T ON H.ID=T.TAGID ORDER BY ID --WITH ROLLUP
	-- SELECT TIMESTAMP,VALUE FROM LOG_HDATA WHERE DATEDIFF(DAY,@DATE,TIMESTAMP)=0 ORDER BY ID,TIMESTAMP

	-- DELETE FROM LOG_HDATA WHERE DATEDIFF(DAY,@DATE,TIMESTAMP)=0;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
