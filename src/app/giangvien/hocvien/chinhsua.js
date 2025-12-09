"use client"

import { useState, useEffect } from "react"

import { X } from "lucide-react"

const scoreToGrade = (score) => {
  if (score >= 9) return "A+"
  if (score >= 8.5) return "A"
  if (score >= 8) return "B+"
  if (score >= 7) return "B"
  if (score >= 6.5) return "C+"
  if (score >= 6) return "C"
  if (score >= 5.5) return "D+"
  if (score >= 5) return "D"
  return "F"
}

const scoreToGPA = (score) => {
  if (score >= 9) return 4.0
  if (score >= 8.5) return 3.7
  if (score >= 8) return 3.5
  if (score >= 7) return 3.0
  if (score >= 6.5) return 2.5
  if (score >= 6) return 2.0
  if (score >= 5.5) return 1.5
  if (score >= 5) return 1.0
  return 0
}

export default function ChinhSua({ student, subject, onClose, onSave }) {
  const [newScores, setNewScores] = useState({
    homework: "",
    midterm: "",
    final: "",
    participation: "",
  })

  const [averageScore, setAverageScore] = useState(subject.averageScore || 0)
  const [letterGrade, setLetterGrade] = useState(subject.letterGrade || "A")
  const [gpa, setGPA] = useState(subject.gpa || 4.0)
  const [errorMessage, setErrorMessage] = useState("")

  const calculateAverage = (homework, midterm, final, participation) => {
    const getNumericValue = (value) => {
      if (value === "" || value === null || value === undefined) return null
      if (value === "--") return null
      const num = Number.parseFloat(value)
      return isNaN(num) ? null : num
    }

    const hw = getNumericValue(homework) ?? (subject.homework === "--" ? 0 : Number.parseFloat(subject.homework) || 0)
    const mid = getNumericValue(midterm) ?? (subject.midterm === "--" ? 0 : Number.parseFloat(subject.midterm) || 0)
    const fin = getNumericValue(final) ?? (subject.final === "--" ? 0 : Number.parseFloat(subject.final) || 0)
    const part = getNumericValue(participation) ?? (subject.participation || 0)

    const average = hw * 0.1 + mid * 0.3 + fin * 0.5 + part * 0.1
    return Math.round(average * 10) / 10
  }

  useEffect(() => {
    const avg = calculateAverage(newScores.homework, newScores.midterm, newScores.final, newScores.participation)
    setAverageScore(avg)
    setLetterGrade(scoreToGrade(avg))
    setGPA(scoreToGPA(avg))
  }, [newScores])

  const handleScoreChange = (field, value) => {
    // Clear error message when user starts typing
    if (errorMessage) {
      setErrorMessage("")
    }
    
    // Allow empty string, numbers, and decimal point during typing
    // This regex allows: empty, digits, single decimal point, and decimal numbers
    // Don't restrict the range during typing - allow any number to be entered
    // Validation will be shown visually with red border, but user can still type and correct
    if (value === "" || /^[0-9]*\.?[0-9]*$/.test(value)) {
      setNewScores((prev) => ({
        ...prev,
        [field]: value,
      }))
    }
  }

  const handleSave = () => {
    // Validate all scores are between 1 and 10
    const validateScore = (value, fieldName) => {
      if (value === "" || value === null || value === undefined) return null
      const num = Number.parseFloat(value)
      if (isNaN(num)) return null
      if (num < 1 || num > 10) {
        return `${fieldName} phải trong khoảng 1-10`
      }
      return null
    }

    const errors = []
    const fieldNames = {
      homework: "Điểm bài tập",
      midterm: "Điểm giữa kỳ",
      final: "Điểm cuối kỳ",
      participation: "Điểm chuyên cần",
    }

    // Check each field that has a new value
    if (newScores.homework !== "") {
      const error = validateScore(newScores.homework, fieldNames.homework)
      if (error) errors.push(error)
    }
    if (newScores.midterm !== "") {
      const error = validateScore(newScores.midterm, fieldNames.midterm)
      if (error) errors.push(error)
    }
    if (newScores.final !== "") {
      const error = validateScore(newScores.final, fieldNames.final)
      if (error) errors.push(error)
    }
    if (newScores.participation !== "") {
      const error = validateScore(newScores.participation, fieldNames.participation)
      if (error) errors.push(error)
    }

    // If there are errors, show them and don't save
    if (errors.length > 0) {
      setErrorMessage(errors.join(". ") + ". Vui lòng nhập lại.")
      return
    }

    // Clear error message if validation passes
    setErrorMessage("")

    const getValue = (newValue, oldValue) => {
      if (newValue !== "") {
        const num = Number.parseFloat(newValue)
        return isNaN(num) ? oldValue : num
      }
      return oldValue === "--" ? "--" : oldValue
    }

    const updatedScores = {
      homework: getValue(newScores.homework, subject.homework),
      midterm: getValue(newScores.midterm, subject.midterm),
      final: getValue(newScores.final, subject.final),
      participation: newScores.participation !== "" ? Number.parseFloat(newScores.participation) : subject.participation,
      averageScore,
      letterGrade,
      gpa,
    }
    onSave(updatedScores)
  }

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex justify-between items-start p-6 border-b">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Chỉnh sửa điểm sinh viên</h1>
            <p className="text-sm text-blue-600 mt-1">
              Chỉnh sửa các thành phần điểm cho sinh viên trong môn học đã chọn.
            </p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 transition">
            <X size={24} />
          </button>
        </div>

        {/* Student Info */}
        <div className="grid grid-cols-2 gap-6 p-6 border-b bg-gray-50">
          <div>
            <label className="text-sm text-gray-600">MSSV</label>
            <p className="text-lg font-semibold text-gray-900">{student.id || student.mssv}</p>
          </div>
          <div>
            <label className="text-sm text-gray-600">Họ tên</label>
            <p className="text-lg font-semibold text-gray-900">{student.name}</p>
          </div>
        </div>

        {/* Scores Section */}
        <div className="p-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Score Components */}
            <div className="lg:col-span-2">
              <div className="space-y-4">
                <div className="grid grid-cols-3 gap-4 text-sm font-semibold text-gray-900 mb-4">
                  <div>Thành phần</div>
                  <div>Điểm cũ</div>
                  <div>Điểm mới</div>
                </div>

                {/* Homework */}
                <div className="grid grid-cols-3 gap-4 items-center">
                  <div className="text-gray-900 font-medium">Bài tập</div>
                  <div className="text-gray-900">{subject.homework === "--" ? "--" : subject.homework}</div>
                  <input
                    type="text"
                    inputMode="decimal"
                    placeholder="1-10"
                    value={newScores.homework}
                    onChange={(e) => handleScoreChange("homework", e.target.value)}
                    className={`border rounded px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      newScores.homework &&
                      (Number.parseFloat(newScores.homework) < 1 || Number.parseFloat(newScores.homework) > 10)
                        ? "border-red-500 focus:ring-red-500"
                        : "border-gray-300"
                    }`}
                  />
                </div>

                {/* Midterm */}
                <div className="grid grid-cols-3 gap-4 items-center">
                  <div className="text-gray-900 font-medium">Giữa kỳ</div>
                  <div className="text-gray-900">{subject.midterm === "--" ? "--" : subject.midterm}</div>
                  <input
                    type="text"
                    inputMode="decimal"
                    placeholder="1-10"
                    value={newScores.midterm}
                    onChange={(e) => handleScoreChange("midterm", e.target.value)}
                    className={`border rounded px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      newScores.midterm &&
                      (Number.parseFloat(newScores.midterm) < 1 || Number.parseFloat(newScores.midterm) > 10)
                        ? "border-red-500 focus:ring-red-500"
                        : "border-gray-300"
                    }`}
                  />
                </div>

                {/* Final */}
                <div className="grid grid-cols-3 gap-4 items-center">
                  <div className="text-gray-900 font-medium">Cuối kỳ</div>
                  <div className="text-gray-900">{subject.final === "--" ? "--" : subject.final}</div>
                  <input
                    type="text"
                    inputMode="decimal"
                    placeholder="1-10"
                    value={newScores.final}
                    onChange={(e) => handleScoreChange("final", e.target.value)}
                    className={`border rounded px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      newScores.final &&
                      (Number.parseFloat(newScores.final) < 1 || Number.parseFloat(newScores.final) > 10)
                        ? "border-red-500 focus:ring-red-500"
                        : "border-gray-300"
                    }`}
                  />
                </div>

                {/* Participation */}
                <div className="grid grid-cols-3 gap-4 items-center">
                  <div className="text-gray-900 font-medium">Chuyên cần</div>
                  <div className="text-gray-900">{subject.participation}</div>
                  <input
                    type="text"
                    inputMode="decimal"
                    placeholder="1-10"
                    value={newScores.participation}
                    onChange={(e) => handleScoreChange("participation", e.target.value)}
                    className={`border rounded px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      newScores.participation &&
                      (Number.parseFloat(newScores.participation) < 1 || Number.parseFloat(newScores.participation) > 10)
                        ? "border-red-500 focus:ring-red-500"
                        : "border-gray-300"
                    }`}
                  />
                </div>
              </div>

              <p className="text-xs text-gray-500 mt-6">Lịch sử chỉnh sửa điểm sẽ được lưu lại.</p>
              
              {/* Error Message */}
              {errorMessage && (
                <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-700 font-medium">{errorMessage}</p>
                </div>
              )}
            </div>

            {/* Calculated Results */}
            <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-lg p-4 flex flex-col justify-center space-y-6">
              <div>
                <p className="text-sm text-gray-900 mb-1">Điểm trung bình môn</p>
                <p className="text-3xl font-bold text-gray-900">{averageScore.toFixed(1)}</p>
              </div>

              <div>
                <p className="text-sm text-gray-900 mb-1">Điểm chữ</p>
                <p className="text-3xl font-bold text-gray-900">{letterGrade}</p>
              </div>

              <div>
                <p className="text-sm text-gray-900 mb-1">GPA môn</p>
                <p className="text-3xl font-bold text-gray-900">{gpa.toFixed(1)}</p>
              </div>
            </div>
          </div>
        </div>

        {/* Footer Buttons */}
        <div className="flex justify-end gap-3 p-6 border-t bg-gray-50">
          <button
            onClick={onClose}
            className="px-6 py-2 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-100 transition"
          >
            Hủy
          </button>
          <button
            onClick={handleSave}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition"
          >
            Lưu
          </button>
        </div>
      </div>
    </div>
  )
}
