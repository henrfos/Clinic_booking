"use client";
import Link from "next/link";
import { usePathname } from "next/navigation";

export default function PageHeader() {
  const pathname = usePathname();
  const isActive = (href: string) =>
    pathname === href || (href === "/book" && pathname === "/");

  return (
    <nav className="flex gap-6">
      <Link href="/book" className={isActive("/book") ? "font-semibold text-blue-700" : "text-gray-700 hover:text-gray-900"}>
        Book
      </Link>
      <Link href="/search" className={isActive("/search") ? "font-semibold text-blue-700" : "text-gray-700 hover:text-gray-900"}>
        Search
      </Link>
    </nav>
  );
}