"use client"

import { useState, useEffect, useRef } from "react"
import { useRouter } from "next/navigation"
import { Mail, Lock, Eye, EyeOff, AlertCircle, Loader2 } from "lucide-react"
import { apiClient } from "@/lib/api"
import { authUtils } from "@/lib/auth"
import { InputOTP, InputOTPGroup, InputOTPSlot } from "@/components/ui/input-otp"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"

export default function LoginPage() {
  const router = useRouter()
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [rememberMe, setRememberMe] = useState(true)
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState("")
  
  // Google OAuth states
  const [showOtpDialog, setShowOtpDialog] = useState(false)
  const [otp, setOtp] = useState("")
  const [otpError, setOtpError] = useState("")
  const [otpLoading, setOtpLoading] = useState(false)
  const [googleUserInfo, setGoogleUserInfo] = useState(null)
  const [otpAttempts, setOtpAttempts] = useState(0)
  const [otpExpiryTime, setOtpExpiryTime] = useState(null)
  const [googleScriptLoaded, setGoogleScriptLoaded] = useState(false)
  const googleButtonRef = useRef(null)
  const isInitializedRef = useRef(false) // Cache để tránh init nhiều lần

  useEffect(() => {
    // Chỉ load Google script nếu người dùng chưa đăng nhập
    if (authUtils.isAuthenticated()) {
      return
    }

    // Google script đã được preload trong layout.jsx, chỉ cần check và init
    const initGoogleWhenReady = () => {
      if (typeof window === "undefined") return

      // Nếu đã có Google API sẵn, init ngay
      if (window.google?.accounts?.id) {
        setGoogleScriptLoaded(true)
        if (!isInitializedRef.current) {
          initializeGoogleSignIn()
        }
        return
      }

      // Script đang được load từ layout.jsx, check theo interval ngắn để init sớm nhất
      let checkCount = 0
      const maxChecks = 50 // Tối đa 5 giây (50 * 100ms)
      const checkInterval = setInterval(() => {
        checkCount++
        if (window.google?.accounts?.id) {
          clearInterval(checkInterval)
          setGoogleScriptLoaded(true)
          if (!isInitializedRef.current) {
            initializeGoogleSignIn()
          }
        } else if (checkCount >= maxChecks) {
          clearInterval(checkInterval)
          // Fallback: nếu sau 5s vẫn chưa load, thử load lại
          const script = document.createElement("script")
          script.src = "https://accounts.google.com/gsi/client"
          script.async = true
          script.onload = () => {
            if (window.google?.accounts?.id) {
              setGoogleScriptLoaded(true)
              if (!isInitializedRef.current) {
                initializeGoogleSignIn()
              }
            }
          }
          document.head.appendChild(script)
        }
      }, 100) // Check mỗi 100ms để init sớm nhất có thể

      return () => clearInterval(checkInterval)
    }

    const cleanup = initGoogleWhenReady()
    return cleanup
  }, [])

  // Initialize Google Sign-In when script is loaded and button ref is ready
  useEffect(() => {
    if (googleScriptLoaded && googleButtonRef.current && !isInitializedRef.current) {
      initializeGoogleSignIn()
    }
  }, [googleScriptLoaded])

  const initializeGoogleSignIn = () => {
    if (typeof window === "undefined" || !window.google?.accounts?.id) return
    
    // Tránh init nhiều lần
    if (isInitializedRef.current) return
    
    const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID
    if (!clientId) {
      console.error("NEXT_PUBLIC_GOOGLE_CLIENT_ID is not set")
      return
    }

    try {
      // Initialize Google Identity Services with One Tap support
      window.google.accounts.id.initialize({
        client_id: clientId,
        callback: handleGoogleResponse,
        auto_select: false, // Không tự động chọn, để người dùng xác nhận
        cancel_on_tap_outside: true, // Đóng prompt khi click ra ngoài
        context: "signin", // Ngữ cảnh đăng nhập
        itp_support: true, // Hỗ trợ Intelligent Tracking Prevention
      })

      // Render Google Sign-In button if ref exists and is empty
      if (googleButtonRef.current && googleButtonRef.current.children.length === 0) {
        window.google.accounts.id.renderButton(googleButtonRef.current, {
          theme: "outline",
          size: "large",
          width: "100%",
          text: "signin_with",
          locale: "vi",
        })
      }

      // Show Google One Tap prompt (Push-based Authentication)
      window.google.accounts.id.prompt((notification) => {
        // Xử lý thông báo về trạng thái của One Tap (chỉ log, không cần xử lý)
        if (notification.isNotDisplayed()) {
          const reason = notification.getNotDisplayedReason()
          // Chỉ log trong dev mode
          if (process.env.NODE_ENV === "development") {
            console.log("One Tap không được hiển thị:", reason)
          }
        }
      })

      // Đánh dấu đã init
      isInitializedRef.current = true
    } catch (err) {
      console.error("Error initializing Google Sign-In:", err)
    }
  }

  const handleGoogleResponse = async (response) => {
    try {
      setIsLoading(true)
      setError("")

      // Decode JWT token from Google
      const payload = JSON.parse(atob(response.credential.split(".")[1]))
      
      // Log để debug
      console.log("Google payload:", payload)
      
      const googleUserData = {
        googleId: payload.sub,
        email: payload.email || "",
        name: payload.name || "",
        pictureUrl: payload.picture || "",
        accessToken: response.credential,
      }

      // Log để kiểm tra
      console.log("Google user data to send:", googleUserData)

      setGoogleUserInfo(googleUserData)

      // TODO: Gửi OTP về email
      // Tạm thời, chúng ta sẽ bỏ qua OTP và gọi API trực tiếp
      // Khi backend hỗ trợ OTP, sẽ thêm API call ở đây
      
      // Simulate OTP sending (remove when backend OTP API is ready)
      // setShowOtpDialog(true)
      // setOtpExpiryTime(Date.now() + 5 * 60 * 1000) // 5 minutes

      // Tạm thời gọi API login trực tiếp (không qua OTP)
      await handleGoogleLoginSubmit(googleUserData)
    } catch (err) {
      console.error("Google login error:", err)
      setError("Đã xảy ra lỗi khi đăng nhập bằng Google")
    } finally {
      setIsLoading(false)
    }
  }

  const handleGoogleLoginSubmit = async (googleUserData) => {
    try {
      const response = await apiClient.post("/api/GoogleAuth/login/truong-khoa", {
        googleId: googleUserData.googleId,
        email: googleUserData.email,
        name: googleUserData.name,
        pictureUrl: googleUserData.pictureUrl,
        accessToken: googleUserData.accessToken,
      })

      if (response.success && response.token) {
        authUtils.setToken(response.token)
        router.push("/tongquan")
      } else {
        setError(response.message || "Đăng nhập thất bại")
      }
    } catch (err) {
      console.error("API error:", err)
      
      // Hiển thị lỗi chi tiết hơn cho người dùng
      let errorMessage = "Không thể kết nối đến server"
      
      if (err.message) {
        errorMessage = err.message
      } else if (err instanceof TypeError) {
        errorMessage = "Lỗi kết nối mạng. Vui lòng kiểm tra server có đang chạy không."
      }
      
      setError(errorMessage)
      
      // Re-throw để có thể handle ở nơi gọi
      throw err
    }
  }

  const handleOtpSubmit = async () => {
    if (otp.length !== 6) {
      setOtpError("Mã xác thực phải có 6 chữ số")
      return
    }

    // Kiểm tra thời gian hết hạn
    if (otpExpiryTime && Date.now() > otpExpiryTime) {
      setOtpError("Mã xác thực đã hết hạn. Vui lòng đăng nhập lại")
      setShowOtpDialog(false)
      setOtp("")
      return
    }

    // Kiểm tra số lần nhập sai
    if (otpAttempts >= 5) {
      setOtpError("Bạn đã nhập sai quá 5 lần. Vui lòng đăng nhập lại")
      setShowOtpDialog(false)
      setOtp("")
      setOtpAttempts(0)
      return
    }

    setOtpLoading(true)
    setOtpError("")

    try {
      // TODO: Gọi API verify OTP
      // Tạm thời, chúng ta sẽ giả định OTP đúng và gọi login API
      // Khi backend có API OTP, sẽ thêm API call ở đây
      
      // Simulate OTP verification (remove when backend OTP API is ready)
      // if (otp !== "123456") {
      //   setOtpAttempts(prev => prev + 1)
      //   setOtpError("Mã xác thực không đúng")
      //   setOtp("")
      //   return
      // }

      // OTP đúng, tiến hành đăng nhập
      await handleGoogleLoginSubmit(googleUserInfo)
    } catch (err) {
      setOtpError(err.message || "Xác thực thất bại")
      setOtpAttempts(prev => prev + 1)
    } finally {
      setOtpLoading(false)
    }
  }

  const handleLogin = async (e) => {
    e.preventDefault()
    setError("")
    setIsLoading(true)

    try {
      const response = await apiClient.post("/api/truong-khoa/auth/login", {
        email: email.trim(),
        password: password.trim(),
      })

      if (response.accessToken) {
        authUtils.setToken(response.accessToken)
        router.push("/tongquan")
      }
    } catch (err) {
      setError(err.message || "Email hoặc mật khẩu không đúng")
    } finally {
      setIsLoading(false)
    }
  }

  const handleGoogleLogin = () => {
    if (typeof window === "undefined") return
    
    // Script đã được preload trong layout, nên thường đã sẵn sàng
    if (window.google?.accounts?.id) {
      // Trigger Google One Tap prompt manually để hiển thị ngay
      try {
        window.google.accounts.id.prompt((notification) => {
          if (notification.isNotDisplayed()) {
            const reason = notification.getNotDisplayedReason()
            // Nếu One Tap không hiển thị được, button Google đã được render sẽ xử lý
            if (reason === "browser_not_supported" || reason === "invalid_client") {
              console.warn("Google One Tap không hỗ trợ:", reason)
            }
          }
        })
      } catch (err) {
        console.error("Error triggering Google prompt:", err)
        // Không set error, để button Google vẫn có thể click được
      }
    } else {
      // Script chưa load xong, init lại
      if (!isInitializedRef.current) {
        setError("Đang tải Google Sign-In...")
        // Retry sau 200ms
        setTimeout(() => {
          if (window.google?.accounts?.id) {
            setError("")
            initializeGoogleSignIn()
            handleGoogleLogin()
          }
        }, 200)
      } else {
        setError("Google Sign-In chưa sẵn sàng. Vui lòng thử lại sau")
      }
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-white flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo và tiêu đề */}
        <div className="text-center mb-8">
          <div className="flex justify-center mb-4">
            <div className="w-16 h-16 bg-[#4A90D9] rounded-lg flex items-center justify-center">
              <span className="text-white text-2xl font-bold">S</span>
            </div>
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Sea Dragon LMS</h1>
          <p className="text-gray-600">Đăng nhập vào hệ thống</p>
        </div>

        {/* Form đăng nhập */}
        <div className="bg-white rounded-xl shadow-lg p-8">
          <form onSubmit={handleLogin} className="space-y-6">
            {/* Email */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="Nhập email của bạn"
                  className="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9] focus:border-transparent"
                  required
                />
              </div>
            </div>

            {/* Mật khẩu */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Mật khẩu</label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Nhập mật khẩu"
                  className="w-full pl-10 pr-12 py-3 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9] focus:border-transparent"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                </button>
              </div>
            </div>

            {/* Thông báo lỗi */}
            {error && (
              <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />
                <span>{error}</span>
              </div>
            )}

            {/* Ghi nhớ đăng nhập và Quên mật khẩu */}
            <div className="flex items-center justify-between">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="w-4 h-4 text-[#4A90D9] border-gray-300 rounded focus:ring-[#4A90D9]"
                />
                <span className="text-sm text-gray-700">Ghi nhớ đăng nhập</span>
              </label>
              <a href="#" className="text-sm text-[#4A90D9] hover:underline">
                Quên mật khẩu?
              </a>
            </div>

            {/* Nút đăng nhập */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-[#4A90D9] text-white py-3 rounded-lg font-medium hover:bg-[#3a7bc8] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? "Đang đăng nhập..." : "Đăng nhập"}
            </button>
          </form>

          {/* Separator */}
          <div className="relative my-6">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-200"></div>
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-white text-gray-500">Hoặc</span>
            </div>
          </div>

          {/* Nút đăng nhập bằng Google */}
          {isLoading ? (
            <div className="w-full flex items-center justify-center gap-2 py-3">
              <Loader2 className="w-5 h-5 animate-spin text-[#4A90D9]" />
              <span className="text-gray-600">Đang xử lý...</span>
            </div>
          ) : (
            <>
              {/* Google button container - sẽ được render bởi Google SDK */}
              <div ref={googleButtonRef} className="w-full flex justify-center min-h-[40px]"></div>
              
              {/* Fallback button - luôn hiển thị để trigger Google One Tap hoặc button */}
              {googleScriptLoaded && (
                <button
                  onClick={handleGoogleLogin}
                  type="button"
                  className="w-full bg-white border border-gray-200 text-gray-700 py-3 rounded-lg font-medium hover:bg-gray-50 transition-colors flex items-center justify-center gap-3"
                  title="Đăng nhập bằng Google (One Tap sẽ tự động hiển thị nếu có sẵn)"
                >
                  <svg className="w-5 h-5" viewBox="0 0 24 24">
                    <path
                      fill="#4285F4"
                      d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                    />
                    <path
                      fill="#34A853"
                      d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                    />
                    <path
                      fill="#FBBC05"
                      d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                    />
                    <path
                      fill="#EA4335"
                      d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                    />
                  </svg>
                  <span>Đăng nhập bằng Google</span>
                </button>
              )}
            </>
          )}
        </div>

        {/* Copyright */}
        <p className="text-center text-sm text-gray-500 mt-8">
          © 2025 Sea Dragon LMS. All rights reserved.
        </p>
      </div>

      {/* OTP Dialog */}
      <Dialog open={showOtpDialog} onOpenChange={setShowOtpDialog}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Nhập mã xác thực</DialogTitle>
            <DialogDescription>
              Mã xác thực đã được gửi về email <strong>{googleUserInfo?.email}</strong>
              <br />
              Vui lòng nhập mã 6 chữ số để hoàn tất đăng nhập
            </DialogDescription>
          </DialogHeader>
          
          <div className="space-y-4 py-4">
            <div className="flex justify-center">
              <InputOTP
                maxLength={6}
                value={otp}
                onChange={(value) => {
                  setOtp(value)
                  setOtpError("")
                }}
                onComplete={handleOtpSubmit}
              >
                <InputOTPGroup>
                  <InputOTPSlot index={0} />
                  <InputOTPSlot index={1} />
                  <InputOTPSlot index={2} />
                  <InputOTPSlot index={3} />
                  <InputOTPSlot index={4} />
                  <InputOTPSlot index={5} />
                </InputOTPGroup>
              </InputOTP>
            </div>

            {otpError && (
              <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />
                <span>{otpError}</span>
              </div>
            )}

            {otpExpiryTime && (
              <div className="text-center text-sm text-gray-600">
                Mã xác thực còn hiệu lực trong:{" "}
                <strong>
                  {Math.max(0, Math.floor((otpExpiryTime - Date.now()) / 1000 / 60))}:
                  {String(Math.max(0, Math.floor(((otpExpiryTime - Date.now()) / 1000) % 60))).padStart(2, "0")}
                </strong>
              </div>
            )}

            {otpAttempts > 0 && (
              <div className="text-center text-sm text-orange-600">
                Số lần nhập sai: {otpAttempts}/5
              </div>
            )}

            <div className="flex gap-3">
              <button
                onClick={() => {
                  setShowOtpDialog(false)
                  setOtp("")
                  setOtpError("")
                  setOtpAttempts(0)
                }}
                className="flex-1 px-4 py-2 border border-gray-200 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
                disabled={otpLoading}
              >
                Hủy
              </button>
              <button
                onClick={handleOtpSubmit}
                disabled={otp.length !== 6 || otpLoading}
                className="flex-1 px-4 py-2 bg-[#4A90D9] text-white rounded-lg hover:bg-[#3a7bc8] transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                {otpLoading && <Loader2 className="w-4 h-4 animate-spin" />}
                Xác thực
              </button>
            </div>

            <div className="text-center">
              <button
                onClick={() => {
                  // TODO: Gửi lại OTP
                  setOtpError("")
                  setOtpAttempts(0)
                }}
                className="text-sm text-[#4A90D9] hover:underline"
              >
                Gửi lại mã xác thực
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}

