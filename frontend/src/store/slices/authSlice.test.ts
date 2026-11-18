import { beforeEach, describe, expect, it } from "vitest";
import reducer, { logout, setCredentials } from "./authSlice";

const response = {
  token: "jwt-token",
  email: "staff@example.com",
  firstName: "Sam",
  lastName: "Staff",
  roles: ["Staff"],
  expiration: "2030-01-01T00:00:00Z",
};

describe("authSlice", () => {
  beforeEach(() => localStorage.clear());

  it("stores the session on login", () => {
    const state = reducer(undefined, setCredentials(response));

    expect(state.isAuthenticated).toBe(true);
    expect(state.token).toBe("jwt-token");
    expect(state.user?.roles).toEqual(["Staff"]);
    expect(localStorage.getItem("token")).toBe("jwt-token");
  });

  it("clears the session on logout", () => {
    const loggedIn = reducer(undefined, setCredentials(response));
    const state = reducer(loggedIn, logout());

    expect(state.isAuthenticated).toBe(false);
    expect(state.token).toBeNull();
    expect(localStorage.getItem("token")).toBeNull();
    expect(localStorage.getItem("user")).toBeNull();
  });
});
