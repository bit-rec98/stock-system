import { API_URL } from "../config";
import { store } from "../store";

/** Downloads an authenticated file from the API (PDF/Excel exports) using the token in the store. */
export async function downloadFile(path: string, filename: string): Promise<void> {
  const token = store.getState().auth.token;
  const response = await fetch(`${API_URL}${path}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });

  if (!response.ok) {
    throw new Error(`Download failed with status ${response.status}`);
  }

  const url = window.URL.createObjectURL(await response.blob());
  try {
    const link = document.createElement("a");
    link.href = url;
    link.download = filename;
    link.click();
  } finally {
    window.URL.revokeObjectURL(url);
  }
}
