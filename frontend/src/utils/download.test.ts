import { afterEach, describe, expect, it, vi } from "vitest";
import { store } from "../store";
import { setCredentials } from "../store/slices/authSlice";
import { downloadFile } from "./download";

describe("downloadFile", () => {
  afterEach(() => vi.restoreAllMocks());

  it("sends the bearer token from the store and fails on non-2xx responses", async () => {
    store.dispatch(
      setCredentials({
        token: "abc",
        email: "a@b.co",
        firstName: "A",
        lastName: "B",
        roles: ["Admin"],
        expiration: "2030-01-01T00:00:00Z",
      })
    );
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, status: 403 });
    vi.stubGlobal("fetch", fetchMock);

    await expect(downloadFile("/products/export/pdf", "x.pdf")).rejects.toThrow("403");

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe("/api/products/export/pdf");
    expect((init as RequestInit).headers).toEqual({ Authorization: "Bearer abc" });
  });
});
