import { Button, Card, Form, Input, InputNumber, Modal, Popconfirm, Space, Table, Upload, message } from 'antd';
import { PlusOutlined, UploadOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { CreateLegalDocumentPayload, LegalDocumentItem } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { useAuthStore } from '@/store/authStore';

function fileUrl(path?: string) {
  if (!path) return '#';
  const normalized = path.replace(/^FILELUU[/\\]/i, '').replace(/\\/g, '/');
  return `/files/${normalized}`;
}

export function LegalDocumentsPage() {
  const role = useAuthStore((s) => s.user?.role);
  const canMaintain = role === 'Admin' || role === 'Technician';
  const isAdmin = role === 'Admin';
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<LegalDocumentItem | null>(null);
  const [form] = Form.useForm<CreateLegalDocumentPayload>();

  const listQuery = useQuery({
    queryKey: ['legal-documents'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<LegalDocumentItem[]>>('/api/legal-documents');
      return res.data.data ?? [];
    },
  });

  const saveMutation = useMutation({
    mutationFn: async (values: CreateLegalDocumentPayload) => {
      if (editing) {
        await axiosClient.put(`/api/legal-documents/${editing.id}`, values);
      } else {
        await axiosClient.post('/api/legal-documents', values);
      }
    },
    onSuccess: () => {
      message.success('Lưu hồ sơ pháp lý thành công');
      setOpen(false);
      setEditing(null);
      form.resetFields();
      queryClient.invalidateQueries({ queryKey: ['legal-documents'] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Lưu thất bại');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => axiosClient.delete(`/api/legal-documents/${id}`),
    onSuccess: () => {
      message.success('Đã xóa hồ sơ');
      queryClient.invalidateQueries({ queryKey: ['legal-documents'] });
    },
    onError: () => message.error('Xóa thất bại (chỉ Admin)'),
  });

  const uploadMutation = useMutation({
    mutationFn: async ({ id, file }: { id: number; file: File }) => {
      const formData = new FormData();
      formData.append('file', file);
      await axiosClient.post(`/api/legal-documents/${id}/file`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    onSuccess: () => {
      message.success('Tải file thành công');
      queryClient.invalidateQueries({ queryKey: ['legal-documents'] });
    },
    onError: () => message.error('Tải file thất bại'),
  });

  if (listQuery.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader
        title="Hồ sơ pháp lý"
        subtitle="Thông tin hồ sơ ThongtinHS và file đính kèm"
        extra={
          canMaintain ? (
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => {
                setEditing(null);
                form.resetFields();
                setOpen(true);
              }}
            >
              Thêm hồ sơ
            </Button>
          ) : null
        }
      />
      <Card className="app-card">
        <Table
          rowKey="id"
          dataSource={listQuery.data ?? []}
          columns={[
            { title: 'Giấy kiểm định', dataIndex: 'inspectionCertificate', width: 140 },
            { title: 'Giấy nhập khẩu', dataIndex: 'importPermit', width: 140 },
            { title: 'Mã TB', dataIndex: 'deviceCodes', ellipsis: true },
            {
              title: 'Giá VAT',
              dataIndex: 'priceWithVat',
              width: 120,
              render: (v) => (v != null ? Number(v).toLocaleString('vi-VN') : '—'),
            },
            {
              title: 'File',
              dataIndex: 'filePath',
              width: 100,
              render: (p) =>
                p ? (
                  <a href={fileUrl(p)} target="_blank" rel="noreferrer">Tải</a>
                ) : (
                  '—'
                ),
            },
            ...(canMaintain
              ? [
                  {
                    title: '',
                    width: 220,
                    render: (_: unknown, r: LegalDocumentItem) => (
                      <Space>
                        <Upload
                          showUploadList={false}
                          beforeUpload={(file) => {
                            uploadMutation.mutate({ id: r.id, file });
                            return false;
                          }}
                        >
                          <Button size="small" icon={<UploadOutlined />}>File</Button>
                        </Upload>
                        <Button
                          size="small"
                          onClick={() => {
                            setEditing(r);
                            form.setFieldsValue({
                              inspectionCertificate: r.inspectionCertificate,
                              importPermit: r.importPermit,
                              circulationPermit: r.circulationPermit,
                              biddingInfo: r.biddingInfo,
                              priceWithVat: r.priceWithVat,
                              deviceCodes: r.deviceCodes,
                            });
                            setOpen(true);
                          }}
                        >
                          Sửa
                        </Button>
                        {isAdmin && (
                          <Popconfirm title="Xóa hồ sơ?" onConfirm={() => deleteMutation.mutate(r.id)}>
                            <Button size="small" danger>Xóa</Button>
                          </Popconfirm>
                        )}
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
        title={editing ? 'Cập nhật hồ sơ' : 'Thêm hồ sơ pháp lý'}
        okText="Lưu"
        cancelText="Hủy"
        confirmLoading={saveMutation.isPending}
        onCancel={() => {
          setOpen(false);
          setEditing(null);
          form.resetFields();
        }}
        onOk={() => form.submit()}
        width={640}
      >
        <Form form={form} layout="vertical" onFinish={(v) => saveMutation.mutate(v)}>
          <Form.Item name="inspectionCertificate" label="Mã giấy kiểm định">
            <Input />
          </Form.Item>
          <Form.Item name="importPermit" label="Mã giấy nhập khẩu">
            <Input />
          </Form.Item>
          <Form.Item name="circulationPermit" label="Mã giấy phép lưu hành">
            <Input />
          </Form.Item>
          <Form.Item name="biddingInfo" label="Thông tin gói thầu">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="priceWithVat" label="Giá có VAT (VND)">
            <InputNumber style={{ width: '100%' }} min={0} />
          </Form.Item>
          <Form.Item name="deviceCodes" label="Mã thiết bị (phẩy)">
            <Input placeholder="TB-DEMO-001, TB-DEMO-002" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}