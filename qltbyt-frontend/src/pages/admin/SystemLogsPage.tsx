import { Button, Card, DatePicker, Form, Input, InputNumber, Space, Table } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import dayjs, { type Dayjs } from 'dayjs';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, PagedResult } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';

const { RangePicker } = DatePicker;

export interface SystemAuditLogItem {
  id: number;
  userId?: number;
  username?: string;
  userFullName?: string;
  content?: string;
  timestamp?: string;
  deviceId?: number;
  deviceCode?: string;
  deviceName?: string;
}

export function SystemLogsPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [userId, setUserId] = useState<number | null>(null);
  const [deviceId, setDeviceId] = useState<number | null>(null);
  const [range, setRange] = useState<[Dayjs | null, Dayjs | null] | null>(null);

  const [applied, setApplied] = useState({
    search: '',
    userId: null as number | null,
    deviceId: null as number | null,
    from: null as string | null,
    to: null as string | null,
  });

  const logsQuery = useQuery({
    queryKey: ['system-logs', page, pageSize, applied],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (applied.search) params.set('search', applied.search);
      if (applied.userId != null) params.set('userId', String(applied.userId));
      if (applied.deviceId != null) params.set('deviceId', String(applied.deviceId));
      if (applied.from) params.set('fromDate', applied.from);
      if (applied.to) params.set('toDate', applied.to);

      const res = await axiosClient.get<ApiResponse<PagedResult<SystemAuditLogItem>>>(
        `/api/system-logs?${params}`,
      );
      return res.data.data!;
    },
  });

  const applyFilters = () => {
    setPage(1);
    setApplied({
      search: search.trim(),
      userId,
      deviceId,
      from: range?.[0] ? range[0].startOf('day').toISOString() : null,
      to: range?.[1] ? range[1].startOf('day').toISOString() : null,
    });
  };

  return (
    <div>
      <PageHeader
        title="Nhật ký hệ thống"
        subtitle="Log thao tác người dùng (LogHethong)"
      />
      <Card className="app-card" style={{ marginBottom: 16 }}>
        <Form layout="inline" onFinish={applyFilters}>
          <Form.Item label="Nội dung">
            <Input
              allowClear
              prefix={<SearchOutlined />}
              placeholder="Tìm theo nội dung..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              style={{ width: 240 }}
            />
          </Form.Item>
          <Form.Item label="ID tài khoản">
            <InputNumber
              min={1}
              placeholder="User ID"
              value={userId ?? undefined}
              onChange={(v) => setUserId(v ?? null)}
              style={{ width: 120 }}
            />
          </Form.Item>
          <Form.Item label="ID thiết bị">
            <InputNumber
              min={1}
              placeholder="Device ID"
              value={deviceId ?? undefined}
              onChange={(v) => setDeviceId(v ?? null)}
              style={{ width: 120 }}
            />
          </Form.Item>
          <Form.Item label="Khoảng thời gian">
            <RangePicker
              format="DD/MM/YYYY"
              value={range}
              onChange={(v) => setRange(v)}
            />
          </Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={logsQuery.isFetching}>
                Lọc
              </Button>
              <Button
                onClick={() => {
                  setSearch('');
                  setUserId(null);
                  setDeviceId(null);
                  setRange(null);
                  setPage(1);
                  setApplied({
                    search: '',
                    userId: null,
                    deviceId: null,
                    from: null,
                    to: null,
                  });
                }}
              >
                Xóa lọc
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>

      <Card className="app-card" bodyStyle={{ padding: 0 }}>
        <Table
          rowKey="id"
          loading={logsQuery.isLoading || logsQuery.isFetching}
          dataSource={logsQuery.data?.items ?? []}
          pagination={{
            current: page,
            pageSize,
            total: logsQuery.data?.totalCount ?? 0,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50', '100'],
            showTotal: (t) => `Tổng ${t} bản ghi`,
            onChange: (p, size) => {
              setPage(p);
              setPageSize(size || 20);
            },
          }}
          columns={[
            {
              title: 'Thời gian',
              dataIndex: 'timestamp',
              width: 160,
              render: (t) => (t ? dayjs(t).format('DD/MM/YYYY HH:mm:ss') : '—'),
            },
            {
              title: 'Người dùng',
              width: 180,
              ellipsis: true,
              render: (_, r) => {
                const name = r.userFullName || r.username;
                if (!name && !r.userId) return '—';
                return name ? `${name}${r.userId ? ` (#${r.userId})` : ''}` : `#${r.userId}`;
              },
            },
            {
              title: 'Nội dung',
              dataIndex: 'content',
              ellipsis: true,
              render: (c) => c || '—',
            },
            {
              title: 'Thiết bị',
              width: 200,
              ellipsis: true,
              render: (_, r) => {
                if (!r.deviceId) return '—';
                const label = r.deviceCode || r.deviceName || `#${r.deviceId}`;
                return <Link to={`/devices/${r.deviceId}`}>{label}</Link>;
              },
            },
          ]}
        />
      </Card>
    </div>
  );
}
