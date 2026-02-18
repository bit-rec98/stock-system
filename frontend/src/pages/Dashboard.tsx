import {
  Box,
  Card,
  CardContent,
  Grid,
  Typography,
  CircularProgress,
} from "@mui/material";
import {
  Inventory,
  Category,
  LocalShipping,
  Warning,
  AttachMoney,
} from "@mui/icons-material";
import {
  useGetProductsQuery,
  useGetLowStockProductsQuery,
} from "../store/api/productsApi";
import { useGetCategoriesQuery } from "../store/api/categoriesApi";
import { useGetSuppliersQuery } from "../store/api/suppliersApi";

interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
  loading?: boolean;
}

function StatCard({ title, value, icon, color, loading }: StatCardProps) {
  return (
    <Card>
      <CardContent>
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Box>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h4" fontWeight="bold">
              {loading ? <CircularProgress size={24} /> : value}
            </Typography>
          </Box>
          <Box
            sx={{
              backgroundColor: `${color}20`,
              borderRadius: "50%",
              p: 1.5,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            {icon}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
}

export default function Dashboard() {
  const { data: productsData, isLoading: productsLoading } =
    useGetProductsQuery({
      pageNumber: 1,
      pageSize: 1,
    });
  const { data: lowStockProducts, isLoading: lowStockLoading } =
    useGetLowStockProductsQuery();
  const { data: categories, isLoading: categoriesLoading } =
    useGetCategoriesQuery();
  const { data: suppliers, isLoading: suppliersLoading } =
    useGetSuppliersQuery();

  const { data: allProducts } = useGetProductsQuery({
    pageNumber: 1,
    pageSize: 1000,
  });

  const totalInventoryValue =
    allProducts?.items.reduce(
      (sum, product) => sum + product.price * product.quantity,
      0,
    ) || 0;

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Overview of your hardware store inventory
      </Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Total Products"
            value={productsData?.totalCount || 0}
            icon={<Inventory sx={{ fontSize: 32, color: "#1976d2" }} />}
            color="#1976d2"
            loading={productsLoading}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Categories"
            value={categories?.length || 0}
            icon={<Category sx={{ fontSize: 32, color: "#9c27b0" }} />}
            color="#9c27b0"
            loading={categoriesLoading}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Suppliers"
            value={suppliers?.length || 0}
            icon={<LocalShipping sx={{ fontSize: 32, color: "#2e7d32" }} />}
            color="#2e7d32"
            loading={suppliersLoading}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Low Stock Items"
            value={lowStockProducts?.length || 0}
            icon={<Warning sx={{ fontSize: 32, color: "#ed6c02" }} />}
            color="#ed6c02"
            loading={lowStockLoading}
          />
        </Grid>
      </Grid>

      <Grid container spacing={3} sx={{ mt: 2 }}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: "flex", alignItems: "center", mb: 2 }}>
                <AttachMoney sx={{ fontSize: 28, color: "#2e7d32", mr: 1 }} />
                <Typography variant="h6">Inventory Value</Typography>
              </Box>
              <Typography variant="h3" fontWeight="bold" color="success.main">
                $
                {totalInventoryValue.toLocaleString(undefined, {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2,
                })}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                Total value of all products in stock
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Box sx={{ display: "flex", alignItems: "center", mb: 2 }}>
                <Warning sx={{ fontSize: 28, color: "#ed6c02", mr: 1 }} />
                <Typography variant="h6">Low Stock Alert</Typography>
              </Box>
              {lowStockLoading ? (
                <CircularProgress />
              ) : lowStockProducts && lowStockProducts.length > 0 ? (
                <Box sx={{ maxHeight: 200, overflow: "auto" }}>
                  {lowStockProducts.slice(0, 5).map((product) => (
                    <Box
                      key={product.id}
                      sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        py: 1,
                        borderBottom: "1px solid",
                        borderColor: "divider",
                      }}
                    >
                      <Box>
                        <Typography variant="body2" fontWeight="medium">
                          {product.name}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          SKU: {product.sku}
                        </Typography>
                      </Box>
                      <Typography
                        variant="body2"
                        sx={{
                          color: "error.main",
                          fontWeight: "bold",
                        }}
                      >
                        {product.quantity} left
                      </Typography>
                    </Box>
                  ))}
                </Box>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  All products are well stocked!
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
