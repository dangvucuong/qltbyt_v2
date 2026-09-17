import { Button, Card, Form, Input, Modal, Popconfirm, Select, Space, Table, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { CreateUserPayload, UpdateUserPayload, UserAccountItem } from '@/types/phase2';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

const ACCOUNT_TYPES = [
  { value: 1, label: 'Quản trị (Admin)' },
  { value: 2, label: 'Kỹ thuật' },
  { value: 3, label: 'Nhân viên khoa' },
];

/** Legacy LoaiTK=0 cũng là Admin; chuẩn hóa để khớp option Select. */
function normalizeAccountType(value?: number | string | null): number | undefined {
  if (value === undefined || value === null || value === '') return undefined;
  const n = Number(value);
  if (Number.isNaN(n)) return undefined;
  if (n <= 1) return 1;
  if (n === 2) return 2;
  return 3;
}

function accountTypeLabel(value?: number | string | null): string {
  const n = normalizeAccountType(value);
  return ACCOUNT_TYPES.find((t) => t.value === n)?.label ?? String(value ?? '');
}

export function UsersPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<UserAccountItem | null>(null);
  const [form] = Form.useForm();

  const usersQuery = useQuery({
    queryKey: ['users'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<UserAccountItem[]>>('/api/users');
      return res.data.data ?? [];
    },
  });

  const saveMutation = useMutation({
    mutationFn: async (values: CreateUserPayload & { newPassword?: string }) => {
      if (editing) {
        const payload: UpdateUserPayload = {
          fullName: values.fullName,
          phone: values.phone,
          email: values.email,
          accountType: values.accountType,
          organizationIds: values.organizationIds,
          newPassword: values.newPassword,
        };
        await axiosClient.put(`/api/users/${editing.id}`, payload);
      } else {
        await axiosClient.post('/api/users', values);
      }
    },
    onSuccess: () => {
      message.success('Lưu tài khoản thành công');
      setOpen(false);
      setEditing(null);
      form.resetFields();
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: () => message.error('Lưu thất bại'),
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => axiosClient.delete(`/api/users/${id}`),
    onSuccess: () => {
      message.success('Đã xóa tài khoản');
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: () => message.error('Xóa thất bại'),
  });

  if (usersQuery.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader
        title="Tài khoản"
        subtitle="Quản lý đăng nhập và phân quyền"
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditing(null);
              form.resetFields();
              setOpen(true);
            }}
          >
            Thêm tài khoản
          </Button>
        }
      />
      <Card className="app-card">
        <Table
          rowKey="id"
          dataSource={usersQuery.data ?? []}
          columns={[
            { title: 'Đăng nhập', dataIndex: 'username' },
            { title: 'Họ tên', dataIndex: 'fullName' },
            { title: 'Email', dataIndex: 'email' },
            { title: 'Loại TK', dataIndex: 'accountType', render: (v) => accountTypeLabel(v) },
            { title: 'Role', dataIndex: 'role' },
            {
              title: '',
              render: (_, r) => (
                <Space>
                  <Button
                    size="small"
                    onClick={() => {
                      setEditing(r);
                      form.setFieldsValue({
                        ...r,
                        accountType: normalizeAccountType(r.accountType),
                      });
                      setOpen(true);
                    }}
                  >
                    Sửa
                  </Button>
                  <Popconfirm title="Xóa tài khoản?" onConfirm={() => deleteMutation.mutate(r.id)}>
                    <Button size="small" danger>Xóa</Button>
                  </Popconfirm>
                </Space>
              ),
            },
          ]}
        />
      </Card>
      <Modal
        title={editing ? 'Sửa tài khoản' : 'Thêm tài khoản'}
        open={open}
        onCancel={() => setOpen(false)}
        onOk={() => form.submit()}
        confirmLoading={saveMutation.isPending}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={(v) => saveMutation.mutate(v)}>
          {!editing && (
            <>
              <Form.Item name="username" label="Tên đăng nhập" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
              <Form.Item name="password" label="Mật khẩu" rules={[{ required: true }]}>
                <Input.Password />
              </Form.Item>
            </>
          )}
          <Form.Item name="fullName" label="Họ tên">
            <Input />
          </Form.Item>
          <Form.Item name="email" label="Email">
            <Input />
          </Form.Item>
          <Form.Item name="phone" label="Điện thoại">
            <Input />
          </Form.Item>
          <Form.Item name="accountType" label="Loại tài khoản" rules={[{ required: true }]}>
            <Select options={ACCOUNT_TYPES} optionFilterProp="label" />
          </Form.Item>
          <Form.Item name="organizationIds" label="Mã đơn vị (MaDV)">
            <Input placeholder="VD: 1 hoặc 1;2" />
          </Form.Item>
          {editing && (
            <Form.Item name="newPassword" label="Mật khẩu mới">
              <Input.Password />
            </Form.Item>
          )}
        </Form>
      </Modal>
    </div>
  );
}
