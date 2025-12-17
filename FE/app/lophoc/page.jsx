"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { Search, ChevronDown, Eye, UserCheck, UserPlus, X, Check, ChevronLeft, ChevronRight } from "lucide-react"
import { apiClient } from "@/lib/api"

export default function LopHocPage() {
  const router = useRouter()

  const [currentView, setCurrentView] = useState("list")
  const [classes, setClasses] = useState([])
  const [loadingClasses, setLoadingClasses] = useState(true)
  const [classesError, setClassesError] = useState(null)

  const [selectedClass, setSelectedClass] = useState(null)
  const [classDetail, setClassDetail] = useState(null)
  const [students, setStudents] = useState([])
  const [loadingDetail, setLoadingDetail] = useState(false)
  const [detailError, setDetailError] = useState(null)

  // Bộ lọc
  const [searchTerm, setSearchTerm] = useState("")
  const [selectedHocKyId, setSelectedHocKyId] = useState("")
  const [selectedKhoaId, setSelectedKhoaId] = useState("")
  const [selectedNganhId, setSelectedNganhId] = useState("")
  const [selectedMonId, setSelectedMonId] = useState("")

  const [hocKyOptions, setHocKyOptions] = useState([])
  const [khoaOptions, setKhoaOptions] = useState([])
  const [nganhOptions, setNganhOptions] = useState([])
  const [monOptions, setMonOptions] = useState([])
  const [loadingOptions, setLoadingOptions] = useState(false)
  const [optionsError, setOptionsError] = useState(null)

  // State cho phân công / đổi giảng viên
  const [showPhanCongModal, setShowPhanCongModal] = useState(false)
  const [showSuccessModal, setShowSuccessModal] = useState(false)
  const [phanCongClass, setPhanCongClass] = useState(null)
  const [lecturerOptions, setLecturerOptions] = useState([])
  const [loadingLecturers, setLoadingLecturers] = useState(false)
  const [assignError, setAssignError] = useState(null)
  const [selectedGiangVienId, setSelectedGiangVienId] = useState("")
  const [selectedGiangVienName, setSelectedGiangVienName] = useState("")

  const [currentPage, setCurrentPage] = useState(1)

  // Load options cho bộ lọc
  useEffect(() => {
    const fetchOptions = async () => {
      try {
        setLoadingOptions(true)
        setOptionsError(null)

        const [khoaRes, nganhRes, monRes, hocKyRes] = await Promise.all([
          apiClient.get("/api/truong-khoa/options/khoa"),
          apiClient.get("/api/truong-khoa/options/nganh"),
          apiClient.get("/api/truong-khoa/options/mon-hoc"),
          apiClient.get("/api/truong-khoa/options/hoc-ky"),
        ])

        setKhoaOptions(Array.isArray(khoaRes) ? khoaRes : [])
        setNganhOptions(Array.isArray(nganhRes) ? nganhRes : [])
        setMonOptions(Array.isArray(monRes) ? monRes : [])
        setHocKyOptions(Array.isArray(hocKyRes) ? hocKyRes : [])
      } catch (error) {
        setOptionsError(error instanceof Error ? error.message : "Không thể tải dữ liệu bộ lọc")
      } finally {
        setLoadingOptions(false)
      }
    }

    fetchOptions()
  }, [])

  // Khi chọn khoa -> load lại ngành tương ứng
  useEffect(() => {
    const fetchNganh = async () => {
      try {
        if (!selectedKhoaId) {
          const data = await apiClient.get("/api/truong-khoa/options/nganh")
          setNganhOptions(Array.isArray(data) ? data : [])
          return
        }
        const data = await apiClient.get(`/api/truong-khoa/options/nganh?khoaId=${selectedKhoaId}`)
        setNganhOptions(Array.isArray(data) ? data : [])
      } catch {
        // giữ nguyên danh sách cũ nếu lỗi
      }
    }

    fetchNganh()
    setSelectedNganhId("")
  }, [selectedKhoaId])

  // Load danh sách lớp học theo bộ lọc từ API Trưởng khoa
  useEffect(() => {
    const fetchClasses = async () => {
      try {
        setLoadingClasses(true)
        setClassesError(null)

        const params = new URLSearchParams()
        if (searchTerm.trim()) params.append("Q", searchTerm.trim())
        if (selectedKhoaId) params.append("KhoaId", selectedKhoaId)
        if (selectedNganhId) params.append("NganhId", selectedNganhId)
        if (selectedMonId) params.append("MonHocId", selectedMonId)
        // selectedHocKyId hiện chưa được backend hỗ trợ filter, nên chỉ dùng cho hiển thị

        const qs = params.toString()
        const url = `/api/truong-khoa/lop-hoc${qs ? `?${qs}` : ""}`
        const data = await apiClient.get(url)
        setClasses(Array.isArray(data) ? data : [])
      } catch (error) {
        setClassesError(error instanceof Error ? error.message : "Không thể tải danh sách lớp học")
        setClasses([])
      } finally {
        setLoadingClasses(false)
      }
    }

    fetchClasses()
  }, [searchTerm, selectedKhoaId, selectedNganhId, selectedMonId])

  const loadLecturersIfNeeded = async () => {
    if (lecturerOptions.length > 0) return
    try {
      setLoadingLecturers(true)
      setAssignError(null)
      const data = await apiClient.get("/api/truong-khoa/options/giang-vien")
      setLecturerOptions(Array.isArray(data) ? data : [])
    } catch (error) {
      setAssignError(error instanceof Error ? error.message : "Không thể tải danh sách giảng viên")
    } finally {
      setLoadingLecturers(false)
    }
  }

  const openAssignModal = async (classItem) => {
    setPhanCongClass(classItem)
    setSelectedGiangVienId("")
    setAssignError(null)
    await loadLecturersIfNeeded()
    setShowPhanCongModal(true)
  }

  const handleViewDetail = async (classItem) => {
    setSelectedClass(classItem)
    setCurrentView("detail")
    setLoadingDetail(true)
    setDetailError(null)

    try {
      const [detail, svList] = await Promise.all([
        apiClient.get(`/api/truong-khoa/lop-hoc/${classItem.id}`),
        apiClient.get(`/api/truong-khoa/lop-hoc/${classItem.id}/sinh-vien`),
      ])

      setClassDetail(detail)
      setStudents(svList || [])
    } catch (error) {
      setDetailError(error instanceof Error ? error.message : "Không thể tải chi tiết lớp học")
      setStudents([])
    } finally {
      setLoadingDetail(false)
    }
  }

  const handlePhanCong = (classItem) => {
    setPhanCongClass(classItem)

    if (classItem?.giangVien) {
      // Lớp đã được phân công trước đó
      setSelectedGiangVienName(classItem.giangVien)
      setShowSuccessModal(true)
    } else {
      // Lớp chưa có giảng viên
      openAssignModal(classItem)
    }
  }

  const handleSavePhanCong = async () => {
    if (!phanCongClass || !selectedGiangVienId) return

    try {
      setAssignError(null)
      setLoadingLecturers(true)

      await apiClient.post(`/api/truong-khoa/lop-hoc/${phanCongClass.id}/assign-giang-vien`, {
        giangVienId: Number(selectedGiangVienId),
        overrideConflicts: false,
        overrideReason: null,
        actorNguoiDungId: null,
      })

      const selectedOption =
        lecturerOptions.find((x) => x.id === Number(selectedGiangVienId)) || null
      const newName = selectedOption?.label || selectedGiangVienName

      setSelectedGiangVienName(newName)

      // Cập nhật lại danh sách lớp để hiển thị giảng viên mới
      setClasses((prev) =>
        prev.map((c) => (c.id === phanCongClass.id ? { ...c, giangVien: newName } : c)),
      )

      setShowPhanCongModal(false)
      setShowSuccessModal(true)
    } catch (error) {
      setAssignError(error instanceof Error ? error.message : "Không thể phân công giảng viên")
    } finally {
      setLoadingLecturers(false)
    }
  }

  const handleOpenChangeInstructor = () => {
    if (!phanCongClass) return
    setShowSuccessModal(false)
    openAssignModal(phanCongClass)
  }

  const handleBack = () => {
    setCurrentView("list")
    setSelectedClass(null)
    setClassDetail(null)
    setStudents([])
  }

  const renderStudentStatus = (tongDiem) => {
    if (tongDiem == null)
      return { label: "Chưa có điểm", className: "bg-gray-100 text-gray-600 border border-gray-300" }
    if (tongDiem >= 8)
      return { label: "Đang học tốt", className: "bg-green-100 text-green-600 border border-green-300" }
    if (tongDiem >= 5)
      return { label: "Cảnh báo", className: "bg-yellow-100 text-yellow-600 border border-yellow-300" }
    return { label: "Nguy cơ trượt", className: "bg-red-100 text-red-600 border border-red-300" }
  }

  // Chi tiết lớp học
  if (currentView === "detail" && selectedClass) {
    const detail = classDetail || selectedClass

    return (
      <main className="flex-1 p-6">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-[#4A90D9] text-2xl font-bold">Quản lý lớp học</h1>
          <button
            onClick={handleBack}
            className="px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
          >
            Quay lại danh sách
          </button>
        </div>

        <div className="bg-white rounded-xl p-6 shadow-sm mb-6">
          {loadingDetail ? (
            <p className="text-sm text-gray-500">Đang tải chi tiết lớp học...</p>
          ) : detailError ? (
            <p className="text-sm text-red-500">{detailError}</p>
          ) : (
            <div className="grid grid-cols-5 gap-6">
              <div>
                <p className="text-gray-500 text-sm mb-1">Tên lớp</p>
                <p className="text-gray-800 font-medium">{detail?.tenLop ?? detail?.mon ?? "—"}</p>
              </div>
              <div>
                <p className="text-gray-500 text-sm mb-1">Giảng viên phụ trách</p>
                <p className="text-gray-800 font-medium">{detail?.giangVien || "Chưa phân công"}</p>
              </div>
              <div>
                <p className="text-gray-500 text-sm mb-1">Mã lớp</p>
                <p className="text-gray-800 font-medium">{detail?.maLop}</p>
              </div>
              <div>
                <p className="text-gray-500 text-sm mb-1">Số lượng sinh viên</p>
                <p className="text-gray-800 font-medium">
                  {detail?.totalStudents != null ? detail.totalStudents : students.length || "—"}
                </p>
              </div>
              <div>
                <p className="text-gray-500 text-sm mb-1">Học kì</p>
                <p className="text-gray-800 font-medium">{detail?.hocKy || "—"}</p>
              </div>
            </div>
          )}
        </div>

        <div className="bg-white rounded-xl p-6 shadow-sm">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-gray-800 font-semibold text-lg">Danh sách sinh viên</h2>
            <div className="px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700">
              Lịch: Chưa kết nối
            </div>
          </div>

          <div className="flex items-center justify-between mb-4">
            <div className="relative w-[300px]">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Tìm kiếm sinh viên..."
                className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:border-[#4A90D9]"
              />
            </div>
            <div className="relative">
              <select className="appearance-none px-4 py-2 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none">
                <option>Tất cả ngành</option>
              </select>
              <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            </div>
          </div>

          {loadingDetail ? (
            <p className="text-sm text-gray-500">Đang tải danh sách sinh viên...</p>
          ) : detailError ? (
            <p className="text-sm text-red-500">{detailError}</p>
          ) : (
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">MSSV</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Họ tên</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Khóa</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Ngành</th>
                  <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">Tổng điểm</th>
                  <th className="text-center py-3 px-4 text-sm font-medium text-gray-500">Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                {students.map((sv, idx) => {
                  const status = renderStudentStatus(sv.tongDiem)
                  return (
                    <tr key={idx} className="border-b border-gray-100">
                      <td className="py-3 px-4 text-sm text-gray-700">{sv.mssv}</td>
                      <td className="py-3 px-4 text-sm text-gray-700">{sv.hoTen}</td>
                      <td className="py-3 px-4 text-sm text-gray-700">—</td>
                      <td className="py-3 px-4 text-sm text-gray-700">{sv.nganh}</td>
                      <td className="py-3 px-4 text-sm text-center">
                        <span
                          className={
                            sv.tongDiem >= 8
                              ? "text-green-500"
                              : sv.tongDiem >= 5
                                ? "text-yellow-500"
                                : sv.tongDiem != null
                                  ? "text-red-500"
                                  : "text-gray-400"
                          }
                        >
                          {sv.tongDiem != null ? sv.tongDiem : "-"}
                        </span>
                      </td>
                      <td className="py-3 px-4 text-center">
                        <span className={`px-3 py-1 rounded text-xs font-medium ${status.className}`}>
                          {status.label}
                        </span>
                      </td>
                    </tr>
                  )
                })}
                {students.length === 0 && (
                  <tr>
                    <td colSpan={6} className="py-4 text-center text-sm text-gray-500">
                      Chưa có sinh viên trong lớp này.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>
      </main>
    )
  }

  // Danh sách lớp học
  return (
    <main className="flex-1 p-6">
      <h1 className="text-[#4A90D9] text-2xl font-bold mb-6">Quản lý lớp học</h1>

      <div className="bg-white rounded-xl p-4 shadow-sm mb-6">
        <div className="flex items-center gap-4">
          {/* Khóa (hiển thị từ options/hoc-ky, hiện chưa filter backend) */}
          <div className="relative flex-1 max-w-[180px]">
            <select
              value={selectedHocKyId}
              onChange={(e) => setSelectedHocKyId(e.target.value)}
              className="w-full appearance-none px-4 py-2.5 pr-10 bg-[#4A5568] text-white rounded-lg text-sm"
            >
              <option value="">Tất cả khóa</option>
              {hocKyOptions.map((hk) => (
                <option key={hk.id} value={hk.id}>
                  {hk.label}
                </option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white pointer-events-none" />
          </div>

          {/* Khoa */}
          <div className="relative flex-1 max-w-[180px]">
            <select
              value={selectedKhoaId}
              onChange={(e) => setSelectedKhoaId(e.target.value)}
              className="w-full appearance-none px-4 py-2.5 pr-10 bg-[#4A5568] text-white rounded-lg text-sm"
            >
              <option value="">Tất cả khoa</option>
              {khoaOptions.map((khoa) => (
                <option key={khoa.id} value={khoa.id}>
                  {khoa.label}
                </option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white pointer-events-none" />
          </div>

          {/* Ngành */}
          <div className="relative flex-1 max-w-[180px]">
            <select
              value={selectedNganhId}
              onChange={(e) => setSelectedNganhId(e.target.value)}
              className="w-full appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white"
            >
              <option value="">Tất cả ngành</option>
              {nganhOptions.map((nganh) => (
                <option key={nganh.id} value={nganh.id}>
                  {nganh.label}
                </option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          </div>

          {/* Môn học */}
          <div className="relative flex-1 max-w-[180px]">
            <select
              value={selectedMonId}
              onChange={(e) => setSelectedMonId(e.target.value)}
              className="w-full appearance-none px-4 py-2.5 pr-10 bg-[#4A5568] text-white rounded-lg text-sm"
            >
              <option value="">Tất cả môn</option>
              {monOptions.map((mon) => (
                <option key={mon.id} value={mon.id}>
                  {mon.label}
                </option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white pointer-events-none" />
          </div>

          {/* Tìm kiếm */}
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value)
                setCurrentPage(1)
              }}
              placeholder="Tìm kiếm lớp học..."
              className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg text-sm"
            />
          </div>
        </div>
        {optionsError && (
          <p className="mt-2 text-xs text-red-500">{optionsError}</p>
        )}
      </div>

      <div className="bg-white rounded-xl p-6 shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-gray-800 font-semibold text-lg">Danh sách lớp học</h2>
          {loadingClasses && <p className="text-sm text-gray-500">Đang tải...</p>}
          {!loadingClasses && classesError && <p className="text-sm text-red-500">{classesError}</p>}
        </div>

        <table className="w-full">
          <thead>
            <tr className="border-b border-gray-200">
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Mã lớp</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Tên lớp</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Môn</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Khoa</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Ngành</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Giảng viên</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Sinh viên</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Trạng thái</th>
              <th className="text-left py-3 px-2 text-sm font-medium text-gray-500">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {classes.map((item) => (
              <tr key={item.id} className="border-b border-gray-100">
                <td className="py-3 px-2 text-sm text-gray-700">{item.maLop}</td>
                <td className="py-3 px-2 text-sm text-gray-700">{item.tenLop}</td>
                <td className="py-3 px-2 text-sm text-gray-700">{item.mon}</td>
                <td className="py-3 px-2 text-sm text-gray-700">{item.khoaId}</td>
                <td className="py-3 px-2 text-sm text-gray-700">{item.nganhId}</td>
                <td className="py-3 px-2 text-sm text-gray-700">{item.giangVien || "Chưa phân công"}</td>
                <td className="py-3 px-2 text-sm text-gray-700">{item.sinhVien ?? "-"}</td>
                <td className="py-3 px-2">
                  <span
                    className={`px-2 py-1 rounded text-xs font-medium border ${
                      item.trangThai === 1 ? "text-green-600 border-green-400" : "text-red-600 border-red-400"
                    }`}
                  >
                    {item.trangThai === 1 ? "Đang hoạt động" : "Ngừng hoạt động"}
                  </span>
                </td>
                <td className="py-3 px-2">
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => handlePhanCong(item)}
                      className="text-gray-500 hover:text-[#4A90D9]"
                      title={item.giangVien ? "Lớp này đã được phân công" : "Phân công giảng viên"}
                    >
                      {item.giangVien ? (
                        <UserCheck className="w-5 h-5" />
                      ) : (
                        <UserPlus className="w-5 h-5" />
                      )}
                    </button>
                    <button
                      onClick={() => handleViewDetail(item)}
                      className="text-gray-500 hover:text-[#4A90D9]"
                      title="Xem chi tiết lớp"
                    >
                      <Eye className="w-5 h-5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {!loadingClasses && !classesError && classes.length === 0 && (
              <tr>
                <td colSpan={9} className="py-4 text-center text-sm text-gray-500">
                  Không có lớp học nào.
                </td>
              </tr>
            )}
          </tbody>
        </table>

        <div className="flex items-center justify-end gap-2 mt-4">
          <button className="p-2 text-gray-400">
            <ChevronLeft className="w-4 h-4" />
          </button>
          <button className="w-8 h-8 bg-[#4A90D9] text-white rounded text-sm">1</button>
          <button className="w-8 h-8 text-gray-600 text-sm">2</button>
          <button className="w-8 h-8 text-gray-600 text-sm">3</button>
          <button className="w-8 h-8 text-gray-600 text-sm">4</button>
          <span className="text-gray-400">...</span>
          <button className="w-8 h-8 text-gray-600 text-sm">40</button>
          <button className="p-2 text-gray-400">
            <ChevronRight className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Modal Phân công giảng viên */}
      {showPhanCongModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-[450px]">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold">Phân công giảng viên</h3>
              <button onClick={() => setShowPhanCongModal(false)}>
                <X className="w-5 h-5 text-gray-400" />
              </button>
            </div>
            <p className="text-sm text-gray-500 mb-4">
              Chọn giảng viên phụ trách cho lớp {phanCongClass?.mon ?? phanCongClass?.tenLop}
            </p>
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">Giảng viên</label>
              <select
                value={selectedGiangVienId}
                onChange={(e) => setSelectedGiangVienId(e.target.value)}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm bg-gray-100"
              >
                <option value="">-- Chọn giảng viên --</option>
                {lecturerOptions.map((gv) => (
                  <option key={gv.id} value={gv.id}>
                    {gv.label}
                  </option>
                ))}
              </select>
            </div>
            {assignError && (
              <p className="text-sm text-red-500 mb-2">{assignError}</p>
            )}
            <div className="bg-blue-50 p-4 rounded-lg mb-6">
              <p className="text-sm text-gray-700">Lịch dạy: Thứ 2, 7:00-11:30</p>
              <p className="text-sm text-gray-700">Phòng: P.A01</p>
            </div>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowPhanCongModal(false)}
                className="px-6 py-2 border border-gray-300 rounded-lg text-sm"
              >
                Hủy
              </button>
              <button
                onClick={handleSavePhanCong}
                disabled={loadingLecturers || !selectedGiangVienId}
                className="px-6 py-2 bg-[#1E293B] text-white rounded-lg text-sm disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loadingLecturers ? "Đang lưu..." : "Lưu"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal Success */}
      {showSuccessModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-[#1E293B] rounded-xl p-6 w-[450px] text-white">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-6 h-6 bg-green-500 rounded-full flex items-center justify-center">
                <Check className="w-4 h-4" />
              </div>
              <h3 className="text-lg font-semibold">Lớp này đã được phân công</h3>
            </div>
            <p className="text-gray-300 text-sm mb-6">
              Lớp học này đã được phân công cho giảng viên{" "}
              {selectedGiangVienName || phanCongClass?.giangVien}.
            </p>
            <div className="flex justify-between">
              <button
                onClick={handleOpenChangeInstructor}
                className="px-6 py-2 bg-[#F5A623] text-white rounded-lg text-sm font-medium"
              >
                Đổi giảng viên
              </button>
              <button
                onClick={() => setShowSuccessModal(false)}
                className="px-6 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </main>
  )
}
