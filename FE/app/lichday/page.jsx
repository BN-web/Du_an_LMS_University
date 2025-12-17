"use client"

import { useState, useEffect, useMemo } from "react"
import { useSearchParams } from "next/navigation"
import {
  ChevronLeft,
  ChevronRight,
  Clock,
  MapPin,
  User,
  Filter,
  Edit,
  Trash2,
  Upload,
  Plus,
  X,
  Calendar as CalendarIcon,
  AlertTriangle,
} from "lucide-react"
import { Calendar } from "@/components/ui/calendar"
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { apiClient } from "@/lib/api"
import { authUtils } from "@/lib/auth"
import { checkScheduleConflict } from "@/lib/schedule-utils"

// Hàm chuyển đổi thứ trong tuần
const getDayOfWeek = (date) => {
  const days = ["Chủ nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7"]
  return days[date.getDay()]
}

export default function LichDayPage() {
  const searchParams = useSearchParams()
  const giangVienIdParam = searchParams?.get("giangVienId")

  const [activeTab, setActiveTab] = useState("thoikhoabieu") // "thoikhoabieu" hoặc "lichthi"
  const [selectedDate, setSelectedDate] = useState(new Date())
  const [selectedGiangVien, setSelectedGiangVien] = useState(giangVienIdParam || "Tất cả")
  const [selectedLop, setSelectedLop] = useState("Tất cả")
  const [selectedPhong, setSelectedPhong] = useState("Tất cả")

  // State cho modal tạo lịch
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isCreateThiModalOpen, setIsCreateThiModalOpen] = useState(false)
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [editingSchedule, setEditingSchedule] = useState(null)

  const [thiFormData, setThiFormData] = useState({
    monThi: "",
    lopHoc: "",
    ngayThi: null,
    gioBatDau: "09:00 SA",
    gioKetThuc: "11:00 SA",
    giamThi: "",
    loaiKiemTra: "Giữa kì",
    coSo: "Cơ sở 1",
    toaNha: "Toàn nhà A",
    phong: "",
  })

  const [lichDayData, setLichDayData] = useState([])
  const [lichThiData, setLichThiData] = useState([])
  const [giangVienData, setGiangVienData] = useState([])
  const [lopOptions, setLopOptions] = useState([])
  const [phongOptions, setPhongOptions] = useState([])
  const [isLoading, setIsLoading] = useState(true)

  const [formData, setFormData] = useState({
    lopHoc: "",
    giangVienId: "",
    hinhThucHoc: "Trực tiếp",
    batDauTuNgay: null,
    denNgay: null,
    gioBatDau: "09:00 AM",
    gioKetThuc: "10:30 AM",
    coSo: "Cơ sở 1",
    toaNha: "Toàn nhà A",
    phong: "",
  })

  const [editFormData, setEditFormData] = useState({
    lopHoc: "",
    hinhThucHoc: "Trực tiếp",
    batDauTuNgay: null,
    denNgay: null,
    gioBatDau: "09:00 AM",
    gioKetThuc: "10:30 AM",
    coSo: "Cơ sở 1",
    toaNha: "Toàn nhà A",
    phong: "Phòng 205",
  })

  const [conflictWarning, setConflictWarning] = useState(null)
  const [editConflictWarning, setEditConflictWarning] = useState(null)
  const [startDateOpen, setStartDateOpen] = useState(false)
  const [endDateOpen, setEndDateOpen] = useState(false)
  const [editStartDateOpen, setEditStartDateOpen] = useState(false)
  const [editEndDateOpen, setEditEndDateOpen] = useState(false)
  const [apiTestResult, setApiTestResult] = useState(null)

  // Test API function
  const testLopHocAPI = async () => {
    try {
      setApiTestResult("Đang test API...")
      const result = await apiClient.get("/api/truong-khoa/options/lop-hoc")
      setApiTestResult(`✅ Thành công! Nhận được ${Array.isArray(result) ? result.length : 0} lớp học`)
      console.log("Test API result:", result)
    } catch (error) {
      setApiTestResult(`❌ Lỗi: ${error.message}`)
      console.error("Test API error:", error)
    }
  }

  // Function để fetch danh sách lịch dạy
  const fetchLichDayData = async () => {
    try {
      if (lopOptions.length === 0) {
        setLichDayData([])
        return
      }

      // Lấy tất cả buổi học từ các lớp
      const allBuoiHocPromises = lopOptions.map(async (lop) => {
        try {
          const buoiHocData = await apiClient.get(`/api/truong-khoa/lop-hoc/${lop.id}/buoi-hoc`)
          const buoiHocList = Array.isArray(buoiHocData) ? buoiHocData : (buoiHocData?.data || [])
          
          // Map dữ liệu về format mong đợi
          return buoiHocList.map((bh) => {
            const thoiGianBatDau = new Date(bh.thoiGianBatDau || bh.startTime)
            const thoiGianKetThuc = new Date(bh.thoiGianKetThuc || bh.endTime)
            
            // Format thời gian
            const formatTime12h = (date) => {
              const hour = date.getHours()
              const min = date.getMinutes()
              const period = hour >= 12 ? "PM" : "AM"
              const hour12 = hour === 0 ? 12 : hour > 12 ? hour - 12 : hour
              return `${hour12}:${String(min).padStart(2, "0")} ${period}`
            }
            
            const gioBatDau = formatTime12h(thoiGianBatDau)
            const gioKetThuc = formatTime12h(thoiGianKetThuc)
            
            // Parse tên môn từ label (format: "TenLop (MaLop) — TenMon")
            const labelParts = lop.label?.split(" — ") || []
            const tenMon = labelParts.length > 1 ? labelParts[1] : (labelParts[0] || "N/A")
            
            // Lấy thông tin giảng viên từ lớp học
            let giangVienName = "N/A"
            let giangVienIdFromLop = null
            
            // Thử lấy từ API lớp học chi tiết
            try {
              // Lưu giảng viên ID từ lớp học (nếu có trong lop object)
              // Hoặc fetch từ API lớp học chi tiết
            } catch (error) {
              // Ignore
            }

            return {
              id: bh.buoiHocId || bh.id,
              ngay: thoiGianBatDau,
              lop: lop.maLop || lop.label,
              monHoc: tenMon,
              phongHoc: bh.phongHoc || bh.phong || "N/A",
              phongHocId: bh.phongHocId || null, // Lưu ID phòng học
              giangVien: giangVienName,
              giangVienId: bh.giangVienId || null,
              lopHocId: lop.id, // Lưu ID lớp học để lấy giảng viên sau
              thoiGian: `${gioBatDau} - ${gioKetThuc}`,
              gioBatDau: `${String(thoiGianBatDau.getHours()).padStart(2, "0")}:${String(thoiGianBatDau.getMinutes()).padStart(2, "0")}`,
              gioKetThuc: `${String(thoiGianKetThuc.getHours()).padStart(2, "0")}:${String(thoiGianKetThuc.getMinutes()).padStart(2, "0")}`,
              hinhThucHoc: bh.ghiChu || "Trực tiếp",
            }
          })
        } catch (error) {
          console.warn(`Không thể lấy buổi học cho lớp ${lop.id}:`, error)
          return []
        }
      })

      const allBuoiHocResults = await Promise.allSettled(allBuoiHocPromises)
      let allLichDay = allBuoiHocResults
        .filter(result => result.status === 'fulfilled')
        .flatMap(result => result.value)

      // Lấy thông tin giảng viên từ các lớp học
      const uniqueLopIds = [...new Set(allLichDay.map(l => l.lopHocId).filter(Boolean))]
      const lopGiangVienMap = new Map()
      
      await Promise.allSettled(
        uniqueLopIds.map(async (lopId) => {
          try {
            const lopDetail = await apiClient.get(`/api/truong-khoa/lop-hoc/${lopId}`)
            if (lopDetail?.giangVien) {
              lopGiangVienMap.set(lopId, lopDetail.giangVien)
            }
          } catch (error) {
            // Ignore errors
          }
        })
      )

      // Map giảng viên vào lịch dạy
      allLichDay = allLichDay.map((lich) => {
        const giangVienName = lich.lopHocId && lopGiangVienMap.has(lich.lopHocId)
          ? lopGiangVienMap.get(lich.lopHocId)
          : (lich.giangVienId && giangVienData.find(gv => gv.id === lich.giangVienId)?.ten) || "N/A"
        
        return {
          ...lich,
          giangVien: giangVienName,
        }
      })

      setLichDayData(allLichDay)
    } catch (error) {
      console.error("Lỗi khi tải danh sách lịch dạy:", error)
      setLichDayData([])
    }
  }

  // Fetch lịch thi từ API
  const fetchLichThiData = async () => {
    try {
      if (!lopOptions || lopOptions.length === 0) {
        setLichThiData([])
        return
      }

      // Fetch lịch thi cho tất cả các lớp
      const allBuoiThiPromises = lopOptions.map(async (lop) => {
        try {
          const response = await apiClient.get(`/api/truong-khoa/lop-hoc/${lop.id}/buoi-thi`)
          const buoiThiArray = Array.isArray(response) ? response : (response?.data || [])

          return buoiThiArray.map((bt) => {
            const ngayThi = bt.ngayThi ? new Date(bt.ngayThi) : null
            if (!ngayThi) return null

            // Parse thời gian từ gioBatDau và gioKetThuc nếu có
            let thoiGian = ""
            if (bt.gioBatDau && bt.gioKetThuc) {
              const gioBatDau = new Date(bt.gioBatDau)
              const gioKetThuc = new Date(bt.gioKetThuc)
              const startTime = `${String(gioBatDau.getHours()).padStart(2, "0")}:${String(gioBatDau.getMinutes()).padStart(2, "0")}`
              const endTime = `${String(gioKetThuc.getHours()).padStart(2, "0")}:${String(gioKetThuc.getMinutes()).padStart(2, "0")}`
              thoiGian = `${startTime} - ${endTime}`
            } else {
              thoiGian = "Chưa có giờ"
            }

            // Lấy tên môn học từ lớp học
            let tenMon = ""
            try {
              const lopLabel = lop.label || ""
              // Parse từ label: "TenLop (MaLop) — TenMon"
              const parts = lopLabel.split("—")
              if (parts.length > 1) {
                tenMon = parts[parts.length - 1].trim()
              } else {
                tenMon = lopLabel
              }
            } catch (error) {
              tenMon = lop.label || "N/A"
            }

            // Lấy tên giám thị nếu có
            let giamThiName = "Chưa phân công"
            if (bt.giamThiId) {
              const giamThi = giangVienData.find(gv => gv.id === bt.giamThiId)
              if (giamThi) {
                giamThiName = giamThi.ten
              }
            }

            return {
              id: bt.buoiThiId || bt.id,
              ngay: ngayThi,
              lop: lop.maLop || lop.label,
              monHoc: tenMon,
              phongHoc: bt.phongHoc || bt.maPhong || "N/A",
              phongHocId: bt.phongHocId || null,
              giangVien: giamThiName, // Dùng cho giám thị
              giangVienId: bt.giamThiId || null,
              lopHocId: lop.id,
              thoiGian: thoiGian,
              gioBatDau: bt.gioBatDau ? new Date(bt.gioBatDau).toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" }) : "",
              gioKetThuc: bt.gioKetThuc ? new Date(bt.gioKetThuc).toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" }) : "",
              hinhThuc: bt.hinhThuc || "Trực tiếp",
              isLichThi: true, // Đánh dấu đây là lịch thi
            }
          }).filter(item => item != null)
        } catch (error) {
          console.warn(`Không thể lấy buổi thi cho lớp ${lop.id}:`, error)
          return []
        }
      })

      const allBuoiThiResults = await Promise.allSettled(allBuoiThiPromises)
      const allLichThi = allBuoiThiResults
        .filter(result => result.status === 'fulfilled')
        .flatMap(result => result.value)

      setLichThiData(allLichThi)
    } catch (error) {
      console.error("Lỗi khi tải danh sách lịch thi:", error)
      setLichThiData([])
    }
  }

  // Load danh sách giảng viên, lớp, phòng từ API options
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true)

        // Sử dụng Promise.allSettled để không bị dừng khi một API lỗi
        const [giangVienResult, lopResult, phongResult] = await Promise.allSettled([
          apiClient.get("/api/truong-khoa/options/giang-vien"),
          apiClient.get("/api/truong-khoa/options/lop-hoc"),
          apiClient.get("/api/truong-khoa/options/phong-hoc"),
        ])

        // Xử lý response - API trả về array trực tiếp hoặc có thể được wrap
        const extractArray = (result, apiName = "API") => {
          // Kiểm tra nếu là Promise rejection
          if (result.status === 'rejected') {
            const error = result.reason
            const errorDetails = {
              message: error?.message || "Unknown error",
              status: error?.status,
              url: error?.url,
              responseText: error?.responseText,
              data: error?.data,
            }
            console.error(`❌ ${apiName} call failed:`, errorDetails)
            
            // Hiển thị chi tiết lỗi 500
            if (error?.status === 500) {
              console.error(`🔍 Chi tiết lỗi 500 từ ${apiName}:`)
              if (error?.responseText) {
                console.error("Response text:", error.responseText.substring(0, 1000))
              }
              if (error?.data) {
                console.error("Error data:", error.data)
              }
            }
            
            return []
          }

          const data = result.value
          console.log(`${apiName} response:`, data)

          if (!data) {
            console.warn(`${apiName}: No data returned`)
            return []
          }

          if (Array.isArray(data)) {
            console.log(`${apiName}: Received array with ${data.length} items`)
            return data
          }
          // Nếu response được wrap trong object có property data
          if (data && Array.isArray(data.data)) {
            console.log(`${apiName}: Received wrapped data array with ${data.data.length} items`)
            return data.data
          }
          // Nếu response được wrap trong object có property Data
          if (data && Array.isArray(data.Data)) {
            console.log(`${apiName}: Received wrapped Data array with ${data.Data.length} items`)
            return data.Data
          }
          console.warn(`${apiName}: Unexpected data format:`, data)
          return []
        }

        const giangVienArray = extractArray(giangVienResult, "Giảng viên")
        const lopArray = extractArray(lopResult, "Lớp học")
        const phongArray = extractArray(phongResult, "Phòng học")

        // Map giảng viên với format đúng
        const mappedLecturers = giangVienArray.map((item) => ({
          id: item.id || item.Id || item.giangVienId,
          ten: item.label || item.Label || item.ten || item.Ten,
          trangThai: "Hoạt động",
        }))

        // Map lớp học - đảm bảo có id, label và mã lớp
        const mappedLopOptions = lopArray
          .filter((item) => item != null) // Lọc bỏ các item null/undefined
          .map((item) => {
            try {
              const id = item.id || item.Id || item.lopHocId
              const label = item.label || item.Label || item.tenLop || item.TenLop || ""
              
              // Parse mã lớp từ label (format: "TenLop (MaLop) — TenMon")
              // Hoặc lấy trực tiếp từ item nếu có
              let maLop = item.maLop || item.MaLop || ""
              
              // Nếu không có mã lớp trong item, parse từ label
              if (!maLop && label) {
                // Tìm pattern: "TenLop (MaLop) — TenMon" hoặc "TenLop (MaLop)"
                const match = label.match(/\(([^)]+)\)/)
                if (match && match[1]) {
                  maLop = match[1].trim()
                }
              }
              
              // Nếu vẫn không có mã lớp, dùng label hoặc tạo từ id
              if (!maLop) {
                // Thử tìm mã lớp trong label nếu có format khác
                if (label && label.length > 0) {
                  maLop = label
                } else {
                  maLop = id ? `LH${String(id).padStart(6, '0')}` : "N/A"
                }
              }
              
              return {
                id: id || null,
                label: label || `Lớp ${id}` || "N/A",
                maLop: maLop,
              }
            } catch (err) {
              console.warn("Lỗi khi map lớp học:", item, err)
              return null
            }
          })
          .filter((item) => item != null && item.id != null) // Lọc bỏ các item không hợp lệ

        // Map phòng học - đảm bảo có id và label
        const mappedPhongOptions = phongArray.map((item) => ({
          id: item.id || item.Id || item.phongHocId,
          label: item.label || item.Label || item.tenPhong || item.TenPhong,
        }))

        console.log("Dữ liệu đã load:", {
          giangVien: mappedLecturers.length,
          lopHoc: mappedLopOptions.length,
          phongHoc: mappedPhongOptions.length,
        })
        
        // Log chi tiết nếu có lỗi với lớp học
        if (lopResult.status === 'rejected') {
          const error = lopResult.reason
          console.group("❌ Lỗi API lớp học")
          console.error("Message:", error?.message)
          console.error("Status:", error?.status)
          console.error("URL:", error?.url)
          if (error?.responseText) {
            console.error("Response Text (full):", error.responseText)
          }
          if (error?.data) {
            console.error("Error Data:", error.data)
          }
          if (error?.stack) {
            console.error("Stack:", error.stack)
          }
          console.groupEnd()
          
          // Hiển thị hướng dẫn nếu là lỗi SSL/CORS
          if (error?.message?.includes("Failed to fetch") || error?.message?.includes("Network")) {
            console.warn(`
⚠️ Có thể do:
1. SSL Certificate: Mở https://localhost:7133/swagger trong trình duyệt và chấp nhận certificate
2. CORS: Kiểm tra backend có chạy và CORS đã được cấu hình
3. Server chưa chạy: Đảm bảo backend đang chạy tại https://localhost:7133

Để test API trực tiếp, mở: https://localhost:7133/api/truong-khoa/options/lop-hoc
            `)
          } else if (error?.status === 500) {
            console.warn(`
⚠️ Lỗi 500 từ server - Có thể do:
1. Database connection issue - Kiểm tra connection string trong appsettings.json
2. SQL query error - Kiểm tra logs trong backend console
3. Null reference - Kiểm tra dữ liệu trong database

Vui lòng kiểm tra backend logs để xem chi tiết lỗi.
            `)
          }
        } else if (mappedLopOptions.length === 0 && lopArray.length > 0) {
          console.warn("⚠️ Không thể map lớp học, dữ liệu gốc:", lopArray)
        } else if (mappedLopOptions.length > 0) {
          // Log mẫu dữ liệu để debug
          console.log("✅ Mẫu dữ liệu lớp học:", mappedLopOptions.slice(0, 3))
        }

        setGiangVienData(mappedLecturers)
        setLopOptions(mappedLopOptions)
        setPhongOptions(mappedPhongOptions)

        // Hiển thị cảnh báo nếu có API nào lỗi
        const errors = []
        if (giangVienResult.status === 'rejected') errors.push("giảng viên")
        if (lopResult.status === 'rejected') errors.push("lớp học")
        if (phongResult.status === 'rejected') errors.push("phòng học")
        
        if (errors.length > 0) {
          console.warn(`Không thể tải dữ liệu cho: ${errors.join(", ")}`)
        }
      } catch (error) {
        console.error("Lỗi khi tải dữ liệu:", error)
        // Không hiển thị alert để tránh làm phiền user nếu chỉ một API lỗi
        // Chỉ log để debug
        setGiangVienData([])
        setLichDayData([])
        setLopOptions([])
        setPhongOptions([])
      } finally {
        setIsLoading(false)
      }
    }

    fetchData()
  }, [giangVienIdParam])

  // Load danh sách lịch dạy khi có lopOptions
  useEffect(() => {
    if (lopOptions.length > 0) {
      fetchLichDayData()
      fetchLichThiData()
    } else {
      setLichDayData([])
      setLichThiData([])
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lopOptions])

  useEffect(() => {
    if (giangVienIdParam) {
      setSelectedGiangVien(giangVienIdParam)
    }
  }, [giangVienIdParam])

  // Lấy danh sách lớp học từ options (hiển thị theo mã lớp)
  const lopList = useMemo(() => {
    // Sử dụng lopOptions để lấy danh sách mã lớp
    return lopOptions.map((lop) => lop.maLop || lop.label)
  }, [lopOptions])

  const phongList = useMemo(() => {
    // Lấy phòng từ cả lịch dạy và lịch thi
    const phongsFromDay = lichDayData.map((lich) => lich.phongHoc)
    const phongsFromThi = lichThiData.map((lich) => lich.phongHoc)
    const uniquePhongs = [...new Set([...phongsFromDay, ...phongsFromThi])]
    return uniquePhongs
  }, [lichDayData, lichThiData])

  // Lọc lịch dạy theo các tiêu chí
  const filteredLichDay = useMemo(() => {
    return lichDayData.filter((lich) => {
      if (selectedGiangVien !== "Tất cả" && lich.giangVienId !== selectedGiangVien) {
        return false
      }

      if (selectedLop !== "Tất cả" && lich.lop !== selectedLop) {
        return false
      }

      if (selectedPhong !== "Tất cả" && lich.phongHoc !== selectedPhong) {
        return false
      }

      const lichDate = new Date(lich.ngay)
      const selected = new Date(selectedDate)
      if (
        lichDate.getDate() !== selected.getDate() ||
        lichDate.getMonth() !== selected.getMonth() ||
        lichDate.getFullYear() !== selected.getFullYear()
      ) {
        return false
      }

      return true
    })
  }, [selectedDate, selectedGiangVien, selectedLop, selectedPhong, lichDayData])

  // Lọc lịch thi theo các tiêu chí
  const filteredLichThi = useMemo(() => {
    return lichThiData.filter((lich) => {
      // Lịch thi không filter theo giảng viên (vì có giám thị)
      // Nhưng có thể filter theo giám thị nếu cần
      if (selectedGiangVien !== "Tất cả" && lich.giangVienId !== selectedGiangVien) {
        return false
      }

      if (selectedLop !== "Tất cả" && lich.lop !== selectedLop) {
        return false
      }

      if (selectedPhong !== "Tất cả" && lich.phongHoc !== selectedPhong) {
        return false
      }

      const lichDate = new Date(lich.ngay)
      const selected = new Date(selectedDate)
      if (
        lichDate.getDate() !== selected.getDate() ||
        lichDate.getMonth() !== selected.getMonth() ||
        lichDate.getFullYear() !== selected.getFullYear()
      ) {
        return false
      }

      return true
    })
  }, [selectedDate, selectedGiangVien, selectedLop, selectedPhong, lichThiData])

  // Ngày có lịch trong tháng (bao gồm cả lịch dạy và lịch thi)
  const datesWithSchedule = useMemo(() => {
    const dates = new Set()
    lichDayData.forEach((lich) => {
      const date = new Date(lich.ngay)
      dates.add(date.toDateString())
    })
    lichThiData.forEach((lich) => {
      const date = new Date(lich.ngay)
      dates.add(date.toDateString())
    })
    return dates
  }, [lichDayData, lichThiData])

  // Các hàm util cho date/time
  const formatDateForInput = (date) => {
    if (!date) return ""
    const d = new Date(date)
    const month = String(d.getMonth() + 1).padStart(2, "0")
    const day = String(d.getDate()).padStart(2, "0")
    const year = d.getFullYear()
    return `${month}/${day}/${year}`
  }

  const parseDateFromInput = (dateString) => {
    if (!dateString) return null
    if (dateString instanceof Date) return dateString
    const [month, day, year] = dateString.split("/")
    return new Date(year, month - 1, day)
  }

  // Format date để gửi lên API (giữ nguyên local time, không chuyển sang UTC)
  // Format: YYYY-MM-DDTHH:mm:ss với timezone offset để đảm bảo ngày không bị lệch
  const formatDateForAPI = (date) => {
    if (!date) return null
    const d = new Date(date)
    const year = d.getFullYear()
    const month = String(d.getMonth() + 1).padStart(2, "0")
    const day = String(d.getDate()).padStart(2, "0")
    const hours = String(d.getHours()).padStart(2, "0")
    const minutes = String(d.getMinutes()).padStart(2, "0")
    const seconds = String(d.getSeconds()).padStart(2, "0")
    
    // Lấy timezone offset (ví dụ: +07:00)
    const offset = -d.getTimezoneOffset()
    const offsetHours = String(Math.floor(Math.abs(offset) / 60)).padStart(2, "0")
    const offsetMinutes = String(Math.abs(offset) % 60).padStart(2, "0")
    const offsetSign = offset >= 0 ? "+" : "-"
    
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}${offsetSign}${offsetHours}:${offsetMinutes}`
  }

  // Kiểm tra trùng lịch khi form tạo thay đổi (tạm thời chỉ hiển thị cảnh báo nếu dùng schedule-utils).
  useEffect(() => {
    if (!formData.batDauTuNgay || !formData.gioBatDau || !formData.gioKetThuc || !formData.lopHoc) {
      setConflictWarning(null)
      return
    }

    // Hiện tại chưa có dữ liệu lịch thực tế, nên tạm thời không chạy check phức tạp
    setConflictWarning(null)
  }, [formData.batDauTuNgay, formData.denNgay, formData.gioBatDau, formData.gioKetThuc, formData.lopHoc])

  const handleCreateSchedule = async () => {
    if (!formData.lopHoc || !formData.batDauTuNgay || !formData.phong) {
      alert("Vui lòng chọn lớp, ngày và phòng học")
      return
    }

    try {
      const startDate = parseDateFromInput(formData.batDauTuNgay)
      const endDate = parseDateFromInput(formData.denNgay) || startDate

      if (!startDate) {
        alert("Ngày bắt đầu không hợp lệ")
        return
      }

      const parseTime = (timeStr) => {
        if (!timeStr) return { hour: 9, min: 0 }
        const cleanTime = timeStr.replace(/AM|PM/i, "").trim()
        const [hour, min] = cleanTime.split(":")
        const isPM = timeStr.toUpperCase().includes("PM")
        let hour24 = parseInt(hour, 10) || 9
        const min24 = parseInt(min, 10) || 0
        if (isPM && hour24 !== 12) hour24 += 12
        if (!isPM && hour24 === 12) hour24 = 0
        return { hour: hour24, min: min24 }
      }

      const startTime = parseTime(formData.gioBatDau)
      const endTime = parseTime(formData.gioKetThuc)

      const thoiGianBatDau = new Date(startDate)
      thoiGianBatDau.setHours(startTime.hour, startTime.min, 0, 0)

      const thoiGianKetThuc = new Date(startDate)
      thoiGianKetThuc.setHours(endTime.hour, endTime.min, 0, 0)

      const lopHocId = parseInt(formData.lopHoc, 10)
      const phongHocId = parseInt(formData.phong, 10)

      if (isNaN(lopHocId) || lopHocId <= 0) {
        alert("Lớp học không hợp lệ")
        return
      }

      if (isNaN(phongHocId) || phongHocId <= 0) {
        alert("Phòng học không hợp lệ")
        return
      }

      const userId = authUtils.getUserId()
      const giangVienId = formData.giangVienId && formData.giangVienId !== "" 
        ? parseInt(formData.giangVienId, 10) 
        : null

      // Chuẩn bị request body
      const requestBody = {
        lopHocId: lopHocId,
        phongHocId: phongHocId,
        thoiGianBatDau: thoiGianBatDau.toISOString(),
        thoiGianKetThuc: thoiGianKetThuc.toISOString(),
        overrideConflicts: false,
        overrideReason: "",
      }

      // Chỉ thêm các trường optional nếu có giá trị
      if (giangVienId && giangVienId > 0) {
        requestBody.giangVienId = giangVienId
      }

      if (formData.hinhThucHoc && formData.hinhThucHoc.trim() !== "") {
        requestBody.ghiChu = formData.hinhThucHoc
      }

      if (userId) {
        const actorId = parseInt(userId, 10)
        if (!isNaN(actorId) && actorId > 0) {
          requestBody.actorNguoiDungId = actorId
        }
      }

      await apiClient.post("/api/truong-khoa/lich-day", requestBody)

      alert("Tạo lịch dạy thành công")
      setFormData({
        lopHoc: "",
        giangVienId: "",
        hinhThucHoc: "Trực tiếp",
        batDauTuNgay: null,
        denNgay: null,
        gioBatDau: "09:00 AM",
        gioKetThuc: "10:30 AM",
        coSo: "Cơ sở 1",
        toaNha: "Toàn nhà A",
        phong: "",
      })
      setConflictWarning(null)
      setIsCreateModalOpen(false)

      // Reload danh sách lịch dạy
      await fetchLichDayData()
      setSelectedDate(startDate)
    } catch (error) {
      console.error("Lỗi khi tạo lịch dạy:", error)
      alert(error.message || "Có lỗi xảy ra khi tạo lịch dạy")
    }
  }

  const handleEditSchedule = (schedule) => {
    setEditingSchedule(schedule)

    // Parse tên phòng để lấy cơ sở, tòa nhà, phòng
    const phongParts = (schedule.phongHoc || "Phòng 205").split(" ")
    let coSo = "Cơ sở 1"
    let toaNha = "Toàn nhà A"
    let phong = "Phòng 205"

    if (phongParts.length >= 3) {
      phong = phongParts[0]
      toaNha = phongParts[1]
      coSo = phongParts.slice(2).join(" ")
    } else if (phongParts.length === 2) {
      phong = phongParts[0]
      toaNha = phongParts[1]
    } else if (phongParts.length === 1) {
      phong = phongParts[0]
    }

    const formatTime12h = (time24) => {
      if (!time24) return "09:00 AM"
      const [hour, min] = time24.split(":")
      const hourNum = parseInt(hour)
      const period = hourNum >= 12 ? "PM" : "AM"
      const hour12 = hourNum === 0 ? 12 : hourNum > 12 ? hourNum - 12 : hourNum
      return `${hour12}:${min} ${period}`
    }

    setEditFormData({
      lopHoc: schedule.lop,
      hinhThucHoc: schedule.hinhThucHoc || "Trực tiếp",
      batDauTuNgay: formatDateForInput(new Date(schedule.ngay)),
      denNgay: formatDateForInput(new Date(schedule.ngay)),
      gioBatDau: formatTime12h(schedule.gioBatDau),
      gioKetThuc: formatTime12h(schedule.gioKetThuc),
      coSo,
      toaNha,
      phong,
      phongHocId: schedule.phongHocId || null, // Lưu ID phòng học
    })
    setIsEditModalOpen(true)
  }

  // Đơn giản hoá: không check trùng lịch cho form sửa khi chưa có nguồn dữ liệu thực tế
  useEffect(() => {
    setEditConflictWarning(null)
  }, [editFormData.batDauTuNgay, editFormData.denNgay, editFormData.gioBatDau, editFormData.gioKetThuc, editFormData.lopHoc])

  const handleUpdateSchedule = async () => {
    if (!editFormData.batDauTuNgay || !editFormData.phong || !editingSchedule) {
      alert("Vui lòng điền đầy đủ thông tin")
      return
    }

    try {
      const startDate = parseDateFromInput(editFormData.batDauTuNgay)
      if (!startDate) {
        alert("Ngày bắt đầu không hợp lệ")
        return
      }

      const parseTime = (timeStr) => {
        if (!timeStr) return { hour: 9, min: 0 }
        const cleanTime = timeStr.replace(/AM|PM/i, "").trim()
        const [hour, min] = cleanTime.split(":")
        const isPM = timeStr.toUpperCase().includes("PM")
        let hour24 = parseInt(hour, 10) || 9
        const min24 = parseInt(min, 10) || 0
        if (isPM && hour24 !== 12) hour24 += 12
        if (!isPM && hour24 === 12) hour24 = 0
        return { hour: hour24, min: min24 }
      }

      const startTime = parseTime(editFormData.gioBatDau)
      const endTime = parseTime(editFormData.gioKetThuc)

      const thoiGianBatDau = new Date(startDate)
      thoiGianBatDau.setHours(startTime.hour, startTime.min, 0, 0)

      const thoiGianKetThuc = new Date(startDate)
      thoiGianKetThuc.setHours(endTime.hour, endTime.min, 0, 0)

      // Lấy phòng học ID
      let phongHocId = null
      
      // Ưu tiên lấy từ editFormData.phongHocId (đã lưu khi mở modal)
      if (editFormData.phongHocId) {
        phongHocId = editFormData.phongHocId
      } else if (editingSchedule?.phongHocId) {
        // Lấy từ editingSchedule
        phongHocId = editingSchedule.phongHocId
      } else {
        // Tìm phòng học từ tên phòng trong phongOptions
        const fullPhongName = `${editFormData.coSo} - ${editFormData.toaNha} - ${editFormData.phong}`
        const phongOption = phongOptions.find(
          (p) => p.label === fullPhongName || 
                 p.label === editFormData.phong ||
                 (p.label && p.label.includes(editFormData.phong))
        )
        
        if (phongOption && phongOption.id) {
          phongHocId = phongOption.id
        } else {
          // Thử parse trực tiếp nếu là số
          const parsedId = parseInt(editFormData.phong, 10)
          if (!isNaN(parsedId) && parsedId > 0) {
            phongHocId = parsedId
          } else {
            alert("Phòng học không hợp lệ. Vui lòng chọn lại phòng học từ danh sách.")
            return
          }
        }
      }

      if (!phongHocId || phongHocId <= 0) {
        alert("Phòng học không hợp lệ. Vui lòng chọn lại phòng học từ danh sách.")
        return
      }

      const userId = authUtils.getUserId()

      const requestBody = {
        phongHocId: phongHocId,
        thoiGianBatDau: thoiGianBatDau.toISOString(),
        thoiGianKetThuc: thoiGianKetThuc.toISOString(),
        overrideConflicts: false,
        overrideReason: "",
      }

      if (editFormData.hinhThucHoc && editFormData.hinhThucHoc.trim() !== "") {
        requestBody.ghiChu = editFormData.hinhThucHoc
      }

      if (userId) {
        const actorId = parseInt(userId, 10)
        if (!isNaN(actorId) && actorId > 0) {
          requestBody.actorNguoiDungId = actorId
        }
      }

      await apiClient.put(`/api/truong-khoa/lich-day/${editingSchedule.id}`, requestBody)

      alert("Cập nhật lịch dạy thành công")
      setIsEditModalOpen(false)
      setEditingSchedule(null)
      setEditConflictWarning(null)
      setEditFormData({
        lopHoc: "",
        batDauTuNgay: null,
        denNgay: null,
        gioBatDau: "09:00 AM",
        gioKetThuc: "10:30 AM",
        coSo: "Cơ sở 1",
        toaNha: "Toàn nhà A",
        phong: "Phòng 205",
        phongHocId: null,
      })

      // Reload danh sách lịch dạy
      await fetchLichDayData()
    } catch (error) {
      console.error("Lỗi khi cập nhật lịch dạy:", error)
      alert(error.message || "Có lỗi xảy ra khi cập nhật lịch dạy")
    }
  }

  const handleCreateThi = async () => {
    if (!thiFormData.lopHoc || !thiFormData.ngayThi || !thiFormData.phong) {
      alert("Vui lòng chọn lớp, ngày thi và phòng học")
      return
    }

    if (!thiFormData.gioBatDau || !thiFormData.gioKetThuc) {
      alert("Vui lòng nhập giờ bắt đầu và giờ kết thúc")
      return
    }

    try {
      const ngayThi = parseDateFromInput(thiFormData.ngayThi)
      if (!ngayThi) {
        alert("Ngày thi không hợp lệ")
        return
      }

      const lopHocId = parseInt(thiFormData.lopHoc, 10)
      const phongHocId = parseInt(thiFormData.phong, 10)

      if (isNaN(lopHocId) || lopHocId <= 0) {
        alert("Lớp học không hợp lệ")
        return
      }

      if (isNaN(phongHocId) || phongHocId <= 0) {
        alert("Phòng học không hợp lệ")
        return
      }

      // Parse giờ bắt đầu và kết thúc
      const parseTime = (timeStr) => {
        if (!timeStr) return null
        const cleanTime = timeStr.replace(/SA|CH|AM|PM/i, "").trim()
        const [hour, min] = cleanTime.split(":")
        const isPM = timeStr.toUpperCase().includes("PM") || timeStr.toUpperCase().includes("CH")
        let hour24 = parseInt(hour, 10) || 9
        const min24 = parseInt(min, 10) || 0
        if (isPM && hour24 !== 12) hour24 += 12
        if (!isPM && hour24 === 12) hour24 = 0
        return { hour: hour24, min: min24 }
      }

      const startTime = parseTime(thiFormData.gioBatDau)
      const endTime = parseTime(thiFormData.gioKetThuc)

      if (!startTime || !endTime) {
        alert("Giờ bắt đầu hoặc giờ kết thúc không hợp lệ")
        return
      }

      // Tạo DateTime từ ngày và giờ
      const thoiGianBatDau = new Date(ngayThi)
      thoiGianBatDau.setHours(startTime.hour, startTime.min, 0, 0)

      const thoiGianKetThuc = new Date(ngayThi)
      thoiGianKetThuc.setHours(endTime.hour, endTime.min, 0, 0)

      if (thoiGianKetThuc <= thoiGianBatDau) {
        alert("Giờ kết thúc phải sau giờ bắt đầu")
        return
      }

      // Kiểm tra xung đột với lịch dạy nếu có giám thị
      if (thiFormData.giamThi) {
        const giamThiId = parseInt(thiFormData.giamThi, 10)
        const conflicts = lichDayData.filter((lich) => {
          if (lich.giangVienId !== giamThiId) return false
          const lichDate = new Date(lich.ngay)
          if (lichDate.toDateString() !== ngayThi.toDateString()) return false
          
          // Kiểm tra trùng thời gian
          const lichStart = new Date(lich.ngay)
          const lichEnd = new Date(lich.ngay)
          if (lich.gioBatDau) {
            const [h, m] = lich.gioBatDau.split(":")
            lichStart.setHours(parseInt(h, 10), parseInt(m, 10), 0, 0)
          }
          if (lich.gioKetThuc) {
            const [h, m] = lich.gioKetThuc.split(":")
            lichEnd.setHours(parseInt(h, 10), parseInt(m, 10), 0, 0)
          }
          
          return (thoiGianBatDau < lichEnd && thoiGianKetThuc > lichStart)
        })

        if (conflicts.length > 0) {
          const giamThiName = giangVienData.find(gv => gv.id === giamThiId)?.ten || "Giám thị"
          setConflictWarning({
            message: `Giảng viên "${giamThiName}" đã có lịch dạy vào thời gian này.`
          })
          return
        }
      }

      setConflictWarning(null)

      const userId = authUtils.getUserId()

      // Chuẩn bị request body
      // Sử dụng formatDateForAPI để tránh lệch ngày do timezone
      // Tạo Date object với giờ 12:00 (giữa ngày) để đảm bảo ngày không bị lệch khi backend lấy .Date
      const ngayThiWithTime = new Date(ngayThi)
      ngayThiWithTime.setHours(12, 0, 0, 0) // Đặt giờ 12:00 để tránh lệch ngày khi backend lấy .Date
      
      const requestBody = {
        lopHocId: lopHocId,
        phongHocId: phongHocId,
        ngayThi: formatDateForAPI(ngayThiWithTime),
        hinhThuc: "Trực tiếp", // Mặc định
        overrideConflicts: false,
        overrideReason: "",
      }
      
      console.log("Ngày thi gửi lên API:", {
        ngayThiInput: thiFormData.ngayThi,
        ngayThiParsed: ngayThi,
        ngayThiFormatted: formatDateForAPI(ngayThiWithTime),
        ngayThiLocal: ngayThiWithTime.toLocaleDateString("vi-VN")
      })

      // Chỉ thêm giamThiId nếu có giá trị
      if (thiFormData.giamThi && thiFormData.giamThi !== "") {
        const giamThiId = parseInt(thiFormData.giamThi, 10)
        if (!isNaN(giamThiId) && giamThiId > 0) {
          requestBody.giamThiId = giamThiId
        }
      }

      // Chỉ thêm actorNguoiDungId nếu có userId
      if (userId) {
        const actorId = parseInt(userId, 10)
        if (!isNaN(actorId) && actorId > 0) {
          requestBody.actorNguoiDungId = actorId
        }
      }

      await apiClient.post("/api/truong-khoa/lich-thi", requestBody)

      alert("Tạo lịch thi thành công")
      setThiFormData({
        monThi: "",
        lopHoc: "",
        ngayThi: null,
        gioBatDau: "09:00 SA",
        gioKetThuc: "11:00 SA",
        giamThi: "",
        loaiKiemTra: "Giữa kì",
        coSo: "Cơ sở 1",
        toaNha: "Toàn nhà A",
        phong: "",
      })
      setConflictWarning(null)
      setIsCreateThiModalOpen(false)

      // Reload danh sách lịch thi
      await fetchLichThiData()
    } catch (error) {
      console.error("Lỗi khi tạo lịch thi:", error)
      alert(error.message || "Có lỗi xảy ra khi tạo lịch thi")
    }
  }

  return (
    <main className="flex-1 p-6">
      <h1 className="text-[#4A90D9] text-2xl font-bold mb-6">Quản lí lịch</h1>

      {/* Tabs */}
      <div className="bg-white rounded-xl shadow-sm mb-6">
        <div className="flex items-center border-b border-gray-200">
          <button
            onClick={() => setActiveTab("thoikhoabieu")}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              activeTab === "thoikhoabieu"
                ? "text-[#4A90D9] border-b-2 border-[#4A90D9]"
                : "text-gray-600 hover:text-gray-800"
            }`}
          >
            Thời khóa biểu
          </button>
          <button
            onClick={() => setActiveTab("lichthi")}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              activeTab === "lichthi"
                ? "text-[#4A90D9] border-b-2 border-[#4A90D9]"
                : "text-gray-600 hover:text-gray-800"
            }`}
          >
            Lịch Thi
          </button>
          <div className="ml-auto mr-6">
            {activeTab === "thoikhoabieu" ? (
              <button
                onClick={() => setIsCreateModalOpen(true)}
                className="flex items-center gap-2 px-4 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8] transition-colors"
              >
                <Plus className="w-4 h-4" />
                Tạo lịch
              </button>
            ) : (
              <button
                onClick={() => setIsCreateThiModalOpen(true)}
                className="flex items-center gap-2 px-4 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8] transition-colors"
              >
                <Plus className="w-4 h-4" />
                Tạo lịch thi mới
              </button>
            )}
          </div>
        </div>

        <div className="p-6">
          <div className="grid grid-cols-1 lg:grid-cols-[350px_1fr] gap-6">
            {/* Cột trái: Calendar và Bộ lọc */}
            <div className="space-y-6">
              {/* Calendar */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <Calendar
                  mode="single"
                  selected={selectedDate}
                  onSelect={setSelectedDate}
                  className="w-full"
                  modifiers={{
                    hasSchedule: (date) => datesWithSchedule.has(date.toDateString()),
                  }}
                  modifiersClassNames={{
                    hasSchedule: "!bg-orange-500 !text-white !rounded-full !font-semibold",
                  }}
                />
              </div>

              {/* Bộ lọc */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <div className="flex items-center justify-between mb-4">
                  <div className="flex items-center gap-2">
                    <Filter className="w-5 h-5 text-gray-600" />
                    <h3 className="text-sm font-semibold text-gray-800">Bộ lọc</h3>
                  </div>
                  {lopOptions.length === 0 && !isLoading && (
                    <button
                      onClick={testLopHocAPI}
                      className="text-xs px-2 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors"
                      title="Test API lớp học"
                    >
                      Test API
                    </button>
                  )}
                </div>
                {apiTestResult && (
                  <div className={`mb-3 p-2 rounded text-xs ${
                    apiTestResult.includes("✅") 
                      ? "bg-green-50 text-green-700" 
                      : "bg-red-50 text-red-700"
                  }`}>
                    {apiTestResult}
                  </div>
                )}
                <div className="space-y-4">
                  {/* Lọc theo giảng viên */}
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-2">Giảng viên</label>
                    <select
                      value={selectedGiangVien}
                      onChange={(e) => setSelectedGiangVien(e.target.value)}
                      className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                    >
                      <option value="Tất cả">Tất cả</option>
                      {giangVienData.map((gv) => (
                        <option key={gv.id} value={gv.id}>
                          {gv.ten || "N/A"}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Lọc theo lớp học */}
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-2">Lớp học</label>
                    <select
                      value={selectedLop}
                      onChange={(e) => setSelectedLop(e.target.value)}
                      disabled={isLoading || lopOptions.length === 0}
                      className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] disabled:bg-gray-100 disabled:cursor-not-allowed"
                    >
                      <option value="Tất cả">
                        {isLoading ? "Đang tải..." : lopOptions.length === 0 ? "Không có dữ liệu" : "Tất cả"}
                      </option>
                      {lopOptions.map((lop) => (
                        <option key={lop.id} value={lop.maLop}>
                          {lop.maLop}
                        </option>
                      ))}
                    </select>
                    {!isLoading && lopOptions.length === 0 && (
                      <p className="text-xs text-red-500 mt-1">Không thể tải danh sách lớp học</p>
                    )}
                  </div>

                  {/* Lọc theo phòng học */}
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-2">Phòng học</label>
                    <select
                      value={selectedPhong}
                      onChange={(e) => setSelectedPhong(e.target.value)}
                      className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                    >
                      <option value="Tất cả">Tất cả</option>
                      {phongList.map((phong) => (
                        <option key={phong} value={phong}>
                          {phong}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>
            </div>

            {/* Cột phải: Danh sách lịch */}
            <div>
              {(activeTab === "thoikhoabieu" ? filteredLichDay : filteredLichThi).length === 0 ? (
                <div className="bg-white border border-gray-200 rounded-lg p-12 text-center">
                  <p className="text-gray-500">
                    {activeTab === "thoikhoabieu" 
                      ? "Không có lịch dạy nào trong ngày được chọn." 
                      : "Không có lịch thi nào trong ngày được chọn."}
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {(activeTab === "thoikhoabieu" ? filteredLichDay : filteredLichThi).map((lich) => {
                    const lichDate = new Date(lich.ngay)
                    const dayOfWeek = getDayOfWeek(lichDate)

                    return (
                      <div
                        key={lich.id}
                        className="bg-white border border-gray-200 rounded-lg p-5 relative"
                      >
                        <span className="absolute top-4 right-4 px-2 py-1 bg-[#4A90D9] text-white text-xs font-medium rounded">
                          {activeTab === "thoikhoabieu" ? "Lịch dạy" : "Lịch thi"}
                        </span>

                        <div className="mb-4">
                          <h3 className="text-lg font-bold text-gray-800 mb-1">{lich.monHoc}</h3>
                          <p className="text-sm text-gray-600">{lich.lop}</p>
                        </div>

                        <div className="space-y-3 mb-4">
                          <div className="flex items-center gap-3">
                            <Clock className="w-4 h-4 text-gray-400" />
                            <span className="text-sm text-gray-700">
                              {dayOfWeek}, {lich.thoiGian}
                            </span>
                          </div>
                          <div className="flex items-center gap-3">
                            <MapPin className="w-4 h-4 text-gray-400" />
                            <span className="text-sm text-gray-700">{lich.phongHoc}</span>
                          </div>
                          {activeTab === "thoikhoabieu" ? (
                            <div className="flex items-center gap-3">
                              <User className="w-4 h-4 text-gray-400" />
                              <span className="text-sm text-gray-700">GV:
                                {lich.giangVien && lich.giangVien !== "N/A" ? lich.giangVien : "Chưa phân công"}
                              </span>
                            </div>
                          ) : (
                            <div className="flex items-center gap-3">
                              <User className="w-4 h-4 text-gray-400" />
                              <span className="text-sm text-gray-700">
                                Giám thị: {lich.giangVien && lich.giangVien !== "N/A" ? lich.giangVien : "Chưa phân công"}
                              </span>
                            </div>
                          )}
                        </div>

                        <div className="flex items-center gap-2 pt-4 border-t border-gray-100">
                          <button
                            onClick={() => handleEditSchedule(lich)}
                            className="flex items-center gap-2 px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-100 rounded transition-colors"
                          >
                            <Edit className="w-4 h-4" />
                            Sửa
                          </button>
                          <button className="flex items-center gap-2 px-3 py-1.5 text-sm text-red-600 hover:bg-red-50 rounded transition-colors">
                            <Trash2 className="w-4 h-4" />
                            Xóa
                          </button>
                          {activeTab === "lichthi" && (
                            <button className="flex items-center gap-2 px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-100 rounded transition-colors ml-auto">
                              <Upload className="w-4 h-4" />
                              Xuất danh sách phòng thi
                            </button>
                          )}
                        </div>
                      </div>
                    )
                  })}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Modal Tạo Lịch Dạy */}
      <Dialog open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto p-0">
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <DialogTitle className="text-xl font-bold text-gray-800">Tạo Lịch Dạy</DialogTitle>
          </div>

          <div className="p-6 space-y-4">
            {/* Lớp học */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Lớp học</label>
              <select
                value={formData.lopHoc}
                onChange={(e) => setFormData({ ...formData, lopHoc: e.target.value })}
                disabled={isLoading || lopOptions.length === 0}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9] disabled:bg-gray-100 disabled:cursor-not-allowed"
              >
                <option value="">
                  {isLoading ? "Đang tải dữ liệu..." : lopOptions.length === 0 ? "Không có dữ liệu lớp học" : "Chọn lớp học"}
                </option>
                {lopOptions.map((lop) => (
                  <option key={lop.id} value={lop.id}>
                    {lop.maLop || lop.label || `Lớp ${lop.id}`}
                  </option>
                ))}
              </select>
              {!isLoading && lopOptions.length === 0 && (
                <p className="text-xs text-red-500 mt-1">Không thể tải danh sách lớp học. Vui lòng thử lại sau.</p>
              )}
            </div>

            {/* Giảng viên (tùy chọn) */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Giảng viên (tùy chọn)</label>
              <select
                value={formData.giangVienId}
                onChange={(e) => setFormData({ ...formData, giangVienId: e.target.value })}
                disabled={isLoading}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9] disabled:bg-gray-100 disabled:cursor-not-allowed"
              >
                <option value="">
                  {isLoading ? "Đang tải dữ liệu..." : "Tự động theo lớp"}
                </option>
                {giangVienData.map((gv) => (
                  <option key={gv.id} value={gv.id}>
                    {gv.ten || `Giảng viên ${gv.id}`}
                  </option>
                ))}
              </select>
            </div>

            {/* Hình thức học */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Hình thức học</label>
              <div className="relative">
                <select
                  value={formData.hinhThucHoc}
                  onChange={(e) => setFormData({ ...formData, hinhThucHoc: e.target.value })}
                  className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                >
                  <option value="Trực tiếp">Trực tiếp</option>
                  <option value="Trực tuyến">Trực tuyến</option>
                  <option value="Kết hợp">Kết hợp</option>
                </select>
              </div>
            </div>

            {/* Ngày bắt đầu và kết thúc */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Bắt đầu từ ngày</label>
                <Popover open={startDateOpen} onOpenChange={setStartDateOpen}>
                  <PopoverTrigger asChild>
                    <button
                      type="button"
                      className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white text-left focus:outline-none focus:ring-2 focus:ring-[#4A90D9] flex items-center justify-between"
                    >
                      <span className={formData.batDauTuNgay ? "text-gray-900" : "text-gray-400"}>
                        {formData.batDauTuNgay
                          ? formatDateForInput(parseDateFromInput(formData.batDauTuNgay))
                          : "mm/dd/yyyy"}
                      </span>
                      <CalendarIcon className="w-4 h-4 text-gray-400" />
                    </button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0" align="start">
                    <Calendar
                      mode="single"
                      selected={
                        formData.batDauTuNgay
                          ? parseDateFromInput(formData.batDauTuNgay)
                          : undefined
                      }
                      onSelect={(date) => {
                        if (date) {
                          const dateStr = formatDateForInput(date)
                          setFormData({
                            ...formData,
                            batDauTuNgay: dateStr,
                          })
                          setStartDateOpen(false)
                        }
                      }}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Đến ngày</label>
                <Popover open={endDateOpen} onOpenChange={setEndDateOpen}>
                  <PopoverTrigger asChild>
                    <button
                      type="button"
                      className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white text-left focus:outline-none focus:ring-2 focus:ring-[#4A90D9] flex items-center justify-between"
                    >
                      <span className={formData.denNgay ? "text-gray-900" : "text-gray-400"}>
                        {formData.denNgay
                          ? formatDateForInput(parseDateFromInput(formData.denNgay))
                          : "mm/dd/yyyy"}
                      </span>
                      <CalendarIcon className="w-4 h-4 text-gray-400" />
                    </button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0" align="start">
                    <Calendar
                      mode="single"
                      selected={
                        formData.denNgay ? parseDateFromInput(formData.denNgay) : undefined
                      }
                      onSelect={(date) => {
                        if (date) {
                          const dateStr = formatDateForInput(date)
                          setFormData({
                            ...formData,
                            denNgay: dateStr,
                          })
                          setEndDateOpen(false)
                        }
                      }}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
            </div>

            {/* Giờ bắt đầu và kết thúc */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Giờ bắt đầu</label>
                <div className="relative">
                  <input
                    type="text"
                    value={formData.gioBatDau}
                    onChange={(e) => {
                      const value = e.target.value
                      if (/^(\d{1,2}):(\d{0,2})\s?(AM|PM)?$/i.test(value) || value === "") {
                        setFormData({
                          ...formData,
                          gioBatDau: value,
                        })
                      }
                    }}
                    placeholder="09:00 AM"
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                  />
                  <Clock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Giờ kết thúc</label>
                <div className="relative">
                  <input
                    type="text"
                    value={formData.gioKetThuc}
                    onChange={(e) => {
                      const value = e.target.value
                      if (/^(\d{1,2}):(\d{0,2})\s?(AM|PM)?$/i.test(value) || value === "") {
                        setFormData({
                          ...formData,
                          gioKetThuc: value,
                        })
                      }
                    }}
                    placeholder="10:30 AM"
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                  />
                  <Clock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* Địa điểm */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Địa điểm</label>
              <div className="grid grid-cols-3 gap-3">
                <div className="relative">
                  <select
                    value={formData.coSo}
                    onChange={(e) => setFormData({ ...formData, coSo: e.target.value })}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                  >
                    <option>Cơ sở 1</option>
                    <option>Cơ sở 2</option>
                  </select>
                </div>
                <div className="relative">
                  <select
                    value={formData.toaNha}
                    onChange={(e) => setFormData({ ...formData, toaNha: e.target.value })}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                  >
                    <option>Toàn nhà A</option>
                    <option>Toàn nhà B</option>
                    <option>Toàn nhà C</option>
                  </select>
                </div>
                <div>
                  <select
                    value={formData.phong}
                    onChange={(e) => setFormData({ ...formData, phong: e.target.value })}
                    disabled={isLoading}
                    className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9] disabled:bg-gray-100 disabled:cursor-not-allowed"
                  >
                    <option value="">
                      {isLoading ? "Đang tải..." : "Chọn phòng học"}
                    </option>
                    {phongOptions.map((phong) => (
                      <option key={phong.id} value={phong.id}>
                        {phong.label || `Phòng ${phong.id}`}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            </div>

            {/* Cảnh báo trùng lịch (tạm thời không dùng) */}
            {conflictWarning && (
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <div className="flex items-start gap-3">
                  <AlertTriangle className="w-5 h-5 text-yellow-600 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm font-medium text-yellow-800 mb-1">Phát hiện trùng lịch</p>
                    <p className="text-sm text-yellow-700">{conflictWarning.message}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Nút hành động */}
            <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
              <button
                onClick={() => {
                  setIsCreateModalOpen(false)
                  setFormData({
                    lopHoc: "",
                    giangVienId: "",
                    hinhThucHoc: "Trực tiếp",
                    batDauTuNgay: null,
                    denNgay: null,
                    gioBatDau: "09:00 AM",
                    gioKetThuc: "10:30 AM",
                    coSo: "Cơ sở 1",
                    toaNha: "Toàn nhà A",
                    phong: "",
                  })
                  setConflictWarning(null)
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
              >
                Huỷ
              </button>
              <button
                onClick={handleCreateSchedule}
                className="px-4 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8] transition-colors"
              >
                Lưu Lịch
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Modal Sửa Lịch */}
      <Dialog open={isEditModalOpen} onOpenChange={setIsEditModalOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto p-0">
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <DialogTitle className="text-xl font-bold text-gray-800">
              {activeTab === "thoikhoabieu" ? "Sửa lịch dạy" : "Sửa lịch thi"}
            </DialogTitle>
            <button
              onClick={() => setIsEditModalOpen(false)}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          <div className="p-6 space-y-4">
            {/* Lớp học */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Lớp học</label>
              <div className="relative">
                <select
                  value={editFormData.lopHoc}
                  onChange={(e) => setEditFormData({ ...editFormData, lopHoc: e.target.value })}
                  disabled={lopOptions.length === 0}
                  className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none disabled:bg-gray-100 disabled:cursor-not-allowed"
                >
                  <option value="">
                    {lopOptions.length === 0 ? "Không có dữ liệu lớp học" : "Chọn lớp học"}
                  </option>
                  {lopOptions.map((lop) => (
                    <option key={lop.id} value={lop.id}>
                      {lop.maLop || lop.label}
                    </option>
                  ))}
                </select>
              </div>
              {lopOptions.length === 0 && (
                <p className="text-xs text-red-500 mt-1">Không thể tải danh sách lớp học.</p>
              )}
            </div>

            {/* Hình thức học */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Hình thức học</label>
              <div className="relative">
                <select
                  value={editFormData.hinhThucHoc}
                  onChange={(e) => setEditFormData({ ...editFormData, hinhThucHoc: e.target.value })}
                  className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                >
                  <option value="Trực tiếp">Trực tiếp</option>
                  <option value="Trực tuyến">Trực tuyến</option>
                  <option value="Kết hợp">Kết hợp</option>
                </select>
              </div>
            </div>

            {/* Ngày bắt đầu và kết thúc */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Bắt đầu từ ngày</label>
                <Popover open={editStartDateOpen} onOpenChange={setEditStartDateOpen}>
                  <PopoverTrigger asChild>
                    <button
                      type="button"
                      className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white text-left focus:outline-none focus:ring-2 focus:ring-[#4A90D9] flex items-center justify-between"
                    >
                      <span className={editFormData.batDauTuNgay ? "text-gray-900" : "text-gray-400"}>
                        {editFormData.batDauTuNgay
                          ? formatDateForInput(parseDateFromInput(editFormData.batDauTuNgay))
                          : "mm/dd/yyyy"}
                      </span>
                      <CalendarIcon className="w-4 h-4 text-gray-400" />
                    </button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0" align="start">
                    <Calendar
                      mode="single"
                      selected={
                        editFormData.batDauTuNgay
                          ? parseDateFromInput(editFormData.batDauTuNgay)
                          : undefined
                      }
                      onSelect={(date) => {
                        if (date) {
                          const dateStr = formatDateForInput(date)
                          setEditFormData({
                            ...editFormData,
                            batDauTuNgay: dateStr,
                          })
                          setEditStartDateOpen(false)
                        }
                      }}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Đến ngày</label>
                <Popover open={editEndDateOpen} onOpenChange={setEditEndDateOpen}>
                  <PopoverTrigger asChild>
                    <button
                      type="button"
                      className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white text-left focus:outline-none focus:ring-2 focus:ring-[#4A90D9] flex items-center justify-between"
                    >
                      <span className={editFormData.denNgay ? "text-gray-900" : "text-gray-400"}>
                        {editFormData.denNgay
                          ? formatDateForInput(parseDateFromInput(editFormData.denNgay))
                          : "mm/dd/yyyy"}
                      </span>
                      <CalendarIcon className="w-4 h-4 text-gray-400" />
                    </button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0" align="start">
                    <Calendar
                      mode="single"
                      selected={
                        editFormData.denNgay ? parseDateFromInput(editFormData.denNgay) : undefined
                      }
                      onSelect={(date) => {
                        if (date) {
                          const dateStr = formatDateForInput(date)
                          setEditFormData({
                            ...editFormData,
                            denNgay: dateStr,
                          })
                          setEditEndDateOpen(false)
                        }
                      }}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
            </div>

            {/* Giờ bắt đầu và kết thúc */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Giờ bắt đầu</label>
                <div className="relative">
                  <input
                    type="text"
                    value={editFormData.gioBatDau}
                    onChange={(e) => {
                      const value = e.target.value
                      if (/^(\d{1,2}):(\d{0,2})\s?(AM|PM)?$/i.test(value) || value === "") {
                        setEditFormData({
                          ...editFormData,
                          gioBatDau: value,
                        })
                      }
                    }}
                    placeholder="09:00 AM"
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                  />
                  <Clock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Giờ kết thúc</label>
                <div className="relative">
                  <input
                    type="text"
                    value={editFormData.gioKetThuc}
                    onChange={(e) => {
                      const value = e.target.value
                      if (/^(\d{1,2}):(\d{0,2})\s?(AM|PM)?$/i.test(value) || value === "") {
                        setEditFormData({
                          ...editFormData,
                          gioKetThuc: value,
                        })
                      }
                    }}
                    placeholder="10:30 AM"
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                  />
                  <Clock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* Địa điểm */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Địa điểm</label>
              <div className="grid grid-cols-3 gap-3">
                <div className="relative">
                  <select
                    value={editFormData.coSo}
                    onChange={(e) => setEditFormData({ ...editFormData, coSo: e.target.value })}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                  >
                    <option>Cơ sở 1</option>
                    <option>Cơ sở 2</option>
                  </select>
                </div>
                <div className="relative">
                  <select
                    value={editFormData.toaNha}
                    onChange={(e) => setEditFormData({ ...editFormData, toaNha: e.target.value })}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                  >
                    <option>Toàn nhà A</option>
                    <option>Toàn nhà B</option>
                    <option>Toàn nhà C</option>
                  </select>
                </div>
                <div className="relative">
                  <select
                    value={editFormData.phong}
                    onChange={(e) => setEditFormData({ ...editFormData, phong: e.target.value })}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                  >
                    <option>Phòng 205</option>
                    <option>Phòng 206</option>
                    <option>Phòng 301</option>
                    <option>Phòng 302</option>
                    <option>Phòng 403</option>
                  </select>
                </div>
              </div>
            </div>

            {/* Cảnh báo trùng lịch (tạm thời không dùng) */}
            {editConflictWarning && (
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <div className="flex items-start gap-3">
                  <AlertTriangle className="w-5 h-5 text-yellow-600 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm font-medium text-yellow-800 mb-1">Phát hiện trùng lịch</p>
                    <p className="text-sm text-yellow-700">{editConflictWarning.message}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Nút hành động */}
            <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
              <button
                onClick={() => {
                  setIsEditModalOpen(false)
                  setEditingSchedule(null)
                  setEditFormData({
                    lopHoc: "",
                    batDauTuNgay: null,
                    denNgay: null,
                    gioBatDau: "09:00 AM",
                    gioKetThuc: "10:30 AM",
                    coSo: "Cơ sở 1",
                    toaNha: "Toàn nhà A",
                    phong: "Phòng 205",
                  })
                  setEditConflictWarning(null)
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
              >
                Huỷ
              </button>
              <button
                onClick={handleUpdateSchedule}
                className="px-4 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8] transition-colors"
              >
                Lưu Lịch
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Modal Tạo Lịch Thi */}
      <Dialog open={isCreateThiModalOpen} onOpenChange={setIsCreateThiModalOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto p-0">
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <DialogTitle className="text-xl font-bold text-gray-800">Tạo Lịch Thi</DialogTitle>
            <button
              onClick={() => setIsCreateThiModalOpen(false)}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          <div className="p-6 space-y-4">
            {/* Môn thi */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Môn thi</label>
              <input
                type="text"
                value={thiFormData.monThi}
                onChange={(e) => setThiFormData({ ...thiFormData, monThi: e.target.value })}
                placeholder="Nhập tên môn thi"
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
              />
            </div>

            {/* Lớp */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Lớp</label>
              <select
                value={thiFormData.lopHoc}
                onChange={(e) => {
                  const selectedLop = lopOptions.find(lop => lop.id.toString() === e.target.value)
                  // Tự động điền môn thi từ lớp học
                  let monThi = thiFormData.monThi
                  if (selectedLop && !monThi) {
                    const label = selectedLop.label || ""
                    const parts = label.split("—")
                    if (parts.length > 1) {
                      monThi = parts[parts.length - 1].trim()
                    }
                  }
                  setThiFormData({ ...thiFormData, lopHoc: e.target.value, monThi })
                }}
                disabled={isLoading || lopOptions.length === 0}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9] disabled:bg-gray-100 disabled:cursor-not-allowed"
              >
                <option value="">
                  {isLoading ? "Đang tải dữ liệu..." : lopOptions.length === 0 ? "Không có dữ liệu lớp học" : "Chọn lớp học"}
                </option>
                {lopOptions.map((lop) => (
                  <option key={lop.id} value={lop.id}>
                    {lop.maLop || lop.label || `Lớp ${lop.id}`}
                  </option>
                ))}
              </select>
              {!isLoading && lopOptions.length === 0 && (
                <p className="text-xs text-red-500 mt-1">Không thể tải danh sách lớp học. Vui lòng thử lại sau.</p>
              )}
            </div>

            {/* Ngày */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Ngày</label>
              <Popover>
                <PopoverTrigger asChild>
                  <button
                    type="button"
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white text-left focus:outline-none focus:ring-2 focus:ring-[#4A90D9] flex items-center justify-between"
                  >
                    <span className={thiFormData.ngayThi ? "text-gray-900" : "text-gray-400"}>
                      {thiFormData.ngayThi
                        ? formatDateForInput(parseDateFromInput(thiFormData.ngayThi))
                        : "mm/dd/yyyy"}
                    </span>
                    <CalendarIcon className="w-4 h-4 text-gray-400" />
                  </button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar
                    mode="single"
                    selected={thiFormData.ngayThi ? parseDateFromInput(thiFormData.ngayThi) : undefined}
                    onSelect={(date) => {
                      if (date) {
                        setThiFormData({ ...thiFormData, ngayThi: formatDateForInput(date) })
                      }
                    }}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>

            {/* Giờ bắt đầu và kết thúc */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Giờ bắt đầu</label>
                <div className="relative">
                  <input
                    type="text"
                    value={thiFormData.gioBatDau}
                    onChange={(e) => {
                      const value = e.target.value
                      if (/^(\d{1,2}):(\d{0,2})\s?(SA|CH|AM|PM)?$/i.test(value) || value === "") {
                        setThiFormData({ ...thiFormData, gioBatDau: value })
                      }
                    }}
                    placeholder="09:00 SA"
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                  />
                  <Clock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Giờ kết thúc</label>
                <div className="relative">
                  <input
                    type="text"
                    value={thiFormData.gioKetThuc}
                    onChange={(e) => {
                      const value = e.target.value
                      if (/^(\d{1,2}):(\d{0,2})\s?(SA|CH|AM|PM)?$/i.test(value) || value === "") {
                        setThiFormData({ ...thiFormData, gioKetThuc: value })
                      }
                    }}
                    placeholder="11:00 SA"
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                  />
                  <Clock className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* Giám thị */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Giám thị</label>
              <select
                value={thiFormData.giamThi}
                onChange={(e) => setThiFormData({ ...thiFormData, giamThi: e.target.value })}
                disabled={isLoading}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9] disabled:bg-gray-100 disabled:cursor-not-allowed"
              >
                <option value="">
                  {isLoading ? "Đang tải dữ liệu..." : "Chọn giám thị"}
                </option>
                {giangVienData.map((gv) => (
                  <option key={gv.id} value={gv.id}>
                    {gv.ten || `Giảng viên ${gv.id}`}
                  </option>
                ))}
              </select>
            </div>

            {/* Loại kiểm tra */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Loại kiểm tra</label>
              <select
                value={thiFormData.loaiKiemTra}
                onChange={(e) => setThiFormData({ ...thiFormData, loaiKiemTra: e.target.value })}
                className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
              >
                <option>Giữa kì</option>
                <option>Cuối kì</option>
                <option>Kiểm tra thường xuyên</option>
                <option>Thi tốt nghiệp</option>
              </select>
            </div>

            {/* Địa điểm */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Địa điểm</label>
              <div className="grid grid-cols-3 gap-3">
                <div className="relative">
                  <select
                    value={thiFormData.coSo}
                    onChange={(e) => setThiFormData({ ...thiFormData, coSo: e.target.value })}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                  >
                    <option>Cơ sở 1</option>
                    <option>Cơ sở 2</option>
                    <option>Cơ sở 3</option>
                  </select>
                </div>
                <div className="relative">
                  <select
                    value={thiFormData.toaNha}
                    onChange={(e) => setThiFormData({ ...thiFormData, toaNha: e.target.value })}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                  >
                    <option>Toàn nhà A</option>
                    <option>Toàn nhà B</option>
                    <option>Toàn nhà C</option>
                  </select>
                </div>
                <div className="relative">
                  <select
                    value={thiFormData.phong}
                    onChange={(e) => setThiFormData({ ...thiFormData, phong: e.target.value })}
                    disabled={isLoading}
                    className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-[#4A90D9] disabled:bg-gray-100 disabled:cursor-not-allowed appearance-none"
                  >
                    <option value="">
                      {isLoading ? "Đang tải..." : "Chọn phòng"}
                    </option>
                    {phongOptions.map((phong) => (
                      <option key={phong.id} value={phong.id}>
                        {phong.label || `Phòng ${phong.id}`}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            </div>

            {/* Cảnh báo trùng lịch (nếu có) */}
            {conflictWarning && (
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <div className="flex items-start gap-3">
                  <AlertTriangle className="w-5 h-5 text-yellow-600 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm font-medium text-yellow-800 mb-1">Cảnh báo: Xung đột lịch</p>
                    <p className="text-sm text-yellow-700">{conflictWarning.message}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Nút hành động */}
            <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
              <button
                onClick={() => {
                  setIsCreateThiModalOpen(false)
                  setThiFormData({
                    monThi: "",
                    lopHoc: "",
                    ngayThi: null,
                    gioBatDau: "09:00 SA",
                    gioKetThuc: "11:00 SA",
                    giamThi: "",
                    loaiKiemTra: "Giữa kì",
                    coSo: "Cơ sở 1",
                    toaNha: "Toàn nhà A",
                    phong: "",
                  })
                  setConflictWarning(null)
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
              >
                Hủy
              </button>
              <button
                onClick={handleCreateThi}
                className="px-4 py-2 bg-gray-800 text-white rounded-lg text-sm font-medium hover:bg-gray-700 transition-colors"
              >
                Lưu
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </main>
  )
}
