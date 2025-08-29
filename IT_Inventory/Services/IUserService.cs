using IT_Inventory.Models;

namespace DBIT_Inventory.Services
{

    public interface IUserService
    {
        Users AuthenticateUser(string username, string password);
        void InitializeAdmin();
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);


    }

}