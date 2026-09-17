import { Spin } from 'antd';

export function LoadingCenter() {
  return (
    <div className="app-loading-wrap">
      <Spin size="large" tip="Đang tải..." />
    </div>
  );
}
