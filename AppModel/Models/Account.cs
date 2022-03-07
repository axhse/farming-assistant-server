using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Models
{
    public class Account : IAsyncAccount
    {
        private readonly RequestSender _requestSender;
        private readonly Dictionary<Field, Recommendation[]> _recommendations;

        public Account()
        {
            _requestSender = new RequestSender();
            _recommendations = new();
            CustomerInfo = new CustomerInfo();
        }

        public string Username { get; protected set; }
        public CustomerInfo CustomerInfo { get; protected set; } = new CustomerInfo();
        public string AuthToken
        {
            get => _requestSender.AuthToken;
            init => _requestSender.AuthToken = value;
        }

        public static bool UsernameIsCorrect(string username) => username != null && 6 <= username.Length
                && username.Length <= 20 && !new Regex(@"[^a-z0-9]").IsMatch(username);
        public static bool PasswordIsCorrect(string password) => password != null && 8 <= password.Length
                && password.Length <= 40;

        public async Task<string[]> SignUpAsync(string username, string password) =>
            await Task.Run(() => SignUp(username, password));
        public async Task<string[]> SignInAsync(string username, string password) =>
            await Task.Run(() => SignIn(username, password));
        public async Task<string[]> UpdateCustomerInfoAsync() =>
            await Task.Run(() => UpdateCustomerInfo());
        public async Task<string[]> LoadRecommendationsAsync(Field field) =>
            await Task.Run(() => LoadRecommendations(field));

        public Recommendation[] GetRecommendations(Field field) => _recommendations[field];

        private string[] SignUp(string username, string password)
        {
            string[] signUpErrors = _requestSender.SignUp(username, password);
            if (signUpErrors.Length == 0)
            {
                Username = username;
                string[] updateInfoErrors = UpdateCustomerInfo();
                if (updateInfoErrors.Length != 0)
                {
                    CustomerInfo = new();
                }
                return updateInfoErrors;
            }
            return signUpErrors;
        }

        private string[] SignIn(string username, string password)
        {
            string[] signInErrors = _requestSender.SignIn(username, password);
            if (signInErrors.Length == 0)
            {
                string[] getInfoErrors = _requestSender.GetCustomerInfo(out CustomerInfo customerInfo);
                if (getInfoErrors.Length == 0)
                {
                    Username = username;
                    CustomerInfo = customerInfo;
                }
                return getInfoErrors;
            }
            return signInErrors;
        }

        private string[] UpdateCustomerInfo() => _requestSender.UpdateCustomerInfo(CustomerInfo);

        private string[] LoadRecommendations(Field field)
        {
            if (_recommendations.Count > StaticSettings.ConfigVariables.FieldListLimitSize * 1.1) { _recommendations.Clear(); }
            bool updateRequired = true;
            if (_recommendations.ContainsKey(field))
            {
                if (_recommendations[field].Length > 0)
                {
                    updateRequired = false;
                    foreach (Recommendation recommendation in _recommendations[field])
                    {
                        if (!recommendation.IsRelevant)
                        {
                            updateRequired = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                _recommendations.Add(field, Array.Empty<Recommendation>());
            }
            if (updateRequired)
            {
                string[] getRecommendationsErrors =
                    _requestSender.GetRecommendations(field, out Recommendation[] newRecommendations);
                if (getRecommendationsErrors.Length == 0)
                {
                    _recommendations[field] = newRecommendations;
                }
                return getRecommendationsErrors;
            }
            return Array.Empty<string>();
        }
    }
}
