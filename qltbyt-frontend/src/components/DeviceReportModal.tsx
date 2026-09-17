import { Form, Input, Modal, Select, message } from 'antd';
import { useMutation } from '@tanstack/react-query';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { CreateDeviceEventPayload, EventDetail } from '@/types/events';

const EVENT_TYPES = [
  { value: 1, label: 'Báo hỏng' },
  { value: 2, label: 'Kiểm tra' },
  { value: 3, label: 'Yêu cầu khác' },
  { value: 4, label: 'Chuyển giao' },
];

type Props = {
  open: boolean;
  deviceId: number;
  onClose: () => void;
  onSuccess?: () => void;
};

export function DeviceReportModal({ open, deviceId, onClose, onSuccess }: Props) {
  const [form] = Form.useForm<CreateDeviceEventPayload>();

  const mutation = useMutation({
    mutationFn: async (values: CreateDeviceEventPayload) => {
      const res = await axiosClient.post<ApiResponse<EventDetail>>(
        `/api/devices/${deviceId}/events`,
        values,
      );
      return res.data;
    },
    onSuccess: (data) => {
      if (data.success) {
        message.success(data.message || 'Gửi yêu cầu thành công');
        form.resetFields();
        onClose();
        onSuccess?.();
      } else {
        message.error(data.message || 'Gửi yêu cầu thất bại');
      }
    },
    onError: () => message.error('Gửi yêu cầu thất bại'),
  });

  return (
    <Modal
      title="Gửi yêu cầu / báo sự cố"
      open={open}
      onCancel={onClose}
      confirmLoading={mutation.isPending}
      onOk={() => form.submit()}
      okText="Gửi"
      cancelText="Hủy"
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{ eventTypeId: 1 }}
        onFinish={(v) => mutation.mutate(v)}
      >
        <Form.Item name="eventTypeId" label="Loại" rules={[{ required: true }]}>
          <Select options={EVENT_TYPES} />
        </Form.Item>
        <Form.Item name="keyword" label="Từ khóa">
          <Input placeholder="Báo hỏng, kiểm tra..." />
        </Form.Item>
        <Form.Item
          name="content"
          label="Nội dung"
          rules={[{ required: true, message: 'Nhập nội dung yêu cầu' }]}
        >
          <Input.TextArea rows={4} placeholder="Mô tả sự cố hoặc yêu cầu" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
