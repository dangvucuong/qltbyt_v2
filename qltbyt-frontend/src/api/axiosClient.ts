import axios from 'axios';
import { useAuthStore } from '@/store/authStore';
import { getApiOrigin } from '@/utils/apiBase';

const axiosClient = axios.create({
  // Production: VITE_API_BASE_URL=http://27.118.16.152:8016
  // Development: để trống để Vite proxy /api, /files, ...
  baseURL: getApiOrigin(),
  headers: { 'Content-Type': 'application/json' },
});

axiosClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

axiosClient.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  },
);

export default axiosClient;
