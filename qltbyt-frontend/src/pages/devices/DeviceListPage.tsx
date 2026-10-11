import { Button, Card, Input, Select, Space, Table, Tag, Tooltip, Upload, message } from 'antd';
import { PlusOutlined, SearchOutlined, UploadOutlined } from '@ant-design/icons';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router-dom';
import dayjs from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type {
  ApiResponse,
  DeviceListItem,
  MasterItem,
  OrganizationItem,
  PagedResult,
} from '@/types/api';
import type { BiddingPackageItem } from '@/types/phase3';
import { PageHeader } from '@/components/PageHeader';
import { useAuthStore } from '@/store/authStore';

function statusTagColor(name?: string) {
  if (!name) return 'default';
  const n = name.toLowerCase();
  if (n.includes('hoạt') || n.includes('hoat')) return 'success';
  if (n.includes('hỏng') || n.includes('hong')) return 'error';
  return 'processing';
}

export function DeviceListPage() {
  const [params, setParams] = useSearchParams();
  const role = useAuthStore((s) => s.user?.role);
  const canImport = role === 'Admin' || role === 'Technician';
  const page = Number(params.get('page') || 1);
  const pageSizeRaw = Number(params.get('pageSize') || 20);
  const pageSize = [10, 20, 50, 100].includes(pageSizeRaw) ? pageSizeRaw : 20;
  const search = params.get('search') || '';
  const organizationId = params.get('organizationId');
  const statusId = params.get('statusId');
  const typeId = params.get('typeId');
  const biddingPackageCode = params.get('biddingPackageCode') || '';

  const devicesQuery = useQuery({
    queryKey: ['devices', page, pageSize, search, organizationId, statusId, typeId, biddingPackageCode],
    queryFn: async () => {
      const q = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (search) q.set('search', search);
      if (organizationId) q.set('organizationId', organizationId);
      if (statusId) q.set('statusId', statusId);
      if (typeId) q.set('typeId', typeId);
      if (biddingPackageCode) q.set('biddingPackageCode', biddingPackageCode);
      const res = await axiosClient.get<ApiResponse<PagedResult<DeviceListItem>>>(
        `/api/devices?${q}`,
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

  const biddingPackages = useQuery({
    queryKey: ['bidding-packages-lookup'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<BiddingPackageItem[]>>('/api/bidding');
      return res.data.data ?? [];
    },
  });

  function biddingPackageLabel(pkg: BiddingPackageItem) {
    const code = pkg.packageCode?.trim();
    const name = pkg.name?.trim();
    if (code && name) return `Gói ${code} - ${name}`;
    return name || code || '—';
  }

  const biddingPackageOptions =
    biddingPackages.data
      ?.map((p) => {
        const code = p.packageCode?.trim();
        if (!code) return null;
        return { value: code, label: biddingPackageLabel(p) };
      })
      .filter((o): o is { value: string; label: string } => o !== null) ?? [];

  const data = devicesQuery.data;

  const importMutation = useMutation({
    mutationFn: async (file: File) => {
      const form = new FormData();
      form.append('file', file);
      await axiosClient.post('/api/devices/import', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    onSuccess: () => {
      message.success('Import thiết bị từ Excel thành công');
      devicesQuery.refetch();
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Import thất bại');
    },
  });

  return (
    <div>
      <PageHeader
        title="Danh sách thiết bị"
        subtitle="Tìm kiếm, lọc và quản lý thiết bị y tế"
        extra={
          <Space>
            {canImport && (
              <Upload
                accept=".xlsx,.xls"
                showUploadList={false}
                beforeUpload={(file) => {
                  importMutation.mutate(file);
                  return false;
                }}
              >
                <Button icon={<UploadOutlined />} loading={importMutation.isPending}>
                  Import Excel
                </Button>
              </Upload>
            )}
            <Link to="/devices/new">
              <Button type="primary" icon={<PlusOutlined />} size="middle">
                Thêm thiết bị
              </Button>
            </Link>
          </Space>
        }
      />
      <div className="app-filter-bar">
        <Space wrap size="middle">
          <Input.Search
            placeholder="Tìm mã TB, tên thiết bị..."
            allowClear
            defaultValue={search}
            enterButton={<SearchOutlined />}
            onSearch={(v) => {
              const next = new URLSearchParams(params);
              next.set('page', '1');
              if (v) next.set('search', v);
              else next.delete('search');
              setParams(next);
            }}
            style={{ width: 280 }}
          />
          <Select
            placeholder="Đơn vị sử dụng"
            allowClear
            showSearch
            optionFilterProp="label"
            style={{ width: 200 }}
            value={organizationId ? Number(organizationId) : undefined}
            options={orgs.data?.map((o) => ({ value: o.id, label: o.name }))}
            onChange={(v) => {
              const next = new URLSearchParams(params);
              next.set('page', '1');
              if (v) next.set('organizationId', String(v));
              else next.delete('organizationId');
              setParams(next);
            }}
          />
          <Select
            placeholder="Trạng thái"
            allowClear
            style={{ width: 180 }}
            value={statusId ? Number(statusId) : undefined}
            options={masterStatus.data?.map((s) => ({ value: s.id, label: s.name }))}
            onChange={(v) => {
              const next = new URLSearchParams(params);
              next.set('page', '1');
              if (v) next.set('statusId', String(v));
              else next.delete('statusId');
              setParams(next);
            }}
          />
          <Select
            placeholder="Loại thiết bị"
            allowClear
            style={{ width: 180 }}
            value={typeId ? Number(typeId) : undefined}
            options={masterTypes.data?.map((t) => ({ value: t.id, label: t.name }))}
            onChange={(v) => {
              const next = new URLSearchParams(params);
              next.set('page', '1');
              if (v) next.set('typeId', String(v));
              else next.delete('typeId');
              setParams(next);
            }}
          />
          <Select
            placeholder="Gói thầu"
            allowClear
            showSearch
            optionFilterProp="label"
            loading={biddingPackages.isLoading}
            style={{ width: 320 }}
            value={biddingPackageCode || undefined}
            options={biddingPackageOptions}
            onChange={(v) => {
              const next = new URLSearchParams(params);
              next.set('page', '1');
              if (v) next.set('biddingPackageCode', v);
              else next.delete('biddingPackageCode');
              setParams(next);
            }}
          />
        </Space>
      </div>
      <Card className="app-card" bodyStyle={{ padding: 0 }}>
        <Table
          rowKey="id"
          size="middle"
          tableLayout="fixed"
          loading={devicesQuery.isLoading}
          dataSource={data?.items ?? []}
          pagination={{
            current: page,
            pageSize,
            total: data?.totalCount ?? 0,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50', '100'],
            showTotal: (t) => `Tổng ${t} thiết bị`,
            onChange: (nextPage, nextSize) => {
              const size = nextSize || pageSize;
              const next = new URLSearchParams(params);
              const sizeChanged = size !== pageSize;
              next.set('page', String(sizeChanged ? 1 : nextPage));
              if (size === 20) next.delete('pageSize');
              else next.set('pageSize', String(size));
              setParams(next);
            },
          }}
          columns={[
            {
              title: 'Mã TB',
              width: 110,
              ellipsis: true,
              render: (_, row) => (
                <Link to={`/devices/${row.id}`}>{row.deviceCode || row.id}</Link>
              ),
            },
            {
              title: 'Tên thiết bị',
              dataIndex: 'name',
              ellipsis: true,
              render: (name, row) => (
                <Link to={`/devices/${row.id}`}>{name || '—'}</Link>
              ),
            },
            { title: 'Loại', dataIndex: 'typeName', width: 140 },
            { title: 'Đơn vị', dataIndex: 'organizationName', ellipsis: true },
            {
              title: 'Trạng thái',
              dataIndex: 'statusName',
              width: 150,
              ellipsis: { showTitle: false },
              render: (t) => {
                if (!t) return '—';
                return (
                  <Tooltip title={t}>
                    <Tag color={statusTagColor(t)} className="device-list-status-tag">
                      <span className="device-list-status-text">{t}</span>
                    </Tag>
                  </Tooltip>
                );
              },
            },
            {
              title: 'Hạn kiểm định',
              dataIndex: 'inspectionExpiryDate',
              width: 120,
              align: 'right',
              render: (d) => (d ? dayjs(d).format('DD/MM/YYYY') : '—'),
            },
            {
              title: 'Thao tác',
              width: 100,
              render: (_, row) => (
                <Link to={`/devices/${row.id}/edit`}>Chỉnh sửa</Link>
              ),
            },
          ]}
        />
      </Card>
    </div>
  );
}
