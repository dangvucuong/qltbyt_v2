import { Button, Card, Form, Input, Modal, Popconfirm, Select, Space, Table, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, MasterItem, OrganizationItem } from '@/types/api';
import type { OrganizationDetail } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

type FormValues = {
  name: string;
  typeId?: number;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
};

export function OrganizationsPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<OrganizationItem | null>(null);
  const [form] = Form.useForm<FormValues>();

  const listQuery = useQuery({
    queryKey: ['organizations'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<OrganizationItem[]>>('/api/organizations');
      return res.data.data ?? [];
    },
  });

  const orgTypesQuery = useQuery({
    queryKey: ['org-types'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<MasterItem[]>>('/api/master/org-types');
      return res.data.data ?? [];
    },
  });

  const saveMutation = useMutation({
    mutationFn: async (values: FormValues) => {
      if (editing) {
        await axiosClient.put(`/api/organizations/${editing.id}`, values);
      } else {
        await axiosClient.post('/api/organizations', values);
      }
    },
    onSuccess: () => {
      message.success('Lưu đơn vị thành công');
      setOpen(false);
      setEditing(null);
      form.resetFields();
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Lưu thất bại');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => axiosClient.delete(`/api/organizations/${id}`),
    onSuccess: () => {
      message.success('Đã xóa đơn vị');
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Xóa thất bại');
    },
  });

  const openEdit = async (row: OrganizationItem) => {
    setEditing(row);
    const res = await axiosClient.get<ApiResponse<OrganizationDetail>>(`/api/organizations/${row.id}`);
    const detail = res.data.data;
    form.setFieldsValue({
      name: detail?.name ?? row.name,
      typeId: detail?.typeId ?? row.typeId,
      phone: detail?.phone,
      email: detail?.email,
      address: detail?.address,
      taxCode: detail?.taxCode,
    });
    setOpen(true);
  };

  if (listQuery.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader
        title="Đơn vị / Khoa phòng"
        subtitle="Quản lý đơn vị sử dụng thiết bị"
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
            Thêm đơn vị
          </Button>
        }
      />
      <Card className="app-card">
        <Table
          rowKey="id"
          dataSource={listQuery.data ?? []}
          columns={[
            { title: 'Tên đơn vị', dataIndex: 'name' },
            { title: 'Loại', dataIndex: 'typeName' },
            { title: 'Điện thoại', dataIndex: 'phone' },
            { title: 'Email', dataIndex: 'email' },
            {
              title: '',
              width: 160,
              render: (_, r) => (
                <Space>
                  <Button size="small" onClick={() => openEdit(r)}>Sửa</Button>
                  <Popconfirm title="Xóa đơn vị này?" onConfirm={() => deleteMutation.mutate(r.id)}>
                    <Button size="small" danger loading={deleteMutation.isPending}>Xóa</Button>
                  </Popconfirm>
                </Space>
              ),
            },
          ]}
        />
      </Card>
      <Modal
        open={open}
        title={editing ? 'Cập nhật đơn vị' : 'Thêm đơn vị'}
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
          <Form.Item name="name" label="Tên đơn vị" rules={[{ required: true, message: 'Nhập tên' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="typeId" label="Loại đơn vị">
            <Select
              allowClear
              options={(orgTypesQuery.data ?? []).map((t) => ({ value: t.id, label: t.name }))}
            />
          </Form.Item>
          <Form.Item name="phone" label="Điện thoại">
            <Input />
          </Form.Item>
          <Form.Item name="email" label="Email">
            <Input />
          </Form.Item>
          <Form.Item name="address" label="Địa chỉ">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="taxCode" label="Mã số thuế">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
