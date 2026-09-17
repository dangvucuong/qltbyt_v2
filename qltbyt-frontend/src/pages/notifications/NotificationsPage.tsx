import { Button, Card, List, Space, Typography, message } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { NotificationItem } from '@/types/events';
import { PageHeader } from '@/components/PageHeader';
import { LoadingCenter } from '@/components/LoadingCenter';

export function NotificationsPage() {
  const queryClient = useQueryClient();
  const q = useQuery({
    queryKey: ['notifications'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<NotificationItem[]>>('/api/notifications');
      return res.data.data ?? [];
    },
  });

  const markRead = useMutation({
    mutationFn: async (id: number) => {
      await axiosClient.put(`/api/notifications/${id}/read`);
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
  });

  const markAllRead = useMutation({
    mutationFn: async () => {
      await axiosClient.put('/api/notifications/read-all');
    },
    onSuccess: () => {
      message.success('Đã đánh dấu tất cả là đã đọc');
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
    onError: () => message.error('Thao tác thất bại'),
  });

  const unreadCount = (q.data ?? []).filter((n) => !n.isRead).length;

  if (q.isLoading) return <LoadingCenter />;

  return (
    <div>
      <PageHeader
        title="Thông báo"
        subtitle="Danh sách cảnh báo và tin nội bộ"
        extra={
          unreadCount > 0 ? (
            <Button
              type="primary"
              loading={markAllRead.isPending}
              onClick={() => markAllRead.mutate()}
            >
              Đánh dấu đã đọc tất cả ({unreadCount})
            </Button>
          ) : null
        }
      />
      <Card className="app-card">
        <List
          dataSource={q.data ?? []}
          locale={{ emptyText: 'Không có thông báo' }}
          renderItem={(item) => (
            <List.Item
              actions={
                !item.isRead
                  ? [
                      <Button type="link" size="small" onClick={() => markRead.mutate(item.id)}>
                        Đánh dấu đã đọc
                      </Button>,
                    ]
                  : undefined
              }
            >
              <List.Item.Meta
                title={
                  <Space>
                    <Typography.Text strong>{item.title}</Typography.Text>
                    {!item.isRead && <Typography.Text type="warning">Mới</Typography.Text>}
                  </Space>
                }
                description={
                  <>
                    <div>{item.content}</div>
                    <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                      {item.createdAt ? dayjs(item.createdAt).format('DD/MM/YYYY HH:mm') : ''}
                    </Typography.Text>
                  </>
                }
              />
            </List.Item>
          )}
        />
      </Card>
    </div>
  );
}
