"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { ChevronLeft, Loader2, AlertCircle } from "lucide-react"
import { apiClient } from "@/lib/api"

export default function HoSoTruongKhoaPage() {
  const router = useRouter()
  const [hoSoData, setHoSoData] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetchHoSo = async () => {
      try {
        setIsLoading(true)
        setError(null)
        const response = await apiClient.get("/api/truong-khoa/ho-so")
        
        if (response.success && response.data) {
          // Log để debug
          console.log("Ho so data:", response.data)
          console.log("Avatar URL:", response.data.avatar)
          setHoSoData(response.data)
        } else {
          setError(response.message || "Không thể tải hồ sơ")
        }
      } catch (err) {
        console.error("Lỗi khi tải hồ sơ:", err)
        setError(err.message || "Không thể tải hồ sơ trưởng khoa")
      } finally {
        setIsLoading(false)
      }
    }

    fetchHoSo()
  }, [])

  // Hiển thị loading state
  if (isLoading) {
    return (
      <main className="flex-1 p-6">
        <div className="flex items-center gap-4 mb-6">
          <button onClick={() => router.back()} className="text-gray-500 hover:text-gray-700">
            <ChevronLeft className="w-6 h-6" />
          </button>
          <h1 className="text-[#4A90D9] text-2xl font-bold">Hồ Sơ Trưởng Khoa</h1>
        </div>
        <div className="bg-white rounded-xl p-6 shadow-sm flex items-center justify-center min-h-[400px]">
          <div className="flex flex-col items-center gap-4">
            <Loader2 className="w-8 h-8 animate-spin text-[#4A90D9]" />
            <p className="text-gray-600">Đang tải hồ sơ...</p>
          </div>
        </div>
      </main>
    )
  }

  // Hiển thị error state
  if (error || !hoSoData) {
    return (
      <main className="flex-1 p-6">
        <div className="flex items-center gap-4 mb-6">
          <button onClick={() => router.back()} className="text-gray-500 hover:text-gray-700">
            <ChevronLeft className="w-6 h-6" />
          </button>
          <h1 className="text-[#4A90D9] text-2xl font-bold">Hồ Sơ Trưởng Khoa</h1>
        </div>
        <div className="bg-white rounded-xl p-6 shadow-sm">
          <div className="flex items-center gap-3 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <div>
              <p className="font-medium">Lỗi khi tải hồ sơ</p>
              <p className="text-sm">{error || "Không thể tải hồ sơ trưởng khoa"}</p>
            </div>
          </div>
        </div>
      </main>
    )
  }

  // Xác định màu badge trạng thái
  const getTrangThaiBadgeClass = (trangThai) => {
    if (!trangThai) return "bg-gray-100 text-gray-600"
    
    const trangThaiLower = trangThai.toLowerCase()
    if (trangThaiLower.includes("hoạt động") || trangThaiLower.includes("đang")) {
      return "bg-green-100 text-green-600"
    } else if (trangThaiLower.includes("khóa") || trangThaiLower.includes("tạm")) {
      return "bg-yellow-100 text-yellow-600"
    } else if (trangThaiLower.includes("nghỉ")) {
      return "bg-red-100 text-red-600"
    }
    return "bg-gray-100 text-gray-600"
  }

  return (
    <main className="flex-1 p-6">
      <div className="flex items-center gap-4 mb-6">
        <button onClick={() => router.back()} className="text-gray-500 hover:text-gray-700">
          <ChevronLeft className="w-6 h-6" />
        </button>
        <h1 className="text-[#4A90D9] text-2xl font-bold">Hồ Sơ Trưởng Khoa</h1>
      </div>

      <div className="bg-white rounded-xl p-6 shadow-sm">
        <div className="flex gap-8">
          {/* Avatar */}
          <div className="flex-shrink-0 relative">
            {hoSoData.avatar ? (
              <img
                key={hoSoData.avatar} // Key để force re-render khi avatar thay đổi
                src={hoSoData.avatar}
                alt={hoSoData.hoTen || "Trưởng khoa"}
                className="w-32 h-32 rounded-full object-cover border-4 border-gray-100"
                onError={(e) => {
                  // Nếu ảnh từ Google không load được, ẩn img và hiển thị placeholder
                  e.target.style.display = "none"
                  const placeholder = e.target.parentElement?.querySelector(".avatar-placeholder")
                  if (placeholder) {
                    placeholder.classList.remove("hidden")
                  }
                }}
              />
            ) : null}
            {/* Placeholder avatar nếu không có ảnh hoặc ảnh lỗi */}
            <div 
              className={`w-32 h-32 rounded-full border-4 border-gray-100 bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-4xl font-bold avatar-placeholder ${hoSoData.avatar ? "hidden" : ""}`}
            >
              {hoSoData.hoTen ? hoSoData.hoTen.charAt(0).toUpperCase() : "T"}
            </div>
          </div>

          {/* Thông tin */}
          <div className="flex-1 grid grid-cols-3 gap-x-12 gap-y-4">
            <div>
              <p className="text-gray-500 text-sm mb-1">Mã Trưởng Khoa</p>
              <p className="text-gray-800 font-medium">{hoSoData.maGiangVien || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Họ và Tên</p>
              <p className="text-gray-800 font-medium">{hoSoData.hoTen || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Giới Tính</p>
              <p className="text-gray-800 font-medium">{hoSoData.gioiTinh || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Ngày Sinh</p>
              <p className="text-gray-800 font-medium">{hoSoData.ngaySinh || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Email</p>
              <p className="text-gray-800 font-medium">{hoSoData.email || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Số Điện Thoại</p>
              <p className="text-gray-800 font-medium">{hoSoData.soDienThoai || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Học Vị/Chức Danh</p>
              <p className="text-gray-800 font-medium">{hoSoData.hocVi || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Chức Vụ</p>
              <p className="text-gray-800 font-medium">{hoSoData.chucVu || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Khoa</p>
              <p className="text-gray-800 font-medium">{hoSoData.tenKhoa || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Chuyên Ngành</p>
              <p className="text-gray-800 font-medium">{hoSoData.chuyenMon || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Địa Chỉ</p>
              <p className="text-gray-800 font-medium">{hoSoData.diaChi || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Tổng Số Lớp Giảng Dạy</p>
              <p className="text-gray-800 font-medium">{hoSoData.tongSoLopGiangDay ?? 0}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Ngày Gia Nhập</p>
              <p className="text-gray-800 font-medium">{hoSoData.ngayGiaNhap || "N/A"}</p>
            </div>
            <div>
              <p className="text-gray-500 text-sm mb-1">Trạng Thái</p>
              <span className={`inline-block px-3 py-1 rounded-full text-xs font-medium ${getTrangThaiBadgeClass(hoSoData.trangThai)}`}>
                {hoSoData.trangThai || "N/A"}
              </span>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}

