import {
  Button,
  Card,
  Col,
  Form,
  Input,
  InputNumber,
  Modal,
  Row,
  Select,
  Statistic,
  Table,
  Tabs,
  message,
} from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

type Commodity = { id: number; code?: string; name?: string; activeIngredient?: string; stockOnHand: number };
type Batch = { id: number; batchCode?: string };
type StockImport = {
  id: number;
  commodityId: number;
  commodityName?: string;
  batchId?: number;
  batchCode?: string;
  quantity?: number;
  stockBalance?: number;
  expiryDate?: string;
};
type StockExport = { id: number; importId: number; quantity?: number; note?: string; createdAt?: string };
type WarehouseReport = {
  totalCommodities: number;
  totalStockUnits: number;
  importLines: number;
  exportLines: number;
};

export function WarehousePage() {
  const queryClient = useQueryClient();
  const [commodityOpen, setCommodityOpen] = useState(false);
  const [batchOpen, setBatchOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [exportOpen, setExportOpen] = useState(false);
  const [commodityForm] = Form.useForm();
  const [batchForm] = Form.useForm();
  const [importForm] = Form.useForm();
  const [exportForm] = Form.useForm();

  const commoditiesQuery = useQuery({
    queryKey: ['warehouse-commodities'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<Commodity[]>>('/api/warehouse/commodities');
      return res.data.data ?? [];
    },
  });

  const batchesQuery = useQuery({
    queryKey: ['warehouse-batches'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<Batch[]>>('/api/warehouse/batches');
      return res.data.data ?? [];
    },
  });

  const importsQuery = useQuery({
    queryKey: ['warehouse-imports'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<StockImport[]>>('/api/warehouse/imports');
      return res.data.data ?? [];
    },
  });

  const exportsQuery = useQuery({
    queryKey: ['warehouse-exports'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<StockExport[]>>('/api/warehouse/exports');
      return res.data.data ?? [];
    },
  });

  const reportQuery = useQuery({
    queryKey: ['warehouse-report'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<WarehouseReport>>('/api/warehouse/report');
      return res.data.data!;
    },
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['warehouse-commodities'] });
    queryClient.invalidateQueries({ queryKey: ['warehouse-imports'] });
    queryClient.invalidateQueries({ queryKey: ['warehouse-exports'] });
    queryClient.invalidateQueries({ queryKey: ['warehouse-report'] });
  };

  const createCommodity = useMutation({
    mutationFn: async (values: { code: string; name: string; activeIngredient?: string }) =>
      axiosClient.post('/api/warehouse/commodities', values),
    onSuccess: () => {
      message.success('Thêm hàng hóa thành công');
      setCommodityOpen(false);
      commodityForm.resetFields();
      invalidate();
    },
    onError: (e: { response?: { data?: { message?: string } } }) =>
      message.error(e.response?.data?.message ?? 'Thêm hàng hóa thất bại'),
  });

  const createBatch = useMutation({
    mutationFn: async (values: { batchCode: string }) =>
      axiosClient.post('/api/warehouse/batches', values),
    onSuccess: () => {
      message.success('Thêm lô hàng thành công');
      setBatchOpen(false);
      batchForm.resetFields();
      queryClient.invalidateQueries({ queryKey: ['warehouse-batches'] });
    },
    onError: () => message.error('Thêm lô thất bại'),
  });

  const importStock = useMutation({
    mutationFn: async (values: {
      commodityId: number;
      batchId?: number;
      quantity: number;
      expiryDate?: string;
    }) => axiosClient.post('/api/warehouse/imports', values),
    onSuccess: () => {
      message.success('Nhập kho thành công');
      setImportOpen(false);
      importForm.resetFields();
      invalidate();
    },
    onError: (e: { response?: { data?: { message?: string } } }) =>
      message.error(e.response?.data?.message ?? 'Nhập kho thất bại'),
  });

  const exportStock = useMutation({
    mutationFn: async (values: { importId: number; quantity: number; note?: string }) =>
      axiosClient.post('/api/warehouse/exports', values),
    onSuccess: () => {
      message.success('Xuất kho thành công');
      setExportOpen(false);
      exportForm.resetFields();
      invalidate();
    },
    onError: (e: { response?: { data?: { message?: string } } }) =>
      message.error(e.response?.data?.message ?? 'Xuất kho thất bại'),
  });

  if (commoditiesQuery.isLoading || reportQuery.isLoading) return <LoadingCenter />;
  const commodities = commoditiesQuery.data ?? [];
  const batches = batchesQuery.data ?? [];
  const imports = importsQuery.data ?? [];

  return (
    <div>
      <PageHeader title="Kho TVT / thuốc" subtitle="Nhập xuất lô hàng và tồn kho" />
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic title="Mặt hàng" value={reportQuery.data?.totalCommodities ?? 0} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic title="Tồn kho (đơn vị)" value={reportQuery.data?.totalStockUnits ?? 0} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic title="Dòng nhập" value={reportQuery.data?.importLines ?? 0} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="app-card">
            <Statistic title="Dòng xuất" value={reportQuery.data?.exportLines ?? 0} />
          </Card>
        </Col>
      </Row>

      <Card className="app-card" style={{ marginTop: 16 }}>
        <Tabs
          items={[
            {
              key: 'commodities',
              label: 'Hàng hóa',
              children: (
                <>
                  <Button type="primary" icon={<PlusOutlined />} onClick={() => setCommodityOpen(true)}>
                    Thêm hàng hóa
                  </Button>
                  <Table
                    style={{ marginTop: 12 }}
                    rowKey="id"
                    dataSource={commodities}
                    columns={[
                      { title: 'Mã', dataIndex: 'code', width: 120 },
                      { title: 'Tên', dataIndex: 'name' },
                      { title: 'Hoạt chất', dataIndex: 'activeIngredient', ellipsis: true },
                      { title: 'Tồn', dataIndex: 'stockOnHand', width: 80 },
                    ]}
                    pagination={{ pageSize: 10 }}
                  />
                </>
              ),
            },
            {
              key: 'imports',
              label: 'Nhập kho',
              children: (
                <>
                  <Button type="primary" icon={<PlusOutlined />} onClick={() => setImportOpen(true)}>
                    Nhập kho
                  </Button>
                  <Table
                    style={{ marginTop: 12 }}
                    rowKey="id"
                    dataSource={imports}
                    columns={[
                      { title: 'Hàng', dataIndex: 'commodityName' },
                      { title: 'Lô', dataIndex: 'batchCode', width: 120 },
                      { title: 'SL nhap', dataIndex: 'quantity', width: 90 },
                      { title: 'Tồn', dataIndex: 'stockBalance', width: 80 },
                    ]}
                    pagination={{ pageSize: 10 }}
                  />
                </>
              ),
            },
            {
              key: 'exports',
              label: 'Xuất kho',
              children: (
                <>
                  <Button type="primary" icon={<PlusOutlined />} onClick={() => setExportOpen(true)}>
                    Xuất kho
                  </Button>
                  <Table
                    style={{ marginTop: 12 }}
                    rowKey="id"
                    loading={exportsQuery.isLoading}
                    dataSource={exportsQuery.data ?? []}
                    columns={[
                      { title: 'ID nhập', dataIndex: 'importId', width: 100 },
                      { title: 'SL xuat', dataIndex: 'quantity', width: 90 },
                      { title: 'Ghi chu', dataIndex: 'note', ellipsis: true },
                    ]}
                    pagination={{ pageSize: 10 }}
                  />
                </>
              ),
            },
          ]}
        />
      </Card>

      <Modal title="Thêm hàng hóa" open={commodityOpen} onCancel={() => setCommodityOpen(false)} onOk={() => commodityForm.submit()} confirmLoading={createCommodity.isPending}>
        <Form form={commodityForm} layout="vertical" onFinish={(v) => createCommodity.mutate(v)}>
          <Form.Item name="code" label="Mã hàng" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="name" label="Tên hàng" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="activeIngredient" label="Hoạt chất">
            <Input />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Thêm lô" open={batchOpen} onCancel={() => setBatchOpen(false)} onOk={() => batchForm.submit()} confirmLoading={createBatch.isPending}>
        <Form form={batchForm} layout="vertical" onFinish={(v) => createBatch.mutate(v)}>
          <Form.Item name="batchCode" label="Mã lô" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Nhập kho" open={importOpen} onCancel={() => setImportOpen(false)} onOk={() => importForm.submit()} confirmLoading={importStock.isPending}>
        <Form form={importForm} layout="vertical" onFinish={(v) => importStock.mutate(v)}>
          <Form.Item name="commodityId" label="Hàng hóa" rules={[{ required: true }]}>
            <Select
              options={commodities.map((c) => ({ value: c.id, label: `${c.code} - ${c.name}` }))}
            />
          </Form.Item>
          <Form.Item name="batchId" label="Lô hàng">
            <Select allowClear options={batches.map((b) => ({ value: b.id, label: b.batchCode }))} />
          </Form.Item>
          <Form.Item name="quantity" label="Số lượng" rules={[{ required: true }]}>
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Xuất kho" open={exportOpen} onCancel={() => setExportOpen(false)} onOk={() => exportForm.submit()} confirmLoading={exportStock.isPending}>
        <Form form={exportForm} layout="vertical" onFinish={(v) => exportStock.mutate(v)}>
          <Form.Item name="importId" label="Dòng nhập" rules={[{ required: true }]}>
            <Select
              options={imports
                .filter((i) => (i.stockBalance ?? 0) > 0)
                .map((i) => ({
                  value: i.id,
                  label: `${i.commodityName} (tồn ${i.stockBalance})`,
                }))}
            />
          </Form.Item>
          <Form.Item name="quantity" label="Số lượng" rules={[{ required: true }]}>
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="note" label="Ghi chu">
            <Input />
          </Form.Item>
        </Form>
      </Modal>

      <Button style={{ marginTop: 12 }} onClick={() => setBatchOpen(true)}>Thêm lô hàng</Button>
    </div>
  );
}