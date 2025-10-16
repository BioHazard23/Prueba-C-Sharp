using Microsoft.AspNetCore.Mvc;
using PruebaCSharp.Models;
using PruebaCSharp.Services;

namespace PruebaCSharp.Controllers
{
    public class EmailLogsController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailLogsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        // GET: EmailLogs
        public async Task<IActionResult> Index()
        {
            try
            {
                var emailLogs = await _emailService.GetEmailLogsAsync();
                return View(emailLogs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el historial de correos: {ex.Message}";
                return View(new List<EmailLog>());
            }
        }

        // GET: EmailLogs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var emailLog = await _emailService.GetEmailLogByIdAsync(id);
                if (emailLog == null)
                {
                    TempData["ErrorMessage"] = "Registro de correo no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(emailLog);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar los detalles del correo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
