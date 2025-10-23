"use client";

import { createContext, useCallback, useContext, useMemo, useRef, useState } from "react";

type ToastVariant = "success" | "error" | "info";

export interface ToastItem {
  id: number;
  message: string;
  variant: ToastVariant;
  duration?: number; 
}

type ToastContextType = {
  show: (message: string, opts?: { variant?: ToastVariant; duration?: number }) => void;
  success: (message: string, opts?: { duration?: number }) => void;
  error: (message: string, opts?: { duration?: number }) => void;
  info: (message: string, opts?: { duration?: number }) => void;
};

const ToastContext = createContext<ToastContextType | null>(null);

export function useToast(): ToastContextType {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within <ToastProvider />");
  return ctx;
}

export default function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const nextId = useRef(1);

  const remove = useCallback((id: number) => {
    setToasts((list) => list.filter((t) => t.id !== id));
  }, []);

  const show = useCallback((message: string, opts?: { variant?: ToastVariant; duration?: number }) => {
    const id = nextId.current++;
    const variant = opts?.variant ?? "info";
    const duration = opts?.duration ?? 3500;

    setToasts((list) => [...list, { id, message, variant, duration }]);

    if (duration > 0) {
      setTimeout(() => remove(id), duration);
    }
  }, [remove]);

  const api = useMemo<ToastContextType>(() => ({
    show,
    success: (message, opts) => show(message, { ...opts, variant: "success" }),
    error: (message, opts) => show(message, { ...opts, variant: "error" }),
    info: (message, opts) => show(message, { ...opts, variant: "info" }),
  }), [show]);

  return (
    <ToastContext.Provider value={api}>
      {children}

      {/* Renderer */}
      <div className="fixed right-4 top-4 z-50 flex w-full max-w-sm flex-col gap-2">
        {toasts.map((t) => (
          <ToastCard key={t.id} toast={t} onClose={() => remove(t.id)} />
        ))}
      </div>
    </ToastContext.Provider>
  );
}

function ToastCard({ toast, onClose }: { toast: ToastItem; onClose: () => void }) {
  const base = "w-full rounded-lg border shadow-sm p-3 text-sm flex items-start gap-3";
  const variantStyles: Record<ToastVariant, string> = {
    success: "bg-emerald-50 border-emerald-200 text-emerald-900",
    error:   "bg-red-50 border-red-200 text-red-900",
    info:    "bg-blue-50 border-blue-200 text-blue-900",
  };

  const iconDot: Record<ToastVariant, string> = {
    success: "bg-emerald-500",
    error:   "bg-red-500",
    info:    "bg-blue-500",
  };

  return (
    <div className={`${base} ${variantStyles[toast.variant]}`} role="status" aria-live="polite">
      <span className={`mt-1 inline-block h-2 w-2 rounded-full ${iconDot[toast.variant]}`} />
      <div className="flex-1">{toast.message}</div>
      <button
        onClick={onClose}
        className="ml-2 rounded-md px-2 py-1 text-xs hover:bg-black/5"
        aria-label="Close"
      >
        ✕
      </button>
    </div>
  );
}