using Microsoft.EntityFrameworkCore;
using PruebaCSharp.Data;
using PruebaCSharp.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PruebaCSharp.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment?> UpdateAppointmentAsync(int id, Appointment appointment);
        Task<bool> CancelAppointmentAsync(int id);
        Task<bool> MarkAppointmentAsAttendedAsync(int id);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<bool> ValidateScheduleConflictsAsync(int doctorId, int patientId, DateTime date, TimeSpan time, int? excludeId = null);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly HospitalDbContext _context;
        private readonly IEmailService _emailService;

        public AppointmentService(HospitalDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.EmailLogs)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            // Validate appointment data
            if (!appointment.ValidateDateTime())
                throw new ArgumentException("Appointment date and time must be in the future");

            // Check for conflicts
            if (!await ValidateScheduleConflictsAsync(appointment.DoctorId, appointment.PatientId, 
                appointment.AppointmentDate, appointment.AppointmentTime))
                throw new InvalidOperationException("Schedule conflict detected. Doctor or patient already has an appointment at this time");

            // Verify patient and doctor exist
            var patient = await _context.Patients.FindAsync(appointment.PatientId);
            if (patient == null)
                throw new ArgumentException("Patient not found");

            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor == null)
                throw new ArgumentException("Doctor not found");

            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Send confirmation email
            try
            {
                await _emailService.SendAppointmentConfirmationAsync(appointment);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the appointment creation
                Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
            }

            return appointment;
        }

        public async Task<Appointment?> UpdateAppointmentAsync(int id, Appointment appointment)
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
                return null;

            // Validate appointment data
            if (!appointment.ValidateDateTime())
                throw new ArgumentException("Appointment date and time must be in the future");

            // Check for conflicts (excluding current appointment)
            if (!await ValidateScheduleConflictsAsync(appointment.DoctorId, appointment.PatientId, 
                appointment.AppointmentDate, appointment.AppointmentTime, id))
                throw new InvalidOperationException("Schedule conflict detected. Doctor or patient already has an appointment at this time");

            // Verify patient and doctor exist
            var patient = await _context.Patients.FindAsync(appointment.PatientId);
            if (patient == null)
                throw new ArgumentException("Patient not found");

            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor == null)
                throw new ArgumentException("Doctor not found");

            existingAppointment.PatientId = appointment.PatientId;
            existingAppointment.DoctorId = appointment.DoctorId;
            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.AppointmentTime = appointment.AppointmentTime;
            existingAppointment.Notes = appointment.Notes;
            existingAppointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingAppointment;
        }

        public async Task<bool> CancelAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            appointment.Cancel();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAppointmentAsAttendedAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            appointment.MarkAsAttended();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateScheduleConflictsAsync(int doctorId, int patientId, DateTime date, TimeSpan time, int? excludeId = null)
        {
            var appointmentDateTime = date.Date.Add(time);

            // Check doctor conflicts
            var doctorConflictsQuery = _context.Appointments
                .Where(a => a.DoctorId == doctorId && 
                           a.AppointmentDate.Date == date.Date && 
                           a.AppointmentTime == time &&
                           a.Status != AppointmentStatus.Cancelled);

            if (excludeId.HasValue)
                doctorConflictsQuery = doctorConflictsQuery.Where(a => a.Id != excludeId.Value);

            if (await doctorConflictsQuery.AnyAsync())
                return false;

            // Check patient conflicts
            var patientConflictsQuery = _context.Appointments
                .Where(a => a.PatientId == patientId && 
                           a.AppointmentDate.Date == date.Date && 
                           a.AppointmentTime == time &&
                           a.Status != AppointmentStatus.Cancelled);

            if (excludeId.HasValue)
                patientConflictsQuery = patientConflictsQuery.Where(a => a.Id != excludeId.Value);

            if (await patientConflictsQuery.AnyAsync())
                return false;

            return true;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }
    }

    public interface IEmailService
    {
        Task SendAppointmentConfirmationAsync(Appointment appointment);
        Task<IEnumerable<EmailLog>> GetEmailLogsAsync();
        Task<EmailLog?> GetEmailLogByIdAsync(int id);
        Task<IEnumerable<EmailLog>> GetEmailLogsByAppointmentAsync(int appointmentId);
    }

    public class EmailService : IEmailService
    {
        private readonly HospitalDbContext _context;
        private readonly IConfiguration _configuration;

        public EmailService(HospitalDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SendAppointmentConfirmationAsync(Appointment appointment)
        {
            Console.WriteLine($"📧 Iniciando envío de email para cita {appointment.Id} a {appointment.Patient.Email}");
            
            var emailLog = new EmailLog
            {
                AppointmentId = appointment.Id,
                RecipientEmail = appointment.Patient.Email,
                Subject = "Confirmación de Cita Médica - Hospital San Vicente",
                Body = GenerateAppointmentEmailBody(appointment),
                SentAt = DateTime.UtcNow,
                Status = EmailStatus.Failed // Inicialmente como fallido
            };

            try
            {
                Console.WriteLine($"📧 Enviando email real con SendGrid a {emailLog.RecipientEmail}...");
                
                // Obtener configuración de SendGrid
                var sendGridApiKey = _configuration["EmailSettings:SendGridApiKey"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];
                
                if (string.IsNullOrEmpty(sendGridApiKey) || sendGridApiKey == "SG.your-sendgrid-api-key-here")
                {
                    throw new InvalidOperationException("SendGrid API Key no configurada. Por favor, configura tu API Key en appsettings.json");
                }
                
                var client = new SendGridClient(sendGridApiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(appointment.Patient.Email, appointment.Patient.Name);
                var subject = emailLog.Subject;
                var plainTextContent = emailLog.Body;
                var htmlContent = GenerateHtmlEmailBody(appointment);
                
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);
                var responseBody = await response.Body.ReadAsStringAsync();
                
                Console.WriteLine($"📧 SendGrid Response Status: {response.StatusCode}");
                Console.WriteLine($"📧 SendGrid Response Body: {responseBody}");
                
                // SendGrid devuelve 202 (Accepted) para emails exitosos
                Console.WriteLine($"🔍 Verificando estado: IsSuccessStatusCode={response.IsSuccessStatusCode}, StatusCode={response.StatusCode}");
                
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    emailLog.Status = EmailStatus.Sent;
                    Console.WriteLine($"✅ Email enviado exitosamente a {emailLog.RecipientEmail}");
                    Console.WriteLine($"🔍 Estado cambiado a: {emailLog.Status}");
                }
                else
                {
                    emailLog.Status = EmailStatus.Failed;
                    emailLog.ErrorMessage = $"SendGrid error: {response.StatusCode} - {responseBody}";
                    Console.WriteLine($"❌ Error de SendGrid: {emailLog.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error enviando email: {ex.Message}");
                emailLog.Status = EmailStatus.Failed;
                emailLog.ErrorMessage = ex.Message;
            }

            Console.WriteLine($"🔍 ANTES de guardar - Estado: {emailLog.Status}");
            _context.EmailLogs.Add(emailLog);
            await _context.SaveChangesAsync();
            Console.WriteLine($"🔍 DESPUÉS de guardar - Estado: {emailLog.Status}");
            Console.WriteLine($"📧 EmailLog guardado con estado: {emailLog.Status}");
        }

        public async Task<IEnumerable<EmailLog>> GetEmailLogsAsync()
        {
            return await _context.EmailLogs
                .Include(e => e.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
                .OrderByDescending(e => e.SentAt)
                .ToListAsync();
        }

        public async Task<EmailLog?> GetEmailLogByIdAsync(int id)
        {
            return await _context.EmailLogs
                .Include(e => e.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EmailLog>> GetEmailLogsByAppointmentAsync(int appointmentId)
        {
            return await _context.EmailLogs
                .Include(e => e.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
                .Where(e => e.AppointmentId == appointmentId)
                .OrderByDescending(e => e.SentAt)
                .ToListAsync();
        }

        private string GenerateAppointmentEmailBody(Appointment appointment)
        {
            var patientName = appointment.Patient.Name ?? "Paciente";
            var doctorName = appointment.Doctor.Name ?? "Doctor";
            var specialty = appointment.Doctor.Specialty ?? "Especialidad";
            var date = appointment.AppointmentDate.ToString("yyyy-MM-dd");
            var time = appointment.AppointmentTime.ToString(@"hh\:mm");
            
            return "Estimado " + patientName + 
                   ", Esta es una confirmacion de su cita medica en el Hospital San Vicente. " +
                   "Doctor: " + doctorName + 
                   ", Especialidad: " + specialty + 
                   ", Fecha: " + date + 
                   ", Hora: " + time + 
                   ". Por favor, llegue 15 minutos antes de su hora programada. " +
                   "Atentamente, Equipo del Hospital San Vicente";
        }

        private string GenerateHtmlEmailBody(Appointment appointment)
        {
            var patientName = appointment.Patient.Name ?? "Paciente";
            var doctorName = appointment.Doctor.Name ?? "Doctor";
            var specialty = appointment.Doctor.Specialty ?? "Especialidad";
            var date = appointment.AppointmentDate.ToString("yyyy-MM-dd");
            var time = appointment.AppointmentTime.ToString(@"hh\:mm");
            
            return "<html><body><h2>Hospital San Vicente</h2>" +
                   "<h3>Confirmacion de Cita Medica</h3>" +
                   "<p>Estimado " + patientName + ",</p>" +
                   "<p>Le confirmamos su cita medica con los siguientes detalles:</p>" +
                   "<p><strong>Doctor:</strong> " + doctorName + "</p>" +
                   "<p><strong>Especialidad:</strong> " + specialty + "</p>" +
                   "<p><strong>Fecha:</strong> " + date + "</p>" +
                   "<p><strong>Hora:</strong> " + time + "</p>" +
                   "<p>Por favor, llegue 15 minutos antes de su hora programada.</p>" +
                   "<p>Atentamente, Equipo del Hospital San Vicente</p>" +
                   "</body></html>";
        }

    }
}
