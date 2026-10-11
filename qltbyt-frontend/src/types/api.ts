export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserInfo {
  id: number;
  username: string;
  fullName?: string;
  role: string;
  accountType?: number;
  organizationIds?: string;
}

export interface LoginResponse {
  token: string;
  user: UserInfo;
}

export interface DeviceListItem {
  id: number;
  deviceCode?: string;
  name?: string;
  typeId?: number;
  typeName?: string;
  organizationId?: number;
  organizationName?: string;
  statusId?: number;
  statusName?: string;
  inspectionExpiryDate?: string;
}

export interface DeviceDetail extends DeviceListItem {
  technicalCode?: string;
  identityCode?: string;
  modelCode?: string;
  serial?: string;
  manufacturer?: string;
  countryOfOrigin?: string;
  yearOfManufacture?: string;
  technicalSpecs?: string;
  conditionNote?: string;
  deviceGroup?: string;
  classification?: string;
  handoverOrgId?: number;
  maintenanceOrgId?: number;
  biddingPackageCode?: string;
  quantity?: number;
  unit?: string;
  unitPrice?: string;
  totalAmount?: string;
  locationCode?: string;
  procurementSource?: string;
  warrantyDueMarker?: string;
  maintenanceDueMarker?: string;
  calibrationDueMarker?: string;
  inspectionDueMarker?: string;
  putIntoUseMarker?: string;
  specializedName?: string;
  /** Tên file hợp đồng bảo trì (IDHoso / HDBaotri/{id}/) */
  legalDocumentRef?: string;
  parentDeviceCode?: string;
  receivedDate?: string;
  note?: string;
  updatedAt?: string;
  files: DeviceFile[];
  codes: DeviceCode[];
}

export interface DeviceFile {
  id: number;
  path?: string;
  description?: string;
  fileDate?: string;
}

export interface DeviceCode {
  id: number;
  qrCode?: string;
  nfc?: string;
}

export interface MasterItem {
  id: number;
  name?: string;
}

export interface BiddingPackageLookup {
  packageCode?: string;
  name?: string;
  winningVendor?: string;
}

export interface OrganizationItem {
  id: number;
  name?: string;
  typeName?: string;
  typeId?: number;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
}

export interface ChartItem {
  label: string;
  value: number;
}

export interface OrganizationStats {
  organizationId?: number;
  label: string;
  deviceCount: number;
  totalAssetValue: number;
}

export interface DashboardSummary {
  totalDevices: number;
  expiringInspectionCount: number;
  byStatus: ChartItem[];
  byType: ChartItem[];
  byOrganization: OrganizationStats[];
}

export type { EventListItem, EventDetail, NotificationItem } from './events';
