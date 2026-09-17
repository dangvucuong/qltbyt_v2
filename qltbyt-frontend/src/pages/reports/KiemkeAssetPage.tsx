import { useEffect, useMemo, useState } from 'react';
import {
  Button,
  Card,
  Col,
  DatePicker,
  Form,
  InputNumber,
  Row,
  Select,
  Space,
  Table,
  Typography,
  message,
} from 'antd';
import { FileExcelOutlined, SearchOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import dayjs, { type Dayjs } from 'dayjs';
import axiosClient from '@/api/axiosClient';
import { PageHeader } from '@/components/PageHeader';
import { useAuthStore } from '@/store/authStore';
import type { ApiResponse, OrganizationItem, PagedResult } from '@/types/api';
import type { KiemkeAssetRow } from '@/types/phase3';

const PAGE_SIZE_OPTIONS = ['10', '20', '50', '100'];
const DEFAULT_PAGE_SIZE = 10;

const PROCUREMENT_OPTIONS = [
  { value: '1', label: 'Nguồn dự án' },
  { value: '2', label: 'Quỹ phát triển sự nghiệp' },
  { value: '3', label: 'Tài trợ - viện trợ' },
  { value: '4', label: 'Nguồn ngân sách' },
  { value: '5', label: 'Nguồn vay kích cầu' },
];

const LOCATION_OPTIONS = [
  { value: 'CS1', label: 'CS1' },
  { value: 'CS2', label: 'CS2' },
  { value: 'CS1+CS2', label: 'CS1+CS2' },
];

function formatMoney(v?: number) {
  return v != null ? Number(v).toLocaleString('vi-VN') : '';
}

function buildQuery(params: Record<string, string | number | boolean | undefined | null>) {
  const q = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v === undefined || v === null || v === '') return;
    if (typeof v === 'number' && Number.isNaN(v)) return;
    if (k !== 'excludeZeroAmount' && v === false) return;
    q.set(k, String(v));
  });
  return q.toString();
}

function normalizePaged<T>(payload: PagedResult<T> | undefined | null): PagedResult<T> {
  if (!payload) return { items: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0 };
  const raw = payload as PagedResult<T> & { Items?: T[]; TotalCount?: number };
  return {
    items: raw.items ?? raw.Items ?? [],
    totalCount: raw.totalCount ?? raw.TotalCount ?? 0,
    page: raw.page ?? 1,
    pageSize: raw.pageSize ?? 10,
    totalPages: raw.totalPages ?? 0,
  };
}

async function downloadFile(path: string, filename: string) {
  const res = await axiosClient.get(path, { responseType: 'blob' });
  const url = window.URL.createObjectURL(new Blob([res.data]));
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  window.URL.revokeObjectURL(url);
}

function paginationLabel(total: number, range: [number, number]) {
  return `${range[0]}-${range[1]} / ${total} bản ghi`;
}

export function KiemkeAssetPage() {
  const user = useAuthStore((s) => s.user);
  const isStaff = user?.role === 'Admin' || user?.role === 'Technician';
  const hideAmount = user?.role === 'User';

  const [departmentOrgId, setDepartmentOrgId] = useState<number | undefined>();
  const [deptPage, setDeptPage] = useState(1);
  const [deptPageSize, setDeptPageSize] = useState(DEFAULT_PAGE_SIZE);

  const [submitted, setSubmitted] = useState(false);
  const [orgId, setOrgId] = useState<number | undefined>();
  const [location, setLocation] = useState<string | undefined>();
  const [procurement, setProcurement] = useState<string | undefined>();
  const [receivedYear, setReceivedYear] = useState<Dayjs | null>(null);
  const [minAmount, setMinAmount] = useState<number | undefined>();
  const [maxAmount, setMaxAmount] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const orgsQuery = useQuery({
    queryKey: ['organizations'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<OrganizationItem[]>>('/api/organizations');
      return res.data.data ?? [];
    },
  });

  const orgOptions = useMemo(
    () => (orgsQuery.data ?? []).map((o) => ({ value: o.id, label: o.name })),
    [orgsQuery.data],
  );

  const deptParams = useMemo(
    () => ({
      organizationId: departmentOrgId,
      excludeZeroAmount: true,
      page: deptPage,
      pageSize: deptPageSize,
    }),
    [departmentOrgId, deptPage, deptPageSize],
  );

  const deptQuery = useQuery({
    queryKey: ['kiemke-department', deptParams],
    enabled: !isStaff && !!departmentOrgId,
    queryFn: async () => {
      const qs = buildQuery(deptParams);
      const res = await axiosClient.get<ApiResponse<PagedResult<KiemkeAssetRow>>>(
        `/api/asset-inventory?${qs}`,
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.message || 'Không tải được dữ liệu kiểm kê');
      }
      return normalizePaged(res.data.data);
    },
  });

  const inventoryParams = useMemo(
    () => ({
      organizationId: orgId,
      locationCode: location,
      procurementSource: procurement,
      receivedYear: receivedYear?.year(),
      minAmount,
      maxAmount,
      excludeZeroAmount: false,
      page,
      pageSize,
    }),
    [orgId, location, procurement, receivedYear, minAmount, maxAmount, page, pageSize],
  );

  const inventoryQuery = useQuery({
    queryKey: ['kiemke-inventory', inventoryParams, submitted],
    enabled: isStaff && submitted,
    queryFn: async () => {
      const qs = buildQuery(inventoryParams);
      const res = await axiosClient.get<ApiResponse<PagedResult<KiemkeAssetRow>>>(
        `/api/asset-inventory?${qs}`,
      );
      if (!res.data.success || !res.data.data) {
        throw new Error(res.data.message || 'Không tải được dữ liệu kiểm kê');
      }
      return normalizePaged(res.data.data);
    },
  });

  useEffect(() => {
    if (inventoryQuery.error) {
      message.error(inventoryQuery.error instanceof Error ? inventoryQuery.error.message : 'Tải dữ liệu thất bại');
    }
  }, [inventoryQuery.error]);

  useEffect(() => {
    if (deptQuery.error) {
      message.error(deptQuery.error instanceof Error ? deptQuery.error.message : 'Tải dữ liệu thất bại');
    }
  }, [deptQuery.error]);

  const baseColumns = [
    {
      title: 'Tên thiết bị',
      dataIndex: 'name',
      ellipsis: true,
      render: (v: string, row: KiemkeAssetRow) => (
        <Link to={`/devices/${row.id}`}>{v || row.id}</Link>
      ),
    },
    { title: 'Mã tài sản', dataIndex: 'identityCode', width: 110 },
    { title: 'Hãng SX', dataIndex: 'manufacturer', width: 120 },
    { title: 'Model', dataIndex: 'modelCode', width: 100 },
    { title: 'Serial', dataIndex: 'serial', width: 100 },
    { title: 'Số lượng', dataIndex: 'quantity', width: 80 },
    { title: 'ĐVT', dataIndex: 'unit', width: 70 },
    { title: 'Khoa phòng', dataIndex: 'organizationName', width: 140, ellipsis: true },
  ];

  const amountColumns = hideAmount
    ? []
    : [
        {
          title: 'Nguyên giá',
          dataIndex: 'totalAmount',
          width: 120,
          render: formatMoney,
        },
      ];

  const staffExtraColumns = isStaff
    ? [
        { title: 'Vị trí', dataIndex: 'locationCode', width: 90 },
        { title: 'Nguồn mua sắm', dataIndex: 'procurementSourceLabel', width: 160, ellipsis: true },
        {
          title: 'Tiếp nhận',
          dataIndex: 'receivedDate',
          width: 110,
          render: (v: string) => (v ? dayjs(v).format('DD/MM/YYYY') : ''),
        },
        {
          title: 'Khấu hao',
          dataIndex: 'depreciationAmount',
          width: 120,
          render: formatMoney,
        },
      ]
    : [];

  const columns = [
    ...baseColumns,
    ...amountColumns,
    ...staffExtraColumns,
    { title: 'Ghi chú', dataIndex: 'note', width: 120, ellipsis: true },
  ];

  const onExportDepartment = async () => {
    if (!departmentOrgId) {
      message.warning('Vui lòng chọn khoa/phòng');
      return;
    }
    try {
      const qs = buildQuery({ ...deptParams, page: 1, pageSize: 5000 });
      await downloadFile(`/api/asset-inventory/export?${qs}`, 'Thong-ke-TS-theo-khoa.xlsx');
      message.success('Đã xuất Excel');
    } catch {
      message.error('Xuất Excel thất bại');
    }
  };

  const onExportInventory = async () => {
    try {
      const qs = buildQuery({ ...inventoryParams, page: 1, pageSize: 5000 });
      await downloadFile(`/api/asset-inventory/export?${qs}`, 'Kiemke_TBYT.xlsx');
      message.success('Đã xuất Excel');
    } catch {
      message.error('Xuất Excel thất bại');
    }
  };

  const onRunInventory = () => {
    setPage(1);
    setSubmitted(true);
  };

  if (!isStaff) {
    return (
      <div>
        <PageHeader
          title="Kiểm kê tài sản"
          subtitle="Thống kê tài sản theo khoa phòng được phân quyền"
        />
        <Card className="app-card">
          <Space wrap style={{ marginBottom: 16 }}>
            <Typography.Text>Khoa, phòng:</Typography.Text>
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="Chọn khoa/phòng"
              style={{ minWidth: 280 }}
              loading={orgsQuery.isLoading}
              options={orgOptions}
              value={departmentOrgId}
              onChange={(v) => {
                setDepartmentOrgId(v);
                setDeptPage(1);
              }}
            />
            <Button type="primary" icon={<FileExcelOutlined />} onClick={onExportDepartment}>
              Xuất Excel
            </Button>
          </Space>
          <Table
            rowKey="id"
            size="small"
            loading={deptQuery.isLoading}
            dataSource={deptQuery.data?.items ?? []}
            columns={columns}
            scroll={{ x: 1200 }}
            pagination={{
              current: deptPage,
              pageSize: deptPageSize,
              total: deptQuery.data?.totalCount ?? 0,
              showSizeChanger: true,
              pageSizeOptions: PAGE_SIZE_OPTIONS,
              onChange: (p, ps) => {
                setDeptPage(p);
                setDeptPageSize(ps);
              },
              showTotal: paginationLabel,
            }}
          />
        </Card>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title="Kiểm kê tài sản"
        subtitle="Báo cáo kiểm kê thiết bị y tế — lọc đa tiêu chí và xuất Excel"
      />
      <Card className="app-card" title="Báo cáo kiểm kê thiết bị y tế">
        <Form layout="vertical">
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item label="Khoa phòng">
                <Select
                  allowClear
                  showSearch
                  optionFilterProp="label"
                  placeholder="Tất cả"
                  options={orgOptions}
                  loading={orgsQuery.isLoading}
                  value={orgId}
                  onChange={(v) => setOrgId(v ?? undefined)}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Vị trí (CS1 - CS2)">
                <Select allowClear placeholder="Tất cả" options={LOCATION_OPTIONS} value={location} onChange={(v) => setLocation(v ?? undefined)} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Nguồn mua sắm">
                <Select
                  allowClear
                  placeholder="Tất cả"
                  options={PROCUREMENT_OPTIONS}
                  value={procurement}
                  onChange={(v) => setProcurement(v ?? undefined)}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Thời điểm tiếp nhận (năm)">
                <DatePicker
                  picker="year"
                  style={{ width: '100%' }}
                  value={receivedYear}
                  onChange={(v) => setReceivedYear(v ?? null)}
                  placeholder="Chọn năm"
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Giá thấp nhất (VNĐ)">
                <InputNumber
                  style={{ width: '100%' }}
                  min={0}
                  formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(v) => Number((v ?? '').replace(/,/g, ''))}
                  value={minAmount}
                  onChange={(v) => setMinAmount(v == null || Number.isNaN(v) ? undefined : v)}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Giá cao nhất (VNĐ)">
                <InputNumber
                  style={{ width: '100%' }}
                  min={0}
                  formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(v) => Number((v ?? '').replace(/,/g, ''))}
                  value={maxAmount}
                  onChange={(v) => setMaxAmount(v == null || Number.isNaN(v) ? undefined : v)}
                />
              </Form.Item>
            </Col>
          </Row>
          <Space>
            <Button type="primary" icon={<SearchOutlined />} onClick={onRunInventory}>
              Thực hiện
            </Button>
            <Button icon={<FileExcelOutlined />} onClick={onExportInventory} disabled={!submitted}>
              Xuất Excel
            </Button>
          </Space>
        </Form>
      </Card>
      {submitted && (
        <Card className="app-card" style={{ marginTop: 16 }}>
          <Table
            rowKey="id"
            size="small"
            loading={inventoryQuery.isLoading}
            dataSource={inventoryQuery.data?.items ?? []}
            columns={columns}
            scroll={{ x: 1500 }}
            pagination={{
              current: page,
              pageSize,
              total: inventoryQuery.data?.totalCount ?? 0,
              showSizeChanger: true,
              pageSizeOptions: PAGE_SIZE_OPTIONS,
              onChange: (p, ps) => {
                setPage(p);
                setPageSize(ps);
              },
              showTotal: paginationLabel,
            }}
          />
        </Card>
      )}
    </div>
  );
}
