"use client"

import { useState, useEffect } from "react"
import { ChevronLeft, Search, Calendar, Users, UserCheck, UserX, TrendingUp } from "lucide-react"

export default function XemDiemDanh({ classData, session, onBack }) {
  const [searchTerm, setSearchTerm] = useState("")
  const [attendance, setAttendance] = useState({})

  // Load dữ liệu điểm danh đã lưu từ localStorage
  useEffect(() => {
    if (typeof window !== "undefined" && session) {
      const storageKey = `attendance_${classData.code}_${session.date}_${session.number}`
      const savedData = localStorage.getItem(storageKey)
      if (savedData) {
        try {
          const parsed = JSON.parse(savedData)
          setAttendance(parsed)
        } catch (e) {
          console.error("Error parsing attendance data:", e)
          // Nếu không có dữ liệu, mặc định tất cả có mặt
          const defaultAttendance = {}
          classData.students.forEach((student) => {
            defaultAttendance[student.mssv] = true
          })
          setAttendance(defaultAttendance)
        }
      } else {
        // Nếu không có dữ liệu, mặc định tất cả có mặt
        const defaultAttendance = {}
        classData.students.forEach((student) => {
          defaultAttendance[student.mssv] = true
        })
        setAttendance(defaultAttendance)
      }
    }
  }, [classData, session])

  const presentCount = Object.values(attendance).filter((v) => v === true).length
  const absentCount = Object.values(attendance).filter((v) => v === false).length
  const totalCount = Object.keys(attendance).length || classData.students.length
  const attendanceRate = totalCount > 0 ? ((presentCount / totalCount) * 100).toFixed(0) : "0"

  const filteredStudents = classData.students.filter(
    (student) =>
      student.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      student.mssv.toLowerCase().includes(searchTerm.toLowerCase()) ||
      student.email.toLowerCase().includes(searchTerm.toLowerCase()),
  )

  // Format date từ session
  const formatSessionDate = (session) => {
    if (!session) return ""
    return `Buổi ${session.number} - ${session.date}`
  }

  return (
    <div className="flex-1 overflow-auto bg-white min-h-screen">
      {/* Header */}
      <div className="bg-blue-100 border-b border-blue-200 px-8 py-6 flex items-center gap-4">
        <button onClick={onBack} className="p-1 hover:bg-blue-200 rounded transition-colors">
          <ChevronLeft size={24} className="text-gray-800" />
        </button>
        <h1 className="text-3xl font-bold text-gray-800">Lớp học</h1>
      </div>

      {/* Content */}
      <div className="p-8">
        {/* Blue Header Banner */}
        <div className="bg-blue-500 rounded-lg p-6 text-white mb-6">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-2xl font-bold mb-1">Điểm danh sinh viên</h2>
              <p className="text-blue-100">Mã lớp: {classData.code}</p>
              <p className="text-blue-100">Môn: {classData.name}</p>
            </div>
            <div className="text-right flex items-center gap-2">
              <Calendar className="w-5 h-5 text-blue-100" />
              <div className="text-sm text-blue-100">{formatSessionDate(session)}</div>
            </div>
          </div>
        </div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-4 gap-4 mb-6">
          {/* Tổng số sinh viên */}
          <div className="bg-white rounded-lg p-4 border-l-4 border-purple-500 shadow-sm">
            <div className="flex items-center gap-2 mb-2">
              <Users className="w-5 h-5 text-purple-500" />
            </div>
            <p className="text-sm text-gray-600 mb-1">Tổng số sinh viên</p>
            <p className="text-3xl font-bold text-gray-800">{totalCount}</p>
          </div>

          {/* Có mặt */}
          <div className="bg-white rounded-lg p-4 border-l-4 border-green-500 shadow-sm">
            <div className="flex items-center gap-2 mb-2">
              <UserCheck className="w-5 h-5 text-green-500" />
            </div>
            <p className="text-sm text-gray-600 mb-1">Có mặt</p>
            <p className="text-3xl font-bold text-gray-800">{presentCount}</p>
          </div>

          {/* Vắng mặt */}
          <div className="bg-white rounded-lg p-4 border-l-4 border-red-500 shadow-sm">
            <div className="flex items-center gap-2 mb-2">
              <UserX className="w-5 h-5 text-red-500" />
            </div>
            <p className="text-sm text-gray-600 mb-1">Vắng mặt</p>
            <p className="text-3xl font-bold text-gray-800">{absentCount}</p>
          </div>

          {/* Tỉ lệ */}
          <div className="bg-white rounded-lg p-4 border-l-4 border-purple-500 shadow-sm">
            <div className="flex items-center gap-2 mb-2">
              <TrendingUp className="w-5 h-5 text-purple-500" />
            </div>
            <p className="text-sm text-gray-600 mb-1">Tỉ lệ</p>
            <p className="text-3xl font-bold text-gray-800">{attendanceRate}%</p>
          </div>
        </div>

        {/* Search Bar */}
        <div className="mb-6">
          <div className="relative max-w-md">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
            <input
              type="text"
              placeholder="Tìm kiếm theo tên, MSSV, email....."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 rounded-lg bg-gray-200 text-gray-700 placeholder-gray-500 focus:outline-none"
            />
          </div>
        </div>

        {/* Attendance Table */}
        <div className="bg-white rounded-lg overflow-hidden shadow-sm">
          <table className="w-full">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">STT</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">MSSV</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">HỌ VÀ TÊN</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Gmail</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">TRẠNG THÁI</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">ĐIỂM DANH</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {filteredStudents.map((student, idx) => {
                const isPresent = attendance[student.mssv] !== undefined ? attendance[student.mssv] : true
                return (
                  <tr key={student.mssv} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 text-sm text-gray-600">{idx + 1}</td>
                    <td className="px-6 py-4 text-sm text-gray-600">{student.mssv}</td>
                    <td className="px-6 py-4 text-sm text-gray-800 font-medium">{student.name}</td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      <a href={`mailto:${student.email}`} className="text-blue-600 hover:underline">
                        {student.email}
                      </a>
                    </td>
                    <td className="px-6 py-4 text-sm">
                      <span
                        className={`px-3 py-1 rounded-full text-sm font-medium ${
                          isPresent ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"
                        }`}
                      >
                        {isPresent ? "Có mặt" : "Vắng"}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm">
                      {/* Toggle Switch - Disabled (Read-only) */}
                      <button
                        disabled
                        className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors cursor-not-allowed ${
                          isPresent ? "bg-blue-500" : "bg-gray-300"
                        }`}
                      >
                        <span
                          className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                            isPresent ? "translate-x-6" : "translate-x-1"
                          }`}
                        />
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

