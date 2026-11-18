import { apiSlice } from "./apiSlice";
import { PagedResult, UserFilterParams, UserListItem } from "../../types";

export const usersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getUsers: builder.query<PagedResult<UserListItem>, UserFilterParams>({
      query: (params) => ({ url: "/users", params }),
      providesTags: [{ type: "User", id: "LIST" }],
    }),
  }),
});

export const { useGetUsersQuery } = usersApi;
