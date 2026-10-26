import type { Album, AlbumInput, AlbumListResponse, ValidationProblem } from "./types";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly validationErrors: Record<string, string[]> = {},
  ) {
    super(message);
  }
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: init?.body
      ? { "Content-Type": "application/json", ...init.headers }
      : init?.headers,
  });

  if (!response.ok) {
    let problem: ValidationProblem | undefined;
    if (response.headers.get("content-type")?.includes("application/problem+json")) {
      problem = (await response.json()) as ValidationProblem;
    }
    throw new ApiError(response.status, problem?.title ?? `Request failed (${response.status}).`, problem?.errors);
  }

  return response.status === 204 ? (undefined as T) : ((await response.json()) as T);
}

export function getAlbums(search: string, format: string, signal?: AbortSignal) {
  const query = new URLSearchParams();
  if (search) query.set("search", search);
  if (format) query.set("format", format);
  const suffix = query.size ? `?${query}` : "";
  return request<AlbumListResponse>(`/api/albums${suffix}`, { signal });
}

export function getAlbum(id: number, signal?: AbortSignal) {
  return request<Album>(`/api/albums/${id}`, { signal });
}

export function createAlbum(album: AlbumInput) {
  return request<Album>("/api/albums", { method: "POST", body: JSON.stringify(album) });
}

export function updateAlbum(id: number, album: AlbumInput) {
  return request<Album>(`/api/albums/${id}`, { method: "PUT", body: JSON.stringify(album) });
}

export function deleteAlbum(id: number) {
  return request<void>(`/api/albums/${id}`, { method: "DELETE" });
}
