import { lichDayData } from "./data"

/**
 * Kiểm tra xem có trùng lịch không
 * @param {string} giangVienId - ID giảng viên
 * @param {string} ngay - Ngày dạy (format: YYYY-MM-DD)
 * @param {string} gioBatDau - Giờ bắt đầu (format: HH:mm)
 * @param {string} gioKetThuc - Giờ kết thúc (format: HH:mm)
 * @param {string} excludeLichId - ID lịch cần loại trừ (khi cập nhật)
 * @returns {Object} { isConflict: boolean, conflictingSchedule: Object|null }
 */
export function checkScheduleConflict(giangVienId, ngay, gioBatDau, gioKetThuc, excludeLichId = null) {
  // Chuyển đổi giờ sang phút để so sánh
  const timeToMinutes = (time) => {
    const [hours, minutes] = time.split(":").map(Number)
    return hours * 60 + minutes
  }

  const startMinutes = timeToMinutes(gioBatDau)
  const endMinutes = timeToMinutes(gioKetThuc)

  // Tìm lịch trùng
  const conflictingSchedule = lichDayData.find((lich) => {
    // Loại trừ lịch hiện tại nếu đang cập nhật
    if (excludeLichId && lich.id === excludeLichId) {
      return false
    }

    // Kiểm tra cùng giảng viên và cùng ngày
    if (lich.giangVienId !== giangVienId || lich.ngay !== ngay) {
      return false
    }

    // Kiểm tra trùng thời gian
    const lichStartMinutes = timeToMinutes(lich.gioBatDau)
    const lichEndMinutes = timeToMinutes(lich.gioKetThuc)

    // Trùng nếu có khoảng thời gian chồng lấn
    return (
      (startMinutes >= lichStartMinutes && startMinutes < lichEndMinutes) ||
      (endMinutes > lichStartMinutes && endMinutes <= lichEndMinutes) ||
      (startMinutes <= lichStartMinutes && endMinutes >= lichEndMinutes)
    )
  })

  return {
    isConflict: conflictingSchedule !== undefined,
    conflictingSchedule: conflictingSchedule || null,
  }
}

/**
 * Lưu log override vào nhật ký
 * @param {Object} logData - Dữ liệu log
 * @param {string} logData.nguoiThucHien - Người thực hiện
 * @param {string} logData.lyDo - Lý do override
 * @param {string} logData.monHoc - Môn học
 * @param {string} logData.lichCu - Thông tin lịch cũ (nếu có)
 * @param {string} logData.lichMoi - Thông tin lịch mới
 */
export function saveOverrideLog(logData) {
  // Trong thực tế, đây sẽ gọi API để lưu vào database
  // Hiện tại chỉ log ra console
  const logEntry = {
    thoiGian: new Date().toLocaleString("vi-VN"),
    ...logData,
  }

  console.log("Override Log:", logEntry)

  // Lưu vào localStorage để demo (trong thực tế sẽ lưu vào database)
  const existingLogs = JSON.parse(localStorage.getItem("overrideLogs") || "[]")
  existingLogs.push(logEntry)
  localStorage.setItem("overrideLogs", JSON.stringify(existingLogs))

  return logEntry
}

/**
 * Lấy danh sách log override
 * @returns {Array} Danh sách log
 */
export function getOverrideLogs() {
  // Trong thực tế, đây sẽ gọi API để lấy từ database
  const logs = JSON.parse(localStorage.getItem("overrideLogs") || "[]")
  return logs
}

