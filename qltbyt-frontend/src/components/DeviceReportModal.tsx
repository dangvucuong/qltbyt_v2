import { AutoComplete, Button, Form, Input, Modal, Select, Upload, message } from 'antd';
import { UploadOutlined } from '@ant-design/icons';
import { useMutation, useQuery } from '@tanstack/react-query';
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

  const keywordsQuery = useQuery({
    queryKey: ['master-event-keywords'],
    enabled: open,
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<{ id: number; name?: string }[]>>(
        '/api/master/event-keywords',
      );
      return (res.data.data ?? []).map((x) => ({
        id: x.id,
        keyword: x.name || '',
      }));
    },
  });

  const uploadImageMutation = useMutation({
    mutationFn: async (file: File) => {
      const fd = new FormData();
      fd.append('file', file);
      fd.append('kind', 'image');
      const res = await axiosClient.post<ApiResponse<unknown>>(
        `/api/devices/${deviceId}/incident-files`,
        fd,
        { headers: { 'Content-Type': 'multipart/form-data' } },
      );
      if (!res.data.success) throw new Error(res.data.message || 'Upload ảnh thất bại');
    },
    onSuccess: () => message.success('Đã lưu hình ảnh sự cố'),
    onError: (err: Error) => message.error(err.message || 'Upload ảnh thất bại'),
  });

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

  const customUpload = async (options: {
    file: string | Blob | File;
    onSuccess?: (body: unknown) => void;
    onError?: (err: Error) => void;
  }) => {
    try {
      const raw = options.file;
      const file = raw instanceof File ? raw : new File([raw as Blob], 'anh-su-co.jpg');
      await uploadImageMutation.mutateAsync(file);
      options.onSuccess?.({});
    } catch (e) {
      options.onError?.(e instanceof Error ? e : new Error('Upload thất bại'));
    }
  };

  const keywordOptions = (keywordsQuery.data ?? [])
    .filter((k) => k.keyword)
    .map((k) => ({ value: k.keyword }));

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
      width={560}
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
          <AutoComplete
            options={keywordOptions}
            placeholder="Chọn hoặc nhập từ khóa"
            filterOption={(input, option) =>
              (option?.value ?? '').toLowerCase().includes(input.toLowerCase())
            }
          />
        </Form.Item>
        <Form.Item
          name="content"
          label="Nội dung"
          rules={[{ required: true, message: 'Nhập nội dung yêu cầu' }]}
        >
          <Input.TextArea rows={4} placeholder="Mô tả sự cố hoặc yêu cầu" />
        </Form.Item>
        <Form.Item label="Hình ảnh sự cố">
          <Upload
            customRequest={customUpload}
            showUploadList={false}
            accept="image/*"
            disabled={uploadImageMutation.isPending}
          >
            <Button icon={<UploadOutlined />} loading={uploadImageMutation.isPending}>
              Upload ảnh (Hinh_anh_su_co)
            </Button>
          </Upload>
        </Form.Item>
      </Form>
    </Modal>
  );
}
