"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"

import { Home, BookOpen, CheckSquare, Users, Calendar, Bell, ChevronLeft, LogOut } from "lucide-react"

export default function Sidebar({ currentPage = "tongquan", onPageChange = () => {}, onCollapseChange = () => {} }) {
  const router = useRouter()
  const [isCollapsed, setIsCollapsed] = useState(false)

  // Notify parent when collapse state changes
  useEffect(() => {
    onCollapseChange(isCollapsed)
  }, [isCollapsed, onCollapseChange])
  const [unreadCount, setUnreadCount] = useState(0)

  // Load unread count from localStorage on mount
  useEffect(() => {
    const count = localStorage.getItem("unreadNotificationCount")
    if (count) {
      setUnreadCount(parseInt(count, 10))
    } else {
      // Initialize with default count if not set
      setUnreadCount(2) // 2 unread notifications initially
      localStorage.setItem("unreadNotificationCount", "2")
    }
  }, [])

  // Listen for notification count updates
  useEffect(() => {
    const handleNotificationUpdate = (event) => {
      setUnreadCount(event.detail)
    }

    window.addEventListener("notificationCountUpdated", handleNotificationUpdate)

    return () => {
      window.removeEventListener("notificationCountUpdated", handleNotificationUpdate)
    }
  }, [])

  const menuItems = [
    { id: "tongquan", label: "Tổng quan", icon: Home },
    { id: "lophoc", label: "Lớp học", icon: BookOpen },
    { id: "baitap", label: "Bài tập", icon: CheckSquare },
    { id: "hocvien", label: "Học viên", icon: Users },
    { id: "lichday", label: "Lịch dạy", icon: Calendar },
    { id: "thongbao", label: "Thông báo", icon: Bell },
  ]

  return (
    <div
      className={`${
        isCollapsed ? "w-20" : "w-80"
      } fixed left-0 top-0 h-screen bg-white border-r border-gray-200 flex flex-col transition-all duration-300 z-30`}
      style={{ backgroundColor: "#FFFFFF" }}
    >
      {/* Logo Section */}
      <div className="flex items-center justify-between p-5 border-b border-gray-200">
        <div className={`flex items-center gap-3 ${isCollapsed ? "justify-center w-full" : ""}`}>
          <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-blue-800 rounded-lg flex items-center justify-center shadow-md">
            <span className="text-white font-bold text-base">S</span>
          </div>
          {!isCollapsed && (
            <div className="flex flex-col">
              <span className="text-blue-600 font-bold text-base leading-tight">Sea Dragon</span>
              <span className="text-blue-600 font-bold text-base leading-tight">LMS</span>
            </div>
          )}
        </div>
        <button
          onClick={() => setIsCollapsed(!isCollapsed)}
          className="p-1.5 hover:bg-gray-200 rounded transition-colors"
        >
          <ChevronLeft 
            size={20} 
            className={`text-gray-700 transition-transform duration-300 ${isCollapsed ? 'rotate-180' : ''}`} 
          />
        </button>
      </div>

      {/* Menu Items */}
      <nav className="flex-1 overflow-y-auto py-5">
        <ul className="space-y-2 px-3">
          {menuItems.map((item) => {
            const Icon = item.icon
            const isActive = currentPage === item.id
            return (
              <li key={item.id}>
                <button
                  onClick={() => onPageChange(item.id)}
                  className={`w-full flex items-center gap-4 px-4 py-3 rounded-lg transition-colors ${
                    isActive ? "bg-blue-100 text-blue-700 font-semibold" : "text-gray-700 hover:bg-gray-100"
                  }`}
                >
                  <Icon size={24} className="flex-shrink-0" />
                  {!isCollapsed && (
                    <>
                      <span className="flex-1 text-left text-lg font-medium">{item.label}</span>
                      {item.id === "thongbao" && unreadCount > 0 && (
                        <span className="bg-blue-500 text-white text-sm rounded-full w-7 h-7 flex items-center justify-center font-semibold">
                          {unreadCount}
                        </span>
                      )}
                    </>
                  )}
                </button>
              </li>
            )
          })}
        </ul>
      </nav>

      {/* User Section */}
      <div className="border-t border-gray-200 p-5 space-y-3">
        {/* User Profile */}
        <button
          onClick={() => router.push("/giangvien/avatar")}
          className={`w-full flex items-center hover:bg-gray-100 rounded-lg p-3 transition-colors ${
            isCollapsed ? "justify-center" : "gap-3"
          }`}
        >
          <div className="w-12 h-12 rounded-full flex-shrink-0 overflow-hidden cursor-pointer shadow-md bg-gradient-to-br from-orange-400 to-orange-600">
            <img 
              src="https://static.topcv.vn/company_logos/U1iwaehTitGAEylND0btRiLpHsuAtYyU_1719220999____94320d1131518f152cb3d6cc9617fbe5.png" 
              alt="Avatar" 
              className="w-full h-full object-cover"
            />
          </div>
          {!isCollapsed && (
            <div className="text-lg text-left">
              <p className="font-medium text-gray-800 truncate">Xin chào, Nguyễn Văn A</p>
            </div>
          )}
        </button>

        {/* Logout Button */}
        <button
          onClick={() => {
            // Clear authentication
            localStorage.removeItem("isAuthenticated")
            localStorage.removeItem("userEmail")
            // Redirect to login
            router.push("/giangvien/dangnhap")
          }}
          className="w-full flex items-center gap-3 px-4 py-3 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors font-medium"
        >
          <LogOut size={22} />
          {!isCollapsed && <span className="text-lg">Đăng xuất</span>}
        </button>
      </div>
    </div>
  )
}

