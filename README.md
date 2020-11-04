SharpSCADA - 工控网关, 轻量级组态软件.
===================
简介
-------------
采用技术：
开发语言：C#
运行环境：.NET Framework
数据库：SQL Server

功能：
-------------

* 1.轻量级工控网关：
支持当前几种主要的工业协议如西门子的Profinet、AB的EtherNetIPs、施耐德的Modbus和OPC。采用类OPC接口网关。

* 2.数据采集、归档、预警及配置工具
支持实时数据采集、历史数据归档、变量触发预警，并使用TagConfig工具简单的配置实现。

* 3.人机界面（设计时和运行时）

*设计时：
采用Microsoft Visual Studio + 设计器插件（在VS2010-VS2015社区版测试通过）。
通过继承HMIControlBase接口并书写极少量的代码即可实现复杂的图元组件。
支持图元拖放、组合、连线、变量绑定及编辑功能。

*运行时：Microsoft Visual Studio编译运行为可执行文件。


环境准备
-------------
Windows：支持的操作系统：Windows 7/8/10/Server 2008
.NET Framework 4.0/4.5/4.6
SQLServer Express 2014/2008

项目安装
-------------

下载最新版本，解压后:

* 1.可直接打开项目工程文件测试源代码：
..\SCADA\Program下运行DataExchange.sln（支持VS2010-2015各版本）

* 2.可运行可执行文件测试：
Server端测试：在目录 ..\SCADA\Program\BatchCoreTest\bin\Debug下运行BatchCoreTest.exe
Client端测试：在目录 ..\SCADA\Program\CoreTest\bin\Debug下运行CoreTest.exe
请参考Document文件夹中的教程：《部署流程》和《设计流程》，如有问题可参考《FAQ》文档。

Quick Start
-------------
* 1.还原数据库
* 2.修改配置文件并复制到C盘根目录下
* 3.修改数据库内驱动程序的路径
* 4.运行DEMO
具体流程可参看《部署流程》。

开发工具推荐
-------------
Visual Studio/Blend：做为组态设计器，推荐VS2010，VS2015版本。

项目结构
-------------
驱动程序目前支持：

* 已发布：
内存数据库
Modbus TCP/RTU、
OPC DA、
Siemens S300/200/1200/1500、
Panasonic 、
AB EtherNetIP、
Omron UDP
* 后续发布：
DDE、
Mitsubishi 

文件目录
-------------
* Database目录[存放数据文件]：
db2014.bak文件为SQL Server2014数据备份文件。
db2008.bak文件为SQL Server2008数据备份文件。
test.opf为Kepserver 4.5数据文件（可通过该软件还原为变量表）。
两个csv文件为两组变量。

* DataConfig目录[存放配置文件]：
host.cfg为主配置文件，第一行为网关服务器名/IP地址。如在本地测试，按默认lochost即可。
client.xml为客户端配置文件。
server.xml为网关服务配置文件。

* dll目录[存放驱动程序及第三方组件]：
如OPCDriver即为OPC 通讯组件。
Dynamicdatadisplay：开源归档数据显示组件，http://dynamicdatadisplay.codeplex.com/
WPFToolkit：WPF开源扩展工具包，http://wpftoolkit.codeplex.com
libnodave：西门子驱动开源库(https://github.com/netdata/libnodave)

* TagConfig目录[存放配置工具]：
可方便配置驱动、组、变量、报警、量程等信息。支持导入导出。

* Program目录[存放源代码]：
BatchCoreTest工程为网关服务器测试代码（控制台显示）。
BatchCoreService工程同BatchCoreTest，但可编译为Windows服务。
DataService工程为框架及主要接口组件。
CoreTest工程为样例文件。包含一系列界面元素。
HMIControl工程为图元组件。可支持工具栏拖放。
LinkableControlDesignTime工程为Visual Studio设计器支持插件。
DataHelper工程为SQL数据库帮助组件，同时为变量数据归档提供支持。
ClientDriver、ModbusDriver、OPCDriver、FileDriver为各类通讯组件。

* Example目录[存放样例]：
参照Document/部署流程，还原数据库，修改配置文件。
启动BatchCoreTest.exe(服务端)。
再启动CoreTest.exe(客户端)。

计划：
-------------
* 支持.NET Core。(目前已有测试版在CoreApp文件夹）
* 实现更多的通讯接口：如欧姆龙、OPC UA等。
* 更丰富的图元组件：如楼宇自控、化工等各行业。
* 功能扩展：如进一步处理数据、过程控制等。
* 安全性：安全性是重中之重，目前做的很不够。

Showcase
-------------
![](https://github.com/GavinYellow/SharpSCADA/raw/master/Showcase/guage.png)
![](https://github.com/GavinYellow/SharpSCADA/raw/master/Showcase/Receiving1.png)
![](https://github.com/GavinYellow/SharpSCADA/raw/master/Showcase/scada1.png)

个人主页
-------------
http://www.cnblogs.com/evilcat/

联系方式
-------------
hijkl1999@yeah.net
QQ群：102486275

贡献代码
-------------
[topmail](https://github.com/topmail),  [qwe7922142](https://github.com/qwe7922142),  [tonyshen277](https://github.com/tonyshen277),  [yangjingzhao123](https://github.com/yangjingzhao123), [xiebinghai](https://github.com/xiebinghai)

License
-------------
LGPL 
