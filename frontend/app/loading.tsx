export default function GlobalLoading() {
  return (
    <div className="container p-8">
      <div className="flex items-center gap-3 text-gray-600">
        <span className="inline-block h-5 w-5 animate-spin rounded-full border-2 border-gray-300 border-t-transparent" />
        <span>Loading…</span>
      </div>
    </div>
  );
}