-- Cap nhat ten trang thai thiet bi (tieng Viet co dau)
USE db_QLTBYT_NEW;
GO

UPDATE dbo.DMTrangthai SET TenTrangthai = N'Đang hoạt động' WHERE MaTrangthai = 1;
UPDATE dbo.DMTrangthai SET TenTrangthai = N'Không hoạt động / Hỏng' WHERE MaTrangthai = 2;
UPDATE dbo.DMTrangthai SET TenTrangthai = N'Chưa sử dụng' WHERE MaTrangthai = 3;

UPDATE dbo.DMTrangthai SET TenTrangthai = N'Đang hoạt động' WHERE TenTrangthai = N'Dang hoat dong';
UPDATE dbo.DMTrangthai SET TenTrangthai = N'Không hoạt động / Hỏng' WHERE TenTrangthai = N'Khong hoat dong / Hong';
UPDATE dbo.DMTrangthai SET TenTrangthai = N'Chưa sử dụng' WHERE TenTrangthai = N'Chua su dung';
