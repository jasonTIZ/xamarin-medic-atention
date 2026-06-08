# Screenshots - Medical Attention App

Esta carpeta contiene capturas de pantalla de la aplicación en diferentes estados y flujos.

## 📸 Cómo Agregar Screenshots

1. Ejecuta la app en un emulador o dispositivo
2. Toma capturas de pantalla de cada pantalla principal
3. Renómbralas según el formato: `01_login.png`, `02_patient_list.png`, etc.
4. Colócalas en esta carpeta
5. Actualiza la sección correspondiente abajo

## 📱 Pantallas Principales

### 1. Login
**Archivo**: `01_login.png`
- Email y contraseña
- Botón de login
- Indicador de carga
- Manejo de errores

### 2. Pacientes - Listado
**Archivo**: `02_patient_list.png`
- Listado de todos los pacientes
- Barra de búsqueda
- Items con nombre, cédula, prioridad
- Botón para crear nuevo paciente

### 3. Pacientes - Detalle
**Archivo**: `03_patient_detail.png`
- Información completa del paciente
- Historial de consultas
- Datos de contacto
- Botones de acción (editar, eliminar)

### 4. Triage - Listado Agrupado
**Archivo**: `04_triage_list.png`
- Pacientes agrupados por prioridad
- Colores distintivos para cada nivel:
  - **Rojo** (Emergencia)
  - **Amarillo** (Urgente)
  - **Verde** (No urgente)
  - **Azul** (Planificada)
- Swipe para cambiar prioridad

### 5. Triage - Cambiar Prioridad
**Archivo**: `05_triage_priority_change.png`
- Action sheet con opciones de prioridad
- Seleccionar nuevo nivel
- Confirmación

### 6. Registro de Consulta
**Archivo**: `06_register_consultation.png`
- Formulario con campos:
  - Motivo
  - Diagnóstico
  - Tratamiento
  - Notas
- Botón guardar
- Validación de campos

### 7. Historial de Consultas
**Archivo**: `07_consultation_history.png`
- Lista de consultas por paciente
- Fechas y doctor responsable
- Descripción breve
- Opciones de filtro

### 8. Perfil de Usuario
**Archivo**: `08_profile.png`
- Información del usuario logueado
- Nombre y rol
- Email
- Botón de logout

### 9. Estado Offline
**Archivo**: `09_offline_indicator.png`
- Banner rojo indicando sin conexión
- Cambios guardados localmente
- Indicador de sincronización pendiente

### 10. Estado Online/Sincronizando
**Archivo**: `10_syncing.png`
- Indicador de sincronización activa
- Spinner de carga
- Mensaje "Sincronizando cambios..."

---

## 📷 Flujos Completos (Secuencias)

### Flujo 1: Login y Acceso
```
01_login.png
    ↓
02_patient_list.png (home después de login)
```

### Flujo 2: Ver Paciente Completo
```
02_patient_list.png (tap en paciente)
    ↓
03_patient_detail.png
    ↓
07_consultation_history.png (tap en historial)
```

### Flujo 3: Triaje y Cambio de Prioridad
```
04_triage_list.png (pantalla inicial)
    ↓
05_triage_priority_change.png (swipe en paciente)
    ↓
04_triage_list.png (actualizado con nueva prioridad)
```

### Flujo 4: Crear Consulta
```
04_triage_list.png (botón "Registrar Consulta")
    ↓
06_register_consultation.png (formulario)
    ↓
04_triage_list.png (actualizado)
```

### Flujo 5: Modo Offline
```
09_offline_indicator.png (sin conexión)
    ↓
02_patient_list.png (datos del caché)
    ↓
10_syncing.png (recupera conexión)
```

---

## 🎨 Especificaciones de Screenshots

### Dispositivo Recomendado
- **Resolución**: 1080x2340px (Pixel 4a - Android)
- **Formato**: PNG
- **Tamaño**: < 1MB por imagen
- **Orientación**: Portrait (vertical)

### Información a Incluir
- Usuario logueado visible (ej: "Dr. García" en header)
- Estado de sincronización (si aplica)
- Hora del dispositivo
- Conexión WiFi/Datos visible

---

## 📝 Template para Documentar Screenshot

```markdown
## [Número]. [Nombre de Pantalla]

**Archivo**: `XX_descripcion.png`

**Descripción**: Una línea describiendo qué se ve

**Elementos principales**:
- Elemento 1
- Elemento 2
- Elemento 3

**Interacciones**:
- Qué hace el usuario
- Dónde toca
- Qué sucede después

**Casos especiales**:
- Validaciones mostradas
- Estados de error
- Estados de carga
```

---

## 📊 Checklist de Cobertura

- [ ] Pantalla de Login (éxito y error)
- [ ] Listado de Pacientes (vacío y con datos)
- [ ] Búsqueda de Pacientes (resultados y sin resultados)
- [ ] Detalle de Paciente (con y sin consultas)
- [ ] Registro de Paciente (formulario)
- [ ] Triage agrupado (todos los niveles de prioridad)
- [ ] Cambio de Prioridad (action sheet)
- [ ] Registro de Consulta (formulario)
- [ ] Historial de Consultas (con filtros)
- [ ] Perfil de Usuario (información y logout)
- [ ] Indicador Offline (banner de sin conexión)
- [ ] Indicador Sincronizando (spinner y mensaje)
- [ ] Menu Lateral (con información del usuario)
- [ ] Errores comunes (conexión, validación, servidor)

---

## 🎬 Videos Demo (Opcional)

También puedes incluir videos de:
- Flujo completo de login y navegación
- Crear paciente y consulta
- Sistema de triage
- Funcionamiento offline

---

## 📎 Links Útiles

- [Volver a Documentación](../README.md)
- [Guía de Instalación](../../README.md#instalación)
- [Arquitectura](../architecture/ARQUITECTURA.md)

---

**Nota**: Las imágenes deben ser actualizadas cuando hay cambios en la UI.
Última actualización: Junio 2024
