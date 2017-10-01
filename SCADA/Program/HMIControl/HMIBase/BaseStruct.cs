using System;
using System.Collections.Generic;
using System.Windows;

namespace HMIControl
{
    public struct ConnectInfo
    {
        public Rect DesignerRect;

        public Point Position;

        public ConnectOrientation Orient;

        public static readonly ConnectInfo Empty;

        static ConnectInfo()
        {
            Empty = new ConnectInfo();
        }

        public static bool operator ==(ConnectInfo info1, ConnectInfo info2)
        {
            return ((info1.DesignerRect == info2.DesignerRect) &&
                    (info1.Position == info2.Position) &&
                    (info1.Orient == info2.Orient));
        }

        public static bool operator !=(ConnectInfo info1, ConnectInfo info2)
        {
            return !(info1 == info2);
        }

        public static bool Equals(ConnectInfo info1, ConnectInfo info2)
        {
            return info1 == info2;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is ConnectInfo)) return false;
            return Equals(this, (ConnectInfo)obj);
        }

        public override int GetHashCode()
        {
            return this.DesignerRect.GetHashCode() ^ this.Position.GetHashCode() ^ this.Orient.GetHashCode();
        }
    }

    public enum ConnectOrientation
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    public interface ITagLink
    {
        string Node { get; }
    }

    public interface ITagReader : ITagLink
    {
        string TagReadText { get; set; }
        string[] GetActions();
        Action SetTagReader(string key, Delegate tagChanged);
        IList<ITagLink> Children { get; }
    }

    public interface ITagWindow : ITagLink
    {
        bool IsModel { get; set; }
        bool IsUnique { get; set; }
        string TagWindowText { get; set; }
    }

    public interface ITagWriter : ITagLink
    {
        bool IsPulse { get; set; }
        string TagWriteText { get; set; }
        bool SetTagWriter(IEnumerable<Delegate> tagWriter);
    }    

    public class TagActions
    {
        public const string RUN = "运行";
        public const string ALARM = "报警";
        public const string SP = "理论值";
        public const string PV = "实际值";
        public const string BYPASS = "旁通";
        public const string RAWNAME = "料名";
        public const string CAPTION = "标题";
        public const string TEXT = "文本";
        public const string ON = "开";
        public const string OFF = "关";
        public const string ONOFF = "开/关";
        public const string START = "启动";
        public const string STOP = "停止";
        public const string DEVICENAME = "设备名";
        public const string ONALARM = "开不到位";
        public const string OFFALARM = "关不到位";
        public const string LEFTALARM = "左不到位";
        public const string RIGHTALARM = "右不到位";
        public const string WARN = "提醒";
        public const string PRESS = "按下按钮";
        public const string HIGHLEVEL = "高料位";
        public const string LOWLEVEL = "低料位";
        public const string SPEED = "速度";
        public const string AMPS = "电流";
        public const string LEFT = "左";
        public const string RIGHT = "右";
        public const string MID = "中";
        public const string STATE = "状态变化";
        public const string STATE1 = "状态1";
        public const string STATE2 = "状态2";
        public const string STATE3 = "状态3";
        public const string STATE4 = "状态4";
        public const string STATE5 = "状态5";
        public const string STATE6 = "状态6";
        public const string STATE7 = "状态7";
        public const string STATE8 = "状态8";
        public const string VISIBLE = "可见性";
        public const string ENABLE = "使能";
        public const string DISABLE = "失效";
    }

}
