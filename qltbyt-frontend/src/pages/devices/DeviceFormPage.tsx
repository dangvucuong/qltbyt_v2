import { Button, Card, Col, DatePicker, Form, Input, Row, Select, Space, message } from 'antd';
import { useEffect } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { Link, useNavigate, useParams } from 'react-router-dom';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceDetail, MasterItem, OrganizationItem } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

export function DeviceFormPage() {
  const { id } = useParams();
  const isEdit = Boolean(id && id !== 'new');
  const deviceId = isEdit ? Number(id) : 0;
  const navigate = useNavigate();
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
      name: d.name,
      typeId: d.typeId,
      organizationId: d.organizationId,
      statusId: d.statusId,
      manufacturer: d.manufacturer,
      countryOfOrigin: d.countryOfOrigin,
      modelCode: d.modelCode,
      serial: d.serial,
      parentDeviceCode: d.parentDeviceCode,
      note: d.note,
      receivedDate: d.receivedDate ? dayjs(d.receivedDate) : undefined,
      inspectionExpiryDate: d.inspectionExpiryDate
        ? dayjs(d.inspectionExpiryDate)
        : undefined,
    });
  }, [deviceQuery.data, isEdit, form]);

  const saveMutation = useMutation({
    mutationFn: async (values: Record<string, unknown>) => {
      const payload = {
        name: values.name,
        typeId: values.typeId,
        organizationId: values.organizationId,
        statusId: values.statusId,
        manufacturer: values.manufacturer,
        countryOfOrigin: values.countryOfOrigin,
        modelCode: values.modelCode,
        serial: values.serial,
        parentDeviceCode: values.parentDeviceCode,
        note: values.note,
        receivedDate: values.receivedDate
          ? (values.receivedDate as dayjs.Dayjs).format('YYYY-MM-DD')
          : undefined,
        inspectionExpiryDate: values.inspectionExpiryDate
          ? (values.inspectionExpiryDate as dayjs.Dayjs).format('YYYY-MM-DD')
          : undefined,
      };
      if (isEdit) {
        await axiosClient.put(`/api/devices/${deviceId}`, payload);
      } else {
        await axiosClient.post('/api/devices', {
          ...payload,
          deviceCode: values.deviceCode,
        });
      }
    },
    onSuccess: () => {
      message.success(isEdit ? 'Cập nhật thiết bị thành công' : 'Thêm thiết bị thành công');
      navigate('/devices');
    },
    onError: () => message.error('Lưu thất bại. Kiểm tra dữ liệu và thử lại.'),
  });

  if (isEdit && deviceQuery.isLoading) return <LoadingCenter />;

  const title = isEdit ? 'Chỉnh sửa thiết bị' : 'Thêm thiết bị mới';

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
      <Card className="app-card">
        <Form
          form={form}
          layout="vertical"
          onFinish={(v) => saveMutation.mutate(v)}
          requiredMark="optional"
        >
          <Row gutter={24}>
            {!isEdit && (
              <Col xs={24} md={12}>
                <Form.Item
                  name="deviceCode"
                  label="Mã thiết bị"
                  rules={[{ required: true, message: 'Nhập mã thiết bị' }]}
                >
                  <Input placeholder="VD: TB-2024-001" />
                </Form.Item>
              </Col>
            )}
            <Col xs={24} md={isEdit ? 24 : 12}>
              <Form.Item
                name="name"
                label="Tên thiết bị"
                rules={[{ required: true, message: 'Nhập tên thiết bị' }]}
              >
                <Input placeholder="Tên đầy đủ theo hồ sơ" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="typeId" label="Loại thiết bị" rules={[{ required: true }]}>
                <Select
                  placeholder="Chọn loại"
                  options={masterTypes.data?.map((t) => ({ value: t.id, label: t.name }))}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item
                name="organizationId"
                label="Đơn vị sử dụng"
                rules={[{ required: true, message: 'Chọn đơn vị' }]}
              >
                <Select
                  showSearch
                  optionFilterProp="label"
                  placeholder="Chọn đơn vị"
                  options={orgs.data?.map((o) => ({ value: o.id, label: o.name }))}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="statusId" label="Trạng thái" rules={[{ required: true }]}>
                <Select
                  placeholder="Chọn trạng thái"
                  options={masterStatus.data?.map((s) => ({ value: s.id, label: s.name }))}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="manufacturer" label="Hãng sản xuất">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="countryOfOrigin" label="Nước sản xuất">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="parentDeviceCode" label="Mã thiết bị chính (nếu là thiết bị con)">
                <Input placeholder="Mã TB cha" />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item name="receivedDate" label="Ngày tiếp nhận">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item name="inspectionExpiryDate" label="Hạn kiểm định">
                <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col xs={24}>
              <Form.Item name="note" label="Ghi chú">
                <Input.TextArea rows={3} placeholder="Ghi chú bổ sung" />
              </Form.Item>
            </Col>
          </Row>
          <Space size="middle">
            <Button type="primary" htmlType="submit" loading={saveMutation.isPending}>
              Lưu thiết bị
            </Button>
            <Link to="/devices">
              <Button>Hủy</Button>
            </Link>
          </Space>
        </Form>
      </Card>
    </div>
  );
}
