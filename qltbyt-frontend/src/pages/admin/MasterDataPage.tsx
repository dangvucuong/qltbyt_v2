import { Button, Card, Input, Modal, Popconfirm, Space, Table, Tabs, message } from 'antd';
import { PlusOutlined, SearchOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, MasterItem } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

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

export function MasterDataPage() {
  const queryClient = useQueryClient();

  const typesQuery = useQuery({
    queryKey: ['master-types'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<MasterItem[]>>('/api/master/device-types');
      return res.data.data ?? [];
    },
  });

  const statusQuery = useQuery({
    queryKey: ['master-status'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<MasterItem[]>>('/api/master/status');
      return res.data.data ?? [];
    },
  });

  const mutate = useMutation({
    mutationFn: async (fn: () => Promise<unknown>) => fn(),
    onSuccess: () => {
      message.success('Cập nhật danh mục thành công');
      queryClient.invalidateQueries({ queryKey: ['master-types'] });
      queryClient.invalidateQueries({ queryKey: ['master-status'] });
    },
    onError: (err: { response?: { data?: { message?: string } } }) => {
      message.error(err.response?.data?.message ?? 'Thao tác thất bại');
    },
  });

  if (typesQuery.isLoading || statusQuery.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader title="Danh mục hệ thống" subtitle="Loại thiết bị và trạng thái" />
      <Tabs
        items={[
          {
            key: 'types',
            label: 'Loại thiết bị',
            children: (
              <MasterTable
                title="Loại thiết bị"
                items={typesQuery.data ?? []}
                loading={mutate.isPending}
                onAdd={(name) =>
                  mutate.mutate(() =>
                    axiosClient.post('/api/admin/master/device-types', { name }),
                  )
                }
                onSave={(id, name) =>
                  mutate.mutate(() =>
                    axiosClient.put(`/api/admin/master/device-types/${id}`, { name }),
                  )
                }
                onDelete={(id) =>
                  mutate.mutate(() => axiosClient.delete(`/api/admin/master/device-types/${id}`))
                }
              />
            ),
          },
          {
            key: 'status',
            label: 'Trạng thái thiết bị',
            children: (
              <MasterTable
                title="Trạng thái"
                items={statusQuery.data ?? []}
                loading={mutate.isPending}
                onAdd={(name) =>
                  mutate.mutate(() =>
                    axiosClient.post('/api/admin/master/device-statuses', { name }),
                  )
                }
                onSave={(id, name) =>
                  mutate.mutate(() =>
                    axiosClient.put(`/api/admin/master/device-statuses/${id}`, { name }),
                  )
                }
                onDelete={(id) =>
                  mutate.mutate(() =>
                    axiosClient.delete(`/api/admin/master/device-statuses/${id}`),
                  )
                }
              />
            ),
          },
        ]}
      />
    </div>
  );
}
