import { apiSlice } from "./apiSlice";
import { LoginCredentials, RegisterData, AuthResponse, ChangePasswordData, ChangeEmailData, CreatedUser } from "../../types";

export const authApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, LoginCredentials>({
      query: (credentials) => ({
        url: "/auth/login",
        method: "POST",
        body: credentials,
      }),
    }),
    register: builder.mutation<CreatedUser, RegisterData>({
      query: (data) => ({
        url: "/auth/register",
        method: "POST",
        body: data,
      }),
      invalidatesTags: [{ type: "User", id: "LIST" }],
    }),
    changeEmail: builder.mutation<AuthResponse, ChangeEmailData>({
      query: (data) => ({
        url: "/auth/change-email",
        method: "POST",
        body: data,
      }),
      invalidatesTags: [{ type: "User", id: "LIST" }],
    }),
    changePassword: builder.mutation<{ message: string }, ChangePasswordData>({
      query: (data) => ({
        url: "/auth/change-password",
        method: "POST",
        body: data,
      }),
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useChangeEmailMutation,
  useChangePasswordMutation,
} = authApi;
