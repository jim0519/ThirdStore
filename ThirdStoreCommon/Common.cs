using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ThirdStoreCommon.Models;

namespace ThirdStoreCommon
{
    public class Constants
    {
        public const char SplitChar = ',';

        public const string ThirdStoreDBKey = "ThirdStoreDB";
        public const string SystemUser = "System";
        public const string MinDate = "1970-01-01";
        public const string MaxDate = "2050-12-31";
        public const string UsedSKUSuffix = "_D";
        public const string UsedNameSuffix = "RTS ";
        public const string FirstInvJobItems = "FirstInvJobItems";
        public const string JobItemInvList = "JobItemInvList";
        public const string JobItemInvIDs = "JobItemInvIDs";
        public const string FirstInvJobItemsRef = "FirstInvJobItemsRef";
    }


    public enum ResultType
    {
        Sucess=0,
        Failed=1
    }

    public enum ComponentLifeStyle
    {
        Singleton = 0,
        Transient = 1,
        LifetimeScope = 2
    }

    public enum YesNo
    { 
        Y=1,
        N=0
    }

    public enum ThirdStoreJobItemStatus
    {
        PENDING=1,
        READY=2,
        ALLOCATED=3,
        SHIPPED=4
    }

    public enum ThirdStoreJobItemType
    {
        SELFSTORED=1,
        DISTRIBUTED=2,
        PARTS=3
    }

    public enum ThirdStoreJobItemCondition
    {
        NEW = 1,
        USED = 2,
    }

    public enum eBayIDType
    { 
        ItemID,
        TransactionID
    }

    public enum ThirdStoreItemType
    { 
        PART=1,
        SINGLE=2,
        COMBO=3
    }

    public enum ThirdStoreSupplier
    {
        P = 1,
        A = 2,
        S = 3,
        T = 4,
        O = 5
    }

    public enum NotifyType
    {
        Success,
        Error
    }


    public sealed class ThirdStoreCacheKey
    {
        public const string ThirdStoreJobItemConditionListCache = "ThirdStoreJobItemConditionListCache";
    }

    public class ThirdStoreReturnMessage
    {
        public bool IsSuccess { get; set; }
        public string ErrorMesage { get; set; }
    }
}
