import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { RoleRoute } from '@/components/RoleRoute';
import { MainLayout } from '@/layouts/MainLayout';
import { LoginPage } from '@/pages/LoginPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { DeviceListPage } from '@/pages/devices/DeviceListPage';
import { DeviceDetailPage } from '@/pages/devices/DeviceDetailPage';
import { DeviceFormPage } from '@/pages/devices/DeviceFormPage';
import { EventListPage } from '@/pages/events/EventListPage';
import { UsersPage } from '@/pages/admin/UsersPage';
import { MasterDataPage } from '@/pages/admin/MasterDataPage';
import { SystemLogsPage } from '@/pages/admin/SystemLogsPage';
import { PersonnelPage } from '@/pages/personnel/PersonnelPage';
import { UsageLogsPage } from '@/pages/usage/UsageLogsPage';
import { OrganizationsPage } from '@/pages/org/OrganizationsPage';
import { BiddingPage } from '@/pages/bidding/BiddingPage';
import { ReportsPage } from '@/pages/reports/ReportsPage';
import { KiemkeAssetPage } from '@/pages/reports/KiemkeAssetPage';
import { LegalDocumentsPage } from '@/pages/legal/LegalDocumentsPage';
import { WarehousePage } from '@/pages/warehouse/WarehousePage';
import { SignDocumentPage } from '@/pages/sign/SignDocumentPage';
import { ForgotPasswordPage } from '@/pages/auth/ForgotPasswordPage';
import { ResetPasswordPage } from '@/pages/auth/ResetPasswordPage';
import { ProfilePage } from '@/pages/profile/ProfilePage';
import { NotificationsPage } from '@/pages/notifications/NotificationsPage';
import { QrScanPage } from '@/pages/scan/QrScanPage';

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/devices" element={<DeviceListPage />} />
            <Route path="/devices/new" element={<DeviceFormPage />} />
            <Route path="/devices/:id" element={<DeviceDetailPage />} />
            <Route path="/devices/:id/edit" element={<DeviceFormPage />} />
            <Route path="/events" element={<EventListPage />} />
            <Route path="/usage-logs" element={<UsageLogsPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/scan-qr" element={<QrScanPage />} />
            {/* baocaoTS — mọi role (kể cả User / LoaiTK 3) */}
            <Route path="/asset-inventory" element={<KiemkeAssetPage />} />

            {/* baocao — LoaiTK 0,1,2 */}
            <Route element={<RoleRoute access="reports" />}>
              <Route path="/reports" element={<ReportsPage />} />
            </Route>

            {/* goithau — LoaiTK 0,1 */}
            <Route element={<RoleRoute access="bidding" />}>
              <Route path="/bidding" element={<BiddingPage />} />
            </Route>

            {/* Kho — LoaiTK 0 hoặc 3 */}
            <Route element={<RoleRoute access="warehouse" />}>
              <Route path="/warehouse" element={<WarehousePage />} />
            </Route>

            {/* Nhân sự / hồ sơ / ký số — Ban lãnh đạo + Tổ kho */}
            <Route element={<RoleRoute access="operations" />}>
              <Route path="/personnel" element={<PersonnelPage />} />
              <Route path="/legal-documents" element={<LegalDocumentsPage />} />
              <Route path="/sign-document" element={<SignDocumentPage />} />
            </Route>

            <Route element={<RoleRoute access="admin" />}>
              <Route path="/users" element={<UsersPage />} />
              <Route path="/organizations" element={<OrganizationsPage />} />
              <Route path="/admin/master" element={<MasterDataPage />} />
              <Route path="/admin/system-logs" element={<SystemLogsPage />} />
            </Route>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
