using Microsoft.AspNetCore.Mvc;
using PruebaCSharp.Models;
using PruebaCSharp.Services;

namespace PruebaCSharp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return View(appointments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading appointments: {ex.Message}";
                return View(new List<Appointment>());
            }
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Cita no encontrada";
                    return RedirectToAction(nameof(Index));
                }
                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading appointment details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Patients = await _patientService.GetAllPatientsAsync();
                ViewBag.Doctors = await _doctorService.GetAllDoctorsAsync();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading data: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            try
            {
                Console.WriteLine($"POST Create called - PatientId: {appointment.PatientId}, DoctorId: {appointment.DoctorId}, Date: {appointment.AppointmentDate}, Time: {appointment.AppointmentTime}");
                Console.WriteLine($"Model binding - PatientId: {appointment.PatientId}, DoctorId: {appointment.DoctorId}");
                
                // Remove navigation properties from ModelState validation
                ModelState.Remove("Patient");
                ModelState.Remove("Doctor");
                ModelState.Remove("EmailLogs");
                
                // Fix DateTime Kind for PostgreSQL
                if (appointment.AppointmentDate != default)
                {
                    appointment.AppointmentDate = DateTime.SpecifyKind(appointment.AppointmentDate, DateTimeKind.Utc);
                }
                
                if (ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is valid, creating appointment...");
                    await _appointmentService.CreateAppointmentAsync(appointment);
                    TempData["SuccessMessage"] = "Cita creada exitosamente y correo de confirmación enviado";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("ModelState is invalid:");
                    foreach (var kvp in ModelState)
                    {
                        Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value.AttemptedValue}");
                        foreach (var error in kvp.Value.Errors)
                        {
                            Console.WriteLine($"  Error: {error.ErrorMessage}");
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating appointment: {ex.Message}";
            }

            try
            {
                ViewBag.Patients = await _patientService.GetAllPatientsAsync();
                ViewBag.Doctors = await _doctorService.GetAllDoctorsAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading data: {ex.Message}";
            }

            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Cita no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Patients = await _patientService.GetAllPatientsAsync();
                ViewBag.Doctors = await _doctorService.GetAllDoctorsAsync();
                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading appointment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,AppointmentDate,AppointmentTime,Notes")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                TempData["ErrorMessage"] = "Invalid appointment ID";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(id, appointment);
                    if (updatedAppointment == null)
                    {
                        TempData["ErrorMessage"] = "Cita no encontrada";
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["SuccessMessage"] = "Cita actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating appointment: {ex.Message}";
            }

            try
            {
                ViewBag.Patients = await _patientService.GetAllPatientsAsync();
                ViewBag.Doctors = await _doctorService.GetAllDoctorsAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading data: {ex.Message}";
            }

            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Cita no encontrada";
                    return RedirectToAction(nameof(Index));
                }
                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading appointment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _appointmentService.DeleteAppointmentAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Appointment deleted successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cita no encontrada";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting appointment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Appointments/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cita cancelada exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cita no encontrada";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling appointment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Appointments/MarkAttended/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttended(int id)
        {
            try
            {
                var result = await _appointmentService.MarkAppointmentAsAttendedAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cita marcada como atendida";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cita no encontrada";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error marking appointment as attended: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/ByPatient/5
        public async Task<IActionResult> ByPatient(int id)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByPatientAsync(id);
                var patient = await _patientService.GetPatientByIdAsync(id);
                ViewBag.Patient = patient;
                return View(appointments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading patient appointments: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Appointments/ByDoctor/5
        public async Task<IActionResult> ByDoctor(int id)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(id);
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                ViewBag.Doctor = doctor;
                return View(appointments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading doctor appointments: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
