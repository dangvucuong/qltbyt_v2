USE [master]
GO
/****** Object:  Database [db_QLTBYT_NEW]    Script Date: 08/03/2026 11:42:16 ******/
CREATE DATABASE [db_QLTBYT_NEW] ON  PRIMARY 
( NAME = N'db_QLTBYT', FILENAME = N'D:\DVC\PROJECT\QLTBYT_NEW\DATA\db_QLTBYT_NEW.mdf' , SIZE = 23552KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'db_QLTBYT_NEW_log', FILENAME = N'D:\DVC\PROJECT\QLTBYT_NEW\DATA\db_QLTBYT_NEW_log.ldf' , SIZE = 7616KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [db_QLTBYT_NEW] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [db_QLTBYT_NEW].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [db_QLTBYT_NEW] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET ANSI_NULLS OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET ANSI_PADDING OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET ARITHABORT OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [db_QLTBYT_NEW] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [db_QLTBYT_NEW] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [db_QLTBYT_NEW] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET  DISABLE_BROKER
GO
ALTER DATABASE [db_QLTBYT_NEW] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [db_QLTBYT_NEW] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [db_QLTBYT_NEW] SET  READ_WRITE
GO
ALTER DATABASE [db_QLTBYT_NEW] SET RECOVERY SIMPLE
GO
ALTER DATABASE [db_QLTBYT_NEW] SET  MULTI_USER
GO
ALTER DATABASE [db_QLTBYT_NEW] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [db_QLTBYT_NEW] SET DB_CHAINING OFF
GO
USE [db_QLTBYT_NEW]
GO
/****** Object:  Table [dbo].[ThongtinHS]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThongtinHS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Magiaykiemdinh] [nvarchar](50) NULL,
	[Magiaynhapkhau] [nvarchar](50) NULL,
	[Thoihangiaynhapkhau] [nvarchar](50) NULL,
	[Magiayphepluuhanh] [nvarchar](50) NULL,
	[Thoihangiayphepluuhanh] [nvarchar](50) NULL,
	[MagiayCO] [nvarchar](50) NULL,
	[XuatxuCO] [nvarchar](50) NULL,
	[MaCQ] [nvarchar](50) NULL,
	[NamsxCQ] [nvarchar](50) NULL,
	[MatokhaiHQ] [nvarchar](50) NULL,
	[Packinglist] [nvarchar](50) NULL,
	[Mavandon] [nvarchar](50) NULL,
	[MaInvoiceNK] [nvarchar](50) NULL,
	[MahoadonBH] [nvarchar](50) NULL,
	[FileHS] [nvarchar](max) NULL,
	[Thongtingoithau] [nvarchar](max) NULL,
	[GiacoVAT] [float] NULL,
	[GiatruVAT] [float] NULL,
	[Thongtinkhauhao] [nvarchar](max) NULL,
	[Nguonvon] [nvarchar](50) NULL,
	[MaThietbi] [nvarchar](max) NULL,
 CONSTRAINT [PK_ThongtinHS\] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Thongbaostatus]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Thongbaostatus](
	[ID] [int] NOT NULL,
	[IDthongbao] [int] NULL,
	[IDuser] [int] NULL,
	[read_status] [int] NULL,
 CONSTRAINT [PK_Thongbaostatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Thongbao]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Thongbao](
	[ID] [int] NOT NULL,
	[Noidung] [nchar](500) NULL,
	[Ngaygio] [datetime] NULL,
	[Nguoitao] [nchar](50) NULL,
	[Trangthai] [int] NULL,
	[Tieude] [nchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Taikhoan]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Taikhoan](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Tendangnhap] [nvarchar](50) NULL,
	[Matkhau] [nvarchar](50) NULL,
	[Admin] [int] NULL,
	[Hoten] [nvarchar](200) NULL,
	[Dienthoai] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[RegID] [nvarchar](500) NULL,
	[LoaiTK] [int] NULL,
	[MaDV] [nvarchar](500) NULL,
 CONSTRAINT [PK_Taikhoan] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Solothuocvattu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Solothuocvattu](
	[ID] [int] NOT NULL,
	[Solo] [nvarchar](100) NULL,
	[IDnhacungcap] [int] NULL,
	[Ngaygio] [datetime] NULL,
	[IDnguoinhap] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Quanlyvattu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Quanlyvattu](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Tenvattu] [nvarchar](500) NULL,
	[Donvitinh] [nvarchar](100) NULL,
	[Soluong] [int] NULL,
	[MaTB] [int] NULL,
 CONSTRAINT [PK_Quanlyvattu] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Quanlythietbi]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Quanlythietbi](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaTB] [nvarchar](50) NULL,
	[MaKT] [nvarchar](50) NULL,
	[Madinhdanh] [nvarchar](100) NULL,
	[TenTB] [nvarchar](max) NULL,
	[Loai] [int] NULL,
	[Kyhieu] [nvarchar](500) NULL,
	[Serial] [nvarchar](500) NULL,
	[ThongsoKT] [nvarchar](max) NULL,
	[Ngaytiepnhan] [datetime] NULL,
	[Donvibaotri] [int] NULL,
	[Donvibangiao] [int] NULL,
	[Donvisudung] [int] NULL,
	[Nguoiquanly] [int] NULL,
	[Thoigianbh] [nvarchar](50) NULL,
	[HangSX] [nvarchar](500) NULL,
	[NuocSX] [nvarchar](500) NULL,
	[NamSX] [nvarchar](50) NULL,
	[Vattudikem] [nvarchar](500) NULL,
	[MocTGBH] [nvarchar](50) NULL,
	[MocTGBT] [nvarchar](50) NULL,
	[MocTGHieuchuan] [nvarchar](50) NULL,
	[MocTGKiemdinh] [nvarchar](50) NULL,
	[Trangthai] [int] NULL,
	[Tinhtrang] [nvarchar](500) NULL,
	[IDHoso] [nvarchar](max) NULL,
	[Ngaycapnhat] [datetime] NULL,
	[idGoithau] [nvarchar](50) NULL,
	[Soluong] [float] NULL,
	[Dongia] [nvarchar](500) NULL,
	[Thanhtien] [nvarchar](500) NULL,
	[Ghichu] [nvarchar](max) NULL,
	[NDXuly] [nvarchar](500) NULL,
	[ChiphiXuly] [float] NULL,
	[Phanloai] [nvarchar](20) NULL,
	[Phannhom] [nvarchar](500) NULL,
	[Nguonmuasam] [nvarchar](500) NULL,
	[Thoigianduavaosd] [nvarchar](500) NULL,
	[Donvicungcap] [int] NULL,
	[SoTTMua] [nvarchar](50) NULL,
	[Tenviettat] [nvarchar](500) NULL,
	[Thietbichinh] [nvarchar](500) NULL,
	[Trangthaisua] [nchar](10) NULL,
	[Taikhoancapnhat] [int] NULL,
	[stt_bbbg] [nvarchar](50) NULL,
	[dvt] [nvarchar](50) NULL,
	[Hankiemdinh] [date] NULL,
 CONSTRAINT [PK_Thongtinchung] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Quanlythietbi', @level2type=N'COLUMN',@level2name=N'Trangthai'
GO
/****** Object:  Table [dbo].[QuanlyNhansu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuanlyNhansu](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Hoten] [nvarchar](50) NULL,
	[SoCMND] [nvarchar](50) NULL,
	[Dienthoai] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[MaDV] [int] NULL,
	[LoaiNhansu] [int] NULL,
	[IDTaikhoan] [int] NULL,
 CONSTRAINT [PK_QuanlyNhansu] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuanlyMa]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuanlyMa](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IDTB] [int] NULL,
	[QRCODE] [nvarchar](50) NULL,
	[NFC] [nvarchar](50) NULL,
	[IDNguoiSD] [int] NULL,
 CONSTRAINT [PK_QuanlyMa] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Quanlyfile]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Quanlyfile](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Path] [nvarchar](500) NULL,
	[Mota] [nvarchar](500) NULL,
	[idTB] [int] NULL,
	[Ngaythang] [date] NULL,
 CONSTRAINT [PK_Quanlyfile] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuanlyDV]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuanlyDV](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Tendonvi] [nvarchar](500) NULL,
	[Masothue] [nvarchar](50) NULL,
	[Diachi] [nvarchar](max) NULL,
	[Dienthoai] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[Nguoichiutrachnhiem] [nvarchar](50) NULL,
	[MaLoai] [int] NULL,
 CONSTRAINT [PK_QuanlyDV] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Nhapthuocvattu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Nhapthuocvattu](
	[ID] [int] NOT NULL,
	[IDhanghoa] [int] NOT NULL,
	[IDsolo] [int] NULL,
	[IDkho] [int] NULL,
	[Hansudung] [date] NULL,
	[IDdvt] [int] NULL,
	[SLnhap] [int] NULL,
	[Dongia] [int] NULL,
	[Hoadon] [nvarchar](1) NULL,
	[IDnguoinhap] [int] NULL,
	[Ngaygio] [datetime] NULL,
	[SLtonkho] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Nhacungcap]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Nhacungcap](
	[ID] [int] NOT NULL,
	[Thongtin] [nvarchar](500) NULL,
	[MST] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LogThietbi]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogThietbi](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaTB] [int] NULL,
	[Loai] [int] NULL,
	[Noidung] [nvarchar](500) NULL,
	[Ngaygio] [datetime] NULL,
	[NguoiQL] [int] NULL,
	[Taikhoan] [nvarchar](50) NULL,
	[Trangthai] [int] NULL,
	[Chiphi] [float] NULL,
	[Keyword] [nvarchar](500) NULL,
	[Idbatdau] [int] NULL,
 CONSTRAINT [PK_LogThietbi] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1-BAOHONG; 2-DE NGHI KIEM TRA; 3-YEU CAU KHAC' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LogThietbi', @level2type=N'COLUMN',@level2name=N'Loai'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1- bao hong; 2- de nghi kiem tra; 3- yeu cau khac;4- chuyen giao tbyt' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'LogThietbi', @level2type=N'COLUMN',@level2name=N'Noidung'
GO
/****** Object:  Table [dbo].[LogHethong]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogHethong](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Taikhoan] [int] NULL,
	[Noidung] [nvarchar](max) NULL,
	[Thoigian] [datetime] NULL,
	[MaTB] [int] NULL,
 CONSTRAINT [PK_LogHethong] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InputByDate]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InputByDate](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaTB] [nchar](50) NOT NULL,
	[Sogio] [int] NULL,
	[Soca] [int] NULL,
	[InputDate] [date] NOT NULL,
	[Taikhoan] [int] NULL,
 CONSTRAINT [PK_InputByDate] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Goithau]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Goithau](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idGoithau] [nvarchar](50) NULL,
	[Tengoithau] [nvarchar](max) NULL,
	[Tinhtrang] [nvarchar](500) NULL,
	[Donvitrungthau] [nvarchar](500) NULL,
	[MST] [nvarchar](200) NULL,
	[SoHD] [nvarchar](50) NULL,
	[Giatri] [float] NULL,
 CONSTRAINT [PK_Goithau] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventKeyword]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventKeyword](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Keyword] [nchar](50) NOT NULL,
 CONSTRAINT [PK_EventKeyword] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMTrangthai]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMTrangthai](
	[MaTrangthai] [int] NOT NULL,
	[TenTrangthai] [nvarchar](500) NULL,
 CONSTRAINT [PK_DMTrangthai] PRIMARY KEY CLUSTERED 
(
	[MaTrangthai] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMthuocvattu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMthuocvattu](
	[ID] [int] NOT NULL,
	[Hanghoa] [nvarchar](200) NULL,
	[Mahanghoa] [nvarchar](20) NULL,
	[Hoatchat] [nvarchar](100) NULL,
	[Hamluong] [nvarchar](50) NULL,
	[IDloai] [int] NULL,
	[Ngaygio] [datetime] NULL,
	[UserID] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMTacvu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMTacvu](
	[Matacvu] [int] IDENTITY(1,1) NOT NULL,
	[Tentacvu] [nvarchar](500) NULL,
 CONSTRAINT [PK_DMTacvu] PRIMARY KEY CLUSTERED 
(
	[Matacvu] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMloaithuocvattu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMloaithuocvattu](
	[ID] [int] NOT NULL,
	[Tenloai] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMLoaiTB]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMLoaiTB](
	[MaloaiTB] [int] IDENTITY(1,1) NOT NULL,
	[TenloaiTB] [nvarchar](500) NULL,
 CONSTRAINT [PK_DMLoaiTB] PRIMARY KEY CLUSTERED 
(
	[MaloaiTB] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMkho]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMkho](
	[ID] [int] NOT NULL,
	[Tenkho] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMdonvitinh]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMdonvitinh](
	[ID] [int] NOT NULL,
	[dvt] [nvarchar](20) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DanhmucLoaiNS]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DanhmucLoaiNS](
	[MaloaiNS] [int] IDENTITY(1,1) NOT NULL,
	[TenloaiNS] [nvarchar](500) NULL,
 CONSTRAINT [PK_DanhmucLoaiNS] PRIMARY KEY CLUSTERED 
(
	[MaloaiNS] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DanhmucHoso]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DanhmucHoso](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LinkHS] [nvarchar](max) NULL,
	[Tenhienthi] [nvarchar](max) NULL,
 CONSTRAINT [PK_DanhmucHoso] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DanhmucDV]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DanhmucDV](
	[MaloaiDV] [int] IDENTITY(1,1) NOT NULL,
	[TenloaiDV] [nvarchar](500) NULL,
 CONSTRAINT [PK_DanhmucDV] PRIMARY KEY CLUSTERED 
(
	[MaloaiDV] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Country]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Country](
	[CountryID] [int] NOT NULL,
	[CountryName] [varchar](50) NULL,
	[TwoCharCountryCode] [char](2) NULL,
	[ThreeCharCountryCode] [char](3) NULL,
 CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED 
(
	[CountryID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Xuatthuocvattu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Xuatthuocvattu](
	[ID] [int] NOT NULL,
	[IDnhap] [int] NOT NULL,
	[SLxuat] [int] NULL,
	[IDchidinh] [int] NULL,
	[Ghichu] [nvarchar](500) NULL,
	[Dongia] [int] NULL,
	[Hoadon] [nvarchar](1) NULL,
	[Ngaygio] [datetime] NULL,
	[IDnguoinhap] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_report]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_report]
AS
SELECT     dbo.Quanlythietbi.ID, dbo.Quanlythietbi.MaTB, dbo.Quanlythietbi.TenTB, dbo.Quanlythietbi.Loai, dbo.Quanlythietbi.Kyhieu, dbo.Quanlythietbi.Serial, dbo.Quanlythietbi.ThongsoKT, 
                      dbo.Quanlythietbi.Ngaytiepnhan, dbo.Quanlythietbi.Donvibangiao, dbo.Quanlythietbi.Donvisudung, dbo.Quanlythietbi.Nguoiquanly, dbo.Quanlythietbi.Thoigianbh, dbo.Quanlythietbi.HangSX, 
                      dbo.Quanlythietbi.NuocSX, dbo.Quanlythietbi.NamSX, dbo.Quanlythietbi.Vattudikem, dbo.Quanlythietbi.MocTGBH, dbo.Quanlythietbi.MocTGBT, dbo.Quanlythietbi.MocTGHieuchuan, 
                      dbo.Quanlythietbi.MocTGKiemdinh, dbo.Quanlythietbi.Trangthai, dbo.Quanlythietbi.IDHoso, dbo.Quanlythietbi.Ngaycapnhat, dbo.Quanlythietbi.idGoithau, dbo.Quanlythietbi.Soluong, 
                      dbo.Quanlythietbi.Dongia, dbo.Quanlythietbi.Thanhtien, dbo.Quanlythietbi.Ghichu, dbo.Quanlythietbi.NDXuly, dbo.Quanlythietbi.ChiphiXuly, dbo.Quanlythietbi.Phanloai, 
                      dbo.DMLoaiTB.TenloaiTB
FROM         dbo.Quanlythietbi LEFT OUTER JOIN
                      dbo.DMLoaiTB ON dbo.Quanlythietbi.Loai = dbo.DMLoaiTB.MaloaiTB
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Quanlythietbi"
            Begin Extent = 
               Top = 19
               Left = 51
               Bottom = 332
               Right = 222
            End
            DisplayFlags = 280
            TopColumn = 2
         End
         Begin Table = "DMLoaiTB"
            Begin Extent = 
               Top = 80
               Left = 325
               Bottom = 303
               Right = 485
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 34
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or =' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_report'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_report'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_report'
GO
/****** Object:  View [dbo].[v_PushNhansu]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_PushNhansu]
AS
SELECT     Tendangnhap, RegID, ID, LoaiTK, Email, Dienthoai, Hoten
FROM         dbo.Taikhoan
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Taikhoan"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 232
               Right = 198
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 11
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 3765
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_PushNhansu'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_PushNhansu'
GO
/****** Object:  View [dbo].[v_Logthietbi]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_Logthietbi]
AS
SELECT     dbo.LogThietbi.ID, dbo.LogThietbi.MaTB, dbo.LogThietbi.Loai, dbo.LogThietbi.Ngaygio, dbo.LogThietbi.Taikhoan, dbo.LogThietbi.Trangthai, dbo.LogThietbi.NguoiQL, dbo.Quanlythietbi.Donvisudung, 
                      dbo.Quanlythietbi.Nguoiquanly, dbo.Quanlythietbi.Serial, dbo.LogThietbi.Chiphi, dbo.LogThietbi.Noidung
FROM         dbo.LogThietbi INNER JOIN
                      dbo.Quanlythietbi ON dbo.LogThietbi.MaTB = dbo.Quanlythietbi.ID
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "LogThietbi"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 2
         End
         Begin Table = "Quanlythietbi"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 136
               Right = 431
            End
            DisplayFlags = 280
            TopColumn = 25
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 14
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_Logthietbi'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_Logthietbi'
GO
/****** Object:  View [dbo].[v_userthongbao]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_userthongbao]
AS
SELECT dbo.Thongbaostatus.IDthongbao, dbo.Thongbaostatus.IDuser, dbo.Thongbaostatus.read_status, dbo.Thongbao.Noidung, dbo.Thongbao.Ngaygio, dbo.Thongbao.Nguoitao, dbo.Thongbao.Tieude, dbo.Thongbaostatus.ID, dbo.Taikhoan.Hoten
FROM  dbo.Thongbaostatus INNER JOIN
         dbo.Thongbao ON dbo.Thongbaostatus.IDthongbao = dbo.Thongbao.ID INNER JOIN
         dbo.Taikhoan ON dbo.Thongbao.Nguoitao = dbo.Taikhoan.ID
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Thongbaostatus"
            Begin Extent = 
               Top = 37
               Left = 15
               Bottom = 358
               Right = 293
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Thongbao"
            Begin Extent = 
               Top = 23
               Left = 402
               Bottom = 363
               Right = 697
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Taikhoan"
            Begin Extent = 
               Top = 12
               Left = 798
               Bottom = 259
               Right = 1073
            End
            DisplayFlags = 280
            TopColumn = 1
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_userthongbao'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_userthongbao'
GO
/****** Object:  View [dbo].[v_Thongbao]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_Thongbao]
AS
SELECT dbo.Thongbao.ID, dbo.Thongbao.Noidung, dbo.Taikhoan.Hoten, dbo.Thongbao.Ngaygio, dbo.Taikhoan.Tendangnhap, dbo.Thongbao.Nguoitao, dbo.Thongbao.Trangthai
FROM  dbo.Thongbao INNER JOIN
         dbo.Taikhoan ON dbo.Thongbao.Nguoitao = dbo.Taikhoan.ID
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[26] 4[34] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Thongbao"
            Begin Extent = 
               Top = 12
               Left = 76
               Bottom = 259
               Right = 351
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Taikhoan"
            Begin Extent = 
               Top = 12
               Left = 427
               Bottom = 259
               Right = 702
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_Thongbao'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_Thongbao'
GO
/****** Object:  View [dbo].[v_Thietbi]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_Thietbi]
AS
SELECT     dbo.Quanlythietbi.ID, dbo.Quanlythietbi.MaTB, dbo.Quanlythietbi.TenTB, dbo.Quanlythietbi.Loai, dbo.Quanlythietbi.Kyhieu, dbo.Quanlythietbi.Serial, dbo.Quanlythietbi.ThongsoKT, 
                      dbo.Quanlythietbi.Ngaytiepnhan, dbo.Quanlythietbi.Donvibangiao, dbo.Quanlythietbi.Donvisudung, dbo.Quanlythietbi.Nguoiquanly, dbo.Quanlythietbi.Thoigianbh, dbo.Quanlythietbi.HangSX, 
                      dbo.Quanlythietbi.NuocSX, dbo.Quanlythietbi.NamSX, dbo.Quanlythietbi.Vattudikem, dbo.Quanlythietbi.MocTGBH, dbo.Quanlythietbi.MocTGBT, dbo.Quanlythietbi.MocTGHieuchuan, 
                      dbo.Quanlythietbi.MocTGKiemdinh, dbo.Quanlythietbi.Trangthai, dbo.Quanlythietbi.IDHoso, dbo.Quanlythietbi.Ngaycapnhat, dbo.Quanlythietbi.idGoithau, dbo.Quanlythietbi.Soluong, 
                      dbo.Quanlythietbi.Dongia, dbo.Quanlythietbi.Thanhtien, dbo.Quanlythietbi.Ghichu, dbo.Quanlythietbi.NDXuly, dbo.Quanlythietbi.ChiphiXuly, dbo.Quanlythietbi.Phanloai, dbo.QuanlyMa.QRCODE, 
                      dbo.QuanlyMa.NFC, dbo.Quanlythietbi.Madinhdanh
FROM         dbo.Quanlythietbi FULL OUTER JOIN
                      dbo.QuanlyMa ON dbo.Quanlythietbi.ID = dbo.QuanlyMa.IDTB
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Quanlythietbi"
            Begin Extent = 
               Top = 27
               Left = 599
               Bottom = 304
               Right = 770
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "QuanlyMa"
            Begin Extent = 
               Top = 59
               Left = 963
               Bottom = 179
               Right = 1123
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 35
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Fil' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_Thietbi'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'ter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_Thietbi'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_Thietbi'
GO
/****** Object:  View [dbo].[v_report1]    Script Date: 08/03/2026 11:42:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_report1]
AS
SELECT     dbo.v_report.ID, dbo.v_report.MaTB, dbo.v_report.TenTB, dbo.v_report.Loai, dbo.v_report.Kyhieu, dbo.v_report.Serial, dbo.v_report.ThongsoKT, dbo.v_report.Ngaytiepnhan, 
                      dbo.v_report.Donvibangiao, dbo.v_report.Donvisudung, dbo.v_report.Nguoiquanly, dbo.v_report.Thoigianbh, dbo.v_report.HangSX, dbo.v_report.NuocSX, dbo.v_report.NamSX, 
                      dbo.v_report.Vattudikem, dbo.v_report.MocTGBH, dbo.v_report.MocTGBT, dbo.v_report.MocTGHieuchuan, dbo.v_report.MocTGKiemdinh, dbo.v_report.Trangthai, dbo.v_report.IDHoso, 
                      dbo.v_report.Ngaycapnhat, dbo.v_report.idGoithau, dbo.v_report.Soluong, dbo.v_report.Dongia, dbo.v_report.Thanhtien, dbo.v_report.Ghichu, dbo.v_report.NDXuly, dbo.v_report.ChiphiXuly, 
                      dbo.v_report.Phanloai, dbo.v_report.TenloaiTB, dbo.QuanlyDV.Tendonvi
FROM         dbo.v_report LEFT OUTER JOIN
                      dbo.QuanlyDV ON dbo.v_report.Donvisudung = dbo.QuanlyDV.ID
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "v_report"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 236
               Right = 209
            End
            DisplayFlags = 280
            TopColumn = 4
         End
         Begin Table = "QuanlyDV"
            Begin Extent = 
               Top = 6
               Left = 247
               Bottom = 212
               Right = 435
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_report1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_report1'
GO
/****** Object:  Default [DF_LogThietbi_Trangthai]    Script Date: 08/03/2026 11:42:17 ******/
ALTER TABLE [dbo].[LogThietbi] ADD  CONSTRAINT [DF_LogThietbi_Trangthai]  DEFAULT ((1)) FOR [Trangthai]
GO

