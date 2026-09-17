import { useState } from 'react';
import { Button, Card, Form, Input, message, Typography } from 'antd';
import { Link } from 'react-router-dom';
import axios from 'axios';
import type { ApiResponse } from '@/types/api';

type ForgotRes = { resetToken?: string; message?: string };

export function ForgotPasswordPage() {
  const [form] = Form.useForm();
  const [devToken, setDevToken] = useState<string | null>(null);
  return (
    <div className="app-login-root">
      <div className="app-login-panel" style={{ gridColumn: '1 / -1', maxWidth: 480, margin: 'auto' }}>
        <Card>
          <Typography.Title level={4}>Quên mật khẩu</Typography.Title>
          <Form form={form} layout="vertical" onFinish={async (v) => {
            try {
              const res = await axios.post<ApiResponse<ForgotRes>>('/api/auth/forgot-password', v);
              message.success(res.data.message ?? 'Đã gửi yêu cầu');
              if (res.data.data?.resetToken) setDevToken(res.data.data.resetToken);
            } catch {
              message.error('Không tìm thấy tài khoản hoặc lỗi kết nối');
            }
          }}>
            <Form.Item name="username" label="Tài khoản" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item name="email" label="Email (nếu có)">
              <Input />
            </Form.Item>
            <Button type="primary" htmlType="submit" block>Đặt lại mật khẩu</Button>
          </Form>
          {devToken && (
            <Typography.Paragraph type="warning" style={{ marginTop: 16 }}>
              Dev: token đặt lại — <Link to={`/reset-password?token=${encodeURIComponent(devToken)}&username=${form.getFieldValue('username')}`}>Đặt mật khẩu mới</Link>
            </Typography.Paragraph>
          )}
          <Link to="/login">Quay lại đăng nhập</Link>
        </Card>
      </div>
    </div>
  );
}