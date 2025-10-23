import Spinner from "./Spinner";

export default function LoadingOverlay({ text = "Processing…" }: { text?: string }) {
  return (
    <div className="absolute inset-0 z-10 grid place-items-center bg-white/70 backdrop-blur-[1px]">
      <div className="flex items-center gap-3 text-gray-700">
        <Spinner className="h-6 w-6" />
        <span>{text}</span>
      </div>
    </div>
  );
}