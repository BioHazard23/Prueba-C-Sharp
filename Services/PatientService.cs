using Microsoft.EntityFrameworkCore;
using PruebaCSharp.Data;
using PruebaCSharp.Models;

namespace PruebaCSharp.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient?> GetPatientByDocumentAsync(string document);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<Patient?> UpdatePatientAsync(int id, Patient patient);
        Task<bool> DeletePatientAsync(int id);
        Task<bool> IsDocumentUniqueAsync(string document, int? excludeId = null);
    }

    public class PatientService : IPatientService
    {
        private readonly HospitalDbContext _context;

        public PatientService(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _context.Patients
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Patient?> GetPatientByDocumentAsync(string document)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => p.Document == document);
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            if (!patient.ValidateDocument())
                throw new ArgumentException("Invalid document format");

            if (!patient.ValidateEmail())
                throw new ArgumentException("Invalid email format");

            if (!patient.ValidateAge())
                throw new ArgumentException("Invalid age");

            if (!await IsDocumentUniqueAsync(patient.Document))
                throw new InvalidOperationException("Un paciente con este documento ya existe");

            patient.CreatedAt = DateTime.UtcNow;
            patient.UpdatedAt = DateTime.UtcNow;

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient?> UpdatePatientAsync(int id, Patient patient)
        {
            var existingPatient = await _context.Patients.FindAsync(id);
            if (existingPatient == null)
                return null;

            if (!patient.ValidateDocument())
                throw new ArgumentException("Invalid document format");

            if (!patient.ValidateEmail())
                throw new ArgumentException("Invalid email format");

            if (!patient.ValidateAge())
                throw new ArgumentException("Invalid age");

            if (!await IsDocumentUniqueAsync(patient.Document, id))
                throw new InvalidOperationException("Otro paciente con este documento ya existe");

            existingPatient.Name = patient.Name;
            existingPatient.Document = patient.Document;
            existingPatient.Age = patient.Age;
            existingPatient.Phone = patient.Phone;
            existingPatient.Email = patient.Email;
            existingPatient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingPatient;
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return false;

            // Check if patient has appointments
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.PatientId == id && a.Status != AppointmentStatus.Cancelled);

            if (hasAppointments)
                throw new InvalidOperationException("Cannot delete patient with active appointments");

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDocumentUniqueAsync(string document, int? excludeId = null)
        {
            var query = _context.Patients.Where(p => p.Document == document);
            
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return !await query.AnyAsync();
        }
    }

    public interface IDoctorService
    {
        Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
        Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(string specialty);
        Task<Doctor?> GetDoctorByIdAsync(int id);
        Task<Doctor?> GetDoctorByDocumentAsync(string document);
        Task<Doctor> CreateDoctorAsync(Doctor doctor);
        Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor);
        Task<bool> DeleteDoctorAsync(int id);
        Task<bool> IsDocumentUniqueAsync(string document, int? excludeId = null);
        Task<IEnumerable<string>> GetSpecialtiesAsync();
    }

    public class DoctorService : IDoctorService
    {
        private readonly HospitalDbContext _context;

        public DoctorService(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            return await _context.Doctors
                .Where(d => d.Specialty.ToLower().Contains(specialty.ToLower()))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            return await _context.Doctors
                .Include(d => d.Appointments)
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Doctor?> GetDoctorByDocumentAsync(string document)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.Document == document);
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            if (!doctor.ValidateDocument())
                throw new ArgumentException("Invalid document format");

            if (!doctor.ValidateEmail())
                throw new ArgumentException("Invalid email format");

            if (!doctor.ValidateSpecialty())
                throw new ArgumentException("Invalid specialty");

            if (!await IsDocumentUniqueAsync(doctor.Document))
                throw new InvalidOperationException("Un médico con este documento ya existe");

            doctor.CreatedAt = DateTime.UtcNow;
            doctor.UpdatedAt = DateTime.UtcNow;

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor == null)
                return null;

            if (!doctor.ValidateDocument())
                throw new ArgumentException("Invalid document format");

            if (!doctor.ValidateEmail())
                throw new ArgumentException("Invalid email format");

            if (!doctor.ValidateSpecialty())
                throw new ArgumentException("Invalid specialty");

            if (!await IsDocumentUniqueAsync(doctor.Document, id))
                throw new InvalidOperationException("Otro médico con este documento ya existe");

            existingDoctor.Name = doctor.Name;
            existingDoctor.Document = doctor.Document;
            existingDoctor.Specialty = doctor.Specialty;
            existingDoctor.Phone = doctor.Phone;
            existingDoctor.Email = doctor.Email;
            existingDoctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingDoctor;
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return false;

            // Check if doctor has appointments
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.DoctorId == id && a.Status != AppointmentStatus.Cancelled);

            if (hasAppointments)
                throw new InvalidOperationException("Cannot delete doctor with active appointments");

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDocumentUniqueAsync(string document, int? excludeId = null)
        {
            var query = _context.Doctors.Where(d => d.Document == document);
            
            if (excludeId.HasValue)
                query = query.Where(d => d.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<string>> GetSpecialtiesAsync()
        {
            return await _context.Doctors
                .Select(d => d.Specialty)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }
    }
}
