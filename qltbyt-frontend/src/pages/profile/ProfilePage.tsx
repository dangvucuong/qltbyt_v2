import { Button, Card, Form, Input, message } from 'antd';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';

export function ProfilePage() {
  const [form] = Form.useForm();

  const onFinish = async (values: { currentPassword: string; newPassword: string }) => {
    try {
      await axiosClient.post<ApiResponse<object>>('/api/auth/change-password', values);
      message.success('Đổi mật khẩu thành công');
      form.resetFields();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      message.error(err.response?.data?.message ?? 'Đổi mật khẩu thất bại');
    }
  };

  return (
    <div>
      <PageHeader title="Tài khoản" subtitle="Đổi mật khẩu đăng nhập" />
      <Card className="app-card" style={{ maxWidth: 480 }}>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item name="currentPassword" label="Mật khẩu hiện tại" rules={[{ required: true }]}>
            <Input.Password />
          </Form.Item>
          <Form.Item name="newPassword" label="Mật khẩu mới" rules={[{ required: true, min: 6 }]}>
            <Input.Password />
          </Form.Item>
          <Button type="primary" htmlType="submit">Lưu mật khẩu</Button>
        </Form>
      </Card>
    </div>
  );
}