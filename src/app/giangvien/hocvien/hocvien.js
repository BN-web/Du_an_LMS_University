"use client"

import { useState, useMemo } from "react"

import { ChevronLeft, ChevronRight, Eye, Search } from "lucide-react"

import XemChiTiet from "./xemchitiet"

export default function HocVien() {
  const [currentPage, setCurrentPage] = useState(1)
  const [selectedKhoa, setSelectedKhoa] = useState("")
  const [selectedKhoaFilter, setSelectedKhoaFilter] = useState("")
  const [selectedNganh, setSelectedNganh] = useState("")
  const [selectedMon, setSelectedMon] = useState("")
  const [searchTerm, setSearchTerm] = useState("")
  const [selectedStudent, setSelectedStudent] = useState(null)

  const allKhoas = ["Khóa 13", "Khóa 14", "Khóa 15"]

  const facultyData = {
    "Khoa CNTT": ["Kỹ thuật IT", "Phần mềm", "Hệ thống"],
    "Khoa Kinh tế": ["Quản trị kinh doanh", "Kinh tế học"],
    "Khoa Kỹ thuật": ["Cơ khí", "Điện tử"],
  }

  const allFaculties = Object.keys(facultyData)

  const getMajorsForFaculty = (faculty) => {
    return faculty ? facultyData[faculty] || [] : []
  }

  const allSubjects = ["Lập trình Web", "Cơ sở dữ liệu", "Lập trình Java", "Mạng máy tính"]

  const students = [
    {
      id: "20131",
      name: "Nguyễn Hải Trường",
      khoa: "Khóa 13",
      khoa_faculty: "Khoa CNTT",
      nghe: "Kỹ thuật IT",
      lopTrinh: "Lập trình Web",
      trangThai: "Đang học",
    },
    {
      id: "20132",
      name: "Trần Thị Lan",
      khoa: "Khóa 13",
      khoa_faculty: "Khoa CNTT",
      nghe: "Phần mềm",
      lopTrinh: "Cơ sở dữ liệu",
      trangThai: "Dừng hoạt động",
    },
    {
      id: "20133",
      name: "Lê Văn Minh",
      khoa: "Khóa 13",
      khoa_faculty: "Khoa Kinh tế",
      nghe: "Quản trị kinh doanh",
      lopTrinh: "Lập trình Web",
      trangThai: "Đang học",
    },
    {
      id: "20134",
      name: "Phạm Ánh Dương",
      khoa: "Khóa 14",
      khoa_faculty: "Khoa CNTT",
      nghe: "Kỹ thuật IT",
      lopTrinh: "Lập trình Java",
      trangThai: "Đang học",
    },
    {
      id: "20135",
      name: "Đặng Quốc Huy",
      khoa: "Khóa 14",
      khoa_faculty: "Khoa Kỹ thuật",
      nghe: "Cơ khí",
      lopTrinh: "Mạng máy tính",
      trangThai: "Dừng hoạt động",
    },
    {
      id: "20136",
      name: "Vũ Thị Hương",
      khoa: "Khóa 15",
      khoa_faculty: "Khoa CNTT",
      nghe: "Phần mềm",
      lopTrinh: "Lập trình Web",
      trangThai: "Đang học",
    },
    {
      id: "20137",
      name: "Bùi Văn Sơn",
      khoa: "Khóa 15",
      khoa_faculty: "Khoa Kinh tế",
      nghe: "Kinh tế học",
      lopTrinh: "Cơ sở dữ liệu",
      trangThai: "Đang học",
    },
    {
      id: "20138",
      name: "Hoàng Thị Nhi",
      khoa: "Khóa 13",
      khoa_faculty: "Khoa Kỹ thuật",
      nghe: "Điện tử",
      lopTrinh: "Lập trình Java",
      trangThai: "Dừng hoạt động",
    },
  ]

  const filteredStudents = useMemo(() => {
    return students.filter((student) => {
      const khoaMatch = !selectedKhoa || student.khoa === selectedKhoa
      const khoaFacultyMatch = !selectedKhoaFilter || student.khoa_faculty === selectedKhoaFilter
      const nganhMatch = !selectedNganh || student.nghe === selectedNganh
      const monMatch = !selectedMon || student.lopTrinh === selectedMon

      const searchMatch =
        !searchTerm ||
        student.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
        student.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        student.lopTrinh.toLowerCase().includes(searchTerm.toLowerCase())

      return khoaMatch && khoaFacultyMatch && nganhMatch && monMatch && searchMatch
    })
  }, [selectedKhoa, selectedKhoaFilter, selectedNganh, selectedMon, searchTerm])

  const stats = {
    total: filteredStudents.length,
    active: filteredStudents.filter((s) => s.trangThai === "Đang học").length,
    inactive: filteredStudents.filter((s) => s.trangThai === "Dừng hoạt động").length,
  }

  const handleFacultyChange = (value) => {
    setSelectedKhoaFilter(value)
    setSelectedNganh("")
  }

  const getStatusColor = (status) => {
    switch (status) {
      case "Đang học":
        return "bg-green-50 text-green-700 border border-green-200 font-medium"
      case "Dừng hoạt động":
        return "bg-orange-50 text-orange-700 border border-orange-200 font-medium"
      default:
        return "bg-gray-50 text-gray-700 border border-gray-200 font-medium"
    }
  }

  const handleViewDetails = (student) => {
    setSelectedStudent(student)
  }

  const handleBackFromDetails = () => {
    setSelectedStudent(null)
  }

  if (selectedStudent) {
    return <XemChiTiet student={selectedStudent} onBack={handleBackFromDetails} />
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-blue-100" style={{ padding: "32px" }}>
      <div className="max-w-[98%] mx-auto">
        {/* Title outside card */}
        <h1 className="text-2xl font-bold mb-3" style={{ color: "#083b74" }}>Quản lý sinh viên</h1>

        {/* Main Card Container */}
        <div className="bg-white rounded-2xl shadow-lg p-2.5">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-blue-500">
            <div className="flex items-center gap-4">
              <div className="w-1 h-16 bg-blue-500 rounded"></div>
              <div>
                <p className="text-gray-600 text-sm font-medium">Tổng số học viên</p>
                <p className="text-4xl font-bold text-gray-900 mt-1">{stats.total}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-green-500">
            <div className="flex items-center gap-4">
              <div className="w-1 h-16 bg-green-500 rounded"></div>
              <div>
                <p className="text-gray-600 text-sm font-medium">Hoạt động</p>
                <p className="text-4xl font-bold text-green-600 mt-1">{stats.active}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-red-500">
            <div className="flex items-center gap-4">
              <div className="w-1 h-16 bg-red-500 rounded"></div>
              <div>
                <p className="text-gray-600 text-sm font-medium">Dừng hoạt động</p>
                <p className="text-4xl font-bold text-red-600 mt-1">{stats.inactive}</p>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4 mb-8">
          <div className="flex flex-wrap gap-4 items-center">
            <select
              value={selectedKhoa}
              onChange={(e) => setSelectedKhoa(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 bg-white hover:border-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Tất cả khóa</option>
              {allKhoas.map((khoa) => (
                <option key={khoa} value={khoa}>
                  {khoa}
                </option>
              ))}
            </select>

            <select
              value={selectedKhoaFilter}
              onChange={(e) => handleFacultyChange(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 bg-white hover:border-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Tất cả khoa</option>
              {allFaculties.map((faculty) => (
                <option key={faculty} value={faculty}>
                  {faculty}
                </option>
              ))}
            </select>

            <select
              value={selectedNganh}
              onChange={(e) => setSelectedNganh(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 bg-white hover:border-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">{selectedKhoaFilter ? "Tất cả ngành" : "Tất cả ngành"}</option>
              {getMajorsForFaculty(selectedKhoaFilter).map((major) => (
                <option key={major} value={major}>
                  {major}
                </option>
              ))}
            </select>

            <select
              value={selectedMon}
              onChange={(e) => setSelectedMon(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 bg-white hover:border-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Tất cả môn</option>
              {allSubjects.map((subject) => (
                <option key={subject} value={subject}>
                  {subject}
                </option>
              ))}
            </select>

            <div className="flex-1 flex items-center gap-2 border border-gray-300 rounded-lg px-4 py-2 bg-white">
              <Search className="w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Tìm kiếm học viên..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="flex-1 outline-none text-gray-700 placeholder-gray-500"
              />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="bg-gray-100 border-b border-gray-200">
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">MSSV</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">Họ tên</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">Khóa</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">Khoa</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">Ngành</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">Lớp</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">Trạng thái</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredStudents.map((student, index) => (
                  <tr key={index} className="border-b border-gray-200 hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 text-sm text-gray-900">{student.id}</td>
                    <td className="px-6 py-4 text-sm text-gray-900">{student.name}</td>
                    <td className="px-6 py-4 text-sm text-gray-900">{student.khoa}</td>
                    <td className="px-6 py-4 text-sm text-gray-900">{student.khoa_faculty}</td>
                    <td className="px-6 py-4 text-sm text-gray-900">{student.nghe}</td>
                    <td className="px-6 py-4 text-sm text-gray-900">{student.lopTrinh}</td>
                    <td className="px-6 py-4 text-sm">
                      <span
                        className={`px-3 py-1 rounded-full font-medium text-center inline-block min-w-[120px] ${getStatusColor(student.trangThai)}`}
                      >
                        {student.trangThai}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm">
                      <Eye
                        className="w-5 h-5 text-gray-400 cursor-pointer hover:text-gray-600 transition-colors"
                        onClick={() => handleViewDetails(student)}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="flex items-center justify-between px-6 py-4 border-t border-gray-200 bg-white">
            <div className="text-sm text-gray-600">
              Trang <span className="font-semibold">1</span> / 40
            </div>
            <div className="flex items-center gap-2">
              <button className="p-2 border border-gray-300 rounded-lg hover:bg-gray-100 transition-colors">
                <ChevronLeft className="w-4 h-4 text-gray-600" />
              </button>
              <button className="w-10 h-10 bg-blue-600 text-white rounded-lg font-semibold text-sm">1</button>
              <button className="w-10 h-10 border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors text-sm font-semibold">
                2
              </button>
              <button className="w-10 h-10 border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors text-sm font-semibold">
                3
              </button>
              <button className="w-10 h-10 border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors text-sm font-semibold">
                4
              </button>
              <span className="text-gray-400">...</span>
              <button className="w-10 h-10 border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors text-sm font-semibold">
                40
              </button>
              <button className="p-2 border border-gray-300 rounded-lg hover:bg-gray-100 transition-colors">
                <ChevronRight className="w-4 h-4 text-gray-600" />
              </button>
            </div>
          </div>
        </div>
        </div>
      </div>
    </div>
  )
}

