// Base URL of the API.
// - Local development: leave VITE_API_BASE_URL empty; Vite proxies "/api" to the backend.
// - Deployed: set VITE_API_BASE_URL to the API origin (e.g. https://stock-api.example.com).
const origin = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");

export const API_URL = `${origin}/api`;
