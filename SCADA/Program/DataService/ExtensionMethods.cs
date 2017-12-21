using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DataService
{
    public static class ExtMethods
    {
        public static string GetExceptionMsg(this Exception e)
        {
            string err = string.Empty;
            Exception exp = e;
            while (exp != null)
            {
                err += string.Format("\n {0}", exp.Message);
                exp = exp.InnerException;
            }
            err += string.Format("\n {0}", e.StackTrace);
            return err;
        }

        public static bool ModifyItemName(this ITag tag, string name)
        {
            IDataServer server = tag.Parent.Server;
            lock (server.SyncRoot)
            {
                int index = server.GetItemProperties(tag.ID);
                if (index < 0) return false;
                var meta = server.MetaDataList[index];
                if (meta.Name == name) return true;
                server.MetaDataList[index] = new TagMetaData(meta.ID, meta.GroupID, name, meta.Address, meta.DataType, meta.Size, meta.Archive, meta.Maximum, meta.Minimum, meta.Cycle);
                server.RemoveItemIndex(meta.Name);
                server.AddItemIndex(name, tag);
                return true;
            }
        }

        public static SubCondition FindSubConditon(this IAlarmServer server, string sourceName, SubAlarmType alarmType)
        {
            var conds = server.QueryConditions(sourceName);
            if (conds == null) return SubCondition.Empty;
            foreach (ICondition cond in conds)
            {
                SubCondition sub = cond.FindSubConditon(alarmType);
                if (sub.SubAlarmType == alarmType)
                    return sub;
            }
            return SubCondition.Empty;
        }

        public static SubCondition FindSubConditon(this ICondition cond, SubAlarmType alarmType)
        {
            var subs = cond.SubConditions;
            if (subs != null && subs.Count > 0)
            {
                foreach (var sub in subs)
                {
                    if (sub.SubAlarmType == alarmType)
                    {
                        return sub;
                    }
                }
            }
            return SubCondition.Empty;
        }

        public static bool HasScaling(this IDataServer server, string tagName)
        {
            ITag tag = server[tagName];
            if (tag == null) return false;
            int scaleid = server.GetScaleByID(tag.ID);
            return scaleid >= 0;
        }

        public static bool HasAlarm(this IDataServer dserver, string sourceName)
        {
            IAlarmServer server = dserver as IAlarmServer;
            if (server == null) return false;
            List<ICondition> conds = server.ConditionList as List<ICondition>;
            return conds == null || conds.Count == 0 ? false : conds.BinarySearch(new DigitAlarm(0, sourceName)) >= 0;
        }

        public static bool HasSubCondition(this IDataServer dserver, string sourceName, SubAlarmType alarmType)
        {
            IAlarmServer server = dserver as IAlarmServer;
            if (server == null) return false;
            var conds = server.QueryConditions(sourceName);
            if (conds == null) return false;
            foreach (ICondition cond in conds)
            {
                var subs = cond.SubConditions;
                if (subs != null && subs.Count > 0)
                {
                    foreach (var sub in subs)
                    {
                        if (sub.SubAlarmType == alarmType)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static ItemData<object> ReadValueEx(this IReaderWriter reader, DeviceAddress address)
        {
            switch (address.VarType)
            {
                case DataType.BOOL:
                    var bit = reader.ReadBit(address);
                    return new ItemData<object>(bit.Value, bit.TimeStamp, bit.Quality);
                case DataType.BYTE:
                    var bt = reader.ReadByte(address);
                    return new ItemData<object>(bt.Value, bt.TimeStamp, bt.Quality);
                case DataType.WORD:
                    var ush = reader.ReadUInt16(address);
                    return new ItemData<object>(ush.Value, ush.TimeStamp, ush.Quality);
                case DataType.SHORT:
                    var sh = reader.ReadInt16(address);
                    return new ItemData<object>(sh.Value, sh.TimeStamp, sh.Quality);
                case DataType.DWORD:
                    var dw = reader.ReadUInt32(address);
                    return new ItemData<object>(dw.Value, dw.TimeStamp, dw.Quality);
                case DataType.INT:
                    var it = reader.ReadInt32(address);
                    return new ItemData<object>(it.Value, it.TimeStamp, it.Quality);
                case DataType.FLOAT:
                    var fl = reader.ReadFloat(address);
                    return new ItemData<object>(fl.Value, fl.TimeStamp, fl.Quality);
                case DataType.STR:
                    var str = reader.ReadString(address, address.DataSize);
                    return new ItemData<object>(str.Value, str.TimeStamp, str.Quality);
            }
            return new ItemData<object>(null, 0, QUALITIES.QUALITY_BAD);
        }

        public static int WriteValueEx(this IReaderWriter writer, DeviceAddress address, object value)
        {
            switch (address.VarType)
            {
                case DataType.BOOL:
                    return writer.WriteBit(address, Convert.ToBoolean(value));
                case DataType.BYTE:
                    return writer.WriteBits(address, Convert.ToByte(value));
                case DataType.WORD:
                    return writer.WriteUInt16(address, Convert.ToUInt16(value));
                case DataType.SHORT:
                    return writer.WriteInt16(address, Convert.ToInt16(value));
                case DataType.DWORD:
                    return writer.WriteUInt32(address, Convert.ToUInt32(value));
                case DataType.INT:
                    return writer.WriteInt32(address, Convert.ToInt32(value));
                case DataType.FLOAT:
                    return writer.WriteFloat(address, Convert.ToSingle(value));
                case DataType.STR:
                    return writer.WriteString(address, value.ToString());
            }
            return -1;
        }

        public static HistoryData[] BatchRead(DataSource source, params ITag[] itemArray)
        {
            int len = itemArray.Length;
            HistoryData[] values = new HistoryData[len];
            for (int i = 0; i < len; i++)
            {
                itemArray[i].Refresh(source);
                values[i].ID = itemArray[i].ID;
                values[i].Value = itemArray[i].Value;
                values[i].TimeStamp = itemArray[i].TimeStamp;
            }
            return values;
        }

        public static int BatchWrite(IDictionary<ITag, object> items)
        {
            int rev = 0;
            foreach (var tag in items)
            {
                if (tag.Key.Write(tag.Value) < 0)
                    rev = -1;
            }
            return rev;
        }

        public static List<PDUArea> AssignFromPDU(this ICache cacheReader, int PDU, params DeviceAddress[] addrsArr)
        {
            List<PDUArea> rangeList = new List<PDUArea>();
            int count = addrsArr.Length;
            if (count > 0)
            {
                //Array.Sort(addrsArr);
                DeviceAddress start = addrsArr[0];
                start.Bit = 0;
                int bitCount = cacheReader.ByteCount;
                if (count > 1)
                {
                    int cacheLength = 0;//缓冲区的大小
                    int cacheIndexStart = 0;
                    int startIndex = 0;
                    DeviceAddress segmentEnd, tagAddress;
                    DeviceAddress segmentStart = start;
                    for (int j = 1, i = 1; i < count; i++, j++)
                    {
                        tagAddress = addrsArr[i];//当前变量地址 
                        int offset1 = cacheReader.GetOffset(tagAddress, segmentStart);
                        if (offset1 > (PDU / cacheReader.ByteCount))
                        {
                            segmentEnd = addrsArr[i - 1];
                            int len = cacheReader.GetOffset(segmentEnd, segmentStart);
                            len += segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + len);
                            addrsArr[i] = tagAddress;
                            rangeList.Add(new PDUArea(segmentStart, len, startIndex, j));
                            startIndex += j; j = 0;
                            cacheLength += len;//更新缓存长度
                            cacheIndexStart = cacheLength;
                            segmentStart = tagAddress;//更新数据片段的起始地址
                        }
                        else
                        {
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + offset1);
                            addrsArr[i] = tagAddress;
                        }
                        if (i == count - 1)
                        {
                            segmentEnd = addrsArr[i];
                            int segmentLength = cacheReader.GetOffset(segmentEnd, segmentStart);
                            if (segmentLength > PDU / cacheReader.ByteCount)
                            {
                                segmentEnd = addrsArr[i - 1];
                                segmentLength = segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            }
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + segmentLength);
                            addrsArr[i] = tagAddress;
                            segmentLength += segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            rangeList.Add(new PDUArea(segmentStart, segmentLength, startIndex, j + 1));
                            cacheLength += segmentLength;
                        }
                    }
                    cacheReader.Size = cacheLength;
                }
                else
                    cacheReader.Size = start.DataSize <= bitCount ? 1 : start.DataSize / bitCount;//改变Cache的Size属性值将创建Cache的内存区域
            }
            return rangeList;
        }

        //调用前应对地址数组排序(是否加锁？)
        public static ItemData<Storage>[] PLCReadMultiple(this IPLCDriver plc, ICache cache, DeviceAddress[] addrsArr)
        {
            if (addrsArr == null || cache == null || addrsArr.Length == 0) return null;
            int len = addrsArr.Length;
            ItemData<Storage>[] items = new ItemData<Storage>[len];
            int offset = 0; long now = DateTime.Now.ToFileTime();
            List<PDUArea> areas = cache.AssignFromPDU(plc.PDU, addrsArr);
            foreach (PDUArea area in areas)
            {
                byte[] rcvBytes = plc.ReadBytes(area.Start, (ushort)area.Len);
                Buffer.BlockCopy(rcvBytes, 0, cache.Cache, offset, rcvBytes.Length);
                offset += rcvBytes.Length / cache.ByteCount;
            }
            for (int i = 0; i < len; i++)
            {
                switch (addrsArr[i].VarType)
                {
                    case DataType.BOOL:
                        items[i].Value.Boolean = cache.ReadBit(addrsArr[i]).Value;
                        break;
                    case DataType.BYTE:
                        items[i].Value.Byte = cache.ReadByte(addrsArr[i]).Value;
                        break;
                    case DataType.WORD:
                        items[i].Value.Word = cache.ReadUInt16(addrsArr[i]).Value;
                        break;
                    case DataType.SHORT:
                        items[i].Value.Int16 = cache.ReadInt16(addrsArr[i]).Value;
                        break;
                    case DataType.DWORD:
                        items[i].Value.DWord = cache.ReadUInt32(addrsArr[i]).Value;
                        break;
                    case DataType.INT:
                        items[i].Value.Int32 = cache.ReadInt32(addrsArr[i]).Value;
                        break;
                    case DataType.FLOAT:
                        items[i].Value.Single = cache.ReadFloat(addrsArr[i]).Value;
                        break;
                    case DataType.STR:
                        var item = cache.ReadString(addrsArr[i], addrsArr[i].DataSize);
                        break;
                }
                items[i].Quality = QUALITIES.QUALITY_GOOD;
                items[i].TimeStamp = now;
            }
            return items;
        }

        public static int PLCWriteMultiple(this IPLCDriver plc, ICache cache, DeviceAddress[] addrArr, object[] buffer, int limit)
        {
            if (cache == null || addrArr == null || buffer == null || addrArr.Length != buffer.Length) return -1;
            if (addrArr.Length == 1) return plc.WriteValue(addrArr[0], buffer[0]);
            lock (plc)//不锁定会有并发冲突问题；锁定也不能保障绝对安全，如有人现场操作会导致数据刷新
            {
                List<PDUArea> areas = cache.AssignFromPDU(plc.PDU, addrArr);
                int offset = 0;
                foreach (PDUArea area in areas)
                {
                    byte[] rcvBytes = plc.ReadBytes(area.Start, (ushort)area.Len);
                    if (rcvBytes == null) return -1;
                    Buffer.BlockCopy(rcvBytes, 0, cache.Cache, offset, rcvBytes.Length);
                    offset += rcvBytes.Length / cache.ByteCount;
                }
                DeviceAddress start = addrArr[0];
                int startIndex = 0;
                int endIndex = 0;
                while (endIndex < addrArr.Length)
                {
                    if (start.Area != addrArr[endIndex].Area || start.DBNumber != addrArr[endIndex].DBNumber || endIndex - startIndex >= limit)
                    {
                        for (int i = startIndex; i < endIndex; i++)
                        {
                            cache.WriteValue(addrArr[i], buffer[i]);
                        }
                        int c1 = start.CacheIndex; int c2 = addrArr[endIndex - 1].CacheIndex;
                        byte[] bytes = new byte[cache.ByteCount * (c2 - c1 + 1)];
                        Buffer.BlockCopy(cache.Cache, c1, bytes, 0, bytes.Length);
                        if (plc.WriteBytes(start, bytes) < 0) return -1;
                        start = addrArr[endIndex];
                        startIndex = endIndex;
                    }
                    endIndex++;
                }
            }
            return 0;
        }
        /// <summary>
        /// string RightFrom
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RightFrom(this string text, int index)
        {
            return text.Substring(index + 1, text.Length - index - 1);
        }

        public static string Right(this string text, int length)
        {
            return text.Substring(text.Length - length, length);
        }

        /// <summary>
        /// Convert to Datetime
        /// </summary>
        /// <param name="filetime"></param>
        /// <returns></returns>

        public static DateTime ToDateTime(this long filetime)
        {
            return filetime == 0 ? DateTime.Now : DateTime.FromFileTime(filetime);
        }

        public static Type ToType(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.BOOL:
                    return typeof(bool);
                case DataType.BYTE:
                    return typeof(byte);
                case DataType.WORD:
                    return typeof(ushort);
                case DataType.SHORT:
                    return typeof(short);
                case DataType.INT:
                    return typeof(int);
                case DataType.DWORD:
                    return typeof(uint);
                case DataType.FLOAT:
                    return typeof(float);
                case DataType.STR:
                    return typeof(string);
                default:
                    return typeof(object);
            }
        }

        public static string ToFormatString(this int num, int len)
        {
            string str = num.ToString();
            int off = len - str.Length;
            return off > 0 ? string.Concat(new string('0', off), str) : str;
        }

        public static bool IsEquals(this byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

        public static string ConvertToString(this byte[] bits)
        {
            char[] chars = new char[bits.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                chars[i] = (char)bits[i];
            }
            return new string(chars);
        }

        public static byte[] ConvertToArray(this string bits)
        {
            var chars = bits.ToCharArray();
            byte[] arr = new byte[chars.Length];
            for (int i = 0; i < chars.Length; i++)
            {
                arr[i] = (byte)chars[i];
            }
            return arr;
        }

        public static int BitSwap(this byte bit)
        {
            return (bit < 8 ? bit + 8 : bit - 8);
        }

        [Obsolete]
        public static Storage ToStorage(this ITag tag, object obj)
        {
            Storage value = Storage.Empty;
            var str = obj as string;
            switch (tag.Address.VarType)
            {
                case DataType.BOOL:
                    value.Boolean = str == null ? Convert.ToBoolean(obj) : str == "0" ? false : str == "1" ? true : bool.Parse(str);
                    break;
                case DataType.BYTE:
                    value.Byte = Convert.ToByte(obj);
                    break;
                case DataType.WORD:
                    value.Word = Convert.ToUInt16(obj);
                    break;
                case DataType.SHORT:
                    value.Int16 = Convert.ToInt16(obj);
                    break;
                case DataType.DWORD:
                    value.DWord = Convert.ToUInt32(obj);
                    break;
                case DataType.INT:
                    value.Int32 = Convert.ToInt32(obj);
                    break;
                case DataType.FLOAT:
                    value.Single = Convert.ToSingle(obj);
                    break;
            }
            return value;
        }

        public static byte[] ToByteArray(this ITag tag)
        {
            switch (tag.Address.VarType)
            {
                case DataType.BOOL:
                    return new byte[] { tag.Value.Boolean ? (byte)1 : (byte)0 };
                case DataType.BYTE:
                    return new byte[] { tag.Value.Byte };
                case DataType.WORD:
                    return BitConverter.GetBytes(tag.Value.Word);
                case DataType.SHORT:
                    return BitConverter.GetBytes(tag.Value.Int16);
                case DataType.DWORD:
                    return BitConverter.GetBytes(tag.Value.DWord);
                case DataType.INT:
                    return BitConverter.GetBytes(tag.Value.Int32);
                case DataType.FLOAT:
                    return BitConverter.GetBytes(tag.Value.Single);
                case DataType.STR:
                    return Encoding.ASCII.GetBytes(tag.ToString());
                default:
                    return new byte[0];
            }
        }

        public static byte[] ToByteArray(this ITag tag, Storage value)
        {
            switch (tag.Address.VarType)
            {
                case DataType.BOOL:
                    return new byte[] { value.Boolean ? (byte)1 : (byte)0 };
                case DataType.BYTE:
                    return new byte[] { value.Byte };
                case DataType.WORD:
                    return BitConverter.GetBytes(value.Word);
                case DataType.SHORT:
                    return BitConverter.GetBytes(value.Int16);
                case DataType.DWORD:
                    return BitConverter.GetBytes(value.DWord);
                case DataType.INT:
                    return BitConverter.GetBytes(value.Int32);
                case DataType.FLOAT:
                    return BitConverter.GetBytes(value.Single);
                case DataType.STR:
                    return Encoding.ASCII.GetBytes(tag.ToString());
                default:
                    return new byte[0];
            }
        }

        public static object GetValue(this ITag tag, Storage value)
        {
            switch (tag.Address.VarType)
            {
                case DataType.BOOL:
                    return value.Boolean;
                case DataType.BYTE:
                    return value.Byte;
                case DataType.WORD:
                    return value.Word;
                case DataType.SHORT:
                    return value.Int16;
                case DataType.DWORD:
                    return value.DWord;
                case DataType.INT:
                    return value.Int32;
                case DataType.FLOAT:
                    return value.Single;
                case DataType.STR:
                    return tag.ToString();
                default:
                    return null;
            }
        }

        public static float ValueToScale(this ITag tag, float value)
        {
            IDataServer srv = tag.Parent.Server;
            int ind = srv.GetScaleByID(tag.ID);
            Scaling meta = ind < 0 ? Scaling.Empty : srv.ScalingList[ind];
            if (meta.ScaleType == ScaleType.None)
            {
                return value;
            }
            else
            {
                double temp = (value - meta.EULo) / (meta.EUHi - meta.EULo);
                if (meta.ScaleType == ScaleType.SquareRoot)
                    temp = temp * temp;
                return (meta.RawHi - meta.RawLo) * (float)temp + meta.RawLo;
            }
        }

        public static float ScaleToValue(this ITag tag, Storage value)
        {
            DataType type = tag.Address.VarType;
            if (type == DataType.BOOL) return value.Boolean ? 1f : 0f;
            IDataServer srv = tag.Parent.Server;
            int ind = srv.GetScaleByID(tag.ID);
            Scaling meta = ind < 0 ? Scaling.Empty : srv.ScalingList[ind];
            if (meta.ScaleType == ScaleType.None)
            {
                switch (type)
                {
                    case DataType.BYTE:
                        return value.Byte;
                    case DataType.WORD:
                        return value.Word;
                    case DataType.SHORT:
                        return value.Int16;
                    case DataType.DWORD:
                        return value.DWord;
                    case DataType.INT:
                        return value.Int32;
                    case DataType.FLOAT:
                        return value.Single;
                    case DataType.STR:
                        return float.Parse(tag.ToString());
                    default:
                        return 0f;
                }
            }
            else
            {
                double temp;
                switch (type)
                {
                    case DataType.BYTE:
                        temp = (value.Byte - meta.RawLo) / (meta.RawHi - meta.RawLo);
                        break;
                    case DataType.WORD:
                        temp = (value.Word - meta.RawLo) / (meta.RawHi - meta.RawLo);
                        break;
                    case DataType.SHORT:
                        temp = (value.Int16 - meta.RawLo) / (meta.RawHi - meta.RawLo);
                        break;
                    case DataType.DWORD:
                        temp = (value.DWord - meta.RawLo) / (meta.RawHi - meta.RawLo);
                        break;
                    case DataType.INT:
                        temp = (value.Int32 - meta.RawLo) / (meta.RawHi - meta.RawLo);
                        break;
                    case DataType.FLOAT:
                        temp = (value.Single - meta.RawLo) / (meta.RawHi - meta.RawLo);
                        break;
                    default:
                        return 0f;
                }
                if (meta.ScaleType == ScaleType.SquareRoot)
                    temp = Math.Sqrt(temp);
                return (meta.EUHi - meta.EULo) * (float)temp + meta.EULo;
            }
        }

        public static float ScaleToValue(this ITag tag)
        {
            return ScaleToValue(tag, tag.Value);
        }

        public static string GetTagName(this ITag tag)
        {
            IDataServer srv = tag.Parent.Server;
            int ind = srv.GetItemProperties(tag.ID);
            return ind < 0 ? null : srv.MetaDataList[ind].Name;
        }

        public static string GetTagName(this IDataServer srv, short id)
        {
            int ind = srv.GetItemProperties(id);
            return ind < 0 ? null : srv.MetaDataList[ind].Name;
        }

        public static TagMetaData GetMetaData(this ITag tag)
        {
            IDataServer srv = tag.Parent.Server;
            int index = srv.GetItemProperties(tag.ID);
            return index < 0 ? new TagMetaData() : srv.MetaDataList[index];
        }
    }

    public static class Utility
    {
        private static readonly ushort[] crcTable = {
            0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
            0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
            0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
            0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
            0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
            0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
            0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
            0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
            0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
            0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
            0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
            0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
            0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
            0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
            0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
            0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
            0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
            0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
            0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
            0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
            0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
            0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
            0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
            0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
            0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
            0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
            0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
            0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
            0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
            0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
            0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
            0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040
        };

        /// <summary>
        /// Converts an array of bytes to an ASCII byte array
        /// </summary>
        /// <param name="numbers">The byte array</param>
        /// <returns>An array of ASCII byte values</returns>
        public static string GetAsciiBytes(byte[] numbers)
        {
            string str = string.Empty;
            for (int i = 0; i < numbers.Length; i++)
            {
                str += numbers[i].ToString("X2");
            }
            return str;
        }

        /// <summary>
        /// Converts an array of UInt16 to an ASCII byte array
        /// </summary>
        /// <param name="numbers">The ushort array</param>
        /// <returns>An array of ASCII byte values</returns>
        public static string GetAsciiBytes(ushort[] numbers)
        {
            string str = string.Empty;
            for (int i = 0; i < numbers.Length; i++)
            {
                str += numbers[i].ToString("X4");
            }
            return str;
        }

        /// <summary>
        /// Converts a network order byte array to an array of UInt16 values in host order
        /// </summary>
        /// <param name="networkBytes">The network order byte array</param>
        /// <returns>The host order ushort array</returns>
        public static ushort[] NetworkBytesToHostUInt16(byte[] networkBytes)
        {
            if (networkBytes == null)
                throw new ArgumentNullException("networkBytes");

            if (networkBytes.Length % 2 != 0)
                throw new FormatException("NetworkBytesNotEven");

            ushort[] result = new ushort[networkBytes.Length / 2];

            for (int i = 0; i < result.Length; i++)
                result[i] = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(networkBytes, i * 2));

            return result;
        }

        /// <summary>
        /// Converts a hex string to a byte array.
        /// </summary>
        /// <param name="hex">The hex string</param>
        /// <returns>Array of bytes</returns>
        public static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                throw new ArgumentNullException("hex");

            if (hex.Length % 2 != 0)
                throw new FormatException("HexCharacterCountNotEven");

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            return bytes;
        }

        /// <summary>
        /// Calculate Longitudinal Redundancy Check.
        /// </summary>
        /// <param name="data">The data used in LRC</param>
        /// <returns>LRC value</returns>
        public static byte CalculateLrc(byte[] data, int len = 0)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (len == 0) len = data.Length;
            byte lrc = 0;
            for (int i = 0; i < len; i++)
            { lrc += data[i]; }

            lrc = (byte)((lrc ^ 0xFF) + 1);

            return lrc;
        }

        /// <summary>
        /// Calculate Cyclical Redundancy Check
        /// </summary>
        /// <param name="data">The data used in CRC</param>
        /// <returns>CRC value</returns>
        public static byte[] CalculateCrc(byte[] data, int len = 0)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (len == 0) len = data.Length;
            ushort crc = ushort.MaxValue;
            for (int i = 0; i < len; i++)
            {
                byte tableIndex = (byte)(crc ^ data[i]);
                crc >>= 8;
                crc ^= crcTable[tableIndex];
            }

            return BitConverter.GetBytes(crc);
        }

        public static bool CheckSumCRC(byte[] frame)
        {
            int len = frame.Length;
            byte[] chk = CalculateCrc(frame, len - 2);
            return (chk[0] == frame[len - 2] && chk[1] == frame[len - 1]);
        }

        public static unsafe short NetToInt16(byte[] value, int startIndex)
        {
            if (value == null || startIndex > value.Length)
            {
                throw new NotImplementedException();
            }
            if (startIndex > value.Length - 2)
                return value[value.Length - startIndex];
            fixed (byte* numRef = &(value[startIndex]))
            {
                return (short)((numRef[0] << 8) | numRef[1]);
            }
        }

        public static unsafe int NetToInt32(byte[] value, int startIndex)
        {
            if (value == null || startIndex > value.Length)
            {
                throw new NotImplementedException();
            }
            if (startIndex > value.Length - 4)
                return value[value.Length - startIndex];
            fixed (byte* numRef = &(value[startIndex]))
            {
                return (int)((numRef[0] << 24) | (numRef[1] << 16) | (numRef[2] << 8) | numRef[3]);
            }
        }

        public static unsafe float NetToSingle(byte[] value, int startIndex)
        {
            int a = NetToInt32(value, startIndex);
            return *(float*)&a;
        }

        public static string ConvertToString(byte[] bytes, int start = 0, int len = 0)
        {
            //西门子300、400
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
            var klen = bytes[start + 1];
            return Encoding.ASCII.GetString(bytes, start + 2, klen).Trim((char)0);
        }

        public static ushort ReverseInt16(short value)
        {
            ushort low = (ushort)(((ushort)value & (ushort)0xFFU) << 8);
            ushort high = (ushort)(((ushort)value & (ushort)0xFF00U) >> 8);
            return (ushort)(low | high);
        }

        /// <summary>
        /// 掐头去尾得到数据字符串 
        /// </summary>
        /// <param name="allStr">全部字符串</param>
        /// <param name="startStr">头字符串</param>
        /// <param name="endStr">尾字符串</param>
        /// <returns></returns>
        public static string Pinchstring(string allStr, string startStr, string endStr)
        {
            int i1 = allStr.IndexOf(startStr);
            int i2 = allStr.IndexOf(endStr);
            string result;
            try
            {
                result = allStr.Substring(i1 + startStr.Length, i2 - i1 - startStr.Length);
            }
            catch
            {
                return string.Empty;
            }
            return result;
        }

        /// <summary>
        /// 异或校验
        /// </summary>
        /// <param name="xorStr">传进来进行校验的字符串</param>
        /// <returns>校验码</returns>
        public static string XorCheck(string xorStr)
        {

            try
            {
                //小写转换成大写，因为XOR校验区分大小写
                /*****************************************************
                * XOR校验（异或校验）
                * VerifyByte是得到的校验码
                ***************************************************/
                xorStr = xorStr.ToUpper();
                byte[] bt = Encoding.Default.GetBytes(xorStr);
                byte VerifyByte = bt[0];
                for (int i = 1; i < bt.Length; i++)
                {
                    VerifyByte = (byte)(VerifyByte ^ bt[i]);
                }

                /**********************************
                 * 校验码如果小于0X10则十位补零
                 * **********************************/
                string xor = "";
                if (VerifyByte < 16)
                {
                    xor = "0" + VerifyByte.ToString("X");
                }
                else
                {
                    xor = VerifyByte.ToString("X");
                }
                return xor;
            }
            catch
            {
                throw;
            }
        }
    }
}
