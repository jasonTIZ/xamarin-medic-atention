# Medical Attention - Sistema de Atención Médica Rural

[![Build Status](https://github.com/yourusername/medical-attention/actions/workflows/build-android-apk.yml/badge.svg)](https://github.com/yourusername/medical-attention/actions)
![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-iOS%20%7C%20Android-lightgrey)
![Status](https://img.shields.io/badge/status-Production%20Ready-brightgreen)

**Medical Attention** es una solución integral de gestión médica diseñada para zonas rurales con conectividad limitada. Permite a profesionales de salud gestionar pacientes, registrar consultas y priorizar casos de manera eficiente, funcionando completamente sin conexión a internet.

## Tabla de Contenidos

- [Características](#características)
- [Stack Tecnológico](#stack-tecnológico)
- [Instalación](#instalación)
  - [Instalación en Teléfono](#instalación-en-teléfono)
  - [Ejecución Local del API](#ejecución-local-del-api)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Documentación](#documentación)
- [Uso Rápido](#uso-rápido)
- [Credenciales de Prueba](#credenciales-de-prueba)
- [Solución de Problemas](#solución-de-problemas)
- [Contribuciones](#contribuciones)
- [Licencia](#licencia)

---

## Características

### Gestión de Pacientes
- Crear, editar y eliminar pacientes
- Búsqueda avanzada por nombre o cédula
- Historial completo de consultas
- Información de contacto y domicilio
- Sincronización automática con servidor

### Sistema de Triage
- Priorización en 4 niveles: Emergencia (Roja), Urgente (Amarilla), No Urgente (Verde), Planificada (Azul)
- Cambio dinámico de prioridad
- Agrupación visual por nivel
- Indicador de tiempo desde última consulta
- Interfaz intuitiva con swipe actions

### Registro de Consultas
- Registro completo de nueva consulta (motivo, diagnóstico, tratamiento, notas)
- Historial detallado con fechas y doctor responsable
- Filtro de consultas por rango de fechas
- Búsqueda en historial
- Exportación de datos (futuro)

### Autenticación y Seguridad
- Login con email y contraseña
- JWT tokens con expiración de 24 horas
- Auto-login con validación local
- Almacenamiento seguro con SecureStorage
- Cierre de sesión seguro

### Funcionalidad Offline
- **Modo offline completo**: Funciona sin internet
- **Caché inteligente**: Datos disponibles localmente
- **Sincronización automática**: Envía cambios al servidor cuando hay conexión
- **Indicador de estado**: Visual claro de online/offline
- **Resolución de conflictos**: Manejo automático de cambios
- **Base de datos local**: SQLite con persistencia completa

### Interfaz de Usuario
- Navegación con TabBar (Pacientes, Triaje, Perfil)
- Menú lateral con información de usuario
- Logout rápido desde menú
- Indicador de sincronización
- Diseño responsivo y adaptativo

---

## Stack Tecnológico

### Frontend (Aplicación Móvil)
| Tecnología | Versión | Propósito |
|-----------|---------|----------|
| **Xamarin.Forms** | 5.0+ | Framework multiplataforma |
| **C#** | 12 | Lenguaje de programación |
| **SQLite** | 3.0+ | Base de datos local |
| **Entity Framework Core** | 9 | ORM para BD local |
| **Xamarin.Essentials** | 1.7+ | APIs nativas (SecureStorage, Connectivity) |
| **MVVM Pattern** | - | Arquitectura de UI |

### Backend (API)
| Tecnología | Versión | Propósito |
|-----------|---------|----------|
| **ASP.NET Core** | 9 | Framework servidor |
| **C#** | 12 | Lenguaje de programación |
| **SQLite** | 3.0+ | Base de datos servidor |
| **Entity Framework Core** | 9 | ORM |
| **JWT** | - | Autenticación |
| **Swagger/OpenAPI** | 3.0 | Documentación API |

### DevOps
| Herramienta | Versión | Propósito |
|-----------|---------|----------|
| **GitHub Actions** | - | CI/CD |
| **.NET CLI** | 9 | Build y ejecución |
| **Docker** | (Opcional) | Containerización |

---

## 📦 Instalación

### Requisitos Previos

#### Para instalar en teléfono
- Teléfono Android con USB debugging habilitado
- Cable USB
- **ADB** instalado en tu PC
- GitHub Actions configurado (para compilar APK)

#### Para ejecutar el API localmente
- **.NET 9 SDK** instalado
- **Git** instalado
- Conexión a red (WiFi en teléfono)

#### Opcional: Para compilar desde tu PC
- **Visual Studio 2022** o **Visual Studio Code**
- **Xamarin** instalado
- **Android SDK** configurado

---

### Instalación en Teléfono

#### Opción A: GitHub Actions (Recomendado - Sin compilar en tu PC)

Esta opción es ideal si no quieres compilar en tu máquina. El build se realiza automáticamente en GitHub.

**Paso 1: Trigger the build on GitHub**

1. Asegúrate de que el repositorio está en **GitHub** (si no lo está, sube primero)
2. Abre la pestaña **Actions** de tu repositorio
3. Selecciona el workflow **Build Android APK**
4. Haz click en **Run workflow** (o espera a que se active automáticamente con push a cualquier rama)
5. El build puede tomar 5-15 minutos en la primera ejecución (instalación de Xamarin)

**Paso 2: Descargar APK**

1. Una vez completado, abre el workflow completado
2. Vé a **Artifacts**
3. Descarga `medical-atention-android-debug` (archivo ZIP)
4. Extrae el archivo `.apk`

**Paso 3: Preparar el teléfono**

En tu PC (Windows/Mac/Linux):

```bash
# Habilitar USB debugging en teléfono
# Configuración → Opciones de desarrollador → USB Debugging (ON)

# Verificar que el teléfono está conectado
adb devices

# Instalar APK
adb install -r /ruta/al/descargado/com.companyname.medical_atention-Signed.apk
```

#### Opción B: Compilar Localmente (Alternativa)

Si prefieres compilar en tu máquina:

```bash
# Clonar repositorio
git clone https://github.com/yourusername/medical-attention.git
cd medical-attention

# Compilar con Visual Studio 2022 o:
cd Medical-atentionApp/Medical-atention
msbuild Medical-atention.csproj /p:Configuration=Release /p:Platform=Android

# El APK se generará en: bin/Release/
```

---

### Ejecución Local del API

**Paso 1: Actualizar URL del API**

Encuentra tu IP de máquina:

```bash
# Linux/Mac
hostname -I | awk '{print $1}'

# Windows (en PowerShell)
ipconfig | Select-String IPv4
```

Edita este archivo:
```
Medical-atentionApp/Medical-atention/Medical-atention/Constants/AppConstants.cs
```

Reemplaza `AppConstants.ApiBaseUrl` con tu IP:

```csharp
public static string ApiBaseUrl = "http://192.168.1.100:5258"; // Tu IP
```

**Paso 2: Ejecutar el API**

```bash
cd MedicalAtention.API
dotnet run --urls "http://0.0.0.0:5258"
```

El API estará disponible en:
- **En tu PC**: http://localhost:5258
- **Desde teléfono**: http://192.168.1.100:5258 (reemplaza IP)

**Paso 3: Verificar que está corriendo**

```bash
# En otra terminal
curl http://localhost:5258/health
# Debería retornar: {"status":"healthy"}
```

---

### Flujo Completo de Instalación

```
┌─────────────────────────────────────────────────────────┐
│ 1. Preparar Repositorio                                 │
│    $ git clone <repo>                                   │
│    $ cd medical-attention                               │
└─────────────────────────────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 2. Compilar en GitHub Actions (o localmente)            │
│    GitHub Actions → Run workflow → Esperar build        │
│    O: $ msbuild Medical-atention.csproj (local)         │
└─────────────────────────────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 3. Descargar y instalar APK                             │
│    $ adb install -r medical_atention-Signed.apk         │
└─────────────────────────────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 4. Actualizar AppConstants.ApiBaseUrl                   │
│    Cambiar URL con tu IP local                          │
└─────────────────────────────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 5. Ejecutar API en tu PC                                │
│    $ cd MedicalAtention.API                             │
│    $ dotnet run                                         │
└─────────────────────────────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 6. Abrir App en teléfono y Loguear                      │
│    Email: admin@medic.com                               │
│    Password: Admin123!                                   │
└─────────────────────────────────────────────────────────┘
```

---

## Estructura del Proyecto

```
Medical-Attention/
│
├── Medical-atentionApp/              Aplicación Móvil (Xamarin.Forms)
│   └── Medical-atention/
│       ├── Views/                    Páginas XAML (UI)
│       │   ├── AppShell.xaml
│       │   ├── LoginPage.xaml
│       │   ├── PatientListPage.xaml
│       │   ├── TriagePage.xaml
│       │   ├── PatientDetailPage.xaml
│       │   ├── PatientHistoryPage.xaml
│       │   ├── RegisterPatientPage.xaml
│       │   ├── RegisterConsultationPage.xaml
│       │   └── ProfilePage.xaml
│       │
│       ├── ViewModels/               Lógica de negocio (MVVM)
│       │   ├── LoginViewModel
│       │   ├── PatientListViewModel
│       │   ├── TriageViewModel
│       │   ├── PatientDetailViewModel
│       │   ├── PatientHistoryViewModel
│       │   ├── RegisterPatientViewModel
│       │   ├── RegisterConsultationViewModel
│       │   └── ProfileViewModel
│       │
│       ├── Models/                   Modelos de datos
│       │   ├── Patient
│       │   ├── Consultation
│       │   ├── User
│       │   ├── LoginRequest/Response
│       │   └── PatientHistoryModels
│       │
│       ├── Services/                 Servicios (API, BD, etc)
│       │   ├── AuthService
│       │   ├── PatientService
│       │   ├── ConsultationService
│       │   ├── SyncService
│       │   └── HttpClientService
│       │
│       ├── Data/                     Persistencia local
│       │   ├── MedicalAttentionContext (DbContext)
│       │   └── Migrations/
│       │
│       ├── Constants/                Configuración
│       │   ├── AppConstants.cs       URLs, Keys
│       │   └── PriorityHelper.cs
│       │
│       ├── Converters/               Convertidores XAML
│       │   └── PriorityLevelToColorConverter
│       │
│       ├── Helpers/                  Utilidades
│       │   ├── JwtHelper
│       │   ├── DateHelper
│       │   └── ValidationHelper
│       │
│       └── App.xaml.cs               Entry point
│
├── MedicalAtention.API/              API REST (ASP.NET Core)
│   ├── Controllers/                  Endpoints
│   │   ├── AuthController.cs
│   │   ├── PatientsController.cs
│   │   ├── ConsultationsController.cs
│   │   ├── TriageController.cs
│   │   └── HealthController.cs
│   │
│   ├── Services/                     Servicios de negocio
│   │   ├── AuthService.cs
│   │   ├── PatientService.cs
│   │   └── ConsultationService.cs
│   │
│   ├── Models/                       Entidades de BD
│   │   ├── User.cs
│   │   ├── Patient.cs
│   │   ├── Consultation.cs
│   │   └── TriageRecord.cs
│   │
│   ├── DTOs/                         Data Transfer Objects
│   │   ├── LoginRequest/Response
│   │   ├── PatientResponse
│   │   ├── ConsultationResponse
│   │   └── PatientHistoryResponse
│   │
│   ├── Data/                         Persistencia
│   │   ├── MedicalAttentionContext.cs
│   │   ├── DbInitializer.cs
│   │   └── Migrations/
│   │
│   ├── Middleware/                   Middlewares
│   │   ├── ErrorHandlingMiddleware
│   │   └── JwtMiddleware
│   │
│   ├── Program.cs                    Configuración
│   └── appsettings.json
│
├── .github/workflows/                Automatización (CI/CD)
│   └── build-android-apk.yml         Build automático de APK
│
├── docs/                             Documentación Técnica
│   ├── research/                     Investigación y justificación
│   │   └── INVESTIGACION.md
│   ├── architecture/                 Diseño de arquitectura
│   │   └── ARQUITECTURA.md
│   ├── api-contracts/                Especificación de API
│   │   └── API_CONTRACTS.md
│   ├── screenshots/                  Capturas de la app
│   └── README.md                     Index de docs
│
├── README.md                         Este archivo
├── DB_SCHEMA.sql                     Esquema de base de datos
└── .gitignore                        Archivos ignorados por git
```

---

## Documentación

La documentación completa está en la carpeta [`docs/`](./docs/README.md):

### 📖 [Investigación](./docs/research/INVESTIGACION.md)
- Contexto y problema en zonas rurales
- Stack tecnológico justificado
- Requisitos funcionales
- Decisiones arquitectónicas

### 🏗️ [Arquitectura](./docs/architecture/ARQUITECTURA.md)
- Diagrama de arquitectura
- Explicación de capas
- Flujos de datos reales
- Modelos de datos
- Seguridad implementada

### 🔌 [API Contracts](./docs/api-contracts/API_CONTRACTS.md)
- Todos los endpoints
- Request/Response examples
- Mock data
- Rate limiting
- Ejemplos con cURL

---

## 🚀 Uso Rápido

### 1. Loguear en la App

**Credenciales de Administrador**:
```
Email: admin@medic.com
Password: Admin123!
```

**Credenciales de Doctor**:
```
Email: doctor@medic.com
Password: Doctor123!
```

### 2. Navegar por la App

- **Pacientes**: Listado de todos los pacientes, búsqueda, crear nuevo
- **Triaje**: Priorización de pacientes por nivel de urgencia
- **Perfil**: Información del usuario logueado
- **Menú**: Logout en la esquina superior

### 3. Funcionalidades Principales

```
┌─ PACIENTES
│  ├─ Ver listado completo
│  ├─ Buscar por nombre o cédula
│  ├─ Ver detalles y historial
│  ├─ Crear nuevo paciente
│  └─ Editar información
│
├─ TRIAGE
│  ├─ Ver pacientes agrupados por prioridad
│  ├─ Cambiar prioridad (deslizar)
│  ├─ Registrar nueva consulta
│  └─ Historial de cambios
│
└─ CONSULTAS
   ├─ Registrar nueva consulta
   ├─ Ver historial completo
   ├─ Filtrar por fechas
   └─ Búsqueda avanzada
```

---

## 📋 Credenciales de Prueba

### Usuarios Predefinidos

| Email | Password | Rol |
|-------|----------|-----|
| admin@medic.com | Admin123! | Administrador |
| doctor@medic.com | Doctor123! | Doctor |

### Pacientes de Prueba

Se cargan automáticamente en la base de datos. Puedes ver sus detalles en la app.

### Mock Data Disponible

Ver [`API_CONTRACTS.md`](./docs/api-contracts/API_CONTRACTS.md) para ejemplos completos de requests/responses.

---

## Solución de Problemas

### El teléfono no se conecta al API

**Síntoma**: "No se puede conectar al servidor"

**Soluciones**:
1. Verifica que teléfono y PC estén en la **misma red WiFi**
2. Confirma que el API está ejecutando: `dotnet run`
3. Comprueba tu IP: `ipconfig` (Windows) o `hostname -I` (Linux)
4. Actualiza `AppConstants.ApiBaseUrl` con tu IP correcta
5. Reinstala la app después de cambiar la URL

### GitHub Actions workflow falla

**Síntoma**: "Error en el build de APK"

**Soluciones**:
1. Revisa el log en **Actions** → workflow fallido
2. Los primeros builds son más lentos (instala Xamarin)
3. Reintenta con **Run workflow** manualmente
4. Verifica que no haya archivos no compilables en la rama

### App se cierra al loguear

**Síntoma**: Crash después de ingresar credenciales

**Soluciones**:
1. Verifica que el API está corriendo
2. Comprueba la URL en `AppConstants.cs`
3. Limpia la caché de la app: `adb shell pm clear com.companyname.medical_atention`
4. Reinstala: `adb install -r app.apk`

### Base de datos local corrupta

**Síntoma**: "Database error" o app no abre

**Soluciones**:
1. Desinstala la app: `adb uninstall com.companyname.medical_atention`
2. Borra archivos de BD locales
3. Reinstala: `adb install -r app.apk`
4. Loguea nuevamente

### Sincronización no funciona

**Síntoma**: Cambios offline no se sincronizan al conectar

**Soluciones**:
1. Asegúrate de estar online (indicador en la UI)
2. Espera 5-10 segundos a que la sincronización automática inicie
3. Swipe down (refresh) en la pantalla de pacientes
4. Revisa los logs del API en consola

---

## Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

### Estándares de Código

- **C#**: Sigue [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **XAML**: Sangría de 4 espacios, nombres descriptivos
- **Commits**: Mensajes claros en inglés o español
- **Documentación**: Actualiza docs si cambias arquitectura

---

## Licencia

Este proyecto está bajo la licencia MIT. Ver [`LICENSE`](./LICENSE) para detalles.

---

## Soporte

### Documentación Completa
- [Documentación Técnica](./docs/README.md)
- [Arquitectura](./docs/architecture/ARQUITECTURA.md)
- [API Contracts](./docs/api-contracts/API_CONTRACTS.md)

### Reportar Problemas
- GitHub Issues: [Issues](https://github.com/yourusername/medical-attention/issues)
- Email: support@medical-attention.com

### Comunidad
- Telegram: [@MedicalAttentionCommunity](https://t.me/medicalattention)
- Slack: [Workspace](https://medicalattention.slack.com)

---

## Roadmap

### v1.0 (Actual)
- [x] Autenticación con JWT
- [x] CRUD de pacientes
- [x] Sistema de triage
- [x] Registro de consultas
- [x] Funcionalidad offline
- [x] Sincronización automática

### v2.0 (Planificado)
- [ ] Cliente Web
- [ ] Dashboard administrativo
- [ ] Reportes analíticos
- [ ] Integración de hospital

### v3.0+ (Futuro)
- [ ] Push notifications
- [ ] Telemedicina
- [ ] Machine Learning
- [ ] Integración de sensores

---

## Autores

- **Jason Madrigal** - Desarrollo Frontend & Backend
- **Equipo de Desarrollo** - Contribuciones

---

## Agradecimientos

- Comunidad de Xamarin.Forms
- ASP.NET Core team
- Todos los que contribuyen a hacer mejores sistemas de salud

---

**Última actualización**: Junio 2024  
**Versión**: 1.0  
**Estado**: ✅ Producción
