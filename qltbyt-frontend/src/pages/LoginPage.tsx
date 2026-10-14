import { Button, Card, Form, Input, message, Typography } from 'antd';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, LoginResponse } from '@/types/api';
import { useAuthStore } from '@/store/authStore';
import { getApiOrigin } from '@/utils/apiBase';

export function LoginPage() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const token = useAuthStore((s) => s.token);

  if (token) {
    navigate('/dashboard', { replace: true });
  }

  const onFinish = async (values: { username: string; password: string }) => {
    try {
      const res = await axiosClient.post<ApiResponse<LoginResponse>>('/api/auth/login', {
        username: values.username.trim(),
        password: values.password,
      });
      const body = res.data;
      if (!body.success || !body.data) {
        message.error(body.message || 'Đăng nhập thất bại');
        return;
      }
      setAuth(body.data.token, body.data.user);
      message.success('Đăng nhập thành công');
      navigate('/dashboard');
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        if (!err.response) {
          const api = getApiOrigin() || 'http://localhost:5149';
          message.error(`Không kết nối API (${api}). Kiểm tra backend đang chạy và CORS.`);
          return;
        }
        const apiBody = err.response.data as ApiResponse<LoginResponse> | undefined;
        const apiMsg = apiBody?.message || apiBody?.errors?.join('; ');
        if (err.response.status === 401) {
          message.error(apiMsg || 'Sai tên đăng nhập hoặc mật khẩu.');
          return;
        }
        if (err.response.status === 400) {
          message.error(apiMsg || 'Dữ liệu đăng nhập không hợp lệ.');
          return;
        }
        if (err.response.status === 404) {
          message.error(
            `Không tìm thấy API đăng nhập (${err.config?.baseURL || ''}/api/auth/login). Kiểm tra VITE_API_BASE_URL và publish lại frontend.`,
          );
          return;
        }
        message.error(apiMsg || `Lỗi máy chủ (${err.response.status}).`);
        return;
      }
      message.error('Đăng nhập thất bại.');
    }
  };

  return (
    <div className="app-login-root">
      <div className="app-login-hero">
        <div className="app-login-hero-brand">
          <img
            className="app-login-hero-logo"
            src="/brand/dvc-techflow-logo.png"
            alt="DVC Techflow"
          />
          <div>
            <div className="app-login-hero-brand-name">DVC TECHFLOW</div>
            <div className="app-login-hero-brand-tag">Làm chủ công nghệ – Làm chủ tương lai</div>
          </div>
        </div>
        <h1>Hệ thống quản lý thiết bị y tế</h1>
        <p>
          Theo dõi tài sản, kiểm định và vận hành thiết bị trên một nền tảng thống nhất,
          phù hợp quy trình bệnh viện.
        </p>
        <div className="app-login-hero-tags">
          <span className="app-login-hero-tag">An toàn</span>
          <span className="app-login-hero-tag">Chính xác</span>
          <span className="app-login-hero-tag">Hiệu quả</span>
        </div>
      </div>
      <div className="app-login-panel">
        <Card className="app-login-card" bordered={false}>
          <Typography.Title level={4} style={{ marginBottom: 4, fontWeight: 700 }}>
            Đăng nhập
          </Typography.Title>
          <Typography.Paragraph type="secondary" style={{ marginBottom: 24 }}>
            Sử dụng tài khoản được cấp để truy cập hệ thống.
          </Typography.Paragraph>
          <Form layout="vertical" onFinish={onFinish} size="large">
            <Form.Item
              name="username"
              label="Tài khoản"
              rules={[{ required: true, message: 'Nhập tài khoản' }]}
            >
              <Input placeholder="admin" autoComplete="username" />
            </Form.Item>
            <Form.Item
              name="password"
              label="Mật khẩu"
              rules={[{ required: true, message: 'Nhập mật khẩu' }]}
            >
              <Input.Password placeholder="Mật khẩu" autoComplete="current-password" />
            </Form.Item>
            <Button type="primary" htmlType="submit" block style={{ height: 44 }}>
              Đăng nhập
            </Button>
            <div style={{ textAlign: 'center', marginTop: 12 }}>
              <Link to="/forgot-password">Quên mật khẩu?</Link>
            </div>
          </Form>
          <Typography.Paragraph type="secondary" style={{ marginTop: 20, fontSize: 12 }}>
            Tài khoản demo: admin / kythuat / user — mật khẩu Sample@123
          </Typography.Paragraph>
        </Card>
        <footer className="app-login-footer">
          <div className="app-login-footer-copy">
            © {new Date().getFullYear()} DVC TECHFLOW. Bảo lưu mọi quyền.
          </div>
          <div className="app-login-footer-meta">
            Công ty cổ phần giải pháp công nghệ và chuyển đổi số DVC
          </div>
          <div className="app-login-footer-links">
            <a href="https://dvctechflow.com" target="_blank" rel="noreferrer">
              dvctechflow.com
            </a>
            <span>·</span>
            <a href="tel:0962839367">Hotline 0962 8393 67</a>
            <span>·</span>
            <a href="mailto:info@dvctechflow.com">info@dvctechflow.com</a>
          </div>
        </footer>
      </div>
    </div>
  );
}
