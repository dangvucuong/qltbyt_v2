import {
  Button,
  Card,
  Descriptions,
  Empty,
  Form,
  Input,
  InputNumber,
  List,
  Space,
  Table,
  Tabs,
  Upload,
  message,
} from 'antd';
import {
  EditOutlined,
  FileOutlined,
  RollbackOutlined,
  SwapOutlined,
  UploadOutlined,
  WarningOutlined,
  PrinterOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { useState, useEffect } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceDetail, DeviceListItem, OrganizationItem } from '@/types/api';
import type { DeviceHistoryItem, DeviceSupplyItem, LegalDocumentItem } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { DeviceReportModal } from '@/components/DeviceReportModal';
import { TransferDeviceModal } from '@/components/TransferDeviceModal';
import { DeviceCameraCapture } from '@/components/DeviceCameraCapture';
import { DeviceUsageLogsPanel } from '@/components/DeviceUsageLogsPanel';
import { useAuthStore } from '@/store/authStore';
import { openPrintHtml } from '@/utils/openPrintHtml';
import { apiUrl } from '@/utils/apiBase';

function fileUrl(path?: string) {
  if (!path) return '#';
  const normalized = path.replace(/\\/g, '/');
  if (/^Captures\//i.test(normalized)) {
    return apiUrl(`/captures/${normalized.replace(/^Captures\//i, '')}`);
  }
  if (/^HDBaotri\//i.test(normalized)) {
    return apiUrl(`/hdbaotri/${normalized.replace(/^HDBaotri\//i, '')}`);
  }
  const files = normalized.replace(/^FILELUU[/\\]/i, '');
  return apiUrl(`/files/${files}`);
}

function maintenanceContractUrl(deviceId: number, fileName?: string) {
  if (!fileName?.trim()) return undefined;
  return apiUrl(`/hdbaotri/${deviceId}/${encodeURIComponent(fileName.trim())}`);
}

const PROCUREMENT_LABELS: Record<string, string> = {
  '1': 'Nguồn dự án',
  '2': 'Quỹ phát triển sự nghiệp',
  '3': 'Tài trợ - viện trợ',
  '4': 'Nguồn ngân sách',
  '5': 'Nguồn vay kích cầu',
};

const MAINTENANCE_LABELS: Record<number, string> = {
  1: 'Tự bảo trì',
  2: 'Bảo trì cơ bản',
  3: 'Bảo trì trọn gói',
  4: 'Bảo trì cơ bản có thay thế linh kiện',
};

function formatMarker(raw?: string) {
  if (!raw?.trim()) return '—';
  const d = dayjs(raw);
  return d.isValid() ? d.format('DD/MM/YYYY') : raw;
}

function formatMoney(raw?: string) {
  if (!raw?.trim()) return '—';
  const n = Number(String(raw).replace(/,/g, ''));
  return Number.isFinite(n) ? n.toLocaleString('vi-VN') : raw;
}

export function DeviceDetailPage() {
  const [reportOpen, setReportOpen] = useState(false);
  const [transferOpen, setTransferOpen] = useState(false);
  const { id } = useParams();
  const deviceId = Number(id);
  const queryClient = useQueryClient();
  const role = useAuthStore((s) => s.user?.role);
  const canMaintain = role === 'Admin' || role === 'Technician';
  const [qrForm] = Form.useForm();
  const [supplyForm] = Form.useForm();
  const [childForm] = Form.useForm();

  const deviceQuery = useQuery({
    queryKey: ['device', deviceId],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceDetail>>(`/api/devices/${deviceId}`);
      return res.data.data!;
    },
    enabled: deviceId > 0,
  });

  const childrenQuery = useQuery({
    queryKey: ['device-children', deviceId],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceListItem[]>>(
        `/api/devices/${deviceId}/children`,
      );
      return res.data.data ?? [];
    },
    enabled: deviceId > 0,
  });

  const historyQuery = useQuery({
    queryKey: ['device-history', deviceId],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceHistoryItem[]>>(
        `/api/devices/${deviceId}/history`,
      );
      return res.data.data ?? [];
    },
    enabled: deviceId > 0,
  });

  const suppliesQuery = useQuery({
    queryKey: ['device-supplies', deviceId],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceSupplyItem[]>>(
        `/api/devices/${deviceId}/supplies`,
      );
      return res.data.data ?? [];
    },
    enabled: deviceId > 0,
  });

  const legalQuery = useQuery({
    queryKey: ['device-legal', deviceId],
    queryFn: async () => {
      try {
        const res = await axiosClient.get<ApiResponse<LegalDocumentItem>>(
          `/api/devices/${deviceId}/legal-document`,
        );
        return res.data.data ?? null;
      } catch {
        return null;
      }
    },
    enabled: deviceId > 0,
  });

  const orgsQuery = useQuery({
    queryKey: ['organizations'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<OrganizationItem[]>>('/api/organizations');
      return res.data.data ?? [];
    },
    enabled: transferOpen,
  });

  const signPdfMutation = useMutation({
    mutationFn: async (fileId: number) => {
      await axiosClient.post(`/api/devices/files/${fileId}/sign-pdf`);
    },
    onSuccess: () => {
      message.success('Ký số PDF thành công');
      queryClient.invalidateQueries({ queryKey: ['device', deviceId] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Ký số thất bại');
    },
  });

  const uploadMutation = useMutation({
    mutationFn: async (file: File) => {
      const form = new FormData();
      form.append('file', file);
      await axiosClient.post(`/api/devices/${deviceId}/files`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    onSuccess: () => {
      message.success('Tải lên tài liệu thành công');
      queryClient.invalidateQueries({ queryKey: ['device', deviceId] });
    },
    onError: () => message.error('Tải lên thất bại'),
  });

  const uploadContractMutation = useMutation({
    mutationFn: async (file: File) => {
      const form = new FormData();
      form.append('file', file);
      await axiosClient.post(`/api/devices/${deviceId}/maintenance-contract`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    onSuccess: () => {
      message.success('Upload hợp đồng bảo trì thành công');
      queryClient.invalidateQueries({ queryKey: ['device', deviceId] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Upload hợp đồng bảo trì thất bại');
    },
  });

  const transferMutation = useMutation({
    mutationFn: async (values: {
      toOrganizationId: number;
      handoverOrganizationId?: number;
      note?: string;
    }) => {
      await axiosClient.post(`/api/devices/${deviceId}/transfer`, values);
    },
    onSuccess: () => {
      message.success('Chuyển giao thiết bị thành công');
      setTransferOpen(false);
      queryClient.invalidateQueries({ queryKey: ['device', deviceId] });
      queryClient.invalidateQueries({ queryKey: ['device-history', deviceId] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Chuyển giao thất bại');
    },
  });

  const qrMutation = useMutation({
    mutationFn: async (values: { qrCode?: string; nfc?: string }) => {
      await axiosClient.put(`/api/devices/${deviceId}/qr`, values);
    },
    onSuccess: () => {
      message.success('Cập nhật mã QR/NFC thành công');
      queryClient.invalidateQueries({ queryKey: ['device', deviceId] });
    },
    onError: () => message.error('Cập nhật thất bại'),
  });

  const addSupplyMutation = useMutation({
    mutationFn: async (values: { name: string; unit?: string; quantity?: number }) => {
      await axiosClient.post(`/api/devices/${deviceId}/supplies`, values);
    },
    onSuccess: () => {
      message.success('Thêm vật tư thành công');
      supplyForm.resetFields();
      queryClient.invalidateQueries({ queryKey: ['device-supplies', deviceId] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Thêm vật tư thất bại');
    },
  });

  const deleteSupplyMutation = useMutation({
    mutationFn: async (supplyId: number) => {
      await axiosClient.delete(`/api/devices/${deviceId}/supplies/${supplyId}`);
    },
    onSuccess: () => {
      message.success('Đã xóa vật tư');
      queryClient.invalidateQueries({ queryKey: ['device-supplies', deviceId] });
    },
    onError: () => message.error('Xóa thất bại'),
  });

  const createChildMutation = useMutation({
    mutationFn: async (values: { deviceCode: string; name: string }) => {
      const parent = deviceQuery.data;
      if (!parent?.deviceCode) throw new Error('no parent');
      await axiosClient.post('/api/devices', {
        deviceCode: values.deviceCode.trim(),
        name: values.name.trim(),
        typeId: parent.typeId ?? 1,
        organizationId: parent.organizationId ?? 1,
        statusId: parent.statusId ?? 1,
        parentDeviceCode: parent.deviceCode,
      });
    },
    onSuccess: () => {
      message.success('Đã thêm thiết bị con');
      childForm.resetFields();
      queryClient.invalidateQueries({ queryKey: ['device-children', deviceId] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Thêm thiết bị con thất bại');
    },
  });

  const uploadLegalFileMutation = useMutation({
    mutationFn: async (file: File) => {
      const legalId = legalQuery.data?.id;
      if (!legalId) throw new Error('no legal');
      const form = new FormData();
      form.append('file', file);
      await axiosClient.post(`/api/legal-documents/${legalId}/file`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    onSuccess: () => {
      message.success('Tải file hồ sơ thành công');
      queryClient.invalidateQueries({ queryKey: ['device-legal', deviceId] });
    },
    onError: () => message.error('Tải file thất bại'),
  });

  const firstCode = deviceQuery.data?.codes?.[0];

  useEffect(() => {
    if (firstCode) {
      qrForm.setFieldsValue({ qrCode: firstCode.qrCode, nfc: firstCode.nfc });
    }
  }, [deviceId, firstCode?.qrCode, firstCode?.nfc, qrForm]);

  if (deviceId <= 0) {
    return (
      <Card className="app-card">
        <Empty description="Mã thiết bị không hợp lệ" />
      </Card>
    );
  }

  if (deviceQuery.isLoading) return <LoadingCenter />;

  if (deviceQuery.isError || !deviceQuery.data) {
    return (
      <Card className="app-card">
        <Empty description="Không tìm thấy thiết bị hoặc không có quyền xem" />
        <Link to="/devices">
          <Button style={{ marginTop: 16 }}>Quay lại danh sách</Button>
        </Link>
      </Card>
    );
  }

  const d = deviceQuery.data;

  return (
    <div>
      <PageHeader
        title={d.name || 'Chi tiết thiết bị'}
        subtitle={d.deviceCode ? `Mã: ${d.deviceCode}` : undefined}
        crumbs={[
          { title: 'Thiết bị', href: '/devices' },
          { title: d.deviceCode || 'Chi tiết' },
        ]}
        extra={
          <Space>
            <Link to="/devices">
              <Button icon={<RollbackOutlined />}>Quay lại</Button>
            </Link>
            {canMaintain && (
              <Button icon={<SwapOutlined />} onClick={() => setTransferOpen(true)}>
                Chuyển giao
              </Button>
            )}
            {canMaintain && (
              <>
                <Button
                  icon={<PrinterOutlined />}
                  onClick={async () => {
                    try {
                      await openPrintHtml(`/api/reports/devices/${deviceId}/print/repair`);
                    } catch {
                      message.error('Không mở được phiếu sửa chữa');
                    }
                  }}
                >
                  In phiếu sửa chữa
                </Button>
                <Button
                  icon={<PrinterOutlined />}
                  onClick={async () => {
                    try {
                      await openPrintHtml(`/api/reports/devices/${deviceId}/print/inspection`);
                    } catch {
                      message.error('Không mở được phiếu kiểm định');
                    }
                  }}
                >
                  In phiếu kiểm định
                </Button>
              </>
            )}
            <Button icon={<WarningOutlined />} onClick={() => setReportOpen(true)}>
              Báo sự cố
            </Button>
            <Link to={`/devices/${deviceId}/edit`}>
              <Button type="primary" icon={<EditOutlined />}>Chỉnh sửa</Button>
            </Link>
          </Space>
        }
      />
      <Card className="app-card" title="Thông tin chung">
        <Descriptions bordered column={{ xs: 1, sm: 2 }} size="middle">
          <Descriptions.Item label="Mã thiết bị">{d.deviceCode || '—'}</Descriptions.Item>
          <Descriptions.Item label="Mã định danh">{d.identityCode || '—'}</Descriptions.Item>
          <Descriptions.Item label="Tên thiết bị" span={2}>{d.name || '—'}</Descriptions.Item>
          <Descriptions.Item label="Tên chuyên ngành">{d.specializedName || '—'}</Descriptions.Item>
          <Descriptions.Item label="Loại">{d.typeName || '—'}</Descriptions.Item>
          <Descriptions.Item label="Nhóm thiết bị">{d.deviceGroup || '—'}</Descriptions.Item>
          <Descriptions.Item label="Phân loại">{d.classification || '—'}</Descriptions.Item>
          <Descriptions.Item label="Đơn vị sử dụng">{d.organizationName || '—'}</Descriptions.Item>
          <Descriptions.Item label="Trạng thái">{d.statusName || '—'}</Descriptions.Item>
          <Descriptions.Item label="Tình trạng">{d.conditionNote || '—'}</Descriptions.Item>
          <Descriptions.Item label="Ký hiệu / Model">{d.modelCode || '—'}</Descriptions.Item>
          <Descriptions.Item label="Serial">{d.serial || '—'}</Descriptions.Item>
          <Descriptions.Item label="Thông số kỹ thuật" span={2}>
            {d.technicalSpecs || '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Hãng sản xuất">{d.manufacturer || '—'}</Descriptions.Item>
          <Descriptions.Item label="Nước sản xuất">{d.countryOfOrigin || '—'}</Descriptions.Item>
          <Descriptions.Item label="Năm sản xuất">{d.yearOfManufacture || '—'}</Descriptions.Item>
          <Descriptions.Item label="Cơ sở">{d.locationCode || '—'}</Descriptions.Item>
          <Descriptions.Item label="Nguồn mua sắm">
            {d.procurementSource
              ? PROCUREMENT_LABELS[d.procurementSource] || d.procurementSource
              : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Hình thức bảo trì">
            {d.maintenanceOrgId != null
              ? MAINTENANCE_LABELS[d.maintenanceOrgId] || String(d.maintenanceOrgId)
              : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Gói thầu">{d.biddingPackageCode || '—'}</Descriptions.Item>
          <Descriptions.Item label="Số lượng">
            {d.quantity != null ? d.quantity : '—'}
            {d.unit ? ` ${d.unit}` : ''}
          </Descriptions.Item>
          <Descriptions.Item label="Đơn giá">{formatMoney(d.unitPrice)}</Descriptions.Item>
          <Descriptions.Item label="Thành tiền / nguyên giá">
            {formatMoney(d.totalAmount)}
          </Descriptions.Item>
          <Descriptions.Item label="Ngày tiếp nhận">
            {d.receivedDate ? dayjs(d.receivedDate).format('DD/MM/YYYY') : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Đưa vào sử dụng">
            {formatMarker(d.putIntoUseMarker)}
          </Descriptions.Item>
          <Descriptions.Item label="Mốc bảo hành">
            {formatMarker(d.warrantyDueMarker)}
          </Descriptions.Item>
          <Descriptions.Item label="Mốc bảo trì">
            {formatMarker(d.maintenanceDueMarker)}
          </Descriptions.Item>
          <Descriptions.Item label="Mốc hiệu chuẩn">
            {formatMarker(d.calibrationDueMarker)}
          </Descriptions.Item>
          <Descriptions.Item label="Mốc kiểm định">
            {formatMarker(d.inspectionDueMarker)}
          </Descriptions.Item>
          <Descriptions.Item label="Hạn kiểm định">
            {d.inspectionExpiryDate
              ? dayjs(d.inspectionExpiryDate).format('DD/MM/YYYY')
              : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Thiết bị chính">{d.parentDeviceCode || '—'}</Descriptions.Item>
          <Descriptions.Item label="Ghi chú" span={2}>{d.note || '—'}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card className="app-card" title="Hợp đồng bảo trì" style={{ marginTop: 16 }}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          {d.legalDocumentRef ? (
            <div>
              <FileOutlined style={{ marginRight: 8, color: '#0d9488' }} />
              <a
                href={maintenanceContractUrl(deviceId, d.legalDocumentRef)}
                target="_blank"
                rel="noreferrer"
              >
                {d.legalDocumentRef}
              </a>
            </div>
          ) : (
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description="Chưa có hợp đồng bảo trì"
            />
          )}
          {canMaintain && (
            <Upload
              showUploadList={false}
              beforeUpload={(file) => {
                uploadContractMutation.mutate(file);
                return false;
              }}
            >
              <Button
                icon={<UploadOutlined />}
                loading={uploadContractMutation.isPending}
              >
                {d.legalDocumentRef ? 'Thay hợp đồng bảo trì' : 'Tải lên hợp đồng bảo trì'}
              </Button>
            </Upload>
          )}
        </Space>
      </Card>
      <Card className="app-card" style={{ marginTop: 16 }}>
        <Tabs
          items={[
            {
              key: 'children',
              label: 'Thiết bị con',
              children: (
                <>
                  {canMaintain && (
                    <Card size="small" type="inner" title="Thêm thiết bị con" style={{ marginBottom: 16 }}>
                      <Form
                        form={childForm}
                        layout="inline"
                        onFinish={(v) => createChildMutation.mutate(v)}
                      >
                        <Form.Item
                          name="deviceCode"
                          rules={[{ required: true, message: 'Mã TB' }]}
                        >
                          <Input placeholder="Mã thiết bị con" style={{ width: 160 }} />
                        </Form.Item>
                        <Form.Item name="name" rules={[{ required: true, message: 'Tên TB' }]}>
                          <Input placeholder="Tên thiết bị" style={{ width: 220 }} />
                        </Form.Item>
                        <Form.Item>
                          <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            htmlType="submit"
                            loading={createChildMutation.isPending}
                          >
                            Thêm
                          </Button>
                        </Form.Item>
                      </Form>
                    </Card>
                  )}
                  {(childrenQuery.data ?? []).length === 0 ? (
                    <Empty description="Chưa có thiết bị con" image={Empty.PRESENTED_IMAGE_SIMPLE} />
                  ) : (
                    <List
                      dataSource={childrenQuery.data ?? []}
                      renderItem={(item) => (
                        <List.Item>
                          <Link to={`/devices/${item.id}`}>{item.deviceCode}</Link>
                          <span style={{ marginLeft: 8, color: '#64748b' }}>{item.name}</span>
                        </List.Item>
                      )}
                    />
                  )}
                </>
              ),
            },
            {
              key: 'files',
              label: 'Tài liệu',
              children: (
                <>
                  <Upload
                    beforeUpload={(file) => {
                      uploadMutation.mutate(file);
                      return false;
                    }}
                    showUploadList={false}
                  >
                    <Button icon={<UploadOutlined />} loading={uploadMutation.isPending}>
                      Tải lên tài liệu
                    </Button>
                  </Upload>
                  <List
                    style={{ marginTop: 16 }}
                    dataSource={d.files}
                    locale={{ emptyText: 'Chưa có tài liệu' }}
                    renderItem={(f) => (
                      <List.Item
                        actions={
                          canMaintain && f.path?.toLowerCase().includes('.pdf')
                            ? [
                                <Button
                                  key="sign"
                                  size="small"
                                  loading={signPdfMutation.isPending}
                                  onClick={() => signPdfMutation.mutate(f.id)}
                                >
                                  Ký số PDF
                                </Button>,
                              ]
                            : undefined
                        }
                      >
                        <FileOutlined style={{ marginRight: 8, color: '#0d9488' }} />
                        <a href={fileUrl(f.path)} target="_blank" rel="noreferrer">
                          {f.description || f.path}
                        </a>
                      </List.Item>
                    )}
                  />
                </>
              ),
            },
            {
              key: 'camera',
              label: 'Chụp ảnh',
              children: canMaintain ? (
                <DeviceCameraCapture
                  deviceId={deviceId}
                  onDone={() => queryClient.invalidateQueries({ queryKey: ['device', deviceId] })}
                />
              ) : (
                <Empty description="Chỉ kỹ thuật/admin được chụp ảnh" />
              ),
            },
            {
              key: 'history',
              label: 'Lịch sử',
              children: (
                <Table
                  rowKey={(r, i) => `${r.type}-${i}`}
                  loading={historyQuery.isLoading}
                  dataSource={historyQuery.data ?? []}
                  pagination={{ pageSize: 10 }}
                  columns={[
                    { title: 'Loại', dataIndex: 'type', width: 120 },
                    { title: 'Nội dung', dataIndex: 'content', ellipsis: true },
                    {
                      title: 'Thời gian',
                      dataIndex: 'timestamp',
                      width: 160,
                      render: (t) => (t ? dayjs(t).format('DD/MM/YYYY HH:mm') : '—'),
                    },
                    { title: 'Người thực hiện', dataIndex: 'actor', width: 140 },
                  ]}
                />
              ),
            },
            {
              key: 'legal',
              label: 'Hồ sơ pháp lý',
              children: legalQuery.isLoading ? (
                <LoadingCenter />
              ) : !legalQuery.data ? (
                <Empty description="Chưa có hồ sơ pháp lý">
                  {canMaintain && (
                    <Link to="/legal-documents">
                      <Button type="link">Quản lý hồ sơ pháp lý</Button>
                    </Link>
                  )}
                </Empty>
              ) : (
                <>
                  <Descriptions bordered column={{ xs: 1, sm: 2 }} size="small">
                    <Descriptions.Item label="Giấy kiểm định">
                      {legalQuery.data.inspectionCertificate || '—'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Giấy nhập khẩu">
                      {legalQuery.data.importPermit || '—'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Giấy lưu hành">
                      {legalQuery.data.circulationPermit || '—'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Giá có VAT">
                      {legalQuery.data.priceWithVat != null
                        ? Number(legalQuery.data.priceWithVat).toLocaleString('vi-VN')
                        : '—'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Thông tin gói thầu" span={2}>
                      {legalQuery.data.biddingInfo || '—'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Thiết bị (HS)" span={2}>
                      {legalQuery.data.deviceDisplayName ||
                        legalQuery.data.deviceCode ||
                        legalQuery.data.deviceCodes ||
                        '—'}
                    </Descriptions.Item>
                  </Descriptions>
                  {legalQuery.data.filePath && (
                    <div style={{ marginTop: 12 }}>
                      <FileOutlined style={{ marginRight: 8 }} />
                      <a href={fileUrl(legalQuery.data.filePath)} target="_blank" rel="noreferrer">
                        Tài liệu hồ sơ
                      </a>
                    </div>
                  )}
                  {canMaintain && (
                    <Space style={{ marginTop: 16 }}>
                      <Upload
                        showUploadList={false}
                        beforeUpload={(file) => {
                          uploadLegalFileMutation.mutate(file);
                          return false;
                        }}
                      >
                        <Button icon={<UploadOutlined />} loading={uploadLegalFileMutation.isPending}>
                          Tải file hồ sơ
                        </Button>
                      </Upload>
                      <Link to="/legal-documents">
                        <Button>Mở danh sách hồ sơ</Button>
                      </Link>
                    </Space>
                  )}
                </>
              ),
            },
            {
              key: 'usage',
              label: 'Nhật ký sử dụng',
              children: (
                <DeviceUsageLogsPanel deviceId={deviceId} canEdit={canMaintain} />
              ),
            },
            {
              key: 'supplies',
              label: 'Vật tư đi kèm',
              children: (
                <>
                  {canMaintain && (
                    <Card size="small" type="inner" title="Thêm vật tư đi kèm" style={{ marginBottom: 16 }}>
                      <Form
                        form={supplyForm}
                        layout="inline"
                        onFinish={(v) => addSupplyMutation.mutate(v)}
                      >
                        <Form.Item name="name" rules={[{ required: true, message: 'Tên vật tư' }]}>
                          <Input placeholder="Tên vật tư" style={{ width: 200 }} />
                        </Form.Item>
                        <Form.Item name="unit">
                          <Input placeholder="Đơn vị" style={{ width: 100 }} />
                        </Form.Item>
                        <Form.Item name="quantity">
                          <InputNumber placeholder="SL" min={0} style={{ width: 100 }} />
                        </Form.Item>
                        <Form.Item>
                          <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            htmlType="submit"
                            loading={addSupplyMutation.isPending}
                          >
                            Thêm
                          </Button>
                        </Form.Item>
                      </Form>
                    </Card>
                  )}
                  <Table
                    rowKey="id"
                    dataSource={suppliesQuery.data ?? []}
                    pagination={false}
                    columns={[
                      { title: 'Tên vật tư', dataIndex: 'name' },
                      { title: 'Đơn vị', dataIndex: 'unit' },
                      { title: 'Số lượng', dataIndex: 'quantity' },
                      ...(canMaintain
                        ? [
                            {
                              title: '',
                              width: 80,
                              render: (_: unknown, r: DeviceSupplyItem) => (
                                <Button
                                  size="small"
                                  danger
                                  onClick={() => deleteSupplyMutation.mutate(r.id)}
                                >
                                  Xóa
                                </Button>
                              ),
                            },
                          ]
                        : []),
                    ]}
                  />
                </>
              ),
            },
            {
              key: 'qr',
              label: 'QR / NFC',
              children: (
                <Form
                  form={qrForm}
                  layout="vertical"
                  initialValues={{
                    qrCode: firstCode?.qrCode,
                    nfc: firstCode?.nfc,
                  }}
                  onFinish={(v) => qrMutation.mutate(v)}
                >
                  <Form.Item name="qrCode" label="Mã QR">
                    <Input placeholder="Nội dung mã QR" />
                  </Form.Item>
                  <Form.Item name="nfc" label="Mã NFC">
                    <Input placeholder="Mã NFC (nếu có)" />
                  </Form.Item>
                  {canMaintain && (
                    <Button type="primary" htmlType="submit" loading={qrMutation.isPending}>
                      Lưu mã
                    </Button>
                  )}
                </Form>
              ),
            },
          ]}
        />
      </Card>
      <DeviceReportModal
        open={reportOpen}
        deviceId={deviceId}
        onClose={() => setReportOpen(false)}
      />
      <TransferDeviceModal
        open={transferOpen}
        organizations={orgsQuery.data ?? []}
        loading={transferMutation.isPending}
        onClose={() => setTransferOpen(false)}
        onSubmit={(v) => transferMutation.mutate(v)}
      />
    </div>
  );
}
