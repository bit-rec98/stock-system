import { useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Button,
  Card,
  CardContent,
  TextField,
  Typography,
  IconButton,
  Chip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Tooltip,
} from "@mui/material";
import { DataGrid, GridColDef, GridPaginationModel } from "@mui/x-data-grid";
import {
  Add,
  Edit,
  Delete,
  PictureAsPdf,
  TableChart,
  Search,
  FilterList,
  Clear,
} from "@mui/icons-material";
import { toast } from "react-toastify";
import {
  useGetProductsQuery,
  useDeleteProductMutation,
} from "../store/api/productsApi";
import { useGetCategoriesQuery } from "../store/api/categoriesApi";
import { useGetSuppliersQuery } from "../store/api/suppliersApi";
import { ProductFilterParams, Product } from "../types";
import ConfirmDialog from "@/components/ConfirmDialog";

export default function Products() {
  const navigate = useNavigate();
  const [deleteProduct] = useDeleteProductMutation();
  const { data: categories } = useGetCategoriesQuery();
  const { data: suppliers } = useGetSuppliersQuery();

  const [filters, setFilters] = useState<ProductFilterParams>({
    pageNumber: 1,
    pageSize: 10,
    searchTerm: "",
    categoryId: undefined,
    supplierId: undefined,
    isLowStock: undefined,
    sortBy: "createdAt",
    sortDescending: true,
  });

  const [showFilters, setShowFilters] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [productToDelete, setProductToDelete] = useState<Product | null>(null);

  const { data, isLoading, isFetching } = useGetProductsQuery(filters);

  const handlePaginationChange = (model: GridPaginationModel) => {
    setFilters((prev) => ({
      ...prev,
      pageNumber: model.page + 1,
      pageSize: model.pageSize,
    }));
  };

  const handleSearch = useCallback((value: string) => {
    setFilters((prev) => ({ ...prev, searchTerm: value, pageNumber: 1 }));
  }, []);

  const handleClearFilters = () => {
    setFilters({
      pageNumber: 1,
      pageSize: 10,
      searchTerm: "",
      categoryId: undefined,
      supplierId: undefined,
      isLowStock: undefined,
      sortBy: "createdAt",
      sortDescending: true,
    });
  };

  const handleDeleteClick = (product: Product) => {
    setProductToDelete(product);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (productToDelete) {
      try {
        await deleteProduct(productToDelete.id).unwrap();
        toast.success("Product deleted successfully");
      } catch {
        toast.error("Failed to delete product");
      }
    }
    setDeleteDialogOpen(false);
    setProductToDelete(null);
  };

  const handleExportPdf = async () => {
    const token = localStorage.getItem("token");
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        params.append(key, String(value));
      }
    });

    try {
      const response = await fetch(`/api/products/export/pdf?${params}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `inventory-report-${new Date().toISOString().split("T")[0]}.pdf`;
      a.click();
      toast.success("PDF exported successfully");
    } catch {
      toast.error("Failed to export PDF");
    }
  };

  const handleExportExcel = async () => {
    const token = localStorage.getItem("token");
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        params.append(key, String(value));
      }
    });

    try {
      const response = await fetch(`/api/products/export/excel?${params}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `inventory-report-${new Date().toISOString().split("T")[0]}.xlsx`;
      a.click();
      toast.success("Excel exported successfully");
    } catch {
      toast.error("Failed to export Excel");
    }
  };

  const columns: GridColDef[] = [
    { field: "sku", headerName: "SKU", width: 120 },
    { field: "name", headerName: "Product Name", flex: 1, minWidth: 200 },
    { field: "categoryName", headerName: "Category", width: 150 },
    {
      field: "price",
      headerName: "Price",
      width: 100,
      renderCell: (params) => `$${params.value.toFixed(2)}`,
    },
    {
      field: "quantity",
      headerName: "Qty",
      width: 80,
      renderCell: (params) => {
        const isLow = params.row.isLowStock;
        return (
          <Chip
            label={params.value}
            size="small"
            color={isLow ? "error" : "success"}
            variant={isLow ? "filled" : "outlined"}
          />
        );
      },
    },
    {
      field: "isActive",
      headerName: "Status",
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.value ? "Active" : "Inactive"}
          size="small"
          color={params.value ? "success" : "default"}
        />
      ),
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 120,
      sortable: false,
      renderCell: (params) => (
        <Box>
          <Tooltip title="Edit">
            <IconButton
              size="small"
              onClick={() => navigate(`/products/${params.row.id}/edit`)}
            >
              <Edit fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton
              size="small"
              color="error"
              onClick={() => handleDeleteClick(params.row)}
            >
              <Delete fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 3,
        }}
      >
        <Typography variant="h4">Products</Typography>
        <Box sx={{ display: "flex", gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<PictureAsPdf />}
            onClick={handleExportPdf}
          >
            PDF
          </Button>
          <Button
            variant="outlined"
            startIcon={<TableChart />}
            onClick={handleExportExcel}
          >
            Excel
          </Button>
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate("/products/new")}
          >
            Add Product
          </Button>
        </Box>
      </Box>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Box
            sx={{
              display: "flex",
              gap: 2,
              alignItems: "center",
              mb: showFilters ? 2 : 0,
            }}
          >
            <TextField
              placeholder="Search products..."
              size="small"
              value={filters.searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              InputProps={{
                startAdornment: (
                  <Search sx={{ color: "text.secondary", mr: 1 }} />
                ),
              }}
              sx={{ minWidth: 300 }}
            />
            <Button
              variant="outlined"
              startIcon={<FilterList />}
              onClick={() => setShowFilters(!showFilters)}
            >
              Filters
            </Button>
            {(filters.categoryId ||
              filters.supplierId ||
              filters.isLowStock) && (
              <Button
                variant="text"
                startIcon={<Clear />}
                onClick={handleClearFilters}
              >
                Clear
              </Button>
            )}
          </Box>

          {showFilters && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} sm={4}>
                <FormControl fullWidth size="small">
                  <InputLabel>Category</InputLabel>
                  <Select
                    value={filters.categoryId || ""}
                    label="Category"
                    onChange={(e) =>
                      setFilters((prev) => ({
                        ...prev,
                        categoryId: (e.target.value as number) || undefined,
                        pageNumber: 1,
                      }))
                    }
                  >
                    <MenuItem value="">All Categories</MenuItem>
                    {categories?.map((cat) => (
                      <MenuItem key={cat.id} value={cat.id}>
                        {cat.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={4}>
                <FormControl fullWidth size="small">
                  <InputLabel>Supplier</InputLabel>
                  <Select
                    value={filters.supplierId || ""}
                    label="Supplier"
                    onChange={(e) =>
                      setFilters((prev) => ({
                        ...prev,
                        supplierId: (e.target.value as number) || undefined,
                        pageNumber: 1,
                      }))
                    }
                  >
                    <MenuItem value="">All Suppliers</MenuItem>
                    {suppliers?.map((sup) => (
                      <MenuItem key={sup.id} value={sup.id}>
                        {sup.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={4}>
                <FormControl fullWidth size="small">
                  <InputLabel>Stock Status</InputLabel>
                  <Select
                    value={
                      filters.isLowStock === undefined
                        ? ""
                        : filters.isLowStock.toString()
                    }
                    label="Stock Status"
                    onChange={(e) =>
                      setFilters((prev) => ({
                        ...prev,
                        isLowStock:
                          e.target.value === ""
                            ? undefined
                            : e.target.value === "true",
                        pageNumber: 1,
                      }))
                    }
                  >
                    <MenuItem value="">All</MenuItem>
                    <MenuItem value="true">Low Stock</MenuItem>
                    <MenuItem value="false">In Stock</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          )}
        </CardContent>
      </Card>

      <Card>
        <DataGrid
          rows={data?.items || []}
          columns={columns}
          loading={isLoading || isFetching}
          paginationMode="server"
          rowCount={data?.totalCount || 0}
          paginationModel={{
            page: (filters.pageNumber || 1) - 1,
            pageSize: filters.pageSize || 10,
          }}
          onPaginationModelChange={handlePaginationChange}
          pageSizeOptions={[5, 10, 25, 50]}
          disableRowSelectionOnClick
          autoHeight
          sx={{
            "& .MuiDataGrid-cell:focus": {
              outline: "none",
            },
          }}
        />
      </Card>

      <ConfirmDialog
        open={deleteDialogOpen}
        title="Delete Product"
        message={`Are you sure you want to delete "${productToDelete?.name}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteDialogOpen(false)}
      />
    </Box>
  );
}
