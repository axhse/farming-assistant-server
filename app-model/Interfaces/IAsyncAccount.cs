using System.Threading.Tasks;

namespace App.Models
{
    public interface IAsyncAccount
    {
        string Username { get; }
        CustomerInfo CustomerInfo { get; }

        Recommendation[] GetRecommendations(Field field);
        Task<string[]> LoadRecommendationsAsync(Field field);
        Task<string[]> SignInAsync(string username, string password);
        Task<string[]> SignUpAsync(string username, string password);
        Task<string[]> UpdateCustomerInfoAsync();

        /* Also:

        public static bool UsernameIsCorrect(string username);
        public static bool PasswordIsCorrect(string password);

        */
    }
}

