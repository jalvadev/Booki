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
                user = _bookiContext.Users.Where(u => u.Username.ToUpper() == username.ToUpper() && u.Password == password && u.IsVerified).FirstOrDefault();
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

                var result = _bookiContext.Bookshelves
                    .Join(_bookiContext.Users,
                        b => b.Id,
                        u => u.Bookshelf.Id,
                        (b, u) => b 
                    );

                var bookShelve = result.FirstOrDefault();
                user.Bookshelf = bookShelve;
            }
            catch (Exception ex)
            {
                user = null;
            }

            return user;
        }

        public User EditUser(User user)
        {
            User editedUser;

            try
            {
                var result = _bookiContext.Users.Update(user);
                editedUser = result.Entity;
                Save();

            }catch(Exception ex)
            {
                editedUser = null;
            }

            return editedUser;
        }

        public bool CheckIfUsernameIsAvailable(string username)
        {
            bool isAvailable = false;

            try
            {
                isAvailable = !_bookiContext.Users.Where(u => u.Username.ToUpper() == username.ToUpper()).Any();
            }catch(Exception ex)
            {
                isAvailable = true;
            }

            return isAvailable;
        }

        public bool CheckIfEmailIsAvailable(string email)
        {
            bool isAvailable = false;

            try
            {
                isAvailable = !_bookiContext.Users.Where(u => u.Email == email).Any();
            }
            catch (Exception ex)
            {
                isAvailable = true;
            }

            return isAvailable;
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
