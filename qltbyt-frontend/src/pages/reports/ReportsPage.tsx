import { Button, Card, Col, DatePicker, Form, Input, Row, Select, Space, Statistic, Table, Tabs, message } from 'antd';
import {
  DownloadOutlined,
  FileExcelOutlined,
  PrinterOutlined,
  SearchOutlined,
} from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router-dom';
import dayjs from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, MasterItem, OrganizationItem, PagedResult } from '@/types/api';
import type { DeviceReportRow, DeviceReportSummary } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';
import { BaocaoStatisticsTab } from '@/pages/reports/BaocaoStatisticsTab';

async function downloadFile(path: string, filename: string) {
  const res = await axiosClient.get(path, { responseType: 'blob' });
  const url = window.URL.createObjectURL(new Blob([res.data]));
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  window.URL.revokeObjectURL(url);
}

function ReportGridTab() {
  const [params, setParams] = useSearchParams();
  const page = Number(params.get('page') || 1);
  const search = params.get('search') || '';
  const organizationId = params.get('organizationId');
  const statusId = params.get('statusId');
  const typeId = params.get('typeId');

  const gridQuery = useQuery({
    queryKey: ['report-grid', page, search, organizationId, statusId, typeId],
    queryFn: async () => {
      const q = new URLSearchParams({ page: String(page), pageSize: '20' });
      if (search) q.set('search', search);
      if (organizationId) q.set('organizationId', organizationId);
      if (statusId) q.set('statusId', statusId);
      if (typeId) q.set('typeId', typeId);
      const res = await axiosClient.get<ApiResponse<PagedResult<DeviceReportRow>>>(
        `/api/reports/devices/grid?${q}`,
      );
      return res.data.data!;
    },
  });

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

  const data = gridQuery.data;

  return (
    <Card className="app-card">
      <Space wrap style={{ marginBottom: 16 }}>
        <Input
          allowClear
          placeholder="Tìm mã, tên, serial..."
          prefix={<SearchOutlined />}
          value={search}
          onChange={(e) => {
            const next = new URLSearchParams(params);
            if (e.target.value) next.set('search', e.target.value);
            else next.delete('search');
            next.set('page', '1');
            setParams(next);
          }}
          style={{ width: 220 }}
        />
        <Select
          allowClear
          placeholder="Đơn vị"
          style={{ width: 180 }}
          value={organizationId ? Number(organizationId) : undefined}
          options={(orgs.data ?? []).map((o) => ({ value: o.id, label: o.name }))}
          onChange={(v) => {
            const next = new URLSearchParams(params);
            if (v) next.set('organizationId', String(v));
            else next.delete('organizationId');
            next.set('page', '1');
            setParams(next);
          }}
        />
        <Select
          allowClear
          placeholder="Loại TB"
          style={{ width: 160 }}
          value={typeId ? Number(typeId) : undefined}
          options={(masterTypes.data ?? []).map((t) => ({ value: t.id, label: t.name }))}
          onChange={(v) => {
            const next = new URLSearchParams(params);
            if (v) next.set('typeId', String(v));
            else next.delete('typeId');
            next.set('page', '1');
            setParams(next);
          }}
        />
        <Select
          allowClear
          placeholder="Trạng thái"
          style={{ width: 160 }}
          value={statusId ? Number(statusId) : undefined}
          options={(masterStatus.data ?? []).map((s) => ({ value: s.id, label: s.name }))}
          onChange={(v) => {
            const next = new URLSearchParams(params);
            if (v) next.set('statusId', String(v));
            else next.delete('statusId');
            next.set('page', '1');
            setParams(next);
          }}
        />
      </Space>
      <Table
        rowKey="id"
        loading={gridQuery.isLoading}
        dataSource={data?.items ?? []}
        scroll={{ x: 1200 }}
        pagination={{
          current: page,
          pageSize: 20,
          total: data?.totalCount ?? 0,
          onChange: (p) => {
            const next = new URLSearchParams(params);
            next.set('page', String(p));
            setParams(next);
          },
        }}
        columns={[
          {
            title: 'Mã TB',
            dataIndex: 'deviceCode',
            render: (v, row) => <Link to={`/devices/${row.id}`}>{v || row.id}</Link>,
          },
          { title: 'Tên', dataIndex: 'name', ellipsis: true },
          { title: 'Loại', dataIndex: 'typeName' },
          { title: 'Đơn vị', dataIndex: 'organizationName' },
          { title: 'Trạng thái', dataIndex: 'statusName' },
          {
            title: 'Hết hạn KD',
            dataIndex: 'inspectionExpiryDate',
            render: (v) => (v ? dayjs(v).format('DD/MM/YYYY') : ''),
          },
          {
            title: 'Bảo trì',
            dataIndex: 'maintenanceDueDate',
            render: (v) => (v ? dayjs(v).format('DD/MM/YYYY') : ''),
          },
          {
            title: 'Giá trị (VNĐ)',
            dataIndex: 'totalAmount',
            render: (v) => (v != null ? Number(v).toLocaleString('vi-VN') : ''),
          },
        ]}
      />
    </Card>
  );
}

function WarehouseExportTab() {
  const [form] = Form.useForm();

  const onExport = async () => {
    const values = form.getFieldsValue();
    const from = values.range?.[0]?.format('YYYY-MM-DD');
    const to = values.range?.[1]?.format('YYYY-MM-DD');
    const q = new URLSearchParams();
    if (from) q.set('from', from);
    if (to) q.set('to', to);
    try {
      await downloadFile(`/api/reports/warehouse/export?${q}`, 'Bao-cao-kho-TVT.xlsx');
      message.success('Đã tải báo cáo kho');
    } catch {
      message.error('Xuất báo cáo kho thất bại');
    }
  };

  return (
    <Card className="app-card" title="Báo cáo kho TVT theo ngày">
      <Form
        form={form}
        layout="inline"
        initialValues={{
          range: [dayjs().subtract(1, 'month'), dayjs()],
        }}
      >
        <Form.Item name="range" label="Từ ngày — đến ngày">
          <DatePicker.RangePicker format="DD/MM/YYYY" />
        </Form.Item>
        <Form.Item>
          <Button type="primary" icon={<FileExcelOutlined />} onClick={onExport}>
            Xuất Excel
          </Button>
        </Form.Item>
      </Form>
    </Card>
  );
}

export function ReportsPage() {
  const summaryQuery = useQuery({
    queryKey: ['report-summary'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<DeviceReportSummary>>(
        '/api/reports/devices/summary?expiringWithinDays=30',
      );
      return res.data.data!;
    },
  });

  const handleExport = async (path: string, name: string) => {
    try {
      await downloadFile(path, name);
      message.success('Đã tải file Excel');
    } catch {
      message.error('Xuất báo cáo thất bại');
    }
  };

  if (summaryQuery.isLoading) return <LoadingCenter />;
  const s = summaryQuery.data;

  const summaryTab = (
    <div className="printable-report">
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic title="Tổng thiết bị" value={s?.totalDevices ?? 0} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic
              title="Sắp hết hạn kiểm định (30 ngày)"
              value={s?.expiringInspectionCount ?? 0}
              valueStyle={{ color: '#d97706' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic title="Cần bảo trì (theo mốc TB)" value={s?.maintenanceDueCount ?? 0} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic title="Đang hoạt động" value={s?.byStatusActive ?? 0} />
          </Card>
        </Col>
      </Row>
      <Card className="app-card" title="Xuất Excel" style={{ marginTop: 16 }}>
        <Button
          icon={<FileExcelOutlined />}
          onClick={() => handleExport('/api/reports/devices/export', 'Danh-sach-thiet-bi.xlsx')}
        >
          Danh sách thiết bị
        </Button>
        <Button
          style={{ marginLeft: 8 }}
          icon={<DownloadOutlined />}
          onClick={() =>
            handleExport('/api/reports/assets/export', 'Bao-cao-tai-san-thiet-bi.xlsx')
          }
        >
          Báo cáo tài sản thiết bị
        </Button>
        <Button
          style={{ marginLeft: 8 }}
          icon={<DownloadOutlined />}
          onClick={() =>
            handleExport(
              '/api/reports/devices/expiring/export?withinDays=90',
              'Thiet-bi-sap-het-han-kiem-dinh.xlsx',
            )
          }
        >
          Thiết bị sắp hết hạn kiểm định (90 ngày)
        </Button>
        <Button
          style={{ marginLeft: 8 }}
          icon={<DownloadOutlined />}
          onClick={() => handleExport('/api/reports/events/export', 'Su-co-yeu-cau.xlsx')}
        >
          Sự cố / yêu cầu
        </Button>
      </Card>
    </div>
  );

  return (
    <div>
      <PageHeader
        title="Báo cáo thiết bị"
        subtitle="Tổng hợp, lưới lọc và xuất Excel"
        extra={
          <Button icon={<PrinterOutlined />} onClick={() => window.print()}>
            In báo cáo
          </Button>
        }
      />
      <Tabs
        defaultActiveKey="summary"
        items={[
          { key: 'summary', label: 'Tổng hợp', children: summaryTab },
          { key: 'baocao', label: 'Số liệu tổng hợp', children: <BaocaoStatisticsTab /> },
          { key: 'grid', label: 'Lưới báo cáo', children: <ReportGridTab /> },
          { key: 'warehouse', label: 'Kho TVT', children: <WarehouseExportTab /> },
        ]}
      />
    </div>
  );
}
