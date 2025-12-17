"use client"

import { useEffect, useState } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { ChevronLeft } from "lucide-react"
import { apiClient } from "@/lib/api"

export default function PhanCongLopPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const lopHocId = searchParams.get("lopHocId")

  const [classInfo, setClassInfo] = useState(null)
  const [lecturers, setLecturers] = useState([])
  const [giangVienId, setGiangVienId] = useState("")
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState(null)

  useEffect(() => {
    if (!lopHocId) {
      setIsLoading(false)
      return
    }

    const fetchData = async () => {
      try {
        setIsLoading(true)
        setError(null)

        const [lop, gvOptions] = await Promise.all([
          apiClient.get(`/api/truong-khoa/lop-hoc/${lopHocId}`),
          apiClient.get("/api/truong-khoa/options/giang-vien"),
        ])

        setClassInfo(lop)
        setLecturers(Array.isArray(gvOptions) ? gvOptions : [])
      } catch (err) {
        setError(err instanceof Error ? err.message : "Không thể tải dữ liệu phân công")
      } finally {
        setIsLoading(false)
      }
    }

    fetchData()
  }, [lopHocId])

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!lopHocId || !giangVienId) return

    try {
      setIsSubmitting(true)
      setError(null)

      await apiClient.post(`/api/truong-khoa/lop-hoc/${lopHocId}/assign-giang-vien`, {
        giangVienId: Number(giangVienId),
        overrideConflicts: false,
        overrideReason: null,
        actorNguoiDungId: null,
      })

      alert("Phân công giảng viên thành công")
      router.push("/lophoc")
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không thể phân công giảng viên")
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="flex-1 p-6">
      <div className="flex items-center gap-4 mb-6">
        <button onClick={() => router.push("/lophoc")} className="text-gray-500 hover:text-gray-700">
          <ChevronLeft className="w-6 h-6" />
        </button>
        <h1 className="text-[#4A90D9] text-2xl font-bold">Phân công giảng viên cho lớp</h1>
      </div>

      <div className="bg-white rounded-xl p-6 shadow-sm max-w-2xl">
        {!lopHocId ? (
          <p className="text-sm text-red-500">Thiếu thông tin lớp học. Vui lòng quay lại trang Lớp học.</p>
        ) : isLoading ? (
          <p className="text-sm text-gray-500">Đang tải dữ liệu...</p>
        ) : error ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : (
          <>
            <div className="mb-6">
              <h2 className="text-lg font-semibold text-gray-800 mb-1">
                {classInfo?.tenLop || classInfo?.mon || "Lớp học"}
              </h2>
              <p className="text-sm text-gray-600">
                Mã lớp: <span className="font-medium">{classInfo?.maLop}</span>
              </p>
              <p className="text-sm text-gray-600">
                Giảng viên hiện tại:{" "}
                <span className="font-medium">{classInfo?.giangVien || "Chưa phân công"}</span>
              </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Chọn giảng viên mới <span className="text-red-500">*</span>
                </label>
                <select
                  value={giangVienId}
                  onChange={(e) => setGiangVienId(e.target.value)}
                  className="w-full px-4 py-2.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#4A90D9]"
                  required
                >
                  <option value="">-- Chọn giảng viên --</option>
                  {lecturers.map((gv) => (
                    <option key={gv.id} value={gv.id}>
                      {gv.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex items-center justify-end gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => router.push("/lophoc")}
                  className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting || !giangVienId}
                  className="px-4 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isSubmitting ? "Đang phân công..." : "Lưu phân công"}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </main>
  )
}

