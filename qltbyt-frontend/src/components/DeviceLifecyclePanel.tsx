import { Card, Col, Empty, Row, Table, Typography } from 'antd';
import { useQuery } from '@tanstack/react-query';
import {
  Bar,
  CartesianGrid,
  Cell,
  ComposedChart,
  Legend,
  Line,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import dayjs from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import { LoadingCenter } from '@/components/LoadingCenter';

const TCO_COLORS = ['#1D7ABF', '#e74c3c', '#58d68d', '#f5b041'];

export interface DeviceLifecycleReport {
  header: {
    deviceId: number;
    deviceCode?: string;
    deviceName?: string;
    manufacturer?: string;
    modelCode?: string;
    yearOfManufacture?: string;
    statusName?: string;
    healthScore: number;
    nextInspectionDate?: string;
    summaryLine: string;
    statusLine: string;
    isHealthy: boolean;
  };
  tcoBreakdown: { category: string; amountMillion: number }[];
  yearlyHistory: { year: string; breakdowns: number; repairCostMillion: number }[];
  maintenanceLogs: {
    date?: string;
    type: string;
    description?: string;
    cost: number;
    engineer?: string;
    status: string;
  }[];
}

function formatMillion(v: number) {
  return `${v.toLocaleString('vi-VN', { maximumFractionDigits: 3 })} Triệu VNĐ`;
}

function formatVnd(v: number) {
  return v.toLocaleString('vi-VN');
}

interface Props {
  deviceId: number;
}

export function DeviceLifecyclePanel({ deviceId }: Props) {
  const query = useQuery({
    queryKey: ['device-lifecycle', deviceId],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceLifecycleReport>>(
        `/api/devices/${deviceId}/lifecycle`,
      );
      return res.data.data!;
    },
    enabled: deviceId > 0,
  });

  if (query.isLoading) return <LoadingCenter />;
  if (query.isError || !query.data) {
    return <Empty description="Không tải được báo cáo vòng đời thiết bị" />;
  }

  const { header, tcoBreakdown, yearlyHistory, maintenanceLogs } = query.data;
  const hasTco = tcoBreakdown.length > 0;
  const hasHistory = yearlyHistory.some((y) => y.breakdowns > 0 || y.repairCostMillion > 0);

  return (
    <div>
      <Card size="small" style={{ marginBottom: 16 }}>
        <Typography.Title level={4} style={{ marginTop: 0, marginBottom: 8 }}>
          Báo cáo phân tích vòng đời thiết bị
        </Typography.Title>
        <Typography.Paragraph style={{ marginBottom: 4, color: '#2980b9', fontWeight: 600 }}>
          {header.summaryLine}
        </Typography.Paragraph>
        <Typography.Text
          strong
          style={{ color: header.isHealthy ? '#16a34a' : '#dc2626' }}
        >
          {header.statusLine}
        </Typography.Text>
      </Card>

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} lg={12}>
          <Card size="small" title="1. Cơ cấu tổng chi phí sở hữu">
            {!hasTco ? (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="Chưa có dữ liệu chi phí (nguyên giá / chi phí sự cố)"
              />
            ) : (
              <ResponsiveContainer width="100%" height={320}>
                <PieChart>
                  <Pie
                    data={tcoBreakdown}
                    dataKey="amountMillion"
                    nameKey="category"
                    cx="50%"
                    cy="45%"
                    outerRadius={100}
                    label={({ percent }) =>
                      percent != null && percent >= 0.05
                        ? `${(percent * 100).toFixed(0)}%`
                        : ''
                    }
                  >
                    {tcoBreakdown.map((_, i) => (
                      <Cell key={i} fill={TCO_COLORS[i % TCO_COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip
                    formatter={(value: number, name: string) => [formatMillion(value), name]}
                  />
                  <Legend
                    layout="horizontal"
                    verticalAlign="bottom"
                    wrapperStyle={{ fontSize: 12 }}
                    formatter={(value) => {
                      const row = tcoBreakdown.find((x) => x.category === value);
                      return row
                        ? `${value}: ${formatMillion(row.amountMillion)}`
                        : value;
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card size="small" title="2. Tần suất lỗi & chi phí sửa chữa">
            {!hasHistory ? (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="Chưa có lịch sử sự cố / chi phí theo năm"
              />
            ) : (
              <ResponsiveContainer width="100%" height={320}>
                <ComposedChart data={yearlyHistory} margin={{ top: 8, right: 12, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="year" />
                  <YAxis
                    yAxisId="left"
                    allowDecimals={false}
                    width={40}
                    label={{
                      value: 'Số lần hỏng',
                      angle: -90,
                      position: 'insideLeft',
                      style: { fontSize: 11 },
                    }}
                  />
                  <YAxis
                    yAxisId="right"
                    orientation="right"
                    width={48}
                    label={{
                      value: 'Triệu VNĐ',
                      angle: 90,
                      position: 'insideRight',
                      style: { fontSize: 11 },
                    }}
                  />
                  <Tooltip
                    formatter={(value: number, name: string) => {
                      if (name.includes('Chi phí')) return [formatMillion(value), name];
                      return [value, name];
                    }}
                  />
                  <Legend />
                  <Bar
                    yAxisId="left"
                    dataKey="breakdowns"
                    name="Số lần hỏng đột xuất"
                    fill="#5dade2"
                    radius={[4, 4, 0, 0]}
                  />
                  <Line
                    yAxisId="right"
                    type="monotone"
                    dataKey="repairCostMillion"
                    name="Chi phí sửa chữa (Triệu VNĐ)"
                    stroke="#e74c3c"
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                </ComposedChart>
              </ResponsiveContainer>
            )}
          </Card>
        </Col>
      </Row>

      <Card size="small" title="3. Nhật ký bảo trì & sửa chữa">
        <Table
          size="small"
          rowKey={(_, index) => String(index)}
          dataSource={maintenanceLogs}
          locale={{ emptyText: 'Chưa có nhật ký sự cố / bảo trì' }}
          pagination={{ pageSize: 10, showSizeChanger: true }}
          columns={[
            {
              title: 'Ngày thực hiện',
              dataIndex: 'date',
              width: 120,
              render: (v?: string) => (v ? dayjs(v).format('DD/MM/YYYY') : '—'),
              sorter: (a, b) => dayjs(a.date).valueOf() - dayjs(b.date).valueOf(),
            },
            {
              title: 'Loại hình',
              dataIndex: 'type',
              width: 150,
              filters: [...new Set(maintenanceLogs.map((x) => x.type))].map((t) => ({
                text: t,
                value: t,
              })),
              onFilter: (value, record) => record.type === value,
            },
            {
              title: 'Mô tả chi tiết',
              dataIndex: 'description',
              ellipsis: true,
            },
            {
              title: 'Chi phí (VNĐ)',
              dataIndex: 'cost',
              width: 140,
              align: 'right',
              render: (v: number) => formatVnd(v ?? 0),
              sorter: (a, b) => (a.cost ?? 0) - (b.cost ?? 0),
            },
            {
              title: 'Kỹ sư phụ trách',
              dataIndex: 'engineer',
              width: 160,
              render: (v?: string) => v || '—',
            },
            {
              title: 'Trạng thái',
              dataIndex: 'status',
              width: 130,
              filters: [...new Set(maintenanceLogs.map((x) => x.status))].map((t) => ({
                text: t,
                value: t,
              })),
              onFilter: (value, record) => record.status === value,
            },
          ]}
        />
      </Card>
    </div>
  );
}
