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
    public class UserService : IUserService
    {
        private readonly IRepository<T_User> _userRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly HttpContextBase _httpContext;
        private T_User _cachedUser;

        public UserService(IRepository<T_User> userRepository,
            IEncryptionService encryptionService,
            HttpContextBase httpContext)
        {
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _httpContext = httpContext;
        }



        public void InsertUser(T_User user)
        {
            if (user != null)
                _userRepository.Insert(user);
        }

        public void UpdateUser(T_User user)
        {
            if (user != null)
                _userRepository.Update(user);
        }

        public bool RegisterUser(T_User user)
        {
            try
            {
                var inputPassword = user.Password;
                string saltKey = _encryptionService.CreateSaltKey(5);
                user.PasswordSalt = saltKey;
                user.Password = _encryptionService.CreatePasswordHash(inputPassword, saltKey);
                user.StatusID = 1;//TODO: get user active status ID in status list
                var now = DateTime.Now;
                var createBy = Constants.SystemUser;
                user.CreateTime = now;
                user.CreateBy = createBy;
                user.EditTime = now;
                user.EditBy = createBy;
                user.FillOutNull();

                InsertUser(user);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return false;
            }
            
        }

        public bool ValidateUser(string email, string password)
        {
            var user = GetUserByEmail(email);
            if (user == null || user.StatusID != 1)//TODO: get user active status ID in status list
                return false;

            var pwd = _encryptionService.CreatePasswordHash(password, user.PasswordSalt);

            if (pwd != user.Password)
                return false;

            return true;
        }


        public bool SignIn(string email, string password)
        {
            if (!ValidateUser(email, password))
                return false;

            var user = GetUserByEmail(email);

            var now = DateTime.UtcNow.ToLocalTime();

            var ticket = new FormsAuthenticationTicket(
                1 /*version*/,
                user.Email,
                now,
                now.Add(FormsAuthentication.Timeout),
                true,
                user.Email,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.HttpOnly = true;
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }
            //cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            //FormsAuthentication.SetAuthCookie()

            _httpContext.Response.Cookies.Add(cookie);
            _cachedUser = user;
            return true;

        }


        public T_User GetUserByEmail(string email)
        {
            return _userRepository.Table.FirstOrDefault(u => u.Email.Equals(email));
        }


        public bool SignOut()
        {
            _cachedUser = null;
            FormsAuthentication.SignOut();
            return true;
        }

        public T_User GetAuthenticatedUser()
        {
            if (_cachedUser != null)
                return _cachedUser;

            if (_httpContext == null ||
                _httpContext.Request == null ||
                !_httpContext.Request.IsAuthenticated ||
                !(_httpContext.User.Identity is FormsIdentity))
            {
                return null;
            }

            var formsIdentity = (FormsIdentity)_httpContext.User.Identity;
            var user = GetAuthenticatedUserFromTicket(formsIdentity.Ticket);
            if (user != null && user.StatusID == 1)//TODO: get user active status ID in status list
                _cachedUser = user;
            return _cachedUser;
        }

        public T_User GetAuthenticatedUserFromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            var email = ticket.UserData;

            if (String.IsNullOrWhiteSpace(email))
                return null;
            var user = GetUserByEmail(email);
            return user;
        }

        public IList<T_User> GetAllUsers()
        {
            //var allUsers=_cacheManager.Get<IList<T_User>>(ThirdStoreCacheKey.ThirdStoreUserListCache, () => { return _userRepository.Table.Where(u => u.StatusID == 1).ToList(); });
            var query = _userRepository.Table.Where(u => u.StatusID == 1);
            return query.ToList();
        }

        public IPagedList<T_User> SearchUsers(
            string name = null, 
            string email = null, 
            string description = null, 
            int status = -1, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            var query = _userRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u=>u.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(description))
                query = query.Where(u => u.Description.Contains(description));

            if (status != -1)
            {
                query = query.Where(l => l.StatusID.Equals(status));
            }

            query = query.OrderBy(i => i.Email);

            return new PagedList<T_User>(query, pageIndex, pageSize);
        }
    }
}
