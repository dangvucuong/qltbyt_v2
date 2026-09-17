-- Required for BCrypt password hashes (legacy Taikhoan.Matkhau was nvarchar(50))
IF COL_LENGTH('Taikhoan', 'Matkhau') < 200
    ALTER TABLE Taikhoan ALTER COLUMN Matkhau NVARCHAR(200) NULL;