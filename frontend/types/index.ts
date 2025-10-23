export type Id = number;

export interface Clinic   { id: Id; name: string; address?: string | null }
export interface Speciality { id: Id; name: string }
export interface Category { id: Id; name: string }
export interface Doctor {
  id: Id; firstName: string; lastName: string; clinicId: Id; specialityId: Id;
}

export interface PatientRead {
  id: Id; firstName: string; lastName: string; email: string; birthDate: string;
}

export interface DoctorSearchResult {
  doctorFullName: string;
  clinicName: string;
  specialityName: string;
}

export interface AppointmentCreate {
  patientId: Id;
  doctorId: Id;
  clinicId: Id;
  categoryId: Id;
  startUtc: string;
  durationMinutes: number;
}