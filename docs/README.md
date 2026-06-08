# Documentación - Sistema de Atención Médica Rural

Este directorio contiene toda la documentación técnica y arquitectónica del proyecto Medical Attention.

## Contenidos

### Research (./research/INVESTIGACION.md)
Documento de investigación que incluye:
- **Contexto y Problema**: Desafíos en zonas rurales
- **Solución Propuesta**: Características y beneficios
- **Stack Tecnológico**: Justificación de tecnologías (Xamarin, ASP.NET Core, SQLite)
- **Requisitos Funcionales**: Autenticación, pacientes, triage, consultas, offline
- **Decisiones Arquitectónicas**: Offline-first, separación backend/frontend, SQLite local, MVVM

### Architecture (./architecture/ARQUITECTURA.md)
Documento de arquitectura completo que incluye:
- **Diagrama General**: Capas cliente-servidor y flujos de datos
- **Capas Arquitectónicas**: Presentación, Lógica, Servicios, Persistencia
- **Flujos de Datos**: Escenarios reales (login, crear paciente, triage, sincronización)
- **Decisiones Técnicas**: MVVM, Offline-first sync, JWT, EF Core, Retry logic
- **Modelos de Datos**: User, Patient, Consultation, TriageRecord
- **Seguridad**: Autenticación, almacenamiento, comunicación
- **Escalabilidad Futura**: Fases 2, 3, 4 del proyecto

### API Contracts (./api-contracts/API_CONTRACTS.md)
Especificación completa de la API REST que incluye:
- **Authentication**: Login, Refresh token, Logout
- **Patients**: Get all, Get by ID, Create, Update, Delete, Change priority, Get history
- **Consultations**: Create, Get all, Update, Delete
- **Triage**: Get grouped list
- **Error Responses**: 400, 401, 403, 404, 500
- **Headers Requeridos**: JWT Bearer Token
- **Autenticación JWT**: Token format, payload, expiración
- **Rate Limiting**: Recomendaciones
- **Versioning**: API v1
- **Testing con cURL**: Ejemplos de uso

### Screenshots (./screenshots/)
Capturas de pantalla de la aplicación en diferentes estados:
- Pantalla de login
- Listado de pacientes
- Detalle de paciente con historial
- Sistema de triage con priorización
- Registro de consulta
- Perfil de usuario
- Estado offline/online

---

## Estructura del Proyecto

```
Medical-Attention/
├── docs/                          Documentación Técnica
│   ├── research/                  Investigación y justificación
│   ├── architecture/              Diseño de arquitectura
│   ├── api-contracts/             Especificación de API
│   ├── screenshots/               Capturas de pantalla
│   └── README.md                  Este archivo
│
├── Medical-atentionApp/           Aplicación móvil (Xamarin.Forms)
│   ├── Medical-atention/
│   │   ├── Views/                 Páginas XAML
│   │   ├── ViewModels/            Lógica de negocio
│   │   ├── Models/                Modelos de datos
│   │   ├── Services/              Servicios (API, DB, etc)
│   │   ├── Constants/             Constantes (URLs, keys)
│   │   └── Converters/            Convertidores XAML
│   └── ...
│
├── MedicalAttention.API/          API REST (ASP.NET Core)
│   ├── Controllers/               Controladores
│   ├── Services/                  Servicios de negocio
│   ├── Models/                    Entidades de base de datos
│   ├── Data/                      DbContext, migraciones
│   ├── DTOs/                      Data transfer objects
│   └── ...
│
├── .github/workflows/             Automatización (GitHub Actions)
│   └── build-android-apk.yml      Build automático de APK
│
├── README.md                       Guía principal del proyecto
├── DB_SCHEMA.sql                  Schema inicial de BD
└── .gitignore
```

---

## Flujos Principales

### 1. Autenticación
```
Usuario → Login (email/password) → API valida → JWT generado
                                  ↓
                        Guardado en SecureStorage
                                  ↓
                        Acceso a la aplicación
```

### 2. Crear Paciente (Con conexión)
```
Formulario → ViewModel valida → ServiceAPI → HTTP POST
                                              ↓
                                        API almacena en BD
                                              ↓
                                        Respuesta al cliente
                                              ↓
                                        Caché local actualizado
```

### 3. Crear Paciente (Sin conexión)
```
Formulario → ViewModel valida → ServiceAPI → Falla HTTP
                                              ↓
                                        Guardado local con flag "pending_sync"
                                              ↓
                                        Usuario vé indicador "Pendiente"
                                              ↓
                                        Cuando hay conexión:
                                        SyncService detecta y envía
```

### 4. Sistema de Triage
```
TriageViewModel → Carga pacientes (API o caché)
                                  ↓
                        Agrupa por nivel de prioridad
                                  ↓
                        Usuario cambia prioridad
                                  ↓
                        HTTP PATCH (si online) o local pending_sync
                                  ↓
                        UI actualizada inmediatamente
```

---

## Tecnologías Clave

| Capa | Tecnología | Versión | Propósito |
|------|-----------|---------|----------|
| **Frontend** | Xamarin.Forms | 5.0+ | UI multiplataforma (iOS/Android) |
| **Backend** | ASP.NET Core | 9 | API REST robusto |
| **BD Local** | SQLite | 3.0+ | Persistencia offline |
| **ORM** | Entity Framework Core | 9 | Mapeo objeto-relacional |
| **Autenticación** | JWT (Bearer) | - | Autenticación stateless |
| **CI/CD** | GitHub Actions | - | Build automático APK |
| **Patrón** | MVVM | - | Arquitectura frontend |

---

## Requisitos Funcionales Principales

✅ **Autenticación**
- Login con email/contraseña
- JWT con expiración 24h
- Auto-login con validación

✅ **Pacientes**
- CRUD completo
- Búsqueda por nombre/cédula
- Historial de consultas
- Priorización (triage)

✅ **Consultas**
- Registro de nuevas consultas
- Historial completo
- Filtro por fechas
- Notas y diagnósticos

✅ **Triage**
- 4 niveles de prioridad (Roja, Amarilla, Verde, Azul)
- Cambio dinámico de prioridad
- Agrupación visual
- Sincronización automática

✅ **Offline**
- Funcionalidad completa sin internet
- Sincronización automática
- Indicador de estado
- Resolución de conflictos

---

## Decisiones Arquitectónicas

### ✅ Por qué MVVM
- Separación clara de responsabilidades
- Código testeable
- Reutilizable en múltiples vistas
- Soporte nativo en Xamarin.Forms

### ✅ Por qué Offline-First
- Zonas rurales = conectividad intermitente
- Mejora experiencia de usuario
- Datos accesibles siempre
- Sincronización transparente

### ✅ Por qué SQLite Local
- Sin servidor (portátil)
- Datos persistentes
- Sincronización confiable
- Soporte nativo en EF Core

### ✅ Por qué JWT
- Stateless (no requiere sesión servidor)
- Seguro y firmado
- Fácil de validar offline
- Compatible con CORS

---

## Cómo Usar Esta Documentación

### Para Desarrolladores
1. Lee [**ARQUITECTURA.md**](./architecture/ARQUITECTURA.md) para entender el diseño
2. Consulta [**API_CONTRACTS.md**](./api-contracts/API_CONTRACTS.md) para integración backend
3. Refiere [**INVESTIGACION.md**](./research/INVESTIGACION.md) para decisiones técnicas

### Para Diseñadores
1. Revisa [**Screenshots**](./screenshots/) para ver UX actual
2. Lee [**ARQUITECTURA.md**](./architecture/ARQUITECTURA.md) sección "Capa de Presentación"

### Para Project Managers
1. Revisa [**INVESTIGACION.md**](./research/INVESTIGACION.md) "Requisitos Funcionales"
2. Consulta [**ARQUITECTURA.md**](./architecture/ARQUITECTURA.md) "Escalabilidad Futura"

### Para QA
1. Consulta [**API_CONTRACTS.md**](./api-contracts/API_CONTRACTS.md) para casos de prueba
2. Refiere [**INVESTIGACION.md**](./research/INVESTIGACION.md) para requisitos

---

## Roadmap Futuro

### Fase 2 (Q3 2024)
- [ ] Cliente Web (ASP.NET MVC/Blazor)
- [ ] Dashboard administrativo
- [ ] Reportes analíticos

### Fase 3 (Q4 2024)
- [ ] Integración con sistemas de hospital
- [ ] Push notifications para urgencias
- [ ] Sincronización en tiempo real (SignalR)

### Fase 4 (2025)
- [ ] Machine Learning para predicción de riesgo
- [ ] Telemedicina (video consultas)
- [ ] Integración de sensores médicos

---

## Links Rápidos

- [README Principal](../README.md) - Guía de instalación
- [DB Schema](../DB_SCHEMA.sql) - Estructura de base de datos
- [GitHub Actions](../.github/workflows/) - CI/CD

---

## Contacto y Soporte

Para dudas sobre la documentación o arquitectura:
1. Revisa los comentarios en el código
2. Consulta los commits relacionados (`git log --grep="arquitectura"`)
3. Abre un issue en GitHub con etiqueta `documentation`

---

**Última actualización**: Junio 2024  
**Versión de Documentación**: 1.0  
**Aplicable a**: Medical Attention v1.0+
