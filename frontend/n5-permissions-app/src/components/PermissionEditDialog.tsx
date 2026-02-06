import React from 'react';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Alert,
  Box,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { es } from 'date-fns/locale';
import { type Permission, type PermissionFormData, PERMISSION_TYPES } from '../types/permission';
import { permissionService } from '../services/permissionService';

// Schema dinámico según modo (crear vs editar)
const getSchema = (isEditMode: boolean) =>
  yup.object({
    nombreEmpleado: yup
      .string()
      .required('Nombre es requerido')
      .min(2, 'Mínimo 2 caracteres'),
    apellidoEmpleado: yup
      .string()
      .required('Apellido es requerido')
      .min(2, 'Mínimo 2 caracteres'),
    tipoPermiso: yup
      .number()
      .required('Tipo de permiso es requerido')
      .positive('Debe seleccionar un tipo válido'),
    fechaPermiso: isEditMode
      ? yup.date().required('Fecha es requerida')
      : yup
          .date()
          .required('Fecha es requerida')
          .min(new Date(), 'La fecha debe ser futura'),
  });

interface Props {
  permission?: Permission; // Opcional: undefined = modo crear
  open: boolean;
  onClose: () => void;
}

export const PermissionEditDialog: React.FC<Props> = ({ permission, open, onClose }) => {
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState('');

  const isEditMode = !!permission;
  const dialogTitle = isEditMode
    ? `Editar Permiso #${permission.id}`
    : 'Crear Nuevo Permiso';

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PermissionFormData>({
    resolver: yupResolver(getSchema(isEditMode)),
    defaultValues: permission
      ? {
          nombreEmpleado: permission.nombreEmpleado,
          apellidoEmpleado: permission.apellidoEmpleado,
          tipoPermiso: permission.tipoPermiso,
          fechaPermiso: new Date(permission.fechaPermiso),
        }
      : {
          nombreEmpleado: '',
          apellidoEmpleado: '',
          tipoPermiso: 0,
          fechaPermiso: new Date(),
        },
  });

  React.useEffect(() => {
    if (open) {
      reset(
        permission
          ? {
              nombreEmpleado: permission.nombreEmpleado,
              apellidoEmpleado: permission.apellidoEmpleado,
              tipoPermiso: permission.tipoPermiso,
              fechaPermiso: new Date(permission.fechaPermiso),
            }
          : {
              nombreEmpleado: '',
              apellidoEmpleado: '',
              tipoPermiso: 0,
              fechaPermiso: new Date(),
            }
      );
      setError('');
    }
  }, [open, permission, reset]);

  const onSubmit = async (data: PermissionFormData) => {
    try {
      setLoading(true);
      setError('');

      if (isEditMode) {
        await permissionService.modifyPermission(permission.id, data);
      } else {
        await permissionService.requestPermission(data);
      }

      onClose();
    } catch (err) {
      setError(isEditMode ? 'Error al actualizar el permiso' : 'Error al crear el permiso');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{dialogTitle}</DialogTitle>

      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" noValidate>
          <Controller
            name="nombreEmpleado"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Nombre del Empleado"
                fullWidth
                margin="normal"
                error={!!errors.nombreEmpleado}
                helperText={errors.nombreEmpleado?.message}
              />
            )}
          />

          <Controller
            name="apellidoEmpleado"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Apellido del Empleado"
                fullWidth
                margin="normal"
                error={!!errors.apellidoEmpleado}
                helperText={errors.apellidoEmpleado?.message}
              />
            )}
          />

          <Controller
            name="tipoPermiso"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Tipo de Permiso"
                fullWidth
                margin="normal"
                error={!!errors.tipoPermiso}
                helperText={errors.tipoPermiso?.message}
              >
                {!isEditMode && (
                  <MenuItem value={0} disabled>
                    Seleccione un tipo
                  </MenuItem>
                )}
                {PERMISSION_TYPES.map((type) => (
                  <MenuItem key={type.id} value={type.id}>
                    {type.descripcion}
                  </MenuItem>
                ))}
              </TextField>
            )}
          />

          <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={es}>
            <Controller
              name="fechaPermiso"
              control={control}
              render={({ field }) => (
                <DatePicker
                  {...field}
                  label="Fecha del Permiso"
                  slotProps={{
                    textField: {
                      fullWidth: true,
                      margin: 'normal',
                      error: !!errors.fechaPermiso,
                      helperText: errors.fechaPermiso?.message,
                    },
                  }}
                />
              )}
            />
          </LocalizationProvider>
        </Box>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          Cancelar
        </Button>
        <Button
          onClick={handleSubmit(onSubmit)}
          variant="contained"
          disabled={loading}
        >
          {loading
            ? (isEditMode ? 'Guardando...' : 'Creando...')
            : (isEditMode ? 'Guardar Cambios' : 'Crear Permiso')}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
