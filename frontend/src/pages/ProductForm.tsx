import { useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  InputAdornment,
  Switch,
  FormControlLabel,
  CircularProgress,
} from "@mui/material";
import { ArrowBack, Save } from "@mui/icons-material";
import { toast } from "react-toastify";
import {
  useGetProductQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
} from "../store/api/productsApi";
import { useGetCategoriesQuery } from "../store/api/categoriesApi";
import { useGetSuppliersQuery } from "../store/api/suppliersApi";
import { CreateProduct, UpdateProduct } from "../types";

type FormData = CreateProduct & { isActive: boolean };

export default function ProductForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const { data: product, isLoading: productLoading } = useGetProductQuery(
    Number(id),
    { skip: !isEdit },
  );
  const { data: categories } = useGetCategoriesQuery();
  const { data: suppliers } = useGetSuppliersQuery();
  const [createProduct, { isLoading: creating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: updating }] = useUpdateProductMutation();

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    defaultValues: {
      name: "",
      description: "",
      sku: "",
      price: 0,
      quantity: 0,
      categoryId: 0,
      supplierId: null,
      minStockLevel: 10,
      isActive: true,
    },
  });

  useEffect(() => {
    if (product) {
      reset({
        name: product.name,
        description: product.description,
        sku: product.sku,
        price: product.price,
        quantity: product.quantity,
        categoryId: product.categoryId,
        supplierId: product.supplierId,
        minStockLevel: product.minStockLevel,
        isActive: product.isActive,
      });
    }
  }, [product, reset]);

  const onSubmit = async (data: FormData) => {
    try {
      if (isEdit) {
        const updateData: UpdateProduct = {
          name: data.name,
          description: data.description,
          price: data.price,
          quantity: data.quantity,
          categoryId: data.categoryId,
          supplierId: data.supplierId,
          minStockLevel: data.minStockLevel,
          isActive: data.isActive,
        };
        await updateProduct({ id: Number(id), product: updateData }).unwrap();
        toast.success("Product updated successfully");
      } else {
        const createData: CreateProduct = {
          name: data.name,
          description: data.description,
          sku: data.sku,
          price: data.price,
          quantity: data.quantity,
          categoryId: data.categoryId,
          supplierId: data.supplierId,
          minStockLevel: data.minStockLevel,
        };
        await createProduct(createData).unwrap();
        toast.success("Product created successfully");
      }
      navigate("/products");
    } catch (error: unknown) {
      const err = error as { data?: { message?: string } };
      toast.error(err.data?.message || "Failed to save product");
    }
  };

  if (productLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", mt: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: "flex", alignItems: "center", mb: 3 }}>
        <Button
          startIcon={<ArrowBack />}
          onClick={() => navigate("/products")}
          sx={{ mr: 2 }}
        >
          Back
        </Button>
        <Typography variant="h4">
          {isEdit ? "Edit Product" : "Add New Product"}
        </Typography>
      </Box>

      <Card>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Product Name"
                  {...register("name", {
                    required: "Product name is required",
                  })}
                  error={!!errors.name}
                  helperText={errors.name?.message}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="SKU"
                  {...register("sku", {
                    required: "SKU is required",
                    pattern: {
                      value: /^[A-Za-z0-9-]+$/,
                      message:
                        "SKU can only contain letters, numbers, and hyphens",
                    },
                  })}
                  error={!!errors.sku}
                  helperText={errors.sku?.message}
                  disabled={isEdit}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Description"
                  multiline
                  rows={3}
                  {...register("description")}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  label="Price"
                  type="number"
                  inputProps={{ step: "0.01", min: "0" }}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">$</InputAdornment>
                    ),
                  }}
                  {...register("price", {
                    required: "Price is required",
                    min: {
                      value: 0.01,
                      message: "Price must be greater than 0",
                    },
                    valueAsNumber: true,
                  })}
                  error={!!errors.price}
                  helperText={errors.price?.message}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  label="Quantity"
                  type="number"
                  inputProps={{ min: "0" }}
                  {...register("quantity", {
                    required: "Quantity is required",
                    min: { value: 0, message: "Quantity cannot be negative" },
                    valueAsNumber: true,
                  })}
                  error={!!errors.quantity}
                  helperText={errors.quantity?.message}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  label="Min Stock Level"
                  type="number"
                  inputProps={{ min: "0" }}
                  {...register("minStockLevel", {
                    required: "Min stock level is required",
                    min: { value: 0, message: "Cannot be negative" },
                    valueAsNumber: true,
                  })}
                  error={!!errors.minStockLevel}
                  helperText={errors.minStockLevel?.message}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <Controller
                  name="categoryId"
                  control={control}
                  rules={{ required: "Category is required" }}
                  render={({ field }) => (
                    <FormControl fullWidth error={!!errors.categoryId}>
                      <InputLabel>Category</InputLabel>
                      <Select {...field} label="Category">
                        {categories?.map((cat) => (
                          <MenuItem key={cat.id} value={cat.id}>
                            {cat.name}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  )}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <Controller
                  name="supplierId"
                  control={control}
                  render={({ field }) => (
                    <FormControl fullWidth>
                      <InputLabel>Supplier</InputLabel>
                      <Select
                        {...field}
                        label="Supplier"
                        value={field.value || ""}
                      >
                        <MenuItem value="">None</MenuItem>
                        {suppliers?.map((sup) => (
                          <MenuItem key={sup.id} value={sup.id}>
                            {sup.name}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  )}
                />
              </Grid>
              {isEdit && (
                <Grid item xs={12}>
                  <Controller
                    name="isActive"
                    control={control}
                    render={({ field }) => (
                      <FormControlLabel
                        control={<Switch {...field} checked={field.value} />}
                        label="Active"
                      />
                    )}
                  />
                </Grid>
              )}
              <Grid item xs={12}>
                <Box
                  sx={{ display: "flex", gap: 2, justifyContent: "flex-end" }}
                >
                  <Button
                    variant="outlined"
                    onClick={() => navigate("/products")}
                  >
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    variant="contained"
                    startIcon={<Save />}
                    disabled={creating || updating}
                  >
                    {creating || updating ? (
                      <CircularProgress size={24} color="inherit" />
                    ) : isEdit ? (
                      "Update Product"
                    ) : (
                      "Create Product"
                    )}
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
