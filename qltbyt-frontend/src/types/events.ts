export interface EventListItem {
  id: number;
  deviceId?: number;
  deviceCode?: string;
  deviceName?: string;
  typeId?: number;
  typeName?: string;
  content?: string;
  eventDateTime?: string;
  statusId?: number;
  statusName?: string;
  keyword?: string;
  cost?: number;
  accountUsername?: string;
}

export interface EventDetail extends EventListItem {
  managerUserId?: number;
  managerName?: string;
}

export interface CreateDeviceEventPayload {
  eventTypeId: number;
  content: string;
  keyword?: string;
}

export interface ResolveEventPayload {
  resolutionContent: string;
  cost?: number;
}

export interface NotificationItem {
  id: number;
  title?: string;
  content?: string;
  createdAt?: string;
  isRead: boolean;
}
