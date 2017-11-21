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
  `TagID` smallint(6) NOT NULL AUTO_INCREMENT,
  `TagName` varchar(512) NOT NULL,
  `DataType` tinyint(4) NOT NULL,
  `DataSize` smallint(6) NOT NULL,
  `Address` varchar(64) NOT NULL,
  `GroupID` smallint(6) NOT NULL,
  `IsActive` bit(1) NOT NULL,
  `Archive` bit(1) NOT NULL,
  `DefaultValue` varchar(50) DEFAULT NULL,
  `Description` varchar(128) DEFAULT NULL,
  `Maximum` float NOT NULL,
  `Minimum` float NOT NULL,
  `Cycle` int(11) NOT NULL,
  `RowVersion` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`TagID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `registermodule` (
  `DriverID` int(11) NOT NULL AUTO_INCREMENT,
  `AssemblyName` varchar(255) DEFAULT NULL,
  `ClassName` varchar(50) DEFAULT NULL,
  `ClassFullName` varchar(128) DEFAULT NULL,
  `Description` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`DriverID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
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

	--  待完善
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
