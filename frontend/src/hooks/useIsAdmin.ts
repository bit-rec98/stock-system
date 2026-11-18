import { useSelector } from "react-redux";
import { RootState } from "../store";

/**
 * UI convenience only: hides controls the user cannot use.
 * The backend enforces authorization on every request.
 */
export function useIsAdmin(): boolean {
  const roles = useSelector((state: RootState) => state.auth.user?.roles);
  return roles?.includes("Admin") ?? false;
}
