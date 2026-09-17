import type { ThemeConfig } from 'antd';

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
