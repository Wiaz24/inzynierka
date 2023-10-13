namespace PowerPredictor.Services.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string email, string subject, string message);

        void SendResetPasswordEmail(string email, string callbackUrl);

        void SendVerifyAccountEmail(string email, string callbackUrl);
    }
}
