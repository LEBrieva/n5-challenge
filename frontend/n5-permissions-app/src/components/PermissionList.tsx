import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Box,
  CircularProgress,
  Alert,
  Button,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import { format } from 'date-fns';
import type { Permission, FilterValues } from '../types/permission';
import { permissionService } from '../services/permissionService';
import { PermissionEditDialog } from './PermissionEditDialog';

interface PermissionListProps {
  filters: FilterValues;
}

export const PermissionList: React.FC<PermissionListProps> = ({ filters }) => {
  const [permissions, setPermissions] = React.useState<Permission[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string>('');
  const [editingPermission, setEditingPermission] = React.useState<Permission | null>(null);

  const fetchPermissions = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await permissionService.getPermissions();
      setPermissions(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar los permisos';
      setError(errorMessage);
      console.error('Error fetching permissions:', err);
    } finally {
      setLoading(false);
    }
  };

  React.useEffect(() => {
    fetchPermissions();
  }, []);

  const handleEditClick = (permission: Permission) => {
    setEditingPermission(permission);
  };

  const handleEditClose = () => {
    setEditingPermission(null);
    fetchPermissions();
  };

  const filteredPermissions = React.useMemo(() => {
    return permissions.filter((p) => {
      if (filters.searchText) {
        const search = filters.searchText.toLowerCase();
        const matchName = p.nombreEmpleado.toLowerCase().includes(search);
        const matchLastName = p.apellidoEmpleado.toLowerCase().includes(search);
        if (!matchName && !matchLastName) return false;
      }

      if (filters.tipoPermiso && p.tipoPermiso !== filters.tipoPermiso) {
        return false;
      }

      if (filters.fechaDesde) {
        const permDate = new Date(p.fechaPermiso);
        if (permDate < filters.fechaDesde) return false;
      }

      if (filters.fechaHasta) {
        const permDate = new Date(p.fechaPermiso);
        if (permDate > filters.fechaHasta) return false;
      }

      return true;
    });
  }, [permissions, filters]);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert
        severity="error"
        action={
          <Button color="inherit" size="small" onClick={fetchPermissions}>
            Reintentar
          </Button>
        }
      >
        {error}
      </Alert>
    );
  }

  return (
    <>
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Nombre</TableCell>
              <TableCell>Apellido</TableCell>
              <TableCell>Tipo de Permiso</TableCell>
              <TableCell>Fecha</TableCell>
              <TableCell>Acciones</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredPermissions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  {permissions.length === 0
                    ? 'No hay permisos registrados'
                    : 'No se encontraron permisos con los filtros aplicados'}
                </TableCell>
              </TableRow>
            ) : (
              filteredPermissions.map((permission) => (
                <TableRow key={permission.id}>
                  <TableCell>{permission.id}</TableCell>
                  <TableCell>{permission.nombreEmpleado}</TableCell>
                  <TableCell>{permission.apellidoEmpleado}</TableCell>
                  <TableCell>{permission.tipoPermisoDescripcion}</TableCell>
                  <TableCell>
                    {format(new Date(permission.fechaPermiso), 'dd/MM/yyyy')}
                  </TableCell>
                  <TableCell>
                    <IconButton
                      color="primary"
                      onClick={() => handleEditClick(permission)}
                    >
                      <EditIcon />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {editingPermission && (
        <PermissionEditDialog
          permission={editingPermission}
          open={!!editingPermission}
          onClose={handleEditClose}
        />
      )}
    </>
  );
};
