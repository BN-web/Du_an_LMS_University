"use client"

import { useEffect, useState } from "react"
import { usePathname, useRouter } from "next/navigation"
import Sidebar from "./sidebar"

export default function GiangVienLayout({ children }) {
  const pathname = usePathname()
  const router = useRouter()
  const [isChecking, setIsChecking] = useState(true)
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false)

  // Check if we're on login page
  const isLoginPage = pathname.includes("/dangnhap")

  // Check authentication
  useEffect(() => {
    const checkAuth = () => {
      // Skip check if pathname is exactly /giangvien (handled by page.js)
      if (pathname === "/giangvien") {
        setIsChecking(false)
        return
      }

      const isAuthenticated = localStorage.getItem("isAuthenticated") === "true"

      if (!isAuthenticated && !isLoginPage) {
        // Not authenticated and not on login page - redirect to login
        router.push("/giangvien/dangnhap")
        return
      }

      if (isAuthenticated && isLoginPage) {
        // Authenticated but on login page - redirect to dashboard
        router.push("/giangvien/tongquan")
        return
      }

      // Allow access
      setIsChecking(false)
    }

    checkAuth()
  }, [pathname, router, isLoginPage])

  // Determine current page from pathname
  const getCurrentPage = () => {
    if (pathname.includes("/tongquan")) return "tongquan"
    if (pathname.includes("/lophoc")) return "lophoc"
    if (pathname.includes("/baitap")) return "baitap"
    if (pathname.includes("/hocvien")) return "hocvien"
    if (pathname.includes("/lichday")) return "lichday"
    if (pathname.includes("/thongbao")) return "thongbao"
    if (pathname.includes("/avatar")) return "avatar"
    return "tongquan"
  }

  const handlePageChange = (pageId) => {
    const routes = {
      tongquan: "/giangvien/tongquan",
      lophoc: "/giangvien/lophoc",
      baitap: "/giangvien/baitap",
      hocvien: "/giangvien/hocvien",
      lichday: "/giangvien/lichday",
      thongbao: "/giangvien/thongbao",
    }
    if (routes[pageId]) {
      router.push(routes[pageId])
    }
  }

  // Show loading while checking authentication
  if (isChecking) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-blue-600 border-t-transparent mb-4"></div>
          <p className="text-gray-600">Đang kiểm tra...</p>
        </div>
      </div>
    )
  }

  // If login page, don't show sidebar
  if (isLoginPage) {
    return <>{children}</>
  }

  return (
    <div className="flex min-h-screen bg-white">
      <Sidebar 
        currentPage={getCurrentPage()} 
        onPageChange={handlePageChange}
        onCollapseChange={setIsSidebarCollapsed}
      />
      <main 
        className="flex-1 bg-white min-h-screen transition-all duration-300"
        style={{
          marginLeft: isSidebarCollapsed ? '80px' : '320px'
        }}
      >
        {children}
      </main>
    </div>
  )
}

