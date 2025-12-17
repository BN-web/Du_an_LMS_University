"use client"

import { useState } from "react"
import { AlertTriangle, X } from "lucide-react"

export default function ScheduleConflictDialog({
  isOpen,
  onClose,
  onConfirm,
  conflictingSchedule,
  newSchedule,
}) {
  const [reason, setReason] = useState("")
  const [error, setError] = useState("")

  if (!isOpen) return null

  const handleConfirm = () => {
    if (!reason.trim()) {
      setError("Vui lòng nhập lý do override")
      return
    }

    onConfirm(reason)
    setReason("")
    setError("")
  }

  const handleCancel = () => {
    setReason("")
    setError("")
    onClose()
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl p-6 max-w-2xl w-full mx-4 shadow-xl">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-yellow-100 rounded-full flex items-center justify-center">
              <AlertTriangle className="w-6 h-6 text-yellow-600" />
            </div>
            <h2 className="text-xl font-bold text-gray-800">Cảnh báo trùng lịch</h2>
          </div>
          <button
            onClick={handleCancel}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="mb-6">
          <p className="text-gray-700 mb-4">
            Giảng viên đã có lịch dạy trong khoảng thời gian này:
          </p>

          {conflictingSchedule && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-4">
              <p className="text-sm font-medium text-red-800 mb-2">Lịch hiện tại:</p>
              <div className="text-sm text-red-700 space-y-1">
                <p>
                  <span className="font-medium">Môn:</span> {conflictingSchedule.monHoc}
                </p>
                <p>
                  <span className="font-medium">Lớp:</span> {conflictingSchedule.lop}
                </p>
                <p>
                  <span className="font-medium">Thời gian:</span> {conflictingSchedule.ngayDay},{" "}
                  {conflictingSchedule.thoiGian}
                </p>
                <p>
                  <span className="font-medium">Phòng:</span> {conflictingSchedule.phongHoc}
                </p>
              </div>
            </div>
          )}

          {newSchedule && (
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
              <p className="text-sm font-medium text-blue-800 mb-2">Lịch mới:</p>
              <div className="text-sm text-blue-700 space-y-1">
                <p>
                  <span className="font-medium">Môn:</span> {newSchedule.monHoc}
                </p>
                <p>
                  <span className="font-medium">Lớp:</span> {newSchedule.lop}
                </p>
                <p>
                  <span className="font-medium">Thời gian:</span> {newSchedule.ngayDay},{" "}
                  {newSchedule.thoiGian}
                </p>
                <p>
                  <span className="font-medium">Phòng:</span> {newSchedule.phongHoc}
                </p>
              </div>
            </div>
          )}
        </div>

        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Lý do override <span className="text-red-500">*</span>
          </label>
          <textarea
            value={reason}
            onChange={(e) => {
              setReason(e.target.value)
              setError("")
            }}
            placeholder="Nhập lý do override..."
            rows={4}
            className={`w-full px-4 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 ${
              error ? "border-red-300 focus:ring-red-500" : "border-gray-200 focus:ring-[#4A90D9]"
            }`}
          />
          {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
        </div>

        <div className="flex items-center justify-end gap-3">
          <button
            onClick={handleCancel}
            className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
          >
            Hủy
          </button>
          <button
            onClick={handleConfirm}
            className="px-4 py-2 bg-[#4A90D9] text-white rounded-lg text-sm font-medium hover:bg-[#3a7bc8] transition-colors"
          >
            Xác nhận ghi đè
          </button>
        </div>
      </div>
    </div>
  )
}

