"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Download, Bell } from "lucide-react"
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts"
import { apiClient } from "@/lib/api"
import { chartData as fallbackChartData } from "@/lib/data"

export default function TongQuanPage() {
  const router = useRouter()
  const [notifications, setNotifications] = useState([])
  const [dashboardData, setDashboardData] = useState({
    tongSoLop: 0,
    tongGiangVien: 0,
    tongSoHocVien: 0,
    hocVienHoatDong: 0,
    hocVienDaNghi: 0,
    thongBaoGanDay: [],
    bieuDoHoanThanhBaiTap: [],
  })
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setIsLoading(true)
        const response = await apiClient.get("/api/truong-khoa/hoc-vien/thong-ke")
        
        if (response.success && response.data) {
          const data = response.data
          setDashboardData({
            tongSoLop: data.tongSoLop || 0,
            tongGiangVien: data.tongGiangVien || 0,
            tongSoHocVien: data.tongSoHocVien || 0,
            hocVienHoatDong: data.hocVienHoatDong || 0,
            hocVienDaNghi: data.hocVienDaNghi || 0,
            thongBaoGanDay: data.thongBaoGanDay || [],
            bieuDoHoanThanhBaiTap: data.bieuDoHoanThanhBaiTap || [],
          })

          // Lấy danh sách thông báo đã đọc từ localStorage
          const readNotifications = JSON.parse(localStorage.getItem("readNotifications") || "[]")
          
          // Format thông báo và đánh dấu đã đọc nếu có trong localStorage
          const formattedNotifications = (data.thongBaoGanDay || []).map((tb, idx) => {
            const notificationId = tb.id || idx
            return {
              id: notificationId,
              title: "Thông báo",
              content: tb.noiDung || "",
              timeAgo: tb.thoiGian || "",
              daDoc: readNotifications.includes(notificationId),
            }
          })
          setNotifications(formattedNotifications)

          const unreadCount = formattedNotifications.filter((n) => !n.daDoc).length
          localStorage.setItem("unreadNotifications", unreadCount.toString())
          window.dispatchEvent(new Event("notificationsUpdated"))
        }
      } catch (error) {
        console.error("Lỗi khi tải dữ liệu dashboard:", error)
      } finally {
        setIsLoading(false)
      }
    }

    fetchDashboardData()
    
    // Lắng nghe sự kiện cập nhật thông báo từ trang khác
    const handleNotificationsUpdate = () => {
      const readNotifications = JSON.parse(localStorage.getItem("readNotifications") || "[]")
      setNotifications(prev => prev.map(n => ({
        ...n,
        daDoc: readNotifications.includes(n.id)
      })))
    }
    
    window.addEventListener("notificationsUpdated", handleNotificationsUpdate)
    return () => window.removeEventListener("notificationsUpdated", handleNotificationsUpdate)
  }, [])

  // Đếm số thông báo chưa đọc
  const unreadCount = notifications.filter((n) => !n.daDoc).length

  // Đánh dấu tất cả đã đọc
  const handleMarkAllAsRead = () => {
    const allIds = notifications.map(n => n.id)
    const readNotifications = JSON.parse(localStorage.getItem("readNotifications") || "[]")
    const updatedRead = [...new Set([...readNotifications, ...allIds])]
    localStorage.setItem("readNotifications", JSON.stringify(updatedRead))
    
    setNotifications(notifications.map((n) => ({ ...n, daDoc: true })))
    // Cập nhật localStorage để sidebar cập nhật badge
    localStorage.setItem("unreadNotifications", "0")
    window.dispatchEvent(new Event("notificationsUpdated"))
  }

  // Xử lý click vào thông báo
  const handleNotificationClick = (notif) => {
    // Đánh dấu đã đọc
    if (!notif.daDoc) {
      const readNotifications = JSON.parse(localStorage.getItem("readNotifications") || "[]")
      if (!readNotifications.includes(notif.id)) {
        readNotifications.push(notif.id)
        localStorage.setItem("readNotifications", JSON.stringify(readNotifications))
      }
      
      setNotifications(notifications.map((n) => (n.id === notif.id ? { ...n, daDoc: true } : n)))
      // Cập nhật localStorage
      const newUnreadCount = notifications.filter((n) => n.id !== notif.id && !n.daDoc).length
      localStorage.setItem("unreadNotifications", newUnreadCount.toString())
      window.dispatchEvent(new Event("notificationsUpdated"))
    }
    // Chuyển đến trang thông báo
    router.push("/thongbao")
  }

  // Xử lý xuất PDF
  const handleExportPDF = async () => {
    try {
      const date = new Date()
      const dateStr = date.toISOString().split('T')[0]
      const filename = `bao-cao-dashboard-${dateStr}.pdf`
      
      await apiClient.downloadFile("/api/truong-khoa/dashboard/export/pdf", filename)
    } catch (error) {
      console.error("Lỗi khi xuất PDF:", error)
      alert("Không thể xuất PDF: " + (error.message || "Lỗi không xác định"))
    }
  }

  // Xử lý xuất CSV (Excel)
  const handleExportCSV = async () => {
    try {
      const date = new Date()
      const dateStr = date.toISOString().split('T')[0]
      const filename = `bao-cao-dashboard-${dateStr}.csv`
      
      await apiClient.downloadFile("/api/truong-khoa/dashboard/export/csv", filename)
    } catch (error) {
      console.error("Lỗi khi xuất CSV:", error)
      alert("Không thể xuất CSV: " + (error.message || "Lỗi không xác định"))
    }
  }

  return (
    <main className="w-full p-6">
      {/* Header với nút xuất */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-[#4A90D9] text-2xl font-bold">Tổng quan</h1>
        <div className="flex gap-3">
          <button 
            onClick={handleExportPDF}
            disabled={isLoading}
            className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <Download className="w-4 h-4" />
            Xuất PDF
          </button>
          <button 
            onClick={handleExportCSV}
            disabled={isLoading}
            className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <Download className="w-4 h-4" />
            Xuất CSV
          </button>
        </div>
      </div>

      <div className="flex gap-6">
        {/* Cột trái: Thống kê và biểu đồ */}
        <div className="flex-1">
          {/* Thẻ thống kê */}
          <div className="grid grid-cols-3 gap-4 mb-6">
            <div className="bg-white rounded-xl p-5 border-l-4 border-[#4A90D9] shadow-sm">
              <p className="text-[#4A90D9] text-sm font-medium mb-1">Tổng số lớp</p>
              <p className="text-[#4A90D9] text-2xl font-bold">{isLoading ? "..." : dashboardData.tongSoLop}</p>
            </div>
            <div className="bg-white rounded-xl p-5 border-l-4 border-[#22C55E] shadow-sm">
              <p className="text-[#22C55E] text-sm font-medium mb-1">Tổng học viên</p>
              <p className="text-[#22C55E] text-2xl font-bold">{isLoading ? "..." : dashboardData.tongSoHocVien}</p>
            </div>
            <div className="bg-white rounded-xl p-5 border-l-4 border-[#A855F7] shadow-sm">
              <p className="text-[#A855F7] text-sm font-medium mb-1">Giảng viên</p>
              <p className="text-[#A855F7] text-2xl font-bold">{isLoading ? "..." : dashboardData.tongGiangVien}</p>
            </div>
          </div>

          {/* Biểu đồ (kết nối API, fallback data mẫu nếu API không có dữ liệu) */}
          <div className="bg-white rounded-xl p-6 shadow-sm">
            <h2 className="text-gray-800 font-semibold text-lg mb-6">Biểu đồ hoàn thành bài tập</h2>
            <div className="h-[600px]">
              {isLoading ? (
                <div className="flex items-center justify-center h-full">Đang tải...</div>
              ) : (
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={
                      dashboardData.bieuDoHoanThanhBaiTap && dashboardData.bieuDoHoanThanhBaiTap.length > 0
                        ? dashboardData.bieuDoHoanThanhBaiTap.map((item) => ({
                            name: item.monHoc || "Môn học",
                            daNop: item.daNop || 0,
                            chuaNop: item.chuaNop || 0,
                          }))
                        : fallbackChartData
                    }
                    barGap={8}
                  >
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
                    <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fontSize: 12, fill: "#6B7280" }} />
                    <YAxis
                      axisLine={false}
                      tickLine={false}
                      tick={{ fontSize: 12, fill: "#6B7280" }}
                    />
                    <Tooltip 
                      formatter={(value, name) => {
                        if (name === "daNop") return [`${value} bài`, "Đã nộp"]
                        if (name === "chuaNop") return [`${value} bài`, "Chưa nộp"]
                        return value
                      }}
                    />
                    <Legend
                      verticalAlign="bottom"
                      height={36}
                      formatter={(value) => (
                        <span className="text-sm text-gray-600">{value === "daNop" ? "Đã nộp" : "Chưa nộp"}</span>
                      )}
                    />
                    <Bar dataKey="daNop" fill="#22C55E" radius={[4, 4, 0, 0]} barSize={40} name="daNop" />
                    <Bar dataKey="chuaNop" fill="#F5A623" radius={[4, 4, 0, 0]} barSize={40} name="chuaNop" />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </div>
          </div>
        </div>

        {/* Cột phải: Thông báo gần đây */}
        <div className="w-[400px]">
          <div className="bg-white rounded-xl p-5 shadow-sm">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <Bell className="w-5 h-5 text-gray-600" />
                <h2 className="text-gray-800 font-semibold text-base">Thông báo gần đây</h2>
                {unreadCount > 0 && (
                  <span className="bg-red-500 text-white text-xs w-5 h-5 rounded-full flex items-center justify-center">
                    {unreadCount}
                  </span>
                )}
              </div>
              {unreadCount > 0 && (
                <button
                  onClick={handleMarkAllAsRead}
                  className="text-xs text-[#4A90D9] hover:underline"
                >
                  Đánh dấu tất cả đã đọc
                </button>
              )}
            </div>

            <div className="space-y-4">
              {isLoading ? (
                <div className="text-center py-8 text-gray-500">Đang tải...</div>
              ) : notifications.length === 0 ? (
                <div className="text-center py-8 text-gray-500">Không có thông báo</div>
              ) : (
                notifications.map((notif) => (
                  <button
                    key={notif.id}
                    onClick={() => handleNotificationClick(notif)}
                    className="w-full text-left pb-4 border-b border-gray-100 last:border-b-0 last:pb-0 hover:bg-gray-50 -mx-2 px-2 rounded transition-colors"
                  >
                    <div className="flex items-start gap-3">
                      {!notif.daDoc && (
                        <div className="w-2 h-2 bg-[#4A90D9] rounded-full mt-2 flex-shrink-0" />
                      )}
                      {notif.daDoc && <div className="w-2 h-2 flex-shrink-0" />}
                      <div className="flex-1 min-w-0">
                        <h3 className="text-sm font-medium text-gray-800 mb-1">{notif.title}</h3>
                        <p className="text-xs text-gray-600 leading-relaxed mb-2">{notif.content}</p>
                        <p className="text-xs text-gray-400">{notif.timeAgo}</p>
                      </div>
                    </div>
                  </button>
                ))
              )}
            </div>

            <div className="mt-4 pt-4 border-t border-gray-100">
              <button
                onClick={() => router.push("/thongbao")}
                className="text-sm text-[#4A90D9] hover:underline w-full text-left"
              >
                Xem tất cả thông báo
              </button>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}
