namespace ThesisBackend.Helpers.AuthHelpers
{
    public class OTPGenerator
    {

        public string GenerateOTP(int length = 6)
        {
            const string digits = "0123456789";
            var otp = new char[length];
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                otp[i] = digits[random.Next(digits.Length)];
            }
            return new string(otp);
        }

    }
}
