const TOKEN_KEY = "accessToken"

export const authUtils = {
  setToken(token) {
    if (typeof window !== "undefined") {
      localStorage.setItem(TOKEN_KEY, token)
    }
  },

  getToken() {
    if (typeof window !== "undefined") {
      return localStorage.getItem(TOKEN_KEY)
    }
    return null
  },

  removeToken() {
    if (typeof window !== "undefined") {
      localStorage.removeItem(TOKEN_KEY)
    }
  },

  isAuthenticated() {
    return !!this.getToken()
  },

  getUserId() {
    const token = this.getToken()
    if (!token) return null

    try {
      const base64Url = token.split(".")[1]
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/")
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split("")
          .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
          .join("")
      )
      const decoded = JSON.parse(jsonPayload)
      return decoded.sub || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || null
    } catch {
      return null
    }
  },

  getUserInfo() {
    const token = this.getToken()
    if (!token) return null

    try {
      const base64Url = token.split(".")[1]
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/")
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split("")
          .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
          .join("")
      )
      const decoded = JSON.parse(jsonPayload)
      return {
        userId: decoded.sub || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || null,
        fullName: decoded.FullName || decoded["FullName"] || null,
        avatar: decoded.Avatar || decoded["Avatar"] || null,
        email: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || decoded.email || null,
      }
    } catch {
      return null
    }
  },
}

