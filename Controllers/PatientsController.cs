using Microsoft.AspNetCore.Mvc;
using PruebaCSharp.Models;
using PruebaCSharp.Services;

namespace PruebaCSharp.Controllers
{
    public class PatientsController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                return View(patients);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar pacientes: {ex.Message}";
                return View(new List<Patient>());
            }
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Paciente no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading patient details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Document,Age,Phone,Email")] Patient patient)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _patientService.CreatePatientAsync(patient);
                    TempData["SuccessMessage"] = "Paciente registrado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Document", ex.Message);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating patient: {ex.Message}";
            }

            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Paciente no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading patient: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Document,Age,Phone,Email")] Patient patient)
        {
            if (id != patient.Id)
            {
                TempData["ErrorMessage"] = "Invalid patient ID";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var updatedPatient = await _patientService.UpdatePatientAsync(id, patient);
                    if (updatedPatient == null)
                    {
                        TempData["ErrorMessage"] = "Paciente no encontrado";
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["SuccessMessage"] = "Paciente actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Document", ex.Message);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating patient: {ex.Message}";
            }

            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Paciente no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading patient: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _patientService.DeletePatientAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Paciente eliminado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Paciente no encontrado";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting patient: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
