"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Eye, EyeOff, Lock, Mail, X } from "lucide-react"

export default function DangNhap() {
  const router = useRouter()
  const [formData, setFormData] = useState({
    email: "",
    password: "",
  })
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState("")
  const [loading, setLoading] = useState(false)
  const [showGoogleAccounts, setShowGoogleAccounts] = useState(false)
  const [googleLoading, setGoogleLoading] = useState(false)
  const [showAddAccount, setShowAddAccount] = useState(false)
  const [newAccountEmail, setNewAccountEmail] = useState("")
  const [newAccountPassword, setNewAccountPassword] = useState("")
  const [showNewAccountPassword, setShowNewAccountPassword] = useState(false)

  // Google accounts - can be added dynamically
  const [googleAccounts, setGoogleAccounts] = useState([
    {
      id: 1,
      email: "nguyenvana@gmail.com",
      name: "Nguyễn Văn A",
      avatar: "https://ui-avatars.com/api/?name=Nguyen+Van+A&background=4285F4&color=fff",
    },
    {
      id: 2,
      email: "tranthib@gmail.com",
      name: "Trần Thị B",
      avatar: "https://ui-avatars.com/api/?name=Tran+Thi+B&background=34A853&color=fff",
    },
    {
      id: 3,
      email: "levanc@gmail.com",
      name: "Lê Văn C",
      avatar: "https://ui-avatars.com/api/?name=Le+Van+C&background=EA4335&color=fff",
    },
  ])

  // Password validation function
  const validatePassword = (password) => {
    if (password.length < 7) {
      return "Mật khẩu phải có ít nhất 7 ký tự"
    }
    if (!/[a-z]/.test(password)) {
      return "Mật khẩu phải có ít nhất 1 chữ cái thường"
    }
    if (!/[A-Z]/.test(password)) {
      return "Mật khẩu phải có ít nhất 1 chữ cái in hoa"
    }
    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
      return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt"
    }
    return null
  }

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    })
    setError("")
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError("")
    
    // Validation
    if (!formData.email || !formData.password) {
      setError("Vui lòng nhập đầy đủ thông tin")
      return
    }

    // Validate password
    const passwordError = validatePassword(formData.password)
    if (passwordError) {
      setError(passwordError)
      return
    }

    setLoading(true)

    // Simulate API call
    setTimeout(() => {
      // Mock authentication - in real app, this would be an API call
      if (formData.email && formData.password) {
        // Store auth token (in real app, use secure storage)
        localStorage.setItem("isAuthenticated", "true")
        localStorage.setItem("userEmail", formData.email)
        
        // Redirect to dashboard
        router.push("/giangvien/tongquan")
      } else {
        setError("Email hoặc mật khẩu không đúng")
        setLoading(false)
      }
    }, 1000)
  }

  const handleGoogleLogin = () => {
    // Show Google account selection modal
    setShowGoogleAccounts(true)
  }

  const handleSelectGoogleAccount = (account) => {
    setShowGoogleAccounts(false)
    setGoogleLoading(true)

    // Simulate Google OAuth authentication
    setTimeout(() => {
      localStorage.setItem("isAuthenticated", "true")
      localStorage.setItem("userEmail", account.email)
      localStorage.setItem("userName", account.name)
      localStorage.setItem("userAvatar", account.avatar)
      localStorage.setItem("loginMethod", "google")
      
      setGoogleLoading(false)
      router.push("/giangvien/tongquan")
    }, 1500)
  }

  const handleCloseGoogleModal = () => {
    setShowGoogleAccounts(false)
    setShowAddAccount(false)
    setNewAccountEmail("")
    setNewAccountPassword("")
  }

  const handleAddAccount = () => {
    setShowAddAccount(true)
  }

  const handleAddAccountSubmit = (e) => {
    e.preventDefault()
    
    // Validate email is Gmail
    if (!newAccountEmail.includes("@gmail.com")) {
      setError("Vui lòng nhập địa chỉ Gmail hợp lệ")
      return
    }

    if (!newAccountPassword) {
      setError("Vui lòng nhập mật khẩu")
      return
    }

    // Validate password
    const passwordError = validatePassword(newAccountPassword)
    if (passwordError) {
      setError(passwordError)
      return
    }

    setError("")
    setGoogleLoading(true)

    // Simulate Google OAuth authentication
    setTimeout(() => {
      // Extract name from email
      const nameFromEmail = newAccountEmail.split("@")[0].replace(/\./g, " ")
      const nameParts = nameFromEmail.split(" ").map(part => part.charAt(0).toUpperCase() + part.slice(1))
      const displayName = nameParts.join(" ")

      // Create new account
      const newAccount = {
        id: googleAccounts.length + 1,
        email: newAccountEmail,
        name: displayName,
        avatar: `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName)}&background=4285F4&color=fff`,
      }

      // Add to accounts list
      setGoogleAccounts([...googleAccounts, newAccount])
      
      // Auto login with new account
      localStorage.setItem("isAuthenticated", "true")
      localStorage.setItem("userEmail", newAccount.email)
      localStorage.setItem("userName", newAccount.name)
      localStorage.setItem("userAvatar", newAccount.avatar)
      localStorage.setItem("loginMethod", "google")
      
      setGoogleLoading(false)
      setShowAddAccount(false)
      setShowGoogleAccounts(false)
      router.push("/giangvien/tongquan")
    }, 1500)
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo and Title */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-blue-600 to-blue-800 rounded-2xl shadow-lg mb-4">
            <span className="text-white font-bold text-2xl">S</span>
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Sea Dragon LMS</h1>
          <p className="text-gray-600">Đăng nhập vào hệ thống</p>
        </div>

        {/* Login Form */}
        <div className="bg-white rounded-2xl shadow-xl p-8 border border-gray-100">
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Email Input */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-2">
                Email
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Mail className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  id="email"
                  name="email"
                  type="email"
                  required
                  value={formData.email}
                  onChange={handleChange}
                  className="block w-full pl-10 pr-3 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900 placeholder-gray-400"
                  placeholder="Nhập email của bạn"
                />
              </div>
            </div>

            {/* Password Input */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                Mật khẩu
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Lock className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  id="password"
                  name="password"
                  type={showPassword ? "text" : "password"}
                  required
                  value={formData.password}
                  onChange={handleChange}
                  className="block w-full pl-10 pr-10 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900 placeholder-gray-400"
                  placeholder="Nhập mật khẩu"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center"
                >
                  {showPassword ? (
                    <EyeOff className="h-5 w-5 text-gray-400 hover:text-gray-600" />
                  ) : (
                    <Eye className="h-5 w-5 text-gray-400 hover:text-gray-600" />
                  )}
                </button>
              </div>
            </div>

            {/* Remember Me & Forgot Password */}
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <input
                  id="remember"
                  name="remember"
                  type="checkbox"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label htmlFor="remember" className="ml-2 block text-sm text-gray-700">
                  Ghi nhớ đăng nhập
                </label>
              </div>
              <a href="#" className="text-sm text-blue-600 hover:text-blue-700 font-medium">
                Quên mật khẩu?
              </a>
            </div>

            {/* Error Message */}
            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
                {error}
              </div>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              disabled={loading}
              className="w-full bg-gradient-to-r from-blue-600 to-blue-700 text-white py-3 px-4 rounded-lg font-semibold hover:from-blue-700 hover:to-blue-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg"
            >
              {loading ? "Đang đăng nhập..." : "Đăng nhập"}
            </button>
          </form>

          {/* Divider */}
          <div className="mt-6 mb-6">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300"></div>
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-white text-gray-500">Hoặc</span>
              </div>
            </div>
          </div>

          {/* Google Login Button */}
          <button
            type="button"
            onClick={handleGoogleLogin}
            disabled={loading || googleLoading}
            className="w-full flex items-center justify-center gap-3 bg-white border-2 border-gray-300 text-gray-700 py-3 px-4 rounded-lg font-semibold hover:bg-gray-50 hover:border-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-400 focus:ring-offset-2 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed shadow-sm"
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

        </div>

        {/* Footer */}
        <div className="mt-6 text-center text-sm text-gray-500">
          <p>© 2025 Sea Dragon LMS. All rights reserved.</p>
        </div>
      </div>

      {/* Google Account Selection Modal */}
      {showGoogleAccounts && (
        <>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40"
            onClick={handleCloseGoogleModal}
          />
          <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6 animate-in fade-in zoom-in">
              {/* Header */}
              <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                  <svg className="w-8 h-8" viewBox="0 0 24 24">
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
                  <div>
                    <h2 className="text-xl font-bold text-gray-900">Chọn tài khoản</h2>
                    <p className="text-sm text-gray-600">Chọn tài khoản Google để tiếp tục</p>
                  </div>
                </div>
                <button
                  onClick={handleCloseGoogleModal}
                  className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                >
                  <X className="w-5 h-5 text-gray-600" />
                </button>
              </div>

              {/* Account List */}
              <div className="space-y-2 mb-4">
                {googleAccounts.map((account) => (
                  <button
                    key={account.id}
                    onClick={() => handleSelectGoogleAccount(account)}
                    className="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-lg hover:bg-gray-50 hover:border-blue-300 transition-all cursor-pointer text-left"
                  >
                    <img
                      src={account.avatar}
                      alt={account.name}
                      className="w-10 h-10 rounded-full"
                    />
                    <div className="flex-1">
                      <p className="font-medium text-gray-900">{account.name}</p>
                      <p className="text-sm text-gray-600">{account.email}</p>
                    </div>
                    <svg
                      className="w-5 h-5 text-gray-400"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M9 5l7 7-7 7"
                      />
                    </svg>
                  </button>
                ))}
              </div>

              {/* Add Account Option */}
              <div className="border-t border-gray-200 pt-4">
                <button
                  onClick={handleAddAccount}
                  className="w-full flex items-center justify-center gap-2 p-3 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors font-medium"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 4v16m8-8H4"
                    />
                  </svg>
                  Thêm tài khoản khác
                </button>
              </div>
            </div>
          </div>
        </>
      )}

      {/* Add Google Account Modal */}
      {showAddAccount && (
        <>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40"
            onClick={handleCloseGoogleModal}
          />
          <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6 animate-in fade-in zoom-in"
              onClick={(e) => e.stopPropagation()}
            >
              {/* Header */}
              <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                  <svg className="w-8 h-8" viewBox="0 0 24 24">
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
                  <div>
                    <h2 className="text-xl font-bold text-gray-900">Đăng nhập bằng Google</h2>
                    <p className="text-sm text-gray-600">Nhập thông tin tài khoản Gmail của bạn</p>
                  </div>
                </div>
                <button
                  onClick={handleCloseGoogleModal}
                  className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                >
                  <X className="w-5 h-5 text-gray-600" />
                </button>
              </div>

              {/* Form */}
              <form onSubmit={handleAddAccountSubmit} className="space-y-4">
                {/* Email Input */}
                <div>
                  <label htmlFor="newAccountEmail" className="block text-sm font-medium text-gray-700 mb-2">
                    Email Gmail
                  </label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <Mail className="h-5 w-5 text-gray-400" />
                    </div>
                    <input
                      id="newAccountEmail"
                      type="email"
                      required
                      value={newAccountEmail}
                      onChange={(e) => {
                        setNewAccountEmail(e.target.value)
                        setError("")
                      }}
                      className="block w-full pl-10 pr-3 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900 placeholder-gray-400"
                      placeholder="yourname@gmail.com"
                    />
                  </div>
                </div>

                {/* Password Input */}
                <div>
                  <label htmlFor="newAccountPassword" className="block text-sm font-medium text-gray-700 mb-2">
                    Mật khẩu Google
                  </label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <Lock className="h-5 w-5 text-gray-400" />
                    </div>
                    <input
                      id="newAccountPassword"
                      type={showNewAccountPassword ? "text" : "password"}
                      required
                      value={newAccountPassword}
                      onChange={(e) => {
                        setNewAccountPassword(e.target.value)
                        setError("")
                      }}
                      className="block w-full pl-10 pr-10 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-gray-900 placeholder-gray-400"
                      placeholder="Nhập mật khẩu Google"
                    />
                    <button
                      type="button"
                      onClick={() => setShowNewAccountPassword(!showNewAccountPassword)}
                      className="absolute inset-y-0 right-0 pr-3 flex items-center"
                    >
                      {showNewAccountPassword ? (
                        <EyeOff className="h-5 w-5 text-gray-400 hover:text-gray-600" />
                      ) : (
                        <Eye className="h-5 w-5 text-gray-400 hover:text-gray-600" />
                      )}
                    </button>
                  </div>
                </div>

                {/* Error Message */}
                {error && (
                  <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
                    {error}
                  </div>
                )}

                {/* Submit Button */}
                <div className="flex gap-3 pt-2">
                  <button
                    type="button"
                    onClick={handleCloseGoogleModal}
                    className="flex-1 px-4 py-3 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-50 transition-colors"
                  >
                    Hủy
                  </button>
                  <button
                    type="submit"
                    disabled={googleLoading}
                    className="flex-1 px-4 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {googleLoading ? "Đang xử lý..." : "Đăng nhập"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </>
      )}

      {/* Google Loading Overlay */}
      {googleLoading && !showAddAccount && (
        <>
          <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40" />
          <div className="fixed inset-0 flex items-center justify-center z-50">
            <div className="bg-white rounded-2xl shadow-2xl p-8 text-center">
              <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-blue-600 border-t-transparent mb-4"></div>
              <p className="text-gray-700 font-medium">Đang đăng nhập bằng Google...</p>
            </div>
          </div>
        </>
      )}
    </div>
  )
}

