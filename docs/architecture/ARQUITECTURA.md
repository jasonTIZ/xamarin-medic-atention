# Arquitectura - Sistema de Atención Médica Rural

## Diagrama General

```
┌─────────────────────────────────────────────────────────────┐
│                      Dispositivo Móvil                       │
│                    (Xamarin.Forms App)                       │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────┐   │
│  │              PRESENTACIÓN (Views)                    │   │
│  │  ┌─────────────────┬─────────────────────────────┐   │   │
│  │  │ Login Page      │ Patient List Page           │   │   │
│  │  │ Patient Detail  │ Triage Page                 │   │   │
│  │  │ Consultation    │ Register Consultation Page  │   │   │
│  │  └─────────────────┴─────────────────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ▲                                    │
│                          │ Data Binding                       │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │            LÓGICA DE NEGOCIO (ViewModels)            │   │
│  │  ┌──────────────────┬──────────────────────────┐    │   │
│  │  │ LoginViewModel   │ PatientListViewModel     │    │   │
│  │  │ PatientViewModel │ TriageViewModel          │    │   │
│  │  │ ConsultationVM   │ ProfileViewModel         │    │   │
│  │  └──────────────────┴──────────────────────────┘    │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ▲                                    │
│                          │ Dependency Injection              │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              SERVICIOS (Services)                    │   │
│  │  ┌──────────────┬──────────────┬────────────────┐   │   │
│  │  │ AuthService  │ PatientServ. │ ConsultationS. │   │   │
│  │  └──────────────┴──────────────┴────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
│                          ▲                                    │
│                          │ HTTP Requests                     │
│                          ▼                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │        PERSISTENCIA LOCAL (SQLite + EF Core)         │   │
│  │  ┌──────────────┬──────────────┬────────────────┐   │   │
│  │  │ Users DB     │ Patients DB  │ Consultations  │   │   │
│  │  │ Synced Data  │ Cached Data  │ Local Changes  │   │   │
│  │  └──────────────┴──────────────┴────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
         │                                         │
         │ WiFi/Cellular (cuando disponible)     │
         ▼                                         ▼
┌────────────────────────────────────────────────────────────┐
│              API REST (ASP.NET Core 9)                      │
├────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────┐  │
│  │               Controllers                            │  │
│  │  AuthController | PatientsController | Consultations │  │
│  └──────────────────────────────────────────────────────┘  │
│                        ▲                                    │
│                        │                                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            Business Logic / Services                 │  │
│  │  ┌──────────────┬──────────────┬────────────────┐   │  │
│  │  │ AuthService  │ PatientServ. │ ConsultationS. │   │  │
│  │  └──────────────┴──────────────┴────────────────┘   │  │
│  └──────────────────────────────────────────────────────┘  │
│                        ▲                                    │
│                        │ Repositories/EF Core              │
│                        ▼                                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Persistencia (SQLite + EF Core)              │  │
│  │  ┌──────────────┬──────────────┬────────────────┐   │  │
│  │  │ Users        │ Patients     │ Consultations  │   │  │
│  │  │ Synced Data  │ Priority Log │ Metadata       │   │  │
│  │  └──────────────┴──────────────┴────────────────┘   │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────┘
```

## Capas Arquitectónicas

### 1. Capa de Presentación (Views)
**Responsabilidad**: Renderizar UI y capturar interacciones del usuario.

**Componentes**:
- `AppShell.xaml` - Navegación principal con TabBar (Pacientes, Triaje, Perfil)
- `LoginPage` - Autenticación de usuario
- `PatientListPage` - Listado de pacientes con búsqueda
- `PatientDetailPage` - Detalles del paciente + historial
- `TriagePage` - Sistema de priorización por nivel de urgencia
- `RegisterPatientPage` - Formulario para nuevo paciente
- `RegisterConsultationPage` - Formulario para nueva consulta
- `ProfilePage` - Perfil de usuario
- `PatientHistoryPage` - Historial detallado de paciente

**Patrón**: XAML + Code-behind mínimo, lógica en ViewModels.

### 2. Capa de Lógica de Negocio (ViewModels)
**Responsabilidad**: Procesamiento de datos, validación, sincronización.

**ViewModels principales**:
```
- LoginViewModel
  ├─ ValidateCredentials()
  ├─ LoginAsync()
  └─ SaveTokenAsync()

- PatientListViewModel
  ├─ LoadAsync()
  ├─ SearchAsync(query)
  ├─ SyncWithServerAsync()
  └─ HandleOfflineMode()

- TriageViewModel
  ├─ LoadAsync()
  ├─ GroupByPriority()
  ├─ ChangePriorityAsync(patient, level)
  └─ AutoRefreshAsync()

- RegisterPatientViewModel
  ├─ ValidateInput()
  ├─ SaveAsync()
  └─ SyncAsync()

- ConsultationViewModel
  ├─ LoadAsync()
  ├─ RegisterAsync()
  └─ SyncAsync()

- PatientHistoryViewModel
  ├─ LoadAsync()
  ├─ FilterByDateAsync()
  └─ ExportAsync()
```

### 3. Capa de Servicios (Services)
**Responsabilidad**: Comunicación con API, gestión de almacenamiento local, sincronización.

**Servicios implementados**:

#### `IAuthService`
```csharp
- LoginAsync(email, password): Task<LoginResponse>
- RefreshTokenAsync(): Task<string>
- LogoutAsync(): Task
- ValidateTokenAsync(token): Task<bool>
```

#### `IPatientService`
```csharp
- GetAllAsync(): Task<List<Patient>>
- GetByIdAsync(id): Task<Patient>
- CreateAsync(patient): Task<Patient>
- UpdateAsync(patient): Task
- DeleteAsync(id): Task
- SearchAsync(query): Task<List<Patient>>
- GetHistoryAsync(id): Task<List<Consultation>>
```

#### `IConsultationService`
```csharp
- GetAllAsync(): Task<List<Consultation>>
- GetByPatientAsync(patientId): Task<List<Consultation>>
- CreateAsync(consultation): Task<Consultation>
- UpdateAsync(consultation): Task
- DeleteAsync(id): Task
```

#### `ISyncService`
```csharp
- SyncAsync(): Task
- GetSyncStatus(): SyncStatus
- HandleConflictAsync(localData, remoteData): Task
```

### 4. Capa de Persistencia (Data Access)
**Responsabilidad**: Interacción con base de datos local (SQLite).

**Componentes**:
- `MedicalAttentionContext` - DbContext principal
- `Repository<T>` - Patrón genérico de repositorio
- Modelos: `User`, `Patient`, `Consultation`, `TriageRecord`

**Migración inicial**: DB_SCHEMA.sql

---

## Flujo de Datos

### Escenario 1: Login
```
Usuario ingresa credenciales
       ↓
LoginPage → LoginViewModel.LoginAsync()
       ↓
IAuthService.LoginAsync(email, password)
       ↓
HTTP POST /api/auth/login → API
       ↓
API autentica, genera JWT
       ↓
JWT guardado en SecureStorage
       ↓
Navegación a AppShell (TabBar)
```

### Escenario 2: Crear Paciente (Online)
```
Usuario completa formulario
       ↓
RegisterPatientViewModel.SaveAsync()
       ↓
IPatientService.CreateAsync(patient)
       ↓
┌─ HTTP POST /api/patients → API
│       ↓
│  Paciente guardado en servidor
│       ↓
└─ Sincización local en SQLite
       ↓
Confirmación visual al usuario
```

### Escenario 3: Crear Paciente (Offline)
```
Usuario completa formulario
       ↓
RegisterPatientViewModel.SaveAsync()
       ↓
IPatientService.CreateAsync(patient)
       ↓
┌─ HTTP POST falla (sin conexión)
│       ↓
│  Se guarda localmente con flag "pending_sync"
│       ↓
└─ Indicador visual "Pendiente sincronizar"
       ↓
Cuando hay conexión:
  ISyncService.SyncAsync() detecta cambios
       ↓
  Envía datos al servidor
       ↓
  Sincronización completa
```

### Escenario 4: Sistema de Triage (Priorización)
```
TriageViewModel.LoadAsync()
       ↓
Carga pacientes de servidor (o caché si offline)
       ↓
GroupByPriority() agrupa en:
  - Roja (Emergencia)
  - Amarilla (Urgente)
  - Verde (No urgente)
  - Azul (Consulta planificada)
       ↓
Usuario desliza/toca para cambiar prioridad
       ↓
ChangePriorityAsync(patient, newLevel)
       ↓
HTTP PATCH /api/patients/{id}/priority (si online)
  ó guardado localmente con pending_sync
       ↓
UI actualizada inmediatamente (optimistic update)
```

---

## Decisiones Técnicas

### 1. MVVM Pattern
**Por qué**: Separación de concerns, testabilidad, reutilización de lógica.
**Implementación**: ViewModels con INotifyPropertyChanged, Commands binding.

### 2. Offline-First Sync
**Por qué**: Zonas rurales = conectividad intermitente.
**Implementación**:
- Datos en SQLite local como fuente de verdad
- Flag "pending_sync" en registros
- Auto-sync cuando conectividad restaurada
- Manejo de conflictos (server wins, últimowrite wins, etc.)

### 3. JWT Authentication
**Por qué**: Stateless, seguro, no requiere sesión del servidor.
**Implementación**:
- Token en SecureStorage (Xamarin.Essentials)
- Refresh automático antes de expiración
- Validación offline del token

### 4. Entity Framework Core
**Por qué**: Migraciones automáticas, relaciones complejas, LINQ.
**Implementación**:
- DbContext centralizado
- Migrations para evolucionar schema
- Lazy loading para relaciones

### 5. HTTP Client con Retry Logic
**Por qué**: Red inestable en zonas rurales.
**Implementación**: Polly para reintentos exponenciales.

---

## Modelos de Datos

### User
```csharp
- Id: int (PK)
- Email: string (Unique, Required)
- PasswordHash: string
- FullName: string
- Role: string (admin, doctor, nurse)
- CreatedAt: DateTime
- UpdatedAt: DateTime
- IsActive: bool
```

### Patient
```csharp
- Id: int (PK)
- FullName: string (Required)
- IdentificationNumber: string (Unique)
- DateOfBirth: DateTime
- Phone: string
- Address: string
- CreatedAt: DateTime
- UpdatedAt: DateTime
- LastConsultationDate: DateTime?
- SyncedWithServer: bool
```

### Consultation
```csharp
- Id: int (PK)
- PatientId: int (FK)
- DoctorId: int (FK to User)
- Reason: string
- Diagnosis: string
- Treatment: string
- Notes: string
- CreatedAt: DateTime
- UpdatedAt: DateTime
- SyncedWithServer: bool
```

### TriageRecord
```csharp
- Id: int (PK)
- PatientId: int (FK)
- Priority: enum (Red, Yellow, Green, Blue)
- AssignedBy: int (FK to User)
- AssignedAt: DateTime
- UpdatedAt: DateTime
- Reason: string
```

---

## Seguridad

### Autenticación
- ✅ Credenciales validadas en servidor
- ✅ JWT firmado y verificado
- ✅ Token expiración 24 horas
- ✅ Refresh token para renovación

### Almacenamiento
- ✅ Contraseñas hasheadas (bcrypt) en servidor
- ✅ Tokens en SecureStorage del dispositivo
- ✅ Datos sensibles encriptados localmente
- ✅ SQLite con permiso READ/WRITE restringido

### Comunicación
- ✅ HTTPS en producción
- ✅ Validación de certificados
- ✅ CORS en API configurado correctamente

---

## Escalabilidad Futura

### Fase 2
- [ ] Cliente Web (ASP.NET MVC/Blazor)
- [ ] Dashboard administrativo
- [ ] Reportes analíticos

### Fase 3
- [ ] Integración con sistemas de hospital
- [ ] Push notifications para urgencias
- [ ] Sincronización en tiempo real (SignalR)

### Fase 4
- [ ] Machine Learning para predicción de riesgo
- [ ] Telemedicina
- [ ] Integración de sensores médicos
