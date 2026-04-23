using System.ComponentModel.DataAnnotations;

namespace EmailTester.WebApp.Models
{
    public class SmtpTestViewModel
    {
        [Required(ErrorMessage = "Host is required.")]
        [Display(Name = "Host")]
        public string Host { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
        [Display(Name = "Port")]
        public int Port { get; set; } = 587;

        [Display(Name = "Username")]
        public string? Username { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Sender email is required.")]
        [EmailAddress(ErrorMessage = "Sender email must be a valid email address.")]
        [Display(Name = "Sender email")]
        public string SenderEmail { get; set; } = string.Empty;

        [Display(Name = "Sender name")]
        public string? SenderName { get; set; }

        [Required(ErrorMessage = "Secure socket option is required.")]
        [Display(Name = "Secure socket options")]
        public string SecureSocketOption { get; set; } = "StartTls";

        public IReadOnlyList<SmtpTestLogLine>? Log { get; set; }

        public static IReadOnlyList<string> SecureSocketOptionChoices { get; } =
            new[] { "None", "SslOnConnect", "Auto", "StartTls" };
    }

    public enum SmtpTestLogSeverity
    {
        Info,
        Success,
        Warning,
        Error
    }

    public sealed class SmtpTestLogLine
    {
        public SmtpTestLogSeverity Severity { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
