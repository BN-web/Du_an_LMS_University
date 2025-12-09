"use client"

import { useState } from "react"

export default function TaoBaiTap({ onClose }) {
  const [formData, setFormData] = useState({
    tenBaiTap: "",
    loaiBaiTap: "Bài tập",
    thoiHanNop: "",
    soLanLamToiDa: 1,
    thoiGianLamBai: 45,
    chonLop: "WS01",
  })

  const [questions, setQuestions] = useState([
    {
      id: 1,
      content: "",
      options: [
        { id: "A", text: "" },
        { id: "B", text: "" },
        { id: "C", text: "" },
        { id: "D", text: "" },
      ],
      correctAnswer: "A",
    },
  ])

  const [showCalendar, setShowCalendar] = useState(false)
  const [showConfirmation, setShowConfirmation] = useState(false)
  const [selectedDate, setSelectedDate] = useState(null)
  const [calendarMonth, setCalendarMonth] = useState(new Date()) // Initialize calendar with current month

  const handleInputChange = (e) => {
    const { name, value } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: name === "soLanLamToiDa" || name === "thoiGianLamBai" ? Number.parseInt(value) : value,
    }))
  }

  const handleDateInputChange = (e) => {
    const value = e.target.value
    setFormData((prev) => ({
      ...prev,
      thoiHanNop: value,
    }))
    // Parse the date if it matches dd/mm/yyyy format
    const dateMatch = value.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/)
    if (dateMatch) {
      const day = parseInt(dateMatch[1])
      const month = parseInt(dateMatch[2]) - 1
      const year = parseInt(dateMatch[3])
      if (day >= 1 && day <= 31 && month >= 0 && month <= 11) {
        const date = new Date(year, month, day)
        if (date.getDate() === day && date.getMonth() === month && date.getFullYear() === year) {
          setSelectedDate(day)
          setCalendarMonth(date)
        }
      }
    }
  }

  const handleAddQuestion = () => {
    setQuestions((prev) => [
      ...prev,
      {
        id: Math.max(...prev.map((q) => q.id)) + 1,
        content: "",
        options: [
          { id: "A", text: "" },
          { id: "B", text: "" },
          { id: "C", text: "" },
          { id: "D", text: "" },
        ],
        correctAnswer: "A",
      },
    ])
  }

  const handleRemoveQuestion = (id) => {
    if (questions.length > 1) {
      setQuestions((prev) => prev.filter((q) => q.id !== id))
    }
  }

  const handleQuestionChange = (questionId, field, value) => {
    setQuestions((prev) => prev.map((q) => (q.id === questionId ? { ...q, [field]: value } : q)))
  }

  const handleOptionChange = (questionId, optionId, value) => {
    setQuestions((prev) =>
      prev.map((q) =>
        q.id === questionId
          ? {
              ...q,
              options: q.options.map((opt) => (opt.id === optionId ? { ...opt, text: value } : opt)),
            }
          : q,
      ),
    )
  }

  const handleSave = () => {
    setShowConfirmation(true)
  }

  const handleConfirm = () => {
    setShowConfirmation(false)
    console.log("Exercise saved:", formData, questions)
    onClose()
  }

  const getDaysInMonth = (date) => {
    return new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate()
  }

  const getFirstDayOfMonth = (date) => {
    return new Date(date.getFullYear(), date.getMonth(), 1).getDay()
  }

  const goToPreviousMonth = () => {
    setCalendarMonth(new Date(calendarMonth.getFullYear(), calendarMonth.getMonth() - 1))
  }

  const goToNextMonth = () => {
    setCalendarMonth(new Date(calendarMonth.getFullYear(), calendarMonth.getMonth() + 1))
  }

  const getMonthName = (date) => {
    const months = [
      "Tháng Một",
      "Tháng Hai",
      "Tháng Ba",
      "Tháng Tư",
      "Tháng Năm",
      "Tháng Sáu",
      "Tháng Bảy",
      "Tháng Tám",
      "Tháng Chín",
      "Tháng Mười",
      "Tháng Mười Một",
      "Tháng Mười Hai",
    ]
    return months[date.getMonth()]
  }

  const handleDateSelect = (day) => {
    setSelectedDate(day)
    setFormData((prev) => ({
      ...prev,
      thoiHanNop: `${day}/${calendarMonth.getMonth() + 1}/${calendarMonth.getFullYear()}`,
    }))
    setShowCalendar(false)
  }

  const handleTodayClick = () => {
    const today = new Date()
    const day = today.getDate()
    setSelectedDate(day)
    setCalendarMonth(today)
    setFormData((prev) => ({
      ...prev,
      thoiHanNop: `${day}/${today.getMonth() + 1}/${today.getFullYear()}`,
    }))
    setShowCalendar(false)
  }

  const handleClearDate = () => {
    setSelectedDate(null)
    setFormData((prev) => ({
      ...prev,
      thoiHanNop: "",
    }))
  }

  const renderCalendar = () => {
    const daysInMonth = getDaysInMonth(calendarMonth)
    const firstDay = getFirstDayOfMonth(calendarMonth)
    const days = []

    for (let i = 0; i < firstDay; i++) {
      days.push(<div key={`empty-${i}`}></div>)
    }

    for (let day = 1; day <= daysInMonth; day++) {
      days.push(
        <div
          key={day}
          onClick={() => handleDateSelect(day)}
          className="calendar-day"
          style={{
            backgroundColor: day === selectedDate ? "#60a5fa" : "transparent",
            color: day === selectedDate ? "#fff" : "#000",
          }}
          onMouseEnter={(e) => {
            if (day !== selectedDate) {
              e.target.style.backgroundColor = "#e5e7eb"
            }
          }}
          onMouseLeave={(e) => {
            if (day !== selectedDate) {
              e.target.style.backgroundColor = "transparent"
            }
          }}
        >
          {day}
        </div>,
      )
    }

    return days
  }

  return (
    <div style={{ backgroundColor: "#e8f4f8", minHeight: "100vh", padding: "20px" }}>
      <style>{`
        .time-slots {
          display: grid;
          grid-template-columns: repeat(6, 1fr);
          gap: 8px;
          margin-top: 10px;
        }
        .time-slot {
          padding: 8px;
          background: #e0e0e0;
          border-radius: 4px;
          text-align: center;
          cursor: pointer;
          font-size: 12px;
          font-weight: 500;
        }
        .time-slot.selected {
          background: #64b5f6;
          color: white;
        }
        .calendar-header {
          background: #424242;
          color: white;
          padding: 12px;
          border-radius: 4px 4px 0 0;
          display: flex;
          justify-content: space-between;
          align-items: center;
          font-weight: bold;
          min-width: 0;
        }
        .calendar-header span {
          flex: 1;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
          font-size: 14px;
        }
        .calendar-grid {
          display: grid;
          grid-template-columns: repeat(7, 1fr);
          gap: 4px;
          background: white;
          padding: 12px;
          border-radius: 0 0 4px 4px;
          min-width: 0;
        }
        .calendar-day {
          aspect-ratio: 1;
          display: flex;
          align-items: center;
          justify-content: center;
          border-radius: 4px;
          cursor: pointer;
          font-size: 13px;
          font-weight: 500;
          color: #000;
          min-width: 0;
          overflow: hidden;
        }
        .form-card {
          background: white;
          border-radius: 8px;
          padding: 24px;
          box-shadow: 0 2px 8px rgba(0,0,0,0.1);
          max-width: 600px;
          margin: 0 auto;
        }
        .form-group {
          margin-bottom: 20px;
        }
        .form-label {
          display: block;
          font-weight: 600;
          margin-bottom: 8px;
          color: #333;
          font-size: 14px;
        }
        .form-input {
          width: 100%;
          padding: 10px 12px;
          border: 1px solid #ddd;
          border-radius: 6px;
          font-size: 14px;
          font-family: inherit;
          color: #000;
        }
        .form-input:focus {
          outline: none;
          border-color: #5e35b1;
          box-shadow: 0 0 0 3px rgba(94, 53, 177, 0.1);
        }
        .form-row {
          display: grid;
          grid-template-columns: 1fr 1fr;
          gap: 20px;
        }
        .question-block {
          background: #f9f9f9;
          border: 1px solid #e0e0e0;
          border-radius: 6px;
          padding: 16px;
          margin-bottom: 16px;
        }
        .question-title {
          font-weight: 600;
          margin-bottom: 12px;
          color: #333;
          font-size: 14px;
        }
        .option-row {
          display: flex;
          align-items: center;
          gap: 12px;
          margin-bottom: 10px;
        }
        .option-label {
          min-width: 30px;
          font-weight: 600;
          color: #666;
        }
        .option-input {
          flex: 1;
          padding: 8px 12px;
          border: 1px solid #ddd;
          border-radius: 4px;
          font-size: 13px;
          color: #000;
        }
        .btn-group {
          display: flex;
          gap: 12px;
          justify-content: flex-end;
          margin-top: 24px;
        }
        .btn {
          padding: 10px 24px;
          border-radius: 6px;
          border: none;
          font-weight: 600;
          cursor: pointer;
          font-size: 14px;
          transition: all 0.2s;
        }
        .btn-secondary {
          background: white;
          border: 1px solid #ddd;
          color: #333;
        }
        .btn-secondary:hover {
          background: #f5f5f5;
        }
        .btn-primary {
          background: #5e35b1;
          color: white;
        }
        .btn-primary:hover {
          background: #4c2c91;
        }
        .modal-overlay {
          position: fixed;
          top: 0;
          left: 0;
          right: 0;
          bottom: 0;
          background: rgba(0, 0, 0, 0.5);
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 2000;
        }
        .modal-content {
          background: white;
          border-radius: 8px;
          padding: 24px;
          max-width: 500px;
          width: 90%;
          box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        }
        .confirmation-icon {
          width: 50px;
          height: 50px;
          background: #e8f5e9;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          margin-bottom: 16px;
          color: #4caf50;
          font-size: 28px;
        }
        .confirmation-title {
          font-size: 16px;
          font-weight: 600;
          color: #333;
          margin-bottom: 20px;
        }
        .confirmation-buttons {
          display: flex;
          gap: 12px;
          justify-content: flex-end;
        }
        .add-button {
          color: #5e35b1;
          background: none;
          border: none;
          cursor: pointer;
          font-weight: 600;
          font-size: 13px;
          display: flex;
          align-items: center;
          gap: 6px;
        }
        .back-link {
          display: flex;
          align-items: center;
          gap: 8px;
          color: #0066cc;
          text-decoration: none;
          margin-bottom: 20px;
          font-size: 14px;
          cursor: pointer;
        }
        .calendar-nav-btn {
          background: none;
          border: none;
          color: white;
          cursor: pointer;
          font-size: 16px;
          padding: 0 8px;
        }
        .calendar-nav-btn:hover {
          opacity: 0.8;
        }
      `}</style>

      <div className="form-card">
        {/* Header */}
        <div className="back-link" onClick={onClose}>
          <span>←</span> Quay lại
        </div>
        <h1 style={{ fontSize: "24px", fontWeight: "bold", marginBottom: "24px", color: "#333" }}>Tạo bài tập mới</h1>

        {/* Tên bài tập */}
        <div className="form-group">
          <label className="form-label">Tên bài tập</label>
          <input
            type="text"
            name="tenBaiTap"
            value={formData.tenBaiTap}
            onChange={handleInputChange}
            placeholder="Nhập tên bài tập"
            className="form-input"
          />
        </div>

        {/* Loại bài tập */}
        <div className="form-group">
          <label className="form-label">Loại bài tập</label>
          <select name="loaiBaiTap" value={formData.loaiBaiTap} onChange={handleInputChange} className="form-input">
            <option>Bài tập</option>
            <option>Kiểm tra</option>
            <option>Bài tập về nhà</option>
          </select>
        </div>

        {/* Thời hạn nộp & Số lần làm & Thời gian làm bài */}
        <div className="form-row">
          <div className="form-group">
            <label className="form-label">Thời hạn nộp</label>
            <div style={{ position: "relative" }}>
              <input
                type="text"
                name="thoiHanNop"
                value={formData.thoiHanNop}
                placeholder="dd/mm/yyyy"
                onChange={handleDateInputChange}
                onFocus={() => setShowCalendar(true)}
                className="form-input"
                style={{ cursor: "pointer" }}
              />
              {showCalendar && (
                <div
                  style={{
                    position: "absolute",
                    top: "100%",
                    left: 0,
                    width: "100%",
                    minWidth: "280px",
                    maxWidth: "100%",
                    marginTop: "8px",
                    zIndex: 100,
                    background: "white",
                    border: "1px solid #ddd",
                    borderRadius: "6px",
                    boxShadow: "0 2px 12px rgba(0,0,0,0.1)",
                    overflow: "hidden",
                  }}
                >
                  <div className="calendar-header">
                    <span>
                      {getMonthName(calendarMonth)} {calendarMonth.getFullYear()}
                    </span>
                    <div style={{ display: "flex", gap: "8px" }}>
                      <button className="calendar-nav-btn" onClick={goToPreviousMonth}>
                        ↑
                      </button>
                      <button className="calendar-nav-btn" onClick={goToNextMonth}>
                        ↓
                      </button>
                    </div>
                  </div>
                  <div className="calendar-grid">{renderCalendar()}</div>
                  <div
                    style={{
                      padding: "12px",
                      display: "flex",
                      justifyContent: "space-between",
                      borderTop: "1px solid #ddd",
                      fontSize: "12px",
                      color: "#0066cc",
                    }}
                  >
                    <span style={{ cursor: "pointer" }} onClick={handleClearDate}>
                      Xóa
                    </span>
                    <span style={{ cursor: "pointer" }} onClick={handleTodayClick}>
                      Hôm nay
                    </span>
                  </div>
                </div>
              )}
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">Số lần làm tối đa</label>
            <input
              type="number"
              name="soLanLamToiDa"
              value={formData.soLanLamToiDa}
              onChange={handleInputChange}
              className="form-input"
            />
          </div>
        </div>

        <div className="form-group">
          <label className="form-label">Thời gian làm bài (giờ)</label>
          <input
            type="text"
            name="thoiGianLamBai"
            value={formData.thoiGianLamBai}
            onChange={handleInputChange}
            placeholder="Ví dụ: 45"
            className="form-input"
          />
        </div>

        {/* Chọn lớp */}
        <div className="form-group">
          <label className="form-label">Chọn lớp</label>
          <select name="chonLop" value={formData.chonLop} onChange={handleInputChange} className="form-input">
            <option>WS01</option>
            <option>WS02</option>
            <option>WS03</option>
          </select>
        </div>

        {/* Câu hỏi trắc nghiệm */}
        <div className="form-group">
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
            <label className="form-label" style={{ marginBottom: 0 }}>
              Câu hỏi trắc nghiệm
            </label>
            <button className="add-button" onClick={handleAddQuestion}>
              + Thêm câu hỏi
            </button>
          </div>

          {questions.map((question, idx) => (
            <div key={question.id} className="question-block">
              <div
                style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "12px" }}
              >
                <div className="question-title">Câu hỏi {idx + 1}</div>
                {questions.length > 1 && (
                  <button
                    onClick={() => handleRemoveQuestion(question.id)}
                    style={{
                      background: "none",
                      border: "none",
                      color: "#ff5252",
                      cursor: "pointer",
                      fontSize: "16px",
                    }}
                  >
                    ×
                  </button>
                )}
              </div>

              <textarea
                value={question.content}
                onChange={(e) => handleQuestionChange(question.id, "content", e.target.value)}
                placeholder="Nhập nội dung câu hỏi"
                className="form-input"
                style={{ minHeight: "60px", resize: "vertical", marginBottom: "12px", color: "#000" }}
              />

              <div style={{ marginBottom: "8px", fontSize: "13px", fontWeight: "600", color: "#666" }}>
                Các đáp án (chọn đáp án đúng)
              </div>

              {question.options.map((option) => (
                <div key={option.id} className="option-row">
                  <input
                    type="radio"
                    name={`correct-${question.id}`}
                    value={option.id}
                    checked={question.correctAnswer === option.id}
                    onChange={(e) => handleQuestionChange(question.id, "correctAnswer", e.target.value)}
                    style={{ cursor: "pointer" }}
                  />
                  <span className="option-label">{option.id}.</span>
                  <input
                    type="text"
                    value={option.text}
                    onChange={(e) => handleOptionChange(question.id, option.id, e.target.value)}
                    placeholder={`Đáp án ${option.id}`}
                    className="option-input"
                  />
                </div>
              ))}
            </div>
          ))}
        </div>

        {/* Buttons */}
        <div className="btn-group">
          <button className="btn btn-secondary" onClick={onClose}>
            Hủy
          </button>
          <button className="btn btn-primary" onClick={handleSave}>
            Lưu & Đóng
          </button>
        </div>
      </div>

      {showConfirmation && (
        <div className="modal-overlay" onClick={() => setShowConfirmation(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="confirmation-icon">✓</div>
            <div className="confirmation-title">Xác nhận tạo bài tập</div>
            <div className="confirmation-buttons">
              <button className="btn btn-secondary" onClick={() => setShowConfirmation(false)}>
                Hủy
              </button>
              <button className="btn btn-primary" onClick={handleConfirm}>
                Xác nhận
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
