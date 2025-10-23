import { API } from "@/lib/constants";
import { apiFetch } from "@/lib/api";
import type { Clinic, Category, Doctor, Speciality } from "@/types";
import AppointmentForm from "./_components/AppointmentForm";

export default async function BookPage() {
  const [clinics, categories, doctors, specialities] = await Promise.all([
    apiFetch<Clinic[]>(API.clinics),
    apiFetch<Category[]>(API.categories),
    apiFetch<Doctor[]>(API.doctors),
    apiFetch<Speciality[]>(API.specialities),
  ]);

  return (
    <main className="space-y-4">
      <h1 className="text-2xl font-semibold">Book an Appointment</h1>
      <AppointmentForm
        clinics={clinics}
        categories={categories}
        doctors={doctors}
        specialities={specialities}
      />
    </main>
  );
}