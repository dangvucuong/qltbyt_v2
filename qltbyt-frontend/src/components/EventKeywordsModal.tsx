import { Button, Form, Input, List, Modal, Space, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';

type KeywordItem = { id: number; keyword: string };

type Props = {
  open: boolean;
  onClose: () => void;
};

export function EventKeywordsModal({ open, onClose }: Props) {
  const [form] = Form.useForm<{ keyword: string }>();
  const queryClient = useQueryClient();

  const keywordsQuery = useQuery({
    queryKey: ['event-keywords'],
    enabled: open,
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<KeywordItem[]>>('/api/events/keywords');
      return res.data.data ?? [];
    },
  });

  const addMutation = useMutation({
    mutationFn: async (keyword: string) => {
      const res = await axiosClient.post<ApiResponse<KeywordItem>>('/api/events/keywords', {
        keyword,
      });
      if (!res.data.success) throw new Error(res.data.message || 'Thêm thất bại');
      return res.data.data!;
    },
    onSuccess: () => {
      message.success('Thêm từ khóa thành công');
      form.resetFields();
      queryClient.invalidateQueries({ queryKey: ['event-keywords'] });
      queryClient.invalidateQueries({ queryKey: ['master-event-keywords'] });
    },
    onError: (err: Error) => message.error(err.message || 'Thêm từ khóa thất bại'),
  });

  return (
    <Modal
      title="Quản lý từ khóa sự cố"
      open={open}
      onCancel={onClose}
      footer={<Button onClick={onClose}>Đóng</Button>}
      destroyOnClose
      width={520}
    >
      <Form
        form={form}
        layout="inline"
        style={{ marginBottom: 16, rowGap: 8 }}
        onFinish={(v) => addMutation.mutate(v.keyword.trim())}
      >
        <Form.Item
          name="keyword"
          rules={[{ required: true, message: 'Nhập từ khóa' }]}
          style={{ flex: 1, marginInlineEnd: 8 }}
        >
          <Input placeholder="Từ khóa mới (vd: Báo hỏng, Kiểm tra...)" maxLength={200} />
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit" loading={addMutation.isPending}>
            Thêm
          </Button>
        </Form.Item>
      </Form>
      <List
        size="small"
        bordered
        loading={keywordsQuery.isLoading}
        dataSource={keywordsQuery.data ?? []}
        locale={{ emptyText: 'Chưa có từ khóa' }}
        renderItem={(item) => (
          <List.Item>
            <Space>{item.keyword}</Space>
          </List.Item>
        )}
        style={{ maxHeight: 360, overflow: 'auto' }}
      />
      <p style={{ marginTop: 12, marginBottom: 0, color: '#64748b', fontSize: 12 }}>
        Xóa/sửa đầy đủ: Admin → Danh mục → Từ khóa sự cố.
      </p>
    </Modal>
  );
}
