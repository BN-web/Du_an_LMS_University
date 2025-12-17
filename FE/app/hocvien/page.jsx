"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Search, ChevronDown, Eye, Download, ChevronLeft, ChevronRight } from "lucide-react"
import { apiClient } from "@/lib/api"
import { authUtils } from "@/lib/auth"

export default function HocVienPage() {
  const router = useRouter()
  const [currentView, setCurrentView] = useState("list")
  const [selectedStudent, setSelectedStudent] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [limit] = useState(10)
  
  // States for API data
  const [studentsData, setStudentsData] = useState([])
  const [studentDetail, setStudentDetail] = useState(null)
  const [historyData, setHistoryData] = useState([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)
  
  // Pagination states
  const [totalPages, setTotalPages] = useState(0)
  const [total, setTotal] = useState(0)
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState("")
  const [khoaId, setKhoaId] = useState(null)
  const [nganhId, setNganhId] = useState(null)
  const [khoaTuyenSinhId, setKhoaTuyenSinhId] = useState(null)
  const [trangThai, setTrangThai] = useState(null)
  
  // Options for dropdowns
  const [khoaTuyenSinhOptions, setKhoaTuyenSinhOptions] = useState([])
  const [khoaOptions, setKhoaOptions] = useState([])
  const [nganhOptions, setNganhOptions] = useState([])
  
  // History pagination
  const [historyPage, setHistoryPage] = useState(1)
  const [historyTotalPages, setHistoryTotalPages] = useState(0)

  // Fetch danh sách học viên
  const fetchStudents = async (page = 1) => {
    try {
      setIsLoading(true)
      setError(null)
      
      const params = new URLSearchParams({
        page: page.toString(),
        limit: limit.toString(),
      })
      
      if (searchTerm) params.append("search", searchTerm)
      if (khoaId) params.append("khoaId", khoaId.toString())
      if (nganhId) params.append("nganhId", nganhId.toString())
      if (khoaTuyenSinhId) params.append("khoaTuyenSinhId", khoaTuyenSinhId.toString())
      if (trangThai !== null) params.append("trangThai", trangThai.toString())
      
      const response = await apiClient.get(`/api/truong-khoa/hoc-vien?${params.toString()}`)
      
      // Xử lý response - có thể là object với success hoặc array trực tiếp
      if (response && typeof response === 'object') {
        // Nếu có success property
        if ('success' in response) {
          if (response.success && response.data) {
            setStudentsData(response.data.items || response.data.Items || [])
            setTotal(response.data.total || response.data.Total || 0)
            setTotalPages(response.data.totalPages || response.data.TotalPages || 0)
            setCurrentPage(response.data.page || response.data.Page || 1)
          } else {
            // Nếu success = false, có thể có message
            const errorMsg = response.message || response.Message || "Không thể tải danh sách học viên"
            setError(errorMsg)
            setStudentsData([])
            setTotal(0)
            setTotalPages(0)
          }
        } 
        // Nếu không có success property nhưng có data
        else if (response.data) {
          setStudentsData(response.data.items || response.data.Items || [])
          setTotal(response.data.total || response.data.Total || 0)
          setTotalPages(response.data.totalPages || response.data.TotalPages || 0)
          setCurrentPage(response.data.page || response.data.Page || 1)
        }
        // Nếu là array trực tiếp
        else if (Array.isArray(response)) {
          setStudentsData(response)
          setTotal(response.length)
          setTotalPages(1)
          setCurrentPage(1)
        }
        else {
          setStudentsData([])
          setTotal(0)
          setTotalPages(0)
        }
      } else {
        setStudentsData([])
        setTotal(0)
        setTotalPages(0)
      }
    } catch (err) {
      console.error("Lỗi khi tải danh sách học viên:", err)
      
      // Hiển thị thông báo lỗi thân thiện hơn
      let errorMessage = "Không thể tải danh sách học viên"
      
      if (err.message) {
        errorMessage = err.message
      } else if (err.responseText) {
        // Nếu có raw response text, kiểm tra xem có lỗi SQL không
        if (err.responseText.includes('syntax') || err.responseText.includes('WITH')) {
          errorMessage = "Lỗi cơ sở dữ liệu. Vui lòng liên hệ quản trị viên."
        }
      }
      
      setError(errorMessage)
      setStudentsData([])
      setTotal(0)
      setTotalPages(0)
    } finally {
      setIsLoading(false)
    }
  }
  
  // Fetch chi tiết học viên
  const fetchStudentDetail = async (id) => {
    try {
      setIsLoading(true)
      setError(null)
      
      const response = await apiClient.get(`/api/truong-khoa/hoc-vien/${id}`)
      
      if (response.success && response.data) {
        setStudentDetail(response.data)
        setSelectedStudent(response.data.thongTinCaNhan)
      } else {
        setError("Không tìm thấy thông tin học viên")
      }
    } catch (err) {
      console.error("Lỗi khi tải chi tiết học viên:", err)
      setError(err.message || "Không thể tải chi tiết học viên")
    } finally {
      setIsLoading(false)
    }
  }
  
  // Fetch lịch sử điểm
  const fetchHistory = async (id, page = 1) => {
    try {
      setIsLoading(true)
      setError(null)
      
      const params = new URLSearchParams({
        page: page.toString(),
        limit: limit.toString(),
      })
      
      const response = await apiClient.get(`/api/truong-khoa/hoc-vien/${id}/lich-su-diem?${params.toString()}`)
      
      if (response.success && response.data) {
        setHistoryData(response.data.items || [])
        setHistoryTotalPages(response.data.totalPages || 0)
        setHistoryPage(response.data.page || 1)
      } else {
        setHistoryData([])
      }
    } catch (err) {
      console.error("Lỗi khi tải lịch sử điểm:", err)
      setError(err.message || "Không thể tải lịch sử điểm")
      setHistoryData([])
    } finally {
      setIsLoading(false)
    }
  }
  
  // Load options for dropdowns
  useEffect(() => {
    const fetchOptions = async () => {
      try {
        // Fetch khóa tuyển sinh
        const khoaTuyenSinhData = await apiClient.get("/api/truong-khoa/options/khoa-tuyen-sinh")
        setKhoaTuyenSinhOptions(Array.isArray(khoaTuyenSinhData) ? khoaTuyenSinhData : [])

        // Fetch khoa
        const khoaData = await apiClient.get("/api/truong-khoa/options/khoa")
        setKhoaOptions(Array.isArray(khoaData) ? khoaData : [])

        // Fetch ngành (tất cả ngành, không filter theo khoa)
        const nganhData = await apiClient.get("/api/truong-khoa/options/nganh")
        setNganhOptions(Array.isArray(nganhData) ? nganhData : [])
      } catch (error) {
        console.error("Lỗi khi tải options:", error)
      }
    }

    fetchOptions()
  }, [])

  // Load ngành khi khoa thay đổi
  useEffect(() => {
    const fetchNganh = async () => {
      try {
        if (!khoaId) {
          const nganhData = await apiClient.get("/api/truong-khoa/options/nganh")
          setNganhOptions(Array.isArray(nganhData) ? nganhData : [])
        } else {
          const nganhData = await apiClient.get(`/api/truong-khoa/options/nganh?khoaId=${khoaId}`)
          setNganhOptions(Array.isArray(nganhData) ? nganhData : [])
        }
        // Reset ngành khi khoa thay đổi
        setNganhId(null)
      } catch (error) {
        console.error("Lỗi khi tải ngành:", error)
      }
    }

    fetchNganh()
  }, [khoaId])

  // Load data on mount and when filters change
  useEffect(() => {
    if (!authUtils.isAuthenticated()) {
      router.push("/login")
      return
    }
    // Only fetch when page changes, not when filters change (filters will be applied on search button click)
    fetchStudents(currentPage)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPage])
  
  const handleViewDetail = async (student) => {
    setSelectedStudent(student)
    setCurrentView("detail")
    await fetchStudentDetail(student.id)
  }

  const handleViewHistory = async () => {
    if (selectedStudent?.id) {
      setCurrentView("history")
      await fetchHistory(selectedStudent.id, historyPage)
    }
  }
  
  const handleSearch = () => {
    setCurrentPage(1)
    fetchStudents(1)
  }
  
  const handleExportPDF = async () => {
    try {
      const params = new URLSearchParams({
        format: "pdf",
        loaiBaoCao: "danh-sach",
      })
      
      if (khoaId) params.append("khoaId", khoaId.toString())
      if (nganhId) params.append("nganhId", nganhId.toString())
      if (khoaTuyenSinhId) params.append("khoaTuyenSinhId", khoaTuyenSinhId.toString())
      
      // Note: API này trả về file, cần xử lý download
      const response = await apiClient.get(`/api/truong-khoa/hoc-vien/xuat-bao-cao?${params.toString()}`)
      alert("Chức năng xuất PDF đang được phát triển")
    } catch (err) {
      console.error("Lỗi khi xuất PDF:", err)
      alert("Không thể xuất PDF: " + (err.message || "Lỗi không xác định"))
    }
  }
  
  const handleExportExcel = async () => {
    try {
      const params = new URLSearchParams({
        format: "excel",
        loaiBaoCao: "danh-sach",
      })
      
      if (khoaId) params.append("khoaId", khoaId.toString())
      if (nganhId) params.append("nganhId", nganhId.toString())
      if (khoaTuyenSinhId) params.append("khoaTuyenSinhId", khoaTuyenSinhId.toString())
      
      // Note: API này trả về file, cần xử lý download
      const response = await apiClient.get(`/api/truong-khoa/hoc-vien/xuat-bao-cao?${params.toString()}`)
      alert("Chức năng xuất Excel đang được phát triển")
    } catch (err) {
      console.error("Lỗi khi xuất Excel:", err)
      alert("Không thể xuất Excel: " + (err.message || "Lỗi không xác định"))
    }
  }
  
  const handlePageChange = (page) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page)
      fetchStudents(page)
    }
  }
  
  const handleHistoryPageChange = (page) => {
    if (page >= 1 && page <= historyTotalPages && selectedStudent?.id) {
      setHistoryPage(page)
      fetchHistory(selectedStudent.id, page)
    }
  }

  const handleBack = () => {
    if (currentView === "history") {
      setCurrentView("detail")
    } else {
      setCurrentView("list")
      setSelectedStudent(null)
    }
  }

  const getStatusStyle = (status) => {
    switch (status) {
      case "Đang học":
        return "bg-green-100 text-green-600 border-green-300"
      case "Đã nghỉ":
        return "bg-red-100 text-red-600 border-red-300"
      default:
        return "bg-gray-100 text-gray-600 border-gray-300"
    }
  }

  // Trang Lịch sử thay đổi điểm
  if (currentView === "history") {
    const startItem = (historyPage - 1) * limit + 1
    const endItem = Math.min(historyPage * limit, total)
    
    return (
      <main className="flex-1 p-6">
        <div className="flex items-center gap-4 mb-6">
          <button onClick={handleBack} className="text-gray-500 hover:text-gray-700">
            <ChevronLeft className="w-6 h-6" />
          </button>
          <h1 className="text-[#4A90D9] text-2xl font-bold">Lịch sử thay đổi điểm</h1>
        </div>

        {isLoading && (
          <div className="bg-white rounded-xl p-6 shadow-sm text-center">
            <p className="text-gray-500">Đang tải...</p>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-4 mb-4">
            <p className="text-red-600">{error}</p>
          </div>
        )}

        {!isLoading && !error && (
          <div className="bg-white rounded-xl p-6 shadow-sm">
            {historyData.length === 0 ? (
              <p className="text-center text-gray-500 py-8">Không có lịch sử thay đổi điểm</p>
            ) : (
              <>
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">THỜI GIAN</th>
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">NGƯỜI THỰC HIỆN</th>
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">MÔN HỌC</th>
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">THÀNH PHẦN ĐIỂM</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">ĐIỂM CŨ</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">ĐIỂM MỚI</th>
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">LÝ DO</th>
                    </tr>
                  </thead>
                  <tbody>
                    {historyData.map((item, idx) => (
                      <tr key={idx} className="border-b border-gray-100">
                        <td className="py-4 px-4 text-sm text-gray-700">{item.thoiGian || ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700 font-medium">{item.nguoiThucHien || ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700">{item.monHoc || ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700">{item.thanhPhanDiem || ""}</td>
                        <td className="py-4 px-4 text-sm text-center text-red-500">{item.diemCu ?? ""}</td>
                        <td className="py-4 px-4 text-sm text-center text-green-500">{item.diemMoi ?? ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700">{item.lyDo || ""}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                <div className="flex items-center justify-between mt-4">
                  <p className="text-sm text-gray-500">
                    Hiển thị {startItem}-{endItem} của {total} mục
                  </p>
                  <div className="flex items-center gap-2">
                    <button 
                      onClick={() => handleHistoryPageChange(historyPage - 1)}
                      disabled={historyPage <= 1}
                      className="p-2 text-gray-400 disabled:opacity-50 disabled:cursor-not-allowed hover:text-gray-600"
                    >
                      <ChevronLeft className="w-4 h-4" />
                    </button>
                    {Array.from({ length: Math.min(5, historyTotalPages) }, (_, i) => {
                      let pageNum
                      if (historyTotalPages <= 5) {
                        pageNum = i + 1
                      } else if (historyPage <= 3) {
                        pageNum = i + 1
                      } else if (historyPage >= historyTotalPages - 2) {
                        pageNum = historyTotalPages - 4 + i
                      } else {
                        pageNum = historyPage - 2 + i
                      }
                      return (
                        <button
                          key={pageNum}
                          onClick={() => handleHistoryPageChange(pageNum)}
                          className={`w-8 h-8 rounded text-sm ${
                            historyPage === pageNum
                              ? "bg-[#4A90D9] text-white"
                              : "text-gray-600 hover:bg-gray-100"
                          }`}
                        >
                          {pageNum}
                        </button>
                      )
                    })}
                    <button 
                      onClick={() => handleHistoryPageChange(historyPage + 1)}
                      disabled={historyPage >= historyTotalPages}
                      className="p-2 text-gray-400 disabled:opacity-50 disabled:cursor-not-allowed hover:text-gray-600"
                    >
                      <ChevronRight className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>
        )}
      </main>
    )
  }

  // Trang Chi tiết sinh viên
  if (currentView === "detail" && selectedStudent && studentDetail) {
    const thongTin = studentDetail.thongTinCaNhan || {}
    const bangDiem = studentDetail.bangDiemMonHoc || []
    
    return (
      <main className="flex-1 p-6">
        <div className="flex items-center gap-4 mb-6">
          <button onClick={handleBack} className="text-gray-500 hover:text-gray-700">
            <ChevronLeft className="w-6 h-6" />
          </button>
          <h1 className="text-[#4A90D9] text-2xl font-bold">Chi tiết sinh viên</h1>
        </div>

        {isLoading && (
          <div className="bg-white rounded-xl p-6 shadow-sm text-center">
            <p className="text-gray-500">Đang tải...</p>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-4 mb-4">
            <p className="text-red-600">{error}</p>
          </div>
        )}

        {!isLoading && !error && (
          <>
            <div className="bg-white rounded-xl p-6 shadow-sm mb-6">
              <div className="flex gap-8">
                {thongTin.avatar ? (
                  <img src={thongTin.avatar} alt="Avatar" className="w-32 h-32 rounded-full object-cover" />
                ) : (
                  <div className="w-32 h-32 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-4xl font-bold">
                    {thongTin.hoTen ? thongTin.hoTen.charAt(0).toUpperCase() : "H"}
                  </div>
                )}
                <div className="flex-1 grid grid-cols-3 gap-x-12 gap-y-4">
                  <div>
                    <p className="text-gray-500 text-sm">MSSV</p>
                    <p className="text-gray-800 font-medium">{thongTin.mssv || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Họ tên</p>
                    <p className="text-gray-800 font-medium">{thongTin.hoTen || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Giới tính</p>
                    <p className="text-gray-800 font-medium">{thongTin.gioiTinh || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Ngày sinh</p>
                    <p className="text-gray-800 font-medium">{thongTin.ngaySinh || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Điện thoại</p>
                    <p className="text-gray-800 font-medium">{thongTin.dienThoai || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Email</p>
                    <p className="text-gray-800 font-medium">{thongTin.email || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Chuyên ngành</p>
                    <p className="text-gray-800 font-medium">{thongTin.chuyenNganh || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Khóa</p>
                    <p className="text-gray-800 font-medium">{thongTin.khoa || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">GPA tích lũy</p>
                    <p className="text-green-500 font-medium">{thongTin.gpaTichLuy ?? ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Tổng tín chỉ</p>
                    <p className="text-gray-800 font-medium">{thongTin.tongTinChi ?? ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Ngày tạo hồ sơ</p>
                    <p className="text-gray-800 font-medium">{thongTin.ngayTaoHoSo || ""}</p>
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Trạng thái</p>
                    <span
                      className={`inline-block px-3 py-1 rounded text-xs font-medium border ${getStatusStyle(thongTin.trangThai)}`}
                    >
                      {thongTin.trangThai || ""}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-xl p-6 shadow-sm">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <h2 className="text-gray-800 font-semibold text-lg">Bảng điểm môn học</h2>
                </div>
                <button
                  onClick={handleViewHistory}
                  className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                >
                  Lịch sử thay đổi điểm
                </button>
              </div>
              {bangDiem.length === 0 ? (
                <p className="text-center text-gray-500 py-8">Không có dữ liệu bảng điểm</p>
              ) : (
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">MÔN HỌC</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">BÀI TẬP</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">GIỮA KỲ</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">CUỐI KỲ</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">CHUYÊN CẦN</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">TB MÔN</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">ĐIỂM CHỮ</th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">GPA MÔN</th>
                    </tr>
                  </thead>
                  <tbody>
                    {bangDiem.map((item, idx) => (
                      <tr key={idx} className="border-b border-gray-100">
                        <td className="py-4 px-4 text-sm text-gray-700">{item.monHoc || ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700 text-center">{item.diemBaiTap ?? ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700 text-center">{item.diemGiuaKy ?? ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700 text-center">{item.diemCuoiKy ?? ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700 text-center">{item.diemChuyenCan ?? ""}</td>
                        <td className="py-4 px-4 text-sm text-center text-red-500">{item.diemTrungBinhMon ?? ""}</td>
                        <td className="py-4 px-4 text-sm text-gray-700 text-center">{item.diemChu || ""}</td>
                        <td className="py-4 px-4 text-sm text-center text-green-500">{item.gpaMon ?? ""}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </>
        )}
      </main>
    )
  }

  // Trang Danh sách sinh viên
  return (
    <main className="flex-1 p-6">
      <h1 className="text-[#4A90D9] text-2xl font-bold mb-6">Quản lý sinh viên</h1>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#4A90D9] shadow-sm">
          <p className="text-[#4A90D9] text-sm font-medium mb-1">Tổng số học viên</p>
          <p className="text-[#4A90D9] text-2xl font-bold">{total}</p>
        </div>
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#22C55E] shadow-sm">
          <p className="text-[#22C55E] text-sm font-medium mb-1">Học viên đang học</p>
          <p className="text-[#22C55E] text-2xl font-bold">
            {studentsData.filter((s) => s.trangThai === "Đang học").length}
          </p>
        </div>
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#EF4444] shadow-sm">
          <p className="text-[#EF4444] text-sm font-medium mb-1">Học viên đã nghỉ</p>
          <p className="text-[#EF4444] text-2xl font-bold">
            {studentsData.filter((s) => s.trangThai === "Đã nghỉ").length}
          </p>
        </div>
      </div>

      <div className="flex items-center gap-4 mb-6">
        <div className="relative">
          <select 
            className="appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
            value={khoaTuyenSinhId || ""}
            onChange={(e) => setKhoaTuyenSinhId(e.target.value ? parseInt(e.target.value) : null)}
          >
            <option value="">Tất cả khóa</option>
            {khoaTuyenSinhOptions.map((option) => (
              <option key={option.id} value={option.id}>
                {option.label}
              </option>
            ))}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        </div>
        <div className="relative">
          <select 
            className="appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
            value={khoaId || ""}
            onChange={(e) => setKhoaId(e.target.value ? parseInt(e.target.value) : null)}
          >
            <option value="">Tất cả khoa</option>
            {khoaOptions.map((option) => (
              <option key={option.id} value={option.id}>
                {option.label}
              </option>
            ))}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        </div>
        <div className="relative">
          <select 
            className="appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
            value={nganhId || ""}
            onChange={(e) => setNganhId(e.target.value ? parseInt(e.target.value) : null)}
          >
            <option value="">Tất cả ngành</option>
            {nganhOptions.map((option) => (
              <option key={option.id} value={option.id}>
                {option.label}
              </option>
            ))}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        </div>
        <div className="relative">
          <select 
            className="appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white"
            value={trangThai !== null ? trangThai.toString() : ""}
            onChange={(e) => setTrangThai(e.target.value ? parseInt(e.target.value) : null)}
          >
            <option value="">Tất cả trạng thái</option>
            <option value="1">Đang học</option>
            <option value="2">Đã tốt nghiệp</option>
            <option value="3">Đã nghỉ</option>
            <option value="4">Cảnh báo</option>
            <option value="0">Bị khóa</option>
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        </div>
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Tìm kiếm học viên..."
            className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg text-sm"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onKeyPress={(e) => e.key === "Enter" && handleSearch()}
          />
        </div>
        <button
          onClick={handleSearch}
          className="px-4 py-2.5 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8]"
        >
          Tìm kiếm
        </button>
      </div>

      <div className="bg-white rounded-xl p-6 shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-gray-800 font-semibold text-lg">Danh sách sinh viên</h2>
          <div className="flex gap-3">
            <button 
              onClick={handleExportPDF}
              className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
            >
              <Download className="w-4 h-4" />
              Xuất PDF
            </button>
            <button 
              onClick={handleExportExcel}
              className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
            >
              <Download className="w-4 h-4" />
              Xuất Excel
            </button>
          </div>
        </div>
        {isLoading && (
          <div className="text-center py-8">
            <p className="text-gray-500">Đang tải...</p>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-4 mb-4">
            <p className="text-red-600">{error}</p>
          </div>
        )}

        {!isLoading && !error && (
          <>
            {studentsData.length === 0 ? (
              <p className="text-center text-gray-500 py-8">Không có dữ liệu học viên</p>
            ) : (
              <>
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">MSSV</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Họ tên</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Khóa</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Khoa</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Ngành</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Môn/Lớp</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Giảng viên</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Tổng tín chỉ</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">GPA</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Trạng thái</th>
                      <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {studentsData.map((student) => (
                      <tr key={student.id || student.mssv} className="border-b border-gray-100">
                        <td className="py-3 px-2 text-sm text-gray-700">{student.mssv || ""}</td>
                        <td className="py-3 px-2 text-sm text-gray-700">{student.hoTen || ""}</td>
                        <td className="py-3 px-2 text-sm text-gray-700">{student.khoa || ""}</td>
                        <td className="py-3 px-2 text-sm text-gray-700">{student.tenKhoa || ""}</td>
                        <td className="py-3 px-2 text-sm text-gray-700">{student.nganh || ""}</td>
                        <td className="py-3 px-2 text-sm text-gray-700">{student.monLop || ""}</td>
                        <td className="py-3 px-2 text-sm text-gray-700">{student.giangVien || ""}</td>
                        <td className="py-3 px-2 text-sm text-gray-700">{student.tongTinChi ?? ""}</td>
                        <td className="py-3 px-2 text-sm text-green-500">{student.gpa ?? ""}</td>
                        <td className="py-3 px-2">
                          <span className={`px-2 py-1 rounded text-xs font-medium border ${getStatusStyle(student.trangThai)}`}>
                            {student.trangThai || ""}
                          </span>
                        </td>
                        <td className="py-3 px-2">
                          <button onClick={() => handleViewDetail(student)} className="text-gray-500 hover:text-[#4A90D9]">
                            <Eye className="w-5 h-5" />
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                <div className="flex items-center justify-end gap-2 mt-4">
                  <button 
                    onClick={() => handlePageChange(currentPage - 1)}
                    disabled={currentPage <= 1}
                    className="p-2 text-gray-400 disabled:opacity-50 disabled:cursor-not-allowed hover:text-gray-600"
                  >
                    <ChevronLeft className="w-4 h-4" />
                  </button>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum
                    if (totalPages <= 5) {
                      pageNum = i + 1
                    } else if (currentPage <= 3) {
                      pageNum = i + 1
                    } else if (currentPage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i
                    } else {
                      pageNum = currentPage - 2 + i
                    }
                    return (
                      <button
                        key={pageNum}
                        onClick={() => handlePageChange(pageNum)}
                        className={`w-8 h-8 rounded text-sm ${
                          currentPage === pageNum
                            ? "bg-[#4A90D9] text-white"
                            : "text-gray-600 hover:bg-gray-100"
                        }`}
                      >
                        {pageNum}
                      </button>
                    )
                  })}
                  <button 
                    onClick={() => handlePageChange(currentPage + 1)}
                    disabled={currentPage >= totalPages}
                    className="p-2 text-gray-400 disabled:opacity-50 disabled:cursor-not-allowed hover:text-gray-600"
                  >
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
              </>
            )}
          </>
        )}
      </div>
    </main>
  )
}
