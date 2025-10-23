"use client";
import { useState } from "react";
import { API } from "@/lib/constants";
import { apiFetch } from "@/lib/api";
import type { DoctorSearchResult } from "@/types";
import Spinner from "@/components/Spinner";
import { useToast } from "@/components/toast/ToastProvider";

export default function DoctorSearch() {
  const toast = useToast();

  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState<DoctorSearchResult[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setResults(null);

    const query = q.trim();
    if (!query) {
      const msg = "Enter a first or last name to search.";
      setError(msg);
      toast.info("Type at least one character to search.");
      return;
    }

    setLoading(true);
    try {
      const data = await apiFetch<DoctorSearchResult[]>(
        `${API.doctorsSearch}?q=${encodeURIComponent(query)}`
      );
      setResults(data);
      toast.success(`Found ${data.length} doctor${data.length === 1 ? "" : "s"}.`);
    } catch (e: any) {
      if (e.status === 404) {
        setResults([]);
        toast.info("No doctors found.");
      } else {
        const msg = e.message ?? "Search failed.";
        setError(msg);
        toast.error(msg);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className="card p-5 space-y-4">
      <form onSubmit={onSubmit} className="flex gap-2">
        <input
          className="flex-1"
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder="e.g. Grey"
        />
        <button className="btn">{loading ? "Searching…" : "Search"}</button>
      </form>

      {loading && (
        <div className="flex items-center gap-3 text-gray-600">
          <Spinner /> <span>Loading…</span>
        </div>
      )}

      {error && <p className="error" role="alert">{error}</p>}
      {results && results.length === 0 && <p>No doctors found.</p>}

      {Array.isArray(results) && results.length > 0 && (
        <ul className="grid gap-3 sm:grid-cols-2">
          {results.map((r, i) => (
            <li key={i} className="p-4 border rounded-lg bg-white">
              <div className="font-semibold">{r.doctorFullName}</div>
              <div className="text-sm text-gray-600">
                {r.specialityName} @ {r.clinicName}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}