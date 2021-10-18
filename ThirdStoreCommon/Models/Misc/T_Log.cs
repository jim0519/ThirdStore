using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.Misc
{
    public partial class T_Log : BaseEntity
    {
        public DateTime Date { get; set; }
        public string Thread { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
    }
}
