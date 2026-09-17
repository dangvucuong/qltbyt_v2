import { useMemo, useState } from 'react';
import {
  Button,
  Collapse,
  DatePicker,
  InputNumber,
  Select,
  Space,
  Table,
  Typography,
  message,
} from 'antd';
import { FileExcelOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import dayjs, { type Dayjs } from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, PagedResult } from '@/types/api';
import type { ColumnsType } from 'antd/es/table';
import type { BaocaoAmountOption, BaocaoAssetRow, BaocaoOperationalRow } from '@/types/phase3';

const PAGE_SIZE_OPTIONS = ['10', '20', '50', '100'];
const DEFAULT_PAGE_SIZE = 10;

async function downloadFile(path: string, filename: string) {
  const res = await axiosClient.get(path, { responseType: 'blob' });
  const url = window.URL.createObjectURL(new Blob([res.data]));
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  window.URL.revokeObjectURL(url);
}

function formatMoney(v?: number) {
  return v != null ? Number(v).toLocaleString('vi-VN') : '';
}

function buildQuery(params: Record<string, string | number | undefined>) {
  const q = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== '') q.set(k, String(v));
  });
  return q.toString();
}

function paginationLabel(total: number, range: [number, number]) {
  return `${range[0]}-${range[1]} / ${total} bản ghi`;
}

const PROCUREMENT_OPTIONS = [
  { value: '1', label: 'Nguồn dự án' },
  { value: '2', label: 'Quỹ phát triển sự nghiệp' },
  { value: '3', label: 'Tài trợ - viện trợ' },
  { value: '4', label: 'Nguồn ngân sách' },
  { value: '5', label: 'Nguồn vay kích cầu' },
];

const CLASSIFICATION_OPTIONS = ['A', 'B', 'C', 'D'].map((v) => ({ value: v, label: v }));

const LOCATION_OPTIONS = [
  { value: 'CS1', label: 'CS1' },
  { value: 'CS2', label: 'CS2' },
  { value: 'CS1+CS2', label: 'CS1+CS2' },
];

const STATUS_OPTIONS = [
  { value: '1', label: 'Hoạt động bình thường' },
  { value: '2', label: 'Không hoạt động' },
  { value: '3', label: 'Chưa đưa vào sử dụng' },
  { value: '4', label: 'Hoạt động, cần sửa chữa nhỏ' },
];

const DEVICE_GROUP_OPTIONS = [
  { value: 'Chẩn đoán', label: 'Chẩn đoán' },
  { value: 'Điều trị', label: 'Điều trị' },
  { value: 'Phân tích', label: 'Phân tích' },
  { value: 'Thiết bị', label: 'Thiết bị' },
];

const MAINTENANCE_OPTIONS = [
  { value: '1', label: 'Tự bảo trì' },
  { value: '2', label: 'Bảo trì cơ bản' },
  { value: '3', label: 'Bảo trì trọn gói' },
  { value: '4', label: 'Bảo trì cơ bản có thay thế linh kiện' },
];

const baseDeviceColumns = [
  { title: 'Tên TB', dataIndex: 'name', ellipsis: true },
  { title: 'Hãng SX', dataIndex: 'manufacturer', width: 120 },
  { title: 'Ký hiệu', dataIndex: 'modelCode', width: 100 },
  { title: 'Serial', dataIndex: 'serial', width: 100 },
  { title: 'Đơn vị SD', dataIndex: 'organizationName', width: 140, ellipsis: true },
  { title: 'Số lượng', dataIndex: 'quantity', width: 80 },
  { title: 'ĐVT', dataIndex: 'unit', width: 70 },
  { title: 'Mã định danh', dataIndex: 'identityCode', width: 110 },
];

const assetColumns = [
  ...baseDeviceColumns,
  { title: 'Nhóm lọc', dataIndex: 'filterLabel', width: 160, ellipsis: true },
  { title: 'Thành tiền', dataIndex: 'totalAmount', width: 120, render: formatMoney },
  {
    title: 'Ngày tiếp nhận',
    dataIndex: 'receivedDate',
    width: 110,
    render: (v: string) => (v ? dayjs(v).format('DD/MM/YYYY') : ''),
  },
  { title: 'Khấu hao', dataIndex: 'depreciationAmount', width: 120, render: formatMoney },
];

interface AssetSectionProps {
  active: boolean;
  section: 'procurement' | 'classification' | 'location';
  filterLabel: string;
  options: { value: string; label: string }[];
  defaultValue: string;
  exportName: string;
}

function AssetReportSection({
  active,
  section,
  filterLabel,
  options,
  defaultValue,
  exportName,
}: AssetSectionProps) {
  const [value, setValue] = useState(defaultValue);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const query = useQuery({
    queryKey: ['baocao-asset', section, value, page, pageSize],
    enabled: active,
    queryFn: async () => {
      const qs = buildQuery({ section, value, page, pageSize });
      const res = await axiosClient.get<ApiResponse<PagedResult<BaocaoAssetRow>>>(
        `/api/reports/baocao/assets?${qs}`,
      );
      return res.data.data!;
    },
  });

  const onExport = async () => {
    try {
      const qs = buildQuery({ section, value, page: 1, pageSize: 5000 });
      await downloadFile(`/api/reports/baocao/assets/export?${qs}`, exportName);
      message.success('Đã xuất Excel');
    } catch {
      message.error('Xuất Excel thất bại');
    }
  };

  return (
    <>
      <Space wrap style={{ marginBottom: 12 }}>
        <Typography.Text>{filterLabel}</Typography.Text>
        <Select
          style={{ minWidth: 240 }}
          value={value}
          options={options}
          onChange={(v) => {
            setValue(v);
            setPage(1);
          }}
        />
        <Button type="primary" icon={<FileExcelOutlined />} onClick={onExport}>
          Xuất Excel
        </Button>
      </Space>
      <Table
        rowKey="id"
        size="small"
        loading={query.isLoading}
        dataSource={query.data?.items ?? []}
        columns={assetColumns}
        scroll={{ x: 1400 }}
        pagination={{
          current: page,
          pageSize,
          total: query.data?.totalCount ?? 0,
          showSizeChanger: true,
          pageSizeOptions: PAGE_SIZE_OPTIONS,
          onChange: (nextPage, nextSize) => {
            setPage(nextPage);
            setPageSize(nextSize);
          },
          showTotal: paginationLabel,
        }}
      />
    </>
  );
}

interface OperationalSectionProps {
  active: boolean;
  section: string;
  filterLabel?: string;
  options?: { value: string; label: string }[];
  defaultValue?: string;
  exportName: string;
  buildParams: (
    page: number,
    pageSize: number,
    value: string,
  ) => Record<string, string | number | undefined>;
  columns: ColumnsType<BaocaoOperationalRow>;
}

function OperationalReportSection({
  active,
  section,
  filterLabel,
  options,
  defaultValue,
  exportName,
  buildParams,
  columns,
}: OperationalSectionProps) {
  const [value, setValue] = useState(defaultValue ?? '');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const params = useMemo(
    () => buildParams(page, pageSize, value),
    [buildParams, page, pageSize, value],
  );

  const query = useQuery({
    queryKey: ['baocao-operational', section, params],
    enabled: active,
    queryFn: async () => {
      const qs = buildQuery({ section, ...params });
      const res = await axiosClient.get<ApiResponse<PagedResult<BaocaoOperationalRow>>>(
        `/api/reports/baocao/operational?${qs}`,
      );
      return res.data.data!;
    },
  });

  const onExport = async () => {
    try {
      const qs = buildQuery({ section, ...buildParams(1, 5000, value), pageSize: 5000 });
      await downloadFile(`/api/reports/baocao/operational/export?${qs}`, exportName);
      message.success('Đã xuất Excel');
    } catch {
      message.error('Xuất Excel thất bại');
    }
  };

  return (
    <>
      <Space wrap style={{ marginBottom: 12 }}>
        {filterLabel && options && (
          <>
            <Typography.Text>{filterLabel}</Typography.Text>
            <Select
              style={{ minWidth: 240 }}
              value={value}
              options={options}
              onChange={(v) => {
                setValue(v);
                setPage(1);
              }}
            />
          </>
        )}
        <Button type="primary" icon={<FileExcelOutlined />} onClick={onExport}>
          Xuất Excel
        </Button>
      </Space>
      <Table
        rowKey={(r: BaocaoOperationalRow, i) => `${r.id ?? 'f'}-${i}`}
        size="small"
        loading={query.isLoading}
        dataSource={query.data?.items ?? []}
        columns={columns}
        scroll={{ x: 1300 }}
        pagination={{
          current: page,
          pageSize,
          total: query.data?.totalCount ?? 0,
          showSizeChanger: true,
          pageSizeOptions: PAGE_SIZE_OPTIONS,
          onChange: (nextPage, nextSize) => {
            setPage(nextPage);
            setPageSize(nextSize);
          },
          showTotal: paginationLabel,
        }}
      />
    </>
  );
}

function UsageSection({ active }: { active: boolean }) {
  const [range, setRange] = useState<[Dayjs, Dayjs]>([dayjs().subtract(30, 'day'), dayjs()]);
  const [minAmount, setMinAmount] = useState<number | undefined>();
  const [maxAmount, setMaxAmount] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const from = range[0].format('YYYY-MM-DD');
  const to = range[1].format('YYYY-MM-DD');

  const amountOptionsQuery = useQuery({
    queryKey: ['baocao-amount-options', from, to],
    enabled: active,
    queryFn: async () => {
      const qs = buildQuery({ from, to });
      const res = await axiosClient.get<ApiResponse<BaocaoAmountOption[]>>(
        `/api/reports/baocao/usage/amount-options?${qs}`,
      );
      return res.data.data ?? [];
    },
  });

  const params = useMemo(
    () => ({ value: undefined, from, to, minAmount, maxAmount, page, pageSize }),
    [from, to, minAmount, maxAmount, page, pageSize],
  );

  const query = useQuery({
    queryKey: ['baocao-operational', 'usage', params],
    enabled: active,
    queryFn: async () => {
      const qs = buildQuery({ section: 'usage', ...params });
      const res = await axiosClient.get<ApiResponse<PagedResult<BaocaoOperationalRow>>>(
        `/api/reports/baocao/operational?${qs}`,
      );
      return res.data.data!;
    },
  });

  const onExport = async () => {
    try {
      const qs = buildQuery({
        section: 'usage',
        from,
        to,
        minAmount,
        maxAmount,
        page: 1,
        pageSize: 5000,
      });
      await downloadFile(`/api/reports/baocao/operational/export?${qs}`, 'Hieu-suat-TB.xlsx');
      message.success('Đã xuất Excel');
    } catch {
      message.error('Xuất Excel thất bại');
    }
  };

  const usageColumns = [
    ...baseDeviceColumns,
    { title: 'Thành tiền', dataIndex: 'totalAmount', width: 120, render: formatMoney },
    { title: 'Số giờ', dataIndex: 'usageHours', width: 90 },
    { title: 'Số ca', dataIndex: 'usageSessions', width: 90 },
  ];

  return (
    <>
      <Space wrap style={{ marginBottom: 12 }}>
        <Typography.Text>Khoảng ngày:</Typography.Text>
        <DatePicker.RangePicker
          value={range}
          format="DD/MM/YYYY"
          onChange={(v) => {
            if (v?.[0] && v[1]) {
              setRange([v[0], v[1]]);
              setPage(1);
            }
          }}
        />
        <Typography.Text>Nguyên giá từ:</Typography.Text>
        <Select
          allowClear
          placeholder="Chọn mức giá"
          style={{ width: 160 }}
          loading={amountOptionsQuery.isLoading}
          options={(amountOptionsQuery.data ?? []).map((o) => ({
            value: o.amount,
            label: o.display,
          }))}
          value={minAmount}
          onChange={(v) => {
            setMinAmount(v);
            setPage(1);
          }}
        />
        <Typography.Text>đến:</Typography.Text>
        <Select
          allowClear
          placeholder="Chọn mức giá"
          style={{ width: 160 }}
          loading={amountOptionsQuery.isLoading}
          options={(amountOptionsQuery.data ?? []).map((o) => ({
            value: o.amount,
            label: o.display,
          }))}
          value={maxAmount}
          onChange={(v) => {
            setMaxAmount(v);
            setPage(1);
          }}
        />
        <Button type="primary" icon={<FileExcelOutlined />} onClick={onExport}>
          Xuất Excel
        </Button>
      </Space>
      <Table
        rowKey={(r: BaocaoOperationalRow) => String(r.id)}
        size="small"
        loading={query.isLoading}
        dataSource={query.data?.items ?? []}
        columns={usageColumns}
        scroll={{ x: 1200 }}
        pagination={{
          current: page,
          pageSize,
          total: query.data?.totalCount ?? 0,
          showSizeChanger: true,
          pageSizeOptions: PAGE_SIZE_OPTIONS,
          onChange: (nextPage, nextSize) => {
            setPage(nextPage);
            setPageSize(nextSize);
          },
          showTotal: paginationLabel,
        }}
      />
    </>
  );
}

function InspectionSection({ active }: { active: boolean }) {
  const [days, setDays] = useState(30);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const params = useMemo(
    () => ({ value: undefined, daysUntilInspection: days, page, pageSize }),
    [days, page, pageSize],
  );

  const query = useQuery({
    queryKey: ['baocao-operational', 'inspection', params],
    enabled: active,
    queryFn: async () => {
      const qs = buildQuery({ section: 'inspection', ...params });
      const res = await axiosClient.get<ApiResponse<PagedResult<BaocaoOperationalRow>>>(
        `/api/reports/baocao/operational?${qs}`,
      );
      return res.data.data!;
    },
  });

  const onExport = async () => {
    try {
      const qs = buildQuery({
        section: 'inspection',
        daysUntilInspection: days,
        page: 1,
        pageSize: 5000,
      });
      await downloadFile(`/api/reports/baocao/operational/export?${qs}`, 'TB-sap-kiem-dinh.xlsx');
      message.success('Đã xuất Excel');
    } catch {
      message.error('Xuất Excel thất bại');
    }
  };

  const columns = [
    ...baseDeviceColumns,
    {
      title: 'Hạn kiểm định',
      dataIndex: 'inspectionExpiryDate',
      width: 120,
      render: (v: string) => (v ? dayjs(v).format('DD/MM/YYYY') : ''),
    },
    { title: 'Ghi chú', dataIndex: 'filterLabel', width: 120 },
  ];

  return (
    <>
      <Space wrap style={{ marginBottom: 12 }}>
        <Typography.Text>Cảnh báo trước (ngày):</Typography.Text>
        <InputNumber
          min={1}
          max={365}
          value={days}
          onChange={(v) => {
            setDays(v ?? 30);
            setPage(1);
          }}
        />
        <Button type="primary" icon={<FileExcelOutlined />} onClick={onExport}>
          Xuất Excel
        </Button>
      </Space>
      <Table
        rowKey={(r: BaocaoOperationalRow, i) => `${r.id ?? 'f'}-${i}`}
        size="small"
        loading={query.isLoading}
        dataSource={query.data?.items ?? []}
        columns={columns}
        scroll={{ x: 1200 }}
        pagination={{
          current: page,
          pageSize,
          total: query.data?.totalCount ?? 0,
          showSizeChanger: true,
          pageSizeOptions: PAGE_SIZE_OPTIONS,
          onChange: (nextPage, nextSize) => {
            setPage(nextPage);
            setPageSize(nextSize);
          },
          showTotal: paginationLabel,
        }}
      />
    </>
  );
}

export function BaocaoStatisticsTab() {
  const [assetOpenKeys, setAssetOpenKeys] = useState<string[]>([]);
  const [operationalOpenKeys, setOperationalOpenKeys] = useState<string[]>([]);
  const [groupOpenKeys, setGroupOpenKeys] = useState<string[]>([]);

  const operationalBasicColumns = [
    ...baseDeviceColumns,
    { title: 'Nhóm lọc', dataIndex: 'filterLabel', width: 180, ellipsis: true },
    { title: 'Thành tiền', dataIndex: 'totalAmount', width: 120, render: formatMoney },
  ];

  const downtimeColumns = [
    ...baseDeviceColumns,
    { title: 'Người báo', dataIndex: 'filterLabel', width: 120 },
    {
      title: 'Bắt đầu hỏng',
      dataIndex: 'breakdownStartedAt',
      width: 140,
      render: (v: string) => (v ? dayjs(v).format('DD/MM/YYYY HH:mm') : ''),
    },
    { title: 'Sửa xong', dataIndex: 'repairedAt', width: 140 },
    { title: 'Thời gian dừng', dataIndex: 'downtimeNote', width: 140 },
  ];

  return (
    <div className="printable-report">
      <Collapse
        destroyInactivePanel
        activeKey={groupOpenKeys}
        onChange={(keys) => setGroupOpenKeys(Array.isArray(keys) ? keys : [keys])}
        items={[
          {
            key: 'asset',
            label: 'Báo cáo tài sản',
            children: (
              <Collapse
                destroyInactivePanel
                accordion
                activeKey={assetOpenKeys}
                onChange={(keys) => setAssetOpenKeys(Array.isArray(keys) ? keys : keys ? [keys] : [])}
                items={[
                  {
                    key: 'procurement',
                    label: 'Thống kê thiết bị theo nguồn mua sắm',
                    children: (
                      <AssetReportSection
                        active={assetOpenKeys.includes('procurement')}
                        section="procurement"
                        filterLabel="Lựa chọn nguồn mua sắm:"
                        options={PROCUREMENT_OPTIONS}
                        defaultValue="1"
                        exportName="TB-theo-nguon-von.xlsx"
                      />
                    ),
                  },
                  {
                    key: 'classification',
                    label: 'Thống kê thiết bị theo phân loại',
                    children: (
                      <AssetReportSection
                        active={assetOpenKeys.includes('classification')}
                        section="classification"
                        filterLabel="Lựa chọn phân loại thiết bị:"
                        options={CLASSIFICATION_OPTIONS}
                        defaultValue="C"
                        exportName="TB-theo-phan-loai.xlsx"
                      />
                    ),
                  },
                  {
                    key: 'location',
                    label: 'Thống kê thiết bị theo vị trí',
                    children: (
                      <AssetReportSection
                        active={assetOpenKeys.includes('location')}
                        section="location"
                        filterLabel="Lựa chọn vị trí thiết bị:"
                        options={LOCATION_OPTIONS}
                        defaultValue="CS1"
                        exportName="TB-theo-vi-tri.xlsx"
                      />
                    ),
                  },
                ]}
              />
            ),
          },
          {
            key: 'operational',
            label: 'Báo cáo hoạt động chuyên môn',
            children: (
              <Collapse
                destroyInactivePanel
                accordion
                activeKey={operationalOpenKeys}
                onChange={(keys) =>
                  setOperationalOpenKeys(Array.isArray(keys) ? keys : keys ? [keys] : [])
                }
                items={[
                  {
                    key: 'status',
                    label: 'Thống kê thiết bị theo trạng thái',
                    children: (
                      <OperationalReportSection
                        active={operationalOpenKeys.includes('status')}
                        section="status"
                        filterLabel="Lựa chọn trạng thái thiết bị:"
                        options={STATUS_OPTIONS}
                        defaultValue="2"
                        exportName="Thong-ke-TB-theo-tinh-trang.xlsx"
                        buildParams={(p, ps, v) => ({ value: v, page: p, pageSize: ps })}
                        columns={operationalBasicColumns}
                      />
                    ),
                  },
                  {
                    key: 'devicegroup',
                    label: 'Thống kê thiết bị theo phân nhóm',
                    children: (
                      <OperationalReportSection
                        active={operationalOpenKeys.includes('devicegroup')}
                        section="devicegroup"
                        filterLabel="Lựa chọn phân nhóm thiết bị:"
                        options={DEVICE_GROUP_OPTIONS}
                        defaultValue="Điều trị"
                        exportName="TB-theo-phan-nhom.xlsx"
                        buildParams={(p, ps, v) => ({ value: v, page: p, pageSize: ps })}
                        columns={operationalBasicColumns}
                      />
                    ),
                  },
                  {
                    key: 'maintenance',
                    label: 'Thống kê thiết bị theo hình thức bảo trì',
                    children: (
                      <OperationalReportSection
                        active={operationalOpenKeys.includes('maintenance')}
                        section="maintenance"
                        filterLabel="Lựa chọn hình thức bảo trì:"
                        options={MAINTENANCE_OPTIONS}
                        defaultValue="3"
                        exportName="TB-theo-HT-bao-tri.xlsx"
                        buildParams={(p, ps, v) => ({ value: v, page: p, pageSize: ps })}
                        columns={operationalBasicColumns}
                      />
                    ),
                  },
                  {
                    key: 'inspection',
                    label: 'Thiết bị sắp hết hạn kiểm định',
                    children: (
                      <InspectionSection active={operationalOpenKeys.includes('inspection')} />
                    ),
                  },
                  {
                    key: 'downtime',
                    label: 'Thống kê thời gian thiết bị không hoạt động',
                    children: (
                      <OperationalReportSection
                        active={operationalOpenKeys.includes('downtime')}
                        section="downtime"
                        exportName="Thong-ke-TB-theo-thoi-gian-khong-HD.xlsx"
                        buildParams={(p, ps) => ({ page: p, pageSize: ps })}
                        columns={downtimeColumns}
                      />
                    ),
                  },
                  {
                    key: 'usage',
                    label: 'Thống kê hiệu suất sử dụng thiết bị',
                    children: <UsageSection active={operationalOpenKeys.includes('usage')} />,
                  },
                ]}
              />
            ),
          },
        ]}
      />
    </div>
  );
}
