using Booki.Models;
using Booki.Models.Context;
using Booki.Repositories.Interfaces;

namespace Booki.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly BookiContext _bookiContext;
        private bool _disposed;

        public UserRepository(BookiContext bookiContext)
        {
            _bookiContext = bookiContext;
            _disposed = false;
        }

        public User LoginUser(string username, string password)
        {
            User user = null;
            try
            {
                user = _bookiContext.Users.Where(u => u.Username == username && u.Password == password).FirstOrDefault();
            }catch (Exception ex)
            {
                user = null;
            }

            return user;
        }

        public User RegisterUser(User user)
        {
            try
            {
                var result = _bookiContext.Users.Add(user);
                user = result.Entity;
                Save();
            }catch(Exception ex)
            {
                user = null;
            }

            return user;
        }

        public void Save()
        {
            _bookiContext.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed && disposing)
                _bookiContext.Dispose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
