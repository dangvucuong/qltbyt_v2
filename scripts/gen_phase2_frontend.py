# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FE = ROOT / "qltbyt-frontend" / "src"


def w(rel, content):
    p = FE / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content.strip() + "\n", encoding="utf-8", newline="\n")
    print("write", rel)


w(
    "types/events.ts",
    """
export interface EventListItem {
  id: number;
  deviceId?: number;
  deviceCode?: string;
  deviceName?: string;
  typeId?: number;
  typeName?: string;
  content?: string;
  eventDateTime?: string;
  statusId?: number;
  statusName?: string;
  keyword?: string;
  cost?: number;
  accountUsername?: string;
}

export interface EventDetail extends EventListItem {
  managerUserId?: number;
  managerName?: string;
}

export interface CreateDeviceEventPayload {
  eventTypeId: number;
  content: string;
  keyword?: string;
}

export interface ResolveEventPayload {
  resolutionContent: string;
  cost?: number;
}

export interface NotificationItem {
  id: number;
  title?: string;
  content?: string;
  createdAt?: string;
  isRead: boolean;
}
""",
)

w(
    "components/NotificationBell.tsx",
    """
import { Badge, Button, Dropdown, List, Typography } from 'antd';
import { BellOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { NotificationItem } from '@/types/events';

export function NotificationBell() {
  const queryClient = useQueryClient();
  const q = useQuery({
    queryKey: ['notifications'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<NotificationItem[]>>('/api/notifications');
      return res.data.data ?? [];
    },
    refetchInterval: 60_000,
  });

  const markRead = useMutation({
    mutationFn: async (id: number) => {
      await axiosClient.put(`/api/notifications/${id}/read`);
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
  });

  const unread = (q.data ?? []).filter((n) => !n.isRead).length;

  const menu = {
    items: [
      {
        key: 'list',
        label: (
          <div style={{ width: 320, maxHeight: 360, overflow: 'auto' }}>
            <List
              size="small"
              dataSource={q.data ?? []}
              locale={{ emptyText: 'Không có thông báo' }}
              renderItem={(item) => (
                <List.Item
                  onClick={() => !item.isRead && markRead.mutate(item.id)}
                  style={{ cursor: 'pointer', opacity: item.isRead ? 0.65 : 1 }}
                >
                  <List.Item.Meta
                    title={<Typography.Text strong>{item.title}</Typography.Text>}
                    description={
                      <>
                        <div>{item.content}</div>
                        <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                          {item.createdAt ? dayjs(item.createdAt).format('DD/MM/YYYY HH:mm') : ''}
                        </Typography.Text>
                      </>
                    }
                  />
                </List.Item>
              )}
            />
          </div>
        ),
      },
    ],
  };

  return (
    <Dropdown menu={menu} trigger={['click']} placement="bottomRight">
      <Badge count={unread} size="small">
        <Button type="text" icon={<BellOutlined />} aria-label="Thông báo" />
      </Badge>
    </Dropdown>
  );
}
""",
)

w(
    "components/DeviceReportModal.tsx",
    """
import { Form, Input, Modal, Select, message } from 'antd';
import { useMutation } from '@tanstack/react-query';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { CreateDeviceEventPayload, EventDetail } from '@/types/events';

const EVENT_TYPES = [
  { value: 1, label: 'Báo hỏng' },
  { value: 2, label: 'Kiểm tra' },
  { value: 3, label: 'Yêu cầu khác' },
  { value: 4, label: 'Chuyển giao' },
];

type Props = {
  open: boolean;
  deviceId: number;
  onClose: () => void;
  onSuccess?: () => void;
};

export function DeviceReportModal({ open, deviceId, onClose, onSuccess }: Props) {
  const [form] = Form.useForm<CreateDeviceEventPayload>();

  const mutation = useMutation({
    mutationFn: async (values: CreateDeviceEventPayload) => {
      const res = await axiosClient.post<ApiResponse<EventDetail>>(
        `/api/devices/${deviceId}/events`,
        values,
      );
      return res.data;
    },
    onSuccess: (data) => {
      if (data.success) {
        message.success(data.message || 'Gửi yêu cầu thành công');
        form.resetFields();
        onClose();
        onSuccess?.();
      } else {
        message.error(data.message || 'Gửi yêu cầu thất bại');
      }
    },
    onError: () => message.error('Gửi yêu cầu thất bại'),
  });

  return (
    <Modal
      title="Gửi yêu cầu / báo sự cố"
      open={open}
      onCancel={onClose}
      confirmLoading={mutation.isPending}
      onOk={() => form.submit()}
      okText="Gửi"
      cancelText="Hủy"
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{ eventTypeId: 1 }}
        onFinish={(v) => mutation.mutate(v)}
      >
        <Form.Item name="eventTypeId" label="Loại" rules={[{ required: true }]}>
          <Select options={EVENT_TYPES} />
        </Form.Item>
        <Form.Item name="keyword" label="Từ khóa">
          <Input placeholder="Báo hỏng, kiểm tra..." />
        </Form.Item>
        <Form.Item
          name="content"
          label="Nội dung"
          rules={[{ required: true, message: 'Nhập nội dung yêu cầu' }]}
        >
          <Input.TextArea rows={4} placeholder="Mô tả sự cố hoặc yêu cầu" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
""",
)

w(
    "pages/events/EventListPage.tsx",
    """
import { Button, Card, Form, Input, Select, Space, Table, Tag, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { Link } from 'react-router-dom';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, PagedResult } from '@/types/api';
import type { EventListItem, ResolveEventPayload } from '@/types/events';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { useAuthStore } from '@/store/authStore';

export function EventListPage() {
  const user = useAuthStore((s) => s.user);
  const canResolve = user?.role === 'Admin' || user?.role === 'Technician';
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  const statusId = Form.useWatch('statusId', form);

  const eventsQuery = useQuery({
    queryKey: ['events', statusId],
    queryFn: async () => {
      const params = new URLSearchParams({ page: '1', pageSize: '50' });
      if (statusId) params.set('statusId', String(statusId));
      const res = await axiosClient.get<ApiResponse<PagedResult<EventListItem>>>(
        `/api/events?${params}`,
      );
      return res.data.data!;
    },
  });

  const resolveMutation = useMutation({
    mutationFn: async ({ id, payload }: { id: number; payload: ResolveEventPayload }) => {
      await axiosClient.put(`/api/events/${id}`, payload);
    },
    onSuccess: () => {
      message.success('Cập nhật xử lý thành công');
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
    onError: () => message.error('Xử lý thất bại'),
  });

  if (eventsQuery.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader title="Sự cố & yêu cầu" subtitle="Theo dõi báo hỏng và xử lý" />
      <Card className="app-card">
        <Form form={form} layout="inline" style={{ marginBottom: 16 }}>
          <Form.Item name="statusId" label="Trạng thái">
            <Select
              allowClear
              placeholder="Tất cả"
              options={[
                { value: 1, label: 'Chờ xử lý' },
                { value: 2, label: 'Đã xử lý' },
              ]}
            />
          </Form.Item>
          <Button type="primary" onClick={() => eventsQuery.refetch()}>Lọc</Button>
        </Form>
        <Table
          rowKey="id"
          dataSource={eventsQuery.data?.items ?? []}
          pagination={false}
          columns={[
            {
              title: 'Thiết bị',
              render: (_, r) => (
                <Space>
                  <Link to={`/devices/${r.deviceId}`}>{r.deviceCode}</Link>
                  <span style={{ color: '#64748b' }}>{r.deviceName}</span>
                </Space>
              ),
            },
            { title: 'Loại', dataIndex: 'typeName', width: 120 },
            { title: 'Nội dung', dataIndex: 'content', ellipsis: true },
            {
              title: 'Thời gian',
              width: 140,
              render: (_, r) =>
                r.eventDateTime ? dayjs(r.eventDateTime).format('DD/MM/YYYY HH:mm') : '—',
            },
            {
              title: 'Trạng thái',
              width: 110,
              render: (_, r) => (
                <Tag color={r.statusId === 2 ? 'green' : 'orange'}>{r.statusName}</Tag>
              ),
            },
            {
              title: '',
              width: 120,
              render: (_, r) =>
                canResolve && r.statusId === 1 ? (
                  <Button
                    size="small"
                    type="link"
                    onClick={() => {
                      const resolutionContent = prompt('Nội dung xử lý:', 'Đã xử lý');
                      if (!resolutionContent) return;
                      resolveMutation.mutate({
                        id: r.id,
                        payload: { resolutionContent, cost: 0 },
                      });
                    }}
                  >
                    Xử lý
                  </Button>
                ) : null,
            },
          ]}
        />
      </Card>
    </div>
  );
}
""",
)

if __name__ == "__main__":
    # patch api.ts - append types manually in script
    api_ts = FE / "types" / "api.ts"
    text = api_ts.read_text(encoding="utf-8")
    if "EventListItem" not in text:
        text += "\nexport type { EventListItem, EventDetail, NotificationItem } from './events';\n"
        api_ts.write_text(text, encoding="utf-8", newline="\n")
        print("patch api.ts")

    # patch App.tsx
    app = FE / "App.tsx"
    app_text = app.read_text(encoding="utf-8")
    if "EventListPage" not in app_text:
        app_text = app_text.replace(
            "import { DeviceFormPage } from '@/pages/devices/DeviceFormPage';",
            "import { DeviceFormPage } from '@/pages/devices/DeviceFormPage';\n"
            "import { EventListPage } from '@/pages/events/EventListPage';",
        )
        app_text = app_text.replace(
            "<Route path=\"/devices/:id/edit\" element={<DeviceFormPage />} />",
            "<Route path=\"/devices/:id/edit\" element={<DeviceFormPage />} />\n"
            "            <Route path=\"/events\" element={<EventListPage />} />",
        )
        app.write_text(app_text, encoding="utf-8", newline="\n")
        print("patch App.tsx")

    # patch MainLayout
    layout = FE / "layouts" / "MainLayout.tsx"
    lt = layout.read_text(encoding="utf-8")
    if "NotificationBell" not in lt:
        lt = lt.replace(
            "  MedicineBoxOutlined,\n  UserOutlined,",
            "  AlertOutlined,\n  MedicineBoxOutlined,\n  UserOutlined,",
        )
        lt = lt.replace(
            "import { useAuthStore } from '@/store/authStore';",
            "import { useAuthStore } from '@/store/authStore';\nimport { NotificationBell } from '@/components/NotificationBell';",
        )
        lt = lt.replace(
            "const selected = location.pathname.startsWith('/devices')",
            "const selected = location.pathname.startsWith('/events')\n    ? '/events'\n    : location.pathname.startsWith('/devices')",
        )
        lt = lt.replace(
            "            {\n              key: '/devices',",
            "            {\n              key: '/events',\n              icon: <AlertOutlined />,\n              label: <Link to=\"/events\">Sự cố / yêu cầu</Link>,\n            },\n            {\n              key: '/devices',",
        )
        lt = lt.replace(
            "<div style={{ display: 'flex', alignItems: 'center' }}>",
            "<div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>\n              <NotificationBell />",
        )
        layout.write_text(lt, encoding="utf-8", newline="\n")
        print("patch MainLayout")

    # patch DeviceDetailPage
    detail = FE / "pages" / "devices" / "DeviceDetailPage.tsx"
    dt = detail.read_text(encoding="utf-8")
    if "DeviceReportModal" not in dt:
        dt = dt.replace(
            "import { EditOutlined, FileOutlined, RollbackOutlined, UploadOutlined } from '@ant-design/icons';",
            "import {\n  EditOutlined,\n  FileOutlined,\n  RollbackOutlined,\n  UploadOutlined,\n  WarningOutlined,\n} from '@ant-design/icons';",
        )
        dt = dt.replace(
            "import { LoadingCenter } from '@/components/LoadingCenter';",
            "import { LoadingCenter } from '@/components/LoadingCenter';\nimport { DeviceReportModal } from '@/components/DeviceReportModal';",
        )
        dt = dt.replace(
            "export function DeviceDetailPage() {",
            "export function DeviceDetailPage() {\n  const [reportOpen, setReportOpen] = useState(false);",
        )
        dt = dt.replace(
            "import { Link, useParams } from 'react-router-dom';",
            "import { useState } from 'react';\nimport { Link, useParams } from 'react-router-dom';",
        )
        dt = dt.replace(
            "<Link to={`/devices/${deviceId}/edit`}>",
            "<Button icon={<WarningOutlined />} onClick={() => setReportOpen(true)}>\n              Báo sự cố\n            </Button>\n            <Link to={`/devices/${deviceId}/edit`}>",
        )
        dt = dt.replace(
            "    </div>\n  );\n}\n",
            "      <DeviceReportModal\n        open={reportOpen}\n        deviceId={deviceId}\n        onClose={() => setReportOpen(false)}\n      />\n    </div>\n  );\n}\n",
        )
        detail.write_text(dt, encoding="utf-8", newline="\n")
        print("patch DeviceDetailPage")

    print("frontend phase2 done")
