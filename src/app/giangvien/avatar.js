"use client"

import { ArrowLeft } from "lucide-react"

export default function Avatar({ onBack }) {
  const instructor = {
    id: "00001",
    name: "Nguyễn Hải Trường",
    dob: "15/08/1982",
    gender: "Nam",
    email: "truong.nh@example.edu",
    phone: "0987654321",
    degree: "Tiến sĩ",
    position: "Giảng viên",
    faculty: "Khoa Học Máy Tính",
    specialization: "Trí Tuệ Nhân Tạo & Học Máy",
    address: "123 Example St, District 1, Ho Chi Minh City",
    totalClasses: "42",
    joinDate: "20/08/2010",
    avatar: "https://static.topcv.vn/company_logos/U1iwaehTitGAEylND0btRiLpHsuAtYyU_1719220999____94320d1131518f152cb3d6cc9617fbe5.png",
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      {/* Header with Back Button */}
      <div className="mb-8">
        <button
          onClick={onBack}
          className="flex items-center gap-2 text-gray-700 hover:text-gray-900 mb-6 transition-colors"
        >
          <ArrowLeft size={24} />
        </button>
        <h1 className="text-3xl font-bold text-gray-900">Hồ Sơ Giảng Viên</h1>
      </div>

      {/* Profile Card */}
      <div className="bg-white rounded-lg shadow-md p-8">
        <div className="flex gap-12">
          {/* Avatar Section */}
          <div className="flex-shrink-0">
            <div className="w-48 h-48 rounded-full overflow-hidden border-4 border-gray-200 shadow-lg">
              <img
                src={instructor.avatar || "/placeholder.svg"}
                alt={instructor.name}
                className="w-full h-full object-cover"
              />
            </div>
          </div>

          {/* Information Grid */}
          <div className="flex-1">
            <div className="grid grid-cols-3 gap-x-12 gap-y-8">
              {/* Column 1 */}
              <div className="space-y-6">
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Mã Giảng Viên</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.id}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Ngày Sinh</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.dob}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Học Vị/Chức Danh</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.degree}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Chuyên Ngành</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.specialization}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Địa Chỉ</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.address}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Ngày Gia Nhập</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.joinDate}</p>
                </div>
              </div>

              {/* Column 2 */}
              <div className="space-y-6">
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Họ và Tên</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.name}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Email</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.email}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Chức Vụ</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.position}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Khoa</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.faculty}</p>
                </div>
              </div>

              {/* Column 3 */}
              <div className="space-y-6">
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Giới Tính</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.gender}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Số Điện Thoại</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.phone}</p>
                </div>
                <div className="h-12" />
                <div>
                  <p className="text-sm text-gray-600 font-medium mb-1">Tổng Số Lớp Giảng Dạy</p>
                  <p className="text-lg font-semibold text-gray-900">{instructor.totalClasses}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

