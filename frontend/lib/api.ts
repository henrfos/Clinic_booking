export async function apiFetch<T>(
  url: string,
  opts: RequestInit & { timeoutMs?: number } = {}
): Promise<T> {
  const { timeoutMs, headers, ...rest } = opts;
  const controller = new AbortController();
  const id = timeoutMs ? setTimeout(() => controller.abort(), timeoutMs) : null;

  const res = await fetch(url, {
    ...rest,
    headers: { "Content-Type": "application/json", ...(headers || {}) },
    signal: controller.signal,
    cache: "no-store", 
  });

  if (id) clearTimeout(id);

  const text = await res.text();
  let body: any = null;
  try { body = text ? JSON.parse(text) : null; } catch { body = text; }

  if (!res.ok) {
    const err: any = new Error(typeof body === "string" ? body : (body?.title ?? res.statusText));
    err.status = res.status;
    err.body = body;
    throw err;
  }
  return body as T;
}