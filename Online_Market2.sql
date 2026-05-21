USE QL_OnlineMarket;

SELECT * FROM NGUOIDUNG 

select * from donhang 
select * from sanpham 
select * from danhgia
select * from giohang
select *  from ChiTietDonHang

delete from sanpham
where masp =1039;
delete from ChiTietDonHang
where masp between 1006 and 1030;
delete from giohang
where masp between 1006 and 1031;
delete from sanpham
where masp = 1018;
delete from danhgia
where masp between 1006 and 1027;

select * from danhmuc
INSERT INTO DANHMUC (MADM, TENDM) 
VALUES 
(1, N'Trái cây các loại'),
(2, N'Dâu tây các loại'),
(3, N'Quà tặng trái cây'),
(4, N'Đi chợ');
--====Lệnh xóa sp nếu gặp lỗi============
DELETE FROM SanPham WHERE MASP = 1017;
--=======================================


-- Insert 20 sản phẩm (DM: tất cả loại trái cây)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, DONVITINH, URL_ANH, NGAYTHEM, TRANGTHAI, LUOTMUA, MADM, MAKM) 
VALUES 
-- Sản phẩm 1-5: Trái cây cao cấp
(N'Xoài cát chu loại 1', N'Xoài cát chu loại 1 trái to, ngọt thanh, thơm ngon', 85000, 50, N'kg', 'xoai_cat_chu.jpg', GETDATE(), N'Còn hàng', 120, 1, NULL)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, DONVITINH, URL_ANH, NGAYTHEM, TRANGTHAI, LUOTMUA, MADM, MAKM) 
VALUES 
(N'Nho xanh Autumn Crisp Nam Phi - 500G (I0004657)', N'Nho xanh không hạt nhập khẩu, trái to, giòn ngọt tự nhiên', 125000, 25, N'hộp', 'nho_xanh.jpg', GETDATE(), N'Còn hàng', 85, 1, NULL),
(N'Táo Envy New Zealand', N'Táo Envy New Zealand vỏ đỏ bóng, thịt giòn, vị ngọt thanh', 98000, 40, N'kg', 'tao-envy.jpg', GETDATE(), N'Còn hàng', 95, 1, NULL),
(N'Dâu tây Hàn Quốc', N'Dâu tây Hàn Quốc trái to, đỏ tươi, vị ngọt thanh', 180000, 20, N'hộp', 'dau_han_quoc.jpg', GETDATE(), N'Còn hàng', 65, 1, NULL),
(N'Cam sành Hà Giang', N'Cam sành Hà Giang trái to, mọng nước, vị ngọt đậm đà', 75000, 35, N'kg', 'cam-sanh.jpg', GETDATE(), N'Còn hàng', 110, 1, NULL),

-- Sản phẩm 6-10: Trái cây nhiệt đới
(N'Sầu riêng Monthong', N'Sầu riêng Monthong Thái Lan thơm ngon, cơm dày', 250000, 15, N'kg', 'sau-rieng.jpg', GETDATE(),N'Còn hàng', 45, 1, NULL),
(N'Chuối Laba Đà Lạt', N'Chuối Laba Đà Lạt trái to, thơm ngon đặc trưng', 45000, 60, N'nải', 'chuoi-laba.jpg', GETDATE(), N'Còn hàng', 150, 1, NULL),
(N'Măng cụt tươi', N'Măng cụt tươi trái to, vỏ tím, ruột trắng ngần', 68000, 20, N'kg', 'mang-cut.jpg', GETDATE(), N'Còn hàng', 75, 1, NULL),
(N'Ổi không hạt', N'Ổi không hạt giòn ngọt, ít chua, giàu vitamin C', 35000, 45, N'kg', 'oi-khong-hat.jpg', GETDATE(), N'Còn hàng', 90, 1, NULL),
(N'Dưa hấu không hạt', N'Dưa hấu không hạt trái tròn, ruột đỏ tươi, vị ngọt mát', 55000, 30, N'trái', 'dua-hau.jpg', GETDATE(), N'Còn hàng', 80, 1, NULL),

-- Sản phẩm 11-15: Trái cây nhập khẩu
(N'Việt quất New Zealand', N'Việt quất New Zealand trái to, ngọt thanh, giàu chất chống oxy hóa', 220000, 18, N'hộp', 'viet-quat.jpg', GETDATE(), N'Còn hàng', 40, 1, NULL),
(N'Cherry Mỹ đỏ', N'Cherry Mỹ đỏ trái to, giòn ngọt, màu đỏ đậm', 350000, 12, N'hộp', 'cherry-my.jpg', GETDATE(), N'Còn hàng', 30, 1, NULL);

--(N'Lê Hàn Quốc', N'Lê Hàn Quốc trái to, vỏ vàng, thịt giòn ngọt', 89000, 25, N'trái', 'le-han-quoc.jpg', GETDATE(), 1, 55, 1, NULL),
--(N'Kiwi vàng New Zealand', N'Kiwi vàng New Zealand vị ngọt thanh, giàu vitamin C', 120000, 22, N'hộp', 'kiwi-vang.jpg', GETDATE(), 1, 48, 1, NULL),
--(N'Nho đỏ Úc', N'Nho đỏ Úc trái to, vị ngọt đậm, không hạt', 145000, 20, N'hộp', 'nho-do-uc.jpg', GETDATE(), 1, 35, 1, NULL),

---- Sản phẩm 16-20: Trái cây đặc sản
--(N'Mận tam hoa Sơn La', N'Mận tam hoa Sơn La trái to, vị chua ngọt cân bằng', 42000, 40, N'kg', 'man-tam-hoa.jpg', GETDATE(), 1, 70, 1, NULL),
--(N'Vải thiều Thanh Hà', N'Vải thiều Thanh Hà trái to, cùi dày, vị ngọt thanh', 65000, 28, N'kg', 'vai-thieu.jpg', GETDATE(), 1, 85, 1, NULL),
--(N'Bơ sáp Đà Lạt', N'Bơ sáp Đà Lạt trái dài, thịt dẻo, béo ngậy', 52000, 35, N'kg', 'bo-sap.jpg', GETDATE(), 1, 95, 1, NULL),
--(N'Chôm chôm Thái', N'Chôm chôm Thái trái to, tơ đỏ, cùi dày ngọt', 48000, 32, N'kg', 'chom-chom.jpg', GETDATE(), 1, 60, 1, NULL),
--(N'Thanh long ruột đỏ', N'Thanh long ruột đỏ Long An trái to, vị ngọt thanh', 32000, 50, N'kg', 'thanh-long.jpg', GETDATE(), 1, 110, 1, NULL);

-- Insert khuyến mãi
INSERT INTO KHUYENMAI (TENKM, MOTA, PHANTRAMGIAM, NGAYBATDAU, NGAYKETTHUC, TRANGTHAI) VALUES 
(N'Giảm giá mùa hè', N'Khuyến mãi đặc biệt mùa hè', 10, '2024-01-01', '2024-12-31', N'Đang áp dụng'),
(N'Khuyến mãi cuối tuần', N'Giảm giá cuối tuần', 15, '2024-01-01', '2024-12-31', N'Đang áp dụng'),
(N'Giảm giá đặc biệt', N'Khuyến mãi đặc biệt', 20, '2024-01-01', '2024-06-30', N'Đang áp dụng');


-- Insert đánh giá
INSERT INTO DanhGia (MAND, MASP, SOSAO, NOIDUNG, NGAYDANHGIA) VALUES 
(1, 1006, 5, N'Xoài rất ngon, ngọt thanh, sẽ mua lại!', GETDATE()),
(2, 1006, 4, N'Xoài tươi, giá cả hợp lý', DATEADD(day, -1, GETDATE())),
(3, 1027, 5, N'Chất lượng tuyệt vời, giao hàng nhanh', DATEADD(day, -2, GETDATE())),
(1, 1018, 4, N'Nho ngọt, không hạt, rất thích', DATEADD(day, -3, GETDATE())),
(2, 1018, 3, N'Nho ngon nhưng hơi đắt', DATEADD(day, -4, GETDATE())),
(3, 1019, 5, N'Táo giòn ngọt, rất đáng mua', DATEADD(day, -5, GETDATE()));

select * from sanpham
select * from nguoidung 
select * from danhmuc 


-- Insert 10 sản phẩm dâu tây (DM: Dâu tây các loại)
INSERT INTO SanPham (TENSP, MOTA, GIA, SOLUONGTON, DONVITINH, URL_ANH, NGAYTHEM, TRANGTHAI, LUOTMUA, MADM, MAKM) 
VALUES 
(N'Dâu tây New Zealand', N'Dâu tây New Zealand trái to, đỏ tươi, vị ngọt thanh, giàu vitamin C', 195000, 30, N'hộp 250g', 'dau_tay_newzealand.jpg', GETDATE(), N'Còn hàng', 85, 2, NULL),
(N'Dâu tây Đà Lạt loại 1', N'Dâu tây Đà Lạt trái đều, màu đỏ cam, vị chua ngọt tự nhiên', 125000, 45, N'hộp 500g', 'dau_tay_dalat_1.jpg', GETDATE(), N'Còn hàng', 150, 2, NULL),
(N'Dâu tây Mỹ', N'Dâu tây nhập khẩu từ Mỹ, trái lớn, giòn ngọt, thơm đặc trưng', 220000, 25, N'hộp 300g', 'dau_tay_my.jpg', GETDATE(), N'Còn hàng', 60, 2, NULL),
(N'Dâu tây Đà Lạt loại 2', N'Dâu tây Đà Lạt trái vừa, vị chua ngọt cân đối, thích hợp làm sinh tố', 95000, 60, N'hộp 500g', 'dau_tay_dalat_2.jpg', GETDATE(), N'Còn hàng', 200, 2, NULL),
(N'Dâu tây Nhật Bản', N'Dâu tây Nhật Bản trái hình trái tim, ngọt lịm, hương thơm nồng nàn', 280000, 15, N'hộp 200g', 'dau_tay_nhat.jpg', GETDATE(), N'Còn hàng', 40, 2, NULL),
(N'Dâu tây organic', N'Dâu tây trồng hữu cơ, không thuốc trừ sâu, an toàn cho sức khỏe', 175000, 35, N'hộp 400g', 'dau_tay_organic.png', GETDATE(), N'Còn hàng', 75, 2, NULL),
(N'Dâu tây Hàn Quốc', N'Dâu tây Hàn Quốc trái dài, vị ngọt đậm, màu đỏ rực rỡ', 195000, 28, N'hộp 250g', 'dau_tay_hanquoc.jpg', GETDATE(), N'Còn hàng', 65, 2, NULL),
(N'Dâu tây baby', N'Dâu tây trái nhỏ, ngọt thanh, thích hợp cho trẻ em và trang trí', 85000, 50, N'hộp 300g', 'dau_tay_baby.jpg', GETDATE(), N'Còn hàng', 110, 2, NULL),
(N'Dâu tây đông lạnh', N'Dâu tây đông lạnh giữ trọn hương vị, tiện lợi cho làm sinh tố và bánh', 65000, 80, N'túi 500g', 'dau_tay_donglanh.jpg', GETDATE(), N'Còn hàng', 180, 2, NULL),
(N'Dâu tây tráng miệng', N'Dâu tây cao cấp chuyên dùng cho món tráng miệng và nhà hàng', 155000, 20, N'hộp 200g', 'dau_tay_trangmieng.jpg', GETDATE(), N'Còn hàng', 45, 2, NULL);

select * from sanpham 


-- Thêm cột phương thức giao hàng
ALTER TABLE DonHang 
ADD HINHTHUCGIAOHANG NVARCHAR(100) NULL;

ALTER TABLE DanhGia 
ADD HINHANH NVARCHAR(500) NULL;

select * from DanhGia

ALTER TABLE DonHang
ADD 
    HoTenNhanHang NVARCHAR(100),
    TinhThanh NVARCHAR(50),
    QuanHuyen NVARCHAR(50),
    PhuongXa NVARCHAR(50),
    HinhThucThanhToan NVARCHAR(50),
    PhuongThucGiaoHang NVARCHAR(50),
    PhiGiaoHang DECIMAL(18,2) DEFAULT 0,
    TongThanhToan DECIMAL(18,2) DEFAULT 0;


CREATE TABLE DiaChiNguoiDung (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NguoiDungId INT NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai NVARCHAR(20) NOT NULL,
    DiaChi NVARCHAR(255) NOT NULL,
    TinhThanh NVARCHAR(50) NOT NULL,
    QuanHuyen NVARCHAR(50) NOT NULL,
    PhuongXa NVARCHAR(50) NOT NULL,
    MacDinh BIT DEFAULT 0,
    LoaiDiaChi NVARCHAR(50),
    FOREIGN KEY (NguoiDungId) REFERENCES NguoiDung(MAND)
);

select * from donhang

-- Thêm cột trạng thái chi tiết nếu chưa có
ALTER TABLE DonHang 
ADD TrangThaiChiTiet NVARCHAR(50) DEFAULT N'Đang xử lý';

-- Cập nhật dữ liệu mẫu
UPDATE DonHang SET TrangThaiChiTiet = N'Đang xử lý' WHERE TRANG_THAI = N'Chờ xác nhận';
UPDATE DonHang SET TrangThaiChiTiet = N'Đang giao' WHERE TRANG_THAI = N'Đang giao hàng';
UPDATE DonHang SET TrangThaiChiTiet = N'Hoàn tất' WHERE TRANG_THAI = N'Đã giao';
UPDATE DonHang SET TrangThaiChiTiet = N'Hủy' WHERE TRANG_THAI = N'Đã hủy';

select * from nguoidung 

-- Cập nhật user thành Admin
UPDATE NguoiDung SET VAITRO = 'Admin' WHERE TENDANGNHAP = 'admin';

-- Hoặc tạo user Admin mới
INSERT INTO NguoiDung (TENDANGNHAP, MATKHAU, HOTEN, EMAIL, DIACHI, SODIENTHOAI, VAITRO)
VALUES ('admin', '123', N'Quản trị viên', 'admin@onlinemarket.com', N'Hà Nội', '0987654321', 'Admin');




SELECT * FROM NguoiDung WHERE TENDANGNHAP = 'admin';

SELECT * FROM NguoiDung 



WHERE TENDANGNHAP = 'admin' AND MATKHAU = '123456';

-- Xóa admin cũ nếu có
DELETE FROM NguoiDung WHERE TENDANGNHAP = 'admin';

-- Tạo admin mới với mật khẩu KHÔNG mã hóa (để test)
INSERT INTO NguoiDung (TENDANGNHAP, MATKHAU, HOTEN, EMAIL, DIACHI, SODIENTHOAI, VAITRO)
VALUES 
('admin', '123456', N'Admin User', 'admin@test.com', N'Hà Nội', '0987654321', 'Admin');

-- Tạo thêm user thường để test
INSERT INTO NguoiDung (TENDANGNHAP, MATKHAU, HOTEN, EMAIL, DIACHI, SODIENTHOAI, VAITRO)
VALUES 
('user1', '123456', N'Người dùng 1', 'user1@test.com', N'TPHCM', '0912345678', 'User');

-- Kiểm tra
SELECT * FROM NguoiDung;


select * from donhang 
ALTER TABLE DonHang
ADD DISCOUNT_AMOUNT DECIMAL(18,2) NULL,
    DISCOUNT_CODE NVARCHAR(50) NULL;
Select * from KHUYENMAI 



select * from danhmuc