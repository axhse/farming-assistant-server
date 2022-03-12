using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace App.Models
{
    public class RequestSender
    {
        public static readonly string DefaultToken = StaticSettings.ConfigVariables.DefaultToken;

        private readonly CryptoUtils _cryptoUtils;
        private Socket _socket;
        private string _authToken;

        public RequestSender()
        {
            _cryptoUtils = new();
            AuthToken = DefaultToken;
        }
        public RequestSender(string authToken) : this() => AuthToken = authToken;

        public string AuthToken
        {
            get => _authToken;
            set => _authToken = value == null || value == string.Empty ? DefaultToken : value;
        }

        public string[] SignUp(string username, string password)
            => SendRequest(new SignUpRequest(username, password)).Errors;

        public string[] SignIn(string username, string password)
            => SendRequest(new SignInRequest(username, password)).Errors;

        public string[] GetCustomerInfo(out CustomerInfo customerInfo)
        {
            Response response = SendRequest(new GetCustomerInfoRequest());
            if (response.Errors.Length == 0)
            {
                customerInfo = JsonSerializer.Deserialize<CustomerInfo>(response.Parameter);
            }
            else { customerInfo = new CustomerInfo(); }
            return response.Errors;
        }

        public string[] UpdateCustomerInfo(CustomerInfo customerInfo)
            => SendRequest(new UpdateCustomerInfoRequest(customerInfo)).Errors;

        public string[] GetRecommendations(Field targetField, out Recommendation[] recommendations)
        {
            Response response = SendRequest(new GetRecommendationsRequest(targetField));
            if (response.Errors.Length == 0)
            {
                recommendations = JsonSerializer.Deserialize<Recommendation[]>(response.Parameter);
            }
            else { recommendations = Array.Empty<Recommendation>(); }
            return response.Errors;
        }

        protected Response SendRequest(Request request)
        {
            if (!TryConnect()) { return new Response("ConnectionError"); }
            try
            {
                Send(EncodeAndEncryptAes(request.ToJson()));
                Response response = JsonSerializer.Deserialize<Response>(DecryptAesAndDecode(Receive(50000)));
                if (response.NewAuthToken != string.Empty) { AuthToken = response.NewAuthToken; }
                return response;
            }
            catch { return new Response("ConnectionError"); }
        }

        private bool TryConnect()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = StaticSettings.ConfigVariables.SendingTimeout,
                    ReceiveTimeout = StaticSettings.ConfigVariables.ReceivingTimeout
                };
                _socket.Connect(StaticSettings.ConfigVariables.ServerUrl, StaticSettings.ConfigVariables.ServerPort);
                Send(EncodeAndEncryptAes(AuthToken));
                return true;
            }
            catch { return false; }
        }

        private void Send(byte[] data) => _socket.Send(data);

        private byte[] Receive(int limitBufferSize)
        {
            byte[] data = new byte[limitBufferSize];
            int realBufferSize = _socket.Receive(data);
            Array.Resize(ref data, realBufferSize);
            return data;
        }

        private byte[] EncryptRsa(byte[] data) => data;
        private byte[] EncryptAes(byte[] data) => data;
        private byte[] DecryptAes(byte[] data) => data;

        private byte[] EncodeAndEncryptAes(string data) => EncryptAes(Encoding.UTF8.GetBytes(data ?? string.Empty));
        private string DecryptAesAndDecode(byte[] data)
            => Encoding.UTF8.GetString(DecryptAes(data ?? Array.Empty<byte>()));
    }
}
