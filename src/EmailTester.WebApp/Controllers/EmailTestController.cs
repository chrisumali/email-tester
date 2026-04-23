using EmailTester.WebApp.Models;
using EmailTester.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailTester.WebApp.Controllers
{
    public class EmailTestController : Controller
    {
        private readonly ISmtpEmailSendTester _smtpEmailSendTester;

        public EmailTestController(ISmtpEmailSendTester smtpEmailSendTester)
        {
            _smtpEmailSendTester = smtpEmailSendTester;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SmtpTestViewModel
            {
                SecureSocketOption = "StartTls"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SmtpTestViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var validationLines = ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .SelectMany(kv => kv.Value!.Errors.Select(e =>
                        new SmtpTestLogLine
                        {
                            Severity = SmtpTestLogSeverity.Error,
                            Message = string.IsNullOrWhiteSpace(e.ErrorMessage)
                                ? $"{kv.Key}: invalid value."
                                : $"{kv.Key}: {e.ErrorMessage}"
                        }))
                    .ToList();

                model.Log = validationLines;
                return View(model);
            }

            model.Log = await _smtpEmailSendTester.TestSendAsync(model, cancellationToken).ConfigureAwait(false);
            return View(model);
        }
    }
}
