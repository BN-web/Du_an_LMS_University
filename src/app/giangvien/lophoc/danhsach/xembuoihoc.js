"use client"

import { ChevronLeft, Calendar, Clock } from "lucide-react"

export default function XemBuoiHoc({ classData, onBack, onDiemDanhNgay, onXemDiemDanh }) {
  // Dữ liệu mẫu các buổi học
  const sessions = [
    {
      id: 1,
      number: 1,
      date: "2024-09-02",
      dayOfWeek: "Thứ 2",
      startTime: "07:00",
      endTime: "09:00",
      status: "completed", // completed, current, upcoming
    },
    {
      id: 2,
      number: 2,
      date: "2024-09-04",
      dayOfWeek: "Thứ 4",
      startTime: "07:00",
      endTime: "09:00",
      status: "completed",
    },
    {
      id: 3,
      number: 3,
      date: "2024-11-25",
      dayOfWeek: "Thứ 2",
      startTime: "07:00",
      endTime: "09:00",
      status: "current", // Buổi hiện tại - có nút "Điểm danh ngay"
    },
    {
      id: 4,
      number: 4,
      date: "2024-11-27",
      dayOfWeek: "Thứ 4",
      startTime: "07:00",
      endTime: "09:00",
      status: "upcoming", // Buổi sắp tới - không có nút
    },
    {
      id: 5,
      number: 5,
      date: "2024-12-02",
      dayOfWeek: "Thứ 2",
      startTime: "07:00",
      endTime: "09:00",
      status: "upcoming",
    },
  ]

  const handleXemDiemDanh = (session) => {
    if (onXemDiemDanh) {
      onXemDiemDanh(session)
    }
  }

  return (
    <div className="min-h-screen bg-blue-50 p-6">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="flex items-center gap-4 mb-8">
          <button
            onClick={onBack}
            className="p-2 hover:bg-white/50 rounded-lg transition-colors"
          >
            <ChevronLeft className="w-6 h-6 text-gray-600" />
          </button>
          <h1 className="text-3xl font-bold text-gray-900">Lớp học</h1>
        </div>

        {/* Main Content */}
        <div className="bg-white rounded-2xl shadow-sm p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Danh sách buổi học</h2>

          {/* Grid of Session Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {sessions.map((session) => (
              <div
                key={session.id}
                className="bg-white border border-gray-200 rounded-xl p-4 hover:shadow-md transition-shadow"
              >
                {/* Session Number */}
                <div className="mb-4">
                  <h3 className="text-xl font-bold text-gray-900">Buổi {session.number}</h3>
                </div>

                {/* Date */}
                <div className="flex items-center gap-2 mb-3">
                  <Calendar className="w-5 h-5 text-gray-500" />
                  <span className="text-sm text-gray-700">
                    {session.dayOfWeek}, {session.date}
                  </span>
                </div>

                {/* Time */}
                <div className="flex items-center gap-2 mb-4">
                  <Clock className="w-5 h-5 text-gray-500" />
                  <span className="text-sm text-gray-700">
                    {session.startTime} - {session.endTime}
                  </span>
                </div>

                {/* Action Button */}
                {session.status === "completed" && (
                  <div className="mt-4">
                    <button
                      onClick={() => handleXemDiemDanh(session)}
                      className="w-full px-4 py-2 bg-pink-50 text-blue-600 rounded-lg font-medium hover:bg-pink-100 transition-colors text-sm flex items-center justify-center"
                    >
                      Xem điểm danh
                    </button>
                  </div>
                )}

                {session.status === "current" && (
                  <div className="mt-4">
                    <button
                      onClick={() => onDiemDanhNgay(session)}
                      className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors"
                    >
                      Điểm danh ngay
                    </button>
                  </div>
                )}

                {/* No button for upcoming sessions */}
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}

