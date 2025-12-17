"use client"

import { useState, useEffect } from "react"
import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { Home, BookOpen, Users, GraduationCap, Calendar, Bell, ChevronLeft, LogOut } from "lucide-react"
import { recentNotifications } from "@/lib/data"
import { authUtils } from "@/lib/auth"
import { apiClient } from "@/lib/api"

const menuItems = [
  { icon: Home, label: "Tổng quan", href: "/tongquan" },
  { icon: BookOpen, label: "Lớp học", href: "/lophoc" },
  { icon: Users, label: "Học viên", href: "/hocvien" },
  { icon: GraduationCap, label: "Giảng viên", href: "/giangvien" },
  { icon: Calendar, label: "Lịch dạy", href: "/lichday" },
  { icon: Bell, label: "Thông báo", href: "/thongbao" },
]

export default function Sidebar() {
  const pathname = usePathname()
  const router = useRouter()
  const [userInfo, setUserInfo] = useState({ hoTen: "", avatar: null })

  // Khởi tạo giống nhau giữa server và client để tránh lỗi hydration
  const [unreadCount, setUnreadCount] = useState(recentNotifications.filter((n) => !n.daDoc).length)

  useEffect(() => {
    const stored = typeof window !== "undefined" ? localStorage.getItem("unreadNotifications") : null
    if (stored) {
      setUnreadCount(parseInt(stored))
    }

    const handleNotificationsUpdate = () => {
      const latest = localStorage.getItem("unreadNotifications")
      setUnreadCount(latest ? parseInt(latest) : 0)
    }

    window.addEventListener("notificationsUpdated", handleNotificationsUpdate)
    return () => window.removeEventListener("notificationsUpdated", handleNotificationsUpdate)
  }, [])

  // Lấy thông tin user từ API
  useEffect(() => {
    const fetchUserInfo = async () => {
      try {
        const response = await apiClient.get("/api/truong-khoa/ho-so")
        if (response.success && response.data) {
          setUserInfo({
            hoTen: response.data.hoTen || "",
            avatar: response.data.avatar || null,
          })
        }
      } catch (err) {
        console.error("Lỗi khi tải thông tin user:", err)
        // Fallback: lấy từ JWT token nếu API fail
        const jwtInfo = authUtils.getUserInfo()
        if (jwtInfo?.fullName) {
          setUserInfo({
            hoTen: jwtInfo.fullName,
            avatar: jwtInfo.avatar || null,
          })
        }
      }
    }

    if (authUtils.isAuthenticated()) {
      fetchUserInfo()
    }
  }, [])

  const handleLogout = () => {
    authUtils.removeToken()
    router.push("/login")
  }

  return (
    <aside className="w-[250px] bg-white flex flex-col border-r border-gray-100 min-h-screen">
      <div className="p-4 flex items-center gap-2">
        <div className="w-10 h-10 flex items-center justify-center">
          <svg viewBox="0 0 40 40" className="w-10 h-10">
            <circle cx="20" cy="20" r="18" fill="#4A90D9" />
            <path d="M12 20 Q20 10 28 20 Q20 30 12 20" fill="white" />
            <circle cx="24" cy="18" r="3" fill="#FF6B6B" />
          </svg>
        </div>
        <span className="text-[#4A90D9] font-semibold text-lg">Sea Dragon LMS</span>
        <ChevronLeft className="w-5 h-5 text-[#FF6B6B] ml-auto" />
      </div>

      <nav className="flex-1 px-3 py-2">
        {menuItems.map((item, index) => {
          const isActive = pathname === item.href || (pathname === "/" && item.href === "/tongquan")
          return (
            <Link
              key={index}
              href={item.href}
              className={`flex items-center gap-3 px-4 py-3 rounded-lg mb-1 ${
                isActive ? "bg-[#E8F4FD] text-[#4A90D9]" : "text-gray-600 hover:bg-gray-50"
              }`}
            >
              <item.icon className="w-5 h-5" />
              <span className="text-sm font-medium">{item.label}</span>
              {item.href === "/thongbao" && unreadCount > 0 && (
                <span className="ml-auto bg-[#4A90D9] text-white text-xs w-5 h-5 rounded-full flex items-center justify-center">
                  {unreadCount}
                </span>
              )}
            </Link>
          )
        })}
      </nav>

      <div className="p-4 border-t border-gray-100">
        <div className="flex items-center gap-3 mb-3">
          <button
            onClick={() => router.push("/ho-so-truong-khoa")}
            className="flex items-center gap-3 hover:opacity-80 transition-opacity w-full"
          >
            {userInfo.avatar ? (
              <img
                src={userInfo.avatar}
                alt="Avatar"
                className="w-10 h-10 rounded-full object-cover cursor-pointer"
                onError={(e) => {
                  // Nếu ảnh không load được, ẩn img và hiển thị placeholder
                  e.target.style.display = "none"
                  const placeholder = e.target.parentElement?.querySelector(".avatar-placeholder")
                  if (placeholder) {
                    placeholder.classList.remove("hidden")
                  }
                }}
              />
            ) : null}
            {/* Placeholder avatar nếu không có ảnh hoặc ảnh lỗi */}
            <div
              className={`w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-lg font-bold avatar-placeholder ${userInfo.avatar ? "hidden" : ""}`}
            >
              {userInfo.hoTen ? userInfo.hoTen.charAt(0).toUpperCase() : "T"}
            </div>
            <span className="text-sm font-medium text-gray-700 flex-1 text-left truncate">
              Xin chào, {userInfo.hoTen || "Trưởng khoa"}
            </span>
          </button>
        </div>
        <button
          onClick={handleLogout}
          className="w-full bg-[#F5A623] text-white py-2.5 rounded-lg flex items-center justify-center gap-2 text-sm font-medium hover:bg-[#e09620] transition-colors"
        >
          <LogOut className="w-4 h-4" />
          Đăng xuất
        </button>
      </div>
    </aside>
  )
}
