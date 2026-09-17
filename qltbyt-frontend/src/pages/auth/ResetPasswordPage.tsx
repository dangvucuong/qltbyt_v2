import { Button, Card, Form, Input, message, Typography } from 'antd';
import { Link, useSearchParams } from 'react-router-dom';
import axios from 'axios';
import type { ApiResponse } from '@/types/api';

export function ResetPasswordPage() {
  const [params] = useSearchParams();
  const token = params.get('token') ?? '';
  const username = params.get('username') ?? '';
  const [form] = Form.useForm();

  return (
    <div className="app-login-root">
      <div
        className="app-login-panel"
        style={{ gridColumn: '1 / -1', maxWidth: 480, margin: 'auto' }}
      >
        <Card>
          <Typography.Title level={4}>Đặt lại mật khẩu</Typography.Title>
          <Form
            form={form}
            layout="vertical"
            initialValues={{ username, resetToken: token }}
            onFinish={async (values) => {
              try {
                await axios.post<ApiResponse<object>>('/api/auth/reset-password', values);
                message.success('Đã cập nhật mật khẩu');
                form.resetFields(['newPassword']);
              } catch {
                message.error('Token không hợp lệ hoặc đã hết hạn');
              }
            }}
          >
            <Form.Item name="username" label="Tài khoản" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item name="resetToken" label="Token" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item name="newPassword" label="Mật khẩu mới" rules={[{ required: true, min: 6 }]}>
              <Input.Password />
            </Form.Item>
            <Button type="primary" htmlType="submit" block>Lưu mật khẩu</Button>
          </Form>
          <Link to="/login" style={{ marginTop: 16 }}>Quay lại đăng nhập</Link>
        </Card>
      </div>
    </div>
  );
}
