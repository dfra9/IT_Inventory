using System;
using System.Linq;
using IT_Inventory.Models;

namespace IT_Inventory.Services
{
    public class UserService : IUserService
    {
        private readonly DBIT_Inventory _dbContext;
        private readonly Users db;

        public UserService(DBIT_Inventory dbContext)
        {
            _dbContext = dbContext;
        }

        public UserService(Users db)
        {
            this.db = db;
        }

        public void InitializeAdmin()
        {
            var existingAdmin = _dbContext.Users.FirstOrDefault(u => u.Username == "admin" && u.Is_Admin == true && u.Is_Deleted != true);
            if (existingAdmin == null)
            {
                var admin = new Users
                {
                    Username = "admin",
                    Password = HashPassword("123456"),
                    Is_Admin = true,
                    Create_Date = DateTime.Now,
                    Create_By = "System",
                    Is_Deleted = false

                };


                _dbContext.Users.Add(admin);
                _dbContext.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Admin user created");
            }
        }
        public Users AuthenticateUser(string username, string password)
        {
            System.Diagnostics.Debug.WriteLine($"Authentication user: {username}");
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username && u.Is_Deleted != true);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("User Not Found In Database");


                return null;
            }

            var hashedPassword = HashPassword(password);


            bool isPasswordValid = VerifyPassword(password, user.Password);
            System.Diagnostics.Debug.WriteLine($"Password Verification Result {isPasswordValid}");
            if (isPasswordValid)
            {
                user.Last_Login = DateTime.Now;

                return user;
            }


            return hashedPassword == user.Password ? user : null;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            string hashedInputPassword = HashPassword(password);
            return hashedInputPassword == passwordHash;
        }
    }
}