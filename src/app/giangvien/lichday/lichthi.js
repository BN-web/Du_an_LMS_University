"use client"

import { useState } from "react"

import { Calendar, Clock, MapPin, Users } from "lucide-react"

export default function LichThi() {
  const [activeTab, setActiveTab] = useState("lichthi")

  const examData = [
    {
      id: 1,
      subject: "Lập trình web",
      code: "IT1406",
      date: "Thứ năm, 20/12/2025",
      time: "8:00 - 10:30",
      location: "Tòa nhà 1,cơ số 2,phòng 104",
      students: 45,
      instructor: "Giảng viên A",
    },
    {
      id: 2,
      subject: "Lập trình web",
      code: "IT1406",
      date: "Thứ năm, 20/12/2025",
      time: "8:00 - 10:30",
      location: "Tòa nhà 1,cơ số 2,phòng 104",
      students: 45,
      instructor: "Giảng viên A",
    },
    {
      id: 3,
      subject: "Lập trình web",
      code: "IT1406",
      date: "Thứ năm, 20/12/2025",
      time: "8:00 - 10:30",
      location: "Tòa nhà 1,cơ số 2,phòng 104",
      students: 45,
      instructor: "Giảng viên A",
    },
  ]

  return (
    <div className="min-h-screen bg-white p-6">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Lịch thi</h1>

        {/* Tab Navigation */}
        <div className="flex gap-4 mb-6">
          <button
            onClick={() => setActiveTab("thoikhoabieu")}
            className={`px-6 py-2 rounded-lg font-medium transition-colors ${
              activeTab === "thoikhoabieu" ? "bg-gray-400 text-white" : "bg-white text-gray-700 hover:bg-gray-100"
            }`}
          >
            Thời khóa biểu
          </button>
          <button
            onClick={() => setActiveTab("lichthi")}
            className={`px-6 py-2 rounded-lg font-medium transition-colors ${
              activeTab === "lichthi" ? "bg-gray-400 text-white" : "bg-white text-gray-700 hover:bg-gray-100"
            }`}
          >
            Lịch Thi
          </button>
        </div>

        {/* Exam Schedule Content */}
        {activeTab === "lichthi" && (
          <div className="space-y-4">
            {examData.map((exam) => (
              <div key={exam.id} className="bg-white rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow border border-gray-200">
                {/* Header with subject and buttons */}
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">{exam.subject}</h3>
                    <p className="text-sm text-gray-600">{exam.code}</p>
                  </div>
                  <div className="flex gap-2">
                    <button className="px-4 py-1 bg-blue-500 text-white text-sm font-medium rounded hover:bg-blue-600 transition-colors">
                      Giấu kỳ
                    </button>
                    <button className="px-4 py-1 bg-blue-900 text-white text-sm font-medium rounded hover:bg-blue-800 transition-colors">
                      {exam.instructor}
                    </button>
                  </div>
                </div>

                {/* Exam Details Grid */}
                <div className="grid grid-cols-2 gap-6">
                  {/* Left column */}
                  <div className="space-y-4">
                    {/* Exam Date */}
                    <div className="flex gap-3">
                      <Calendar className="w-5 h-5 text-gray-600 flex-shrink-0 mt-1" />
                      <div>
                        <p className="text-sm font-medium text-gray-600">Ngày thi</p>
                        <p className="text-gray-900">{exam.date}</p>
                      </div>
                    </div>

                    {/* Location */}
                    <div className="flex gap-3">
                      <MapPin className="w-5 h-5 text-gray-600 flex-shrink-0 mt-1" />
                      <div>
                        <p className="text-sm font-medium text-gray-600">Địa điểm</p>
                        <p className="text-gray-900">{exam.location}</p>
                      </div>
                    </div>
                  </div>

                  {/* Right column */}
                  <div className="space-y-4">
                    {/* Exam Time */}
                    <div className="flex gap-3">
                      <Clock className="w-5 h-5 text-gray-600 flex-shrink-0 mt-1" />
                      <div>
                        <p className="text-sm font-medium text-gray-600">Thời gian</p>
                        <p className="text-gray-900">{exam.time}</p>
                      </div>
                    </div>

                    {/* Total Students */}
                    <div className="flex gap-3">
                      <Users className="w-5 h-5 text-gray-600 flex-shrink-0 mt-1" />
                      <div>
                        <p className="text-sm font-medium text-gray-600">Tổng sinh viên</p>
                        <p className="text-gray-900">{exam.students}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Timetable Tab Content */}
        {activeTab === "thoikhoabieu" && (
          <div className="bg-white rounded-lg p-6 text-center text-gray-600 border border-gray-200">
            Thời khóa biểu sẽ được hiển thị tại đây
          </div>
        )}
      </div>
    </div>
  )
}

