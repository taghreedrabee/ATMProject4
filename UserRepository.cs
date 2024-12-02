using System.Linq;
using BCrypt.Net;
using System;

namespace ATMAPI.Data
{
    public class UserRepository 
    {
        private readonly AtmDbContext _context;
        private const int WORK_FACTOR = 11;
        private readonly TransactionRepository _transactionRepository;

        public UserRepository(AtmDbContext context, TransactionRepository transactionRepository)
        {
            _context = context;
            _transactionRepository = transactionRepository;
        }

        public void AddUser(User user)
        {
            var userEntity = (User)user;
            userEntity.PasswordHash = HashPassword(user.PasswordHash);
            _context.Users.Add(userEntity);
            _context.SaveChanges();

        }

        public User GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User GetUserById(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == userId);
        }

        public void UpdateUser(User user)
        {
            var userEntity = (User)user;
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == userEntity.UserId);

            if (existingUser != null)
            {
                
                if (!string.IsNullOrEmpty(userEntity.PasswordHash) &&
                    !userEntity.PasswordHash.StartsWith("$2")) 
                {
                    userEntity.PasswordHash = HashPassword(userEntity.PasswordHash);
                }
                else
                {
                    
                    userEntity.PasswordHash = existingUser.PasswordHash;
                }

                _context.Users.Update(userEntity);
                _context.SaveChanges();
            }
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public bool DeleteUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            
                _context.Users.Remove(user);
                _context.SaveChanges();
                return true;
        }

        public bool VerifyPassword(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                return ValidatePassword(password, user.PasswordHash);
            }
            return false;
        }


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WORK_FACTOR);
        }

        private bool ValidatePassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
               
                return false;
            }
        }
    }
}