import { Card, Col, Row, Statistic, Table } from 'antd';
import {
  AlertOutlined,
  MedicineBoxOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DashboardSummary } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

const CHART_COLORS = ['#0d9488', '#0284c7', '#059669', '#d97706', '#7c3aed', '#dc2626'];

export function DashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard', 90],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DashboardSummary>>(
        '/api/dashboard/summary?expiringWithinDays=90',
      );
      return res.data.data;
    },
  });

  if (isLoading) {
    return <LoadingCenter />;
  }

  const summary = data;

  return (
    <div>
      <PageHeader
        title="Tổng quan"
        subtitle="Thống kê nhanh về thiết bị và cảnh báo kiểm định"
      />
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={8}>
          <Card className="app-card app-stat-card">
            <Statistic
              title="Tổng số thiết bị"
              value={summary?.totalDevices ?? 0}
              prefix={<MedicineBoxOutlined style={{ color: '#0d9488' }} />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card className="app-card app-stat-card">
            <Statistic
              title="Sắp hết hạn kiểm định (90 ngày)"
              value={summary?.expiringInspectionCount ?? 0}
              prefix={<AlertOutlined style={{ color: '#d97706' }} />}
            />
          </Card>
        </Col>
      </Row>
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <Card className="app-card" title="Theo trạng thái">
            <ResponsiveContainer width="100%" height={340}>
              <PieChart margin={{ top: 8, right: 8, left: 8, bottom: 8 }}>
                <Pie
                  data={summary?.byStatus ?? []}
                  dataKey="value"
                  nameKey="label"
                  outerRadius={100}
                  innerRadius={48}
                  paddingAngle={1}
                  labelLine={false}
                  label={({ percent, value }) =>
                    percent != null && percent >= 0.08 ? String(value) : ''
                  }
                >
                  {(summary?.byStatus ?? []).map((_, i) => (
                    <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: number, _name, item) => {
                    const label = (item?.payload as { label?: string } | undefined)?.label ?? '';
                    return [`${value}`, label];
                  }}
                />
                <Legend
                  layout="horizontal"
                  verticalAlign="bottom"
                  wrapperStyle={{ paddingTop: 12, fontSize: 12 }}
                  formatter={(value) => {
                    const row = (summary?.byStatus ?? []).find((x) => x.label === value);
                    return row ? `${value}: ${row.value}` : value;
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card className="app-card" title="Theo loại thiết bị">
            <ResponsiveContainer width="100%" height={340}>
              <BarChart
                data={summary?.byType ?? []}
                margin={{ top: 8, right: 8, left: 0, bottom: 72 }}
              >
                <XAxis
                  dataKey="label"
                  interval={0}
                  angle={-35}
                  textAnchor="end"
                  height={80}
                  tick={{ fontSize: 11 }}
                  tickFormatter={(v: string) =>
                    v && v.length > 18 ? `${v.slice(0, 16)}…` : v
                  }
                />
                <YAxis allowDecimals={false} width={44} />
                <Tooltip />
                <Bar dataKey="value" fill="#0d9488" radius={[6, 6, 0, 0]} name="Số lượng" />
              </BarChart>
            </ResponsiveContainer>
          </Card>
        </Col>
      </Row>
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24}>
          <Card className="app-card" title="Theo đơn vị (số lượng & giá trị tài sản)">
            <Table
              size="small"
              pagination={false}
              rowKey={(r) => String(r.organizationId ?? r.label)}
              dataSource={summary?.byOrganization ?? []}
              style={{ marginBottom: 16 }}
              columns={[
                { title: 'Đơn vị', dataIndex: 'label' },
                { title: 'Số thiết bị', dataIndex: 'deviceCount', width: 120 },
                {
                  title: 'Giá trị tài sản (VNĐ)',
                  dataIndex: 'totalAssetValue',
                  render: (v: number) => (v != null ? Number(v).toLocaleString('vi-VN') : ''),
                },
              ]}
            />
            <ResponsiveContainer width="100%" height={280}>
              <BarChart data={summary?.byOrganization ?? []} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
                <XAxis dataKey="label" tick={{ fontSize: 11 }} interval={0} angle={-20} textAnchor="end" height={70} />
                <YAxis yAxisId="count" allowDecimals={false} width={40} />
                <YAxis yAxisId="value" orientation="right" width={56} tickFormatter={(v) => `${Math.round(v / 1e6)}M`} />
                <Tooltip
                  formatter={(value: number, name: string) =>
                    name === 'Giá trị' ? `${Number(value).toLocaleString('vi-VN')} VNĐ` : value
                  }
                />
                <Legend />
                <Bar yAxisId="count" dataKey="deviceCount" name="Số thiết bị" fill="#0d9488" radius={[4, 4, 0, 0]} />
                <Bar yAxisId="value" dataKey="totalAssetValue" name="Giá trị" fill="#0284c7" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </Card>
        </Col>
      </Row>
    </div>
  );
}
