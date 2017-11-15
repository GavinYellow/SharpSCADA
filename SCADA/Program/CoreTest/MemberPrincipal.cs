using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using DatabaseLib;

namespace CoreTest
{
    [Flags]
    public enum RoleType
    {
        NONE = 0,
        OP = 1,
        Electrician = 2,
        QA = 4,
        Admin = 8
    }

    public class MemberPrincipal : IPrincipal
    {
        RoleType _roles = RoleType.NONE;
        SortedList<string, RoleType> _roleDict = new SortedList<string, RoleType>()
            {
                {"一般用户",RoleType.NONE},{"操作员",RoleType.OP}, {"中控员",RoleType.Electrician}, {"质检员",RoleType.QA}, {"管理员",RoleType.Admin},
                {"超级管理员",RoleType.NONE|RoleType.OP|RoleType.Electrician|RoleType.QA|RoleType.Admin}
            };

        MemberIdentity _identity;
        public IIdentity Identity
        {
            get { return _identity; }
        }

        public MemberPrincipal(MemberIdentity identity)
        {
            _identity = identity;
            using (var reader = DataHelper.Instance.ExecuteReader("SELECT ROLE FROM Membership WHERE UserName='" +
                _identity.Name + "'"))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        _roles = (RoleType)reader.GetInt16(0);
                    }
                }
            }
        }

        public bool IsInRole(string roleName)
        {
            RoleType role;
            if (_roleDict.TryGetValue(roleName, out role))
            {
                return _roles.HasFlag(role);
            }
            return false;
        }

        public override string ToString()
        {
            return _roles.ToString();
        }
    }

    public class MemberIdentity : IIdentity
    {
        const int LIMIT = 3;
        int _count;

        public MemberIdentity()
        {
            _authenticationType = "UserID_Password";
        }

        public MemberIdentity(string name)
        {
            _name = name;
            _authenticationType = "UserID_Password";
        }

        public MemberIdentity(string name, string type)
        {
            _name = name;
            _authenticationType = type;
        }

        public int Authenticate(string password)
        {
            if (password == null || password.Length > 10)
                return -1;
            if (IsExceeded) return -4;
            string sql = "SELECT Password FROM dbo.Membership WHERE Username='" + _name + "'";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string pass = reader.GetString(0);
                            if (pass == EncodePassword(password))
                            {
                                _isAuthenticated = true;
                                return 0;
                            }
                            else
                            {
                                _count++;
                                return -1;
                            }
                        }
                    }
                    else
                        return -2;

                }
                else
                    return -3;
            }
            return -5;
        }

        public static string EncodePassword(string input)
        {
            if (input == null) return null;
            using (MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider())
            {
                byte[] data = md5Hasher.ComputeHash(Encoding.ASCII.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static int CreateUser(string name, string password, RoleType role)
        {
            string sql = string.Format("DELETE FROM dbo.Membership WHERE UserName='{0}';"
               + "INSERT INTO dbo.Membership(UserName,Password,ROLE) VALUES('{0}','{1}',{2})",
               name, EncodePassword(password), (int)role);
            return DataHelper.Instance.ExecuteNonQuery(sql);
        }

        public void ChangeUser(string newName)
        {
            if (_name != newName)
            {
                _name = newName;
                _count = 0;
                _isAuthenticated = false;
            }
        }

        public int UpdatePassword(string oldPassword, string newPassword)
        {
            if (oldPassword == null || newPassword == null)
                return -8;
            if (newPassword.Length > 10) return -7;
            int result = 0;
            string sql = "SELECT Password FROM dbo.Membership WHERE Username='" + _name + "'";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string pass = reader.GetString(0);
                            if (pass == EncodePassword(oldPassword))
                            {
                                sql = "UPDATE dbo.Membership SET Password='" + EncodePassword(newPassword) +
                                    "' WHERE Username='" + _name + "'";
                                result = DataHelper.Instance.ExecuteNonQuery(sql);
                                if (result >= 0) result = 0;
                            }
                            else
                                result = -1;
                        }
                    }
                    else
                        result = -2;

                }
                else
                    result = -3;
            }
            return result;
        }

        public bool IsExceeded
        {
            get
            {
                return _count >= LIMIT;
            }
        }

        string _authenticationType;
        public string AuthenticationType
        {
            get { return _authenticationType; }
        }

        bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        string _name;
        public string Name
        {
            get { return _name; }
        }
    }
}
