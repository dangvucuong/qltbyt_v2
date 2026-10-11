import { Button, Card, DatePicker, Form, InputNumber, Space, Statistic, Table, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs, { type Dayjs } from 'dayjs';
import { useMemo, useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { UsageLogItem } from '@/types/phase2';

const { RangePicker } = DatePicker;

function formatLogDate(value?: string) {
  if (!value) return '—';
  const d = dayjs(value);
  return d.isValid() ? d.format('DD/MM/YYYY') : '—';
}

interface DeviceUsageLogsPanelProps {
  deviceId: number;
  canEdit: boolean;
}

export function DeviceUsageLogsPanel({ deviceId, canEdit }: DeviceUsageLogsPanelProps) {
  const queryClient = useQueryClient();
  const [form] = Form.useForm();
  const [range, setRange] = useState<[Dayjs | null, Dayjs | null] | null>(null);

  const logsQuery = useQuery({
    queryKey: ['device-usage-logs', deviceId, range?.[0]?.format('YYYY-MM-DD'), range?.[1]?.format('YYYY-MM-DD')],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (range?.[0]) params.set('fromDate', range[0].startOf('day').toISOString());
      if (range?.[1]) params.set('toDate', range[1].endOf('day').toISOString());
      const qs = params.toString();
      const res = await axiosClient.get<ApiResponse<UsageLogItem[]>>(
        `/api/devices/${deviceId}/usage-logs${qs ? `?${qs}` : ''}`,
      );
      return res.data.data ?? [];
    },
    enabled: deviceId > 0,
  });

  const saveMutation = useMutation({
    mutationFn: async (values: {
      inputDate: Dayjs;
      hours?: number | null;
      sessions?: number | null;
    }) => {
      await axiosClient.post(`/api/devices/${deviceId}/usage-logs`, {
        hours: values.hours ?? undefined,
        sessions: values.sessions ?? undefined,
        inputDate: values.inputDate.startOf('day').format('YYYY-MM-DD'),
      });
    },
    onSuccess: () => {
      message.success('Ghi nhật ký thành công');
      form.resetFields();
      form.setFieldsValue({ inputDate: dayjs() });
      queryClient.invalidateQueries({ queryKey: ['device-usage-logs', deviceId] });
      queryClient.invalidateQueries({ queryKey: ['usage-logs'] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Ghi nhật ký thất bại');
    },
  });

  const totals = useMemo(() => {
    const rows = logsQuery.data ?? [];
    return {
      hours: rows.reduce((s, r) => s + (r.hours ?? 0), 0),
      sessions: rows.reduce((s, r) => s + (r.sessions ?? 0), 0),
      count: rows.length,
    };
  }, [logsQuery.data]);

  const loadDayIntoForm = (row: UsageLogItem) => {
    form.setFieldsValue({
      inputDate: row.inputDate ? dayjs(row.inputDate) : dayjs(),
      hours: row.hours,
      sessions: row.sessions,
    });
  };

  return (
    <Space direction="vertical" size="middle" style={{ width: '100%' }}>
      {canEdit && (
        <Card size="small" type="inner" title="Ghi nhật ký theo ngày">
          <Form
            form={form}
            layout="inline"
            initialValues={{ inputDate: dayjs() }}
            onFinish={(v) => saveMutation.mutate(v)}
          >
            <Form.Item
              name="inputDate"
              label="Ngày"
              rules={[{ required: true, message: 'Chọn ngày' }]}
            >
              <DatePicker format="DD/MM/YYYY" />
            </Form.Item>
            <Form.Item name="hours" label="Số giờ">
              <InputNumber min={0} style={{ width: 100 }} />
            </Form.Item>
            <Form.Item name="sessions" label="Số ca">
              <InputNumber min={0} style={{ width: 100 }} />
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit" loading={saveMutation.isPending}>
                Lưu nhật ký
              </Button>
            </Form.Item>
          </Form>
          <div style={{ marginTop: 8, color: '#64748b', fontSize: 13 }}>
            Nếu ngày đã có dữ liệu thì hệ thống sẽ cập nhật (giống SudungTB).
          </div>
        </Card>
      )}

      <Space wrap style={{ width: '100%', justifyContent: 'space-between' }}>
        <RangePicker
          format="DD/MM/YYYY"
          value={range}
          onChange={(v) => setRange(v)}
          allowEmpty={[true, true]}
        />
        <Space size="large">
          <Statistic title="Số dòng" value={totals.count} />
          <Statistic title="Tổng giờ" value={totals.hours} />
          <Statistic title="Tổng ca" value={totals.sessions} />
        </Space>
      </Space>

      <Table
        rowKey="id"
        loading={logsQuery.isFetching}
        dataSource={logsQuery.data ?? []}
        pagination={{ pageSize: 15, showTotal: (t) => `Tổng ${t} dòng` }}
        locale={{ emptyText: 'Chưa có nhật ký sử dụng' }}
        columns={[
          {
            title: 'Ngày',
            width: 120,
            render: (_, r) => formatLogDate(r.inputDate),
          },
          { title: 'Số giờ', dataIndex: 'hours', width: 100 },
          { title: 'Số ca', dataIndex: 'sessions', width: 100 },
          ...(canEdit
            ? [
                {
                  title: '',
                  width: 100,
                  render: (_: unknown, r: UsageLogItem) => (
                    <Button size="small" onClick={() => loadDayIntoForm(r)}>
                      Sửa
                    </Button>
                  ),
                },
              ]
            : []),
        ]}
      />
    </Space>
  );
}
