import type { Metadata } from "next";
import "./globals.css";
import PageHeader from "@/components/PageHeader";
import Providers from "./providers";

export const metadata: Metadata = {
  title: "Clinic Booking",
  description: "Clinic Appointment Booking System",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <Providers>
          <div className="min-h-screen flex flex-col">
            <header className="border-b bg-white">
              <div className="container h-20 flex items-center">
                <PageHeader />
              </div>
            </header>

            <main className="container flex-1 p-4">{children}</main>

            <footer className="border-t bg-white">
              <div className="container p-4 text-sm text-gray-600">
                © {new Date().getFullYear()} ClinicBooking
              </div>
            </footer>
          </div>
        </Providers>
      </body>
    </html>
  );
}