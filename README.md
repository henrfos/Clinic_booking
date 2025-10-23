# Clinic Appointment Booking System  
*A full-stack medical booking system built with ASP.NET Core, MySQL, and React (Next.js).*

---

## Project Overview
This repository contains a full-stack web application that allows patients to book a doctor's appointment online without authentication.  
Patients can select a clinic, doctor, category, and appointment time, while the system ensures no scheduling conflicts occur.  

---

### ENDPOINTS

### Clinics

| Method | Endpoint | Description |
|--------|-----------|--------------|
| **GET** | `/api/clinics` | Retrieves a complete list of all clinics in the system. Each clinic record includes its unique ID, name, and references to associated doctors. |
| **GET** | `/api/clinics/{id}` | Retrieves a single clinic by its ID. Returns `404 Not Found` if the clinic does not exist. |
| **POST** | `/api/clinics` | Creates a new clinic record. The request body must include a valid clinic name. Duplicate clinic names are not allowed. |
| **PUT** | `/api/clinics/{id}` | Updates the details of an existing clinic. The provided `id` must match the ID in the request body. |
| **DELETE** | `/api/clinics/{id}` | Deletes a clinic from the database. Deletion is blocked if the clinic still has doctors assigned to it. |

---

### Doctors

| Method | Endpoint | Description |
|--------|-----------|--------------|
| **GET** | `/api/doctors` | Returns a list of all doctors across all clinics. Each doctor includes their clinic ID, speciality ID, and relational data if included with `.Include()`. |
| **GET** | `/api/doctors/{id}` | Retrieves a specific doctor by ID. Returns `404` if no doctor exists with that ID. |
| **POST** | `/api/doctors` | Creates a new doctor. A doctor must include a `firstName`, `lastName`, `clinicId`, and `specialityId`. Validation prevents duplicate entries for the same combination of name, clinic, and speciality. |
| **PUT** | `/api/doctors/{id}` | Updates doctor details such as name or associated clinic/speciality. Returns `400` if the ID in the URL doesn’t match the doctor ID in the body. |
| **DELETE** | `/api/doctors/{id}` | Deletes a doctor by ID. Deletion is blocked if the doctor has existing appointments in the system. |
| **GET** | `/api/doctors/search?q={query}` | Searches for doctors by first or last name. Returns a list containing the doctor’s full name, clinic name, and speciality name. Returns `404` if no matches are found. |

---

### Patients

| Method | Endpoint | Description |
|--------|-----------|--------------|
| **GET** | `/api/patients` | Retrieves a list of all patients stored in the system. Only non-sensitive PII (first name, last name, email, birthdate) is returned. |
| **GET** | `/api/patients/{id}` | Retrieves patient details by their unique ID. |
| **GET** | `/api/patients/email/{email}` | Retrieves patient details using their email address. Useful for checking existing patients before booking an appointment. |
| **POST** | `/api/patients` | Creates a new patient record. The request body must include first name, last name, and email. Email addresses are validated for uniqueness. |
| **PUT** | `/api/patients/{id}` | Updates patient details. Returns `400` if ID mismatch occurs. |
| **DELETE** | `/api/patients/{id}` | Deletes a patient. Deletion is blocked if the patient has existing appointments in the system. |

---

### Appointments

| Method | Endpoint | Description |
|--------|-----------|--------------|
| **GET** | `/api/appointments` | Retrieves all appointments from the database, including linked patient, doctor, clinic, and category data. |
| **GET** | `/api/appointments/{id}` | Retrieves details of a specific appointment by ID, including associated patient and doctor info. |
| **POST** | `/api/appointments` | Creates a new appointment. Requires valid `patientId`, `doctorId`, `clinicId`, `categoryId`, `startUtc`, and `durationMinutes`. Validation ensures no clashing appointments exist for the same patient at the same clinic and time. |
| **PUT** | `/api/appointments/{id}` | Updates an existing appointment’s time or duration. Returns `400` for ID mismatch. |
| **DELETE** | `/api/appointments/{id}` | Deletes an appointment by ID. Returns `404` if not found. |

---

### Categories

| Method | Endpoint | Description |
|--------|-----------|--------------|
| **GET** | `/api/categories` | Retrieves a list of all appointment categories (e.g., Consultation, Check-up, Follow-up). |
| **GET** | `/api/categories/{id}` | Retrieves a single category by ID. |
| **POST** | `/api/categories` | Creates a new category. The name must be unique to avoid duplication. |
| **PUT** | `/api/categories/{id}` | Updates a category name or other details. Returns `400` for ID mismatch. |
| **DELETE** | `/api/categories/{id}` | Deletes a category. Deletion is blocked if the category is currently used by any existing appointments. |

---

### Specialities

| Method | Endpoint | Description |
|--------|-----------|--------------|
| **GET** | `/api/specialities` | Retrieves all medical specialities available in the system. |
| **GET** | `/api/specialities/{id}` | Retrieves a single speciality by ID. |
| **POST** | `/api/specialities` | Creates a new medical speciality (e.g., Pediatrics, Cardiology). The name must be unique. |
| **PUT** | `/api/specialities/{id}` | Updates a speciality’s name. Returns `400` for ID mismatch. |
| **DELETE** | `/api/specialities/{id}` | Deletes a speciality if no doctors are assigned to it. |

---

###  Search (Custom Endpoint)

| Method | Endpoint | Description |
|--------|-----------|--------------|
| **GET** | `/api/doctors/search?q={term}` | Searches for doctors by first or last name. Partial matches are allowed. Returns JSON objects with the doctor’s full name, associated clinic, and speciality. Returns `404` if no results are found. |

---

###  Validation and Response Codes
| Code | Meaning |
|------|----------|
| **200 OK** | Request successful |
| **201 Created** | New resource successfully created |
| **204 No Content** | Successful update or delete with no return body |
| **400 Bad Request** | Invalid data or ID mismatch |
| **404 Not Found** | Resource not found |
| **409 Conflict** | Duplicate record or deletion blocked due to dependencies |

---

 **All endpoints return JSON responses**, follow RESTful conventions, and are documented in Swagger (`/doc`) with sample request and response bodies.

---

## Technologies Used

### **Backend**
- ASP.NET Core 9.0 (REST API)
- Entity Framework Core (MySQL provider)
- Swagger / Swashbuckle for API documentation
- CORS configuration for local React frontend
- Data annotations and LINQ validation

### **Database**
- MySQL 8  
- Code-First development using Entity Framework  
- Fully normalized design (3rd Normal Form)  
- Relationships:
  - A Clinic can have many Doctors  
  - Each Doctor belongs to one Clinic and one Speciality  
  - A Patient can have many Appointments  
  - Each Appointment belongs to one Category  

### **Frontend**
- Next.js 14 (React + TypeScript)
- Tailwind CSS for responsive design and transitions
- Toast notifications for UX feedback
- Loading overlays for network requests
- Client-side form validation

---


## ⚙️ Validation Rules

- **Duplicate Prevention**  
  - No two doctors can share the same *FirstName + LastName + ClinicId + SpecialityId* combination.  
  - Category names must be unique.  

- **Dependency Check Before Deletion**  
  - Clinics, doctors, and categories cannot be deleted if linked to existing appointments.  

- **Appointment Conflict Validation**  
  - A patient cannot book overlapping appointments at the same clinic.  

- **PII Compliance**  
  - Only non-sensitive information (First name, Last name, Email, Birthdate) is stored for patients.  

---

## 🚀 How to Run the Project

### **Backend (ASP.NET Core)**
1. Navigate to the `Backend` folder in your terminal.
2. Update the database:
```bash
dotnet ef database update
```
3. Run the api:
```bash
dotnet run
```
4. Swagger available at:
```bash
http://localhost:5202/doc
```

### **Frontend (Next.js)**
1. Navigate to the `frontend` folder in your terminal.
2. Install dependencies:
```bash
npm install
```
3. Add environment variable in .env.local:
```bash
NEXT_PUBLIC_API_BASE=http://localhost:5202
```
4. Run development server:
```bash
npm run dev
```
5. Open in browser:
```bash
http://localhost:3000
```

### REFERENCES
- Microsoft Learn: ASP.NET Core Security & CORS
- Microsoft Docs: Entity Framework Core
- Next.js 14 Documentation
- Tailwind CSS Documentation
- Noroff school slides and backend course examples (Modules: REST API, EF Core, DTOs)
- ChatGPT (OpenAI) — used for code review, debugging, and documentation structuring
