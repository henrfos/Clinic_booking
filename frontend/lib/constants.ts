export const API_BASE = process.env.NEXT_PUBLIC_API_BASE ?? "http://localhost:5202";
export const API = {
  clinics: `${API_BASE}/api/clinics`,
  specialities: `${API_BASE}/api/specialities`,
  categories: `${API_BASE}/api/categories`,
  doctors: `${API_BASE}/api/doctors`,
  doctorsSearch: `${API_BASE}/api/doctors/search`,
  patients: `${API_BASE}/api/patients`,
  patientByEmail: (email: string) => `${API_BASE}/api/patients/by-email?email=${encodeURIComponent(email)}`,
  appointments: `${API_BASE}/api/appointments`,
};