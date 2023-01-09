using AppMonsta.Models;

namespace AppMonsta.Data
{
    public interface IAuthRepo
    {
        Task<User> Login(string username, string password);
        Task<User> Register(User user, string password);
        Task<bool> UserExists(string username);


    }
}
