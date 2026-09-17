import { Form, Input, Modal, Select } from 'antd';
import type { OrganizationItem } from '@/types/api';

export interface TransferDeviceFormValues {
  toOrganizationId: number;
  handoverOrganizationId?: number;
  note?: string;
}

interface Props {
  open: boolean;
  organizations: OrganizationItem[];
  loading?: boolean;
  onClose: () => void;
  onSubmit: (values: TransferDeviceFormValues) => void;
}

export function TransferDeviceModal({ open, organizations, loading, onClose, onSubmit }: Props) {
  const [form] = Form.useForm<TransferDeviceFormValues>();

  return (
    <Modal
      open={open}
      title="Chuyển giao thiết bị"
      okText="Chuyển giao"
      cancelText="Hủy"
      confirmLoading={loading}
      onCancel={() => {
        form.resetFields();
        onClose();
      }}
      onOk={() => form.submit()}
    >
      <Form form={form} layout="vertical" onFinish={onSubmit}>
        <Form.Item
          name="toOrganizationId"
          label="Đơn vị nhận"
          rules={[{ required: true, message: 'Chọn đơn vị nhận' }]}
        >
          <Select
            showSearch
            optionFilterProp="label"
            options={organizations.map((o) => ({
              value: o.id,
              label: o.name ?? String(o.id),
            }))}
          />
        </Form.Item>
        <Form.Item name="handoverOrganizationId" label="Đơn vị bàn giao (nếu có)">
          <Select
            allowClear
            showSearch
            optionFilterProp="label"
            options={organizations.map((o) => ({
              value: o.id,
              label: o.name ?? String(o.id),
            }))}
          />
        </Form.Item>
        <Form.Item name="note" label="Ghi chú">
          <Input.TextArea rows={3} placeholder="Lý do / biên bản chuyển giao" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
