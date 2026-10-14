import type { ThemeConfig } from 'antd';

/** Tone màu theo brand DVC Techflow (dvctechflow.com / logo). */
export const brand = {
  primary: '#1D7ABF',
  primaryBright: '#2698D4',
  primaryDeep: '#216CB5',
  navy: '#003366',
  navyDark: '#0a1628',
  sidebarFrom: '#0a1f3d',
  sidebarTo: '#0c2d5c',
  info: '#2698D4',
  success: '#059669',
  warning: '#d97706',
  error: '#dc2626',
} as const;

export const appTheme: ThemeConfig = {
  token: {
    colorPrimary: brand.primary,
    colorInfo: brand.info,
    colorSuccess: brand.success,
    colorWarning: brand.warning,
    colorError: brand.error,
    colorLink: brand.primary,
    borderRadius: 8,
    fontFamily: "'Be Vietnam Pro', system-ui, -apple-system, sans-serif",
    colorBgLayout: '#f1f5f9',
  },
  components: {
    Layout: {
      headerBg: '#ffffff',
      bodyBg: '#f1f5f9',
      siderBg: brand.sidebarFrom,
    },
    Menu: {
      darkItemBg: 'transparent',
      darkItemSelectedBg: brand.primary,
      darkItemSelectedColor: '#fff',
      darkItemHoverBg: 'rgba(38, 152, 212, 0.18)',
    },
    Table: {
      headerBg: '#f0f7fc',
      headerColor: '#1e3a5f',
      borderColor: '#dbe7f2',
    },
    Button: {
      primaryShadow: '0 2px 0 rgba(29, 122, 191, 0.18)',
    },
  },
};
