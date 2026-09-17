import { Button, message } from 'antd';
import { CameraOutlined } from '@ant-design/icons';
import { useRef, useState } from 'react';
import axiosClient from '@/api/axiosClient';

export function DeviceCameraCapture({ deviceId, onDone }: { deviceId: number; onDone?: () => void }) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [active, setActive] = useState(false);
  const [loading, setLoading] = useState(false);

  const start = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } });
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
        await videoRef.current.play();
      }
      setActive(true);
    } catch {
      message.error('Không mở được camera. Hãy dùng tải ảnh nếu cần.');
    }
  };

  const stop = () => {
    const v = videoRef.current;
    const stream = v?.srcObject as MediaStream | null;
    stream?.getTracks().forEach((t) => t.stop());
    if (v) v.srcObject = null;
    setActive(false);
  };

  const capture = async () => {
    const v = videoRef.current;
    if (!v) return;
    setLoading(true);
    try {
      const canvas = document.createElement('canvas');
      canvas.width = v.videoWidth || 640;
      canvas.height = v.videoHeight || 480;
      canvas.getContext('2d')?.drawImage(v, 0, 0);
      const blob = await new Promise<Blob | null>((resolve) => canvas.toBlob((b) => resolve(b), 'image/jpeg', 0.9));
      if (!blob) throw new Error('capture');
      const form = new FormData();
      form.append('file', blob, `capture-${Date.now()}.jpg`);
      await axiosClient.post(`/api/devices/${deviceId}/captures`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      message.success('Lưu ảnh chụp thành công');
      stop();
      onDone?.();
    } catch {
      message.error('Lưu ảnh thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <video ref={videoRef} style={{ width: '100%', maxWidth: 480, background: '#0f172a', borderRadius: 8 }} />
      <div style={{ marginTop: 12, display: 'flex', gap: 8, flexWrap: 'wrap' }}>
        {!active ? (
          <Button icon={<CameraOutlined />} onClick={start}>Bật camera</Button>
        ) : (
          <>
            <Button type="primary" loading={loading} onClick={capture}>Chụp và lưu</Button>
            <Button onClick={stop}>Tắt camera</Button>
          </>
        )}
      </div>
    </div>
  );
}