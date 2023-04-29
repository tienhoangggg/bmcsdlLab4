/*----------------------------------------------------------
MASV: 20127072 - 20127091 - 20127102 - 20127424
HO TEN CAC THANH VIEN NHOM: Lê Võ Huỳnh Thanh - Lê Trọng Anh Tú - Hoàng Hữu Minh An - Trần Tiến Hoàng
LAB: 03 - NHOM 09
----------------------------------------------------------*/

-- a) Viết script tạo Database có tên QLSVNhom.
USE master
GO

IF DB_ID('QLSVNhom') IS NOT NULL
	DROP DATABASE QLSVNhom
GO

CREATE DATABASE QLSVNhom
GO

USE QLSVNhom
GO
 
-- b) Viết script tạo mới các Table SINHVIEN, NHANVIEN, LOP, HOCPHAN, BANGDIEM như mô tả trên.
CREATE TABLE SINHVIEN
(
	MASV VARCHAR(20),
	HOTEN NVARCHAR(100) NOT NULL,
	NGAYSINH DATETIME,
	DIACHI NVARCHAR(200),
	MALOP VARCHAR(20),
	TENDN NVARCHAR(100) NOT NULL,
	MATKHAU VARBINARY(MAX) NOT NULL
 
	CONSTRAINT PK_SV
	PRIMARY KEY(MASV)
)
 
CREATE TABLE NHANVIEN
(
	MANV VARCHAR(20),
	HOTEN NVARCHAR(100) NOT NULL,
	EMAIL VARCHAR(20),
	LUONG VARBINARY(MAX),
	TENDN NVARCHAR(100) NOT NULL,
	MATKHAU VARBINARY(MAX) NOT NULL,
	CONSTRAINT PK_NV
	PRIMARY KEY(MANV)
)
 
CREATE TABLE LOP
(
	MALOP VARCHAR(20),
	TENLOP NVARCHAR(100) NOT NULL,
	MANV VARCHAR(20)
 
	CONSTRAINT PK_LOP
	PRIMARY KEY(MALOP)
)
 
CREATE TABLE HOCPHAN
(
	MAHP VARCHAR(20),
	TENHP NVARCHAR(100) NOT NULL,
	SOTC INT
 
	CONSTRAINT PK_HP
	PRIMARY KEY(MAHP)
)
 
CREATE TABLE BANGDIEM
(
	MASV VARCHAR(20),
	MAHP VARCHAR(20),
	DIEMTHI VARBINARY(MAX)
 
	CONSTRAINT PK_BD
	PRIMARY KEY(MASV, MAHP)
)

USE QLSVnhom
GO

-- d) Viết các stored procedure và chương trình (sử dụng C#) để thực hiện các yêu cầu sau. Đầu file script ghi chú chi tiết như sau:

----- i>
----- ii> Tạo data để kiểm thử
----------- Tạo khóa phụ
ALTER TABLE LOP
ADD
	CONSTRAINT FK_LOP_NHANVIEN
	FOREIGN KEY(MANV)
	REFERENCES NHANVIEN
GO

ALTER TABLE SINHVIEN
ADD
	CONSTRAINT FK_SINHVIEN_LOP
	FOREIGN KEY(MALOP)
	REFERENCES LOP
GO

ALTER TABLE BANGDIEM
ADD
	CONSTRAINT FK_BANGDIEM_SINHVIEN
	FOREIGN KEY(MASV)
	REFERENCES SINHVIEN,

	CONSTRAINT FK_BANGDIEM_HOCPHAN
	FOREIGN KEY(MAHP)
	REFERENCES HOCPHAN
GO


-- Viết các Stored procedure
---- i) Stored dùng để thêm mới dữ liệu (Insert) vào table SINHVIEN, trong đó

IF OBJECT_ID('SP_INS_PUBLIC_ENCRYPT_NHANVIEN') IS NOT NULL
	DROP PROCEDURE SP_INS_PUBLIC_ENCRYPT_NHANVIEN
GO

CREATE PROCEDURE SP_INS_PUBLIC_ENCRYPT_NHANVIEN @MaNV VARCHAR(20), @HoTen NVARCHAR(100), @email VARCHAR(20),
					@LuongCB VARBINARY(MAX), @TenDN NVARCHAR(100), @Matkhau VARBINARY(MAX)
AS
	INSERT INTO NHANVIEN(MANV,HOTEN,EMAIL,LUONG,TENDN,MATKHAU)
	VALUES(@MaNV, @HoTen, @email, @LuongCB, @TenDN, @Matkhau)
GO

---- ii) Stored dùng để truy vấn dữ liệu nhân viên (NHANVIEN)
IF OBJECT_ID('SP_SEL_PUBLIC_ENCRYPT_NHANVIEN') IS NOT NULL
	DROP PROCEDURE SP_SEL_PUBLIC_ENCRYPT_NHANVIEN
GO

CREATE PROCEDURE SP_SEL_PUBLIC_ENCRYPT_NHANVIEN
AS
	SELECT MANV, HOTEN, EMAIL, LUONG
	FROM NHANVIEN
GO


USE [master]
GO
CREATE LOGIN [lab4] WITH PASSWORD=N'123456789', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=ON, CHECK_POLICY=ON
GO
USE [QLSVNhom]
GO
CREATE USER [lab4] FOR LOGIN [lab4]
GO
USE [QLSVNhom]
GO
ALTER ROLE [db_owner] ADD MEMBER [lab4]
GO


if OBJECT_ID('DANGNHAP') is not null
	drop function DANGNHAP
go
create function DANGNHAP (@tendangnhap nvarchar(100), @matkhau VARBINARY(MAX))
returns varchar(MAX)
as
begin
	DECLARE @res VARCHAR(MAX)
	IF EXISTS(select *
	from NHANVIEN
	WHERE TENDN=@tendangnhap and MATKHAU=@matkhau)
	BEGIN
		DECLARE @time as VARCHAR(MAX) = CONVERT( VARCHAR(MAX) , GETDATE(), 120)
		set @res = CONCAT(
						CONVERT(VARCHAR(MAX), CAST(@tendangnhap AS VARBINARY(MAX)), 1),
						'.',
						CONVERT(VARCHAR(MAX), CAST(@time AS VARBINARY(MAX)), 1),
						'.',
						CONVERT(varchar(MAX), HASHBYTES('SHA1',CONCAT(@tendangnhap, @time, N'passLab3')), 1))
	END
	ELSE
	BEGIN
		set @res = '400'
	END
	RETURN @res
END
GO

--------------- middleware check token
if OBJECT_ID('checkToken') is not null
	drop function checkToken
go
create function checkToken (@token varchar(MAX))
returns VARCHAR(20)
as
BEGIN
	DECLARE @res VARCHAR(20)
	DECLARE @tendangnhap NVARCHAR(MAX)
	DECLARE @time VARCHAR(MAX)
	DECLARE @hash VARCHAR(MAX)
	set @tendangnhap = CAST(CONVERT(VARBINARY(MAX), SUBSTRING(@token, 1, CHARINDEX('.', @token) - 1) , 1) AS NVARCHAR(MAX))
	set @time = CAST(CONVERT(VARBINARY(MAX), SUBSTRING(@token, CHARINDEX('.', @token) + 1, CHARINDEX('.', @token, CHARINDEX('.', @token) + 1) - CHARINDEX('.', @token) - 1) , 1) AS VARCHAR(MAX))
	set @hash = SUBSTRING(@token, CHARINDEX('.', @token, CHARINDEX('.', @token) + 1) + 1, LEN(@token) - CHARINDEX('.', @token, CHARINDEX('.', @token) + 1))
	IF (@hash = CONVERT(varchar(MAX), HASHBYTES('SHA1',CONCAT(@tendangnhap, @time, N'passLab3')), 1) and DATEDIFF(MINUTE, @time, GETDATE()) < 60)
	BEGIN
		select @res = MANV
		from NHANVIEN
		WHERE TENDN = @tendangnhap
	END
	ELSE
	BEGIN
		set @res = ''
	END
	RETURN @res
END
GO

----- iv>   Xây dựng (lập trình) màn hình quản lý lớp học
if OBJECT_ID('dsLop') is not null
	drop proc dsLop
go
CREATE proc dsLop
	@token varchar(MAX)
as
begin
	declare @MaNV as NVARCHAR(20)
	set @MaNV = dbo.checkToken(@token)
	if @MaNV = 'admin'
		begin
		select *
		from LOP
	end
		else
		begin
		select *
		from LOP
		where MANV = @MaNV
	end
end
go

----- v> Xây dựng (lập trình) màn hình sinh viên của từng lớp (lưu ý chỉ được phép thay đổi 
-----    thông tin của những sinh viên thuộc lớp mà nhân viên đó quản lý) 
if OBJECT_ID('dsSinhVien') is not null
	drop proc dsSinhVien
go
CREATE proc dsSinhVien
	@token varchar(MAX),
	@MALOP VARCHAR(20)
as
BEGIN
	declare @MaNV as NVARCHAR(20)
	set @MaNV = dbo.checkToken(@token)
	select MASV, HOTEN, NGAYSINH, DIACHI
	from SINHVIEN
	where MALOP in (select MALOP
	from LOP
	where MALOP = @MALOP and (MANV = @MaNV or @MaNV = 'admin'))
END
go

if OBJECT_ID('editSinhVien') is not null
	drop proc editSinhVien
go
CREATE proc editSinhVien
	@token varchar(MAX),
	@MASV VARCHAR(20),
	@HOTEN NVARCHAR(100),
	@NGAYSINH DATE,
	@DIACHI NVARCHAR(100)
as
begin
	declare @MaNV as NVARCHAR(20)
	set @MaNV = dbo.checkToken(@token)
	if @MaNV = 'admin'
		begin
		update SINHVIEN set HOTEN = @HOTEN, NGAYSINH = @NGAYSINH, DIACHI = @DIACHI where MASV = @MASV
	end
		else
		begin
		update SINHVIEN set HOTEN = @HOTEN, NGAYSINH = @NGAYSINH, DIACHI = @DIACHI where MASV = @MASV and MALOP in (select MALOP
			from LOP
			where MANV = @MaNV)
	end
end
go

if OBJECT_ID('addBangDiem') is not null
	drop proc addBangDiem
go
CREATE proc addBangDiem
	@token varchar(MAX),
	@MASV VARCHAR(20),
	@MAHP VARCHAR(20),
	@DIEM VARBINARY(MAX)
as
begin
	declare @MaNV as NVARCHAR(20)
	set @MaNV = dbo.checkToken(@token)
	if @MaNV = 'admin'
		begin
		insert into BANGDIEM(MASV,MAHP,DIEMTHI)
		values
			(@MASV, @MAHP, @DIEM)
	end
		else
		begin
		if exists (select *
		from LOP
		where MALOP in (select MALOP
			from SINHVIEN
			where MASV = @MASV) and MANV = @MaNV)
			begin
			insert into BANGDIEM(MASV,MAHP,DIEMTHI)
			values
				(@MASV, @MAHP, @DIEM)
		end
	end
end
go

----------- INSERT TABLE
-- Thêm một tài khoản admin
-- private key: <RSAKeyValue><Modulus>tPok4ifYlOKgJ1xrX/1ToLKaW3PZLg0zIoE9//kNZDhnn+RBA8omk2iAAhH4bXyFFODmWV8EAqb+KiwsSQh72Q==</Modulus><Exponent>AQAB</Exponent><P>wCL6kxkYn71Qm7MrIvrmJJ7hX/xaUjJPq/Q30U+wDSs=</P><Q>8SGYiLZmYYl9Akain8KcaoTuYpPBbPd5ivwQpStgQQs=</Q><DP>TzWca8wq1J/tfWLt46vf+TTu4O1eJwjKw68Y29eoUqU=</DP><DQ>CvyG4KO+4m7LPVFOk4zgZ5IK8n7c70QPS5/UelIRnqU=</DQ><InverseQ>fhUrGRLrbBwPsuwbMjhR/HrelpANkqPo3M+k7Q/FEuw=</InverseQ><D>ANNfVbTO1ScPif5u8vop5oLKF364z+/5er/SM5fESt/BBzugitOI8QeH3nfJMutWBuqIGkP3TXH9lbMHQlBOjQ==</D></RSAKeyValue>
EXEC SP_INS_PUBLIC_ENCRYPT_NHANVIEN 'admin', N'admin', 'admin@yahoo.com', 0x4FAACB7F73ED1DBFED508D4B64C85CF9E789B1FB221FE5653ED456B433360C341B0FF2CD2EBFD74575A23B2B6D08A065DB9B22F67B700A216BAF6D9457142147, N'admin', 0xD033E22AE348AEB5660FC2140AEC35850C4DA997
go
EXEC SP_INS_PUBLIC_ENCRYPT_NHANVIEN 'NV01', N'NGUYEN VAN A', 'nva@yahoo.com', 0x4FAACB7F73ED1DBFED508D4B64C85CF9E789B1FB221FE5653ED456B433360C341B0FF2CD2EBFD74575A23B2B6D08A065DB9B22F67B700A216BAF6D9457142147, N'NVA', 0x40BD001563085FC35165329EA1FF5C5ECBDBBEEF
GO
EXEC SP_INS_PUBLIC_ENCRYPT_NHANVIEN 'NV02', N'NGUYEN VAN B', 'nvb@yahoo.com', 0x4FAACB7F73ED1DBFED508D4B64C85CF9E789B1FB221FE5653ED456B433360C341B0FF2CD2EBFD74575A23B2B6D08A065DB9B22F67B700A216BAF6D9457142147, N'NVB', 0x40BD001563085FC35165329EA1FF5C5ECBDBBEEF
GO
--------------- TABLE HOCPHAN
INSERT INTO HOCPHAN
VALUES('CSC15002', N'Bảo mật cơ sở dữ liệu', 4)
INSERT INTO HOCPHAN
VALUES('CSC15003', N'Mã hóa ứng dụng', 4)
INSERT INTO HOCPHAN
VALUES('CSC15005', N'Nhập môn mã hóa mật mã', 4)
INSERT INTO HOCPHAN
VALUES('CSC15006', N'Nhập môn xử lý ngôn ngữ tự nhiên', 4)
INSERT INTO HOCPHAN
VALUES('CSC14005', N'Nhập môn học máy', 4)
GO

--------------- TABLE LOP
INSERT INTO LOP 
VALUES('20CNTThuc', N'K20 Công Nghệ Tri Thức', 'NV01')
INSERT INTO LOP 
VALUES('20MMT', N'K20 Mạng Máy Tính', 'NV02')
INSERT INTO LOP 
VALUES('20KHDL', N'K20 Khoa Học Dữ Liệu', 'NV01')
GO

--------------- TABLE SINHVIEN
INSERT INTO SINHVIEN
VALUES('20127102', N'Hoàng Hữu Minh An', '2002-06-24', N'135B ktx Trần Hưng Đạo Quận 1', 
	   '20CNTThuc', N'20127102@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))

INSERT INTO SINHVIEN
VALUES('20127072', N'Lê Võ Huỳnh Thanh', '2002-06-24', N'Quận 8', 
	   '20CNTThuc', N'20127072@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))

INSERT INTO SINHVIEN
VALUES('20127424', N'Trần Tiến Hoàng', '2002-06-24', N'Quận 8', 
	   '20CNTThuc', N'20127424@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))

INSERT INTO SINHVIEN
VALUES('20127091', N'Lê Trọng Anh Tú', '2002-06-24', N'Quận 10', 
	   '20CNTThuc', N'20127091@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))

INSERT INTO SINHVIEN
VALUES('2012X002', N'Trọng Đạt', '2002-06-24', N'Quận 1', 
	   '20MMT', N'2012X002@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))

INSERT INTO SINHVIEN
VALUES('2012X004', N'Hoàng Tiến', '2002-06-24', N'Quận 3', 
	   '20MMT', N'2012X004@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))


INSERT INTO SINHVIEN
VALUES('2012X010', N'Hoàng Ainz', '2002-06-24', N'Quận 3', 
	   '20KHDL', N'2012X005@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))

INSERT INTO SINHVIEN
VALUES('2012X023', N'Huỳnh Võ', '2002-06-24', N'Quận 2', 
	   '20KHDL', N'2012X011@student.hcmus.edu.vn', HASHBYTES('MD5', '123'))
GO
-- delete from BANGDIEM
-- delete from SINHVIEN
-- delete from LOP
-- delete from HOCPHAN
-- DELETE from NHANVIEN