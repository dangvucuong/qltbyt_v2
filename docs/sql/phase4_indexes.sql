-- Phase 4: goi y index (chay tren db_QLTBYT_NEW khi can)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Device_InspectionExpiry' AND object_id = OBJECT_ID('Device'))
    CREATE INDEX IX_Device_InspectionExpiry ON Device(InspectionExpiryDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Nhapthuocvattu_Hanghoa' AND object_id = OBJECT_ID('Nhapthuocvattu'))
    CREATE INDEX IX_Nhapthuocvattu_Hanghoa ON Nhapthuocvattu(IDhanghoa);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DMthuocvattu_Mahanghoa' AND object_id = OBJECT_ID('DMthuocvattu'))
    CREATE INDEX IX_DMthuocvattu_Mahanghoa ON DMthuocvattu(Mahanghoa);