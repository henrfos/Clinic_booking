import DoctorSearch from "./_components/DoctorSearch";

export default function SearchPage() {
  return (
    <main className="space-y-4">
      <h1 className="text-2xl font-semibold">Search Doctors</h1>
      <DoctorSearch />
    </main>
  );
}