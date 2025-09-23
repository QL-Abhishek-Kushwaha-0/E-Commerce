namespace E_Commerce.Helper
{
    public class EmailMessages
    {
        public const string RegisterSubject = "Welcome to E-Commerce Application";
        public const string ForgotPasswordSubject = "Reset Your Password";
        public static string RegisterSuccessEmail()
        {
            return @"<p>Thank you for registering with us!</p>
                <p>Happy Shopping...</p>";
        }

        public static string ForgotPasswordEmail(string resetLink)
        {
            return $@"<p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>";
        }

    }
}
