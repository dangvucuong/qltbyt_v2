export interface UserAccountItem {
  id: number;
  username?: string;
  fullName?: string;
  phone?: string;
  email?: string;
  accountType?: number;
  role?: string;
  organizationIds?: string;
}

export interface CreateUserPayload {
  username: string;
  password: string;
  fullName?: string;
  phone?: string;
  email?: string;
  accountType: number;
  organizationIds?: string;
}

export interface UpdateUserPayload {
  fullName?: string;
  phone?: string;
  email?: string;
  accountType?: number;
  organizationIds?: string;
  newPassword?: string;
}

export interface PersonnelItem {
  id: number;
  fullName?: string;
  identityNumber?: string;
  phone?: string;
  email?: string;
  organizationId?: number;
  organizationName?: string;
  personnelTypeId?: number;
  userAccountId?: number;
}

export interface UsageLogItem {
  id: number;
  deviceId?: number;
  deviceCode: string;
  deviceName?: string;
  hours?: number;
  sessions?: number;
  inputDate: string;
  userId?: number;
}

export interface CreateUsageLogPayload {
  deviceCode: string;
  hours?: number;
  sessions?: number;
  inputDate: string;
}
