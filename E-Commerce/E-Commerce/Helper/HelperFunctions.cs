using System;
using System.Text.RegularExpressions;
using Serilog;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace E_Commerce.Helper
{
    public class HelperFunctions
    {
        public static Guid GetGuid(string guid)
        {
            if (guid.Length == 0) return Guid.Empty;
            return Guid.TryParse(guid, out Guid resId) ? resId : Guid.Empty;
        }
        public static int GenerateOtp()
        {
            var random = new Random();
            int otp = random.Next(1000, 10000);

            return otp;
        }

        // Set up Twilio to send Messages on Phone Number
        public static void SendOtp(int otp, IConfiguration _config)
        {
            var twilioSettings = _config.GetSection("TwilioMessageSettings");

            var accountSid = twilioSettings["Sid"];
            var authToken = twilioSettings["AuthToken"];

            TwilioClient.Init(accountSid, authToken);
            var messageOptions = new CreateMessageOptions(
              new PhoneNumber("+916306104800"));
            messageOptions.From = new PhoneNumber(twilioSettings["TwilioPhoneNumber"]);
            messageOptions.Body = $"Hey, Your OTP is {otp}";
            var message = MessageResource.Create(messageOptions);
            
        }

        public static int GetStaticOtp(IConfiguration _config)
        {
            var authSettings = _config.GetSection("AuthSettings");
            var staticOtp = authSettings["StaticOtp"];

            Console.WriteLine($"Static OTP : {staticOtp}");

            return int.TryParse(staticOtp, out int otp) ? otp : 1234;
        }

        public static bool IsValidEmail(string contact)
        {
            return Regex.IsMatch(contact, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool IsValidPhone(string contact)
        {
            return Regex.IsMatch(contact, @"^[6-9]\d{9}$");
        }
    }
}
