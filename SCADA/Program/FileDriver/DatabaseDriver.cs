using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using DataService;

namespace FileDriver
{
    [Description("SQL 数据库")]
    public class DataBaseReader : IFileDriver, IMultiReadWrite
    {
        public bool IsClosed
        {
            get
            {
                return m_Conn == null || m_Conn.State != ConnectionState.Open;
            }
        }

        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        int _timeOut;
        public int TimeOut
        {
            get { return m_Conn == null ? _timeOut : m_Conn.ConnectionTimeout; }
            set
            {
                _timeOut = value;
            }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        string _serverIP = ".";
        public string ServerName
        {
            get
            {
                return _serverIP;
            }
            set
            {
                _serverIP = value;
            }
        }

        string _ins;
        public string Instance
        {
            get
            {
                return _ins;
            }
            set
            {
                _ins = value;
            }
        }

        string _database;
        public string FileName
        {
            get
            {
                return _database;
            }
            set
            {
                _database = value;
            }
        }

        List<IGroup> _groups = new List<IGroup>();
        public IEnumerable<IGroup> Groups
        {
            get { return _groups; }
        }

        IDataServer _server;
        public IDataServer Parent
        {
            get { return _server; }
        }

        public DataBaseReader(IDataServer parent, short id, string name)
        {
            _id = id;
            _name = name;
            _server = parent;
        }

        SqlConnection m_Conn;
        public bool Connect()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = (_ins == null ? _serverIP : string.Format(@"{0}\{1}", _serverIP, _ins));
            builder.InitialCatalog = _database;
            builder.ConnectTimeout = _timeOut;
            builder.IntegratedSecurity = true;
            m_Conn = new SqlConnection(builder.ConnectionString);
            //mySqlConnection.ConnectionTimeout = 1;//设置连接超时的时间,写在连接字符串内
            try
            {
                //Open DataBase
                //打开数据库
                m_Conn.Open();
                return m_Conn.State == ConnectionState.Open;
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(e.Message));
                return false;
            }
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            FileDeviceGroup grp = new FileDeviceGroup(id, name, updateRate, active, this);
            _groups.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _groups.Remove(grp);
        }

        public void Dispose()
        {
            foreach (IGroup grp in _groups)
            {
                grp.Dispose();
            }
            _groups.Clear();
            m_Conn.Close(); m_Conn.Dispose();
        }

        public SqlDataReader ExecuteProcedureReader(string sSQL, params SqlParameter[] ParaName)
        {
            SqlCommand command = new SqlCommand(sSQL, m_Conn);
            command.CommandType = CommandType.StoredProcedure;
            if (ParaName != null)
            {
                command.Parameters.AddRange(ParaName);
            }
            try
            {
                if (m_Conn.State == ConnectionState.Closed)
                    m_Conn.Open();
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(e.Message));
                return null;
            }
        }

        public int ExecuteStoredProcedure(string ProName, params SqlParameter[] ParaName)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                cmd.CommandType = CommandType.StoredProcedure;
                if (ParaName != null)
                {
                    cmd.Parameters.AddRange(ParaName);
                }
                SqlParameter param = new SqlParameter();
                cmd.Parameters.Add(param);
                param.Direction = ParameterDirection.ReturnValue;
                if (m_Conn.State == ConnectionState.Closed)
                    m_Conn.Open();
                cmd.ExecuteNonQuery();
                m_Conn.Close();
                return (int)param.Value;
            }
            catch (Exception e)
            {
                m_Conn.Close();
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(e.Message));
                return -1;
            }
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            using (var dataReader = ExecuteProcedureReader("ReadValueByID",
              new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
              new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType }))
            {
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        return dataReader[0] as byte[];
                    }
                }
            }
            return null;
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            ItemData<int> data = new ItemData<int>();
            var dataReader = ExecuteProcedureReader("ReadValueByID",
              new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
              new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType });
            if (dataReader == null)
            {
                data.Quality = QUALITIES.QUALITY_BAD;
                return data;
            }
            while (dataReader.Read())
            {
                data.Value = dataReader.GetInt32(0);
                data.Quality = QUALITIES.QUALITY_GOOD;
            }
            dataReader.Close();
            return data;
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            var res = ReadInt32(address);
            return new ItemData<uint>((uint)res.Value, res.TimeStamp, res.Quality);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            var res = ReadInt16(address);
            return new ItemData<ushort>((ushort)res.Value, res.TimeStamp, res.Quality);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            ItemData<short> data = new ItemData<short>();
            var dataReader = ExecuteProcedureReader("ReadValueByID",
               new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
               new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType });
            if (dataReader == null)
            {
                data.Quality = QUALITIES.QUALITY_BAD;
                return data;
            }
            while (dataReader.Read())
            {
                data.Value = dataReader.GetInt16(0);
                data.Quality = QUALITIES.QUALITY_GOOD;
            }
            dataReader.Close();
            return data;
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            ItemData<byte> data = new ItemData<byte>();
            var dataReader = ExecuteProcedureReader("ReadValueByID",
                  new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                  new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType });
            if (dataReader == null)
            {
                data.Quality = QUALITIES.QUALITY_BAD;
                return data;
            }
            while (dataReader.Read())
            {
                data.Value = dataReader.GetByte(0);
                data.Quality = QUALITIES.QUALITY_GOOD;
            }
            dataReader.Close();
            return data;
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            ItemData<string> data = new ItemData<string>();
            var dataReader = ExecuteProcedureReader("ReadValueByID",
                          new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                          new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType });
            if (dataReader == null)
            {
                data.Quality = QUALITIES.QUALITY_BAD;
                return data;
            }
            while (dataReader.Read())
            {
                data.Value = dataReader.GetString(0);
                data.Quality = QUALITIES.QUALITY_GOOD;
            }
            dataReader.Close();
            return data;
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            ItemData<float> data = new ItemData<float>();
            var dataReader = ExecuteProcedureReader("ReadValueByID",
                  new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                  new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType });
            if (dataReader == null)
            {
                data.Quality = QUALITIES.QUALITY_BAD;
                return data;
            }
            while (dataReader.Read())
            {
                data.Value = dataReader.GetFloat(0);
                data.Quality = QUALITIES.QUALITY_GOOD;
            }
            dataReader.Close();
            return data;
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            ItemData<bool> data = new ItemData<bool>();
            var dataReader = ExecuteProcedureReader("ReadValueByID",
                 new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                   new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType });
            if (dataReader == null)
            {
                data.Quality = QUALITIES.QUALITY_BAD;
                return data;
            }
            while (dataReader.Read())
            {
                data.Value = dataReader.GetBoolean(0);
                data.Quality = QUALITIES.QUALITY_GOOD;
            }
            dataReader.Close();
            return data;
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            ItemData<object> data = new ItemData<object>();
            var dataReader = ExecuteProcedureReader("ReadValueByID",
                new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                  new SqlParameter("@DATATYPE", SqlDbType.TinyInt) { Value = address.VarType });
            if (dataReader == null)
            {
                data.Quality = QUALITIES.QUALITY_BAD;
                return data;
            }
            while (dataReader.Read())
            {
                data.Value = dataReader.GetValue(0);
                data.Quality = QUALITIES.QUALITY_GOOD;
            }
            dataReader.Close();
            return data;
        }

        public int WriteBytes(DeviceAddress address, byte[] bits)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                 new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                 new SqlParameter("@Value", SqlDbType.Variant) { Value = bits });
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                new SqlParameter("@Value", SqlDbType.Variant) { Value = bit });
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                 new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                 new SqlParameter("@Value", SqlDbType.Variant) { Value = bits });
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                 new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                 new SqlParameter("@Value", SqlDbType.Variant) { Value = value });
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                 new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                 new SqlParameter("@Value", SqlDbType.Variant) { Value = value });
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                  new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                  new SqlParameter("@Value", SqlDbType.Variant) { Value = value });
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                  new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                  new SqlParameter("@Value", SqlDbType.Variant) { Value = value });
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                new SqlParameter("@Value", SqlDbType.Variant) { Value = value });
        }

        public int WriteString(DeviceAddress address, string str)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                new SqlParameter("@Value", SqlDbType.Variant, address.DataSize) { Value = str });
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return ExecuteStoredProcedure("UpdateValueByID",
                new SqlParameter("@ID", SqlDbType.SmallInt) { Value = address.CacheIndex },
                new SqlParameter("@Value", SqlDbType.Variant, address.DataSize) { Value = value });
        }


        public ItemData<Storage>[] ReadMultiple(DeviceAddress[] addrsArr)
        {
            int len = addrsArr.Length;
            SqlParameter param = new SqlParameter("@IDARRAY", SqlDbType.VarChar);
            string str = string.Empty;
            for (int i = 0; i < len; i++)
            {
                str += addrsArr[i].CacheIndex + '|' + (int)addrsArr[i].VarType + ',';
            }
            param.Value = str;
            var dataReader = ExecuteProcedureReader("BatchReadByID", param);//可以采用IN 操作符+,
            if (dataReader == null)
                return null;
            else
            {
                ItemData<Storage>[] itemArr = new ItemData<Storage>[len];
                int i = 0;
                while (dataReader.Read())
                {
                    switch (addrsArr[i].VarType)
                    {
                        case DataType.BOOL:
                            itemArr[i].Value.Boolean = dataReader.GetBoolean(0);
                            break;
                        case DataType.BYTE:
                            itemArr[i].Value.Byte = dataReader.GetByte(0);
                            break;
                        case DataType.WORD:
                            itemArr[i].Value.Word = (ushort)dataReader.GetInt16(0);
                            break;
                        case DataType.SHORT:
                            itemArr[i].Value.Int16 = dataReader.GetInt16(0);
                            break;
                        case DataType.DWORD:
                            itemArr[i].Value.DWord = (uint)dataReader.GetInt32(0);
                            break;
                        case DataType.INT:
                            itemArr[i].Value.Int32 = dataReader.GetInt32(0);
                            break;
                        case DataType.FLOAT:
                            itemArr[i].Value.Single = dataReader.GetFloat(0);
                            break;
                    }
                    i++;
                }
                dataReader.Close();
                return itemArr;
            }
        }

        public int WriteMultiple(DeviceAddress[] addrArr, object[] buffer)
        {
            int len = addrArr.Length;
            SqlParameter param = new SqlParameter("@IDARRAY", SqlDbType.VarChar);
            string str = string.Empty;
            for (int i = 0; i < len; i++)
            {
                str += addrArr[i].CacheIndex.ToString() + "|" + buffer[i].ToString() + ",";
            }
            param.Value = str;
            return ExecuteStoredProcedure("BatchUpdateByID", param);
        }

        public FileData[] ReadAll(short groupId)
        {
            FileData[] list = null;
            using (var reader = ExecuteProcedureReader("ReadAll", new SqlParameter("@GroupId", groupId)))
            {
                if (reader == null) return null;
                if (reader.Read())
                {
                    int count = reader.GetInt32(0);
                    list = new FileData[count];
                    reader.NextResult();
                    int i = 0;
                    while (reader.Read())
                    {
                        if (i < count)
                        {
                            list[i].ID = reader.GetInt16(0);
                            switch ((DataType)reader.GetByte(1))
                            {
                                case DataType.BOOL:
                                    list[i].Value.Boolean = Convert.ToBoolean(reader.GetValue(2));
                                    break;
                                case DataType.BYTE:
                                    list[i].Value.Byte = Convert.ToByte(reader.GetValue(2));
                                    break;
                                case DataType.WORD:
                                    list[i].Value.Word = Convert.ToUInt16(reader.GetValue(2));
                                    break;
                                case DataType.SHORT:
                                    list[i].Value.Int16 = Convert.ToInt16(reader.GetValue(2));
                                    break;
                                case DataType.DWORD:
                                    list[i].Value.DWord = Convert.ToUInt32(reader.GetValue(2));
                                    break;
                                case DataType.INT:
                                    list[i].Value.Int32 = Convert.ToInt32(reader.GetValue(2));
                                    break;
                                case DataType.FLOAT:
                                    list[i].Value.Single = Convert.ToSingle(reader.GetValue(2));
                                    break;
                                case DataType.STR:
                                    list[i].Text = Convert.ToString(reader.GetValue(2));
                                    //如何传送字符串？
                                    break;
                            }
                            i++;
                        }
                    }
                }
            }
            return list;
        }

        public int Limit
        {
            get { return 0x100; }
        }

        public event IOErrorEventHandler OnError;

    }
}