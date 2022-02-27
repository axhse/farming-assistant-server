using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace App.Models
{
    public class RequestSender
    {
        public static readonly string DefaultToken = StaticSettings.ConfigVariables.DefaultToken;

        private readonly CryptoUtils _cryptoUtils;
        private Socket _socket;

        public RequestSender()
        {
            _cryptoUtils = new();
            AuthToken = DefaultToken;
        }
        public RequestSender(string authToken) : this()
        {
            AuthToken = authToken == null || authToken == string.Empty ? DefaultToken : authToken;
        }

        public string AuthToken { get; protected set; }

        public string[] SignUp(string username, string password)
            => SendRequest(new SignUpRequest(username, password)).Errors;

        public string[] SignIn(string username, string password)
            => SendRequest(new SignInRequest(username, password)).Errors;

        public string[] UpdateCustomerInfo(CustomerInfo customerInfo)
            => SendRequest(new UpdateCustomerInfoRequest(customerInfo)).Errors;

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
                Send(EncodeAndEncryptAes(JsonSerializer.Serialize((dynamic)request)));
                Response response = JsonSerializer.Deserialize<Response>(DecryptAesAndDecode(Receive(100000)));
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
                
                Send(_cryptoUtils.RsaPublicKey);
                _cryptoUtils.AesKey = _cryptoUtils.DecryptRsa(Receive(256));
                _cryptoUtils.AesIV = _cryptoUtils.DecryptRsa(Receive(256));
                Send(EncodeAndEncryptAes(AuthToken));
                return true;
            }
            catch { return false; }
        }

        private void Send(byte[] data)
        {
            _socket.Send(data);
        }

        private byte[] Receive(int limitBufferSize)
        {
            byte[] data = new byte[limitBufferSize];
            int realBufferSize = _socket.Receive(data);
            Array.Resize(ref data, realBufferSize);
            return data;
        }

        private byte[] EncryptRsa(byte[] data) => _cryptoUtils.EncryptRsa(data);
        private byte[] EncryptAes(byte[] data) => _cryptoUtils.EncryptAes(data);
        private byte[] DecryptAes(byte[] data) => _cryptoUtils.DecryptAes(data);

        private byte[] EncodeAndEncryptAes(string data) => EncryptAes(Encoding.UTF8.GetBytes(data ?? string.Empty));
        private string DecryptAesAndDecode(byte[] data)
            => Encoding.UTF8.GetString(DecryptAes(data ?? Array.Empty<byte>()));
    }

    public class CryptoUtils
    {
        public static readonly RSAEncryptionPadding DefaultRsaPadding = RSAEncryptionPadding.OaepSHA256;
        private readonly RSA _rsa;
        private readonly Aes _aes;

        public CryptoUtils()
        {
            _rsa = RSA.Create(2048);
            _aes = Aes.Create();
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.Zeros;
        }

        public byte[] RsaPublicKey => _rsa.ExportRSAPublicKey();
        public byte[] AesKey
        {
            get => _aes.Key;
            set
            {
                _aes.Key = value;
            }
        }
        public byte[] AesIV
        {
            get => _aes.IV;
            set
            {
                _aes.IV = value;
            }
        }

        public byte[] EncryptRsa(byte[] data) => _rsa.Encrypt(data, DefaultRsaPadding);
        public byte[] DecryptRsa(byte[] data) => _rsa.Decrypt(data, DefaultRsaPadding);

        public byte[] EncryptAes(byte[] data)
        {
            var encryptor = _aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(Encoding.UTF8.GetString(data));
            }
            return msEncrypt.ToArray();
        }

        public byte[] DecryptAes(byte[] data)
        {
            var decryptor = _aes.CreateDecryptor();
            using var msEncrypt = new MemoryStream(data);
            using var csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read);
            using var srEncrypt = new StreamReader(csEncrypt);
            var text = srEncrypt.ReadToEnd();
            while (text.Length > 0 && text[^1] == '\u0000')
            {
                text = text[..^1];
            }
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
