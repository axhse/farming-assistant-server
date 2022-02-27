using System;
using System.Collections.Generic;
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
        public Account(string authToken) : this()
        {
            _requestSender = new RequestSender(authToken);
        }

        public CustomerInfo CustomerInfo { get; protected set; }

        public static Account ConvertFromAccountInfo(AccountInfo accountInfo)
        {
            return new Account(accountInfo.AuthToken) { CustomerInfo = accountInfo.CustomerInfo };
        }

        public AccountInfo ConvertToAccountInfo()
        {
            return new AccountInfo() { AuthToken = _requestSender.AuthToken, CustomerInfo = CustomerInfo };
        }

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
                CustomerInfo.Username = username;
                return _requestSender.UpdateCustomerInfo(CustomerInfo);
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
                        if (((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() > recommendation.RelevanceLimitTimestamp)
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
