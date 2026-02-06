export interface Permission {
  id: number;
  nombreEmpleado: string;
  apellidoEmpleado: string;
  tipoPermiso: number;
  tipoPermisoDescripcion: string;
  fechaPermiso: string; // ISO date string
}

export interface PermissionFormData {
  nombreEmpleado: string;
  apellidoEmpleado: string;
  tipoPermiso: number;
  fechaPermiso: Date;
}

export interface PermissionType {
  id: number;
  descripcion: string;
}

export interface FilterValues {
  searchText: string;
  tipoPermiso: number | null; // null = todos, número = ese tipo
  fechaDesde: Date | null;
  fechaHasta: Date | null;
}

// Tipos de permiso (provienen del backend)
export const PERMISSION_TYPES: PermissionType[] = [
  { id: 1, descripcion: 'Vacaciones' },
  { id: 2, descripcion: 'Licencia médica' },
  { id: 3, descripcion: 'Permiso personal' },
  { id: 4, descripcion: 'Día libre' },
  { id: 5, descripcion: 'Trabajo remoto' }
];
