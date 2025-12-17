"use client"

import { useState, useMemo, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Search, ChevronDown, Eye, Calendar, ChevronLeft, ChevronRight } from "lucide-react"
import { apiClient } from "@/lib/api"

export default function GiangVienPage() {
  const router = useRouter()
  const [searchTerm, setSearchTerm] = useState("")
  const [selectedNganh, setSelectedNganh] = useState("Tất cả ngành")
  const [selectedMon, setSelectedMon] = useState("Tất cả môn")
  const [selectedTrangThai, setSelectedTrangThai] = useState("Tất cả trạng thái")
  const [currentPage, setCurrentPage] = useState(1)
  const [giangVienData, setGiangVienData] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const itemsPerPage = 8

  useEffect(() => {
    const fetchGiangVien = async () => {
      try {
        setIsLoading(true)
        const params = new URLSearchParams()
        if (searchTerm) params.append("q", searchTerm)
        if (selectedNganh !== "Tất cả ngành") {
          const nganhId = parseInt(selectedNganh)
          if (!isNaN(nganhId)) params.append("nganhId", nganhId)
        }
        if (selectedMon !== "Tất cả môn") params.append("monHoc", selectedMon)
        if (selectedTrangThai === "Đang hoạt động") params.append("trangThai", "hoat-dong")
        if (selectedTrangThai === "Không hoạt động") params.append("trangThai", "khong-hoat-dong")

        const queryString = params.toString()
        // Dùng endpoint options giảng viên của Trưởng khoa
        const url = `/api/truong-khoa/options/giang-vien${queryString ? `?${queryString}` : ""}`
        const data = await apiClient.get(url)

        // API trả { id, label }, map sang cấu trúc dùng cho UI
        const giangVienList =
          Array.isArray(data) && data.length > 0
            ? data.map((item) => ({
                id: item.id,
                ten: item.label,
                nganh: "",
                monPhuTrach: [],
                soLopDangDay: 0,
                trangThai: "Hoạt động",
              }))
            : []

        // Lấy danh sách lớp học cho từng giảng viên để lấy ngành và môn phụ trách
        const giangVienLopMap = new Map()

        // Lấy danh sách lớp học cho từng giảng viên (song song để tối ưu)
        await Promise.allSettled(
          giangVienList.map(async (gv) => {
            try {
              const lopHocParams = new URLSearchParams()
              lopHocParams.append("GiangVienId", gv.id)
              const lopHocData = await apiClient.get(
                `/api/truong-khoa/options/lop-hoc?${lopHocParams.toString()}`
              )
              const lopHocList = Array.isArray(lopHocData) ? lopHocData : []
              giangVienLopMap.set(gv.id, lopHocList)
            } catch (error) {
              console.warn(`Không thể lấy lớp học cho giảng viên ${gv.id}:`, error)
              giangVienLopMap.set(gv.id, [])
            }
          })
        )

        // Lấy chi tiết lớp học để lấy ngành (chỉ lấy các lớp unique để tránh duplicate calls)
        const uniqueLopIds = new Set()
        giangVienLopMap.forEach((lopList) => {
          lopList.forEach((lop) => {
            if (lop.id) uniqueLopIds.add(lop.id)
          })
        })

        // Fetch chi tiết lớp học để lấy ngành (batch để tối ưu, giới hạn 50 lớp để tránh quá nhiều requests)
        const lopDetailMap = new Map()
        const lopIdsArray = Array.from(uniqueLopIds).slice(0, 50)
        await Promise.allSettled(
          lopIdsArray.map(async (lopId) => {
            try {
              const lopDetail = await apiClient.get(`/api/truong-khoa/lop-hoc/${lopId}`)
              if (lopDetail) {
                lopDetailMap.set(lopId, {
                  nganh: lopDetail.nganh || null,
                })
              }
            } catch (error) {
              // Ignore errors for individual class details
            }
          })
        )

        // Map thông tin ngành và môn phụ trách cho từng giảng viên
        const giangVienWithDetails = giangVienList.map((gv) => {
          const lopHocList = giangVienLopMap.get(gv.id) || []
          const nganhSet = new Set()
          const monHocSet = new Set()

          lopHocList.forEach((lop) => {
            // Parse môn học từ label: "TenLop (MaLop) — TenMon"
            const labelParts = (lop.label || "").split(" — ")
            if (labelParts.length > 1) {
              const tenMon = labelParts[1].trim()
              if (tenMon) monHocSet.add(tenMon)
            }

            // Lấy ngành từ chi tiết lớp học
            if (lop.id && lopDetailMap.has(lop.id)) {
              const nganh = lopDetailMap.get(lop.id).nganh
              if (nganh) nganhSet.add(nganh)
            }
          })

          return {
            ...gv,
            nganh: Array.from(nganhSet).join(", ") || "N/A",
            monPhuTrach: Array.from(monHocSet),
            soLopDangDay: lopHocList.length,
          }
        })

        setGiangVienData(giangVienWithDetails)
      } catch (error) {
        console.error("Lỗi khi tải danh sách giảng viên:", error)
        setGiangVienData([])
      } finally {
        setIsLoading(false)
      }
    }

    fetchGiangVien()
  }, [searchTerm, selectedNganh, selectedMon, selectedTrangThai])

  const nganhList = useMemo(() => {
    const nganhs = new Set()
    giangVienData.forEach((gv) => {
      if (gv.nganh) {
        gv.nganh.split(", ").forEach((n) => nganhs.add(n))
      }
    })
    return Array.from(nganhs)
  }, [giangVienData])

  const monHocList = useMemo(() => {
    const mons = new Set()
    giangVienData.forEach((gv) => {
      if (Array.isArray(gv.monPhuTrach)) {
        gv.monPhuTrach.forEach((m) => mons.add(m))
      }
    })
    return Array.from(mons)
  }, [giangVienData])

  const filteredData = useMemo(() => {
    return giangVienData.filter((gv) => {
      const matchesSearch =
        searchTerm === "" ||
        (gv.ten || "").toLowerCase().includes(searchTerm.toLowerCase())

      const matchesNganh = selectedNganh === "Tất cả ngành" || (gv.nganh || "").includes(selectedNganh)

      const matchesMon =
        selectedMon === "Tất cả môn" ||
        (Array.isArray(gv.monPhuTrach) && gv.monPhuTrach.includes(selectedMon))

      const matchesTrangThai =
        selectedTrangThai === "Tất cả trạng thái" ||
        gv.trangThai === selectedTrangThai

      return matchesSearch && matchesNganh && matchesMon && matchesTrangThai
    })
  }, [giangVienData, searchTerm, selectedNganh, selectedMon, selectedTrangThai])

  // Tính toán phân trang
  const totalPages = Math.ceil(filteredData.length / itemsPerPage)
  const startIndex = (currentPage - 1) * itemsPerPage
  const endIndex = startIndex + itemsPerPage
  const paginatedData = filteredData.slice(startIndex, endIndex)

  const totalGiangVien = giangVienData.length
  const totalLop = giangVienData.reduce((sum, gv) => sum + (gv.soLopDangDay || 0), 0)

  const handleViewDetail = (id) => {
    router.push(`/giangvien/${id}`)
  }

  const handleViewSchedule = (id) => {
    router.push(`/lichday?giangVienId=${id}`)
  }

  const handlePageChange = (page) => {
    setCurrentPage(page)
    window.scrollTo({ top: 0, behavior: "smooth" })
  }

  // Tạo danh sách số trang
  const getPageNumbers = () => {
    const pages = []
    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i)
      }
    } else {
      if (currentPage <= 3) {
        for (let i = 1; i <= 4; i++) {
          pages.push(i)
        }
        pages.push("...")
        pages.push(totalPages)
      } else if (currentPage >= totalPages - 2) {
        pages.push(1)
        pages.push("...")
        for (let i = totalPages - 3; i <= totalPages; i++) {
          pages.push(i)
        }
      } else {
        pages.push(1)
        pages.push("...")
        for (let i = currentPage - 1; i <= currentPage + 1; i++) {
          pages.push(i)
        }
        pages.push("...")
        pages.push(totalPages)
      }
    }
    return pages
  }

  return (
    <main className="flex-1 p-6">
      <h1 className="text-[#4A90D9] text-2xl font-bold mb-6">Quản lý giảng viên</h1>

      {/* Thống kê */}
      <div className="grid grid-cols-2 gap-4 mb-6">
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#4A90D9] shadow-sm">
          <p className="text-[#4A90D9] text-sm font-medium mb-1">Tổng số giảng viên</p>
          <p className="text-[#4A90D9] text-2xl font-bold">{totalGiangVien}</p>
        </div>
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#EF4444] shadow-sm">
          <p className="text-[#EF4444] text-sm font-medium mb-1">Tổng lớp</p>
          <p className="text-[#EF4444] text-2xl font-bold">{totalLop}</p>
        </div>
      </div>

      {/* Tìm kiếm và lọc */}
      <div className="flex items-center gap-4 mb-6">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Tìm giảng viên..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value)
              setCurrentPage(1)
            }}
            className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
          />
        </div>
        <div className="relative">
          <select
            value={selectedNganh}
            onChange={(e) => {
              setSelectedNganh(e.target.value)
              setCurrentPage(1)
            }}
            className="appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
          >
            <option>Tất cả ngành</option>
            {nganhList.map((nganh) => (
              <option key={nganh} value={nganh}>
                {nganh}
              </option>
            ))}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        </div>
        <div className="relative">
          <select
            value={selectedMon}
            onChange={(e) => {
              setSelectedMon(e.target.value)
              setCurrentPage(1)
            }}
            className="appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
          >
            <option>Tất cả môn</option>
            {monHocList.map((mon) => (
              <option key={mon} value={mon}>
                {mon}
              </option>
            ))}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        </div>
        <div className="relative">
          <select
            value={selectedTrangThai}
            onChange={(e) => {
              setSelectedTrangThai(e.target.value)
              setCurrentPage(1)
            }}
            className="appearance-none px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
          >
            <option>Tất cả trạng thái</option>
            <option>Đang hoạt động</option>
            <option>Không hoạt động</option>
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        </div>
      </div>

      {/* Bảng danh sách */}
      <div className="bg-white rounded-xl p-6 shadow-sm">
        <h2 className="text-gray-800 font-semibold text-lg mb-4">Danh sách giảng viên</h2>
        {paginatedData.length === 0 ? (
          <div className="text-center py-12 text-gray-500">
            <p>Không tìm thấy giảng viên nào phù hợp với điều kiện tìm kiếm.</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200">
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Giảng viên</th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Ngành</th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Môn phụ trách</th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Số lớp đang dạy</th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Trạng thái</th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-500">Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {isLoading ? (
                    <tr>
                      <td colSpan={6} className="py-8 text-center text-gray-500">
                        Đang tải...
                      </td>
                    </tr>
                  ) : paginatedData.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="py-8 text-center text-gray-500">
                        Không tìm thấy giảng viên nào
                      </td>
                    </tr>
                  ) : (
                    paginatedData.map((gv) => (
                      <tr key={gv.id} className="border-b border-gray-100 hover:bg-gray-50">
                        <td className="py-4 px-4 text-sm text-gray-700 font-medium">{gv.ten || "N/A"}</td>
                        <td className="py-4 px-4 text-sm text-gray-700">{gv.nganh || "N/A"}</td>
                        <td className="py-4 px-4 text-sm text-gray-700">
                          {Array.isArray(gv.monPhuTrach) ? gv.monPhuTrach.join(", ") : gv.monPhuTrach || "N/A"}
                        </td>
                        <td className="py-4 px-4 text-sm text-gray-700">{gv.soLopDangDay || 0} lớp</td>
                        <td className="py-4 px-4">
                          <span
                            className={`inline-block px-3 py-1 rounded-full text-xs font-medium ${
                              gv.trangThai === "Hoạt động"
                                ? "bg-green-100 text-green-600"
                                : "bg-red-100 text-red-600"
                            }`}
                          >
                            {gv.trangThai || "N/A"}
                          </span>
                        </td>
                        <td className="py-4 px-4">
                          <div className="flex items-center gap-3">
                            <button
                              onClick={() => handleViewSchedule(gv.id)}
                              className="text-gray-500 hover:text-[#4A90D9] transition-colors"
                              title="Xem lịch dạy"
                            >
                              <Calendar className="w-5 h-5" />
                            </button>
                            <button
                              onClick={() => handleViewDetail(gv.id)}
                              className="text-gray-500 hover:text-[#4A90D9] transition-colors"
                              title="Xem chi tiết"
                            >
                              <Eye className="w-5 h-5" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            {/* Phân trang */}
            {totalPages > 1 && (
              <div className="flex items-center justify-between mt-6">
                <p className="text-sm text-gray-500">
                  Hiển thị {startIndex + 1}-{Math.min(endIndex, filteredData.length)} của {filteredData.length} mục
                </p>
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => handlePageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                    className={`p-2 rounded ${
                      currentPage === 1 ? "text-gray-300 cursor-not-allowed" : "text-gray-600 hover:bg-gray-100"
                    }`}
                  >
                    <ChevronLeft className="w-4 h-4" />
                  </button>
                  {getPageNumbers().map((page, idx) => (
                    <button
                      key={idx}
                      onClick={() => typeof page === "number" && handlePageChange(page)}
                      disabled={typeof page !== "number"}
                      className={`w-8 h-8 rounded text-sm ${
                        page === currentPage
                          ? "bg-[#4A90D9] text-white"
                          : typeof page === "number"
                            ? "text-gray-600 hover:bg-gray-100"
                            : "text-gray-400 cursor-default"
                      }`}
                    >
                      {page}
                    </button>
                  ))}
                  <button
                    onClick={() => handlePageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                    className={`p-2 rounded ${
                      currentPage === totalPages
                        ? "text-gray-300 cursor-not-allowed"
                        : "text-gray-600 hover:bg-gray-100"
                    }`}
                  >
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </main>
  )
}
