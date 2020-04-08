using ThirdStoreCommon.Models.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using ThirdStoreCommon;

namespace ThirdStoreBusiness.AccessControl
{
    public interface IUserService
    {
        IPagedList<T_User> SearchUsers(
            string name = null,
            string email = null,
            string description = null,
            int status = -1,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        void InsertUser(T_User user);

        void UpdateUser(T_User user);

        bool RegisterUser(T_User user);

        IList<T_User> GetAllUsers();

        bool ValidateUser(string email, string password);

        bool SignIn(string email, string password);

        bool SignOut();

        T_User GetUserByEmail(string email);

        T_User GetAuthenticatedUser();

        T_User GetAuthenticatedUserFromTicket(FormsAuthenticationTicket ticket);
    }
}
