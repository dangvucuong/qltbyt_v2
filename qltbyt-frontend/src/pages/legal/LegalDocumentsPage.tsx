import {
  Button,
  Card,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Select,
  Space,
  Table,
  Upload,
  message,
} from 'antd';
import { PlusOutlined, UploadOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceListItem, PagedResult } from '@/types/api';
import type { CreateLegalDocumentPayload, LegalDocumentItem } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { useAuthStore } from '@/store/authStore';
import { apiUrl } from '@/utils/apiBase';

function fileUrl(path?: string) {
  if (!path) return '#';
  const normalized = path.replace(/^FILELUU[/\\]?/i, '').replace(/\\/g, '/');
  return apiUrl(`/files/${normalized}`);
}

function cell(v?: string | null) {
  const t = v?.trim();
  return t ? t : '—';
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

  const devicesQuery = useQuery({
    queryKey: ['devices-picker-legal'],
    enabled: open,
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<PagedResult<DeviceListItem>>>(
        '/api/devices?page=1&pageSize=100&sortBy=name',
      );
      return res.data.data?.items ?? [];
    },
  });

  const deviceOptions = useMemo(
    () =>
      (devicesQuery.data ?? []).map((d) => ({
        value: String(d.id),
        label: d.deviceCode ? `${d.deviceCode} — ${d.name}` : `${d.name} (ID ${d.id})`,
      })),
    [devicesQuery.data],
  );

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
        subtitle="Bảng ThongtinHS (tương đương thongtinhs.aspx). Cột thiết bị hiển thị tên từ ID như hệ cũ."
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
          size="small"
          dataSource={listQuery.data ?? []}
          scroll={{ x: 1400 }}
          pagination={{ pageSize: 20, showTotal: (t) => `Tổng ${t} hồ sơ` }}
          columns={[
            { title: 'ID', dataIndex: 'id', width: 70 },
            {
              title: 'Thiết bị',
              width: 220,
              ellipsis: true,
              render: (_, r) => cell(r.deviceDisplayName || r.deviceCode || (r.deviceCodes !== '0' ? r.deviceCodes : undefined)),
            },
            {
              title: 'Giấy kiểm định',
              dataIndex: 'inspectionCertificate',
              width: 140,
              render: (v: string) => cell(v),
            },
            {
              title: 'Giấy nhập khẩu',
              dataIndex: 'importPermit',
              width: 140,
              render: (v: string) => cell(v),
            },
            {
              title: 'Hạn NK',
              dataIndex: 'importPermitExpiry',
              width: 120,
              render: (v: string) => cell(v),
            },
            {
              title: 'GP lưu hành',
              dataIndex: 'circulationPermit',
              width: 120,
              render: (v: string) => cell(v),
            },
            {
              title: 'Gói thầu',
              dataIndex: 'biddingInfo',
              width: 140,
              ellipsis: true,
              render: (v: string) => cell(v),
            },
            {
              title: 'Giá VAT',
              dataIndex: 'priceWithVat',
              width: 120,
              render: (v) => (v != null ? Number(v).toLocaleString('vi-VN') : '—'),
            },
            {
              title: 'File',
              dataIndex: 'filePath',
              width: 80,
              render: (p) =>
                p ? (
                  <a href={fileUrl(p)} target="_blank" rel="noreferrer">
                    Tải
                  </a>
                ) : (
                  '—'
                ),
            },
            ...(canMaintain
              ? [
                  {
                    title: '',
                    width: 220,
                    fixed: 'right' as const,
                    render: (_: unknown, r: LegalDocumentItem) => (
                      <Space>
                        <Upload
                          showUploadList={false}
                          beforeUpload={(file) => {
                            uploadMutation.mutate({ id: r.id, file });
                            return false;
                          }}
                        >
                          <Button size="small" icon={<UploadOutlined />}>
                            File
                          </Button>
                        </Upload>
                        <Button
                          size="small"
                          onClick={() => {
                            setEditing(r);
                            form.setFieldsValue({
                              inspectionCertificate: r.inspectionCertificate,
                              importPermit: r.importPermit,
                              importPermitExpiry: r.importPermitExpiry,
                              circulationPermit: r.circulationPermit,
                              circulationPermitExpiry: r.circulationPermitExpiry,
                              certificateOfOrigin: r.certificateOfOrigin,
                              qualityCertificate: r.qualityCertificate,
                              manufactureYear: r.manufactureYear,
                              customsDeclaration: r.customsDeclaration,
                              packingList: r.packingList,
                              billOfLading: r.billOfLading,
                              importInvoice: r.importInvoice,
                              salesInvoice: r.salesInvoice,
                              biddingInfo: r.biddingInfo,
                              priceWithVat: r.priceWithVat,
                              priceWithoutVat: r.priceWithoutVat,
                              depreciationInfo: r.depreciationInfo,
                              fundingSource: r.fundingSource,
                              deviceCodes:
                                r.deviceCodes && r.deviceCodes !== '0' ? r.deviceCodes : undefined,
                            });
                            setOpen(true);
                          }}
                        >
                          Sửa
                        </Button>
                        {isAdmin && (
                          <Popconfirm title="Xóa hồ sơ?" onConfirm={() => deleteMutation.mutate(r.id)}>
                            <Button size="small" danger>
                              Xóa
                            </Button>
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
        width={720}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={(v) => saveMutation.mutate(v)}>
          <Form.Item
            name="deviceCodes"
            label="Thiết bị"
            tooltip="Hệ cũ lưu ID thiết bị vào cột MaThietbi (không phải mã MaTB)."
          >
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="Chọn thiết bị"
              options={deviceOptions}
              loading={devicesQuery.isLoading}
            />
          </Form.Item>
          <Form.Item name="inspectionCertificate" label="Mã giấy kiểm định chất lượng">
            <Input />
          </Form.Item>
          <Form.Item name="importPermit" label="Mã giấy phép nhập khẩu">
            <Input />
          </Form.Item>
          <Form.Item name="importPermitExpiry" label="Thời hạn giấy phép nhập khẩu">
            <Input placeholder="dd/MM/yyyy hoặc text" />
          </Form.Item>
          <Form.Item name="circulationPermit" label="Mã giấy phép lưu hành">
            <Input />
          </Form.Item>
          <Form.Item name="circulationPermitExpiry" label="Thời hạn giấy phép lưu hành">
            <Input />
          </Form.Item>
          <Form.Item name="biddingInfo" label="Thông tin gói thầu">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="priceWithVat" label="Giá có VAT (VND)">
            <InputNumber style={{ width: '100%' }} min={0} />
          </Form.Item>
          <Form.Item name="priceWithoutVat" label="Giá trừ VAT (VND)">
            <InputNumber style={{ width: '100%' }} min={0} />
          </Form.Item>
          <Form.Item name="fundingSource" label="Nguồn vốn">
            <Input />
          </Form.Item>
          <Form.Item name="depreciationInfo" label="Thông tin khấu hao">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
