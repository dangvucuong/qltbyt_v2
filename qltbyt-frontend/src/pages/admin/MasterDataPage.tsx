import { Button, Card, Input, Modal, Popconfirm, Space, Table, Tabs, message } from 'antd';
import { PlusOutlined, SearchOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, MasterItem } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

type CatalogKey =
  | 'device-types'
  | 'status'
  | 'org-types'
  | 'countries'
  | 'task-types'
  | 'event-keywords'
  | 'personnel-types'
  | 'document-types';

interface CatalogDef {
  key: CatalogKey;
  label: string;
  title: string;
  getPath: string;
  adminPath: string;
  queryKey: string;
}

const CATALOGS: CatalogDef[] = [
  {
    key: 'device-types',
    label: 'Loại thiết bị',
    title: 'Loại thiết bị',
    getPath: '/api/master/device-types',
    adminPath: '/api/admin/master/device-types',
    queryKey: 'master-device-types',
  },
  {
    key: 'status',
    label: 'Trạng thái TB',
    title: 'Trạng thái thiết bị',
    getPath: '/api/master/status',
    adminPath: '/api/admin/master/device-statuses',
    queryKey: 'master-status',
  },
  {
    key: 'org-types',
    label: 'Loại đơn vị',
    title: 'Loại đơn vị',
    getPath: '/api/master/org-types',
    adminPath: '/api/admin/master/org-types',
    queryKey: 'master-org-types',
  },
  {
    key: 'countries',
    label: 'Nước sản xuất',
    title: 'Nước sản xuất',
    getPath: '/api/master/countries',
    adminPath: '/api/admin/master/countries',
    queryKey: 'master-countries',
  },
  {
    key: 'task-types',
    label: 'Loại nhiệm vụ',
    title: 'Loại nhiệm vụ / tác vụ',
    getPath: '/api/master/task-types',
    adminPath: '/api/admin/master/task-types',
    queryKey: 'master-task-types',
  },
  {
    key: 'event-keywords',
    label: 'Từ khóa sự cố',
    title: 'Từ khóa sự cố',
    getPath: '/api/master/event-keywords',
    adminPath: '/api/admin/master/event-keywords',
    queryKey: 'master-event-keywords',
  },
  {
    key: 'personnel-types',
    label: 'Loại nhân sự',
    title: 'Loại nhân sự',
    getPath: '/api/master/personnel-types',
    adminPath: '/api/admin/master/personnel-types',
    queryKey: 'master-personnel-types',
  },
  {
    key: 'document-types',
    label: 'Loại hồ sơ',
    title: 'Loại hồ sơ',
    getPath: '/api/master/document-types',
    adminPath: '/api/admin/master/document-types',
    queryKey: 'master-document-types',
  },
];

function MasterTable({
  title,
  items,
  onAdd,
  onSave,
  onDelete,
  loading,
}: {
  title: string;
  items: MasterItem[];
  onAdd: (name: string) => void;
  onSave: (id: number, name: string) => void;
  onDelete: (id: number) => void;
  loading?: boolean;
}) {
  const [name, setName] = useState('');
  const [filter, setFilter] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(15);
  const [editing, setEditing] = useState<MasterItem | null>(null);
  const [editName, setEditName] = useState('');

  const filteredItems = useMemo(() => {
    const term = filter.trim().toLowerCase();
    if (!term) return items;
    return items.filter((i) => (i.name ?? '').toLowerCase().includes(term));
  }, [items, filter]);

  const openEdit = (row: MasterItem) => {
    setEditing(row);
    setEditName(row.name ?? '');
  };

  const closeEdit = () => {
    setEditing(null);
    setEditName('');
  };

  const confirmEdit = () => {
    const v = editName.trim();
    if (!v) {
      message.warning('Nhập tên danh mục');
      return;
    }
    if (!editing) return;
    if (v === (editing.name ?? '').trim()) {
      closeEdit();
      return;
    }
    onSave(editing.id, v);
    closeEdit();
  };

  return (
    <Card className="app-card" title={title}>
      <Space wrap style={{ marginBottom: 16 }} size="middle">
        <Input
          allowClear
          prefix={<SearchOutlined />}
          placeholder="Tìm theo tên danh mục..."
          value={filter}
          onChange={(e) => {
            setFilter(e.target.value);
            setPage(1);
          }}
          style={{ width: 280 }}
        />
        <Input
          placeholder="Tên danh mục mới"
          value={name}
          onChange={(e) => setName(e.target.value)}
          onPressEnter={() => {
            if (!name.trim()) {
              message.warning('Nhập tên');
              return;
            }
            onAdd(name.trim());
            setName('');
          }}
          style={{ width: 240 }}
        />
        <Button
          type="primary"
          icon={<PlusOutlined />}
          loading={loading}
          onClick={() => {
            if (!name.trim()) {
              message.warning('Nhập tên');
              return;
            }
            onAdd(name.trim());
            setName('');
          }}
        >
          Thêm
        </Button>
      </Space>
      <Table
        rowKey="id"
        dataSource={filteredItems}
        pagination={{
          current: page,
          pageSize,
          showSizeChanger: true,
          pageSizeOptions: ['10', '15', '20', '50'],
          showTotal: (total) => `Tổng ${total} mục`,
          onChange: (nextPage, nextSize) => {
            setPage(nextPage);
            setPageSize(nextSize || 15);
          },
          onShowSizeChange: (_current, size) => {
            setPage(1);
            setPageSize(size);
          },
        }}
        columns={[
          {
            title: 'ID',
            dataIndex: 'id',
            width: 80,
          },
          {
            title: 'Tên',
            dataIndex: 'name',
            ellipsis: true,
          },
          {
            title: 'Thao tác',
            width: 160,
            render: (_, r) => (
              <Space>
                <Button size="small" onClick={() => openEdit(r)}>
                  Sửa
                </Button>
                <Popconfirm title="Xóa mục này?" onConfirm={() => onDelete(r.id)}>
                  <Button size="small" danger>
                    Xóa
                  </Button>
                </Popconfirm>
              </Space>
            ),
          },
        ]}
      />
      <Modal
        open={editing != null}
        title="Sửa danh mục"
        okText="Lưu"
        cancelText="Hủy"
        confirmLoading={loading}
        onCancel={closeEdit}
        onOk={confirmEdit}
        destroyOnClose
      >
        <Input
          autoFocus
          value={editName}
          onChange={(e) => setEditName(e.target.value)}
          placeholder="Tên danh mục"
          onPressEnter={confirmEdit}
        />
      </Modal>
    </Card>
  );
}

function CatalogTab({ catalog }: { catalog: CatalogDef }) {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: [catalog.queryKey],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<MasterItem[]>>(catalog.getPath);
      return res.data.data ?? [];
    },
  });

  const mutate = useMutation({
    mutationFn: async (fn: () => Promise<unknown>) => fn(),
    onSuccess: (_data, _vars, _ctx) => {
      message.success('Cập nhật danh mục thành công');
      queryClient.invalidateQueries({ queryKey: [catalog.queryKey] });
      // Đồng bộ cache dùng chung (form đơn vị, thiết bị…)
      queryClient.invalidateQueries({ queryKey: ['master'] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Thao tác thất bại');
    },
  });

  if (query.isLoading) return <LoadingCenter />;

  return (
    <MasterTable
      title={catalog.title}
      items={query.data ?? []}
      loading={mutate.isPending}
      onAdd={(name) =>
        mutate.mutate(() => axiosClient.post(catalog.adminPath, { name }))
      }
      onSave={(id, name) =>
        mutate.mutate(() => axiosClient.put(`${catalog.adminPath}/${id}`, { name }))
      }
      onDelete={(id) =>
        mutate.mutate(() => axiosClient.delete(`${catalog.adminPath}/${id}`))
      }
    />
  );
}

export function MasterDataPage() {
  return (
    <div>
      <PageHeader
        title="Danh mục hệ thống"
        subtitle="Quản lý loại thiết bị, trạng thái, đơn vị, nước SX, nhiệm vụ, từ khóa sự cố, nhân sự, hồ sơ"
      />
      <Tabs
        destroyInactiveTabPane
        items={CATALOGS.map((c) => ({
          key: c.key,
          label: c.label,
          children: <CatalogTab catalog={c} />,
        }))}
      />
    </div>
  );
}
