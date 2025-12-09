"use client"

import { useState, useRef, useEffect } from "react"
import { ChevronLeft, ChevronRight } from "lucide-react"

export default function LichDay() {
  const [activeTab, setActiveTab] = useState("schedule")
  const [currentWeekStart, setCurrentWeekStart] = useState(new Date(2025, 0, 14)) // January 14, 2025
  const scheduleScrollRef = useRef(null)
  const [hoveredItem, setHoveredItem] = useState(null)
  const [tooltipPosition, setTooltipPosition] = useState({ x: 0, y: 0 })

  const days = ["Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN"]
  const hours = Array.from({ length: 17 }, (_, i) => 7 + i) // 7:00 to 23:00

  // Helper function to parse time string to decimal hours
  const parseTimeToHours = (timeStr) => {
    const [time] = timeStr.split(" - ")
    const [hours, minutes] = time.split(":").map(Number)
    return hours + minutes / 60
  }

  // Helper function to calculate duration in hours
  const calculateDuration = (timeStr) => {
    const [startStr, endStr] = timeStr.split(" - ")
    const [startHours, startMinutes] = startStr.split(":").map(Number)
    const [endHours, endMinutes] = endStr.split(":").map(Number)
    const start = startHours + startMinutes / 60
    const end = endHours + endMinutes / 60
    return end - start
  }

  const scheduleItems = [
    {
      day: "Thứ 2",
      time: "08:00 - 10:00",
      title: "Lập trình Web",
      code: "CMTT941",
      instructor: "GV: Trần Thị B",
      room: "Phòng 301",
      campus: "Cơ sở 1",
      building: "Toà A",
      type: "Bình thường",
      backgroundColor: "#DBFCE7",
      badgeColor: "#16a34a",
      badgeText: "Bình thường",
    },
    {
      day: "Thứ 3",
      time: "08:50 - 11:00",
      title: "Toán rời rạc",
      code: "TOAI1012",
      instructor: "GV: Phạm Thị D",
      room: "Phòng 201",
      campus: "Cơ sở 1",
      building: "Toà B",
      type: "Online",
      backgroundColor: "#E0E7FF",
      badgeColor: "#6366f1",
      badgeText: "Online",
    },
    {
      day: "Thứ 4",
      time: "10:00 - 12:00",
      title: "Cơ sở dữ liệu",
      code: "CNTT3012",
      instructor: "GV: Nguyễn Văn C",
      room: "Phòng 202",
      campus: "Cơ sở 1",
      building: "Toà C",
      type: "Đã hủy",
      backgroundColor: "#FFE2E2",
      badgeColor: "#ef4444",
      badgeText: "Đã hủy",
    },
    {
      day: "Thứ 5",
      time: "09:00 - 11:00",
      title: "Hệ điều hành",
      code: "CMTT2023",
      instructor: "GV: Hoàng Văn E",
      room: "Phòng 305",
      campus: "Cơ sở 1",
      building: "Toà D",
      type: "Đổi giờ",
      backgroundColor: "#FFEDD4",
      badgeColor: "#f97316",
      badgeText: "Đổi giờ",
    },
    {
      day: "Thứ 7",
      time: "08:00 - 10:00",
      title: "Trí tuệ nhân tạo",
      code: "CMTT4051",
      instructor: "GV: Bùi Thị H",
      room: "Phòng 401",
      campus: "Cơ sở 2",
      building: "Toà D",
      type: "Đổi phòng",
      backgroundColor: "#F3E8FF",
      badgeColor: "#a855f7",
      badgeText: "Đổi phòng",
    },
  ]

  // Legend items for display only (not clickable)
  const legendItems = [
    { type: "Bình thường", backgroundColor: "#DBFCE7", badgeColor: "#16a34a" },
    { type: "Đã hủy", backgroundColor: "#FFE2E2", badgeColor: "#ef4444" },
    { type: "Đổi giờ", backgroundColor: "#FFEDD4", badgeColor: "#f97316" },
    { type: "Đổi phòng", backgroundColor: "#F3E8FF", badgeColor: "#a855f7" },
    { type: "Online", backgroundColor: "#E0E7FF", badgeColor: "#6366f1" },
  ]

  // Format date range
  const formatDateRange = (startDate) => {
    const endDate = new Date(startDate)
    endDate.setDate(endDate.getDate() + 6)

    const startDay = startDate.getDate()
    const startMonth = startDate.getMonth() + 1
    const endDay = endDate.getDate()
    const endMonth = endDate.getMonth() + 1
    const year = startDate.getFullYear()

    return `${startDay}/${startMonth} - ${endDay}/${endMonth}/${year}`
  }

  const handlePrevWeek = () => {
    const newDate = new Date(currentWeekStart)
    newDate.setDate(newDate.getDate() - 7)
    setCurrentWeekStart(newDate)
  }

  const handleNextWeek = () => {
    const newDate = new Date(currentWeekStart)
    newDate.setDate(newDate.getDate() + 7)
    setCurrentWeekStart(newDate)
  }

  const handleThisWeek = () => {
    const today = new Date()
    const dayOfWeek = today.getDay()
    const diff = today.getDate() - dayOfWeek + (dayOfWeek === 0 ? -6 : 1) // Adjust to Monday
    const newDate = new Date(today.setDate(diff))
    newDate.setHours(0, 0, 0, 0)
    setCurrentWeekStart(newDate)
  }

  // Check if selected week is within the current 7-day period (current week)
  const isCurrentWeek = () => {
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    
    const dayOfWeek = today.getDay()
    const diff = today.getDate() - dayOfWeek + (dayOfWeek === 0 ? -6 : 1) // Adjust to Monday
    const currentWeekStartDate = new Date(today)
    currentWeekStartDate.setDate(diff)
    currentWeekStartDate.setHours(0, 0, 0, 0)

    const currentWeekEndDate = new Date(currentWeekStartDate)
    currentWeekEndDate.setDate(currentWeekEndDate.getDate() + 6)
    currentWeekEndDate.setHours(23, 59, 59, 999)

    const selectedWeekStartDate = new Date(currentWeekStart)
    selectedWeekStartDate.setHours(0, 0, 0, 0)

    const selectedWeekEndDate = new Date(selectedWeekStartDate)
    selectedWeekEndDate.setDate(selectedWeekEndDate.getDate() + 6)
    selectedWeekEndDate.setHours(23, 59, 59, 999)

    // Check if selected week overlaps with current week
    return (
      (selectedWeekStartDate >= currentWeekStartDate && selectedWeekStartDate <= currentWeekEndDate) ||
      (selectedWeekEndDate >= currentWeekStartDate && selectedWeekEndDate <= currentWeekEndDate) ||
      (selectedWeekStartDate <= currentWeekStartDate && selectedWeekEndDate >= currentWeekEndDate)
    )
  }

  // Return all schedule items (no filtering)
  const getFilteredScheduleItems = () => {
    return scheduleItems
  }

  // Handle horizontal scroll with mouse wheel
  useEffect(() => {
    const scheduleElement = scheduleScrollRef.current
    if (!scheduleElement) return

    const handleWheel = (e) => {
      // Always convert vertical scroll to horizontal when hovering over schedule area
      if (Math.abs(e.deltaY) > 0) {
        e.preventDefault()
        e.stopPropagation()
        scheduleElement.scrollLeft += e.deltaY
      }
    }

    scheduleElement.addEventListener('wheel', handleWheel, { passive: false })

    return () => {
      scheduleElement.removeEventListener('wheel', handleWheel)
    }
  }, [activeTab])

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-blue-100 p-6">
      <div className="max-w-full bg-white rounded-xl shadow-lg p-6">
        {/* Header */}
        <h1 className="text-2xl font-bold text-blue-900 mb-6">Lịch dạy</h1>

        {/* Tabs */}
        <div className="flex gap-2 mb-6 border-b">
          <button
            onClick={() => setActiveTab("schedule")}
            className={`px-4 py-2 font-medium transition-colors ${
              activeTab === "schedule"
                ? "text-blue-600 border-b-2 border-blue-600"
                : "text-gray-600 hover:text-gray-800"
            }`}
          >
            Thời khóa biểu
          </button>
          <button
            onClick={() => setActiveTab("calendar")}
            className={`px-4 py-2 font-medium transition-colors ${
              activeTab === "calendar"
                ? "text-blue-600 border-b-2 border-blue-600"
                : "text-gray-600 hover:text-gray-800"
            }`}
          >
            Lịch Thi
          </button>
        </div>

        {/* Schedule Tab Content */}
        {activeTab === "schedule" && (
          <>
            {/* Date Range Selector and Legend */}
            <div className="flex items-center gap-6 mb-6 flex-wrap">
              {/* Date Navigation */}
              <div className="flex items-center gap-4">
                <button 
                  onClick={handlePrevWeek}
                  className="p-1 hover:bg-gray-200 rounded"
                >
                  <ChevronLeft size={20} className="text-black" />
                </button>
                <span className="text-gray-700 font-medium">{formatDateRange(currentWeekStart)}</span>
                <button 
                  onClick={handleNextWeek}
                  className="p-1 hover:bg-gray-200 rounded"
                >
                  <ChevronRight size={20} className="text-black" />
                </button>
              </div>

              {/* Legend - Display only, not clickable */}
              <div className="flex items-center gap-4 flex-wrap">
                <span className="text-sm font-medium text-gray-700">Chú thích:</span>
                {legendItems.map((item, idx) => (
                  <div key={idx} className="flex items-center gap-2">
                    <div 
                      className="w-4 h-4 rounded border-2"
                      style={{
                        backgroundColor: item.backgroundColor,
                        borderColor: item.badgeColor,
                      }}
                    ></div>
                    <span className="text-sm text-gray-700">{item.type}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Schedule Grid */}
            <div 
              ref={scheduleScrollRef}
              className="overflow-x-auto overflow-y-hidden scroll-smooth"
              style={{
                scrollbarWidth: 'thin',
                scrollbarColor: '#888 #f1f1f1',
                maxHeight: 'calc(100vh - 300px)',
                height: 'auto',
              }}
            >
              <div className="min-w-max">
                {/* Hours Header */}
                <div className="flex border-b border-gray-300 sticky top-0 z-20">
                  <div className="w-20 flex-shrink-0 py-2 px-2 text-center font-medium text-gray-700 bg-gray-50">Giờ</div>
                  {hours.map((hour) => (
                    <div
                      key={hour}
                      className="w-24 flex-shrink-0 py-2 px-2 text-center font-medium text-gray-700 bg-gray-50 border-l border-gray-300"
                    >
                      {String(hour).padStart(2, "0")}:00
                    </div>
                  ))}
                </div>

                {/* Days and Schedule Items */}
                {days.map((day) => (
                  <div key={day} className="flex border-b border-gray-300 min-h-16">
                    <div className="w-20 flex-shrink-0 py-2 px-2 text-center font-medium text-gray-700 bg-gray-50 border-r border-gray-300 sticky left-0 z-10">
                      {day}
                    </div>

                    {/* Time slots */}
                    {hours.map((hour) => (
                      <div
                        key={`${day}-${hour}`}
                        className="w-24 flex-shrink-0 relative border-l border-gray-300 bg-gray-50"
                      >
                        {/* Schedule items for this slot */}
                        {getFilteredScheduleItems().map((item, idx) => {
                          const startHour = parseTimeToHours(item.time)
                          const duration = calculateDuration(item.time)
                          const endHour = startHour + duration
                          
                          // Only render in the starting hour slot
                          if (item.day === day && startHour >= hour && startHour < hour + 1) {
                            // Calculate left offset within the hour slot (0-100%)
                            const minutesInHour = (startHour - Math.floor(startHour)) * 60
                            const leftPercent = (minutesInHour / 60) * 100
                            
                            // Calculate width in pixels (each hour slot is 96px = w-24)
                            const widthPx = duration * 96
                            
                            return (
                              <div
                                key={idx}
                                className="absolute top-0 p-1.5 rounded m-0.5 text-xs cursor-pointer hover:shadow-md transition-shadow opacity-80"
                                style={{
                                  left: `${leftPercent}%`,
                                  width: `${widthPx}px`,
                                  backgroundColor: item.backgroundColor,
                                  zIndex: 10,
                                }}
                                onMouseEnter={(e) => {
                                  setHoveredItem(item)
                                  const rect = e.currentTarget.getBoundingClientRect()
                                  setTooltipPosition({
                                    x: rect.left + rect.width / 2,
                                    y: rect.top - 10,
                                  })
                                }}
                                onMouseLeave={() => setHoveredItem(null)}
                                onMouseMove={(e) => {
                                  const rect = e.currentTarget.getBoundingClientRect()
                                  setTooltipPosition({
                                    x: rect.left + rect.width / 2,
                                    y: rect.top - 10,
                                  })
                                }}
                              >
                                <div className="flex items-center gap-2 mb-0.5">
                                  <div className="font-bold text-gray-900 truncate flex-1">{item.title}</div>
                                  <span 
                                    className="text-white px-1.5 py-0.5 rounded text-xs font-medium flex-shrink-0"
                                    style={{ backgroundColor: item.badgeColor }}
                                  >
                                    {item.badgeText}
                                  </span>
                                </div>
                                <div className="text-gray-900 text-xs font-semibold truncate mb-0.5">{item.code}</div>
                                <div className="text-gray-900 text-xs font-medium truncate flex items-center gap-2 whitespace-nowrap">
                                  <span>⏰ {item.time}</span>
                                  <span>{item.room}</span>
                                </div>
                              </div>
                            )
                          }
                          return null
                        })}
                      </div>
                    ))}
                  </div>
                ))}
              </div>
            </div>

            {/* Tooltip for schedule items */}
            {hoveredItem && (
              <div
                className="fixed rounded-lg shadow-lg p-4 z-50 pointer-events-none"
                style={{
                  left: `${tooltipPosition.x}px`,
                  top: `${tooltipPosition.y}px`,
                  transform: 'translate(-50%, -100%)',
                  minWidth: '250px',
                  backgroundColor: hoveredItem.backgroundColor,
                }}
              >
                <div className="flex items-center justify-between mb-2">
                  <h3 className="font-bold text-gray-900 text-sm">{hoveredItem.title}</h3>
                  <span 
                    className="text-white px-2 py-1 rounded text-xs font-medium"
                    style={{ backgroundColor: hoveredItem.badgeColor }}
                  >
                    {hoveredItem.badgeText}
                  </span>
                </div>
                <p className="text-gray-900 text-xs font-semibold mb-2">{hoveredItem.code}</p>
                <div className="flex items-center gap-1 mb-2">
                  <span className="text-gray-900 text-xs font-medium">⏰ {hoveredItem.time}</span>
                </div>
                <p className="text-gray-900 text-xs font-medium">
                  Địa điểm: {hoveredItem.campus} - {hoveredItem.building} - {hoveredItem.room}
                </p>
              </div>
            )}
          </>
        )}

        {/* Calendar/Exam Tab Content */}
        {activeTab === "calendar" && (
          <div className="bg-white rounded-lg">
            <div className="space-y-4 p-6">
              <div className="bg-white rounded-lg p-6 border border-gray-200 shadow-sm">
                <div className="flex justify-between items-start mb-6">
                  <div>
                    <h3 className="text-lg font-bold text-gray-900 mb-1">Lập trình web</h3>
                    <p className="text-sm text-gray-600">IT1406, IT1526</p>
                  </div>
                  <div className="flex gap-2">
                    <span className="px-3 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded-md">Giữa kỳ</span>
                    <span className="px-3 py-1 bg-gray-800 text-white text-xs font-medium rounded-md">Giảng viên A</span>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <div className="flex gap-3">
                      <div>
                        <p className="text-sm font-medium text-gray-600 mb-0.5">Ngày thi</p>
                        <p className="text-sm text-gray-900">Thứ năm, 20/12/2025</p>
                      </div>
                    </div>
                    <div className="flex gap-3">
                      <div>
                        <p className="text-sm font-medium text-gray-600 mb-0.5">Địa điểm</p>
                        <p className="text-sm text-gray-900">Tòa nhà 1, cơ sở 2, phòng 104</p>
                      </div>
                    </div>
                  </div>
                  <div className="space-y-4">
                    <div className="flex gap-3">
                      <div>
                        <p className="text-sm font-medium text-gray-600 mb-0.5">Thời gian</p>
                        <p className="text-sm text-gray-900">8:00 - 10:30</p>
                      </div>
                    </div>
                    <div className="flex gap-3">
                      <div>
                        <p className="text-sm font-medium text-gray-600 mb-0.5">Tổng sinh viên</p>
                        <p className="text-sm text-gray-900">45</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
