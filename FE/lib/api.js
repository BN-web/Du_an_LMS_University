// Lấy API base URL từ env hoặc dùng default
// Lưu ý: Nếu dùng HTTPS localhost, có thể gặp lỗi certificate
// Giải pháp: Accept certificate trong trình duyệt hoặc dùng HTTP
const getApiBaseUrl = () => {
  const envUrl = process.env.NEXT_PUBLIC_API_URL
  if (envUrl) return envUrl
  
  // Default: dùng HTTPS như user đã chỉ định
  // Nếu gặp lỗi certificate, có thể đổi sang http://localhost:7133
  return "https://localhost:7133"
}

const API_BASE_URL = getApiBaseUrl()

export const apiClient = {
  async request(url, options = {}) {
    const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null

    const headers = {
      "Content-Type": "application/json",
      ...options.headers,
    }

    if (token) {
      headers.Authorization = `Bearer ${token}`
    }

    const config = {
      ...options,
      headers,
    }

    try {
      const fullUrl = `${API_BASE_URL}${url}`
      
      // Thêm mode và credentials cho CORS
      const fetchConfig = {
        ...config,
        mode: 'cors',
        credentials: 'omit', // Không gửi cookies để tránh CORS issue với AllowAnyOrigin
      }
      
      const response = await fetch(fullUrl, fetchConfig)

      // Một số lỗi (401, 500) có thể trả body rỗng hoặc không phải JSON
      const text = await response.text()
      let data = null
      if (text) {
        try {
          data = JSON.parse(text)
        } catch {
          // Không phải JSON, giữ nguyên data = null
        }
      }

      if (!response.ok) {
        // Lấy thông báo lỗi từ response - kiểm tra nhiều format
        let message = 
          data?.message || 
          data?.Message || 
          data?.error || 
          data?.Error ||
          (typeof data === 'string' ? data : null) ||
          null
        
        // Nếu không có message, tạo message mặc định dựa trên status code
        if (!message) {
          switch (response.status) {
            case 401:
              message = "Bạn chưa đăng nhập hoặc token hết hạn"
              break
            case 403:
              message = "Bạn không có quyền truy cập"
              break
            case 404:
              message = `API endpoint không tìm thấy: ${url}`
              break
            case 500:
              // Kiểm tra xem có lỗi SQL không
              if (text && (text.includes('syntax') || text.includes('SQL') || text.includes('WITH'))) {
                message = "Lỗi cơ sở dữ liệu. Vui lòng liên hệ quản trị viên."
              } else {
                message = "Lỗi máy chủ (500). Vui lòng thử lại sau."
              }
              break
            default:
              message = `Có lỗi xảy ra (HTTP ${response.status})`
          }
        }
        
        // Thêm thông tin chi tiết nếu có
        const error = new Error(message)
        error.status = response.status
        error.data = data
        error.url = url
        error.responseText = text // Lưu raw response để debug
        throw error
      }

      return data
    } catch (error) {
      // Xử lý các loại lỗi network khác nhau
      if (error instanceof TypeError) {
        // Network error, CORS, hoặc SSL certificate error
        let errorMessage = error.message
        
        if (error.message.includes("Failed to fetch") || 
            error.message.includes("NetworkError") ||
            error.message.includes("Network request failed")) {
          
          // Kiểm tra nếu là lỗi SSL/Certificate
          if (API_BASE_URL.startsWith("https://localhost")) {
            errorMessage = `Không thể kết nối đến server tại ${API_BASE_URL}

Có thể do:
1. SSL Certificate - Hãy mở ${API_BASE_URL} trong trình duyệt và chấp nhận certificate
2. Server chưa chạy - Kiểm tra server có đang chạy không
3. CORS chưa được cấu hình - Đã thêm CORS vào backend chưa?

Giải pháp: Mở ${API_BASE_URL}/swagger trong trình duyệt và chấp nhận certificate trước.`
          } else {
            errorMessage = `Không thể kết nối đến server tại ${API_BASE_URL}
            
Vui lòng kiểm tra:
- Server có đang chạy không?
- CORS đã được cấu hình đúng chưa?`
          }
        }
        
        throw new Error(errorMessage)
      }
      
      if (error instanceof Error) {
        throw error
      }
      
      throw new Error("Không thể kết nối đến server")
    }
  },

  get(url, options = {}) {
    return this.request(url, { ...options, method: "GET" })
  },

  post(url, data, options = {}) {
    return this.request(url, {
      ...options,
      method: "POST",
      body: JSON.stringify(data),
    })
  },

  put(url, data, options = {}) {
    return this.request(url, {
      ...options,
      method: "PUT",
      body: JSON.stringify(data),
    })
  },

  delete(url, options = {}) {
    return this.request(url, { ...options, method: "DELETE" })
  },

  // Download file (CSV, PDF, Excel, etc.)
  async downloadFile(url, filename, options = {}) {
    const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null

    const headers = {
      ...options.headers,
    }

    if (token) {
      headers.Authorization = `Bearer ${token}`
    }

    try {
      const fullUrl = `${API_BASE_URL}${url}`
      
      const response = await fetch(fullUrl, {
        ...options,
        method: options.method || "GET",
        headers,
        mode: 'cors',
        credentials: 'omit',
      })

      if (!response.ok) {
        const text = await response.text()
        let errorMessage = `Lỗi khi tải file (HTTP ${response.status})`
        
        try {
          const errorData = JSON.parse(text)
          errorMessage = errorData.message || errorData.Message || errorMessage
        } catch {
          // Không phải JSON, giữ nguyên errorMessage
        }
        
        throw new Error(errorMessage)
      }

      // Lấy blob từ response
      const blob = await response.blob()
      
      // Tạo URL tạm thời và download
      const blobUrl = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = blobUrl
      link.download = filename || 'download'
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(blobUrl)
    } catch (error) {
      console.error("Lỗi khi download file:", error)
      throw error
    }
  },
}

