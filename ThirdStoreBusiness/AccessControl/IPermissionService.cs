using ThirdStoreCommon.Models.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdStoreBusiness.AccessControl
{
    public interface IPermissionService
    {
        void InsertPermission(T_Permission permission);

        void UpdatePermission(T_Permission permission);

        bool Authorize(string permissionRecordSystemName);

        T_Permission GetPermissionByID(int id);

        IList<T_Permission> GetAllPermissions();
    }
}
