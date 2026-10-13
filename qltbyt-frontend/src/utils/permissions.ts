import type { UserInfo } from '@/types/api';

/**
 * Phân quyền theo LoaiTK legacy (taikhoan.aspx / main.master):
 * 0 = Ban lãnh đạo, 1 = Tổ kho thiết bị, 2 = Kỹ sư khu vực, 3 = Nhân viên mạng lưới
 */
export function getAccountType(user?: UserInfo | null): number {
  if (user?.accountType == null) {
    if (user?.role === 'Admin') return 0;
    if (user?.role === 'Technician') return 1;
    return 3;
  }
  return Number(user.accountType);
}

/** Kiểm kê tài sản (baocaoTS) — mọi role */
export function canAccessAssetInventory(_user?: UserInfo | null): boolean {
  return true;
}

/** Số liệu tổng hợp /reports (baocao) — LoaiTK 0,1,2; chặn 3 */
export function canAccessReports(user?: UserInfo | null): boolean {
  if (!user) return false;
  if (user.role === 'Admin' || user.role === 'Technician') return true;
  return getAccountType(user) <= 2;
}

/** Gói thầu — LoaiTK 0,1 */
export function canAccessBidding(user?: UserInfo | null): boolean {
  if (!user) return false;
  if (user.role === 'Admin' || user.role === 'Technician') return true;
  return getAccountType(user) <= 1;
}

/** Kho TVT — LoaiTK 0 hoặc 3 (Nhaphanghoa/baocaokho cũ) */
export function canAccessWarehouse(user?: UserInfo | null): boolean {
  if (!user) return false;
  if (user.role === 'Admin') return true;
  const t = getAccountType(user);
  return t === 0 || t === 3;
}

/** Nhân sự / hồ sơ / ký số — Ban lãnh đạo + Tổ kho */
export function canAccessOperations(user?: UserInfo | null): boolean {
  if (!user) return false;
  return user.role === 'Admin' || user.role === 'Technician' || getAccountType(user) <= 1;
}

/** Tài khoản, danh mục, đơn vị, nhật ký hệ thống */
export function canAccessAdmin(user?: UserInfo | null): boolean {
  if (!user) return false;
  return user.role === 'Admin' || getAccountType(user) === 0;
}

export const ACCOUNT_TYPE_OPTIONS = [
  { value: 0, label: 'Ban lãnh đạo' },
  { value: 1, label: 'Tổ kho, thiết bị' },
  { value: 2, label: 'Kỹ sư khu vực' },
  { value: 3, label: 'Nhân viên mạng lưới' },
] as const;
