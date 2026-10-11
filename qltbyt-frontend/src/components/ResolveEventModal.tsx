import { Button, Form, Input, InputNumber, List, Modal, Upload, message } from 'antd';
import { UploadOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { EventListItem } from '@/types/events';
import { apiUrl } from '@/utils/apiBase';

type FileItem = {
  id: number;
  path?: string;
  description?: string;
  fileDate?: string;
};

type Props = {
  open: boolean;
  event: EventListItem | null;
  loading?: boolean;
  onClose: () => void;
  onSubmit: (payload: { resolutionContent: string; cost?: number }) => void;
};

function fileUrl(path?: string) {
  if (!path) return '#';
  const normalized = path.replace(/\\/g, '/');
  const files = normalized.replace(/^FILELUU[/\\]?/i, '');
  return apiUrl(`/files/${files}`);
}

export function ResolveEventModal({ open, event, loading, onClose, onSubmit }: Props) {
  const [form] = Form.useForm();
  const queryClient = useQueryClient();

  const filesQuery = useQuery({
    queryKey: ['event-files', event?.id],
    enabled: open && !!event?.id,
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<FileItem[]>>(`/api/events/${event!.id}/files`);
      return res.data.data ?? [];
    },
  });

  const uploadMutation = useMutation({
    mutationFn: async (file: File) => {
      const fd = new FormData();
      fd.append('file', file);
      fd.append('kind', 'report');
      const res = await axiosClient.post<ApiResponse<FileItem>>(
        `/api/events/${event!.id}/files`,
        fd,
        { headers: { 'Content-Type': 'multipart/form-data' } },
      );
      if (!res.data.success) throw new Error(res.data.message || 'Upload thất bại');
      return res.data.data!;
    },
    onSuccess: () => {
      message.success('Đã lưu file báo cáo sự cố');
      queryClient.invalidateQueries({ queryKey: ['event-files', event?.id] });
    },
    onError: (err: Error) => message.error(err.message || 'Upload thất bại'),
  });

  const customUpload = async (options: {
    file: string | Blob | File;
    onSuccess?: (body: unknown) => void;
    onError?: (err: Error) => void;
  }) => {
    try {
      const raw = options.file;
      const file = raw instanceof File ? raw : new File([raw as Blob], 'bao-cao.bin');
      await uploadMutation.mutateAsync(file);
      options.onSuccess?.({});
    } catch (e) {
      options.onError?.(e instanceof Error ? e : new Error('Upload thất bại'));
    }
  };

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
      width={640}
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

      <div style={{ marginBottom: 8, fontWeight: 500 }}>File báo cáo sự cố</div>
      <Upload
        customRequest={customUpload}
        showUploadList={false}
        multiple={false}
        disabled={!event?.id || uploadMutation.isPending}
      >
        <Button icon={<UploadOutlined />} loading={uploadMutation.isPending}>
          Chọn file báo cáo (BAO_CAO_SU_CO)
        </Button>
      </Upload>
      <List
        size="small"
        style={{ marginTop: 12 }}
        loading={filesQuery.isLoading}
        dataSource={filesQuery.data ?? []}
        locale={{ emptyText: 'Chưa có file sự cố' }}
        renderItem={(item) => (
          <List.Item
            actions={[
              <a key="open" href={fileUrl(item.path)} target="_blank" rel="noreferrer">
                Mở
              </a>,
            ]}
          >
            <List.Item.Meta
              title={item.description || item.path?.split('/').pop()}
              description={item.path}
            />
          </List.Item>
        )}
      />
    </Modal>
  );
}
