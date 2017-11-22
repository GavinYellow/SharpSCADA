using DataService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace DatabaseLib
{
    public class HDAIOHelper
    {
        static int[] dataLen = new int[] { 5, 5, 5, 5, 6, 6, 8, 8, 8, 8, 8, 8 };

        static string m_Path = @"D:\HDA";

        static HDAIOHelper()
        {
            m_Path = DataHelper.HdaPath;
        }

        public static bool FindFile(DateTime date)
        {
            if (Directory.Exists(m_Path))
            {
                return File.Exists(string.Concat(m_Path, "\\", date.Year.ToString(), "-", date.Month.ToString(), ".bin"));
            }
            return false;
        }

        public static bool CreateFile(int year, int month)
        {
            string path = string.Concat(m_Path, "\\", year.ToString(), "-", month.ToString(), ".bin");
            try
            {
                if (Directory.Exists(m_Path))
                {
                    if (File.Exists(path))
                    {
                        return true;
                    }
                }
                else
                {
                    Directory.CreateDirectory(m_Path);
                }
                using (var stream = File.Create(path, 0x100))
                {
                    stream.Write(new byte[0x100], 0, 0x100);
                    return true;
                }
            }
            catch (Exception err)
            {
                DataHelper.AddErrorLog(err);
                return false;
            }
        }

        public static bool GetRangeFromDatabase(short? ID, ref DateTime start, ref DateTime end)
        {
            using (var reader = DataHelper.Instance.ExecuteReader("SELECT MIN(TIMESTAMP),MAX(TIMESTAMP) FROM LOG_HDATA" + (ID.HasValue ? " WHERE ID=" + ID.Value : "")))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            start = reader.GetDateTime(0);
                        if (!reader.IsDBNull(1))
                            end = reader.GetDateTime(1);
                        return true;
                    }
                }
            }
            //start = end = DateTime.MinValue;
            return false;
        }

        public static void BackUpFile(DateTime date)
        {
            lock (typeof(HDAIOHelper))
            {
                if (WriteToFile(date.AddDays(-1)) == 0)
                {
                    DataHelper.Instance.ExecuteNonQuery(string.Format("DELETE FROM LOG_HDATA WHERE [TIMESTAMP]<='{0}';", date.ToShortDateString()));
                }
            }
        }

        public static int WriteToFile(DateTime date)//每天凌晨写入昨天的数据到文件，可以考虑用服务或计划任务;数据库只保留当天的记录；调度程序负责删除过期记录;历史数据应支持合并
        {
            int year = date.Year; int month = date.Month; int day = date.Day;
            string path = string.Concat(m_Path, "\\", year.ToString(), "-", month.ToString(), ".bin");
            if (CreateFile(year, month))//如该月文件不存在，则创建；否则写入
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek(day * 8, SeekOrigin.Begin);//先读入索引区，定位到该日期的指针
                    byte[] bits = new byte[8];
                    stream.Read(bits, 0, 8);//如果该位置指针为>0的正数，说明该区域已有数据
                    if (BitConverter.ToInt64(bits, 0) > 0) return -1;
                }
                using (var dataReader = DataHelper.Instance.ExecuteProcedureReader("WRITEHDATA", DataHelper.CreateParam("@DATE", SqlDbType.DateTime, date)))
                {
                    if (dataReader == null) return -10;
                    else
                    {
                        dataReader.Read();
                        int cont = dataReader.GetInt32(0);//读入标签数量
                        if (cont == 0) return -2;
                        string path2 = path + ".temp";
                        try
                        {
                            File.Copy(path, path2, true);//先把原文件全部复制到临时文件
                            //Stopwatch sw = Stopwatch.StartNew();
                            using (FileStream stream = File.Open(path2, FileMode.Open))
                            {
                                //w.Seek(8 + day * 8, SeekOrigin.Begin);
                                //w.Seek(0x100, SeekOrigin.Begin);
                                long start = stream.Seek(0, SeekOrigin.End);//定位到文件末尾
                                long end = 0;
                                using (BinaryWriter w = new BinaryWriter(stream))
                                {
                                    w.Write(new SqlDateTime(date).DayTicks);//写入日期
                                    w.Write(cont);///写入标签数量
                                    int count = dataReader.GetInt32(1);
                                    w.Write(count);
                                    HDataFormat[] list = new HDataFormat[count];
                                    if (dataReader.NextResult())
                                    {
                                        int p = 0;
                                        int x = 0;
                                        while (dataReader.Read())//写入标签元数据
                                        {
                                            short id = dataReader.GetInt16(0);//ID号
                                            byte type = dataReader.GetByte(1);//数据类型
                                            int cn = dataReader.GetInt32(2);//标签个数
                                            //list[x].ID = id;
                                            list[x].Type = (DataType)type;
                                            list[x].Count = cn;
                                            //list[x].Offset = p;
                                            w.Write(id);
                                            w.Write(type);
                                            w.Write(cn);
                                            w.Write(p);
                                            p += cn * dataLen[type];
                                            x++;
                                        }
                                        if (dataReader.NextResult())
                                        {
                                            for (int i = 0; i < list.Length; i++)
                                            {
                                                int len = list[i].Count;
                                                for (int j = 0; j < len; j++)
                                                {
                                                    if (dataReader.Read())
                                                    {
                                                        w.Write(dataReader.GetTimeTick(0));
                                                        switch (list[i].Type)
                                                        {
                                                            case DataType.BOOL:
                                                                w.Write(dataReader.GetFloat(1) > 0);
                                                                break;
                                                            case DataType.BYTE:
                                                                w.Write((byte)dataReader.GetFloat(1));
                                                                break;
                                                            case DataType.WORD:
                                                            case DataType.SHORT:
                                                                w.Write((short)dataReader.GetFloat(1));
                                                                break;
                                                            case DataType.INT:
                                                                w.Write((int)dataReader.GetFloat(1));
                                                                break;
                                                            case DataType.FLOAT:
                                                                w.Write(dataReader.GetFloat(1));
                                                                break;
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    end = stream.Position;//文件的结尾，总长度
                                    w.Seek((day - 1) * 8, SeekOrigin.Begin);//定位到索引区
                                    w.Write(start);//写入当日指针
                                    w.Write(end);//写入下一日指针
                                    //w.Close();
                                }
                            }
                            File.Copy(path2, path, true);
                        }
                        catch (Exception err)
                        {
                            DataHelper.AddErrorLog(err);
                            return -3;
                        }
                        finally
                        {
                            if (File.Exists(path2))
                                File.Delete(path2);
                        }
                        //dataReader.Close();
                        return 0;
                        /*写入失败，则将备份文件还原；数据库不做删除动作，保留记录，次日服务检查数据文件是否存在，不存在则合并写入
                        可在服务内建XML文件保存失败记录的日期列表，以便还原；用File.Mov；定时间隔、开始时间也可XML定义。
                        先备份二进制归档库，再加载数据库数据，写入文件；如成功，删除数据库当日记录并删除备份文件
                        sw.Stop();
                         * if (sw.ElapsedTicks > 0) { }
                        */
                    }
                }
            }
            return -10;
        }

        public static IEnumerable<HistoryData> LoadFromFile(DateTime start, DateTime end, bool sdt = false)
        {
            //Stopwatch sw = Stopwatch.StartNew();
            //文件的组织格式：头文件：31，ln为间隔日期，position为指向日期段的指针，sizes为日期段的长度。
            //每日的抬头：按ID次序，包含每个TAG的数量，arr为每个日期所有的标签、每标签数量、数据类型、位置指针。
            //按时间排序，每个标签的值、时间戳。
            string path = string.Concat(m_Path, "\\", start.Year.ToString(), "-", start.Month.ToString(), sdt ? ".sdt" : ".bin");
            if (!File.Exists(path)) yield break;
            int day1 = start.Day;
            int startTicks = new SqlDateTime(start).TimeTicks;
            int endTicks = new SqlDateTime(end).TimeTicks;
            int ln = end.Day - day1 + 1;
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryReader reader = new BinaryReader(stream);
                long[] positions = new long[ln + 1];
                long[] sizes = new long[ln];
                stream.Seek((day1 - 1) * 8, SeekOrigin.Begin);
                positions[0] = reader.ReadInt64();
                for (int i = 0; i < ln; i++)
                {
                    positions[i + 1] = reader.ReadInt64();
                    sizes[i] = positions[i + 1] - positions[i];//每一天数据的长度
                }
                //reader.Close();
                HistoryData data = HistoryData.Empty;
                using (MemoryMappedFile mapp = MemoryMappedFile.CreateFromFile(stream, Guid.NewGuid().ToString(), 0, MemoryMappedFileAccess.Read,
                    HandleInheritability.Inheritable, false))
                {
                    for (int k = 0; k < ln; k++)
                    {
                        if (positions[k] < 0x100 || sizes[k] <= 0 || positions[k] + sizes[k] > stream.Length)
                            continue;
                        using (MemoryMappedViewAccessor acc = mapp.CreateViewAccessor(positions[k], sizes[k], MemoryMappedFileAccess.Read))
                        {
                            long pos = 0;
                            int day = acc.ReadInt32(pos);
                            pos += 8;
                            int count = acc.ReadInt32(pos);
                            pos += 4;
                            HDataFormat[] arr = new HDataFormat[count];
                            for (int i = 0; i < count; i++)
                            {
                                arr[i].ID = acc.ReadInt16(pos);
                                pos += 2;
                                arr[i].Type = (DataType)acc.ReadByte(pos);
                                pos++;
                                arr[i].Count = acc.ReadInt32(pos);//4个字节是预留
                                pos += 8;
                            }
                            long tempos = pos;
                            for (int i = 0; i < count; i++)
                            {
                                int con = arr[i].Count;
                                int j = 0;
                                pos = tempos + acc.ReadInt32(i * 11 + 19);
                                long pf = pos;
                                DataType type = arr[i].Type;
                                int len = dataLen[(int)type];
                                if (k == 0)        //判断是否为起始日期或结束日期
                                {
                                    int ind = BinarySearchTime(acc, pf, con, len, startTicks);
                                    if (ind < 0) ind = ~ind;
                                    j += ind;
                                    pos += ind * len;
                                }
                                if (k == ln - 1)
                                {
                                    int index = BinarySearchTime(acc, pf, con, len, endTicks);
                                    con = index >= 0 ? index : ~index;
                                }
                                while (j++ < con)
                                {
                                    data.ID = arr[i].ID;
                                    data.TimeStamp = new SqlDateTime(day, acc.ReadInt32(pos)).Value;
                                    pos += 4;
                                    switch (type)
                                    {
                                        case DataType.BOOL:
                                            data.Value.Boolean = acc.ReadBoolean(pos);
                                            pos++;
                                            break;
                                        case DataType.BYTE:
                                            data.Value.Byte = acc.ReadByte(pos);
                                            pos++;
                                            break;
                                        case DataType.WORD:
                                        case DataType.SHORT:
                                            data.Value.Int16 = acc.ReadInt16(pos);
                                            pos += 2;
                                            break;
                                        case DataType.INT:
                                            data.Value.Int32 = acc.ReadInt32(pos);
                                            pos += 4;
                                            break;
                                        case DataType.FLOAT:
                                            data.Value.Single = acc.ReadSingle(pos);
                                            pos += 4;
                                            break;
                                    }
                                    yield return data;
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }

        public static IEnumerable<HistoryData> LoadFromFile(DateTime start, DateTime end, short ID, bool sdt = false)
        {
            string path = string.Concat(m_Path, "\\", start.Year.ToString(), "-", start.Month.ToString(), sdt ? ".sdt" : ".bin");//bin-sdt
            if (!File.Exists(path)) yield break;
            int day1 = start.Day;
            int startTicks = new SqlDateTime(start).TimeTicks;//开始日期部分的4位数据
            int endTicks = new SqlDateTime(end).TimeTicks;
            int ln = end.Day - day1 + 1;//日期天数
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long filelen = stream.Length;//文件长度
                BinaryReader reader = new BinaryReader(stream);
                long[] positions = new long[ln];//每日数据指针（指向第一条数据，包括当日数据索引区）
                stream.Seek((day1 - 1) * 8, SeekOrigin.Begin);///找到对应的开始日期索引位置

                for (int i = 0; i < ln; i++)
                {
                    positions[i] = reader.ReadInt64();//读入时间段内每日数据长度值
                }
                long[] sizes = new long[ln];
                for (int i = 0; i < ln; i++)
                {
                    if (positions[i] >= filelen) break;//如果读入长度超过文件大小则退出
                    stream.Seek(positions[i] + 8, SeekOrigin.Begin);//定位文件指针到当日数据开头
                    sizes[i] = reader.ReadInt32();//sizes为当日该标签数
                }
                //reader.Close();
                HistoryData data = HistoryData.Empty;
                //stream.Read(new byte[]
                using (MemoryMappedFile mapp = MemoryMappedFile.CreateFromFile(stream, Guid.NewGuid().ToString(), filelen, MemoryMappedFileAccess.Read,
                    HandleInheritability.Inheritable, false))
                {
                    for (int k = 0; k < ln; k++)//先读入当日索引区
                    {
                        if (positions[k] < 0x100 || sizes[k] <= 0 || positions[k] + sizes[k] > filelen)
                            continue;
                        //if (sizes[k] == 0) continue;
                        long pos = 0;
                        int count = 0;
                        int day = 0;
                        int len = 0;
                        DataType type = DataType.NONE;
                        using (MemoryMappedViewAccessor acc1 = mapp.CreateViewAccessor(positions[k], 12 + sizes[k] * 11, MemoryMappedFileAccess.Read))//12是头的长度，11是一个格式字段的长度
                        {
                            day = acc1.ReadInt32(0);//当日日期部分
                            int index = BinarySearch(acc1, (int)sizes[k], ID);//找到当天 指定标签的记录索引
                            if (index >= 0)
                                index = index * 11 + 12;//如找到，则定位到当日数据的元数据（相对位移）
                            //sw.Stop();
                            else continue;
                            byte tp = acc1.ReadByte(index + 2);//读入数据类型
                            type = (DataType)tp;
                            len = dataLen[tp];//4，6，8分别为存储的标签长度，其中4字节是时间戳
                            count = acc1.ReadInt32(index + 3);//读入数量
                            pos = positions[k] + 12 + sizes[k] * 11 + acc1.ReadInt32(index + 7);//指针指向当日当前标签第一条记录
                        }
                        using (MemoryMappedViewAccessor acc2 = mapp.CreateViewAccessor(pos, count * len, MemoryMappedFileAccess.Read))//重新从头定位文件指针到数据区
                        {
                            pos = 0;
                            int j = 0;
                            if (k == 0)//判断是否为起始日期或结束日期
                            {
                                int ind = BinarySearchTime(acc2, 0, count, len, startTicks);//根据时间排序方式二分法查找当日当前时间节点的数据，如为第一日
                                if (ind < 0) ind = ~ind;
                                j += ind;
                                pos += ind * len;
                            }
                            if (k == ln - 1)
                            {
                                int ind = BinarySearchTime(acc2, 0, count, len, endTicks);//如果为最后一日的数据，则按结束时间定位
                                count = ind >= 0 ? ind : ~ind;
                            }
                            while (j++ < count)
                            {
                                data.ID = ID;
                                data.TimeStamp = new SqlDateTime(day, acc2.ReadInt32(pos)).Value;//日期在前(4位）
                                pos += 4;//数据区也是4位
                                switch (type)
                                {
                                    case DataType.BOOL:
                                        data.Value.Boolean = acc2.ReadBoolean(pos);
                                        pos++;
                                        break;
                                    case DataType.BYTE:
                                        data.Value.Byte = acc2.ReadByte(pos);
                                        pos++;
                                        break;
                                    case DataType.WORD:
                                    case DataType.SHORT:
                                        data.Value.Int16 = acc2.ReadInt16(pos);
                                        pos += 2;
                                        break;
                                    case DataType.INT:
                                        data.Value.Int32 = acc2.ReadInt32(pos);
                                        pos += 4;
                                        break;
                                    case DataType.FLOAT:
                                        data.Value.Single = acc2.ReadSingle(pos);
                                        pos += 4;
                                        break;
                                }
                                yield return data;
                            }
                        }
                    }
                }
                reader.Close();
            }
            yield break;
        }

        public static IEnumerable<HistoryData> LoadFromDatabase(DateTime start, DateTime end, short? ID = null)
        {
            using (var dataReader = DataHelper.Instance.ExecuteProcedureReader("READHDATA",
                DataHelper.CreateParam("@STARTTIME", SqlDbType.DateTime, start),
                DataHelper.CreateParam("@ENDTIME", SqlDbType.DateTime, end),
                DataHelper.CreateParam("@ID", SqlDbType.Int, (object)ID ?? DBNull.Value)))
            {
                if (dataReader == null) yield break;
                HistoryData data = HistoryData.Empty;
                int itime = ID.HasValue ? 0 : 1;
                int ivalue = ID.HasValue ? 1 : 2;
                int itype = ID.HasValue ? 2 : 3;
                while (dataReader.Read())
                {
                    data.ID = ID.HasValue ? ID.Value : dataReader.GetInt16(0);
                    data.TimeStamp = dataReader.GetDateTime(itime);
                    switch ((DataType)dataReader.GetByte(itype))
                    {
                        case DataType.BOOL:
                            data.Value.Boolean = dataReader.GetFloat(ivalue) > 0 ? true : false;
                            break;
                        case DataType.BYTE:
                            data.Value.Byte = Convert.ToByte(dataReader.GetFloat(ivalue));
                            break;
                        case DataType.WORD:
                        case DataType.SHORT:
                            data.Value.Int16 = Convert.ToInt16(dataReader.GetFloat(ivalue));
                            break;
                        case DataType.INT:
                            data.Value.Int32 = Convert.ToInt32(dataReader.GetFloat(ivalue));
                            break;
                        case DataType.FLOAT:
                            data.Value.Single = dataReader.GetFloat(ivalue);
                            break;
                    }
                    yield return data;
                }
            }
            yield break;
        }

        public static IEnumerable<HistoryData> LoadFromDatabaseAtTime(short? ID, params DateTime[] timeStamps)
        {
            StringBuilder sql = new StringBuilder("SELECT ");
            if (ID == null) sql.Append("ID,");
            sql.Append(" [TIMESTAMP],[VALUE],M.DATATYPE FROM LOG_HDATA L INNER JOIN META_TAG M ON L.ID=M.TAGID WHERE");
            if (ID != null) sql.Append("  ID=").Append(ID.Value).Append(" AND ");
            sql.Append(" [TIMESTAMP] IN(");
            for (int i = 0; i < timeStamps.Length; i++)
            {
                sql.Append("'").Append(timeStamps[i]).Append("',");
            }
            using (var dataReader = DataHelper.Instance.ExecuteReader(sql.Append("1)").ToString()))
            {
                if (dataReader == null) yield break;
                HistoryData data = HistoryData.Empty;
                int itime = ID == null ? 0 : 1;
                int ivalue = ID == null ? 1 : 2;
                int itype = ID == null ? 2 : 3;
                while (dataReader.Read())
                {
                    data.ID = ID == null ? dataReader.GetInt16(0) : ID.Value;
                    data.TimeStamp = dataReader.GetDateTime(itime);
                    switch ((DataType)dataReader.GetByte(itype))
                    {
                        case DataType.BOOL:
                            data.Value.Boolean = dataReader.GetFloat(ivalue) > 0 ? true : false;
                            break;
                        case DataType.BYTE:
                            data.Value.Byte = Convert.ToByte(dataReader.GetFloat(ivalue));
                            break;
                        case DataType.WORD:
                        case DataType.SHORT:
                            data.Value.Int16 = Convert.ToInt16(dataReader.GetFloat(ivalue));
                            break;
                        case DataType.INT:
                            data.Value.Int32 = Convert.ToInt32(dataReader.GetFloat(ivalue));
                            break;
                        case DataType.FLOAT:
                            data.Value.Single = dataReader.GetFloat(ivalue);
                            break;
                    }
                    yield return data;
                }
            }
            yield break;
        }

        private static int BinarySearch(MemoryMappedViewAccessor acc, int length, short value)
        {
            int i = 0;
            int num2 = length - 1;
            while (i <= num2)
            {
                int num3 = i + ((num2 - i) >> 1);
                int num4 = acc.ReadInt16(12 + num3 * 11).CompareTo(value);
                if (num4 == 0)
                {
                    return num3;
                }
                if (num4 < 0)
                {
                    i = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return -1;
        }

        private static int BinarySearchTime(MemoryMappedViewAccessor acc, long offset, int count, int len, int ticks)
        {
            int i = 0;
            int num2 = count - 1;
            while (i <= num2)
            {
                int num3 = i + ((num2 - i) >> 1);
                int num4 = acc.ReadInt32(offset + num3 * len).CompareTo(ticks);
                if (num4 == 0)
                {
                    return num3;
                }
                if (num4 < 0)
                {
                    i = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return ~i;
        }

        public static void SDTCompression(int year, int month, float E = 0.7f)
        {
            //Stopwatch sw = Stopwatch.StartNew();
            string path = string.Concat(m_Path, "\\", year.ToString(), "-", month.ToString());
            using (FileStream stream = File.Open(path + ".bin", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (FileStream outstream = File.Create(path + ".sdt"))
                {
                    outstream.Write(new byte[0x100], 0, 0x100);
                    BinaryWriter w = new BinaryWriter(outstream);
                    using (MemoryMappedFile mapp = MemoryMappedFile.CreateFromFile(stream, "map1", stream.Length,
                        MemoryMappedFileAccess.ReadWrite, HandleInheritability.Inheritable, false))
                    {
                        int days = DateTime.DaysInMonth(year, month);
                        long[] ps = new long[days + 1];
                        long[] ps1 = new long[days + 1];
                        long[] sizes = new long[days];
                        MemoryMappedViewAccessor acc1 = mapp.CreateViewAccessor(0, 8 * days);
                        long begin = 0;
                        ps[0] = acc1.ReadInt64(begin);
                        for (int i = 0; i < days; i++)
                        {
                            begin += 8;
                            ps[i + 1] = (i == days - 1 ? stream.Length : acc1.ReadInt64(begin));
                            sizes[i] = ps[i + 1] - ps[i];
                        }
                        acc1.Dispose();
                        for (int i = 0; i < days; i++)
                        {
                            if (ps[i] < 0x100 || sizes[i] <= 0)
                                continue;
                            using (MemoryMappedViewAccessor acc = mapp.CreateViewAccessor(ps[i], sizes[i]))
                            {
                                ps1[i] = outstream.Position;
                                int len = acc.ReadInt32(8);
                                int len1 = len * 11 + 12;
                                HDataFormat[] list = new HDataFormat[len];
                                w.Write(acc.ReadInt32(0));
                                w.Write(acc.ReadInt32(4));
                                w.Write(len);
                                outstream.Write(new byte[len1 - 12], 0, len1 - 12);
                                long pos = 12;
                                int off = 0;
                                for (int j = 0; j < len; j++)
                                {
                                    short id; byte type; int count; int offset;
                                    id = acc.ReadInt16(pos);
                                    type = acc.ReadByte(pos + 2);
                                    count = acc.ReadInt32(pos + 3);
                                    offset = acc.ReadInt32(pos + 7);
                                    list[j].ID = id;
                                    list[j].Type = (DataType)type;
                                    list[j].Offset = off;//此处可采取三次到五次抽样得到E和TLM
                                    if (count < 3)
                                    {
                                        long pos2 = len1 + offset;
                                        for (int m = 0; m < count; m++)
                                        {
                                            w.Write(acc.ReadInt32(pos2));
                                            pos2 += 4;
                                            w.Write(acc.ReadSingle(pos2));
                                            pos2 += 4;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        switch (list[j].Type)
                                        {
                                            case DataType.FLOAT:
                                                {
                                                    int crt = 0; int net = 0;
                                                    float crv = 0; float nev = 0;
                                                    int maxt = 0; int mint = maxt; int sumt = 0;
                                                    float minv = 0; float maxv = minv; float sumv = 0;
                                                    int old_time = 0; int time = 0;
                                                    float mem = 0; float old_mem = 0;
                                                    long pp = len1 + offset;
                                                    long pos2 = pp + 16;
                                                    for (int c = 0; c < 9; c++)
                                                    {
                                                        crt = acc.ReadInt32(pp);
                                                        pp += 4;
                                                        crv = acc.ReadSingle(pp);
                                                        pp += 4;
                                                        if (c > 0)
                                                        {
                                                            float cv = crv - nev;
                                                            int ct = crt - net;
                                                            if (c == 1)
                                                            {
                                                                time = crt;
                                                                mem = crv;
                                                                maxt = mint = ct;
                                                                minv = maxv = cv;
                                                            }
                                                            else
                                                            {
                                                                if (cv > maxv)
                                                                    maxv = cv;
                                                                if (cv < minv)
                                                                    minv = cv;
                                                                if (ct > maxt)
                                                                    maxt = ct;
                                                                if (ct < mint)
                                                                    mint = ct;
                                                            }
                                                            sumv += cv;
                                                            sumt += ct;
                                                        }
                                                        else
                                                        {
                                                            old_mem = crv;
                                                            old_time = crt;
                                                        }
                                                        nev = crv;
                                                        net = crt;
                                                    }
                                                    int TLM = (sumt - maxt - mint) / 2;
                                                    float E1 = E * (sumv - maxv - minv) / 6;
                                                    int sum = 1;
                                                    //old_time = now_time = new_time = 0;
                                                    float timespan;
                                                    w.Write(old_time);
                                                    w.Write(old_mem);
                                                    float k1, k2, k;
                                                    timespan = time - old_time;
                                                    k = (mem - old_mem) / timespan;
                                                    k1 = k + (E1 / timespan);
                                                    k2 = 2 * k - k1;
                                                    for (int m = 2; m < count; m++)
                                                    {
                                                        if (timespan >= TLM || k < k2 || k > k1)
                                                        {
                                                            ++sum;
                                                            w.Write(old_time);
                                                            w.Write(old_mem);
                                                            k1 = k + (E1 / timespan);
                                                            k2 = 2 * k - k1;
                                                        }
                                                        old_time = time;
                                                        old_mem = mem;
                                                        time = acc.ReadInt32(pos2);
                                                        pos2 += 4;
                                                        mem = acc.ReadSingle(pos2);
                                                        pos2 += 4;
                                                        timespan = time - old_time;
                                                        k = (mem - old_mem) / timespan;
                                                    }
                                                    list[j].Count = sum;
                                                    off += sum * 8;
                                                }
                                                break;
                                            case DataType.WORD:
                                            case DataType.SHORT:
                                                {
                                                    int crt = 0; int net = 0;
                                                    short crv = 0; short nev = 0;
                                                    int maxt = 0; int mint = maxt; int sumt = 0;
                                                    int minv = 0; int maxv = minv; int sumv = 0;
                                                    int old_time = 0; int time = 0;
                                                    short mem = 0; short old_mem = 0;
                                                    long pp = len1 + offset;
                                                    long pos2 = pp + 12;
                                                    for (int c = 0; c < 9; c++)
                                                    {
                                                        crt = acc.ReadInt32(pp);
                                                        pp += 4;
                                                        crv = acc.ReadInt16(pp);
                                                        pp += 2;
                                                        if (c > 0)
                                                        {
                                                            int cv = crv - nev;
                                                            int ct = crt - net;
                                                            if (c == 1)
                                                            {
                                                                time = crt;
                                                                maxt = mint = ct;
                                                                mem = crv;
                                                                minv = maxv = cv;
                                                            }
                                                            else
                                                            {
                                                                if (cv > maxv)
                                                                    maxv = cv;
                                                                if (cv < minv)
                                                                    minv = cv;
                                                                if (ct > maxt)
                                                                    maxt = ct;
                                                                if (ct < mint)
                                                                    mint = ct;
                                                            }
                                                            sumv += cv;
                                                            sumt += ct;
                                                        }
                                                        else
                                                        {
                                                            old_mem = crv;
                                                            old_time = crt;
                                                        }
                                                        nev = crv;
                                                        net = crt;
                                                    }
                                                    int TLM = (sumt - maxt - mint) / 2;
                                                    float E1 = E * (sumv - maxv - minv) / 6;
                                                    int sum = 1;
                                                    float timespan;
                                                    w.Write(old_time);
                                                    w.Write(old_mem);
                                                    float k1, k2, k;
                                                    timespan = time - old_time;
                                                    k = (mem - old_mem) / timespan;
                                                    k1 = k + (E1 / timespan);
                                                    k2 = 2 * k - k1;
                                                    for (int m = 2; m < count; m++)
                                                    {
                                                        if (timespan >= TLM || k < k2 || k > k1)
                                                        {
                                                            ++sum;
                                                            w.Write(old_time);
                                                            w.Write(old_mem);
                                                            k1 = k + (E1 / timespan);
                                                            k2 = 2 * k - k1;
                                                        }
                                                        old_time = time;
                                                        old_mem = mem;
                                                        time = acc.ReadInt32(pos2);
                                                        pos2 += 4;
                                                        mem = acc.ReadInt16(pos2);
                                                        pos2 += 2;
                                                        timespan = time - old_time;
                                                        k = (mem - old_mem) / timespan;
                                                    }

                                                    list[j].Count = sum;
                                                    off += sum * 8;
                                                }
                                                break;
                                            default:
                                                {
                                                    byte[] buffer = new byte[count * dataLen[type]];
                                                    stream.Seek(ps[i] + len1 + offset, SeekOrigin.Begin);
                                                    stream.Read(buffer, 0, buffer.Length);
                                                    outstream.Write(buffer, 0, buffer.Length);
                                                    list[j].Count = count;
                                                    off += buffer.Length;
                                                }
                                                break;
                                        }
                                        pos += 11;
                                    }
                                }
                                outstream.Seek(ps1[i] + 12, SeekOrigin.Begin);
                                for (int j = 0; j < len; j++)
                                {
                                    w.Write(list[j].ID);
                                    w.Write((byte)list[j].Type);
                                    w.Write(list[j].Count);
                                    w.Write(list[j].Offset);
                                }
                                ps1[i + 1] = outstream.Seek(0, SeekOrigin.End);
                            }

                        }
                        outstream.Seek(0, SeekOrigin.Begin);
                        for (int i = 0; i < days + 1; i++)
                        {
                            w.Write(ps1[i]);
                        }
                    }
                }
            }
        }

        //遍历两个文件夹下所有历史记录文件；如日期无重复，则复制源路径下文件到目标路径；否则合并到一个文件
        public static unsafe bool Merge(string sourcePath, string targetPath)
        {
            return true;
        }

        public static unsafe ushort ToFloat16(float f)
        {
            uint* i = (uint*)&f;
            uint sign = (*i >> 31) & 0x1;
            uint exponent = ((*i >> 23) & 0xff) - 0x7f;
            uint mantissa = (*i) & 0x7fffff;

            exponent += 0x7;
            uint ret = ((sign & 0x1) << 15);
            ret |= ((exponent & 0xf) << 11);
            ret |= ((mantissa >> 13) & 0x7ff);
            return (ushort)ret;
        }

        public static unsafe float ToFloat32(ushort f)
        {
            ushort* i = (ushort*)&f;
            int sign = (*i >> 15) & 0x1;
            int exponent = ((*i >> 11) & 0xf) - 0x7;
            int mantissa = (*i) & 0x7ff;

            exponent += 0x7f;
            int ret = ((sign & 0x1) << 31);
            ret |= (exponent & 0xff) << 23;
            ret |= (mantissa << 13) & 0x7fffff;
            return *((float*)&ret);
        }


        public static float[] Interpolation(float[] dataIn, int n)
        {
            float[] dataOut = new float[n];
            int lenIn = dataIn.Length;
            float[] divOut = new float[n];
            for (int i = 1; i < n; i++)
            {
                divOut[i] = divOut[i - 1] + lenIn / (float)n;
            }
            int k = 0;
            for (int i = k; i < n; i++)
            {
                for (int j = 0; j < lenIn - 1; j++)
                {
                    if (divOut[i] >= j && divOut[i] < j + 1)
                    {
                        dataOut[i] = (dataIn[j + 1] - dataIn[j]) * (divOut[i] - j) + dataIn[j];
                        k = i;
                    }
                }
            }
            return dataOut;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HDataFormat
    {
        public short ID;
        public DataType Type;
        public int Count;
        public int Offset;

        public HDataFormat(short id, DataType type, int count, int offset)
        {
            ID = id;
            Type = type;
            Count = count;
            Offset = offset;
        }
    }
}