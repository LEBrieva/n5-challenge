import React from 'react';
import { Container, Box, Typography, Button } from '@mui/material';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import AddIcon from '@mui/icons-material/Add';
import { theme } from './theme/theme';
import { FilterPanel } from './components/FilterPanel';
import { PermissionList } from './components/PermissionList';
import { PermissionEditDialog } from './components/PermissionEditDialog';
import type { FilterValues } from './types/permission';

function App() {
  const [refreshKey, setRefreshKey] = React.useState(0);
  const [filters, setFilters] = React.useState<FilterValues>({
    searchText: '',
    tipoPermiso: null,
    fechaDesde: null,
    fechaHasta: null,
  });
  const [isCreating, setIsCreating] = React.useState(false);

  const handleFilterChange = (newFilters: FilterValues) => {
    setFilters(newFilters);
  };

  const handleCreateClick = () => {
    setIsCreating(true);
  };

  const handleDialogClose = () => {
    setIsCreating(false);
    setRefreshKey((prev) => prev + 1); // Recarga lista
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Container maxWidth="lg">
        <Box sx={{ py: 4 }}>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center',
              gap: 1.5,
              mb: 3,
              flexWrap: 'wrap',
            }}
          >
            <img
              src="/images/logo.png"
              alt="N5"
              style={{
                height: '70px',
                width: 'auto',
                verticalAlign: 'middle',
              }}
            />
            <Typography variant="h3" component="h1">
              Challenge - Gestión de Permisos
            </Typography>
          </Box>

          <Box
            sx={{
              mt: 2,
              display: 'flex',
              gap: 4,
              flexDirection: { xs: 'column', md: 'row' },
            }}
          >
            {/* Columna izquierda: Filtros */}
            <Box sx={{ flex: { xs: '1 1 100%', md: '0 0 33%' } }}>
              <FilterPanel onFilterChange={handleFilterChange} />
            </Box>

            {/* Columna derecha: Lista con botón crear */}
            <Box sx={{ flex: { xs: '1 1 100%', md: '1 1 67%' } }}>
              {/* Header de la tabla con botón crear */}
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  mb: 2,
                }}
              >
                <Typography variant="h5">Lista de Permisos</Typography>
                <Button
                  variant="contained"
                  color="primary"
                  startIcon={<AddIcon />}
                  onClick={handleCreateClick}
                >
                  Crear Permiso
                </Button>
              </Box>

              {/* La tabla */}
              <PermissionList key={refreshKey} filters={filters} />
            </Box>
          </Box>
        </Box>

        {/* Modal de crear */}
        <PermissionEditDialog open={isCreating} onClose={handleDialogClose} />
      </Container>
    </ThemeProvider>
  );
}

export default App;
