# -*- coding: utf-8 -*-
"""Modern UI + Vietnamese diacritics. Run: python scripts/patch_frontend_ui_vn.py"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "qltbyt-frontend"

FILES: dict[str, str] = {}

FILES["index.html"] = """<!doctype html>
<html lang="vi">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>QLTBYT — Quản lý thiết bị y tế</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link
      href="https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@400;500;600;700&display=swap"
      rel="stylesheet"
    />
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.tsx"></script>
  </body>
</html>
"""

FILES["src/index.css"] = """html, body, #root {
  height: 100%;
  margin: 0;
}

body {
  font-family: 'Be Vietnam Pro', system-ui, -apple-system, 'Segoe UI', sans-serif;
  -webkit-font-smoothing: antialiased;
  background: #f1f5f9;
}

.app-shell-content {
  margin: 0;
  padding: 24px 28px 32px;
  min-height: calc(100vh - 64px);
  background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
}

.app-page-inner {
  max-width: 1280px;
  margin: 0 auto;
}

.app-card {
  border-radius: 12px !important;
  border: 1px solid #e2e8f0 !important;
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.06) !important;
}

.app-sidebar-brand {
  padding: 20px 20px 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.app-sidebar-brand-title {
  color: #f8fafc;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: 0.02em;
  line-height: 1.3;
}

.app-sidebar-brand-sub {
  color: #94a3b8;
  font-size: 12px;
  margin-top: 4px;
  line-height: 1.4;
}

.app-header-bar {
  background: #fff !important;
  border-bottom: 1px solid #e2e8f0;
  padding: 0 28px !important;
  height: 64px !important;
  line-height: 64px !important;
}

.app-user-chip {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  padding: 6px 12px;
  background: #f1f5f9;
  border-radius: 999px;
  margin-right: 12px;
}

.app-login-root {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 1fr 1fr;
}

@media (max-width: 900px) {
  .app-login-root {
    grid-template-columns: 1fr;
  }
  .app-login-hero {
    display: none;
  }
}

.app-login-hero {
  background: linear-gradient(145deg, #0f766e 0%, #0c4a6e 55%, #0f172a 100%);
  color: #f8fafc;
  padding: 48px 56px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.app-login-hero h1 {
  font-size: 32px;
  font-weight: 700;
  margin: 0 0 16px;
  line-height: 1.25;
}

.app-login-hero p {
  font-size: 16px;
  color: #cbd5e1;
  max-width: 420px;
  line-height: 1.6;
}

.app-login-hero-tags {
  margin-top: 32px;
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.app-login-hero-tag {
  background: rgba(255, 255, 255, 0.12);
  padding: 8px 14px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
}

.app-login-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px;
  background: #f8fafc;
}

.app-login-card {
  width: 100%;
  max-width: 400px;
  border-radius: 16px !important;
  border: 1px solid #e2e8f0 !important;
  box-shadow: 0 8px 30px rgba(15, 23, 42, 0.08) !important;
}

.app-stat-card .ant-statistic-title {
  color: #64748b;
  font-size: 13px;
}

.app-loading-wrap {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 240px;
}

.app-filter-bar {
  margin-bottom: 16px;
  padding: 16px 20px;
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
}
"""

FILES["src/theme/appTheme.ts"] = """import type { ThemeConfig } from 'antd';

export const appTheme: ThemeConfig = {
  token: {
    colorPrimary: '#0d9488',
    colorInfo: '#0284c7',
    colorSuccess: '#059669',
    colorWarning: '#d97706',
    colorError: '#dc2626',
    borderRadius: 8,
    fontFamily: "'Be Vietnam Pro', system-ui, -apple-system, sans-serif",
    colorBgLayout: '#f1f5f9',
  },
  components: {
    Layout: {
      headerBg: '#ffffff',
      bodyBg: '#f1f5f9',
    },
    Menu: {
      darkItemSelectedBg: '#0d9488',
      darkItemSelectedColor: '#fff',
      darkItemHoverBg: 'rgba(255,255,255,0.08)',
    },
    Table: {
      headerBg: '#f8fafc',
      headerColor: '#475569',
      borderColor: '#e2e8f0',
    },
    Button: {
      primaryShadow: '0 2px 0 rgba(13, 148, 136, 0.15)',
    },
  },
};
"""

FILES["src/components/PageHeader.tsx"] = """import { Breadcrumb, Typography } from 'antd';
import type { ReactNode } from 'react';

type Crumb = { title: string; href?: string };

export function PageHeader({
  title,
  subtitle,
  crumbs,
  extra,
}: {
  title: string;
  subtitle?: string;
  crumbs?: Crumb[];
  extra?: ReactNode;
}) {
  return (
    <div style={{ marginBottom: 24 }}>
      {crumbs && crumbs.length > 0 && (
        <Breadcrumb
          style={{ marginBottom: 8 }}
          items={crumbs.map((c) => ({
            title: c.href ? <a href={c.href}>{c.title}</a> : c.title,
          }))}
        />
      )}
      <div
        style={{
          display: 'flex',
          alignItems: 'flex-start',
          justifyContent: 'space-between',
          gap: 16,
          flexWrap: 'wrap',
        }}
      >
        <div>
          <Typography.Title level={3} style={{ margin: 0, fontWeight: 700 }}>
            {title}
          </Typography.Title>
          {subtitle && (
            <Typography.Text type="secondary" style={{ fontSize: 14 }}>
              {subtitle}
            </Typography.Text>
          )}
        </div>
        {extra}
      </div>
    </div>
  );
}
"""

FILES["src/components/LoadingCenter.tsx"] = """import { Spin } from 'antd';

export function LoadingCenter() {
  return (
    <div className="app-loading-wrap">
      <Spin size="large" tip="Đang tải..." />
    </div>
  );
}
"""

FILES["src/main.tsx"] = """import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConfigProvider, App as AntApp } from 'antd';
import viVN from 'antd/locale/vi_VN';
import dayjs from 'dayjs';
import 'dayjs/locale/vi';
import { App } from './App';
import './index.css';
import { useAuthStore } from './store/authStore';
import { appTheme } from './theme/appTheme';

dayjs.locale('vi');

useAuthStore.getState().hydrate();

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, refetchOnWindowFocus: false } },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ConfigProvider locale={viVN} theme={appTheme}>
        <AntApp>
          <App />
        </AntApp>
      </ConfigProvider>
    </QueryClientProvider>
  </React.StrictMode>,
);
"""

FILES["src/layouts/MainLayout.tsx"] = """import { Avatar, Button, Layout, Menu, Typography } from 'antd';
import {
  DashboardOutlined,
  LogoutOutlined,
  MedicineBoxOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';

const { Header, Sider, Content } = Layout;

export function MainLayout() {
  const location = useLocation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);

  const selected = location.pathname.startsWith('/devices')
    ? '/devices'
    : location.pathname.startsWith('/dashboard')
      ? '/dashboard'
      : location.pathname;

  return (
    <Layout style={{ minHeight: '100vh' }}>
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
              key: '/devices',
              icon: <MedicineBoxOutlined />,
              label: <Link to="/devices">Thiết bị</Link>,
            },
          ]}
        />
      </Sider>
      <Layout>
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
            <div style={{ display: 'flex', alignItems: 'center' }}>
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
"""

# Login, Dashboard, Device pages - continue in same script file below main block
# Split write using exec of remaining content

def write_all() -> None:
    for rel, content in FILES.items():
        path = ROOT / rel
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding="utf-8", newline="\n")
    print(f"Patched {len(FILES)} base files")


if __name__ == "__main__":
    write_all()
