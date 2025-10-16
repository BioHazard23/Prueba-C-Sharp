# 🏥 Hospital San Vicente Management System

Web application developed in **C# ASP.NET Core** for comprehensive management of patients, doctors, and medical appointments at Hospital San Vicente.

## 🚀 Key Features

- **Patient Management**: Registration, editing and consultation of personal information
- **Doctor Management**: Administration of doctors by specialty
- **Appointment Management**: Scheduling, cancellation and tracking of medical appointments
- **Business Validations**: Prevention of schedule conflicts and duplicate documents
- **Email System**: Automatic confirmations via SendGrid
- **Dashboard**: Statistics and system overview

## 🛠️ Technologies Used

- **Backend**: C# ASP.NET Core 8.0
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Frontend**: Razor Pages + Bootstrap 5
- **Email**: SendGrid API
- **Architecture**: MVC + Services

## 📋 Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio Code / Visual Studio / JetBrains Rider

## ⚙️ Installation and Configuration

### 1. Clone the Repository
```bash
git clone <https://github.com/BioHazard23/CSharp-Prueba.git>
cd CSharp-Prueba
```

### 2. Configure Database
1. Create PostgreSQL database: `JuanArangoHospitalDB`
2. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=168.119.183.3;Port=5432;Database=JuanArangoHospitalDB;Username=root;Password=s7cq453mt2jnicTaQXKT"
  }
}
```

### 3. Configure SendGrid (Optional)
1. Create account at [SendGrid](https://sendgrid.com/)
2. Generate API Key
3. Update in `appsettings.json`:
```json
{
  "EmailSettings": {
    "SendGridApiKey": "The API Key is in discord",
    "FromEmail": "jumarana1007@gmail.com",
    "FromName": "Hospital San Vicente"
  }
}
```

### 4. Run the Application
```bash
dotnet restore
dotnet ef database update
dotnet run
```

The application will be available at: `http://localhost:3000`

## 📊 Project Structure

```
PruebaCSharp/
├── Controllers/          # MVC Controllers
├── Models/              # Entities and ViewModels
├── Services/            # Business Logic
├── Data/               # Database Context
├── Views/              # Razor Views
└── wwwroot/            # Static Files
```

## 🎯 Functionalities

### Patient Management
- ✅ Registration with unique document validation
- ✅ Personal information editing
- ✅ Patient listing and search

### Doctor Management
- ✅ Registration by specialty
- ✅ Filtering by medical specialty
- ✅ Unique document validation

### Appointment Management
- ✅ Scheduling with conflict validation
- ✅ Cancellation and marking as attended
- ✅ Listing by patient and doctor
- ✅ Automatic email confirmation sending

### Dashboard
- ✅ General system statistics
- ✅ Upcoming appointments
- ✅ Activity summary

## 🔧 Business Validations

- **Unique Documents**: No patients/doctors with the same document allowed
- **Schedule Conflicts**: A doctor/patient cannot have two simultaneous appointments
- **Future Dates**: Appointments can only be scheduled for future dates
- **Required Fields**: Validation of all required fields

## 📧 Email System

- **Automatic Sending**: Confirmation when scheduling appointment
- **Complete History**: Log of all emails sent
- **Status Tracking**: Success/failure delivery tracking
- **SendGrid Integration**: Real email sending

## 🗄️ Database

### Main Entities
- **Patients**: Personal information of patients
- **Doctors**: Doctor data and specialties
- **Appointments**: Medical appointments with status
- **EmailLogs**: History of sent emails

### Enumerations
- **AppointmentStatus**: Scheduled, Cancelled, Attended
- **EmailStatus**: Sent, Failed

## 🚀 Application Usage

1. **Access Dashboard**: General system view
2. **Manage Patients**: Register and administer patients
3. **Manage Doctors**: Administer doctors by specialty
4. **Schedule Appointments**: Create appointments with automatic validations
5. **Consult History**: Review sent emails

## 🐛 Troubleshooting

### Database Connection Error
- Verify connection string in `appsettings.json`
- Ensure PostgreSQL is running
- Validate access credentials

### Emails Not Sending
- Verify SendGrid configuration
- Check API Key in `appsettings.json`
- Consult email history in the application

### Port in Use
```bash
# Change port in Properties/launchSettings.json
# Or kill existing processes:
pkill -f dotnet
```

## 📝 Development Notes

- **Language**: Code in English, interface in Spanish
- **Patterns**: POO implementation, MVC, Repository Pattern
- **Validations**: Data Annotations + business validations
- **Error Handling**: Try-catch with descriptive messages

## 👨‍💻 Author

**Juan Manuel Arango Arana**  


---
