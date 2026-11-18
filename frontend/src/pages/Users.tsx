import { useEffect, useState } from "react";
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  InputAdornment,
  MenuItem,
  TextField,
  Typography,
} from "@mui/material";
import { Add, Search } from "@mui/icons-material";
import { DataGrid, GridColDef, GridPaginationModel } from "@mui/x-data-grid";
import { Controller, useForm } from "react-hook-form";
import { toast } from "react-toastify";
import { useRegisterMutation } from "../store/api/authApi";
import { useGetUsersQuery } from "../store/api/usersApi";
import { RegisterData, UserListItem } from "../types";

const PASSWORD_HINT = "At least 8 characters, with an uppercase letter, a lowercase letter and a number";

const formatDate = (value: string | null) => (value ? new Date(value).toLocaleString() : "Never");

function UserStatus({ user }: { user: UserListItem }) {
  if (!user.isActive) return <Chip label="Disabled" size="small" />;
  if (user.isLockedOut) return <Chip label="Locked" size="small" color="warning" />;
  return <Chip label="Active" size="small" color="success" />;
}

const columns: GridColDef<UserListItem>[] = [
  { field: "email", headerName: "Email", flex: 1, minWidth: 220, sortable: false },
  {
    field: "name",
    headerName: "Name",
    width: 180,
    sortable: false,
    valueGetter: (params) => `${params.row.firstName} ${params.row.lastName}`.trim(),
  },
  {
    field: "role",
    headerName: "Role",
    width: 110,
    sortable: false,
    renderCell: (params) => (
      <Chip
        label={params.value ?? "None"}
        size="small"
        color={params.value === "Admin" ? "primary" : "default"}
        variant="outlined"
      />
    ),
  },
  {
    field: "status",
    headerName: "Status",
    width: 110,
    sortable: false,
    renderCell: (params) => <UserStatus user={params.row} />,
  },
  {
    field: "lastLoginAt",
    headerName: "Last sign-in",
    width: 190,
    sortable: false,
    valueFormatter: (params) => formatDate(params.value as string | null),
  },
  {
    field: "createdAt",
    headerName: "Created",
    width: 190,
    sortable: false,
    valueFormatter: (params) => formatDate(params.value as string),
  },
];

export default function Users() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [searchInput, setSearchInput] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({ page: 0, pageSize: 10 });

  // Wait for a pause in typing before querying, and return to the first page on a new search.
  useEffect(() => {
    const timer = setTimeout(() => {
      setSearchTerm(searchInput.trim());
      setPaginationModel((model) => ({ ...model, page: 0 }));
    }, 400);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const { data, isFetching, isError } = useGetUsersQuery({
    pageNumber: paginationModel.page + 1,
    pageSize: paginationModel.pageSize,
    searchTerm: searchTerm || undefined,
  });

  const [createUser, { isLoading: isCreating }] = useRegisterMutation();

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RegisterData>({
    defaultValues: { email: "", password: "", firstName: "", lastName: "", role: "Staff" },
  });

  const closeDialog = () => {
    setDialogOpen(false);
    reset();
  };

  const onSubmit = async (formData: RegisterData) => {
    try {
      const created = await createUser(formData).unwrap();
      toast.success(`User ${created.email} created as ${created.role}`);
      closeDialog();
    } catch (error: unknown) {
      const err = error as { data?: { message?: string; errors?: string[] | Record<string, string[]> } };
      const details = err.data?.errors;
      const messages = Array.isArray(details) ? details : Object.values(details ?? {}).flat();
      if (messages.length) {
        messages.forEach((m) => toast.error(m));
      } else {
        toast.error(err.data?.message || "Failed to create user");
      }
    }
  };

  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
        <Typography variant="h4">Users</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setDialogOpen(true)}>
          Add User
        </Button>
      </Box>

      <Card>
        <CardContent>
          <TextField
            size="small"
            placeholder="Search by email or name"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            sx={{ mb: 2, width: { xs: "100%", sm: 360 } }}
            inputProps={{ maxLength: 100, "aria-label": "Search users" }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search fontSize="small" />
                </InputAdornment>
              ),
            }}
          />
          {isError && (
            <Typography color="error" sx={{ mb: 2 }}>
              Could not load users.
            </Typography>
          )}
          <DataGrid
            rows={data?.items ?? []}
            columns={columns}
            getRowId={(row) => row.id}
            loading={isFetching}
            disableRowSelectionOnClick
            disableColumnMenu
            autoHeight
            paginationMode="server"
            rowCount={data?.totalCount ?? 0}
            paginationModel={paginationModel}
            onPaginationModelChange={setPaginationModel}
            pageSizeOptions={[5, 10, 25, 50]}
          />
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onClose={closeDialog} maxWidth="sm" fullWidth>
        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <DialogTitle>Add User</DialogTitle>
          <DialogContent>
            <TextField
              fullWidth
              margin="normal"
              label="Email"
              type="email"
              autoComplete="off"
              {...register("email", {
                required: "Email is required",
                pattern: { value: /^\S+@\S+\.\S+$/, message: "Invalid email format" },
              })}
              error={!!errors.email}
              helperText={errors.email?.message}
            />
            <TextField
              fullWidth
              margin="normal"
              label="First name"
              {...register("firstName", { required: "First name is required", maxLength: 100 })}
              error={!!errors.firstName}
              helperText={errors.firstName?.message}
            />
            <TextField
              fullWidth
              margin="normal"
              label="Last name"
              {...register("lastName", { required: "Last name is required", maxLength: 100 })}
              error={!!errors.lastName}
              helperText={errors.lastName?.message}
            />
            <TextField
              fullWidth
              margin="normal"
              label="Initial password"
              type="password"
              autoComplete="new-password"
              {...register("password", {
                required: "Password is required",
                minLength: { value: 8, message: "Password must be at least 8 characters" },
                validate: (v) => (/[A-Z]/.test(v) && /[a-z]/.test(v) && /[0-9]/.test(v)) || PASSWORD_HINT,
              })}
              error={!!errors.password}
              helperText={errors.password?.message ?? PASSWORD_HINT}
            />
            <Controller
              name="role"
              control={control}
              render={({ field }) => (
                <TextField
                  select
                  fullWidth
                  margin="normal"
                  label="Role"
                  {...field}
                  helperText="Admin: full access and user management. Staff: view everything and maintain products, no deletions."
                >
                  <MenuItem value="Staff">Staff</MenuItem>
                  <MenuItem value="Admin">Admin</MenuItem>
                </TextField>
              )}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={closeDialog}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={isCreating}>
              {isCreating ? "Creating..." : "Create user"}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
}
