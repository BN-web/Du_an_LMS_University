"use client"

import { useEffect } from "react"
import { usePathname, useRouter } from "next/navigation"
import Sidebar from "@/components/sidebar"
import { authUtils } from "@/lib/auth"

export default function LayoutWrapper({ children }) {
  const pathname = usePathname()
  const router = useRouter()
  const isLoginPage = pathname === "/login"

  useEffect(() => {
    // Nếu đang ở trang login thì luôn xoá token để đảm bảo trạng thái đăng xuất
    if (isLoginPage) {
      authUtils.removeToken()
      return
    }

    // Các trang khác bắt buộc phải đăng nhập
    if (!authUtils.isAuthenticated()) {
      router.replace("/login")
    }
  }, [isLoginPage, router])

  if (isLoginPage) {
    return <>{children}</>
  }

  return (
    <div className="flex min-h-screen bg-[#F8FAFC] overflow-hidden">
      <Sidebar />
      <div className="flex-1 overflow-y-auto">{children}</div>
    </div>
  )
}
