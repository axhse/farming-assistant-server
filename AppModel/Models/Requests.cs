using System;

namespace App.Models
{
    public class Response
    {
        public Response() { }
        public Response(object errors = null, string parameter = null, string token = null)
        {
            if (errors as string[] != null)
            {
                Errors = errors as string[];
            }
            else if (errors as string != null)
            {
                Errors = new string[] { errors as string };
            }
            else
            {
                Errors = Array.Empty<string>();
            }
            Parameter = parameter ?? string.Empty;
            NewAuthToken = token ?? string.Empty;
        }

        public string[] Errors { get; set; }
        public string Parameter { get; set; }
        public string NewAuthToken { get; set; }
    }


    public abstract class Request
    {
        public string Type { get; set; }
    }

    public class SignUpRequest : Request
    {
        public SignUpRequest(string username, string password)
        {
            Type = nameof(SignUpRequest);
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
        }
        public string Username { get; init; }
        public string Password { get; init; }
    }

    public class SignInRequest : Request
    {
        public SignInRequest(string username, string password)
        {
            Type = nameof(SignInRequest);
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
        }
        public string Username { get; init; }
        public string Password { get; init; }
    }

    public class UpdateCustomerInfoRequest : Request
    {
        public UpdateCustomerInfoRequest(CustomerInfo customerInfo)
        {
            Type = nameof(UpdateCustomerInfoRequest);
            CustomerInfo = customerInfo;
        }
        public CustomerInfo CustomerInfo { get; init; }
    }

    public class GetCustomerInfoRequest : Request
    {
        public GetCustomerInfoRequest()
        {
            Type = nameof(GetCustomerInfoRequest);
        }
    }

    public class GetRecommendationsRequest : Request
    {
        public GetRecommendationsRequest(Field targetField)
        {
            Type = nameof(GetRecommendationsRequest);
            TargetField = targetField;
        }
        public Field TargetField { get; init; }
    }
}
