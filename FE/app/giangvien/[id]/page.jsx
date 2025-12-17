"use client"

import { useParams, useRouter } from "next/navigation"
import { ChevronLeft } from "lucide-react"
import { giangVienData } from "@/lib/data"

export default function ChiTietGiangVienPage() {
  const params = useParams()
  const router = useRouter()
  const giangVienId = params.id

  const giangVien = giangVienData.find((gv) => gv.id === giangVienId)

  if (!giangVien) {
    return (
      <main className="flex-1 p-6">
        <div className="flex items-center gap-4 mb-6">
          <button onClick={() => router.push("/giangvien")} className="text-gray-500 hover:text-gray-700">
            <ChevronLeft className="w-6 h-6" />
          </button>
          <h1 className="text-[#4A90D9] text-2xl font-bold">Chi tiết giảng viên</h1>
        </div>
        <div className="bg-white rounded-xl p-6 shadow-sm">
          <p className="text-gray-500 text-center py-12">Không tìm thấy thông tin giảng viên.</p>
        </div>
      </main>
    )
  }

  return (
    <main className="flex-1 p-6">
      <div className="flex items-center gap-4 mb-6">
        <button onClick={() => router.push("/giangvien")} className="text-gray-500 hover:text-gray-700">
          <ChevronLeft className="w-6 h-6" />
        </button>
        <h1 className="text-[#4A90D9] text-2xl font-bold">Thông Tin Giảng Viên</h1>
      </div>

      <div className="bg-white rounded-xl p-6 shadow-sm">
        <div className="flex gap-8">
          {/* Avatar */}
          <div className="flex-shrink-0">
            <img
              src={giangVien.avatar || "/avatar-user-asian-male.jpg"}
              alt={giangVien.hoTen}
              className="w-32 h-32 rounded-full object-cover border-4 border-gray-100"
            />
          </div>

          {/* Thông tin */}
          <div className="flex-1 grid grid-cols-3 gap-x-12 gap-y-4">
            <div>
              <p className="text-gray-500 text-sm mb-1">Mã Giảng Viên</p>
              <p className="text-gray-800 font-medium">{giangVien.maGiangVien}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Họ và Tên</p>
              <p className="text-gray-800 font-medium">{giangVien.hoTen}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Giới Tính</p>
              <p className="text-gray-800 font-medium">{giangVien.gioiTinh}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Ngày Sinh</p>
              <p className="text-gray-800 font-medium">{giangVien.ngaySinh}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Email</p>
              <p className="text-gray-800 font-medium">{giangVien.email}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Số Điện Thoại</p>
              <p className="text-gray-800 font-medium">{giangVien.soDienThoai}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Học Vị/Chức Danh</p>
              <p className="text-gray-800 font-medium">{giangVien.hocVi}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Chức Vụ</p>
              <p className="text-gray-800 font-medium">{giangVien.chucVu}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Khoa</p>
              <p className="text-gray-800 font-medium">{giangVien.khoa}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Chuyên Ngành</p>
              <p className="text-gray-800 font-medium">{giangVien.chuyenNganh}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Địa Chỉ</p>
              <p className="text-gray-800 font-medium">{giangVien.diaChi}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Tổng Số Lớp Giảng Dạy</p>
              <p className="text-gray-800 font-medium">{giangVien.tongSoLop}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Ngày Gia Nhập</p>
              <p className="text-gray-800 font-medium">{giangVien.ngayGiaNhap}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Trạng Thái</p>
              <span
                className={`inline-block px-3 py-1 rounded-full text-xs font-medium ${
                  giangVien.trangThai === "Đang hoạt động"
                    ? "bg-green-100 text-green-600"
                    : "bg-red-100 text-red-600"
                }`}
              >
                {giangVien.trangThai}
              </span>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}

