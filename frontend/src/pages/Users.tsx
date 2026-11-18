import { Box, Button, Card, CardContent, MenuItem, TextField, Typography } from "@mui/material";
import { Controller, useForm } from "react-hook-form";
import { toast } from "react-toastify";
import { useRegisterMutation } from "../store/api/authApi";
import { RegisterData } from "../types";

const PASSWORD_HINT = "At least 8 characters, with an uppercase letter, a lowercase letter and a number";

export default function Users() {
  const [createUser, { isLoading }] = useRegisterMutation();

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RegisterData>({
    defaultValues: { email: "", password: "", firstName: "", lastName: "", role: "Staff" },
  });

  const onSubmit = async (data: RegisterData) => {
    try {
      const created = await createUser(data).unwrap();
      toast.success(`User ${created.email} created as ${created.role}`);
      reset();
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
      <Typography variant="h4" sx={{ mb: 3 }}>
        Users
      </Typography>

      <Card sx={{ maxWidth: 600 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 2 }}>
            Create user
          </Typography>
          <form onSubmit={handleSubmit(onSubmit)} noValidate>
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
                validate: (v) =>
                  (/[A-Z]/.test(v) && /[a-z]/.test(v) && /[0-9]/.test(v)) || PASSWORD_HINT,
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
            <Button type="submit" variant="contained" disabled={isLoading} sx={{ mt: 2 }}>
              Create user
            </Button>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
