using Santtoary.Shared.Responses;

namespace Santtoary.API.Helpers
{
    public interface IMailHelper
    {
        Response SendMail(string toName, string toEmail, string subject, string body);
    }
}