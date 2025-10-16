using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PruebaCSharp.Models;
using PruebaCSharp.Services;

namespace PruebaCSharp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;

    public HomeController(
        ILogger<HomeController> logger,
        IPatientService patientService,
        IDoctorService doctorService,
        IAppointmentService appointmentService)
    {
        _logger = logger;
        _patientService = patientService;
        _doctorService = doctorService;
        _appointmentService = appointmentService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var patients = await _patientService.GetAllPatientsAsync();
            var doctors = await _doctorService.GetAllDoctorsAsync();
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            var dashboardData = new DashboardViewModel
            {
                TotalPatients = patients.Count(),
                TotalDoctors = doctors.Count(),
                TotalAppointments = appointments.Count(),
                ScheduledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                AttendedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Attended),
                RecentAppointments = appointments
                    .Where(a => a.AppointmentDate >= DateTime.UtcNow.Date)
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .Take(5)
                    .ToList()
            };

            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            TempData["ErrorMessage"] = "Error loading dashboard data";
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
