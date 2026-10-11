import {
  Button,
  Card,
  Col,
  DatePicker,
  Form,
  Input,
  InputNumber,
  Row,
  Select,
  Space,
  Upload,
  message,
} from 'antd';
import { FileOutlined, UploadOutlined } from '@ant-design/icons';
import { useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs, { type Dayjs } from 'dayjs';
import { Link, useNavigate, useParams } from 'react-router-dom';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceDetail, MasterItem, OrganizationItem } from '@/types/api';
import type { BiddingPackageItem } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { apiUrl } from '@/utils/apiBase';

function maintenanceContractUrl(deviceId: number, fileName?: string) {
  if (!fileName?.trim()) return undefined;
  return apiUrl(`/hdbaotri/${deviceId}/${encodeURIComponent(fileName.trim())}`);
}

const DEVICE_GROUP_OPTIONS = [
  { value: 'Chẩn đoán', label: 'Chẩn đoán' },
  { value: 'Điều trị', label: 'Điều trị' },
  { value: 'Phân tích', label: 'Phân tích' },
  { value: 'Thiết bị', label: 'Thiết bị' },
];

const CLASSIFICATION_OPTIONS = ['A', 'B', 'C', 'D'].map((v) => ({ value: v, label: v }));

const LOCATION_OPTIONS = [
  { value: 'CS1', label: 'CS1' },
  { value: 'CS2', label: 'CS2' },
];

const PROCUREMENT_OPTIONS = [
  { value: '1', label: 'Nguồn dự án' },
  { value: '2', label: 'Quỹ phát triển sự nghiệp' },
  { value: '3', label: 'Tài trợ - viện trợ' },
  { value: '4', label: 'Nguồn ngân sách' },
  { value: '5', label: 'Nguồn vay kích cầu' },
];

const MAINTENANCE_OPTIONS = [
  { value: 1, label: 'Tự bảo trì' },
  { value: 2, label: 'Bảo trì cơ bản' },
  { value: 3, label: 'Bảo trì trọn gói' },
  { value: 4, label: 'Bảo trì cơ bản có thay thế linh kiện' },
];

function parseMarker(raw?: string | null): Dayjs | undefined {
  if (!raw?.trim()) return undefined;
  const s = raw.trim();
  const formats = ['YYYY-MM-DD', 'DD/MM/YYYY', 'DD-MM-YYYY', 'YYYY/MM/DD'];
  for (const f of formats) {
    const d = dayjs(s, f, true);
    if (d.isValid()) return d;
  }
  const d = dayjs(s);
  return d.isValid() ? d : undefined;
}

function markerToString(v: unknown): string | undefined {
  if (!v) return undefined;
  if (dayjs.isDayjs(v)) return v.format('YYYY-MM-DD');
  return undefined;
}

function moneyToString(v: unknown): string | undefined {
  if (v === undefined || v === null || v === '') return undefined;
  return String(v);
}

function buildPayload(values: Record<string, unknown>, includeDeviceCode: boolean) {
  const payload: Record<string, unknown> = {
    name: values.name,
    typeId: values.typeId,
    organizationId: values.organizationId,
    statusId: values.statusId,
    identityCode: values.identityCode || undefined,
    modelCode: values.modelCode || undefined,
    serial: values.serial || undefined,
    manufacturer: values.manufacturer || undefined,
    countryOfOrigin: values.countryOfOrigin || undefined,
    yearOfManufacture: values.yearOfManufacture || undefined,
    technicalSpecs: values.technicalSpecs || undefined,
    conditionNote: values.conditionNote || undefined,
    deviceGroup: values.deviceGroup || undefined,
    classification: values.classification || undefined,
    handoverOrgId: values.handoverOrgId ?? null,
    maintenanceOrgId: values.maintenanceOrgId ?? null,
    biddingPackageCode: values.biddingPackageCode || undefined,
    quantity: values.quantity ?? null,
    unit: values.unit || undefined,
    unitPrice: moneyToString(values.unitPrice),
    totalAmount: moneyToString(values.totalAmount),
    locationCode: values.locationCode || undefined,
    procurementSource: values.procurementSource || undefined,
    warrantyDueMarker: markerToString(values.warrantyDueMarker),
    maintenanceDueMarker: markerToString(values.maintenanceDueMarker),
    calibrationDueMarker: markerToString(values.calibrationDueMarker),
    inspectionDueMarker: markerToString(values.inspectionDueMarker),
    putIntoUseMarker: markerToString(values.putIntoUseMarker),
    specializedName: values.specializedName || undefined,
    parentDeviceCode: values.parentDeviceCode || undefined,
    note: values.note || undefined,
    receivedDate: values.receivedDate
      ? (values.receivedDate as Dayjs).format('YYYY-MM-DD')
      : null,
    inspectionExpiryDate: values.inspectionExpiryDate
      ? (values.inspectionExpiryDate as Dayjs).format('YYYY-MM-DD')
      : null,
  };
  if (includeDeviceCode) payload.deviceCode = values.deviceCode;
  return payload;
}

function apiErrorMessage(err: unknown, fallback: string): string {
  if (typeof err === 'object' && err && 'response' in err) {
    const data = (err as { response?: { data?: { message?: string; errors?: string[] } } }).response
      ?.data;
    if (data?.errors?.length) return data.errors.join('; ');
    if (data?.message) return data.message;
  }
  return fallback;
}

export function DeviceFormPage() {
  const { id } = useParams();
  const isEdit = Boolean(id && id !== 'new');
  const deviceId = isEdit ? Number(id) : 0;
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [form] = Form.useForm();

  const masterTypes = useQuery({
    queryKey: ['master', 'device-types'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<MasterItem[]>>('/api/master/device-types');
      return res.data.data ?? [];
    },
  });

  const masterStatus = useQuery({
    queryKey: ['master', 'status'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<MasterItem[]>>('/api/master/status');
      return res.data.data ?? [];
    },
  });

  const orgs = useQuery({
    queryKey: ['organizations'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<OrganizationItem[]>>('/api/organizations');
      return res.data.data ?? [];
    },
  });

  const bidding = useQuery({
    queryKey: ['bidding-packages-lookup'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<BiddingPackageItem[]>>('/api/bidding');
      return res.data.data ?? [];
    },
  });

  const deviceQuery = useQuery({
    queryKey: ['device', deviceId],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceDetail>>(`/api/devices/${deviceId}`);
      return res.data.data!;
    },
    enabled: isEdit && deviceId > 0,
  });

  useEffect(() => {
    if (!deviceQuery.data || !isEdit) return;
    const d = deviceQuery.data;
    form.setFieldsValue({
      deviceCode: d.deviceCode,
      identityCode: d.identityCode,
      name: d.name,
      typeId: d.typeId,
      organizationId: d.organizationId,
      statusId: d.statusId,
      deviceGroup: d.deviceGroup,
      classification: d.classification,
      conditionNote: d.conditionNote,
      modelCode: d.modelCode,
      serial: d.serial,
      technicalSpecs: d.technicalSpecs,
      manufacturer: d.manufacturer,
      countryOfOrigin: d.countryOfOrigin,
      yearOfManufacture: d.yearOfManufacture,
      handoverOrgId: d.handoverOrgId,
      maintenanceOrgId: d.maintenanceOrgId,
      biddingPackageCode: d.biddingPackageCode,
      quantity: d.quantity,
      unit: d.unit,
      unitPrice: d.unitPrice ? Number(String(d.unitPrice).replace(/,/g, '')) : undefined,
      totalAmount: d.totalAmount ? Number(String(d.totalAmount).replace(/,/g, '')) : undefined,
      locationCode: d.locationCode,
      procurementSource: d.procurementSource,
      specializedName: d.specializedName,
      parentDeviceCode: d.parentDeviceCode,
      note: d.note,
      receivedDate: d.receivedDate ? dayjs(d.receivedDate) : undefined,
      inspectionExpiryDate: d.inspectionExpiryDate ? dayjs(d.inspectionExpiryDate) : undefined,
      putIntoUseMarker: parseMarker(d.putIntoUseMarker),
      warrantyDueMarker: parseMarker(d.warrantyDueMarker),
      maintenanceDueMarker: parseMarker(d.maintenanceDueMarker),
      calibrationDueMarker: parseMarker(d.calibrationDueMarker),
      inspectionDueMarker: parseMarker(d.inspectionDueMarker),
    });
  }, [deviceQuery.data, isEdit, form]);

  const saveMutation = useMutation({
    mutationFn: async (values: Record<string, unknown>) => {
      if (isEdit) {
        await axiosClient.put(`/api/devices/${deviceId}`, buildPayload(values, false));
      } else {
        await axiosClient.post('/api/devices', buildPayload(values, true));
      }
    },
    onSuccess: () => {
      message.success(isEdit ? 'Cập nhật thiết bị thành công' : 'Thêm thiết bị thành công');
      navigate('/devices');
    },
    onError: (err) => message.error(apiErrorMessage(err, 'Lưu thất bại. Kiểm tra dữ liệu và thử lại.')),
  });

  const uploadContractMutation = useMutation({
    mutationFn: async (file: File) => {
      const formData = new FormData();
      formData.append('file', file);
      await axiosClient.post(`/api/devices/${deviceId}/maintenance-contract`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    onSuccess: () => {
      message.success('Upload hợp đồng bảo trì thành công');
      queryClient.invalidateQueries({ queryKey: ['device', deviceId] });
    },
    onError: (err) =>
      message.error(apiErrorMessage(err, 'Upload hợp đồng bảo trì thất bại')),
  });

  if (isEdit && deviceQuery.isLoading) return <LoadingCenter />;

  const title = isEdit ? 'Chỉnh sửa thiết bị' : 'Thêm thiết bị mới';
  const orgOptions = (orgs.data ?? []).map((o) => ({ value: o.id, label: o.name }));
  const biddingOptions = (bidding.data ?? []).map((b) => ({
    value: b.packageCode || String(b.id),
    label: `${b.packageCode || b.id}${b.name ? ` — ${b.name}` : ''}`,
  }));

  return (
    <div>
      <PageHeader
        title={title}
        subtitle="Nhập đầy đủ thông tin theo quy định quản lý tài sản y tế"
        crumbs={[
          { title: 'Thiết bị', href: '/devices' },
          { title: title },
        ]}
      />
      <Form
        form={form}
        layout="vertical"
        onFinish={(v) => saveMutation.mutate(v)}
        requiredMark="optional"
        initialValues={{ quantity: 1 }}
      >
        <Card className="app-card" title="Thông tin chung" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            {!isEdit && (
              <Col xs={24} md={8}>
                <Form.Item
                  name="deviceCode"
                  label="Mã thiết bị"
                  rules={[{ required: true, message: 'Nhập mã thiết bị' }]}
                >
                  <Input placeholder="VD: TB-2024-001" />
                </Form.Item>
              </Col>
            )}
            <Col xs={24} md={8}>
              <Form.Item name="identityCode" label="Mã tài sản">
                <Input placeholder="Mã định danh tài sản" />
              </Form.Item>
            </Col>
            <Col xs={24} md={isEdit ? 16 : 8}>
              <Form.Item
                name="name"
                label="Tên thiết bị"
                rules={[{ required: true, message: 'Nhập tên thiết bị' }]}
              >
                <Input placeholder="Tên đầy đủ theo hồ sơ" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item
                name="typeId"
                label="Chủng loại"
                rules={[{ required: true, message: 'Chọn chủng loại' }]}
              >
                <Select
                  showSearch
                  optionFilterProp="label"
                  placeholder="Chọn loại"
                  options={(masterTypes.data ?? []).map((t) => ({ value: t.id, label: t.name }))}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="deviceGroup" label="Phân nhóm">
                <Select allowClear placeholder="Chọn phân nhóm" options={DEVICE_GROUP_OPTIONS} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="classification" label="Phân loại (A–D)">
                <Select allowClear placeholder="Chọn phân loại" options={CLASSIFICATION_OPTIONS} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item
                name="statusId"
                label="Trạng thái"
                rules={[{ required: true, message: 'Chọn trạng thái' }]}
              >
                <Select
                  placeholder="Chọn trạng thái"
                  options={(masterStatus.data ?? []).map((s) => ({ value: s.id, label: s.name }))}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="conditionNote" label="Tình trạng">
                <Input placeholder="Mô tả tình trạng hiện tại" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="specializedName" label="Tên chuyên dùng đặc thù">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="modelCode" label="Ký hiệu / Model">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="serial" label="Serial">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24}>
              <Form.Item name="technicalSpecs" label="Thông số kỹ thuật">
                <Input.TextArea rows={3} />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        <Card className="app-card" title="Xuất xứ & đơn vị" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item name="manufacturer" label="Hãng sản xuất">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="countryOfOrigin" label="Nước sản xuất">
                <Input placeholder="VD: Nhật Bản, Đức..." />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="yearOfManufacture" label="Năm sản xuất">
                <Input placeholder="VD: 2020" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item
                name="organizationId"
                label="Khoa/phòng sử dụng"
                rules={[{ required: true, message: 'Chọn đơn vị sử dụng' }]}
              >
                <Select
                  showSearch
                  optionFilterProp="label"
                  placeholder="Chọn đơn vị"
                  options={orgOptions}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="handoverOrgId" label="Đơn vị bàn giao">
                <Select
                  allowClear
                  showSearch
                  optionFilterProp="label"
                  placeholder="Chọn đơn vị"
                  options={orgOptions}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="parentDeviceCode" label="Mã thiết bị chính (TB con)">
                <Input placeholder="Mã TB cha" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="locationCode" label="Vị trí">
                <Select allowClear placeholder="CS1 / CS2" options={LOCATION_OPTIONS} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="procurementSource" label="Nguồn mua sắm">
                <Select allowClear placeholder="Chọn nguồn" options={PROCUREMENT_OPTIONS} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="maintenanceOrgId" label="Hình thức bảo trì">
                <Select allowClear placeholder="Chọn hình thức" options={MAINTENANCE_OPTIONS} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="biddingPackageCode" label="Gói thầu">
                <Select
                  allowClear
                  showSearch
                  optionFilterProp="label"
                  placeholder="Chọn gói thầu"
                  options={biddingOptions}
                  loading={bidding.isLoading}
                />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        <Card className="app-card" title="Số lượng & giá trị" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col xs={24} md={6}>
              <Form.Item name="quantity" label="Số lượng">
                <InputNumber min={0} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item name="unit" label="Đơn vị tính">
                <Input placeholder="Cái, Bộ..." />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item name="unitPrice" label="Đơn giá (VNĐ)">
                <InputNumber min={0} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item name="totalAmount" label="Thành tiền / nguyên giá (VNĐ)">
                <InputNumber min={0} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        <Card className="app-card" title="Mốc thời gian" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item name="receivedDate" label="Ngày tiếp nhận">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="putIntoUseMarker" label="Thời gian đưa vào sử dụng">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="warrantyDueMarker" label="Mốc thời gian bảo hành">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="maintenanceDueMarker" label="Mốc thời gian bảo trì">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="calibrationDueMarker" label="Mốc thời gian hiệu chuẩn">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="inspectionDueMarker" label="Mốc thời gian kiểm định">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="inspectionExpiryDate" label="Hạn kiểm định">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {isEdit && (
          <Card className="app-card" title="Hợp đồng bảo trì" style={{ marginBottom: 16 }}>
            <Space direction="vertical" size="middle" style={{ width: '100%' }}>
              {deviceQuery.data?.legalDocumentRef ? (
                <div>
                  <FileOutlined style={{ marginRight: 8, color: '#0d9488' }} />
                  <a
                    href={maintenanceContractUrl(deviceId, deviceQuery.data.legalDocumentRef)}
                    target="_blank"
                    rel="noreferrer"
                  >
                    {deviceQuery.data.legalDocumentRef}
                  </a>
                </div>
              ) : (
                <span style={{ color: '#64748b' }}>Chưa có hợp đồng bảo trì — lưu thiết bị trước nếu mới tạo.</span>
              )}
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
                  {deviceQuery.data?.legalDocumentRef
                    ? 'Thay hợp đồng bảo trì'
                    : 'Tải lên hợp đồng bảo trì'}
                </Button>
              </Upload>
            </Space>
          </Card>
        )}

        <Card className="app-card" title="Ghi chú" style={{ marginBottom: 16 }}>
          <Form.Item name="note" label="Ghi chú">
            <Input.TextArea rows={3} placeholder="Ghi chú bổ sung" />
          </Form.Item>
        </Card>

        <Space size="middle">
          <Button type="primary" htmlType="submit" loading={saveMutation.isPending}>
            Lưu thiết bị
          </Button>
          <Link to="/devices">
            <Button>Hủy</Button>
          </Link>
        </Space>
      </Form>
    </div>
  );
}
