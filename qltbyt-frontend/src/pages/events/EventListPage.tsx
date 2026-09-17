import { Button, Card, Col, Form, Row, Select, Space, Table, Tag, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, PagedResult } from '@/types/api';
import type { EventListItem } from '@/types/events';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { useAuthStore } from '@/store/authStore';
import { ResolveEventModal } from '@/components/ResolveEventModal';

export function EventListPage() {
  const user = useAuthStore((s) => s.user);
  const canResolve = user?.role === 'Admin' || user?.role === 'Technician';
  const queryClient = useQueryClient();
  const [form] = Form.useForm();
  const [resolveEvent, setResolveEvent] = useState<EventListItem | null>(null);
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const statusId = Form.useWatch('statusId', form);

  const eventsQuery = useQuery({
    queryKey: ['events', statusId, page],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (statusId) params.set('statusId', String(statusId));
      const res = await axiosClient.get<ApiResponse<PagedResult<EventListItem>>>(
        `/api/events?${params}`,
      );
      return res.data.data!;
    },
  });

  const resolveMutation = useMutation({
    mutationFn: async ({
      id,
      payload,
    }: {
      id: number;
      payload: { resolutionContent: string; cost?: number };
    }) => {
      await axiosClient.put(`/api/events/${id}`, payload);
    },
    onSuccess: () => {
      message.success('Cập nhật xử lý thành công');
      setResolveEvent(null);
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
    onError: () => message.error('Xử lý thất bại'),
  });

  const data = eventsQuery.data;

  return (
    <div>
      <PageHeader title="Sự cố & yêu cầu" subtitle="Theo dõi báo hỏng và xử lý" />
      <Card className="app-card">
        <Form
          form={form}
          layout="vertical"
          style={{ marginBottom: 16 }}
          onValuesChange={() => setPage(1)}
        >
          <Row gutter={16} align="bottom">
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="statusId" label="Trạng thái">
                <Select
                  allowClear
                  placeholder="Tất cả trạng thái"
                  style={{ width: '100%', minWidth: 200 }}
                  options={[
                    { value: 1, label: 'Chờ xử lý' },
                    { value: 2, label: 'Đã xử lý' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8}>
              <Space>
                <Button type="primary" onClick={() => eventsQuery.refetch()}>Áp dụng lọc</Button>
                <Button
                  onClick={() => {
                    form.resetFields();
                    setPage(1);
                  }}
                >
                  Xóa lọc
                </Button>
              </Space>
            </Col>
          </Row>
        </Form>
        {eventsQuery.isLoading ? (
          <LoadingCenter />
        ) : (
          <Table
            rowKey="id"
            size="middle"
            dataSource={data?.items ?? []}
            scroll={{ x: 900, y: 520 }}
            pagination={{
              current: page,
              pageSize,
              total: data?.totalCount ?? 0,
              showSizeChanger: false,
              showTotal: (total) => `Tổng ${total} bản ghi`,
              onChange: (p) => setPage(p),
            }}
            columns={[
              {
                title: 'Mã TB',
                width: 110,
                fixed: 'left',
                render: (_, r) => (
                  <Link to={`/devices/${r.deviceId}`}>{r.deviceCode || r.deviceId}</Link>
                ),
              },
              {
                title: 'Tên thiết bị',
                dataIndex: 'deviceName',
                width: 180,
                ellipsis: true,
              },
              { title: 'Loại', dataIndex: 'typeName', width: 100, ellipsis: true },
              {
                title: 'Nội dung',
                dataIndex: 'content',
                ellipsis: true,
                width: 280,
              },
              {
                title: 'Thời gian',
                width: 130,
                render: (_, r) =>
                  r.eventDateTime ? dayjs(r.eventDateTime).format('DD/MM/YYYY HH:mm') : '—',
              },
              {
                title: 'Trạng thái',
                width: 100,
                render: (_, r) => (
                  <Tag color={r.statusId === 2 ? 'green' : 'orange'}>{r.statusName}</Tag>
                ),
              },
              {
                title: '',
                width: 90,
                fixed: 'right',
                render: (_, r) =>
                  canResolve && r.statusId === 1 ? (
                    <Button size="small" type="link" onClick={() => setResolveEvent(r)}>
                      Xử lý
                    </Button>
                  ) : null,
              },
            ]}
          />
        )}
      </Card>
      <ResolveEventModal
        open={resolveEvent !== null}
        event={resolveEvent}
        loading={resolveMutation.isPending}
        onClose={() => setResolveEvent(null)}
        onSubmit={(payload) => {
          if (!resolveEvent) return;
          resolveMutation.mutate({ id: resolveEvent.id, payload });
        }}
      />
    </div>
  );
}
