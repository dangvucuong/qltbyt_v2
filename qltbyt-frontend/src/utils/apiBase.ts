/** Origin của API (không có dấu / cuối). Dev để trống → dùng proxy Vite. */
export function getApiOrigin(): string {
  const raw = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim() ?? '';
  return raw.replace(/\/+$/, '');
}

/** Ghép path tuyệt đối tới API, ví dụ /api/devices hoặc /files/1/a.pdf */
export function apiUrl(path: string): string {
  const origin = getApiOrigin();
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return origin ? `${origin}${normalized}` : normalized;
}
