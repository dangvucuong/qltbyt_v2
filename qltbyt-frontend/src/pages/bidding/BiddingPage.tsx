import {
  Button,
  Card,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Space,
  Table,
  message,
} from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceListItem } from '@/types/api';
import type { BiddingPackageItem, CreateBiddingPayload } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { useAuthStore } from '@/store/authStore';

const biddingWrapCell = {
  style: {
    whiteSpace: 'normal' as const,
    wordBreak: 'break-word' as const,
    lineHeight: 1.5,
    verticalAlign: 'top' as const,
  },
  className: 'bidding-table-wrap-cell',
};

/** Ba cột mô tả: cùng độ rộng + word wrap */
const BIDDING_WRAP_COL_WIDTH = 280;

export function BiddingPage() {
  const role = useAuthStore((s) => s.user?.role);
  const isAdmin = role === 'Admin';
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<BiddingPackageItem | null>(null);
  const [devicesOpen, setDevicesOpen] = useState<BiddingPackageItem | null>(null);
  const [form] = Form.useForm<CreateBiddingPayload>();

  const listQuery = useQuery({
    queryKey: ['bidding'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<BiddingPackageItem[]>>('/api/bidding');
      return res.data.data ?? [];
    },
  });

  const devicesQuery = useQuery({
    queryKey: ['bidding-devices', devicesOpen?.id],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceListItem[]>>(
        `/api/bidding/${devicesOpen!.id}/devices`,
      );
      return res.data.data ?? [];
    },
    enabled: devicesOpen != null,
  });

  const saveMutation = useMutation({
    mutationFn: async (values: CreateBiddingPayload) => {
      if (editing) {
        await axiosClient.put(`/api/bidding/${editing.id}`, values);
      } else {
        await axiosClient.post('/api/bidding', values);
      }
    },
    onSuccess: () => {
      message.success('Lưu gói thầu thành công');
      setOpen(false);
      setEditing(null);
      form.resetFields();
      queryClient.invalidateQueries({ queryKey: ['bidding'] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Lưu thất bại');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => axiosClient.delete(`/api/bidding/${id}`),
    onSuccess: () => {
      message.success('Đã xóa gói thầu');
      queryClient.invalidateQueries({ queryKey: ['bidding'] });
    },
    onError: () => message.error('Xóa thất bại'),
  });

  if (listQuery.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader
        title="Gói thầu"
        subtitle="Hồ sơ gói thầu và thiết bị liên kết"
        extra={
          isAdmin ? (
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => {
                setEditing(null);
                form.resetFields();
                setOpen(true);
              }}
            >
              Thêm gói thầu
            </Button>
          ) : null
        }
      />
      <Card className="app-card">
        <Table
          rowKey="id"
          className="bidding-packages-table"
          tableLayout="fixed"
          dataSource={listQuery.data ?? []}
          columns={[
            { title: 'Mã gói', dataIndex: 'packageCode', width: 130, ellipsis: true },
            {
              title: 'Tên gói thầu',
              dataIndex: 'name',
              width: BIDDING_WRAP_COL_WIDTH,
              onCell: () => biddingWrapCell,
              onHeaderCell: () => biddingWrapCell,
            },
            {
              title: 'Trạng thái',
              dataIndex: 'status',
              width: BIDDING_WRAP_COL_WIDTH,
              onCell: () => biddingWrapCell,
              onHeaderCell: () => biddingWrapCell,
            },
            {
              title: 'Đơn vị trúng thầu',
              dataIndex: 'winningVendor',
              width: BIDDING_WRAP_COL_WIDTH,
              onCell: () => biddingWrapCell,
              onHeaderCell: () => biddingWrapCell,
            },
            {
              title: 'Giá trị (VND)',
              dataIndex: 'value',
              width: 140,
              render: (v) => (v != null ? Number(v).toLocaleString('vi-VN') : '—'),
            },
            {
              title: 'Số TB',
              dataIndex: 'deviceCount',
              width: 80,
              render: (n, r) => (
                <Button type="link" size="small" onClick={() => setDevicesOpen(r)}>
                  {n}
                </Button>
              ),
            },
            ...(isAdmin
              ? [
                  {
                    title: '',
                    width: 160,
                    render: (_: unknown, r: BiddingPackageItem) => (
                      <Space>
                        <Button
                          size="small"
                          onClick={() => {
                            setEditing(r);
                            form.setFieldsValue({
                              packageCode: r.packageCode,
                              name: r.name,
                              status: r.status,
                              winningVendor: r.winningVendor,
                              taxCode: r.taxCode,
                              contractNumber: r.contractNumber,
                              value: r.value,
                            });
                            setOpen(true);
                          }}
                        >
                          Sửa
                        </Button>
                        <Popconfirm title="Xóa gói thầu?" onConfirm={() => deleteMutation.mutate(r.id)}>
                          <Button size="small" danger>Xóa</Button>
                        </Popconfirm>
                      </Space>
                    ),
                  },
                ]
              : []),
          ]}
        />
      </Card>
      <Modal
        open={open}
        title={editing ? 'Cập nhật gói thầu' : 'Thêm gói thầu'}
        okText="Lưu"
        cancelText="Hủy"
        confirmLoading={saveMutation.isPending}
        onCancel={() => {
          setOpen(false);
          setEditing(null);
          form.resetFields();
        }}
        onOk={() => form.submit()}
      >
        <Form form={form} layout="vertical" onFinish={(v) => saveMutation.mutate(v)}>
          {!editing && (
            <Form.Item
              name="packageCode"
              label="Mã gói thầu"
              rules={[{ required: true, message: 'Nhập mã gói' }]}
            >
              <Input placeholder="VD: GT-2024-01" />
            </Form.Item>
          )}
          <Form.Item name="name" label="Tên gói thầu">
            <Input />
          </Form.Item>
          <Form.Item name="status" label="Tình trạng">
            <Input placeholder="VD: Đang thực hiện" />
          </Form.Item>
          <Form.Item name="winningVendor" label="Đơn vị trúng thầu">
            <Input />
          </Form.Item>
          <Form.Item name="taxCode" label="Mã số thuế">
            <Input />
          </Form.Item>
          <Form.Item name="contractNumber" label="Số hợp đồng">
            <Input />
          </Form.Item>
          <Form.Item name="value" label="Giá trị (VND)">
            <InputNumber style={{ width: '100%' }} min={0} />
          </Form.Item>
        </Form>
      </Modal>
      <Modal
        open={devicesOpen != null}
        title={`Thiết bị trong gói: ${devicesOpen?.name ?? devicesOpen?.packageCode ?? ''}`}
        footer={null}
        onCancel={() => setDevicesOpen(null)}
        width={720}
      >
        <Table
          rowKey="id"
          loading={devicesQuery.isLoading}
          dataSource={devicesQuery.data ?? []}
          pagination={false}
          columns={[
            {
              title: 'Mã TB',
              dataIndex: 'deviceCode',
              render: (c, row) => <Link to={`/devices/${row.id}`}>{c}</Link>,
            },
            { title: 'Tên', dataIndex: 'name' },
            { title: 'Đơn vị', dataIndex: 'organizationName' },
          ]}
        />
      </Modal>
    </div>
  );
}
