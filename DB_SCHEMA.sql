-- MySQL schema para la aplicación de atención médica rural
-- Comentarios agregados para explicar la función de cada tabla y columna.

-- Tabla de pacientes: registro básico de usuarios atendidos en el sistema.
CREATE TABLE IF NOT EXISTS Patient (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,         -- Nombre propio del paciente
    LastName VARCHAR(100) NOT NULL,          -- Apellido del paciente
    DocumentNumber VARCHAR(50),              -- Identificación oficial o cédula
    BirthDate DATETIME,                      -- Fecha de nacimiento
    Gender VARCHAR(20),                      -- Género del paciente
    Address VARCHAR(255),                    -- Dirección domiciliaria
    Phone VARCHAR(50),                       -- Teléfono de contacto
    CreatedAt DATETIME NOT NULL,             -- Fecha de creación del registro
    UpdatedAt DATETIME,                      -- Fecha de última actualización
    IsSynced TINYINT(1) NOT NULL DEFAULT 0   -- Indicador para sincronización offline
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla de consultas médicas: historial de atención asociado a cada paciente.
CREATE TABLE IF NOT EXISTS MedicalConsultation (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PatientId INT NOT NULL,                  -- FK hacia Patient
    ConsultationDate DATETIME NOT NULL,      -- Fecha de la consulta
    Symptoms TEXT,                           -- Síntomas reportados
    Diagnosis TEXT,                          -- Diagnóstico médico
    Treatment TEXT,                          -- Tratamiento indicado
    Notes TEXT,                              -- Notas adicionales de la consulta
    CreatedAt DATETIME NOT NULL,             -- Fecha de creación del registro
    UpdatedAt DATETIME,                      -- Fecha de última actualización
    IsSynced TINYINT(1) NOT NULL DEFAULT 0,  -- Indicador para sincronización offline
    CONSTRAINT fk_MedicalConsultation_Patient FOREIGN KEY (PatientId) REFERENCES Patient(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla de categorías: clasificación de reportes por tipo o problemática.
CREATE TABLE IF NOT EXISTS Category (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,       -- Nombre de la categoría
    Description TEXT                         -- Descripción de la categoría
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla de estados de reporte: seguimiento del estado actual de cada reporte.
CREATE TABLE IF NOT EXISTS ReportStatus (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,       -- Nombre del estado (pendiente, en proceso, cerrado)
    Description TEXT                         -- Descripción del estado
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla de reportes: registros principales de atención, incidentes o casos.
CREATE TABLE IF NOT EXISTS Report (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PatientId INT,                           -- FK opcional hacia Patient
    CategoryId INT NOT NULL,                 -- FK hacia Category
    StatusId INT NOT NULL,                   -- FK hacia ReportStatus
    Title VARCHAR(200) NOT NULL,             -- Título o resumen del reporte
    Description TEXT,                        -- Detalle del reporte
    ReportedAt DATETIME NOT NULL,            -- Fecha y hora del reporte
    Latitude DECIMAL(10,8),                  -- Latitud para geolocalización
    Longitude DECIMAL(11,8),                 -- Longitud para geolocalización
    LocationDescription VARCHAR(255),        -- Descripción textual de la ubicación
    PriorityLevel INT NOT NULL DEFAULT 0,    -- Nivel de prioridad del caso
    IsUrgent TINYINT(1) NOT NULL DEFAULT 0,  -- Indicador de urgencia
    CreatedAt DATETIME NOT NULL,             -- Fecha de creación del registro
    UpdatedAt DATETIME,                      -- Fecha de última actualización
    IsSynced TINYINT(1) NOT NULL DEFAULT 0,  -- Indicador para sincronización offline
    CONSTRAINT fk_Report_Patient FOREIGN KEY (PatientId) REFERENCES Patient(Id) ON DELETE SET NULL,
    CONSTRAINT fk_Report_Category FOREIGN KEY (CategoryId) REFERENCES Category(Id) ON DELETE RESTRICT,
    CONSTRAINT fk_Report_Status FOREIGN KEY (StatusId) REFERENCES ReportStatus(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla de imágenes asociadas a reportes: permite adjuntar evidencia visual.
CREATE TABLE IF NOT EXISTS ReportImage (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportId INT NOT NULL,                   -- FK hacia Report
    ImagePath VARCHAR(255) NOT NULL,         -- Ruta o URI local de la imagen
    ThumbnailPath VARCHAR(255),              -- Ruta o URI de la miniatura
    Caption VARCHAR(255),                    -- Descripción breve de la imagen
    CreatedAt DATETIME NOT NULL,             -- Fecha de creación del registro
    IsSynced TINYINT(1) NOT NULL DEFAULT 0,  -- Indicador para sincronización offline
    CONSTRAINT fk_ReportImage_Report FOREIGN KEY (ReportId) REFERENCES Report(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tabla de comentarios: seguimiento y actualizaciones de cada reporte.
CREATE TABLE IF NOT EXISTS ReportComment (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportId INT NOT NULL,                   -- FK hacia Report
    Author VARCHAR(100),                     -- Autor del comentario
    CommentText TEXT NOT NULL,               -- Texto del comentario o actualización
    CommentDate DATETIME NOT NULL,           -- Fecha de la actualización
    CreatedAt DATETIME NOT NULL,             -- Fecha de creación del registro
    IsSynced TINYINT(1) NOT NULL DEFAULT 0,  -- Indicador para sincronización offline
    CONSTRAINT fk_ReportComment_Report FOREIGN KEY (ReportId) REFERENCES Report(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Índices para optimizar consultas frecuentes en reportes e historial.
CREATE INDEX idx_Report_CategoryId ON Report (CategoryId);
CREATE INDEX idx_Report_StatusId ON Report (StatusId);
CREATE INDEX idx_Report_ReportedAt ON Report (ReportedAt);
CREATE INDEX idx_ReportImage_ReportId ON ReportImage (ReportId);
CREATE INDEX idx_ReportComment_ReportId ON ReportComment (ReportId);
