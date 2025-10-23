"use client";
import { useMemo, useState } from "react";
import { API } from "@/lib/constants";
import { apiFetch } from "@/lib/api";
import type { Clinic, Category, Doctor } from "@/types";
import LoadingOverlay from "@/components/LoadingOverlay";
import { useToast } from "@/components/toast/ToastProvider";

type Props = {
  clinics: Clinic[];
  categories: Category[];
  doctors: Doctor[];
  specialities: { id: number; name: string }[];
};

function toUtcIso(local: string) {
  const d = new Date(local);
  return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString();
}

export default function AppointmentForm({ clinics, categories, doctors }: Props) {
  const toast = useToast();

  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    birthDate: "",
    clinicId: "",
    doctorId: "",
    categoryId: "",
    startLocal: "",
    durationMinutes: 30,
  });

  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const doctorsForClinic = useMemo(() => {
    const id = Number(form.clinicId);
    return doctors.filter((d) => d.clinicId === id);
  }, [doctors, form.clinicId]);

  async function upsertPatient() {
    const email = form.email.trim().toLowerCase();
    try {
      return await apiFetch(`${API.patientByEmail(email)}`);
    } catch {
      return await apiFetch(API.patients, {
        method: "POST",
        body: JSON.stringify({
          firstName: form.firstName,
          lastName: form.lastName,
          email,
          birthDate: form.birthDate || "1900-01-01",
        }),
      });
    }
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (!form.firstName || !form.lastName || !form.email) {
      const msg = "Please fill in patient first name, last name and email.";
      setError(msg);
      toast.error(msg);
      return;
    }
    if (!form.clinicId || !form.doctorId || !form.categoryId || !form.startLocal) {
      const msg = "Please select clinic, doctor, category and a date/time.";
      setError(msg);
      toast.error(msg);
      return;
    }

    setBusy(true);
    try {
      const patient = await upsertPatient();
      const startUtc = toUtcIso(form.startLocal);

      await apiFetch(API.appointments, {
        method: "POST",
        body: JSON.stringify({
          patientId: patient.id,
          doctorId: Number(form.doctorId),
          clinicId: Number(form.clinicId),
          categoryId: Number(form.categoryId),
          startUtc,
          durationMinutes: Number(form.durationMinutes) || 30,
        }),
      });

      toast.success("Appointment booked successfully!");
      setForm((f) => ({ ...f, startLocal: "" }));
    } catch (e: any) {
      if (e.status === 409) {
        const msg = "Selected time clashes with an existing appointment for this patient at this clinic.";
        setError(msg);
        toast.error(msg);
      } else if (e.status === 400) {
        const msg = typeof e.body === "string" ? e.body : (e.body?.title ?? "Invalid data.");
        setError(msg);
        toast.error(msg);
      } else {
        const msg = e.message ?? "Failed to book appointment.";
        setError(msg);
        toast.error(msg);
      }
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="relative">
      {busy && <LoadingOverlay text="Booking appointment…" />}

      <form onSubmit={onSubmit} className="card p-5 space-y-6">
        <div>
          <h2 className="text-lg font-semibold mb-2">Patient</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="field">
              <label className="label">First name</label>
              <input
                value={form.firstName}
                onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))}
              />
            </div>
            <div className="field">
              <label className="label">Last name</label>
              <input
                value={form.lastName}
                onChange={(e) => setForm((f) => ({ ...f, lastName: e.target.value }))}
              />
            </div>
            <div className="field">
              <label className="label">Email</label>
              <input
                type="email"
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
              />
            </div>
            <div className="field">
              <label className="label">Birthdate</label>
              <input
                type="date"
                value={form.birthDate}
                onChange={(e) => setForm((f) => ({ ...f, birthDate: e.target.value }))}
              />
            </div>
          </div>
        </div>

        <div>
          <h2 className="text-lg font-semibold mb-2">Appointment</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="field">
              <label className="label">Clinic</label>
              <select
                value={form.clinicId}
                onChange={(e) => setForm((f) => ({ ...f, clinicId: e.target.value, doctorId: "" }))}
              >
                <option value="">Select clinic</option>
                {clinics.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>

            <div className="field">
              <label className="label">Doctor</label>
              <select
                value={form.doctorId}
                disabled={!form.clinicId}
                onChange={(e) => setForm((f) => ({ ...f, doctorId: e.target.value }))}
              >
                <option value="">Select doctor</option>
                {doctorsForClinic.map((d) => (
                  <option key={d.id} value={d.id}>{d.firstName} {d.lastName}</option>
                ))}
              </select>
            </div>

            <div className="field">
              <label className="label">Category</label>
              <select
                value={form.categoryId}
                onChange={(e) => setForm((f) => ({ ...f, categoryId: e.target.value }))}
              >
                <option value="">Select category</option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>{cat.name}</option>
                ))}
              </select>
            </div>

            <div className="field">
              <label className="label">Date & time</label>
              <input
                type="datetime-local"
                value={form.startLocal}
                onChange={(e) => setForm((f) => ({ ...f, startLocal: e.target.value }))}
              />
            </div>

            <div className="field">
              <label className="label">Duration (minutes)</label>
              <input
                type="number"
                min={10}
                step={5}
                value={form.durationMinutes}
                onChange={(e) => setForm((f) => ({ ...f, durationMinutes: Number(e.target.value) }))}
              />
            </div>
          </div>
        </div>

        {error && <p className="error" role="alert">{error}</p>}

        <div className="flex gap-3">
          <button className="btn" disabled={busy}>
            {busy ? "Booking…" : "Book"}
          </button>
          <button
            type="button"
            className="btn-outline"
            disabled={busy}
            onClick={() => {
              setError(null);
              setForm({
                firstName: "",
                lastName: "",
                email: "",
                birthDate: "",
                clinicId: "",
                doctorId: "",
                categoryId: "",
                startLocal: "",
                durationMinutes: 30,
              });
              toast.info("Form cleared.");
            }}
          >
            Clear
          </button>
        </div>
      </form>
    </div>
  );
}