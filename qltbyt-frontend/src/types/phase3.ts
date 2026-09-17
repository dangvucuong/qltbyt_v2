import type { OrganizationItem } from '@/types/api';

export interface BiddingPackageItem {
  id: number;
  packageCode?: string;
  name?: string;
  status?: string;
  winningVendor?: string;
  taxCode?: string;
  contractNumber?: string;
  value?: number;
  deviceCount: number;
}

export interface CreateBiddingPayload {
  packageCode: string;
  name?: string;
  status?: string;
  winningVendor?: string;
  taxCode?: string;
  contractNumber?: string;
  value?: number;
}

export interface DeviceHistoryItem {
  type: string;
  content?: string;
  timestamp?: string;
  actor?: string;
}

export interface DeviceSupplyItem {
  id: number;
  name?: string;
  unit?: string;
  quantity?: number;
}

export interface DeviceReportSummary {
  totalDevices: number;
  expiringInspectionCount: number;
  maintenanceDueCount: number;
  byStatusActive: number;
  byStatusInactive: number;
}

export interface DeviceReportRow {
  id: number;
  deviceCode?: string;
  identityCode?: string;
  name?: string;
  typeName?: string;
  organizationName?: string;
  statusName?: string;
  manufacturer?: string;
  modelCode?: string;
  serial?: string;
  receivedDate?: string;
  inspectionExpiryDate?: string;
  maintenanceDueDate?: string;
  classification?: string;
  totalAmount?: number;
}

export interface BaocaoAssetRow {
  id: number;
  name?: string;
  identityCode?: string;
  manufacturer?: string;
  modelCode?: string;
  serial?: string;
  quantity?: number;
  unit?: string;
  organizationName?: string;
  filterLabel?: string;
  totalAmount?: number;
  receivedDate?: string;
  depreciationAmount?: number;
}

export interface BaocaoOperationalRow {
  id?: number;
  name?: string;
  identityCode?: string;
  manufacturer?: string;
  modelCode?: string;
  serial?: string;
  quantity?: number;
  unit?: string;
  organizationName?: string;
  filterLabel?: string;
  inspectionExpiryDate?: string;
  breakdownStartedAt?: string;
  repairedAt?: string;
  downtimeNote?: string;
  totalAmount?: number;
  usageHours?: number;
  usageSessions?: number;
}


export interface KiemkeAssetRow {
  id: number;
  name?: string;
  identityCode?: string;
  manufacturer?: string;
  modelCode?: string;
  serial?: string;
  quantity?: number;
  unit?: string;
  organizationName?: string;
  locationCode?: string;
  procurementSourceLabel?: string;
  totalAmount?: number;
  receivedDate?: string;
  depreciationAmount?: number;
  note?: string;
}

export interface BaocaoAmountOption {
  amount: number;
  display: string;
}

export type OrganizationDetail = OrganizationItem;

export interface LegalDocumentItem {
  id: number;
  inspectionCertificate?: string;
  importPermit?: string;
  circulationPermit?: string;
  filePath?: string;
  biddingInfo?: string;
  priceWithVat?: number;
  deviceCodes?: string;
}

export interface CreateLegalDocumentPayload {
  inspectionCertificate?: string;
  importPermit?: string;
  circulationPermit?: string;
  biddingInfo?: string;
  priceWithVat?: number;
  deviceCodes?: string;
}
