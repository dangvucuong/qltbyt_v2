import { Button, Card, DatePicker, Form, InputNumber, Select, Table, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceListItem, PagedResult } from '@/types/api';
import type { CreateUsageLogPayload, UsageLogItem } from '@/types/phase2';
import { PageHeader } from '@/components/PageHeader';
import { useAuthStore } from '@/store/authStore';

function formatLogDate(value?: string) {
  if (!value) return '—';
  const d = dayjs(value);
  return d.isValid() ? d.format('DD/MM/YYYY') : '—';
}

/** Luôn dùng ID thiết bị (thay MaTB) cho API nhật ký sử dụng. */
function devicePickerValue(d: DeviceListItem): string {
  return String(d.id);
}

function deviceLabel(d: DeviceListItem) {
  const code =
    d.deviceCode?.trim() && d.deviceCode.trim() !== String(d.id)
      ? `${d.id} / ${d.deviceCode.trim()}`
      : String(d.id);
  const name = d.name?.trim();
  return name ? `${code} — ${name}` : code;
}

export function UsageLogsPage() {
  const queryClient = useQueryClient();
  const role = useAuthStore((s) => s.user?.role);
  const canEdit = role === 'Admin' || role === 'Technician';
  const [filterForm] = Form.useForm();
  const [createForm] = Form.useForm();
  const deviceCode = Form.useWatch('deviceCode', filterForm);
  const [deviceSearch, setDeviceSearch] = useState('');

  const devicesQuery = useQuery({
    queryKey: ['devices', 'usage-log-picker', deviceSearch],
    queryFn: async () => {
      const q = new URLSearchParams({
        page: '1',
        pageSize: '100',
        sortBy: 'name',
      });
      const term = deviceSearch.trim();
      if (term) q.set('search', term);
      const res = await axiosClient.get<ApiResponse<PagedResult<DeviceListItem>>>(
        `/api/devices?${q}`,
      );
      return res.data.data?.items ?? [];
    },
    staleTime: 30_000,
  });

  const deviceOptions = useMemo(
    () =>
      (devicesQuery.data ?? []).map((d) => ({
        value: devicePickerValue(d),
        label: deviceLabel(d),
      })),
    [devicesQuery.data],
  );

  const logsQuery = useQuery({
    queryKey: ['usage-logs', deviceCode],
    queryFn: async () => {
      const params = deviceCode ? `?deviceCode=${encodeURIComponent(deviceCode)}` : '';
      const res = await axiosClient.get<ApiResponse<UsageLogItem[]>>(`/api/usage-logs${params}`);
      return res.data.data ?? [];
    },
    placeholderData: (prev) => prev,
  });

  const createMutation = useMutation({
    mutationFn: async (payload: CreateUsageLogPayload) => {
      await axiosClient.post('/api/usage-logs', payload);
    },
    onSuccess: () => {
      message.success('Ghi nhật ký thành công');
      createForm.resetFields();
      createForm.setFieldsValue({ inputDate: dayjs() });
      queryClient.invalidateQueries({ queryKey: ['usage-logs'] });
    },
    onError: () => message.error('Ghi nhật ký thất bại'),
  });

  return (
    <div>
      <PageHeader
        title="Nhật ký sử dụng"
        subtitle="Giờ/ca theo ngày — có thể ghi trực tiếp từ trang chi tiết thiết bị"
      />
      {canEdit ? (
        <Card className="app-card" title="Ghi nhật ký mới" style={{ marginBottom: 16 }}>
          <Form
            form={createForm}
            layout="inline"
            initialValues={{ inputDate: dayjs() }}
            onFinish={(v) =>
              createMutation.mutate({
                deviceCode: v.deviceCode,
                hours: v.hours,
                sessions: v.sessions,
                inputDate: v.inputDate.format('YYYY-MM-DD'),
              })
            }
          >
            <Form.Item name="deviceCode" label="Thiết bị" rules={[{ required: true, message: 'Chọn thiết bị' }]}>
              <Select
                showSearch
                allowClear
                placeholder="Gõ tên hoặc mã để tìm thiết bị"
                optionFilterProp="label"
                filterOption={false}
                onSearch={setDeviceSearch}
                notFoundContent={devicesQuery.isFetching ? 'Đang tải...' : 'Không có thiết bị'}
                options={deviceOptions}
                style={{ minWidth: 320 }}
                loading={devicesQuery.isFetching}
              />
            </Form.Item>
            <Form.Item name="inputDate" label="Ngày" rules={[{ required: true }]}>
              <DatePicker format="DD/MM/YYYY" />
            </Form.Item>
            <Form.Item name="hours" label="Số giờ">
              <InputNumber min={0} />
            </Form.Item>
            <Form.Item name="sessions" label="Số ca">
              <InputNumber min={0} />
            </Form.Item>
            <Button type="primary" htmlType="submit" loading={createMutation.isPending}>
              Ghi
            </Button>
          </Form>
        </Card>
      ) : (
        <Card className="app-card" style={{ marginBottom: 16 }}>
          Chỉ Admin / Kỹ thuật được ghi nhật ký. Bạn vẫn có thể xem danh sách bên dưới.
        </Card>
      )}
      <Card className="app-card">
        <Form form={filterForm} layout="inline" style={{ marginBottom: 16 }}>
          <Form.Item name="deviceCode" label="Lọc thiết bị">
            <Select
              showSearch
              allowClear
              placeholder="Tất cả thiết bị"
              optionFilterProp="label"
              filterOption={false}
              onSearch={setDeviceSearch}
              notFoundContent={devicesQuery.isFetching ? 'Đang tải...' : 'Không có thiết bị'}
              options={deviceOptions}
              style={{ minWidth: 320 }}
              loading={devicesQuery.isFetching}
            />
          </Form.Item>
          <Button type="primary" loading={logsQuery.isFetching} onClick={() => logsQuery.refetch()}>
            Lọc
          </Button>
        </Form>
        {logsQuery.isError && (
          <div style={{ marginBottom: 12, color: '#b91c1c' }}>
            Lỗi khi tải dữ liệu. Kiểm tra API hoặc chọn thiết bị khác.
          </div>
        )}
        <Table
          rowKey="id"
          loading={logsQuery.isFetching}
          dataSource={logsQuery.isError ? [] : (logsQuery.data ?? [])}
          columns={[
            {
              title: 'Mã TB',
              width: 110,
              render: (_, r) => {
                const label = r.deviceCode || r.deviceId || '—';
                if (r.deviceId) {
                  return <Link to={`/devices/${r.deviceId}`}>{label}</Link>;
                }
                return label;
              },
            },
            {
              title: 'Tên thiết bị',
              dataIndex: 'deviceName',
              ellipsis: true,
              render: (name) => name || '—',
            },
            {
              title: 'Ngày',
              render: (_, r) => formatLogDate(r.inputDate),
            },
            { title: 'Giờ', dataIndex: 'hours' },
            { title: 'Ca', dataIndex: 'sessions' },
          ]}
        />
      </Card>
    </div>
  );
}
