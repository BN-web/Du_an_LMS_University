"use client"

import { ChevronLeft, Edit2 } from "lucide-react"

import { useState } from "react"

import ChinhSua from "./chinhsua"

export default function XemChiTiet({ student, onBack }) {
  const [visiblePassword, setVisiblePassword] = useState(false)
  const [selectedSubject, setSelectedSubject] = useState(null)
  const [showEditModal, setShowEditModal] = useState(false)

  const getFullStudentData = (student) => {
    const studentGrades = {
      20131: [
        {
          name: "Lập trình Web",
          homework: 8.5,
          midterm: 9.0,
          final: 9.5,
          participation: 10,
          total: 9.2,
          grade: "A+",
          gpa: 4.0,
        },
        {
          name: "Cơ sở dữ liệu",
          homework: 7.0,
          midterm: 8.0,
          final: 7.5,
          participation: 9,
          total: 7.6,
          grade: "B+",
          gpa: 3.5,
        },
        {
          name: "Mạng máy tính",
          homework: 9.0,
          midterm: 8.5,
          final: 9.0,
          participation: 10,
          total: 8.8,
          grade: "A",
          gpa: 3.8,
        },
        {
          name: "Trí tuệ nhân tạo",
          homework: "--",
          midterm: "--",
          final: "--",
          participation: 0,
          total: "--",
          grade: "--",
          gpa: "--",
        },
      ],
      20132: [
        {
          name: "Cơ sở dữ liệu",
          homework: 8.0,
          midterm: 8.5,
          final: 8.2,
          participation: 9,
          total: 8.3,
          grade: "A",
          gpa: 3.8,
        },
        {
          name: "Lập trình Web",
          homework: 7.5,
          midterm: 7.8,
          final: 8.0,
          participation: 8,
          total: 7.8,
          grade: "B+",
          gpa: 3.5,
        },
      ],
      20133: [
        {
          name: "Lập trình Web",
          homework: 9.0,
          midterm: 9.2,
          final: 9.5,
          participation: 10,
          total: 9.3,
          grade: "A+",
          gpa: 4.0,
        },
      ],
      20134: [
        {
          name: "Lập trình Java",
          homework: 8.0,
          midterm: 8.3,
          final: 8.5,
          participation: 9,
          total: 8.3,
          grade: "A",
          gpa: 3.8,
        },
      ],
      20135: [
        {
          name: "Mạng máy tính",
          homework: 7.5,
          midterm: 7.8,
          final: 8.0,
          participation: 8,
          total: 7.8,
          grade: "B+",
          gpa: 3.5,
        },
      ],
      20136: [
        {
          name: "Lập trình Web",
          homework: 9.2,
          midterm: 9.0,
          final: 9.3,
          participation: 10,
          total: 9.2,
          grade: "A+",
          gpa: 4.0,
        },
      ],
      20137: [
        {
          name: "Cơ sở dữ liệu",
          homework: 8.5,
          midterm: 8.8,
          final: 9.0,
          participation: 9,
          total: 8.8,
          grade: "A",
          gpa: 3.8,
        },
      ],
      20138: [
        {
          name: "Lập trình Java",
          homework: 7.0,
          midterm: 7.2,
          final: 7.5,
          participation: 7,
          total: 7.3,
          grade: "B",
          gpa: 3.0,
        },
      ],
    }

    return {
      id: student.id,
      name: student.name,
      gender: "Nam",
      birthDate: "15/08/2002",
      phone: "0987654321",
      email: `${student.name.toLowerCase().replace(/\s+/g, ".")}.nh@email.com`,
      major: student.nghe,
      khoa: student.khoa,
      gpa: "3.8 / 4.0",
      credits: "120",
      enrollDate: "20/08/2020",
      status: student.trangThai,
      avatar: "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=400&fit=crop",
      subjects: studentGrades[student.id] || [],
    }
  }

  const studentData = getFullStudentData(student)

  const handleEditClick = (subject) => {
    setSelectedSubject(subject)
    setShowEditModal(true)
  }

  const handleSaveGrades = (updatedScores) => {
    // Map data from ChinhSua format to display format
    const mappedData = {
      ...selectedSubject,
      homework: updatedScores.homework,
      midterm: updatedScores.midterm,
      final: updatedScores.final,
      participation: updatedScores.participation,
      total: updatedScores.averageScore,
      grade: updatedScores.letterGrade,
      gpa: updatedScores.gpa,
    }
    console.log("[v0] Saved grades:", mappedData)
    setShowEditModal(false)
    // Here you would typically send the data to a backend API
    // and update the subjects state to reflect the changes
  }

  return (
    <div className="min-h-screen bg-white p-6">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="flex items-center gap-4 mb-8 cursor-pointer" onClick={onBack}>
          <ChevronLeft className="w-8 h-8 text-gray-600 hover:text-gray-900" />
          <h1 className="text-3xl font-bold text-gray-900">Chi tiết sinh viên</h1>
        </div>

        {/* Student Profile Card */}
        <div className="bg-white rounded-lg shadow p-8 mb-8">
          <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
            {/* Avatar and Basic Info */}
            <div className="lg:col-span-1 flex flex-col items-center lg:items-start">
              <img
                src={studentData.avatar || "/placeholder.svg"}
                alt={studentData.name}
                className="w-32 h-32 rounded-full object-cover mb-6 shadow-md"
              />
              <div className="space-y-3 w-full">
                <div>
                  <p className="text-sm text-gray-500 font-medium">MSSV</p>
                  <p className="text-lg font-bold text-gray-900">{studentData.id}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500 font-medium">Điện thoại</p>
                  <p className="text-base text-gray-900">{studentData.phone}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500 font-medium">GPA tích lũy</p>
                  <p className="text-lg font-bold text-gray-900">{studentData.gpa}</p>
                </div>
              </div>
            </div>

            {/* Main Info Grid */}
            <div className="lg:col-span-3 grid grid-cols-2 gap-6">
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Họ tên</p>
                <p className="text-base font-semibold text-gray-900">{studentData.name}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Giới tính</p>
                <p className="text-base text-gray-900">{studentData.gender}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Email</p>
                <p className="text-base text-gray-900">{studentData.email}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Chuyên ngành</p>
                <p className="text-base text-gray-900">{studentData.major}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Ngày sinh</p>
                <p className="text-base text-gray-900">{studentData.birthDate}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Khóa</p>
                <p className="text-base text-gray-900">{studentData.khoa}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Tổng tín chỉ</p>
                <p className="text-base text-gray-900">{studentData.credits}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Trạng thái</p>
                <span
                  className={`inline-block px-3 py-1 rounded-full text-sm font-medium border min-w-[120px] text-center ${
                    studentData.status === "Đang học"
                      ? "bg-green-50 text-green-700 border-green-200"
                      : "bg-orange-50 text-orange-700 border-orange-200"
                  }`}
                >
                  {studentData.status}
                </span>
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium mb-2">Ngày tạo hồ sơ</p>
                <p className="text-base text-gray-900">{studentData.enrollDate}</p>
              </div>
            </div>
          </div>
        </div>

        {/* Grades Table */}
        <div>
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Bảng điểm môn học</h2>
          <div className="bg-white rounded-lg shadow overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-gray-100 border-b border-gray-200">
                    <th className="px-6 py-4 text-left text-sm font-semibold text-gray-900">MÔN HỌC</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">BÀI TẬP</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">GIỮA KỲ</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">CUỐI KỲ</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">CHUYÊN CẦN</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">TBM</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">ĐIỂM CHỮ</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">GPA MÔN</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold text-gray-900">THAO TÁC</th>
                  </tr>
                </thead>
                <tbody>
                  {studentData.subjects && studentData.subjects.length > 0 ? (
                    studentData.subjects.map((subject, index) => (
                      <tr key={index} className="border-b border-gray-200 hover:bg-gray-50 transition-colors">
                        <td className="px-6 py-4 text-sm text-gray-900 font-medium">{subject.name}</td>
                        <td className="px-6 py-4 text-center text-sm text-gray-700">
                          {typeof subject.homework === "number" ? subject.homework.toFixed(1) : subject.homework}
                        </td>
                        <td className="px-6 py-4 text-center text-sm text-gray-700">
                          {typeof subject.midterm === "number" ? subject.midterm.toFixed(1) : subject.midterm}
                        </td>
                        <td className="px-6 py-4 text-center text-sm text-gray-700">
                          {typeof subject.final === "number" ? subject.final.toFixed(1) : subject.final}
                        </td>
                        <td className="px-6 py-4 text-center text-sm text-gray-700">{subject.participation}</td>
                        <td className="px-6 py-4 text-center text-sm text-blue-600 font-semibold">
                          {typeof subject.total === "number" ? subject.total.toFixed(1) : subject.total}
                        </td>
                        <td className="px-6 py-4 text-center text-sm text-gray-700 font-medium">{subject.grade}</td>
                        <td className="px-6 py-4 text-center text-sm text-gray-700 font-medium">
                          {typeof subject.gpa === "number" ? subject.gpa.toFixed(1) : subject.gpa}
                        </td>
                        <td className="px-6 py-4 text-center text-sm">
                          <button
                            onClick={() => handleEditClick(subject)}
                            className="inline-block p-2 text-blue-500 hover:text-blue-700 hover:bg-blue-50 rounded transition-colors"
                          >
                            <Edit2 className="w-5 h-5" />
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan="9" className="px-6 py-8 text-center text-gray-500">
                        Không có dữ liệu điểm số
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      {showEditModal && selectedSubject && (
        <ChinhSua
          student={studentData}
          subject={selectedSubject}
          onClose={() => setShowEditModal(false)}
          onSave={handleSaveGrades}
        />
      )}
    </div>
  )
}
