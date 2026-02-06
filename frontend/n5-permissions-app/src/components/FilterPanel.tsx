import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  TextField,
  MenuItem,
  Button,
  Box,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { es } from 'date-fns/locale';
import { PERMISSION_TYPES, type FilterValues } from '../types/permission';

interface FilterPanelProps {
  onFilterChange: (filters: FilterValues) => void;
}

export const FilterPanel: React.FC<FilterPanelProps> = ({ onFilterChange }) => {
  const [searchText, setSearchText] = React.useState('');
  const [tipoPermiso, setTipoPermiso] = React.useState<number | null>(null);
  const [fechaDesde, setFechaDesde] = React.useState<Date | null>(null);
  const [fechaHasta, setFechaHasta] = React.useState<Date | null>(null);

  React.useEffect(() => {
    onFilterChange({
      searchText,
      tipoPermiso,
      fechaDesde,
      fechaHasta,
    });
  }, [searchText, tipoPermiso, fechaDesde, fechaHasta, onFilterChange]);

  const handleClearFilters = () => {
    setSearchText('');
    setTipoPermiso(null);
    setFechaDesde(null);
    setFechaHasta(null);
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h5" gutterBottom>
          Filtros
        </Typography>

        <TextField
          label="Buscar por nombre o apellido"
          fullWidth
          margin="normal"
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          placeholder="Ej: Juan, Pérez..."
        />

        <TextField
          select
          label="Tipo de Permiso"
          fullWidth
          margin="normal"
          value={tipoPermiso ?? ''}
          onChange={(e) => setTipoPermiso(e.target.value ? Number(e.target.value) : null)}
        >
          <MenuItem value="">Todos</MenuItem>
          {PERMISSION_TYPES.map((type) => (
            <MenuItem key={type.id} value={type.id}>
              {type.descripcion}
            </MenuItem>
          ))}
        </TextField>

        <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={es}>
          <DatePicker
            label="Fecha Desde"
            value={fechaDesde}
            onChange={(newValue) => setFechaDesde(newValue)}
            slotProps={{
              textField: {
                fullWidth: true,
                margin: 'normal',
              },
            }}
          />

          <DatePicker
            label="Fecha Hasta"
            value={fechaHasta}
            onChange={(newValue) => setFechaHasta(newValue)}
            slotProps={{
              textField: {
                fullWidth: true,
                margin: 'normal',
              },
            }}
          />
        </LocalizationProvider>

        <Box sx={{ mt: 2 }}>
          <Button
            variant="outlined"
            fullWidth
            onClick={handleClearFilters}
          >
            Limpiar Filtros
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
};
