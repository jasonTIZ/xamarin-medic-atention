# API Contracts - Sistema de Atención Médica Rural

**Base URL**: `http://localhost:5258/api` (desarrollo)  
**Versión**: v1  
**Autenticación**: JWT Bearer Token

---

## 1. Authentication Endpoints

### 1.1 Login
**Endpoint**: `POST /auth/login`

**Descripción**: Autentica un usuario y retorna JWT token.

**Request**:
```json
{
  "email": "doctor@medic.com",
  "password": "Doctor123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Login exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-06-09T10:30:00Z",
    "user": {
      "id": 1,
      "email": "doctor@medic.com",
      "fullName": "Dr. Juan García",
      "role": "doctor"
    }
  }
}
```

**Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Credenciales inválidas"
}
```

**Mock Data**:
```
Email: admin@medic.com
Password: Admin123!

Email: doctor@medic.com
Password: Doctor123!
```

---

### 1.2 Refresh Token
**Endpoint**: `POST /auth/refresh`

**Descripción**: Renueva un JWT token expirado.

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-06-09T10:30:00Z"
  }
}
```

---

### 1.3 Logout
**Endpoint**: `POST /auth/logout`

**Descripción**: Cierra la sesión del usuario (limpia token del servidor).

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Sesión cerrada exitosamente"
}
```

---

## 2. Patients Endpoints

### 2.1 Get All Patients
**Endpoint**: `GET /patients`

**Descripción**: Obtiene listado de todos los pacientes.

**Headers**:
```
Authorization: Bearer {token}
```

**Query Parameters**:
- `search` (optional): Busca por nombre o cédula
- `skip` (optional): Paginación, registros a saltar (default: 0)
- `take` (optional): Paginación, cantidad a traer (default: 50)

**Example**: `GET /patients?search=juan&skip=0&take=20`

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "fullName": "Juan Pérez",
      "identificationNumber": "12345678",
      "dateOfBirth": "1990-05-15T00:00:00Z",
      "phone": "+1234567890",
      "address": "Calle Principal 123",
      "lastConsultationDate": "2024-06-05T10:30:00Z",
      "priority": "Yellow",
      "priorityLabel": "Urgente"
    },
    {
      "id": 2,
      "fullName": "María González",
      "identificationNumber": "87654321",
      "dateOfBirth": "1985-03-20T00:00:00Z",
      "phone": "+0987654321",
      "address": "Avenida Central 456",
      "lastConsultationDate": "2024-06-01T14:00:00Z",
      "priority": "Green",
      "priorityLabel": "No urgente"
    }
  ],
  "totalCount": 125
}
```

---

### 2.2 Get Patient by ID
**Endpoint**: `GET /patients/{id}`

**Descripción**: Obtiene detalles completos de un paciente.

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": 1,
    "fullName": "Juan Pérez",
    "identificationNumber": "12345678",
    "dateOfBirth": "1990-05-15T00:00:00Z",
    "phone": "+1234567890",
    "address": "Calle Principal 123",
    "createdAt": "2024-05-10T08:00:00Z",
    "updatedAt": "2024-06-05T10:30:00Z",
    "lastConsultationDate": "2024-06-05T10:30:00Z",
    "consultations": [
      {
        "id": 1,
        "reason": "Dolor de cabeza",
        "diagnosis": "Migraña",
        "treatment": "Reposo y analgésicos",
        "createdAt": "2024-06-05T10:30:00Z"
      }
    ]
  }
}
```

**Response** (404 Not Found):
```json
{
  "success": false,
  "message": "Paciente no encontrado"
}
```

---

### 2.3 Create Patient
**Endpoint**: `POST /patients`

**Descripción**: Crea un nuevo paciente.

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:
```json
{
  "fullName": "Carlos López",
  "identificationNumber": "11223344",
  "dateOfBirth": "1995-07-22T00:00:00Z",
  "phone": "+1234567890",
  "address": "Calle Nueva 789"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Paciente creado exitosamente",
  "data": {
    "id": 150,
    "fullName": "Carlos López",
    "identificationNumber": "11223344",
    "dateOfBirth": "1995-07-22T00:00:00Z",
    "phone": "+1234567890",
    "address": "Calle Nueva 789",
    "createdAt": "2024-06-08T15:45:00Z",
    "updatedAt": "2024-06-08T15:45:00Z"
  }
}
```

**Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Cédula ya existe en el sistema"
}
```

---

### 2.4 Update Patient
**Endpoint**: `PUT /patients/{id}`

**Descripción**: Actualiza datos de un paciente existente.

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:
```json
{
  "fullName": "Juan Pérez González",
  "phone": "+9999999999",
  "address": "Calle Actualizada 123"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Paciente actualizado exitosamente",
  "data": {
    "id": 1,
    "fullName": "Juan Pérez González",
    "identificationNumber": "12345678",
    "dateOfBirth": "1990-05-15T00:00:00Z",
    "phone": "+9999999999",
    "address": "Calle Actualizada 123",
    "updatedAt": "2024-06-08T16:00:00Z"
  }
}
```

---

### 2.5 Delete Patient
**Endpoint**: `DELETE /patients/{id}`

**Descripción**: Elimina (marca como inactivo) un paciente.

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Paciente eliminado exitosamente"
}
```

---

### 2.6 Change Patient Priority
**Endpoint**: `PATCH /patients/{id}/priority`

**Descripción**: Cambia la prioridad de triage de un paciente.

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:
```json
{
  "priority": "Red",
  "reason": "Paciente con fiebre alta y síntomas de emergencia"
}
```

**Priority Levels**:
- `Red` - Emergencia (rojo)
- `Yellow` - Urgente (amarillo)
- `Green` - No urgente (verde)
- `Blue` - Consulta planificada (azul)

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Prioridad actualizada exitosamente",
  "data": {
    "id": 1,
    "priority": "Red",
    "priorityLabel": "Emergencia",
    "updatedAt": "2024-06-08T16:15:00Z",
    "assignedBy": "Dr. Juan García"
  }
}
```

---

### 2.7 Get Patient History
**Endpoint**: `GET /patients/{id}/history`

**Descripción**: Obtiene historial completo de consultas de un paciente.

**Headers**:
```
Authorization: Bearer {token}
```

**Query Parameters**:
- `fromDate` (optional): Filtrar desde fecha (ISO 8601)
- `toDate` (optional): Filtrar hasta fecha (ISO 8601)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "patientId": 1,
    "patientName": "Juan Pérez",
    "totalConsultations": 5,
    "consultations": [
      {
        "id": 5,
        "date": "2024-06-05T10:30:00Z",
        "reason": "Dolor de cabeza",
        "diagnosis": "Migraña",
        "treatment": "Reposo y analgésicos",
        "doctorName": "Dr. García",
        "notes": "Paciente refiere dolor desde ayer"
      },
      {
        "id": 4,
        "date": "2024-05-28T09:00:00Z",
        "reason": "Control de presión",
        "diagnosis": "Hipertensión controlada",
        "treatment": "Mantener medicación",
        "doctorName": "Dra. López",
        "notes": "Presión: 140/90"
      }
    ]
  }
}
```

---

## 3. Consultations Endpoints

### 3.1 Create Consultation
**Endpoint**: `POST /consultations`

**Descripción**: Registra una nueva consulta para un paciente.

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:
```json
{
  "patientId": 1,
  "reason": "Dolor de garganta",
  "diagnosis": "Faringitis aguda",
  "treatment": "Antibióticos + reposo",
  "notes": "Paciente con fiebre de 38.5°C. Recomendados antibióticos por 7 días"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Consulta registrada exitosamente",
  "data": {
    "id": 25,
    "patientId": 1,
    "patientName": "Juan Pérez",
    "doctorId": 2,
    "doctorName": "Dr. García",
    "reason": "Dolor de garganta",
    "diagnosis": "Faringitis aguda",
    "treatment": "Antibióticos + reposo",
    "notes": "Paciente con fiebre de 38.5°C. Recomendados antibióticos por 7 días",
    "createdAt": "2024-06-08T16:30:00Z"
  }
}
```

---

### 3.2 Get All Consultations
**Endpoint**: `GET /consultations`

**Descripción**: Obtiene todas las consultas (con filtros opcionales).

**Headers**:
```
Authorization: Bearer {token}
```

**Query Parameters**:
- `patientId` (optional): Filtrar por paciente
- `fromDate` (optional): Filtrar desde fecha
- `toDate` (optional): Filtrar hasta fecha
- `skip` (optional): Paginación
- `take` (optional): Cantidad a traer

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": 25,
      "patientId": 1,
      "patientName": "Juan Pérez",
      "doctorName": "Dr. García",
      "reason": "Dolor de garganta",
      "diagnosis": "Faringitis aguda",
      "createdAt": "2024-06-08T16:30:00Z"
    }
  ],
  "totalCount": 142
}
```

---

### 3.3 Update Consultation
**Endpoint**: `PUT /consultations/{id}`

**Descripción**: Actualiza una consulta existente.

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:
```json
{
  "reason": "Dolor de garganta - seguimiento",
  "diagnosis": "Faringitis aguda - mejoría",
  "treatment": "Continuar antibióticos 3 días más",
  "notes": "Paciente mejora notoriamente, fiebre desaparición"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Consulta actualizada exitosamente",
  "data": {
    "id": 25,
    "patientId": 1,
    "updatedAt": "2024-06-08T17:00:00Z"
  }
}
```

---

### 3.4 Delete Consultation
**Endpoint**: `DELETE /consultations/{id}`

**Descripción**: Elimina una consulta.

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Consulta eliminada exitosamente"
}
```

---

## 4. Triage Endpoints

### 4.1 Get Triage List
**Endpoint**: `GET /triage`

**Descripción**: Obtiene lista de pacientes agrupados por prioridad.

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "groups": [
      {
        "title": "Emergencia",
        "priority": "Red",
        "backgroundColor": "#FF4444",
        "textColor": "#FFFFFF",
        "patients": [
          {
            "id": 2,
            "fullName": "María González",
            "identificationNumber": "87654321",
            "lastConsultationText": "Hace 5 días",
            "priority": "Red",
            "priorityLabel": "Emergencia"
          }
        ]
      },
      {
        "title": "Urgente",
        "priority": "Yellow",
        "backgroundColor": "#FFBB33",
        "textColor": "#000000",
        "patients": [
          {
            "id": 1,
            "fullName": "Juan Pérez",
            "identificationNumber": "12345678",
            "lastConsultationText": "Hace 2 días",
            "priority": "Yellow",
            "priorityLabel": "Urgente"
          }
        ]
      },
      {
        "title": "No urgente",
        "priority": "Green",
        "backgroundColor": "#00C851",
        "textColor": "#FFFFFF",
        "patients": []
      },
      {
        "title": "Planificada",
        "priority": "Blue",
        "backgroundColor": "#2196F3",
        "textColor": "#FFFFFF",
        "patients": []
      }
    ],
    "totalPatients": 2,
    "lastSync": "2024-06-08T17:15:00Z"
  }
}
```

---

## 5. Error Responses

Todos los endpoints pueden retornar los siguientes errores:

### 400 Bad Request
```json
{
  "success": false,
  "message": "Datos inválidos",
  "errors": {
    "email": ["El email es requerido"],
    "password": ["La contraseña debe tener al menos 8 caracteres"]
  }
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "No autorizado. Token inválido o expirado"
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "No tiene permisos para acceder a este recurso"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Recurso no encontrado"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Error interno del servidor",
  "details": "[Solo en desarrollo] Error details..."
}
```

---

## 6. Headers Requeridos

Todos los endpoints (excepto Login) requieren:

```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

---

## 7. Autenticación JWT

**Token Format**:
```
Header.Payload.Signature
```

**Payload ejemplo**:
```json
{
  "sub": "1",
  "email": "doctor@medic.com",
  "fullName": "Dr. Juan García",
  "role": "doctor",
  "iat": 1717945800,
  "exp": 1717959200,
  "iss": "MedicalAttentionAPI",
  "aud": "MedicalAttentionApp"
}
```

**Expiración**: 24 horas desde emisión.

---

## 8. Rate Limiting

Se recomienda implementar rate limiting en producción:
- **Login**: 5 intentos por 15 minutos por IP
- **General API**: 100 requests por minuto por token
- **File upload**: 10 uploads por hora

---

## 9. Versioning

**URL Pattern**: `/api/v1/...`

Futuras versiones irán en `/api/v2/...`, `/api/v3/...`, etc.

El cliente debe especificar versión en headers o URL.

---

## 10. Testing con cURL

```bash
# Login
curl -X POST http://localhost:5258/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"doctor@medic.com","password":"Doctor123!"}'

# Get Patients (reemplazar TOKEN)
curl -X GET http://localhost:5258/api/patients \
  -H "Authorization: Bearer TOKEN"

# Create Patient
curl -X POST http://localhost:5258/api/patients \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName":"Test Patient",
    "identificationNumber":"99999999",
    "dateOfBirth":"1990-01-01T00:00:00Z",
    "phone":"+1234567890",
    "address":"Test Address"
  }'
```
