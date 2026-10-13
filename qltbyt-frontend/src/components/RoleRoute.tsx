import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import {
  canAccessAdmin,
  canAccessBidding,
  canAccessOperations,
  canAccessReports,
  canAccessWarehouse,
} from '@/utils/permissions';

type AccessKey = 'reports' | 'bidding' | 'warehouse' | 'operations' | 'admin';

const ACCESS_CHECKS: Record<AccessKey, (user: NonNullable<ReturnType<typeof useAuthStore.getState>['user']>) => boolean> = {
  reports: canAccessReports,
  bidding: canAccessBidding,
  warehouse: canAccessWarehouse,
  operations: canAccessOperations,
  admin: canAccessAdmin,
};

type Props = {
  /** Role JWT (Admin / Technician / User) */
  roles?: string[];
  /** Quyền theo LoaiTK legacy */
  access?: AccessKey;
};

export function RoleRoute({ roles, access }: Props) {
  const user = useAuthStore((s) => s.user);
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (access && !ACCESS_CHECKS[access](user)) {
    return <Navigate to="/dashboard" replace />;
  }

  if (roles && roles.length > 0 && !roles.includes(user.role)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
}
