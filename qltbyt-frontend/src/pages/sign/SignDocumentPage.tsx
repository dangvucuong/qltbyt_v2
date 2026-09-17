import { Button, Card, InputNumber, List, message } from 'antd';
import { FilePdfOutlined } from '@ant-design/icons';
import { useState } from 'react';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceFile } from '@/types/api';

import { PageHeader } from '@/components/PageHeader';

export function SignDocumentPage() {
  const [fileId, setFileId] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<DeviceFile | null>(null);

  const sign = async () => {
    if (!fileId || fileId <= 0) {
      message.warning('Nhập ID file PDF hợp lệ');
      return;
    }
    setLoading(true);
    try {
      const res = await axiosClient.post<ApiResponse<DeviceFile>>(`/api/devices/files/${fileId}/sign-pdf`);
      setResult(res.data.data ?? null);
      message.success(res.data.message ?? 'Ký số thành công');
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      message.error(err.response?.data?.message ?? 'Ký số thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title="Ký số PDF"
        subtitle="Ký số file PDF đã tải lên trong tab Tài liệu thiết bị (chỉ file .pdf)"
      />
      <Card className="app-card">
        <div style={{ display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
          <span>ID file (bảng tài liệu thiết bị):</span>
          <InputNumber min={1} value={fileId} onChange={(v) => setFileId(v ?? null)} />
          <Button type="primary" loading={loading} icon={<FilePdfOutlined />} onClick={sign}>
            Ký số PDF
          </Button>
        </div>
        {result && (
          <List
            style={{ marginTop: 16 }}
            dataSource={[result]}
            renderItem={(f) => (
              <List.Item>
                <FilePdfOutlined style={{ marginRight: 8 }} />
                {f.description || f.path}
              </List.Item>
            )}
          />
        )}
      </Card>
    </div>
  );
}