import { Breadcrumb, Typography } from 'antd';
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

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
            title: c.href ? <Link to={c.href}>{c.title}</Link> : c.title,
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
