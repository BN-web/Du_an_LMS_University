"use client"

import { ChevronLeft } from "lucide-react"

export default function XemChiTietSV({ student, classData, onBack }) {
  return (
    <div className="flex-1 overflow-auto bg-gray-50 min-h-screen">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-8 py-6 flex items-center gap-4">
        {onBack && (
          <button onClick={onBack} className="p-1 hover:bg-gray-100 rounded transition-colors">
            <ChevronLeft size={24} className="text-gray-800" />
          </button>
        )}
        <h1 className="text-3xl font-bold text-gray-900">Lớp học</h1>
      </div>

      {/* Content */}
      <div className="p-8">
        <div className="max-w-6xl mx-auto">
          {/* Statistics Cards */}
          <div className="grid grid-cols-3 gap-6 mb-8">
            {/* Total Sessions */}
            <div className="bg-white p-6 rounded-lg shadow-sm border-l-4 border-blue-500">
              <p className="text-gray-600 text-sm mb-2">Tổng số buổi</p>
              <p className="text-4xl font-bold text-gray-900">8</p>
            </div>

            {/* Present */}
            <div className="bg-white p-6 rounded-lg shadow-sm border-l-4 border-green-500">
              <p className="text-gray-600 text-sm mb-2">Có mặt</p>
              <p className="text-4xl font-bold text-gray-900">6</p>
            </div>

            {/* Absent */}
            <div className="bg-white p-6 rounded-lg shadow-sm border-l-4 border-red-500">
              <p className="text-gray-600 text-sm mb-2">Vắng</p>
              <p className="text-4xl font-bold text-gray-900">1</p>
            </div>
          </div>

          {/* Main Content - Two Columns: Left (Attendance) and Right (Student + Results) */}
          <div className="grid grid-cols-3 gap-8">
            {/* Left Column - Attendance History (spans full height) */}
            <div className="bg-white rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-bold text-gray-900 mb-4">Lịch sử điểm danh</h2>

              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-2 px-2 font-semibold text-gray-700 text-xs">Buổi</th>
                      <th className="text-left py-2 px-2 font-semibold text-gray-700 text-xs">Ngày</th>
                      <th className="text-left py-2 px-2 font-semibold text-gray-700 text-xs">Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 1</td>
                      <td className="py-2 px-2 text-gray-700">15/09/2025</td>
                      <td className="py-2 px-2">
                        <span className="inline-block bg-green-100 text-green-800 px-2 py-0.5 rounded text-xs font-medium">
                          Có mặt
                        </span>
                      </td>
                    </tr>
                    <tr className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 2</td>
                      <td className="py-2 px-2 text-gray-700">22/09/2025</td>
                      <td className="py-2 px-2">
                        <span className="inline-block bg-green-100 text-green-800 px-2 py-0.5 rounded text-xs font-medium">
                          Có mặt
                        </span>
                      </td>
                    </tr>
                    <tr className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 3</td>
                      <td className="py-2 px-2 text-gray-700">22/09/2025</td>
                      <td className="py-2 px-2">
                        <span className="inline-block bg-red-100 text-red-800 px-2 py-0.5 rounded text-xs font-medium">
                          Vắng
                        </span>
                      </td>
                    </tr>
                    <tr className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 4</td>
                      <td className="py-2 px-2 text-gray-700">22/09/2025</td>
                      <td className="py-2 px-2">
                        <span className="inline-block bg-green-100 text-green-800 px-2 py-0.5 rounded text-xs font-medium">
                          Có mặt
                        </span>
                      </td>
                    </tr>
                    <tr className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 5</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                    </tr>
                    <tr className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 6</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                    </tr>
                    <tr className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 7</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                    </tr>
                    <tr className="hover:bg-gray-50">
                      <td className="py-2 px-2 text-gray-700">Buổi 8</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                      <td className="py-2 px-2 text-gray-500">---</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            {/* Right Column - Student Details and Academic Results */}
            <div className="col-span-2 space-y-8">
              {/* Student Details Card */}
              <div className="bg-white rounded-lg shadow-sm p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-4">Chi tiết sinh viên</h2>

                <div className="flex gap-6">
                  <div className="flex-shrink-0">
                    <div className="w-24 h-24 rounded-full bg-gray-300 overflow-hidden">
                      {student?.avatar ? (
                        <img src={student.avatar} alt="Student avatar" className="w-full h-full object-cover" />
                      ) : (
                        <div className="w-full h-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-2xl font-bold">
                          {student?.name ? student.name.charAt(0).toUpperCase() : "S"}
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="flex-1 grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-gray-600 text-xs font-medium mb-1">Họ và tên</p>
                      <p className="text-gray-900 font-semibold">{student?.name || "Lê Minh Anh"}</p>
                    </div>

                    <div>
                      <p className="text-gray-600 text-xs font-medium mb-1">Mã sinh viên</p>
                      <p className="text-gray-900 font-semibold">{student?.mssv || "B20DCCN001"}</p>
                    </div>

                    <div>
                      <p className="text-gray-600 text-xs font-medium mb-1">Ngành</p>
                      <p className="text-gray-900 font-semibold">{student?.major || "Công nghệ phần mềm"}</p>
                    </div>

                    <div>
                      <p className="text-gray-600 text-xs font-medium mb-1">Khoa</p>
                      <p className="text-gray-900 font-semibold">Công nghệ thông tin</p>
                    </div>

                    <div className="col-span-2">
                      <p className="text-gray-600 text-xs font-medium mb-1">Email</p>
                      <p className="text-gray-900 font-semibold">{student?.email || "minhanh.le@example.edu"}</p>
                    </div>

                    <div className="col-span-2">
                      <p className="text-gray-600 text-xs font-medium mb-1">Số điện thoại</p>
                      <p className="text-gray-900 font-semibold">0912345678</p>
                    </div>

                    <div className="col-span-2">
                      <p className="text-gray-600 text-xs font-medium mb-1">Ngày sinh</p>
                      <p className="text-gray-900 font-semibold">10/05/2002</p>
                    </div>

                    <div className="col-span-2">
                      <p className="text-gray-600 text-xs font-medium mb-1">Địa chỉ</p>
                      <p className="text-gray-900 font-semibold">144 Xuân Thủy, Cầu Giấy, Hà Nội</p>
                    </div>
                  </div>
                </div>
              </div>

              {/* Academic Results */}
              <div className="bg-white rounded-lg shadow-sm p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-4">Kết quả học tập</h2>

                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-gray-200">
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">MÔN HỌC</th>
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">BÀI TẬP</th>
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">GIỮA KỲ</th>
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">CUỐI KỲ</th>
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">CHUYÊN CẦN</th>
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">TB MÔN</th>
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">ĐIỂM CHỮ</th>
                        <th className="text-left py-3 px-4 font-semibold text-gray-700 text-xs">GPA MÔN</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr className="border-b border-gray-100 hover:bg-gray-50">
                        <td className="py-3 px-4 text-gray-900 font-medium">Lập trình Web</td>
                        <td className="py-3 px-4 text-gray-700">8.5</td>
                        <td className="py-3 px-4 text-gray-700">9.0</td>
                        <td className="py-3 px-4 text-gray-700">9.5</td>
                        <td className="py-3 px-4 text-gray-700">100%</td>
                        <td className="py-3 px-4 text-green-600 font-semibold">9.2</td>
                        <td className="py-3 px-4 text-gray-700">A+</td>
                        <td className="py-3 px-4 text-green-600 font-semibold">4.0</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
