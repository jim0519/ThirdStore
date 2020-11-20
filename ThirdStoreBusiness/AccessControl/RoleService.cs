using ThirdStoreCommon.Models.AccessControl;
using ThirdStoreCommon;
using ThirdStoreData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web;
using ThirdStoreCommon.Infrastructure;

namespace ThirdStoreBusiness.AccessControl
{
    public interface IRoleService
    {
        void InsertRole(T_Role role);

        void UpdateRole(T_Role role);

        IList<T_Role> GetAllRoles();

        T_Role GetRoleByID(int id);

        IPagedList<T_Role> SearchRoles(
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        void RemoveRolePermission(M_RolePermission rolePermission);
    }

    public class RoleService : IRoleService
    {
        private readonly IRepository<T_Role> _roleRepository;
        private readonly IRepository<M_RolePermission> _rolePermissionRepository;
        //private readonly IWorkContext _workContext;

        public RoleService(IRepository<T_Role> roleRepository,
            IRepository<M_RolePermission> rolePermissionRepository)
        {
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }



        public void InsertRole(T_Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role null");

            _roleRepository.Insert(role);
        }

        public void UpdateRole(T_Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role null");
            
            _roleRepository.Update(role);
        }


        public IList<T_Role> GetAllRoles()
        {
            //var allRoles=_cacheManager.Get<IList<T_Role>>(ThirdStoreCacheKey.ThirdStoreRoleListCache, () => { return _roleRepository.Table.Where(u => u.StatusID == 1).ToList(); });
            var query = _roleRepository.Table.Where(u => u.IsActive);
            return query.ToList();
        }

        public IPagedList<T_Role> SearchRoles(
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            var query = _roleRepository.Table;


            query = query.OrderBy(i => i.ID);

            return new PagedList<T_Role>(query, pageIndex, pageSize);
        }

        public T_Role GetRoleByID(int id)
        {
            var role = _roleRepository.GetById(id);
            return role;
        }

        public void RemoveRolePermission(M_RolePermission rolePermission)
        {
            if (rolePermission == null)
                return;
            _rolePermissionRepository.Delete(rolePermission);
        }
    }
}
