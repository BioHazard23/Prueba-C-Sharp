using Microsoft.AspNetCore.Mvc;
using PruebaCSharp.Models;
using PruebaCSharp.Services;

namespace PruebaCSharp.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET: Doctors
        public async Task<IActionResult> Index(string specialty = "")
        {
            try
            {
                IEnumerable<Doctor> doctors;
                if (!string.IsNullOrEmpty(specialty))
                {
                    doctors = await _doctorService.GetDoctorsBySpecialtyAsync(specialty);
                    ViewBag.Specialty = specialty;
                }
                else
                {
                    doctors = await _doctorService.GetAllDoctorsAsync();
                }
                return View(doctors);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading doctors: {ex.Message}";
                return View(new List<Doctor>());
            }
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Médico no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(doctor);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading doctor details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Document,Specialty,Phone,Email")] Doctor doctor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _doctorService.CreateDoctorAsync(doctor);
                    TempData["SuccessMessage"] = "Médico registrado exitosamente";
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
                TempData["ErrorMessage"] = $"Error creating doctor: {ex.Message}";
            }

            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Médico no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(doctor);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading doctor: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Document,Specialty,Phone,Email")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                TempData["ErrorMessage"] = "Invalid doctor ID";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var updatedDoctor = await _doctorService.UpdateDoctorAsync(id, doctor);
                    if (updatedDoctor == null)
                    {
                        TempData["ErrorMessage"] = "Médico no encontrado";
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["SuccessMessage"] = "Médico actualizado exitosamente";
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
                TempData["ErrorMessage"] = $"Error updating doctor: {ex.Message}";
            }

            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Médico no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(doctor);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading doctor: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _doctorService.DeleteDoctorAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Médico eliminado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Médico no encontrado";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting doctor: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/Specialties
        public async Task<IActionResult> Specialties()
        {
            try
            {
                var specialties = await _doctorService.GetSpecialtiesAsync();
                return View(specialties);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading specialties: {ex.Message}";
                return View(new List<string>());
            }
        }
    }
}
