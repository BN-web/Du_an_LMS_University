module.exports = [
"[externals]/next/dist/compiled/next-server/app-page-turbo.runtime.dev.js [external] (next/dist/compiled/next-server/app-page-turbo.runtime.dev.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/compiled/next-server/app-page-turbo.runtime.dev.js", () => require("next/dist/compiled/next-server/app-page-turbo.runtime.dev.js"));

module.exports = mod;
}),
"[externals]/next/dist/server/app-render/action-async-storage.external.js [external] (next/dist/server/app-render/action-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/action-async-storage.external.js", () => require("next/dist/server/app-render/action-async-storage.external.js"));

module.exports = mod;
}),
"[externals]/next/dist/server/app-render/work-unit-async-storage.external.js [external] (next/dist/server/app-render/work-unit-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/work-unit-async-storage.external.js", () => require("next/dist/server/app-render/work-unit-async-storage.external.js"));

module.exports = mod;
}),
"[externals]/next/dist/server/app-render/work-async-storage.external.js [external] (next/dist/server/app-render/work-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/work-async-storage.external.js", () => require("next/dist/server/app-render/work-async-storage.external.js"));

module.exports = mod;
}),
"[project]/lib/data.js [app-ssr] (ecmascript)", ((__turbopack_context__) => {
"use strict";

// Data cho biểu đồ
__turbopack_context__.s([
    "bangDiemData",
    ()=>bangDiemData,
    "chartData",
    ()=>chartData,
    "classesData",
    ()=>classesData,
    "giangVienData",
    ()=>giangVienData,
    "giangVienList",
    ()=>giangVienList,
    "lichDayData",
    ()=>lichDayData,
    "lichSuDiemData",
    ()=>lichSuDiemData,
    "monHocList",
    ()=>monHocList,
    "nganhList",
    ()=>nganhList,
    "notifications",
    ()=>notifications,
    "phamViList",
    ()=>phamViList,
    "recentNotifications",
    ()=>recentNotifications,
    "studentsData",
    ()=>studentsData,
    "studentsInClass",
    ()=>studentsInClass,
    "thongBaoData",
    ()=>thongBaoData
]);
const chartData = [
    {
        name: "Database",
        daNop: 26,
        chuaNop: 8
    },
    {
        name: "Lập trình Wed",
        daNop: 12,
        chuaNop: 24
    },
    {
        name: "ASP.NET",
        daNop: 26,
        chuaNop: 20
    },
    {
        name: "Lập trình Mobile",
        daNop: 14,
        chuaNop: 38
    }
];
const notifications = [
    {
        id: 1,
        title: "Đã phân công giảng viên"
    },
    {
        id: 2,
        text: "Đã có thay đổi về lịch họp của bộ môn. Thời gian cuộc họp bị dời sang 14h chiều nay tại phòng 204"
    },
    {
        id: 3,
        text: "Đã có thay đổi về lịch họp của bộ môn. Thời gian cuộc họp bị dời sang 14h chiều nay tại phòng 204"
    },
    {
        id: 4,
        text: "Đã có thay đổi về lịch họp của bộ môn. Thời gian cuộc họp bị dời sang 14h chiều nay tại phòng 204"
    },
    {
        id: 5,
        text: "Đã có thay đổi về lịch họp của bộ môn. Thời gian cuộc họp bị dời sang 14h chiều nay tại phòng 204"
    }
];
const recentNotifications = [
    {
        id: 1,
        title: "Đã phân công giảng viên",
        content: "Đã phân công giảng viên mới cho lớp Database. Vui lòng kiểm tra và xác nhận.",
        timeAgo: "2 giờ trước",
        daDoc: true
    },
    {
        id: 2,
        title: "Thay đổi lịch họp",
        content: "Đã có thay đổi về lịch họp của bộ môn. Thời gian cuộc họp bị dời sang 14h chiều nay tại phòng 204",
        timeAgo: "3 giờ trước",
        daDoc: false
    },
    {
        id: 3,
        title: "Thay đổi lịch họp",
        content: "Đã có thay đổi về lịch họp của bộ môn. Thời gian cuộc họp bị dời sang 14h chiều nay tại phòng 204",
        timeAgo: "5 giờ trước",
        daDoc: true
    },
    {
        id: 4,
        title: "Phản hồi mới từ học viên",
        content: "Học viên Trần Thị B đã gửi phản hồi về bài giảng Lập trình Web. Vui lòng xem xét và phản hồi.",
        timeAgo: "1 ngày trước",
        daDoc: true
    }
];
const thongBaoData = [
    {
        id: 1,
        tieuDe: "Thông báo lịch họp Khoa tháng 11/2025",
        nguoiGui: "TS. Nguyễn Hải Trường",
        chucVu: "Trưởng khoa CNTT",
        thoiGian: "21/12/2025 19:00",
        noiDung: "Kính gửi các thầy cô giảng viên, Khoa Công nghệ thông tin trân trọng thông báo lịch họp định kỳ tháng 11/2025 như sau: - Thời gian: 14h00, Thứ Sáu, ngày 15/11/2025 - Địa điểm: Phòng họp 204, Cơ sở 1 - Nội dung: Báo cáo tình hình giảng dạy, thảo luận về chương trình đào tạo mới. Rất mong các thầy cô sắp xếp thời gian tham dự đầy đủ.",
        doiTuong: "Tất cả giảng viên",
        phamVi: "Bộ môn",
        daDoc: false
    },
    {
        id: 2,
        tieuDe: "Thay đổi thông báo vì lí do gửi ngu của Phúc - Trưởng khoa",
        nguoiGui: "Phúc",
        chucVu: "Trưởng khoa",
        thoiGian: "21/12/2025 19:00",
        noiDung: "Thay đổi lịch thi của môn lập trình web sang ngày 21/02/2030",
        doiTuong: "Tất cả sinh viên",
        phamVi: "Bộ môn",
        daDoc: true
    },
    {
        id: 3,
        tieuDe: "Thay đổi lịch thi môn lập trình Web • ADMIN",
        nguoiGui: "ADMIN",
        chucVu: "",
        thoiGian: "21/12/2025 19:00",
        noiDung: "Thay đổi lịch thi của môn lập trình web sang ngày 21/02/2030",
        doiTuong: "Tất cả sinh viên",
        phamVi: "Bộ môn",
        daDoc: true
    }
];
const phamViList = [
    "Bộ môn",
    "Khoa CNTT",
    "Khoa Điện",
    "Khoa Cơ Khí",
    "Tất cả"
];
const classesData = [
    {
        id: 1,
        maLop: "IT101",
        tenLop: "ITS01",
        mon: "Database",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Hải Trường",
        sinhVien: "40/45",
        trangThai: "Đang hoạt động",
        lich: "T2,T5 7:00 - 11:30 P.101 cơ sở 2 HCM"
    },
    {
        id: 2,
        maLop: "IT102",
        tenLop: "ITS02",
        mon: "Lập trình Web",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Văn B",
        sinhVien: "35/45",
        trangThai: "Đang hoạt động",
        lich: "T3,T6 13:00 - 16:30 P.201 cơ sở 1 HCM"
    },
    {
        id: 3,
        maLop: "IT103",
        tenLop: "ITS03",
        mon: "Database",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Hải Trường",
        sinhVien: "40/45",
        trangThai: "Đang hoạt động",
        lich: "T2,T4 8:00 - 11:30 P.301 cơ sở 2 HCM"
    },
    {
        id: 4,
        maLop: "IT104",
        tenLop: "ITS04",
        mon: "ASP.NET",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Hải Trường",
        sinhVien: "40/45",
        trangThai: "Đang hoạt động",
        lich: "T4,T7 7:00 - 10:30 P.102 cơ sở 2 HCM"
    },
    {
        id: 5,
        maLop: "IT105",
        tenLop: "ITS05",
        mon: "Lập trình Mobile",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Hải Trường",
        sinhVien: "40/45",
        trangThai: "Đang hoạt động",
        lich: "T3,T5 14:00 - 17:30 P.401 cơ sở 1 HCM"
    },
    {
        id: 6,
        maLop: "IT106",
        tenLop: "ITS06",
        mon: "Database",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Hải Trường",
        sinhVien: "40/45",
        trangThai: "Chưa hoạt động",
        lich: "T2,T6 9:00 - 12:30 P.501 cơ sở 2 HCM"
    },
    {
        id: 7,
        maLop: "IT107",
        tenLop: "ITS07",
        mon: "Database",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Hải Trường",
        sinhVien: "40/45",
        trangThai: "Chưa hoạt động",
        lich: "T4,T6 7:00 - 10:30 P.201 cơ sở 1 HCM"
    },
    {
        id: 8,
        maLop: "IT108",
        tenLop: "ITS08",
        mon: "Database",
        khoa: "Khóa 13",
        nganh: "Kỹ thuật IT",
        giangVien: "Nguyễn Hải Trường",
        sinhVien: "40/45",
        trangThai: "Chưa hoạt động",
        lich: "T3,T7 13:00 - 16:30 P.302 cơ sở 2 HCM"
    }
];
const studentsInClass = {
    IT101: [
        {
            mssv: "IT101",
            hoTen: "Nguyễn Hải Trường",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 7.0,
            trangThai: "Đang học"
        },
        {
            mssv: "IT102",
            hoTen: "Trần Văn Nam",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 5.0,
            trangThai: "Đang học"
        },
        {
            mssv: "IT103",
            hoTen: "Lê Thị Hoa",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 10,
            trangThai: "Đang học"
        },
        {
            mssv: "IT104",
            hoTen: "Phạm Minh Tuấn",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 4.0,
            trangThai: "Đang học"
        },
        {
            mssv: "IT105",
            hoTen: "Võ Thị Mai",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 8.0,
            trangThai: "Đang học"
        },
        {
            mssv: "IT106",
            hoTen: "Hoàng Văn Đức",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 7.0,
            trangThai: "Đang học"
        },
        {
            mssv: "IT107",
            hoTen: "Ngô Thị Lan",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 7.5,
            trangThai: "Đang học"
        },
        {
            mssv: "IT108",
            hoTen: "Đỗ Văn Hùng",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 9.0,
            trangThai: "Đã nghỉ"
        }
    ],
    IT102: [
        {
            mssv: "IT201",
            hoTen: "Nguyễn Thị Hương",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 8.5,
            trangThai: "Đang học"
        },
        {
            mssv: "IT202",
            hoTen: "Trần Minh Quân",
            khoa: "Khóa 13",
            nganh: "Kỹ thuật IT",
            tongDiem: 6.5,
            trangThai: "Đang học"
        }
    ]
};
const studentsData = [
    {
        mssv: "20131",
        hoTen: "Nguyễn Hải Trường",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đang học",
        ngaySinh: "15/08/2002",
        dienThoai: "0987654321",
        email: "truong.nh@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "3.8 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    },
    {
        mssv: "20132",
        hoTen: "Trần Văn Nam",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đang học",
        ngaySinh: "20/05/2002",
        dienThoai: "0912345678",
        email: "nam.tv@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "3.5 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    },
    {
        mssv: "20133",
        hoTen: "Lê Thị Hoa",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đang học",
        ngaySinh: "10/03/2002",
        dienThoai: "0909876543",
        email: "hoa.lt@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "3.9 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    },
    {
        mssv: "20134",
        hoTen: "Phạm Minh Tuấn",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đang học",
        ngaySinh: "25/12/2001",
        dienThoai: "0977123456",
        email: "tuan.pm@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "2.5 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    },
    {
        mssv: "20135",
        hoTen: "Võ Thị Mai",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đang học",
        ngaySinh: "05/07/2002",
        dienThoai: "0966789012",
        email: "mai.vt@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "3.7 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    },
    {
        mssv: "20136",
        hoTen: "Hoàng Văn Đức",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đang học",
        ngaySinh: "18/09/2002",
        dienThoai: "0955456789",
        email: "duc.hv@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "3.6 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    },
    {
        mssv: "20137",
        hoTen: "Ngô Thị Lan",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đã nghỉ",
        ngaySinh: "30/11/2002",
        dienThoai: "0944321098",
        email: "lan.nt@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "3.2 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    },
    {
        mssv: "20138",
        hoTen: "Đỗ Văn Hùng",
        khoa: "Khóa 13",
        khoaHoc: "Khoa CNTT",
        nganh: "Kỹ thuật IT",
        monLop: "Lập trình Web",
        giangVien: "Nguyễn Hải Trường",
        tongTinChi: 12,
        gpa: 4.0,
        trangThai: "Đã nghỉ",
        ngaySinh: "14/02/2002",
        dienThoai: "0933654321",
        email: "hung.dv@email.com",
        chuyenNganh: "Kỹ thuật IT",
        gpaTichLuy: "2.8 / 4.0",
        ngayTaoHoSo: "20/08/2020"
    }
];
const bangDiemData = [
    {
        mon: "Lập trình Web",
        baiTap: 8.5,
        giuaKy: 9.0,
        cuoiKy: 9.5,
        chuyenCan: "100%",
        tbMon: 9.2,
        diemChu: "A+",
        gpaMon: 4.0
    },
    {
        mon: "Cơ sở dữ liệu",
        baiTap: 7.0,
        giuaKy: 8.0,
        cuoiKy: 7.5,
        chuyenCan: "95%",
        tbMon: 7.6,
        diemChu: "B+",
        gpaMon: 3.5
    },
    {
        mon: "Mạng máy tính",
        baiTap: 9.0,
        giuaKy: 8.5,
        cuoiKy: 9.0,
        chuyenCan: "100%",
        tbMon: 8.8,
        diemChu: "A",
        gpaMon: 3.8
    }
];
const lichSuDiemData = [
    {
        thoiGian: "20/05/2024 10:30 AM",
        nguoiThucHien: "Trần Anh Tuấn",
        monHoc: "Lập trình Web",
        thanhPhanDiem: "Cuối kỳ",
        diemCu: 9.0,
        diemMoi: 9.5
    },
    {
        thoiGian: "18/05/2024 02:15 PM",
        nguoiThucHien: "Hệ thống",
        monHoc: "Cơ sở dữ liệu",
        thanhPhanDiem: "Giữa kỳ",
        diemCu: 7.5,
        diemMoi: 8.0
    },
    {
        thoiGian: "15/05/2024 09:00 AM",
        nguoiThucHien: "Nguyễn Thị Mai",
        monHoc: "Mạng máy tính",
        thanhPhanDiem: "Bài tập",
        diemCu: 8.0,
        diemMoi: 9.0
    },
    {
        thoiGian: "12/04/2024 11:45 AM",
        nguoiThucHien: "Trần Anh Tuấn",
        monHoc: "Lập trình Web",
        thanhPhanDiem: "Bài tập",
        diemCu: 8.0,
        diemMoi: 8.5
    },
    {
        thoiGian: "05/03/2024 04:20 PM",
        nguoiThucHien: "Lê Văn Hùng",
        monHoc: "Cơ sở dữ liệu",
        thanhPhanDiem: "Chuyên cần",
        diemCu: 10,
        diemMoi: 9
    }
];
const giangVienList = [
    "Nguyễn Văn A",
    "Nguyễn Hải Trường",
    "Trần Văn B",
    "Lê Thị C",
    "Phạm Văn D"
];
const giangVienData = [
    {
        id: "00001",
        maGiangVien: "00001",
        hoTen: "Nguyễn Hải Trường",
        gioiTinh: "Nam",
        ngaySinh: "15/08/1982",
        email: "truong.nh@example.edu",
        soDienThoai: "0987654321",
        hocVi: "Tiến sĩ",
        chucVu: "Giảng Viên",
        chuyenNganh: "Trí Tuệ Nhân Tạo & Học Máy",
        khoa: "Khoa Học Máy Tính",
        diaChi: "123 Example St, District 1, Ho Chi Minh City",
        ngayGiaNhap: "20/08/2010",
        tongSoLop: 42,
        trangThai: "Đang hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "Lập trình Web",
        soLopDangDay: 2,
        avatar: "/avatar-user-asian-male.jpg"
    },
    {
        id: "00002",
        maGiangVien: "00002",
        hoTen: "Trần Văn B",
        gioiTinh: "Nam",
        ngaySinh: "20/05/1985",
        email: "b.tran@example.edu",
        soDienThoai: "0912345678",
        hocVi: "Thạc sĩ",
        chucVu: "Giảng Viên",
        chuyenNganh: "Cơ sở Dữ liệu",
        khoa: "Khoa Học Máy Tính",
        diaChi: "456 Main St, District 3, Ho Chi Minh City",
        ngayGiaNhap: "15/09/2012",
        tongSoLop: 35,
        trangThai: "Đang hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "Database",
        soLopDangDay: 3,
        avatar: "/avatar-user-asian-male.jpg"
    },
    {
        id: "00003",
        maGiangVien: "00003",
        hoTen: "Lê Thị C",
        gioiTinh: "Nữ",
        ngaySinh: "10/03/1988",
        email: "c.le@example.edu",
        soDienThoai: "0909876543",
        hocVi: "Tiến sĩ",
        chucVu: "Trưởng Bộ Môn",
        chuyenNganh: "Mạng Máy Tính",
        khoa: "Khoa Học Máy Tính",
        diaChi: "789 Park Ave, District 7, Ho Chi Minh City",
        ngayGiaNhap: "01/10/2015",
        tongSoLop: 28,
        trangThai: "Đang hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "Mạng Máy Tính",
        soLopDangDay: 1,
        avatar: "/avatar-user-asian-male.jpg"
    },
    {
        id: "00004",
        maGiangVien: "00004",
        hoTen: "Phạm Văn D",
        gioiTinh: "Nam",
        ngaySinh: "25/12/1980",
        email: "d.pham@example.edu",
        soDienThoai: "0977123456",
        hocVi: "Thạc sĩ",
        chucVu: "Giảng Viên",
        chuyenNganh: "Lập trình Mobile",
        khoa: "Khoa Học Máy Tính",
        diaChi: "321 Tech St, District 2, Ho Chi Minh City",
        ngayGiaNhap: "05/11/2011",
        tongSoLop: 30,
        trangThai: "Đang hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "Lập trình Mobile",
        soLopDangDay: 2,
        avatar: "/avatar-user-asian-male.jpg"
    },
    {
        id: "00005",
        maGiangVien: "00005",
        hoTen: "Nguyễn Văn A",
        gioiTinh: "Nam",
        ngaySinh: "18/06/1983",
        email: "a.nguyen@example.edu",
        soDienThoai: "0966789012",
        hocVi: "Tiến sĩ",
        chucVu: "Giảng Viên",
        chuyenNganh: "ASP.NET",
        khoa: "Khoa Học Máy Tính",
        diaChi: "654 Dev Blvd, District 1, Ho Chi Minh City",
        ngayGiaNhap: "12/07/2009",
        tongSoLop: 50,
        trangThai: "Không hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "ASP.NET",
        soLopDangDay: 0,
        avatar: "/avatar-user-asian-male.jpg"
    },
    {
        id: "00006",
        maGiangVien: "00006",
        hoTen: "Hoàng Thị E",
        gioiTinh: "Nữ",
        ngaySinh: "30/11/1987",
        email: "e.hoang@example.edu",
        soDienThoai: "0955456789",
        hocVi: "Thạc sĩ",
        chucVu: "Giảng Viên",
        chuyenNganh: "Lập trình Web",
        khoa: "Khoa Học Máy Tính",
        diaChi: "987 Code Lane, District 5, Ho Chi Minh City",
        ngayGiaNhap: "22/03/2013",
        tongSoLop: 25,
        trangThai: "Không hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "Lập trình Web",
        soLopDangDay: 0,
        avatar: "/avatar-user-asian-male.jpg"
    },
    {
        id: "00007",
        maGiangVien: "00007",
        hoTen: "Võ Minh F",
        gioiTinh: "Nam",
        ngaySinh: "14/02/1984",
        email: "f.vo@example.edu",
        soDienThoai: "0944321098",
        hocVi: "Tiến sĩ",
        chucVu: "Giảng Viên",
        chuyenNganh: "Database",
        khoa: "Khoa Học Máy Tính",
        diaChi: "147 Data St, District 10, Ho Chi Minh City",
        ngayGiaNhap: "08/04/2014",
        tongSoLop: 20,
        trangThai: "Không hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "Database",
        soLopDangDay: 0,
        avatar: "/avatar-user-asian-male.jpg"
    },
    {
        id: "00008",
        maGiangVien: "00008",
        hoTen: "Đỗ Thị G",
        gioiTinh: "Nữ",
        ngaySinh: "05/09/1986",
        email: "g.do@example.edu",
        soDienThoai: "0933654321",
        hocVi: "Thạc sĩ",
        chucVu: "Giảng Viên",
        chuyenNganh: "Lập trình Web",
        khoa: "Khoa Học Máy Tính",
        diaChi: "258 Web Ave, District 1, Ho Chi Minh City",
        ngayGiaNhap: "15/06/2016",
        tongSoLop: 15,
        trangThai: "Đang hoạt động",
        nganh: "Khoa CNTT",
        monPhuTrach: "Lập trình Web",
        soLopDangDay: 1,
        avatar: "/avatar-user-asian-male.jpg"
    }
];
const lichDayData = [
    {
        id: 1,
        monHoc: "Database",
        lop: "CNTT-K13",
        ngayDay: "Thứ 2",
        thoiGian: "13:00 - 16:30",
        phongHoc: "P.403 cơ sở 2, HCM",
        giangVien: "Nguyễn Hải Trường",
        thoiLuong: 5,
        ngay: "2025-11-17",
        gioBatDau: "13:00",
        gioKetThuc: "16:30",
        giangVienId: "00001"
    },
    {
        id: 2,
        monHoc: "Lập trình Web",
        lop: "CNTT-K13",
        ngayDay: "Thứ 3",
        thoiGian: "08:00 - 11:30",
        phongHoc: "P.201 cơ sở 1, HCM",
        giangVien: "Nguyễn Hải Trường",
        thoiLuong: 4,
        ngay: "2025-11-18",
        gioBatDau: "08:00",
        gioKetThuc: "11:30",
        giangVienId: "00001"
    },
    {
        id: 3,
        monHoc: "Database",
        lop: "CNTT-K14",
        ngayDay: "Thứ 4",
        thoiGian: "14:00 - 17:30",
        phongHoc: "P.301 cơ sở 2, HCM",
        giangVien: "Trần Văn B",
        thoiLuong: 4,
        ngay: "2025-11-19",
        gioBatDau: "14:00",
        gioKetThuc: "17:30",
        giangVienId: "00002"
    },
    {
        id: 4,
        monHoc: "Lập trình Mobile",
        lop: "CNTT-K13",
        ngayDay: "Thứ 5",
        thoiGian: "09:00 - 12:30",
        phongHoc: "P.401 cơ sở 1, HCM",
        giangVien: "Phạm Văn D",
        thoiLuong: 4,
        ngay: "2025-11-20",
        gioBatDau: "09:00",
        gioKetThuc: "12:30",
        giangVienId: "00004"
    }
];
const nganhList = [
    "Khoa CNTT",
    "Khoa Điện",
    "Khoa Cơ Khí"
];
const monHocList = [
    "Lập trình Web",
    "Database",
    "ASP.NET",
    "Lập trình Mobile",
    "Mạng Máy Tính"
];
}),
"[project]/components/sidebar.jsx [app-ssr] (ecmascript)", ((__turbopack_context__) => {
"use strict";

__turbopack_context__.s([
    "default",
    ()=>Sidebar
]);
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/dist/server/route-modules/app-page/vendored/ssr/react-jsx-dev-runtime.js [app-ssr] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/dist/server/route-modules/app-page/vendored/ssr/react.js [app-ssr] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$client$2f$app$2d$dir$2f$link$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/dist/client/app-dir/link.js [app-ssr] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$navigation$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/navigation.js [app-ssr] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$house$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Home$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/house.js [app-ssr] (ecmascript) <export default as Home>");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$book$2d$open$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__BookOpen$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/book-open.js [app-ssr] (ecmascript) <export default as BookOpen>");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$users$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Users$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/users.js [app-ssr] (ecmascript) <export default as Users>");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$graduation$2d$cap$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__GraduationCap$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/graduation-cap.js [app-ssr] (ecmascript) <export default as GraduationCap>");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$calendar$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Calendar$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/calendar.js [app-ssr] (ecmascript) <export default as Calendar>");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$bell$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Bell$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/bell.js [app-ssr] (ecmascript) <export default as Bell>");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$chevron$2d$left$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__ChevronLeft$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/chevron-left.js [app-ssr] (ecmascript) <export default as ChevronLeft>");
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$log$2d$out$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__LogOut$3e$__ = __turbopack_context__.i("[project]/node_modules/lucide-react/dist/esm/icons/log-out.js [app-ssr] (ecmascript) <export default as LogOut>");
var __TURBOPACK__imported__module__$5b$project$5d2f$lib$2f$data$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/lib/data.js [app-ssr] (ecmascript)");
"use client";
;
;
;
;
;
;
const menuItems = [
    {
        icon: __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$house$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Home$3e$__["Home"],
        label: "Tổng quan",
        href: "/tongquan"
    },
    {
        icon: __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$book$2d$open$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__BookOpen$3e$__["BookOpen"],
        label: "Lớp học",
        href: "/lophoc"
    },
    {
        icon: __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$users$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Users$3e$__["Users"],
        label: "Học viên",
        href: "/hocvien"
    },
    {
        icon: __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$graduation$2d$cap$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__GraduationCap$3e$__["GraduationCap"],
        label: "Giảng viên",
        href: "/giangvien"
    },
    {
        icon: __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$calendar$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Calendar$3e$__["Calendar"],
        label: "Lịch dạy",
        href: "/lichday"
    },
    {
        icon: __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$bell$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__Bell$3e$__["Bell"],
        label: "Thông báo",
        href: "/thongbao"
    }
];
function Sidebar() {
    const pathname = (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$navigation$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["usePathname"])();
    const router = (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$navigation$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["useRouter"])();
    const [unreadCount, setUnreadCount] = (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["useState"])(()=>{
        if ("TURBOPACK compile-time falsy", 0) //TURBOPACK unreachable
        ;
        return __TURBOPACK__imported__module__$5b$project$5d2f$lib$2f$data$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["recentNotifications"].filter((n)=>!n.daDoc).length;
    });
    (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["useEffect"])(()=>{
        const handleNotificationsUpdate = ()=>{
            const stored = localStorage.getItem("unreadNotifications");
            setUnreadCount(stored ? parseInt(stored) : 0);
        };
        window.addEventListener("notificationsUpdated", handleNotificationsUpdate);
        return ()=>window.removeEventListener("notificationsUpdated", handleNotificationsUpdate);
    }, []);
    return /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("aside", {
        className: "w-[250px] bg-white flex flex-col border-r border-gray-100 min-h-screen",
        children: [
            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("div", {
                className: "p-4 flex items-center gap-2",
                children: [
                    /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("div", {
                        className: "w-10 h-10 flex items-center justify-center",
                        children: /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("svg", {
                            viewBox: "0 0 40 40",
                            className: "w-10 h-10",
                            children: [
                                /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("circle", {
                                    cx: "20",
                                    cy: "20",
                                    r: "18",
                                    fill: "#4A90D9"
                                }, void 0, false, {
                                    fileName: "[project]/components/sidebar.jsx",
                                    lineNumber: 44,
                                    columnNumber: 13
                                }, this),
                                /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("path", {
                                    d: "M12 20 Q20 10 28 20 Q20 30 12 20",
                                    fill: "white"
                                }, void 0, false, {
                                    fileName: "[project]/components/sidebar.jsx",
                                    lineNumber: 45,
                                    columnNumber: 13
                                }, this),
                                /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("circle", {
                                    cx: "24",
                                    cy: "18",
                                    r: "3",
                                    fill: "#FF6B6B"
                                }, void 0, false, {
                                    fileName: "[project]/components/sidebar.jsx",
                                    lineNumber: 46,
                                    columnNumber: 13
                                }, this)
                            ]
                        }, void 0, true, {
                            fileName: "[project]/components/sidebar.jsx",
                            lineNumber: 43,
                            columnNumber: 11
                        }, this)
                    }, void 0, false, {
                        fileName: "[project]/components/sidebar.jsx",
                        lineNumber: 42,
                        columnNumber: 9
                    }, this),
                    /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("span", {
                        className: "text-[#4A90D9] font-semibold text-lg",
                        children: "Sea Dragon LMS"
                    }, void 0, false, {
                        fileName: "[project]/components/sidebar.jsx",
                        lineNumber: 49,
                        columnNumber: 9
                    }, this),
                    /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])(__TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$chevron$2d$left$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__ChevronLeft$3e$__["ChevronLeft"], {
                        className: "w-5 h-5 text-[#FF6B6B] ml-auto"
                    }, void 0, false, {
                        fileName: "[project]/components/sidebar.jsx",
                        lineNumber: 50,
                        columnNumber: 9
                    }, this)
                ]
            }, void 0, true, {
                fileName: "[project]/components/sidebar.jsx",
                lineNumber: 41,
                columnNumber: 7
            }, this),
            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("nav", {
                className: "flex-1 px-3 py-2",
                children: menuItems.map((item, index)=>{
                    const isActive = pathname === item.href || pathname === "/" && item.href === "/tongquan";
                    return /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])(__TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$client$2f$app$2d$dir$2f$link$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["default"], {
                        href: item.href,
                        className: `flex items-center gap-3 px-4 py-3 rounded-lg mb-1 ${isActive ? "bg-[#E8F4FD] text-[#4A90D9]" : "text-gray-600 hover:bg-gray-50"}`,
                        children: [
                            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])(item.icon, {
                                className: "w-5 h-5"
                            }, void 0, false, {
                                fileName: "[project]/components/sidebar.jsx",
                                lineNumber: 64,
                                columnNumber: 15
                            }, this),
                            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("span", {
                                className: "text-sm font-medium",
                                children: item.label
                            }, void 0, false, {
                                fileName: "[project]/components/sidebar.jsx",
                                lineNumber: 65,
                                columnNumber: 15
                            }, this),
                            item.href === "/thongbao" && unreadCount > 0 && /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("span", {
                                className: "ml-auto bg-[#4A90D9] text-white text-xs w-5 h-5 rounded-full flex items-center justify-center",
                                children: unreadCount
                            }, void 0, false, {
                                fileName: "[project]/components/sidebar.jsx",
                                lineNumber: 67,
                                columnNumber: 17
                            }, this)
                        ]
                    }, index, true, {
                        fileName: "[project]/components/sidebar.jsx",
                        lineNumber: 57,
                        columnNumber: 13
                    }, this);
                })
            }, void 0, false, {
                fileName: "[project]/components/sidebar.jsx",
                lineNumber: 53,
                columnNumber: 7
            }, this),
            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("div", {
                className: "p-4 border-t border-gray-100",
                children: [
                    /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("div", {
                        className: "flex items-center gap-3 mb-3",
                        children: /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("button", {
                            onClick: ()=>router.push("/ho-so-truong-khoa"),
                            className: "flex items-center gap-3 hover:opacity-80 transition-opacity",
                            children: [
                                /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("img", {
                                    src: "/avatar-user-asian-male.jpg",
                                    alt: "Avatar",
                                    className: "w-10 h-10 rounded-full object-cover cursor-pointer"
                                }, void 0, false, {
                                    fileName: "[project]/components/sidebar.jsx",
                                    lineNumber: 82,
                                    columnNumber: 13
                                }, this),
                                /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("span", {
                                    className: "text-sm font-medium text-gray-700",
                                    children: "Xin chào, Nguyễn Văn A"
                                }, void 0, false, {
                                    fileName: "[project]/components/sidebar.jsx",
                                    lineNumber: 83,
                                    columnNumber: 13
                                }, this)
                            ]
                        }, void 0, true, {
                            fileName: "[project]/components/sidebar.jsx",
                            lineNumber: 78,
                            columnNumber: 11
                        }, this)
                    }, void 0, false, {
                        fileName: "[project]/components/sidebar.jsx",
                        lineNumber: 77,
                        columnNumber: 9
                    }, this),
                    /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])("button", {
                        onClick: ()=>router.push("/login"),
                        className: "w-full bg-[#F5A623] text-white py-2.5 rounded-lg flex items-center justify-center gap-2 text-sm font-medium hover:bg-[#e09620] transition-colors",
                        children: [
                            /*#__PURE__*/ (0, __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$dist$2f$server$2f$route$2d$modules$2f$app$2d$page$2f$vendored$2f$ssr$2f$react$2d$jsx$2d$dev$2d$runtime$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__["jsxDEV"])(__TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$lucide$2d$react$2f$dist$2f$esm$2f$icons$2f$log$2d$out$2e$js__$5b$app$2d$ssr$5d$__$28$ecmascript$29$__$3c$export__default__as__LogOut$3e$__["LogOut"], {
                                className: "w-4 h-4"
                            }, void 0, false, {
                                fileName: "[project]/components/sidebar.jsx",
                                lineNumber: 90,
                                columnNumber: 11
                            }, this),
                            "Đăng xuất"
                        ]
                    }, void 0, true, {
                        fileName: "[project]/components/sidebar.jsx",
                        lineNumber: 86,
                        columnNumber: 9
                    }, this)
                ]
            }, void 0, true, {
                fileName: "[project]/components/sidebar.jsx",
                lineNumber: 76,
                columnNumber: 7
            }, this)
        ]
    }, void 0, true, {
        fileName: "[project]/components/sidebar.jsx",
        lineNumber: 40,
        columnNumber: 5
    }, this);
}
}),
"[externals]/next/dist/server/app-render/after-task-async-storage.external.js [external] (next/dist/server/app-render/after-task-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/after-task-async-storage.external.js", () => require("next/dist/server/app-render/after-task-async-storage.external.js"));

module.exports = mod;
}),
"[externals]/next/dist/server/app-render/dynamic-access-async-storage.external.js [external] (next/dist/server/app-render/dynamic-access-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/dynamic-access-async-storage.external.js", () => require("next/dist/server/app-render/dynamic-access-async-storage.external.js"));

module.exports = mod;
}),
];

//# sourceMappingURL=%5Broot-of-the-server%5D__98de5f59._.js.map