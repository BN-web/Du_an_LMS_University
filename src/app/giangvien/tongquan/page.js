"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { MapPin, Users, Building2, Bell, Database, Download, AlertCircle, CheckCircle, Clock, User } from "lucide-react"

import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts"

export default function TongQuan() {
  const router = useRouter()
  const [unreadCount, setUnreadCount] = useState(0)
  const [notifications, setNotifications] = useState([])

  // Load notifications from localStorage or use default
  useEffect(() => {
    const loadNotifications = () => {
      const savedNotifications = localStorage.getItem("notifications")
      if (savedNotifications) {
        const parsed = JSON.parse(savedNotifications)
        setNotifications(parsed)
        const unread = parsed.filter((n) => !n.read).length
        setUnreadCount(unread)
      } else {
        // Default notifications matching thongbao.js format exactly
        const defaultNotifications = [
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
        ]
        setNotifications(defaultNotifications)
        const unread = defaultNotifications.filter((n) => !n.read).length
        setUnreadCount(unread)
        localStorage.setItem("notifications", JSON.stringify(defaultNotifications))
        localStorage.setItem("unreadNotificationCount", unread.toString())
      }
    }

    loadNotifications()

    // Listen for storage changes to sync with thongbao page
    const handleStorageChange = () => {
      loadNotifications()
    }

    window.addEventListener("storage", handleStorageChange)
    // Also listen for custom event from thongbao page
    window.addEventListener("notificationsUpdated", handleStorageChange)

    return () => {
      window.removeEventListener("storage", handleStorageChange)
      window.removeEventListener("notificationsUpdated", handleStorageChange)
    }
  }, [])

  // Listen for notification count updates
  useEffect(() => {
    const handleNotificationUpdate = (event) => {
      setUnreadCount(event.detail)
      // Reload notifications from localStorage
      const savedNotifications = localStorage.getItem("notifications")
      if (savedNotifications) {
        setNotifications(JSON.parse(savedNotifications))
      }
    }

    window.addEventListener("notificationCountUpdated", handleNotificationUpdate)

    return () => {
      window.removeEventListener("notificationCountUpdated", handleNotificationUpdate)
    }
  }, [])

  // Handle click on notification to navigate to thongbao page
  const handleNotificationClick = () => {
    router.push("/giangvien/thongbao")
  }
  // Today's Schedule Data
  const schedules = [
    {
      id: 1,
      startTime: "7:00",
      endTime: "11:30",
      courseName: "Lập trình mobile",
      room: "Phòng 403",
      studentCount: 45,
    },
    {
      id: 2,
      startTime: "13:00",
      endTime: "18:00",
      courseName: "ASP.NET",
      room: "Phòng 201",
      studentCount: 40,
    },
  ]

  // Statistics Data
  const stats = [
    {
      id: 1,
      label: "Tổng số lớp",
      value: "4",
      icon: Building2,
      bgColor: "bg-blue-100",
      iconColor: "text-blue-600",
    },
    {
      id: 2,
      label: "Tổng học viên",
      value: "75",
      icon: Users,
      bgColor: "bg-blue-100",
      iconColor: "text-blue-600",
    },
  ]

  // Assignment Chart Data
  const chartData = [
    { name: "Database", "Đã nộp": 28, "Chưa nộp": 8 },
    { name: "Lập trình Web", "Đã nộp": 12, "Chưa nộp": 22 },
    { name: "ASP.NET", "Đã nộp": 23, "Chưa nộp": 14 },
    { name: "Mobile", "Đã nộp": 14, "Chưa nộp": 40 },
  ]

  // Recent Activity Data
  const activities = [
    {
      id: 1,
      text: "đã nộp bài",
      course: "Lập trình Mobile",
      boldName: "Trần Công Hoàng Phúc",
    },
    {
      id: 2,
      text: "Đã tạo một bài tập mới",
      course: "Lập trình Mobile",
      boldName: null,
    },
    {
      id: 3,
      text: "đã nghỉ quá số buổi",
      course: "Database",
      boldName: "Trần Công Hoàng Phúc",
    },
    {
      id: 4,
      text: "Đã đăng tài liệu học phần",
      course: "ASP.NET",
      boldName: null,
    },
    {
      id: 5,
      text: "Sinh viên Nguyễn Thị B đã nộp bài",
      course: "Lập trình Mobile",
      boldName: null,
    },
  ]

  return (
    <div className="flex-1 overflow-hidden h-screen" style={{ backgroundColor: "#e3f0ff", padding: "32px" }}>
      {/* Title */}
      <h1 className="text-2xl font-bold mb-3" style={{ color: "#083b74" }}>Tổng quan</h1>

      {/* Main Card Container */}
      <div className="bg-white rounded-2xl shadow-lg p-2.5 flex flex-col" style={{ height: "calc(100% - 60px)" }}>
        {/* Main Container */}
        <div className="flex gap-3 flex-1 min-h-0">
        {/* Left Side - 70% */}
        <div className="flex flex-col gap-3" style={{ width: "70%" }}>
          {/* Lịch dạy */}
          <div className="bg-white rounded-xl shadow-md p-3">
            <h3 className="text-base font-bold mb-2" style={{ color: "#083b74" }}>Lịch dạy hôm nay</h3>
            <div className="space-y-2">
              {schedules.map((schedule) => (
                <div
                  key={schedule.id}
                  className="flex gap-2 p-2 rounded-lg"
                  style={{ backgroundColor: "#f2f6ff" }}
                >
                  <div className="bg-white px-2 py-1 rounded-lg font-bold text-xs" style={{ color: "#083b74" }}>
                    {schedule.startTime} - {schedule.endTime}
                  </div>
                  <div className="flex-1">
                    <p className="font-bold mb-0.5 text-sm" style={{ color: "#083b74" }}>{schedule.courseName}</p>
                    <p className="text-xs" style={{ color: "#083b74" }}>
                      {schedule.room} • {schedule.studentCount} học viên
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Thống kê */}
          <div className="flex gap-3">
            {stats.map((stat) => {
              const StatIcon = stat.icon
              return (
                <div key={stat.id} className="bg-white rounded-xl shadow-md p-3 flex-1 text-center">
                  <StatIcon size={28} className={stat.iconColor} style={{ opacity: 0.8, margin: "0 auto 6px" }} />
                  <p className="text-xs mb-1" style={{ color: "#083b74" }}>{stat.label}</p>
                  <h2 className="text-2xl font-bold" style={{ color: "#083b74" }}>{stat.value}</h2>
                </div>
              )
            })}
            {/* Welcome Card */}
            <div
              className="bg-white rounded-xl shadow-md p-3 flex-1 flex items-center justify-center font-bold text-sm"
              style={{ backgroundColor: "#fff7d6", color: "#083b74" }}
            >
              👋 Xin chào, Nguyễn Văn B
            </div>
          </div>

          {/* Biểu đồ */}
          <div className="bg-white rounded-xl shadow-md p-3 flex-1 flex flex-col">
            <h3 className="text-base font-bold mb-2" style={{ color: "#083b74" }}>Biểu đồ hoàn thành bài tập</h3>
            <div className="w-full flex-1 min-h-0">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData} margin={{ top: 10, right: 20, left: 0, bottom: 30 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis dataKey="name" angle={0} tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #e5e7eb",
                      borderRadius: "8px",
                      fontSize: "12px",
                    }}
                  />
                  <Legend wrapperStyle={{ paddingTop: "10px", fontSize: "11px" }} iconType="square" />
                  <Bar dataKey="Đã nộp" fill="#28a745" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="Chưa nộp" fill="#ff9900" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>

        {/* Right Side - 30% */}
        <div className="flex flex-col gap-3" style={{ width: "30%" }}>
          {/* Thông báo */}
          <div className="bg-white rounded-xl shadow-md p-3 flex-1 flex flex-col min-h-0">
            <h3 className="text-base font-bold mb-2" style={{ color: "#083b74" }}>Thông báo mới</h3>
            <div className="space-y-2 overflow-y-auto flex-1">
              {(() => {
                const unreadNotifications = notifications
                  .filter((n) => !n.read)
                  .sort((a, b) => b.id - a.id)
                  .slice(0, 3)

                if (unreadNotifications.length === 0) {
                  return (
                    <div className="text-center py-4">
                      <p className="text-xs" style={{ color: "#083b74" }}>Không có thông báo mới</p>
                    </div>
                  )
                }

                return unreadNotifications.map((notif) => {
                  const displayTitle = notif.title.length > 25 ? notif.title.substring(0, 25) + "..." : notif.title
                  // Format: "14:00 • 14/02/2005" - using a simple format for now
                  const timeDisplay = notif.time.includes("giờ") || notif.time.includes("ngày") 
                    ? "14:00 • 14/02/2005" 
                    : notif.time
                  return (
                    <div
                      key={notif.id}
                      onClick={handleNotificationClick}
                      className="pb-2 border-b border-gray-300 last:border-b-0 cursor-pointer hover:opacity-80 transition-opacity"
                    >
                      <p className="font-bold mb-1 text-xs" style={{ color: "#083b74" }}>
                        {displayTitle}
                      </p>
                      <small className="text-[10px]" style={{ color: "#083b74" }}>
                        {timeDisplay}
                        <br />
                        Người gửi: {notif.sender}
                      </small>
                    </div>
                  )
                })
              })()}
            </div>
          </div>

          {/* Hoạt động */}
          <div className="bg-white rounded-xl shadow-md p-3 flex-1 flex flex-col min-h-0">
            <h3 className="text-base font-bold mb-2" style={{ color: "#083b74" }}>Hoạt động gần đây</h3>
            <div className="space-y-2 overflow-hidden flex-1">
              {activities.map((activity) => {
                return (
                  <div
                    key={activity.id}
                    className="p-2 rounded-lg"
                    style={{ backgroundColor: "#f5f9ff" }}
                  >
                    <p className="text-xs" style={{ color: "#083b74" }}>
                      {activity.boldName ? (
                        <>
                          <b>{activity.boldName}</b> {activity.text}
                        </>
                      ) : (
                        activity.text
                      )}{" "}
                      <small className="block mt-0.5 text-[10px]" style={{ color: "#083b74" }}>
                        {activity.course}
                      </small>
                    </p>
                  </div>
                )
              })}
            </div>
          </div>
        </div>
      </div>
      </div>
    </div>
  )
}

