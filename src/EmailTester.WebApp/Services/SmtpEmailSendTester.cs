using EmailTester.WebApp.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EmailTester.WebApp.Services
{
    public sealed class SmtpEmailSendTester : ISmtpEmailSendTester
    {
        public async Task<IReadOnlyList<SmtpTestLogLine>> TestSendAsync(SmtpTestViewModel input, CancellationToken cancellationToken = default)
        {
            var lines = new List<SmtpTestLogLine>();
            var secure = MapSecureSocketOption(input.SecureSocketOption);

            lines.Add(new SmtpTestLogLine
            {
                Severity = SmtpTestLogSeverity.Info,
                Message = $"Preparing SMTP test to {input.Host}:{input.Port} using {input.SecureSocketOption}."
            });

            using var client = new SmtpClient();

            try
            {
                lines.Add(new SmtpTestLogLine
                {
                    Severity = SmtpTestLogSeverity.Info,
                    Message = "Connecting to the server…"
                });

                await client.ConnectAsync(input.Host.Trim(), input.Port, secure, cancellationToken).ConfigureAwait(false);

                lines.Add(new SmtpTestLogLine
                {
                    Severity = SmtpTestLogSeverity.Success,
                    Message = $"Connected. Encrypted transport: {(client.IsSecure ? "yes" : "no")}."
                });

                var hasUser = !string.IsNullOrWhiteSpace(input.Username);
                if (hasUser)
                {
                    lines.Add(new SmtpTestLogLine
                    {
                        Severity = SmtpTestLogSeverity.Info,
                        Message = "Authenticating…"
                    });

                    await client.AuthenticateAsync(input.Username!.Trim(), input.Password ?? string.Empty, cancellationToken).ConfigureAwait(false);

                    if (client.IsAuthenticated)
                    {
                        lines.Add(new SmtpTestLogLine
                        {
                            Severity = SmtpTestLogSeverity.Success,
                            Message = "Failed."
                        });
                    }
                    else
                    {
                        lines.Add(new SmtpTestLogLine
                        {
                            Severity = SmtpTestLogSeverity.Success,
                            Message = "Authentication succeeded."
                        });
                    } 
                }
                else
                {
                    lines.Add(new SmtpTestLogLine
                    {
                        Severity = SmtpTestLogSeverity.Warning,
                        Message = "No username was provided; skipping authentication (only appropriate for open relays or IP-based auth)."
                    });
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    string.IsNullOrWhiteSpace(input.SenderName) ? input.SenderEmail.Trim() : input.SenderName.Trim(),
                    input.SenderEmail.Trim()));

                message.To.Add(MailboxAddress.Parse(input.SenderEmail.Trim()));
                message.Subject = "Email Tester — delivery check";
                message.Body = new TextPart("plain")
                {
                    Text = "This is an automated test message from Email Tester. If you received it, SMTP send succeeded."
                };

                lines.Add(new SmtpTestLogLine
                {
                    Severity = SmtpTestLogSeverity.Info,
                    Message = $"Sending a self-addressed test message from {input.SenderEmail.Trim()}…"
                });

                await client.SendAsync(message, cancellationToken).ConfigureAwait(false);

                lines.Add(new SmtpTestLogLine
                {
                    Severity = SmtpTestLogSeverity.Success,
                    Message = "Message accepted by the SMTP server. Check the inbox (and spam folder) for the test email."
                });
            }
            catch (Exception ex)
            {
                lines.Add(new SmtpTestLogLine
                {
                    Severity = SmtpTestLogSeverity.Error,
                    Message = $"SMTP test failed: {ex.Message}"
                });

                if (ex.InnerException is not null)
                {
                    lines.Add(new SmtpTestLogLine
                    {
                        Severity = SmtpTestLogSeverity.Error,
                        Message = $"Inner exception: {ex.InnerException.Message}"
                    });
                }
            }
            finally
            {
                if (client.IsConnected)
                {
                    try
                    {
                        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
                        lines.Add(new SmtpTestLogLine
                        {
                            Severity = SmtpTestLogSeverity.Info,
                            Message = "Disconnected from the server."
                        });
                    }
                    catch (Exception ex)
                    {
                        lines.Add(new SmtpTestLogLine
                        {
                            Severity = SmtpTestLogSeverity.Warning,
                            Message = $"While disconnecting: {ex.Message}"
                        });
                    }
                }
            }

            return lines;
        }

        private static SecureSocketOptions MapSecureSocketOption(string? value)
        {
            return value?.Trim() switch
            {
                "None" => SecureSocketOptions.None,
                "SslOnConnect" => SecureSocketOptions.SslOnConnect,
                "Auto" => SecureSocketOptions.Auto,
                "StartTls" => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.StartTls
            };
        }
    }
}
