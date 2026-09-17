import { Form, Input, InputNumber, Modal } from 'antd';
import type { EventListItem } from '@/types/events';

type Props = {
  open: boolean;
  event: EventListItem | null;
  loading?: boolean;
  onClose: () => void;
  onSubmit: (payload: { resolutionContent: string; cost?: number }) => void;
};

export function ResolveEventModal({ open, event, loading, onClose, onSubmit }: Props) {
  const [form] = Form.useForm();

  return (
    <Modal
      title={event ? `Xử lý: ${event.deviceCode ?? event.id}` : 'Xử lý sự cố'}
      open={open}
      onCancel={onClose}
      confirmLoading={loading}
      onOk={() => form.submit()}
      okText="Lưu xử lý"
      cancelText="Hủy"
      destroyOnClose
      afterOpenChange={(visible) => {
        if (visible) {
          form.setFieldsValue({ resolutionContent: 'Đã xử lý', cost: 0 });
        } else {
          form.resetFields();
        }
      }}
    >
      {event?.content && (
        <p style={{ marginBottom: 12, color: '#64748b' }}>Nội dung báo: {event.content}</p>
      )}
      <Form
        form={form}
        layout="vertical"
        onFinish={(v) => onSubmit({ resolutionContent: v.resolutionContent, cost: v.cost })}
      >
        <Form.Item
          name="resolutionContent"
          label="Nội dung xử lý"
          rules={[{ required: true, message: 'Nhập nội dung xử lý' }]}
        >
          <Input.TextArea rows={4} />
        </Form.Item>
        <Form.Item name="cost" label="Chi phí">
          <InputNumber min={0} style={{ width: '100%' }} />
        </Form.Item>
      </Form>
    </Modal>
  );
}
