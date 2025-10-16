-- Tabla pacientes
CREATE TABLE "Patients" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Document" VARCHAR(20) NOT NULL UNIQUE,
    "Age" INTEGER NOT NULL CHECK ("Age" >= 1 AND "Age" <= 120),
    "Phone" VARCHAR(20) NOT NULL,
    "Email" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC')
);

--Tabla doctores
CREATE TABLE "Doctors" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Document" VARCHAR(20) NOT NULL UNIQUE,
    "Specialty" VARCHAR(50) NOT NULL,
    "Phone" VARCHAR(20) NOT NULL,
    "Email" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC')
);

--Tabla citas
CREATE TABLE "Appointments" (
    "Id" SERIAL PRIMARY KEY,
    "PatientId" INTEGER NOT NULL,
    "DoctorId" INTEGER NOT NULL,
    "AppointmentDate" DATE NOT NULL,
    "AppointmentTime" TIME NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "Notes" VARCHAR(500),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("DoctorId") REFERENCES "Doctors"("Id") ON DELETE CASCADE,
    UNIQUE ("DoctorId", "AppointmentDate", "AppointmentTime"),
    UNIQUE ("PatientId", "AppointmentDate", "AppointmentTime")
);

--Tabla logs de email
CREATE TABLE "EmailLogs" (
    "Id" SERIAL PRIMARY KEY,
    "AppointmentId" INTEGER NOT NULL,
    "RecipientEmail" VARCHAR(100) NOT NULL,
    "Subject" VARCHAR(200) NOT NULL,
    "Body" TEXT NOT NULL,
    "SentAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    "Status" INTEGER NOT NULL DEFAULT 1,
    "ErrorMessage" VARCHAR(500),
    FOREIGN KEY ("AppointmentId") REFERENCES "Appointments"("Id") ON DELETE CASCADE
);

-- create indexes
CREATE INDEX "IX_Patients_Document" ON "Patients"("Document");
CREATE INDEX "IX_Doctors_Document" ON "Doctors"("Document");
CREATE INDEX "IX_Appointments_PatientId" ON "Appointments"("PatientId");
CREATE INDEX "IX_Appointments_DoctorId" ON "Appointments"("DoctorId");
CREATE INDEX "IX_Appointments_Date" ON "Appointments"("AppointmentDate");

-- Verificar tablas
SELECT 'Base de datos recreada exitosamente' as resultado;
SELECT 'Patients' as tabla, COUNT(*) as registros FROM "Patients"
UNION ALL
SELECT 'Doctors' as tabla, COUNT(*) as registros FROM "Doctors"
UNION ALL
SELECT 'Appointments' as tabla, COUNT(*) as registros FROM "Appointments"
UNION ALL
SELECT 'EmailLogs' as tabla, COUNT(*) as registros FROM "EmailLogs";

-- Insertar info en la tabla de pacientes
INSERT INTO "Patients" ("Name", "Document", "Age", "Phone", "Email")
VALUES
    ('Carlos Pérez', '1234567890', 35, '3001234567', 'carlos.perez@email.com'),
    ('Ana Gómez', '9876543210', 28, '3112345678', 'ana.gomez@email.com'),
    ('Luis Fernández', '5678901234', 45, '3123456789', 'luis.fernandez@email.com');

-- Insertar info en la tabla de doctores
INSERT INTO "Doctors" ("Name", "Document", "Specialty", "Phone", "Email")
VALUES
    ('Dr. Pedro Sánchez', '1122334455', 'Cardiología', '3207654321', 'pedro.sanchez@hospital.com'),
    ('Dra. María López', '2233445566', 'Pediatría', '3212345678', 'maria.lopez@hospital.com'),
    ('Dr. Juan Martínez', '3344556677', 'Dermatología', '3223456789', 'juan.martinez@hospital.com');

-- Insertar info en la tabla de citas
INSERT INTO "Appointments" ("PatientId", "DoctorId", "AppointmentDate", "AppointmentTime", "Status", "Notes")
VALUES
    (1, 1, '2025-10-15', '09:00:00', 0, 'Consulta de rutina para revisión de presión arterial'),
    (2, 2, '2025-10-16', '10:30:00', 0, 'Revisión pediátrica general'),
    (3, 3, '2025-10-17', '11:00:00', 0, 'Examen de piel por irritación');

-- Insertar info en la tabla de logs de emails
INSERT INTO "EmailLogs" ("AppointmentId", "RecipientEmail", "Subject", "Body", "SentAt", "Status")
VALUES
    (1, 'carlos.perez@email.com', 'Confirmación de cita médica', 'Estimado Carlos, su cita con el Dr. Pedro Sánchez está confirmada para el 15 de octubre a las 9:00 AM.', NOW(), 1),
    (2, 'ana.gomez@email.com', 'Confirmación de cita médica', 'Estimada Ana, su cita con la Dra. María López está confirmada para el 16 de octubre a las 10:30 AM.', NOW(), 1),
    (3, 'luis.fernandez@email.com', 'Confirmación de cita médica', 'Estimado Luis, su cita con el Dr. Juan Martínez está confirmada para el 17 de octubre a las 11:00 AM.', NOW(), 1);