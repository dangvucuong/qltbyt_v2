import { Button, Card, Form, Input, Modal, Popconfirm, Select, Space, Table, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, OrganizationItem } from '@/types/api';
import type { PersonnelItem } from '@/types/phase2';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

export function PersonnelPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<PersonnelItem | null>(null);
  const [form] = Form.useForm();

  const listQuery = useQuery({
    queryKey: ['personnel'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<PersonnelItem[]>>('/api/personnel');
      return res.data.data ?? [];
    },
  });

  const orgsQuery = useQuery({
    queryKey: ['organizations'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<OrganizationItem[]>>('/api/organizations');
      return res.data.data ?? [];
    },
  });

  const saveMutation = useMutation({
    mutationFn: async (values: Partial<PersonnelItem>) => {
      if (editing) {
        await axiosClient.put(`/api/personnel/${editing.id}`, values);
      } else {
        await axiosClient.post('/api/personnel', values);
      }
    },
    onSuccess: () => {
      message.success('Lưu nhân sự thành công');
      setOpen(false);
      setEditing(null);
      form.resetFields();
      queryClient.invalidateQueries({ queryKey: ['personnel'] });
    },
    onError: () => message.error('Lưu thất bại'),
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => axiosClient.delete(`/api/personnel/${id}`),
    onSuccess: () => {
      message.success('Đã xóa');
      queryClient.invalidateQueries({ queryKey: ['personnel'] });
    },
    onError: () => message.error('Xóa thất bại'),
  });

  if (listQuery.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader
        title="Nhân sự"
        subtitle="Quản lý nhân sự theo đơn vị"
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
            Thêm nhân sự
          </Button>
        }
      />
      <Card className="app-card">
        <Table
          rowKey="id"
          dataSource={listQuery.data ?? []}
          columns={[
            { title: 'Họ tên', dataIndex: 'fullName' },
            { title: 'Đơn vị', dataIndex: 'organizationName' },
            { title: 'Điện thoại', dataIndex: 'phone' },
            { title: 'Email', dataIndex: 'email' },
            {
              title: '',
              render: (_, r) => (
                <Space>
                  <Button
                    size="small"
                    onClick={() => {
                      setEditing(r);
                      form.setFieldsValue(r);
                      setOpen(true);
                    }}
                  >
                    Sửa
                  </Button>
                  <Popconfirm title="Xóa?" onConfirm={() => deleteMutation.mutate(r.id)}>
                    <Button size="small" danger>Xóa</Button>
                  </Popconfirm>
                </Space>
              ),
            },
          ]}
        />
      </Card>
      <Modal
        title={editing ? 'Sửa nhân sự' : 'Thêm nhân sự'}
        open={open}
        onCancel={() => setOpen(false)}
        onOk={() => form.submit()}
        confirmLoading={saveMutation.isPending}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={(v) => saveMutation.mutate(v)}>
          <Form.Item name="fullName" label="Họ tên" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="identityNumber" label="CMND/CCCD">
            <Input />
          </Form.Item>
          <Form.Item name="organizationId" label="Đơn vị">
            <Select
              allowClear
              options={(orgsQuery.data ?? []).map((o) => ({ value: o.id, label: o.name }))}
            />
          </Form.Item>
          <Form.Item name="phone" label="Điện thoại">
            <Input />
          </Form.Item>
          <Form.Item name="email" label="Email">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
