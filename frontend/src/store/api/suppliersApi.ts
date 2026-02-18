import { apiSlice } from "./apiSlice";
import { Supplier, CreateSupplier, UpdateSupplier } from "../../types";

export const suppliersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSuppliers: builder.query<Supplier[], void>({
      query: () => "/suppliers",
      providesTags: (result) =>
        result
          ? [
              ...result.map(({ id }) => ({ type: "Supplier" as const, id })),
              { type: "Supplier", id: "LIST" },
            ]
          : [{ type: "Supplier", id: "LIST" }],
    }),
    getSupplier: builder.query<Supplier, number>({
      query: (id) => `/suppliers/${id}`,
      providesTags: (_result, _error, id) => [{ type: "Supplier", id }],
    }),
    createSupplier: builder.mutation<Supplier, CreateSupplier>({
      query: (supplier) => ({
        url: "/suppliers",
        method: "POST",
        body: supplier,
      }),
      invalidatesTags: [{ type: "Supplier", id: "LIST" }],
    }),
    updateSupplier: builder.mutation<
      Supplier,
      { id: number; supplier: UpdateSupplier }
    >({
      query: ({ id, supplier }) => ({
        url: `/suppliers/${id}`,
        method: "PUT",
        body: supplier,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: "Supplier", id },
        { type: "Supplier", id: "LIST" },
      ],
    }),
    deleteSupplier: builder.mutation<void, number>({
      query: (id) => ({
        url: `/suppliers/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: [{ type: "Supplier", id: "LIST" }],
    }),
  }),
});

export const {
  useGetSuppliersQuery,
  useGetSupplierQuery,
  useCreateSupplierMutation,
  useUpdateSupplierMutation,
  useDeleteSupplierMutation,
} = suppliersApi;
