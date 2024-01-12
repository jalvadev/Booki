using Booki.Models;
using Booki.Models.Context;
using Booki.Repositories.Interfaces;

namespace Booki.Repositories
{
    public class UserReposiroty : IUserRepository
    {
        private readonly BookiContext _bookiContext;
        private bool _disposed;

        public UserReposiroty(BookiContext bookiContext)
        {
            _bookiContext = bookiContext;
            _disposed = false;
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
