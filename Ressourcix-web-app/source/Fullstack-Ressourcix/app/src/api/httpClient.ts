// Kleiner fetch()-Wrapper mit einheitlicher Fehlerbehandlung für alle API-Clients,
// damit Views/Stores nie direkt fetch() aufrufen müssen.

export class ApiError extends Error {
  constructor (
    public readonly status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

// Backend-Fehlerantworten haben die Form { message: "..." } (siehe Program.cs, Results.BadRequest(new { message = ... })
// u.ä.). Wenn keine oder eine andersartige Antwort kommt (z.B. ein bare 401 ohne Body), bleibt es beim Platzhaltertext.
async function extractErrorMessage (response: Response): Promise<string | null> {
  try {
    const body: unknown = await response.json()
    return body !== null && typeof body === 'object' && 'message' in body && typeof body.message === 'string'
      ? body.message
      : null
  } catch {
    return null
  }
}

async function request<T> (path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })

  if (!response.ok) {
    const message
      = (await extractErrorMessage(response)) ?? `${init?.method ?? 'GET'} ${path} failed with ${response.status}`
    console.error('[API]', init?.method ?? 'GET', path, response.status, message)
    throw new ApiError(response.status, message)
  }

  // Nicht nur 204 prüfen: Minimal-API-Endpunkte, die Results.Ok() ohne Payload zurückgeben,
  // antworten mit 200 und leerem Body - response.json() würde darauf mit einem SyntaxError abbrechen.
  const text = await response.text()
  if (!text) {
    return undefined as T
  }
  return JSON.parse(text) as T
}

export const httpClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) => request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PUT', body: body === undefined ? undefined : JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}
