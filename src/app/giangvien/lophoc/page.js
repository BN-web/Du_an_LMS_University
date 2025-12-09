"use client"

import { useState } from "react"
import { Search } from "lucide-react"
import DanhSachSV from "./danhsach/danhsach"
import XemChiTietSV from "./danhsach/xemchitiet"
import DiemDanh from "./danhsach/diemdanh"
import XemBuoiHoc from "./danhsach/xembuoihoc"
import XemDiemDanh from "./danhsach/xemdiemdanh"

export default function LoPhoc() {
  const [view, setView] = useState("list")
  const [selectedClass, setSelectedClass] = useState(null)
  const [selectedStudent, setSelectedStudent] = useState(null)
  const [selectedSession, setSelectedSession] = useState(null)
  const [searchTerm, setSearchTerm] = useState("")
  const [previousView, setPreviousView] = useState("list")

  const classes = [
    {
      id: 1,
      code: "IT1404",
      name: "Lập trình Web",
      subject: "Web Development",
      course: "K20",
      major: "CNTT",
      teacher: "Nguyễn Văn A",
      maxStudents: 45,
      status: "Hoạt động",
      students: [
        {
          mssv: "20181234",
          name: "Nguyễn Văn A",
          major: "Kỹ thuật phần mềm",
          email: "vanaVNDIT@student.edu.vn",
          grade: 8.5,
          status: "Đang học",
        },
        {
          mssv: "20181235",
          name: "Trần Thị B",
          major: "Hệ thống thông tin",
          email: "thib@student.edu.vn",
          grade: 9.2,
          status: "Đang học",
        },
        {
          mssv: "20181236",
          name: "Lê Văn C",
          major: "Kỹ thuật phần mềm",
          email: "levanc@student.edu.vn",
          grade: 7.0,
          status: "Đang học",
        },
        {
          mssv: "20181237",
          name: "Phạm Thị D",
          major: "An toàn thông tin",
          email: "phamthid@student.edu.vn",
          grade: 5.5,
          status: "Đã thôi học",
        },
        {
          mssv: "20181238",
          name: "Nguyễn Hải E",
          major: "Kỹ thuật phần mềm",
          email: "nguyenhai@student.edu.vn",
          grade: 8.2,
          status: "Đang học",
        },
        {
          mssv: "20181239",
          name: "Trần Công F",
          major: "Hệ thống thông tin",
          email: "trancong@student.edu.vn",
          grade: 9.0,
          status: "Đang học",
        },
        {
          mssv: "20181240",
          name: "Lê Minh G",
          major: "Kỹ thuật phần mềm",
          email: "leminh@student.edu.vn",
          grade: 7.8,
          status: "Đang học",
        },
        {
          mssv: "20181241",
          name: "Phạm Văn H",
          major: "An toàn thông tin",
          email: "phamvanh@student.edu.vn",
          grade: 6.5,
          status: "Đang học",
        },
        {
          mssv: "20181242",
          name: "Nguyễn Thị I",
          major: "Kỹ thuật phần mềm",
          email: "nguyenthi@student.edu.vn",
          grade: 8.7,
          status: "Đang học",
        },
        {
          mssv: "20181243",
          name: "Trần Văn J",
          major: "Hệ thống thông tin",
          email: "tranvan@student.edu.vn",
          grade: 9.3,
          status: "Đang học",
        },
      ],
    },
    {
      id: 2,
      code: "IT1405",
      name: "Lập trình Mobile",
      subject: "Mobile Development",
      course: "K20",
      major: "CNTT",
      teacher: "Nguyễn Văn B",
      maxStudents: 40,
      status: "Hoạt động",
      students: [
        {
          mssv: "20181244",
          name: "Lê Thị K",
          major: "Kỹ thuật phần mềm",
          email: "lethik@student.edu.vn",
          grade: 8.0,
          status: "Đang học",
        },
      ],
    },
    {
      id: 3,
      code: "IT1406",
      name: "Database Design",
      subject: "Database",
      course: "K20",
      major: "CNTT",
      teacher: "Nguyễn Văn C",
      maxStudents: 35,
      status: "Hoạt động",
      students: [
        {
          mssv: "20181245",
          name: "Phạm Thị L",
          major: "Hệ thống thông tin",
          email: "phamthil@student.edu.vn",
          grade: 9.1,
          status: "Đang học",
        },
      ],
    },
    {
      id: 4,
      code: "IT1407",
      name: "Cloud Computing",
      subject: "Cloud",
      course: "K20",
      major: "CNTT",
      teacher: "Nguyễn Văn D",
      maxStudents: 38,
      status: "Hoạt động",
      students: [],
    },
    {
      id: 5,
      code: "IT1408",
      name: "AI & Machine Learning",
      subject: "AI",
      course: "K20",
      major: "CNTT",
      teacher: "Nguyễn Văn E",
      maxStudents: 42,
      status: "Hoạt động",
      students: [],
    },
    {
      id: 6,
      code: "IT1409",
      name: "DevOps",
      subject: "DevOps",
      course: "K20",
      major: "CNTT",
      teacher: "Nguyễn Văn F",
      maxStudents: 30,
      status: "Hoạt động",
      students: [],
    },
  ]

  const filteredClasses = classes.filter(
    (cls) =>
      cls.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
      cls.name.toLowerCase().includes(searchTerm.toLowerCase()),
  )

  const handleClassSelect = (cls) => {
    setSelectedClass(cls)
    setView("details")
  }

  const handleDanhSachClick = () => {
    setView("danhsach")
  }

  const handleDiemDanhClick = () => {
    setPreviousView(view) // Lưu view hiện tại trước khi chuyển
    setView("sessions") // Hiện danh sách buổi học thay vì điểm danh trực tiếp
  }

  const handleDiemDanhNgay = (session) => {
    setSelectedSession(session)
    setView("attendance") // Chuyển sang trang điểm danh
  }

  const handleBackFromSessions = () => {
    setView(previousView) // Quay về view trước đó (có thể là "list" hoặc "details")
  }

  const handleSelectStudent = (student) => {
    setSelectedStudent(student)
    setView("student_detail")
  }

  const handleBackToDanhSach = () => {
    setSelectedStudent(null)
    setView("danhsach")
  }

  const handleBackToDetails = () => {
    setView("details")
  }

  const handleBackToList = () => {
    setSelectedClass(null)
    setSelectedStudent(null)
    setSelectedSession(null)
    setView("list")
  }

  const handleBackFromAttendance = () => {
    setSelectedSession(null)
    setView("sessions") // Quay lại danh sách buổi học
  }

  const handleXemDiemDanh = (session) => {
    setSelectedSession(session)
    setView("view_attendance") // Chuyển sang trang xem điểm danh (read-only)
  }

  const handleBackFromViewAttendance = () => {
    setSelectedSession(null)
    setView("sessions") // Quay lại danh sách buổi học
  }

  if (view === "student_detail" && selectedStudent && selectedClass) {
    return <XemChiTietSV student={selectedStudent} classData={selectedClass} onBack={handleBackToDanhSach} />
  }

  if (view === "sessions" && selectedClass) {
    return (
      <XemBuoiHoc
        classData={selectedClass}
        onBack={handleBackFromSessions}
        onDiemDanhNgay={handleDiemDanhNgay}
        onXemDiemDanh={handleXemDiemDanh}
      />
    )
  }

  if (view === "view_attendance" && selectedClass && selectedSession) {
    return (
      <XemDiemDanh
        classData={selectedClass}
        session={selectedSession}
        onBack={handleBackFromViewAttendance}
      />
    )
  }

  if (view === "attendance" && selectedClass) {
    return (
      <DiemDanh
        classData={selectedClass}
        session={selectedSession}
        onBack={handleBackFromAttendance}
      />
    )
  }

  if (view === "danhsach" && selectedClass) {
    return (
      <div className="flex-1 overflow-auto bg-white min-h-screen">
        <div className="bg-blue-100 border-b border-blue-200 px-8 py-6">
          <button onClick={handleBackToList} className="text-blue-600 font-medium mb-2 hover:underline">
            ← Quay lại
          </button>
          <h1 className="text-3xl font-bold text-gray-800">Danh sách sinh viên - {selectedClass.code}</h1>
        </div>

        <div className="p-8 bg-white min-h-full">
          <DanhSachSV classData={selectedClass} onSelectStudent={handleSelectStudent} />
        </div>
      </div>
    )
  }

  if (view === "details" && selectedClass) {
    return (
      <div className="flex-1 overflow-auto bg-white min-h-screen">
        <div className="bg-blue-100 border-b border-blue-200 px-8 py-6">
          <button onClick={handleBackToList} className="text-blue-600 font-medium mb-2 hover:underline">
            ← Quay lại
          </button>
          <h1 className="text-3xl font-bold text-gray-800">Lớp học</h1>
        </div>

        <div className="p-8 bg-white min-h-full">
          <div className="mb-6">
            <h2 className="text-2xl font-bold text-gray-800">
              {selectedClass.name} - {selectedClass.code}
            </h2>
          </div>

          <div className="flex gap-4 mb-6">
            <button
              onClick={handleDanhSachClick}
              className="px-6 py-2 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-50 transition-colors"
            >
              Danh sách sinh viên
            </button>
            <button
              onClick={() => {
                setPreviousView("details") // Lưu view hiện tại là "details"
                handleDiemDanhClick()
              }}
              className="px-6 py-2 text-white rounded-lg font-medium transition-colors"
              style={{ backgroundColor: "#14193E" }}
              onMouseEnter={(e) => e.currentTarget.style.backgroundColor = "#1a2347"}
              onMouseLeave={(e) => e.currentTarget.style.backgroundColor = "#14193E"}
            >
              Điểm danh
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="flex-1 overflow-auto bg-gradient-to-br from-blue-50 to-blue-100 min-h-screen" style={{ padding: "32px" }}>
      {/* Title */}
      <h1 className="text-2xl font-bold mb-3" style={{ color: "#083b74" }}>Lớp học</h1>

      {/* Main Card Container */}
      <div className="bg-white rounded-2xl shadow-lg p-2.5 flex flex-col" style={{ height: "calc(100% - 60px)" }}>
        <div className="mb-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
            <input
              type="text"
              placeholder="Tìm kiếm khóa học...."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full max-w-md pl-10 pr-4 py-2 rounded-lg bg-gray-200 text-gray-700 placeholder-gray-500 focus:outline-none"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 overflow-y-auto flex-1">
          {filteredClasses.map((cls) => (
            <div
              key={cls.id}
              onClick={() => handleClassSelect(cls)}
              className="bg-white rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow cursor-pointer border border-blue-100"
            >
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h3 className="font-semibold text-gray-800">{cls.name}</h3>
                  <p className="text-sm text-gray-600">{cls.code}</p>
                </div>
              </div>

              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2 text-gray-700">
                  <span className="text-gray-600">👥</span>
                  <span>{cls.maxStudents} sinh viên</span>
                </div>
                <div className="flex items-center gap-2 text-gray-700">
                  <span className="text-gray-600">⏰</span>
                  <span>T2, T5: 7:00 - 11:30</span>
                </div>
                <div className="flex items-center gap-2 text-gray-700">
                  <span className="text-gray-600">📍</span>
                  <span>Phòng 403</span>
                </div>
              </div>

              <div className="mt-4 flex gap-2">
                <button
                  onClick={(e) => {
                    e.stopPropagation()
                    setSelectedClass(cls)
                    setView("danhsach")
                  }}
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                >
                  Danh sách
                </button>
                <button
                  onClick={(e) => {
                    e.stopPropagation()
                    setSelectedClass(cls)
                    setPreviousView("list") // Lưu view hiện tại là "list"
                    setView("sessions") // Chuyển sang danh sách buổi học
                  }}
                  className="flex-1 px-3 py-2 text-white rounded-lg text-sm font-medium transition-colors"
                  style={{ backgroundColor: "#14193E" }}
                  onMouseEnter={(e) => e.currentTarget.style.backgroundColor = "#1a2347"}
                  onMouseLeave={(e) => e.currentTarget.style.backgroundColor = "#14193E"}
                >
                  Điểm danh
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
