using ThirdStoreCommon.Models.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon;
using ThirdStoreData;
using ThirdStoreCommon.Infrastructure;

namespace ThirdStoreBusiness.AccessControl
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<T_Permission> _permissionRepository;
        private readonly IWorkContext _workContext;

        public PermissionService(IRepository<T_Permission> permissionRepository,
            IWorkContext workContext)
        {
            _permissionRepository = permissionRepository;
            _workContext = workContext;
        }


        public void InsertPermission(T_Permission permission)
        {
            throw new NotImplementedException();
        }

        public void UpdatePermission(T_Permission permission)
        {
            throw new NotImplementedException();
        }


        public virtual bool Authorize(T_Permission permission, T_User user)
        {
            if (permission == null)
                return false;

            if (user == null)
                return false;

            //old implementation of Authorize method
            //var customerRoles = customer.CustomerRoles.Where(cr => cr.Active);
            //foreach (var role in customerRoles)
            //    foreach (var permission1 in role.PermissionRecords)
            //        if (permission1.SystemName.Equals(permission.SystemName, StringComparison.InvariantCultureIgnoreCase))
            //            return true;

            //return false;

            return Authorize(permission.Name, user);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(string permissionmName)
        {
            return Authorize(permissionmName, _workContext.CurrentUser);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(string permissionName, T_User user)
        {
            if (String.IsNullOrEmpty(permissionName))
                return false;

            var userRoles = user.UserRoles.Where(ur => ur.IsActive);
            foreach (var role in userRoles)
                if (Authorize(permissionName, role))
                    //yes, we have such permission
                    return true;

            //no permission found
            return false;
        }

        protected virtual bool Authorize(string permissionName, T_Role userRole)
        {
            if (String.IsNullOrEmpty(permissionName))
                return false;

            foreach (var permission1 in userRole.RolePermissions)
                if (permission1.Name.Equals(permissionName, StringComparison.InvariantCultureIgnoreCase))
                    return true;

            return false;
           
        }
    }
}
