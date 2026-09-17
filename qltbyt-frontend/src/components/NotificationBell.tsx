import { Badge, Button, Dropdown, List, Typography } from 'antd';
import { BellOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import axiosClient from '@/api/axiosClient';
import type { ApiResponse } from '@/types/api';
import type { NotificationItem } from '@/types/events';

export function NotificationBell() {
  const queryClient = useQueryClient();
  const q = useQuery({
    queryKey: ['notifications'],
    queryFn: async () => {
      const res = await axiosClient.get<ApiResponse<NotificationItem[]>>('/api/notifications');
      return res.data.data ?? [];
    },
    refetchInterval: 60_000,
  });

  const markRead = useMutation({
    mutationFn: async (id: number) => {
      await axiosClient.put(`/api/notifications/${id}/read`);
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
  });

  const unread = (q.data ?? []).filter((n) => !n.isRead).length;

  const menu = {
    items: [
      {
        key: 'list',
        label: (
          <div style={{ width: 320, maxHeight: 360, overflow: 'auto' }}>
            <div style={{ padding: '8px 12px', borderBottom: '1px solid #f0f0f0' }}>
              <Link to="/notifications">Xem tất cả thông báo</Link>
            </div>
            <List
              size="small"
              dataSource={q.data ?? []}
              locale={{ emptyText: 'Không có thông báo' }}
              renderItem={(item) => (
                <List.Item
                  onClick={() => !item.isRead && markRead.mutate(item.id)}
                  style={{ cursor: 'pointer', opacity: item.isRead ? 0.65 : 1 }}
                >
                  <List.Item.Meta
                    title={<Typography.Text strong>{item.title}</Typography.Text>}
                    description={
                      <>
                        <div>{item.content}</div>
                        <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                          {item.createdAt ? dayjs(item.createdAt).format('DD/MM/YYYY HH:mm') : ''}
                        </Typography.Text>
                      </>
                    }
                  />
                </List.Item>
              )}
            />
          </div>
        ),
      },
    ],
  };

  return (
    <Dropdown menu={menu} trigger={['click']} placement="bottomRight">
      <Badge count={unread} size="small">
        <Button type="text" icon={<BellOutlined />} aria-label="Thông báo" />
      </Badge>
    </Dropdown>
  );
}
