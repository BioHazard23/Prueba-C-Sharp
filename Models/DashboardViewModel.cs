using PruebaCSharp.Models;

namespace PruebaCSharp.Models
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int ScheduledAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int AttendedAppointments { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
    }
}
