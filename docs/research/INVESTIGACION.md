# Investigación - Sistema de Atención Médica Rural

## Resumen Ejecutivo

El **Sistema de Atención Médica Rural** es una solución integral diseñada para mejorar la calidad y eficiencia de la atención médica en zonas rurales con conectividad limitada. La aplicación permite a profesionales de salud gestionar pacientes, registrar consultas y priorizar casos de manera eficiente, tanto en línea como sin conexión.

## Contexto y Problema

### Desafíos en Zonas Rurales
- **Conectividad intermitente**: Las zonas rurales carecen de conexión constante a internet
- **Recursos limitados**: Personal médico reducido y equipamiento básico
- **Falta de registro digital**: Historiales médicos en papel o inexistentes
- **Urgencias sin priorización**: Dificultad para identificar casos críticos

### Solución Propuesta
Una aplicación móvil que:
1. Funcione con o sin conexión a internet (sincronización automática)
2. Permita gestión completa de pacientes y consultas
3. Implemente sistema de triage para priorización
4. Proporcione historial médico digital y accesible

## Investigación Tecnológica

### Stack Seleccionado

#### Backend
- **Framework**: ASP.NET Core 9
- **Base de Datos**: SQLite (portátil, sin servidor)
- **ORM**: Entity Framework Core (migraciones, relaciones complejas)
- **Autenticación**: JWT (tokens seguros, sin estado)

#### Frontend
- **Framework**: Xamarin.Forms (código compartido iOS/Android)
- **Patrón**: MVVM (separación concerns, testabilidad)
- **Persistencia Local**: SQLite + Entity Framework Core

### Justificación Tecnológica

| Tecnología | Razón |
|-----------|--------|
| **Xamarin.Forms** | Código único para iOS/Android, desarrollo rápido |
| **ASP.NET Core** | Robusto, performance, comunidad .NET grande |
| **SQLite** | Sin servidor, portátil, perfecto para offline-first |
| **JWT** | Autenticación stateless, ideal para APIs REST |
| **Entity Framework Core** | ORM potente, migraciones automáticas, relaciones |

## Requisitos Funcionales

### Autenticación y Seguridad
- ✅ Login con email/contraseña
- ✅ Tokens JWT con expiración
- ✅ Almacenamiento seguro de credenciales (SecureStorage)
- ✅ Auto-login con validación de token

### Gestión de Pacientes
- ✅ Crear pacientes (nombre, cédula, teléfono, etc.)
- ✅ Listar y buscar pacientes
- ✅ Visualizar historial de consultas
- ✅ Editar datos del paciente

### Sistema de Triage
- ✅ Asignar prioridad (Roja, Amarilla, Verde, Azul)
- ✅ Cambiar prioridad dinámicamente
- ✅ Agrupar pacientes por nivel
- ✅ Indicar tiempo desde última consulta

### Gestión de Consultas
- ✅ Registrar nueva consulta
- ✅ Visualizar historial de consultas
- ✅ Filtrar por fecha
- ✅ Sincronizar cuando hay conexión

### Funcionalidad Offline
- ✅ Caché local de datos
- ✅ Sincronización automática al recuperar conexión
- ✅ Indicador visual de estado online/offline
- ✅ Persistencia de cambios locales

## Decisiones Arquitectónicas Clave

### 1. Offline-First
**Decisión**: Implementar sincronización automática en lugar de bloquear la app sin conexión.
**Justificación**: Mejora UX en zonas rurales, permite trabajo continuo.

### 2. Separación Backend/Frontend
**Decisión**: API REST independiente, Xamarin como cliente.
**Justificación**: Permite versiones futuras (web, otro móvil), mantenimiento independiente.

### 3. SQLite Local
**Decisión**: Base de datos local en el dispositivo, no solo caché.
**Justificación**: Datos persistentes incluso sin conectar nunca al servidor.

### 4. MVVM Pattern
**Decisión**: Usar ViewModels para separar lógica de UI.
**Justificación**: Código testeable, mantenible, reutilizable.

## Validación de Supuestos

- ✅ Xamarin.Forms es viable para aplicaciones médicas
- ✅ SQLite + EF Core permite sincronización confiable
- ✅ JWT es suficiente para seguridad de esta escala
- ✅ Offline-first es crítico en ambiente rural

## Conclusiones

El stack seleccionado es apropiado para:
- Desarrollo rápido con máximo code reuse
- Escalabilidad futura (web, múltiples clientes)
- Robustez en entornos con conectividad limitada
- Facilidad de mantenimiento y evolución

Las decisiones arquitectónicas priorizan **usuario final** (experiencia sin conexión) sobre **comodidad del desarrollador**.
