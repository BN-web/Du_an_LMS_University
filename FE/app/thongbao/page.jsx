"use client"

import { useState, useMemo, useEffect } from "react"
import { Bell, User, Calendar, Plus, Send, X } from "lucide-react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { apiClient } from "@/lib/api"
import { authUtils } from "@/lib/auth"

export default function ThongBaoPage() {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false)
  const [selectedThongBao, setSelectedThongBao] = useState(null)
  const [thongBaoList, setThongBaoList] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [formData, setFormData] = useState({
    phamVi: "all",
    groupId: null,
    tieuDe: "",
    noiDung: "",
  })

  const phamViList = [
    { value: "all", label: "Toàn khoa" },
    { value: "nganh", label: "Ngành" },
    { value: "khoa-tuyen-sinh", label: "Khóa tuyển sinh" },
    { value: "mon", label: "Môn" },
    { value: "lop", label: "Lớp" },
    { value: "giang-vien", label: "Giảng viên" },
    { value: "sv", label: "Sinh viên" },
  ]

  useEffect(() => {
    const fetchThongBao = async () => {
      try {
        setIsLoading(true)
        const dashboardData = await apiClient.get("/api/truong-khoa/dashboard/summary")
        
        // Lấy danh sách thông báo đã đọc từ localStorage
        const readNotifications = JSON.parse(localStorage.getItem("readNotifications") || "[]")
        
        const thongBaos = (dashboardData.thongBaoGanDay || []).map((tb) => ({
          id: tb.id,
          tieuDe: tb.tieuDe,
          nguoiGui: tb.nguoiDang || "Hệ thống",
          chucVu: "",
          thoiGian: tb.ngayTao ? new Date(tb.ngayTao).toLocaleString("vi-VN") : "",
          noiDung: tb.noiDung || tb.tieuDe, // Sửa: lấy noiDung thay vì tieuDe
          doiTuong: "Tất cả",
          phamVi: "all",
          daDoc: readNotifications.includes(tb.id),
        }))
        setThongBaoList(thongBaos)
      } catch (error) {
        console.error("Lỗi khi tải thông báo:", error)
        setThongBaoList([])
      } finally {
        setIsLoading(false)
      }
    }

    fetchThongBao()
  }, [])

  const stats = useMemo(() => {
    const today = new Date()
    const todayStr = today.toLocaleDateString("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric" })

    return {
      daGui: thongBaoList.length,
      tatCa: thongBaoList.length,
      homNay: thongBaoList.filter((tb) => {
        if (!tb.thoiGian) return false
        const tbDate = tb.thoiGian.split(" ")[0]
        return tbDate === todayStr
      }).length,
    }
  }, [thongBaoList])

  // Xử lý click vào thông báo để xem chi tiết và đánh dấu đã đọc
  const handleThongBaoClick = (tb) => {
    // Mở modal chi tiết
    setSelectedThongBao(tb)
    setIsDetailModalOpen(true)
    
    // Đánh dấu thông báo đã đọc nếu chưa đọc
    if (!tb.daDoc) {
      const readNotifications = JSON.parse(localStorage.getItem("readNotifications") || "[]")
      if (!readNotifications.includes(tb.id)) {
        readNotifications.push(tb.id)
        localStorage.setItem("readNotifications", JSON.stringify(readNotifications))
      }
      
      // Đánh dấu thông báo đã đọc
      const updatedList = thongBaoList.map((item) =>
        item.id === tb.id ? { ...item, daDoc: true } : item
      )
      setThongBaoList(updatedList)

      // Cập nhật số thông báo chưa đọc
      const newUnreadCount = updatedList.filter((item) => !item.daDoc).length
      localStorage.setItem("unreadNotifications", newUnreadCount.toString())
      window.dispatchEvent(new Event("notificationsUpdated"))
    }
  }

  const handleCreateNotification = async () => {
    if (!formData.tieuDe || !formData.noiDung) {
      alert("Vui lòng điền đầy đủ thông tin")
      return
    }

    try {
      const userId = authUtils.getUserId()
      if (!userId) {
        alert("Không tìm thấy thông tin người dùng")
        return
      }

      await apiClient.post("/api/truong-khoa/thong-bao", {
        tieuDe: formData.tieuDe,
        noiDung: formData.noiDung,
        nguoiDangId: parseInt(userId),
        target: formData.phamVi,
        groupId: formData.groupId ? parseInt(formData.groupId) : null,
      })

      alert("Tạo thông báo thành công")
      setFormData({
        phamVi: "all",
        groupId: null,
        tieuDe: "",
        noiDung: "",
      })
      setIsCreateModalOpen(false)

      const dashboardData = await apiClient.get("/api/truong-khoa/dashboard/summary")
      
      // Lấy danh sách thông báo đã đọc từ localStorage
      const readNotifications = JSON.parse(localStorage.getItem("readNotifications") || "[]")
      
      const thongBaos = (dashboardData.thongBaoGanDay || []).map((tb) => ({
        id: tb.id,
        tieuDe: tb.tieuDe,
        nguoiGui: tb.nguoiDang || "Hệ thống",
        chucVu: "",
        thoiGian: tb.ngayTao ? new Date(tb.ngayTao).toLocaleString("vi-VN") : "",
        noiDung: tb.noiDung || tb.tieuDe,
        doiTuong: "Tất cả",
        phamVi: "all",
        daDoc: readNotifications.includes(tb.id),
      }))
      setThongBaoList(thongBaos)
      
      // Cập nhật số thông báo chưa đọc
      const unreadCount = thongBaos.filter((tb) => !tb.daDoc).length
      localStorage.setItem("unreadNotifications", unreadCount.toString())
      window.dispatchEvent(new Event("notificationsUpdated"))
    } catch (error) {
      console.error("Lỗi khi tạo thông báo:", error)
      alert(error.message || "Có lỗi xảy ra khi tạo thông báo")
    }
  }

  return (
    <main className="flex-1 p-6">
      <h1 className="text-[#4A90D9] text-2xl font-bold mb-6">Thông báo</h1>

      {/* Thẻ thống kê */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#4A90D9] shadow-sm">
          <p className="text-[#4A90D9] text-sm font-medium mb-1">Đã gửi</p>
          <p className="text-[#4A90D9] text-2xl font-bold">{stats.daGui}</p>
        </div>
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#22C55E] shadow-sm">
          <p className="text-[#22C55E] text-sm font-medium mb-1">Tất cả thông báo</p>
          <p className="text-[#22C55E] text-2xl font-bold">{stats.tatCa}</p>
        </div>
        <div className="bg-white rounded-xl p-5 border-l-4 border-[#F5A623] shadow-sm">
          <p className="text-[#F5A623] text-sm font-medium mb-1">Thông báo hôm nay</p>
          <p className="text-[#F5A623] text-2xl font-bold">{stats.homNay}</p>
        </div>
      </div>

      {/* Lịch sử thông báo */}
      <div className="bg-white rounded-xl p-6 shadow-sm">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-gray-800 font-semibold text-lg">Lịch sử thông báo</h2>
          <button
            onClick={() => setIsCreateModalOpen(true)}
            className="flex items-center gap-2 px-4 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8] transition-colors"
          >
            <Plus className="w-4 h-4" />
            Tạo thông báo mới
          </button>
        </div>

        {/* Danh sách thông báo */}
        <div className="space-y-6">
          {isLoading ? (
            <div className="text-center py-8 text-gray-500">Đang tải...</div>
          ) : thongBaoList.length === 0 ? (
            <div className="text-center py-8 text-gray-500">Không có thông báo</div>
          ) : (
            thongBaoList.map((tb) => (
            <div 
              key={tb.id} 
              className="border-b border-gray-200 pb-6 last:border-b-0 last:pb-0 cursor-pointer hover:bg-gray-50 p-3 -mx-3 rounded-lg transition-colors"
              onClick={() => handleThongBaoClick(tb)}
            >
              {/* Chấm màu xanh và tiêu đề */}
              <div className="flex items-start gap-3 mb-3">
                <div className={`w-2 h-2 rounded-full mt-2 flex-shrink-0 ${tb.daDoc ? "bg-gray-300" : "bg-[#4A90D9]"}`} />
                <h3 className="text-lg font-bold text-gray-800 flex-1">{tb.tieuDe}</h3>
              </div>

              {/* Người gửi và thời gian */}
              <div className="flex items-center gap-4 mb-3 text-sm text-gray-600">
                {tb.nguoiGui && (
                  <div className="flex items-center gap-2">
                    <User className="w-4 h-4" />
                    <span>
                      {tb.nguoiGui}
                      {tb.chucVu && ` • ${tb.chucVu}`}
                    </span>
                  </div>
                )}
                <div className="flex items-center gap-2">
                  <Calendar className="w-4 h-4" />
                  <span>{tb.thoiGian}</span>
                </div>
              </div>

              {/* Nội dung */}
              <p className="text-sm text-gray-700 mb-3 leading-relaxed whitespace-pre-line">{tb.noiDung}</p>

              {/* Đối tượng nhận */}
              <div className="flex items-center gap-2 text-sm text-gray-600">
                <Bell className="w-4 h-4" />
                <span>{tb.doiTuong}</span>
              </div>
            </div>
            ))
          )}
        </div>
      </div>

      {/* Modal Chi tiết thông báo */}
      <Dialog open={isDetailModalOpen} onOpenChange={setIsDetailModalOpen}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          {selectedThongBao && (
            <>
              <DialogHeader>
                <DialogTitle className="text-2xl font-bold text-gray-800 mb-2">
                  {selectedThongBao.tieuDe}
                </DialogTitle>
              </DialogHeader>

              <div className="space-y-4 mt-4">
                {/* Người gửi và thời gian */}
                <div className="flex items-center gap-4 text-sm text-gray-600 pb-4 border-b border-gray-200">
                  {selectedThongBao.nguoiGui && (
                    <div className="flex items-center gap-2">
                      <User className="w-4 h-4" />
                      <span>
                        {selectedThongBao.nguoiGui}
                        {selectedThongBao.chucVu && ` • ${selectedThongBao.chucVu}`}
                      </span>
                    </div>
                  )}
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4" />
                    <span>{selectedThongBao.thoiGian}</span>
                  </div>
                </div>

                {/* Nội dung */}
                <div className="py-4">
                  <p className="text-base text-gray-700 leading-relaxed whitespace-pre-line">
                    {selectedThongBao.noiDung}
                  </p>
                </div>

                {/* Đối tượng nhận */}
                <div className="flex items-center gap-2 text-sm text-gray-600 pt-4 border-t border-gray-200">
                  <Bell className="w-4 h-4" />
                  <span>Đối tượng: {selectedThongBao.doiTuong}</span>
                </div>
              </div>
            </>
          )}
        </DialogContent>
      </Dialog>

      {/* Modal Tạo thông báo mới */}
      <Dialog open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="text-2xl font-bold text-gray-800 mb-2">Tạo thông báo mới</DialogTitle>
            <p className="text-sm text-gray-600">Soạn và gửi thông báo đến giảng viên hoặc sinh viên</p>
          </DialogHeader>

          <div className="space-y-4 mt-6">
            {/* Phạm vi gửi */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Phạm vi gửi</label>
              <div className="relative">
                <select
                  value={formData.phamVi}
                  onChange={(e) => setFormData({ ...formData, phamVi: e.target.value, groupId: null })}
                  className="w-full px-4 py-2.5 pr-10 border border-gray-200 rounded-lg text-sm bg-gray-50 focus:outline-none focus:ring-2 focus:ring-[#4A90D9] appearance-none"
                >
                  {phamViList.map((pv) => (
                    <option key={pv.value} value={pv.value}>
                      {pv.label}
                    </option>
                  ))}
                </select>
                <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
                  <svg className="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </div>
              </div>
            </div>

            {/* Tiêu đề */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Tiêu đề</label>
              <input
                type="text"
                placeholder="Nhập tiêu đề thông báo..."
                value={formData.tieuDe}
                onChange={(e) => setFormData({ ...formData, tieuDe: e.target.value })}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm bg-gray-50 focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
              />
            </div>

            {/* Nội dung */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Nội dung</label>
              <textarea
                placeholder="Nhập nội dung thông báo..."
                value={formData.noiDung}
                onChange={(e) => setFormData({ ...formData, noiDung: e.target.value })}
                rows={8}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm bg-gray-50 focus:outline-none focus:ring-2 focus:ring-[#4A90D9] resize-none"
              />
            </div>

            {/* Nút hành động */}
            <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
              <button
                onClick={() => {
                  setIsCreateModalOpen(false)
                  setFormData({
                    phamVi: "all",
                    groupId: null,
                    tieuDe: "",
                    noiDung: "",
                  })
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
              >
                Hủy
              </button>
              <button
                onClick={handleCreateNotification}
                className="flex items-center gap-2 px-4 py-2 bg-gray-900 text-white rounded-lg text-sm font-medium hover:bg-gray-800 transition-colors"
              >
                <Send className="w-4 h-4" />
                Gửi thông báo
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </main>
  )
}
