import { useState } from "react";
import { Box, Button, Card, CardContent, IconButton, InputAdornment, TextField, Typography } from "@mui/material";
import { Visibility, VisibilityOff } from "@mui/icons-material";
import { useForm } from "react-hook-form";
import { useDispatch, useSelector } from "react-redux";
import { toast } from "react-toastify";
import { useChangeEmailMutation } from "../store/api/authApi";
import { setCredentials } from "../store/slices/authSlice";
import { RootState } from "../store";
import { ChangeEmailData } from "../types";

export default function ChangeEmail() {
  const dispatch = useDispatch();
  const currentEmail = useSelector((state: RootState) => state.auth.user?.email);
  const [changeEmail, { isLoading }] = useChangeEmailMutation();
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ChangeEmailData>({ defaultValues: { newEmail: "", currentPassword: "" } });

  const onSubmit = async (data: ChangeEmailData) => {
    try {
      // The API returns a new session: the token carries the email, so it must be replaced.
      const session = await changeEmail(data).unwrap();
      dispatch(setCredentials(session));
      toast.success("Email changed. Use the new address the next time you sign in.");
      reset();
    } catch (error: unknown) {
      const err = error as { data?: { message?: string; errors?: string[] | Record<string, string[]> } };
      const details = err.data?.errors;
      const messages = Array.isArray(details) ? details : Object.values(details ?? {}).flat();
      if (messages.length) {
        messages.forEach((m) => toast.error(m));
      } else {
        toast.error(err.data?.message || "Failed to change email");
      }
    }
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 3 }}>
        Change Email
      </Typography>

      <Card sx={{ maxWidth: 500 }}>
        <CardContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Your email is also your sign-in name. Current email: <strong>{currentEmail}</strong>
          </Typography>
          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
              <TextField
                label="New Email"
                type="email"
                fullWidth
                autoComplete="off"
                {...register("newEmail", {
                  required: "New email is required",
                  pattern: { value: /^\S+@\S+\.\S+$/, message: "Invalid email format" },
                  maxLength: { value: 256, message: "Email cannot exceed 256 characters" },
                })}
                error={!!errors.newEmail}
                helperText={errors.newEmail?.message}
              />
              <TextField
                label="Current Password"
                type={showPassword ? "text" : "password"}
                fullWidth
                autoComplete="current-password"
                {...register("currentPassword", { required: "Current password is required" })}
                error={!!errors.currentPassword}
                helperText={errors.currentPassword?.message ?? "Required to confirm it is you"}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="Toggle password visibility"
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
              <Button type="submit" variant="contained" disabled={isLoading} sx={{ mt: 1 }}>
                {isLoading ? "Changing..." : "Change Email"}
              </Button>
            </Box>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
