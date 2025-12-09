"use client"

import { useState, useEffect } from "react"

import { ChevronDown, X } from "lucide-react"

export default function ThongBao() {
  const [notifications, setNotifications] = useState([
    {
      id: 1,
      title: "Thông báo lịch họp Khoa tháng 11/2025",
      sender: "TS.Nguyễn Hải Trường",
      department: "Trường khoa CNTT",
      time: "Hôm qua",
      content:
        "Kính gửi các thầy có giảng viên, Khoa Công nghệ thông tin trân trọng thông báo lịch họp định kỳ tháng 11/2025 như sau: - Thời gian: 14h00, Thứ Sáu, ngày 15/11/2025 - Địa điểm: Phòng họp 201 - Nội dung: +Báo cáo tình hình giảng dạy tháng 10 + Kế hoạch tổ chức thi cuối kỳ + Triển khai công tác nghiên cứu khoa học. Để nghỉ các thầy có sắp xếp thời gian tham dự đầy đủ. Trân trọng...",
      read: false,
    },
    {
      id: 2,
      title: "Thông báo lịch họp Khoa tháng 11/2025",
      sender: "TS.Nguyễn Hải Trường",
      department: "Trường khoa CNTT",
      time: "Hôm qua",
      content:
        "Kính gửi các thầy có giảng viên, Khoa Công nghệ thông tin trân trọng thông báo lịch họp định kỳ tháng 11/2025 như sau: - Thời gian: 14h00, Thứ Sáu, ngày 15/11/2025 - Địa điểm: Phòng họp 201 - Nội dung: +Báo cáo tình hình giảng dạy tháng 10 + Kế hoạch tổ chức thi cuối kỳ + Triển khai công tác nghiên cứu khoa học. Để nghỉ các thầy có sắp xếp thời gian tham dự đầy đủ. Trân trọng...",
      read: false,
    },
    {
      id: 3,
      title: "Thông báo lịch họp Khoa tháng 11/2025",
      sender: "TS.Nguyễn Hải Trường",
      department: "Trường khoa CNTT",
      time: "Hôm qua",
      content:
        "Kính gửi các thầy có giảng viên, Khoa Công nghệ thông tin trân trọng thông báo lịch họp định kỳ tháng 11/2025 như sau: - Thời gian: 14h00, Thứ Sáu, ngày 15/11/2025 - Địa điểm: Phòng họp 201 - Nội dung: +Báo cáo tình hình giảng dạy tháng 10 + Kế hoạch tổ chức thi cuối kỳ + Triển khai công tác nghiên cứu khoa học. Để nghỉ các thầy có sắp xếp thời gian tham dự đầy đủ. Trân trọng...",
      read: true,
    },
    {
      id: 4,
      title: "Thông báo về lịch thi cuối kỳ học kỳ 1",
      sender: "PGS.TS. Lê Văn Nam",
      department: "Phòng Đào tạo",
      time: "2 giờ trước",
      content:
        "Kính gửi các thầy cô giảng viên, Phòng Đào tạo thông báo về lịch thi cuối kỳ học kỳ 1 năm học 2025-2026. Lịch thi sẽ được công bố trên hệ thống vào ngày 20/11/2025. Các thầy cô vui lòng kiểm tra và chuẩn bị đề thi theo đúng quy định. Mọi thắc mắc xin liên hệ Phòng Đào tạo. Trân trọng!",
      read: false,
    },
    {
      id: 5,
      title: "Thông báo về việc nộp báo cáo giảng dạy tháng 11",
      sender: "TS. Trần Thị Mai",
      department: "Trường khoa CNTT",
      time: "5 giờ trước",
      content:
        "Kính gửi các thầy cô, Khoa CNTT yêu cầu các thầy cô nộp báo cáo giảng dạy tháng 11/2025 trước ngày 30/11/2025. Báo cáo cần bao gồm: tiến độ giảng dạy, số lượng sinh viên tham gia, kết quả đánh giá giữa kỳ. Vui lòng nộp báo cáo qua hệ thống LMS. Trân trọng cảm ơn!",
      read: false,
    },
    {
      id: 6,
      title: "Thông báo về hội thảo khoa học công nghệ thông tin",
      sender: "PGS.TS. Phạm Đức Hùng",
      department: "Trường khoa CNTT",
      time: "1 ngày trước",
      content:
        "Kính gửi các thầy cô, Khoa CNTT tổ chức hội thảo khoa học về Công nghệ thông tin vào ngày 25/11/2025 tại Hội trường lớn. Chương trình bao gồm các bài báo cáo về AI, Machine Learning, và Blockchain. Mời các thầy cô tham gia và đóng góp ý kiến. Đăng ký tham gia trước ngày 20/11/2025. Trân trọng!",
      read: false,
    },
    {
      id: 7,
      title: "Thông báo về việc cập nhật hệ thống LMS",
      sender: "Phòng CNTT",
      department: "Phòng Công nghệ thông tin",
      time: "3 ngày trước",
      content:
        "Kính gửi các thầy cô, Hệ thống LMS sẽ được nâng cấp và bảo trì vào ngày 18/11/2025 từ 22h00 đến 02h00 ngày 19/11/2025. Trong thời gian này, hệ thống sẽ tạm thời ngừng hoạt động. Các thầy cô vui lòng lưu lại công việc trước thời điểm bảo trì. Xin cảm ơn sự hợp tác của quý thầy cô!",
      read: false,
    },
  ])

  const [expandedId, setExpandedId] = useState(null)
  const [selectedNotification, setSelectedNotification] = useState(null)

  // Load read notifications from localStorage on mount
  useEffect(() => {
    const readNotifications = localStorage.getItem("readNotifications")
    if (readNotifications) {
      const readIds = JSON.parse(readNotifications)
      setNotifications((prev) =>
        prev.map((notif) => ({
          ...notif,
          read: readIds.includes(notif.id),
        }))
      )
    }
  }, [])

  // Update unread count after notifications change
  useEffect(() => {
    const unreadCount = notifications.filter((n) => !n.read).length
    localStorage.setItem("unreadNotificationCount", unreadCount.toString())
    // Use setTimeout to ensure this runs after render
    setTimeout(() => {
      window.dispatchEvent(new CustomEvent("notificationCountUpdated", { detail: unreadCount }))
    }, 0)
  }, [notifications])

  const toggleExpand = (id) => {
    const notification = notifications.find((n) => n.id === id)
    if (notification) {
      const isOpening = selectedNotification?.id !== id
      
      if (isOpening) {
        // Opening modal - mark as read if not already read
        setSelectedNotification(notification)
        
        if (!notification.read) {
          const updatedNotifications = notifications.map((n) =>
            n.id === id ? { ...n, read: true } : n
          )
          setNotifications(updatedNotifications)

          // Save read notification IDs to localStorage
          const readIds = updatedNotifications.filter((n) => n.read).map((n) => n.id)
          localStorage.setItem("readNotifications", JSON.stringify(readIds))

          // Update unread count - dispatch event after state update
          const unreadCount = updatedNotifications.filter((n) => !n.read).length
          localStorage.setItem("unreadNotificationCount", unreadCount.toString())
          setTimeout(() => {
            window.dispatchEvent(new CustomEvent("notificationCountUpdated", { detail: unreadCount }))
          }, 0)
        }
      } else {
        // Closing modal
        setSelectedNotification(null)
      }
    }
  }

  return (
    <>
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-blue-100" style={{ padding: "32px" }}>
        <div className="max-w-4xl mx-auto">
          {/* Title outside card */}
          <h1 className="text-2xl font-bold mb-3" style={{ color: "#083b74" }}>Thông báo</h1>

          {/* Main Card Container */}
          <div className="bg-white rounded-2xl shadow-lg p-2.5">
            <div className="space-y-4">
              {notifications.map((notif) => (
                <div
                  key={notif.id}
                  onClick={() => toggleExpand(notif.id)}
                  className="bg-white rounded-lg shadow-sm p-6 hover:shadow-md transition-shadow border-l-4 border-blue-500 cursor-pointer"
                >
                  <div className="flex items-start gap-4">
                    {!notif.read && <div className="w-3 h-3 bg-blue-600 rounded-full mt-2 flex-shrink-0"></div>}
                    <div className="flex-1">
                      <h3 className="text-lg font-bold text-gray-900">{notif.title}</h3>

                      <div className="flex items-center gap-2 mt-2 text-gray-600">
                        <div className="w-5 h-5 rounded-full bg-gray-300 flex items-center justify-center text-xs">👤</div>
                        <span className="text-sm">
                          {notif.sender} <span className="font-semibold">• {notif.department}</span>
                        </span>
                      </div>

                      <div className="flex items-center gap-2 mt-2 text-gray-500 text-sm">
                        <span>📅</span>
                        <span>{notif.time}</span>
                      </div>

                      <p className="text-gray-700 mt-3 leading-relaxed text-sm line-clamp-2">{notif.content}</p>
                    </div>

                    <div className="flex-shrink-0 p-2 text-gray-400">
                      <ChevronDown size={20} />
                    </div>
                  </div>
                </div>
              ))}

              {notifications.length === 0 && (
                <div className="text-center py-12">
                  <p className="text-gray-500 text-lg">Không có thông báo nào</p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {selectedNotification && (
        <>
          <div
            className="fixed inset-0 bg-black/10 backdrop-blur-sm z-40 cursor-pointer"
            onClick={() => setSelectedNotification(null)}
          />

          <div className="fixed top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 bg-white rounded-lg shadow-2xl z-50 w-full max-w-2xl max-h-screen overflow-y-auto">
            <div className="sticky top-0 bg-white border-b p-6 flex items-center justify-between rounded-t-lg">
              <h2 className="text-2xl font-bold text-gray-900">{selectedNotification.title}</h2>
              <button
                onClick={() => setSelectedNotification(null)}
                className="p-1 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <X size={24} className="text-gray-600" />
              </button>
            </div>

            <div className="p-6">
              <div className="flex items-center gap-2 mb-4 text-gray-600">
                <div className="w-5 h-5 rounded-full bg-gray-300 flex items-center justify-center text-xs">👤</div>
                <span className="text-sm">
                  {selectedNotification.sender}{" "}
                  <span className="font-semibold">• {selectedNotification.department}</span>
                </span>
              </div>

              <div className="flex items-center gap-2 mb-6 text-gray-500 text-sm">
                <span>📅</span>
                <span>{selectedNotification.time}</span>
              </div>

              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-gray-700 leading-relaxed whitespace-pre-wrap">{selectedNotification.content}</p>
              </div>
            </div>
          </div>
        </>
      )}
    </>
  )
}

