import { Geist, Geist_Mono } from "next/font/google"
import Script from "next/script"
import "./globals.css"
import LayoutWrapper from "@/components/layout-wrapper"

const _geist = Geist({ subsets: ["latin"] })
const _geistMono = Geist_Mono({ subsets: ["latin"] })

export const metadata = {
  title: "Sea Dragon LMS - Trưởng Khoa",
  description: "Hệ thống quản lý học tập Sea Dragon LMS",
  generator: 'v0.app'
}

export default function RootLayout({ children }) {
  return (
    <html lang="vi">
      <head>
        {/* Preconnect to Google for faster DNS lookup - helps reduce latency */}
        <link rel="preconnect" href="https://accounts.google.com" crossOrigin="anonymous" />
        <link rel="dns-prefetch" href="https://accounts.google.com" />
      </head>
      <body className="font-sans antialiased">
        <LayoutWrapper>
          {children}
        </LayoutWrapper>
      </body>
    </html>
  )
}
