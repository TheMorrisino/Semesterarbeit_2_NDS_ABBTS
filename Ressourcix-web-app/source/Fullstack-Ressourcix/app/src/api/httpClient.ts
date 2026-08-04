// Kleiner fetch()-Wrapper mit einheitlicher Fehlerbehandlung für alle API-Clients,
// damit Views/Stores nie direkt fetch() aufrufen müssen.

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });

  if (!response.ok) {
    throw new ApiError(response.status, `${init?.method ?? "GET"} ${path} failed with ${response.status}`);
  }

  // Nicht nur 204 prüfen: Minimal-API-Endpunkte, die Results.Ok() ohne Payload zurückgeben,
  // antworten mit 200 und leerem Body - response.json() würde darauf mit einem SyntaxError abbrechen.
  const text = await response.text();
  if (!text) {
    return undefined as T;
  }
  return JSON.parse(text) as T;
}

export const httpClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) => request<T>(path, { method: "POST", body: JSON.stringify(body) }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PUT", body: body !== undefined ? JSON.stringify(body) : undefined }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};
