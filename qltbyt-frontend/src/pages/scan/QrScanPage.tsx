import { Alert, Button, Card, Input, Space, Typography, message } from 'antd';
import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse, DeviceListItem, PagedResult } from '@/types/api';
import { PageHeader } from '@/components/PageHeader';

export function QrScanPage() {
  const navigate = useNavigate();
  const videoRef = useRef<HTMLVideoElement>(null);
  const [manual, setManual] = useState('');
  const [cameraOk, setCameraOk] = useState(false);
  const [cameraError, setCameraError] = useState<string | null>(null);
  const scanningRef = useRef(false);

  const goToDevice = useCallback(
    async (code: string) => {
      const trimmed = code.trim();
      if (!trimmed) return;
      try {
        const q = new URLSearchParams({ page: '1', pageSize: '5', search: trimmed });
        const res = await axiosClient.get<ApiResponse<PagedResult<DeviceListItem>>>(
          `/api/devices?${q}`,
        );
        const items = res.data.data?.items ?? [];
        if (items.length === 1) {
          navigate(`/devices/${items[0].id}`);
          return;
        }
        if (items.length > 1) {
          navigate(`/devices?search=${encodeURIComponent(trimmed)}`);
          return;
        }
        message.warning('Không tìm thấy thiết bị với mã quét');
      } catch {
        message.error('Tra cứu thiết bị thất bại');
      }
    },
    [navigate],
  );

  useEffect(() => {
    let stream: MediaStream | null = null;
    let timer: ReturnType<typeof setInterval> | null = null;
    const detector =
      typeof window !== 'undefined' && 'BarcodeDetector' in window
        ? new (window as unknown as { BarcodeDetector: new (o: { formats: string[] }) => {
            detect: (source: HTMLVideoElement) => Promise<Array<{ rawValue?: string }>>;
          } }).BarcodeDetector({ formats: ['qr_code'] })
        : null;

    const start = async () => {
      if (!detector || !navigator.mediaDevices?.getUserMedia) {
        setCameraError('Trình duyệt không hỗ trợ quét QR tự động — nhập mã thủ công.');
        return;
      }
      try {
        stream = await navigator.mediaDevices.getUserMedia({
          video: { facingMode: 'environment' },
        });
        const video = videoRef.current;
        if (!video) return;
        video.srcObject = stream;
        await video.play();
        setCameraOk(true);
        timer = setInterval(async () => {
          if (scanningRef.current || video.readyState < 2) return;
          try {
            const codes = await detector.detect(video);
            const raw = codes[0]?.rawValue;
            if (raw) {
              scanningRef.current = true;
              await goToDevice(raw);
            }
          } catch {
            /* ignore frame errors */
          }
        }, 800);
      } catch {
        setCameraError('Không mở được camera. Kiểm tra quyền truy cập.');
      }
    };

    start();
    return () => {
      if (timer) clearInterval(timer);
      if (stream) stream.getTracks().forEach((t) => t.stop());
    };
  }, [goToDevice]);

  return (
    <div>
      <PageHeader title="Quét QR thiết bị" subtitle="Mở thiết bị từ mã QR hoặc mã in trên tem" />
      <Card className="app-card" style={{ maxWidth: 560 }}>
        {cameraError && <Alert type="info" showIcon message={cameraError} style={{ marginBottom: 16 }} />}
        {cameraOk && (
          <video
            ref={videoRef}
            style={{ width: '100%', borderRadius: 8, background: '#000', marginBottom: 16 }}
            muted
            playsInline
          />
        )}
        <Typography.Paragraph type="secondary">
          Nhập mã QR / mã thiết bị nếu không dùng camera:
        </Typography.Paragraph>
        <Space.Compact style={{ width: '100%' }}>
          <Input
            value={manual}
            onChange={(e) => setManual(e.target.value)}
            placeholder="VD: QR-TB-DEMO-001"
            onPressEnter={() => goToDevice(manual)}
          />
          <Button type="primary" onClick={() => goToDevice(manual)}>Tra cứu</Button>
        </Space.Compact>
      </Card>
    </div>
  );
}
