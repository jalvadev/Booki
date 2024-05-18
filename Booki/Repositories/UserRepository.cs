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
                user = _bookiContext.Users.Where(u => u.Username == username && u.Password == password && u.IsVerified).FirstOrDefault();
            }catch (Exception ex)
            {
                user = null;
            }

            return user;
        }

        public Tuple<string, string> GetUserSaltAndPass(string username)
        {
            Tuple<string, string> userSaltAndPass = null;

            try
            {
                var pass = _bookiContext.Users.Where(u => u.Username == username).Select(u => u.Password ).FirstOrDefault();
                var salt = _bookiContext.Users.Where(u => u.Username == username).Select(u => u.Salt ).FirstOrDefault();

                userSaltAndPass = new Tuple<string, string>(pass, salt);

            }catch (Exception ex)
            {
                userSaltAndPass = null;
            }

            return userSaltAndPass;
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

        public User UserById(int id)
        {
            User user = null;

            try
            {
                user = _bookiContext.Users.Where(u => u.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                user = null;
            }

            return user;
        }

        public bool CheckIfUsernameIsAvailable(string username)
        {
            bool isTaken = false;

            try
            {
                isTaken = _bookiContext.Users.Where(u => u.Username == username).Any();
            }catch(Exception ex)
            {
                isTaken = true;
            }

            return isTaken;
        }

        public bool CheckIfEmailIsAvailable(string email)
        {
            bool isTaken = false;

            try
            {
                isTaken = _bookiContext.Users.Where(u => u.Email == email).Any();
            }
            catch (Exception ex)
            {
                isTaken = true;
            }

            return isTaken;
        }

        public bool SetUserVerification(Guid token)
        {
            bool verified = false;

            try
            {
                User user = _bookiContext.Users.Where(u => u.VerificationToken == token).FirstOrDefault();
                user.IsVerified = true;

                _bookiContext.Update(user);
                Save();
                
                verified = true;
            }catch(Exception e)
            {
                verified = false;
            }

            return verified;
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
