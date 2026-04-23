using EmailTester.WebApp.Models;

namespace EmailTester.WebApp.Services
{
    public interface ISmtpEmailSendTester
    {
        Task<IReadOnlyList<SmtpTestLogLine>> TestSendAsync(SmtpTestViewModel input, CancellationToken cancellationToken = default);
    }
}
