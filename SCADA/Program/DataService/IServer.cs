using System;
using System.Collections.Generic;

namespace DataService
{
    //历史数据、报警数据的提交和数据均从服务器
    public interface IHDAServer : IDisposable
    {
        IEnumerable<HistoryData> ReadAtTime(params DateTime[] timeStamps);
        IEnumerable<HistoryData> ReadAtTime(short ID, params DateTime[] timeStamps);
        IEnumerable<HistoryData> ReadRaw(DateTime start, DateTime end);
        IEnumerable<HistoryData> ReadRaw(DateTime start, DateTime end, short ID);
        //IEnumerable<HistoryData> ReadProcessed(DateTime start, DateTime end, HDAAggregate aggregates, params short[] aggregateIDs);
    }

    public interface IAlarmServer : IDisposable
    {
        int DisableCondition(string sourceName, AlarmType type);
        int EnableCondition(string sourceName, AlarmType type);
        int RemoveConditon(string sourceName, AlarmType type);
        int RemoveConditons(string sourceName);
        int AckConditions(params ICondition[] conditions);
        IList<ICondition> QueryConditions(string sourceName);
        IEnumerable<AlarmItem> AlarmList { get; }
        IList<ICondition> ConditionList { get; }
        IList<ICondition> ActivedConditionList { get; }
    }

    public interface IDataServer : IDisposable
    {
        ITag this[short id] { get; }
        ITag this[string name] { get; }
        ExpressionEval Eval { get; }
        Object SyncRoot { get; }//对所有涉及集合更改项目使用；包括IGROUP的ADDITEMS
        IList<TagMetaData> MetaDataList { get; }
        IList<Scaling> ScalingList { get; }
        IEnumerable<IDriver> Drivers { get; }
        IEnumerable<string> BrowseItems(BrowseType browseType, string tagName, DataType dataType);
        IDriver AddDriver(short id, string name, string assembly, string className);
        IGroup GetGroupByName(string name);
        int GetScaleByID(short id);
        int GetItemProperties(short id);//返回的是元数据在元数据列表的索引
        bool RemoveDriver(IDriver device);
        bool AddItemIndex(string key, ITag value);
        bool RemoveItemIndex(string key);
        void ActiveItem(bool active, params ITag[] items);
        int BatchWrite(Dictionary<string, object> tags, bool sync);
        HistoryData[] BatchRead(DataSource source, bool sync, params ITag[] itemArray);
    }

    public class FCTCOMMAND
    {
        public const byte fctHead = 0xAB;//报头可加密，如报头不符，则不进行任何操作；客户端Socket发送报警请求，封装于Server
        public const byte fctHdaIdRequest = 30;
        public const byte fctHdaRequest = 31;
        public const byte fctAlarmRequest = 32;
        public const byte fctOrderChange = 33;
        public const byte fctReset = 34;
        public const byte fctXMLHead = 0xEE;//xml协议
        public const byte fctReadSingle = 1;
        public const byte fctReadMultiple = 2;
        public const byte fctWriteSingle = 5;
        public const byte fctWriteMultiple = 15;
    }

    public enum HDAAggregate
    {
        HDANoAggregate,
        HDAInterpolative,
        HDATotal,
        HDAAverage,
        HDATimeAverage,
        HDACount,
        HDAStDev,
        HDAMinimumActualTime,
        HDAMinimum,
        HDAMaximumActualTime,
        HDAMaximum,
        HDAStart,
        HDAEnd,
        HDADelta,
        HDARegSlope,
        HDARegConst,
        HDARegDev,
        HDAVariance,
        HDARange,
        HDADurationGood,
        HDADurationBad,
        HDAPercentGood,
        HDAPercentBad,
        HDAWorstQuality,
        HDAAnnotations,
    }

    public enum BrowseType
    {
        Branch = 1,
        Leaf = 2,
        Flat = 3
    }
}