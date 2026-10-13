import { Avatar, Button, Layout, Menu, Typography } from 'antd';
import {
  BookOutlined,
  DashboardOutlined,
  LogoutOutlined,
  AlertOutlined,
  MedicineBoxOutlined,
  TeamOutlined,
  UserOutlined,
  UserSwitchOutlined,
  BankOutlined,
  FileTextOutlined,
  ProfileOutlined,
  SettingOutlined,
  SolutionOutlined,
  ShopOutlined,
  FileProtectOutlined,
  QrcodeOutlined,
  AuditOutlined,
  HistoryOutlined,
} from '@ant-design/icons';
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { NotificationBell } from '@/components/NotificationBell';
import {
  canAccessAdmin,
  canAccessBidding,
  canAccessOperations,
  canAccessReports,
  canAccessWarehouse,
} from '@/utils/permissions';

const { Header, Sider, Content } = Layout;

export function MainLayout() {
  const location = useLocation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const showReports = canAccessReports(user);
  const showBidding = canAccessBidding(user);
  const showWarehouse = canAccessWarehouse(user);
  const showOperations = canAccessOperations(user);
  const showAdmin = canAccessAdmin(user);

  const selected = location.pathname.startsWith('/legal-documents')
    ? '/legal-documents'
    : location.pathname.startsWith('/admin/system-logs')
    ? '/admin/system-logs'
    : location.pathname.startsWith('/admin/master')
    ? '/admin/master'
    : location.pathname.startsWith('/organizations')
      ? '/organizations'
      : location.pathname.startsWith('/bidding')
        ? '/bidding'
        : location.pathname.startsWith('/warehouse')
          ? '/warehouse'
          : location.pathname.startsWith('/sign-document')
            ? '/sign-document'
            : location.pathname.startsWith('/asset-inventory')
          ? '/asset-inventory'
          : location.pathname.startsWith('/reports')
          ? '/reports'
          : location.pathname.startsWith('/users')
            ? '/users'
            : location.pathname.startsWith('/personnel')
              ? '/personnel'
              : location.pathname.startsWith('/usage-logs')
                ? '/usage-logs'
                : location.pathname.startsWith('/events')
                  ? '/events'
                  : location.pathname.startsWith('/scan-qr')
                    ? '/scan-qr'
                    : location.pathname.startsWith('/devices')
                    ? '/devices'
                    : location.pathname.startsWith('/dashboard')
                      ? '/dashboard'
                      : location.pathname;

  return (
    <Layout className="app-root-layout" style={{ minHeight: '100vh' }}>
      <Sider
        width={248}
        breakpoint="lg"
        collapsedWidth={0}
        style={{
          background: 'linear-gradient(180deg, #0f172a 0%, #1e293b 100%)',
        }}
      >
        <div className="app-sidebar-brand">
          <div className="app-sidebar-brand-title">QLTBYT</div>
          <div className="app-sidebar-brand-sub">Quản lý thiết bị y tế</div>
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selected]}
          style={{ background: 'transparent', borderInlineEnd: 'none' }}
          items={[
            {
              key: '/dashboard',
              icon: <DashboardOutlined />,
              label: <Link to="/dashboard">Tổng quan</Link>,
            },
            {
              key: '/events',
              icon: <AlertOutlined />,
              label: <Link to="/events">Sự cố / yêu cầu</Link>,
            },
            {
              key: '/devices',
              icon: <MedicineBoxOutlined />,
              label: <Link to="/devices">Thiết bị</Link>,
            },
            {
              key: '/scan-qr',
              icon: <QrcodeOutlined />,
              label: <Link to="/scan-qr">Quét QR</Link>,
            },
            {
              key: '/asset-inventory',
              icon: <AuditOutlined />,
              label: <Link to="/asset-inventory">Kiểm kê tài sản</Link>,
            },
            {
              key: '/usage-logs',
              icon: <BookOutlined />,
              label: <Link to="/usage-logs">Nhật ký sử dụng</Link>,
            },
            ...(showReports
              ? [
                  {
                    key: '/reports',
                    icon: <FileTextOutlined />,
                    label: <Link to="/reports">Báo cáo / số liệu</Link>,
                  },
                ]
              : []),
            ...(showBidding
              ? [
                  {
                    key: '/bidding',
                    icon: <ProfileOutlined />,
                    label: <Link to="/bidding">Gói thầu</Link>,
                  },
                ]
              : []),
            ...(showOperations
              ? [
                  {
                    key: '/legal-documents',
                    icon: <SolutionOutlined />,
                    label: <Link to="/legal-documents">Hồ sơ pháp lý</Link>,
                  },
                  {
                    key: '/sign-document',
                    icon: <FileProtectOutlined />,
                    label: <Link to="/sign-document">Ký số PDF</Link>,
                  },
                  {
                    key: '/personnel',
                    icon: <TeamOutlined />,
                    label: <Link to="/personnel">Nhân sự</Link>,
                  },
                ]
              : []),
            ...(showWarehouse
              ? [
                  {
                    key: '/warehouse',
                    icon: <ShopOutlined />,
                    label: <Link to="/warehouse">Kho TVT</Link>,
                  },
                ]
              : []),
            ...(showAdmin
              ? [
                  {
                    key: '/organizations',
                    icon: <BankOutlined />,
                    label: <Link to="/organizations">Đơn vị</Link>,
                  },
                  {
                    key: '/admin/master',
                    icon: <SettingOutlined />,
                    label: <Link to="/admin/master">Danh mục</Link>,
                  },
                  {
                    key: '/admin/system-logs',
                    icon: <HistoryOutlined />,
                    label: <Link to="/admin/system-logs">Nhật ký hệ thống</Link>,
                  },
                  {
                    key: '/users',
                    icon: <UserSwitchOutlined />,
                    label: <Link to="/users">Tài khoản</Link>,
                  },
                ]
              : []),
          ]}
        />
      </Sider>
      <Layout className="app-main-column">
        <Header className="app-header-bar">
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              height: '100%',
            }}
          >
            <Typography.Text strong style={{ color: '#334155', fontSize: 15 }}>
              Bệnh viện / Cơ sở y tế
            </Typography.Text>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <NotificationBell />
              <Link to="/profile">
                <Button type="text" icon={<UserOutlined />}>Tài khoản</Button>
              </Link>
              <span className="app-user-chip">
                <Avatar size="small" icon={<UserOutlined />} style={{ background: '#0d9488' }} />
                <Typography.Text style={{ fontSize: 13 }}>
                  {user?.fullName || user?.username}
                </Typography.Text>
              </span>
              <Button
                type="text"
                icon={<LogoutOutlined />}
                onClick={() => {
                  logout();
                  navigate('/login');
                }}
              >
                Đăng xuất
              </Button>
            </div>
          </div>
        </Header>
        <Content className="app-shell-content">
          <div className="app-page-inner">
            <Outlet />
          </div>
        </Content>
      </Layout>
    </Layout>
  );
}
