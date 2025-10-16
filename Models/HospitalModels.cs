using System.ComponentModel.DataAnnotations;

namespace PruebaCSharp.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        Cancelled,
        Attended
    }

    public enum EmailStatus
    {
        Sent,
        Failed
    }

    public class Patient
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El documento es obligatorio")]
        [StringLength(20, ErrorMessage = "El documento no puede exceder 20 caracteres")]
        public string Document { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La edad es obligatoria")]
        [Range(1, 120, ErrorMessage = "La edad debe estar entre 1 y 120 años")]
        public int Age { get; set; }
        
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public Patient() { }

        public Patient(string name, string document, int age, string phone, string email)
        {
            Name = name;
            Document = document;
            Age = age;
            Phone = phone;
            Email = email;
        }

        public bool ValidateDocument()
        {
            return !string.IsNullOrWhiteSpace(Document) && Document.Length >= 5;
        }

        public bool ValidateEmail()
        {
            return !string.IsNullOrWhiteSpace(Email) && Email.Contains("@");
        }

        public bool ValidateAge()
        {
            return Age > 0 && Age <= 120;
        }
    }

    public class Doctor
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El documento es obligatorio")]
        [StringLength(20, ErrorMessage = "El documento no puede exceder 20 caracteres")]
        public string Document { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La especialidad es obligatoria")]
        [StringLength(50, ErrorMessage = "La especialidad no puede exceder 50 caracteres")]
        public string Specialty { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public Doctor() { }

        public Doctor(string name, string document, string specialty, string phone, string email)
        {
            Name = name;
            Document = document;
            Specialty = specialty;
            Phone = phone;
            Email = email;
        }

        public bool ValidateDocument()
        {
            return !string.IsNullOrWhiteSpace(Document) && Document.Length >= 5;
        }

        public bool ValidateEmail()
        {
            return !string.IsNullOrWhiteSpace(Email) && Email.Contains("@");
        }

        public bool ValidateSpecialty()
        {
            return !string.IsNullOrWhiteSpace(Specialty) && Specialty.Length >= 3;
        }
    }

    public class Appointment
    {
        public int Id { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "El paciente es obligatorio")]
        public int PatientId { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "El médico es obligatorio")]
        public int DoctorId { get; set; }
        
        [Required(ErrorMessage = "La fecha de la cita es obligatoria")]
        public DateTime AppointmentDate { get; set; }
        
        [Required(ErrorMessage = "La hora de la cita es obligatoria")]
        public TimeSpan AppointmentTime { get; set; }
        
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();

        public Appointment() { }

        public Appointment(int patientId, int doctorId, DateTime date, TimeSpan time)
        {
            PatientId = patientId;
            DoctorId = doctorId;
            AppointmentDate = date;
            AppointmentTime = time;
        }

        public bool ValidateDateTime()
        {
            var appointmentDateTime = AppointmentDate.Date.Add(AppointmentTime);
            return appointmentDateTime > DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = AppointmentStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsAttended()
        {
            Status = AppointmentStatus.Attended;
            UpdatedAt = DateTime.UtcNow;
        }

        public DateTime GetAppointmentDateTime()
        {
            return AppointmentDate.Date.Add(AppointmentTime);
        }
    }

    public class EmailLog
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Appointment is required")]
        public int AppointmentId { get; set; }
        
        [Required(ErrorMessage = "Recipient email is required")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        public string RecipientEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Body is required")]
        public string Body { get; set; } = string.Empty;
        
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public EmailStatus Status { get; set; } = EmailStatus.Failed;
        
        [StringLength(500, ErrorMessage = "Error message cannot exceed 500 characters")]
        public string ErrorMessage { get; set; } = string.Empty;

        // Navigation properties
        public virtual Appointment Appointment { get; set; } = null!;

        public EmailLog() { }

        public EmailLog(int appointmentId, string recipientEmail, string subject, string body)
        {
            AppointmentId = appointmentId;
            RecipientEmail = recipientEmail;
            Subject = subject;
            Body = body;
        }

        public void MarkAsSent()
        {
            Status = EmailStatus.Sent;
            SentAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = EmailStatus.Failed;
            ErrorMessage = errorMessage;
            SentAt = DateTime.UtcNow;
        }
    }
}
