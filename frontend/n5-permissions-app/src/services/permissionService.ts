import axios from 'axios';
import type { Permission, PermissionFormData } from '../types/permission';

const API_BASE_URL = 'http://localhost:5057/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para manejo de errores
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error);

    // Error de red (API no disponible)
    if (error.code === 'ERR_NETWORK' || !error.response) {
      const networkError = new Error(
        'No se puede conectar con el servidor. Verifica que la API esté corriendo.'
      );
      return Promise.reject(networkError);
    }

    // Errores HTTP
    if (error.response) {
      const statusCode = error.response.status;
      const message = error.response.data?.message || error.response.data || 'Error del servidor';

      const httpError = new Error(`Error ${statusCode}: ${message}`);
      return Promise.reject(httpError);
    }

    return Promise.reject(error);
  }
);

export const permissionService = {
  getPermissions: async (): Promise<Permission[]> => {
    const response = await api.get<Permission[]>('/permissions');
    return response.data;
  },

  requestPermission: async (data: PermissionFormData): Promise<{ id: number }> => {
    const payload = {
      nombreEmpleado: data.nombreEmpleado,
      apellidoEmpleado: data.apellidoEmpleado,
      tipoPermiso: data.tipoPermiso,
      fechaPermiso: data.fechaPermiso.toISOString(),
    };
    const response = await api.post<{ id: number }>('/permissions', payload);
    return response.data;
  },

  modifyPermission: async (id: number, data: PermissionFormData): Promise<void> => {
    const payload = {
      nombreEmpleado: data.nombreEmpleado,
      apellidoEmpleado: data.apellidoEmpleado,
      tipoPermiso: data.tipoPermiso,
      fechaPermiso: data.fechaPermiso.toISOString(),
    };
    await api.put(`/permissions/${id}`, payload);
  },
};
