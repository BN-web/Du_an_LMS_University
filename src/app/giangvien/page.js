"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"

export default function GiangVienPage() {
  const router = useRouter()

  useEffect(() => {
    // Check authentication
    const isAuthenticated = localStorage.getItem("isAuthenticated") === "true"

    if (isAuthenticated) {
      // Redirect to dashboard if authenticated
      router.push("/giangvien/tongquan")
    } else {
      // Redirect to login if not authenticated
      router.push("/giangvien/dangnhap")
    }
  }, [router])

  // Show loading while redirecting
  return (
    <div className="min-h-screen bg-white flex items-center justify-center">
      <div className="text-center">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-blue-600 border-t-transparent mb-4"></div>
        <p className="text-gray-600">Đang chuyển hướng...</p>
      </div>
    </div>
  )
}

