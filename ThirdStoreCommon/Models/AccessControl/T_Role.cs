using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.AccessControl
{
    public partial class T_Role : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }


        public System.DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual ICollection<T_Permission> RolePermissions { get; set; }
    }
}
