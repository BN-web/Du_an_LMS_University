"use client"

import { useState, useMemo } from "react"
import { Search, Clock, CheckCircle2, FileText, Calendar, BarChart3, X, Check } from "lucide-react"
import TaoBaiTap from "./loc/taobaitap"
import TaiLieuPage from "./loc/tailieu"
import XemChiTietPage from "./chitiet/xemchitiet"
import DownloadMaterials from "./loc/dowtailieu"

const exercises = [
  {
    id: "1",
    title: "Web cơ bản",
    subject: "Lập trình web",
    code: "IT1404",
    classId: "IT1404",
    creationDate: "07/10/2025",
    deadline: "13/11/2025",
    examType: "Giữa kì",
    timeRemaining: "Còn 1 ngày - 23:59 13/11/2025",
    submitted: 20,
    total: 45,
    notSubmitted: 25,
    attempts: 1,
    statuses: ["Chưa nộp", "Đã nộp", "Số lần làm"],
  },
  {
    id: "2",
    title: "Web cơ bản",
    subject: "Lập trình web",
    code: "IT1404",
    classId: "IT1404",
    creationDate: "07/10/2025",
    deadline: "13/11/2025",
    examType: "Giữa kì",
    timeRemaining: "Còn 1 ngày - 23:59 13/11/2025",
    submitted: 20,
    total: 45,
    notSubmitted: 25,
    attempts: 3,
    statuses: ["Chưa nộp", "Đã nộp", "Số lần làm"],
  },
  {
    id: "3",
    title: "Web cơ bản",
    subject: "Lập trình web",
    code: "IT1404",
    classId: "IT1404",
    creationDate: "07/10/2025",
    deadline: "13/11/2025",
    examType: "Giữa kì",
    timeRemaining: "Còn 1 ngày - 23:59 13/11/2025",
    submitted: 20,
    total: 45,
    notSubmitted: 25,
    attempts: 2,
    statuses: ["Chưa nộp", "Đã nộp", "Số lần làm"],
  },
  {
    id: "4",
    title: "Web cơ bản",
    subject: "Lập trình web",
    code: "IT1404",
    classId: "IT1404",
    creationDate: "07/10/2025",
    deadline: "13/11/2025",
    examType: "Giữa kì",
    timeRemaining: "Còn 1 ngày - 23:59 13/11/2025",
    submitted: 20,
    total: 45,
    notSubmitted: 25,
    attempts: 3,
    statuses: ["Chưa nộp", "Đã nộp", "Số lần làm"],
  },
  {
    id: "5",
    title: "Web cơ bản",
    subject: "Lập trình web",
    code: "IT1404",
    classId: "IT1404",
    creationDate: "07/10/2025",
    deadline: "13/11/2025",
    examType: "Giữa kì",
    timeRemaining: "Còn 1 ngày - 23:59 13/11/2025",
    submitted: 20,
    total: 45,
    notSubmitted: 25,
    attempts: 2,
    statuses: ["Chưa nộp", "Đã nộp", "Số lần làm"],
  },
  {
    id: "6",
    title: "Web cơ bản",
    subject: "Lập trình web",
    code: "IT1404",
    classId: "IT1404",
    creationDate: "07/10/2025",
    deadline: "13/11/2025",
    examType: "Giữa kì",
    timeRemaining: "Còn 1 ngày - 23:59 13/11/2025",
    submitted: 20,
    total: 45,
    notSubmitted: 25,
    attempts: 3,
    statuses: ["Chưa nộp", "Đã nộp", "Số lần làm"],
  },
]

export default function BaitapPage() {
  const [searchTerm, setSearchTerm] = useState("")
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [activeTab, setActiveTab] = useState("baitap")
  const [showDetail, setShowDetail] = useState(false)
  const [showUploadModal, setShowUploadModal] = useState(false)

  const progressPercentage = (20 / 45) * 100

  const handleViewDetails = () => {
    setShowDetail(true)
  }

  const handleBackFromDetail = () => {
    setShowDetail(false)
  }

  const handleCreateClick = () => {
    setShowCreateForm(true)
  }

  const handleCloseForm = () => {
    setShowCreateForm(false)
  }

  const handleUploadClick = () => {
    setShowUploadModal(true)
  }

  const handleCloseUploadModal = () => {
    setShowUploadModal(false)
  }

  // Filter exercises based on search term
  const filteredExercises = useMemo(() => {
    if (!searchTerm.trim()) {
      return exercises
    }

    const searchLower = searchTerm.toLowerCase().trim()
    return exercises.filter(
      (exercise) =>
        exercise.title.toLowerCase().includes(searchLower) ||
        exercise.subject.toLowerCase().includes(searchLower) ||
        exercise.code.toLowerCase().includes(searchLower)
    )
  }, [searchTerm])

  return (
    <>
      <style>{`
        .baitap-container {
          flex: 1;
          overflow: auto;
          background: linear-gradient(to bottom right, #eff6ff, #dbeafe);
          padding: 32px;
          min-height: 100vh;
        }

        .baitap-header {
          margin-bottom: 1rem;
        }

        .baitap-title {
          font-size: 2rem;
          font-weight: bold;
          color: #083b74;
          margin-bottom: 12px;
        }

        .baitap-card {
          background: white;
          border-radius: 1rem;
          box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
          padding: 10px;
        }

        .baitap-search-filters {
          display: flex;
          flex-direction: column;
          gap: 1rem;
          flex-wrap: wrap;
        }

        @media (min-width: 768px) {
          .baitap-search-filters {
            flex-direction: row;
            align-items: center;
            gap: 0.75rem;
          }
        }

        .baitap-search-wrapper {
          position: relative;
          flex: 1;
        }

        @media (min-width: 768px) {
          .baitap-search-wrapper {
            min-width: 320px;
          }
        }

        .baitap-search-icon {
          position: absolute;
          left: 0.75rem;
          top: 50%;
          transform: translateY(-50%);
          color: #9ca3af;
          pointer-events: none;
        }

        .baitap-search-input {
          width: 100%;
          padding: 0.5rem 1rem 0.5rem 2.5rem;
          border-radius: 0.5rem;
          border: 1px solid #d1d5db;
          background: white;
          color: #1f2937;
          font-size: 1rem;
        }

        .baitap-search-input::placeholder {
          color: #9ca3af;
        }

        .baitap-search-input:focus {
          outline: none;
          ring: 2px;
          ring-color: #60a5fa;
        }

        .baitap-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
          gap: 1.5rem;
        }

        @media (min-width: 768px) {
          .baitap-grid {
            grid-template-columns: repeat(2, 1fr);
          }
        }

        @media (min-width: 1024px) {
          .baitap-grid {
            grid-template-columns: repeat(3, 1fr);
          }
        }

        .baitap-card {
          background: white;
          border-radius: 0.75rem;
          padding: 1.5rem;
          box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
          transition: box-shadow 0.2s;
        }

        .baitap-card:hover {
          box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }

        .baitap-card-title {
          font-weight: 600;
          color: #1f2937;
          font-size: 1.125rem;
          margin-bottom: 1rem;
          display: -webkit-box;
          -webkit-line-clamp: 2;
          -webkit-box-orient: vertical;
          overflow: hidden;
        }

        .baitap-tags {
          display: flex;
          gap: 0.5rem;
          margin-bottom: 1rem;
        }

        .baitap-tag {
          display: inline-block;
          background: #f3f4f6;
          color: #374151;
          font-size: 0.75rem;
          font-weight: 600;
          padding: 0.25rem 0.75rem;
          border-radius: 0.25rem;
        }

        .baitap-time-info {
          display: flex;
          align-items: center;
          gap: 0.5rem;
          color: #6b7280;
          font-size: 0.875rem;
          margin-bottom: 1rem;
        }

        .baitap-progress-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 0.5rem;
        }

        .baitap-progress-label {
          color: #374151;
          font-weight: 500;
          font-size: 0.875rem;
        }

        .baitap-progress-count {
          color: #374151;
          font-weight: bold;
          font-size: 0.875rem;
        }

        .baitap-progress-bar {
          width: 100%;
          background: #e5e7eb;
          border-radius: 9999px;
          height: 0.5rem;
          overflow: hidden;
          margin-bottom: 1rem;
        }

        .baitap-progress-fill {
          background: #22c55e;
          height: 100%;
          border-radius: 9999px;
          transition: width 0.3s;
        }

        .baitap-statuses {
          display: flex;
          flex-wrap: wrap;
          gap: 0.5rem;
          margin-bottom: 1rem;
        }

        .baitap-status-tag {
          display: inline-flex;
          align-items: center;
          gap: 0.25rem;
          background: #dcfce7;
          color: #166534;
          font-size: 0.75rem;
          font-weight: 600;
          padding: 0.25rem 0.5rem;
          border-radius: 0.25rem;
        }

        .baitap-button {
          width: 100%;
          padding: 0.5rem 1rem;
          border-radius: 0.5rem;
          border: 1px solid #d1d5db;
          background: transparent;
          color: #374151;
          font-weight: 500;
          cursor: pointer;
          transition: background 0.2s;
        }

        .baitap-button:hover {
          background: #f9fafb;
        }
      `}</style>

      {/* Upload Modal */}
      <DownloadMaterials isOpen={showUploadModal} onClose={handleCloseUploadModal} />

      {showCreateForm && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: "rgba(0, 0, 0, 0.5)",
            overflowY: "auto",
            zIndex: 1000,
          }}
        >
          <div style={{ padding: "20px" }}>
            <TaoBaiTap onClose={handleCloseForm} />
          </div>
        </div>
      )}

      {showDetail ? (
        <XemChiTietPage onBack={handleBackFromDetail} />
      ) : (
        <>
          {/* Common Header for both tabs */}
          <div className="baitap-container">
            {/* Title outside card */}
            <h1 className="baitap-title">Bài tập</h1>

            {/* Main Card Container */}
            <div className="baitap-card">
              <div className="baitap-header">
                {/* Search and Filters */}
                <div className="baitap-search-filters">
                {/* Search Bar */}
                <div className="baitap-search-wrapper">
                  <Search className="baitap-search-icon" size={20} />
                  <input
                    type="text"
                    placeholder="Search"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="baitap-search-input"
                  />
                </div>

                {/* Filter Buttons */}
                <button
                  onClick={() => setActiveTab("baitap")}
                  className={`px-4 py-2 rounded-lg border font-medium ${
                    activeTab === "baitap"
                      ? "bg-blue-600 text-white border-blue-600 hover:bg-blue-700"
                      : "bg-white border-gray-300 text-gray-700 hover:bg-gray-50"
                  }`}
                >
                  Bài tập
                </button>

                <button
                  onClick={() => setActiveTab("tailieu")}
                  className={`px-4 py-2 rounded-lg border font-medium ${
                    activeTab === "tailieu"
                      ? "bg-blue-600 text-white border-blue-600 hover:bg-blue-700"
                      : "bg-white border-gray-300 text-gray-700 hover:bg-gray-50"
                  }`}
                >
                  Tài liệu
                </button>

                {activeTab === "baitap" ? (
                  <button
                    onClick={handleCreateClick}
                    className="px-4 py-2 rounded-lg bg-white border border-gray-300 text-gray-700 hover:bg-gray-50 font-medium"
                  >
                    Tạo bài tập
                  </button>
                ) : (
                  <button
                    onClick={handleUploadClick}
                    className="px-4 py-2 rounded-lg bg-white border border-gray-300 text-gray-700 hover:bg-gray-50 font-medium"
                  >
                    Tải tài liệu
                  </button>
                )}
              </div>
            </div>

            {/* Content based on active tab */}
            {activeTab === "baitap" ? (
              <div className="baitap-grid">
                {filteredExercises.length === 0 ? (
                  <div className="col-span-full text-center py-12">
                    <p className="text-gray-500 text-lg">Không tìm thấy bài tập nào</p>
                  </div>
                ) : (
                  filteredExercises.map((exercise) => {
                  const progressPercentage = (exercise.submitted / exercise.total) * 100
                  return (
                    <div key={exercise.id} className="baitap-card" style={{ padding: 0, overflow: "hidden" }}>
                      {/* Header with Gradient */}
                      <div
                        style={{
                          background: "linear-gradient(to right, #93c5fd, #60a5fa)",
                          padding: "1rem",
                          display: "flex",
                          justifyContent: "space-between",
                          alignItems: "flex-start",
                        }}
                      >
                        <div>
                          <div style={{ display: "flex", alignItems: "center", gap: "0.5rem", marginBottom: "0.25rem" }}>
                            <FileText size={16} color="white" />
                            <span style={{ color: "white", fontSize: "0.875rem", fontWeight: 600 }}>{exercise.title}</span>
                          </div>
                          <div style={{ color: "rgba(255, 255, 255, 0.9)", fontSize: "0.75rem", marginLeft: "1.5rem" }}>{exercise.subject}</div>
                        </div>
                        <button
                          style={{
                            backgroundColor: "rgba(255, 255, 255, 0.2)",
                            color: "white",
                            padding: "0.375rem 0.75rem",
                            borderRadius: "0.375rem",
                            border: "1px solid rgba(255, 255, 255, 0.3)",
                            fontSize: "0.75rem",
                            fontWeight: 500,
                            cursor: "pointer",
                            backdropFilter: "blur(10px)",
                          }}
                        >
                          {exercise.examType}
                        </button>
                      </div>

                      {/* Information Cards */}
                      <div style={{ padding: "1.25rem" }}>
                        {/* First row: Mã lớp and Ngày tạo */}
                        <div style={{ display: "grid", gridTemplateColumns: "repeat(2, 1fr)", gap: "1rem", marginBottom: "1rem" }}>
                          {/* Mã lớp */}
                          <div style={{ backgroundColor: "#eff6ff", padding: "0.875rem", borderRadius: "0.625rem", display: "flex", alignItems: "center", gap: "0.625rem", border: "1px solid #dbeafe" }}>
                            <BarChart3 size={18} color="#60a5fa" />
                            <div>
                              <div style={{ fontSize: "0.75rem", color: "#94a3b8", marginBottom: "0.25rem" }}>Mã lớp</div>
                              <div style={{ fontSize: "0.875rem", fontWeight: 600, color: "#475569" }}>{exercise.classId}</div>
                            </div>
                          </div>

                          {/* Ngày tạo */}
                          <div style={{ backgroundColor: "#f0fdf4", padding: "0.875rem", borderRadius: "0.625rem", display: "flex", alignItems: "center", gap: "0.625rem", border: "1px solid #dcfce7" }}>
                            <Calendar size={18} color="#4ade80" />
                            <div>
                              <div style={{ fontSize: "0.75rem", color: "#94a3b8", marginBottom: "0.25rem" }}>Ngày tạo</div>
                              <div style={{ fontSize: "0.875rem", fontWeight: 600, color: "#475569" }}>{exercise.creationDate}</div>
                            </div>
                          </div>
                        </div>

                        {/* Second row: Hạn nộp */}
                        <div style={{ backgroundColor: "#fef2f2", padding: "0.875rem", borderRadius: "0.625rem", display: "flex", alignItems: "center", gap: "0.625rem", border: "1px solid #fee2e2" }}>
                          <Clock size={18} color="#f87171" />
                          <div>
                            <div style={{ fontSize: "0.75rem", color: "#94a3b8", marginBottom: "0.25rem" }}>Hạn nộp</div>
                            <div style={{ fontSize: "0.875rem", fontWeight: 600, color: "#475569" }}>{exercise.deadline}</div>
                          </div>
                        </div>
                      </div>

                      {/* Progress Section */}
                      <div style={{ padding: "0 1.25rem", marginBottom: "1.25rem" }}>
                        <div style={{ marginBottom: "0.5rem", fontSize: "0.75rem", fontWeight: 500, color: "#64748b" }}>
                          Tiến độ nộp bài
                        </div>
                        <div style={{ position: "relative", marginBottom: "0.5rem" }}>
                          <div
                            style={{
                              width: "100%",
                              height: "0.5rem",
                              backgroundColor: "#f1f5f9",
                              borderRadius: "9999px",
                              overflow: "hidden",
                            }}
                          >
                            <div
                              style={{
                                width: `${progressPercentage}%`,
                                height: "100%",
                                background: "linear-gradient(to right, #93c5fd, #60a5fa)",
                                borderRadius: "9999px",
                              }}
                            />
                          </div>
                        </div>
                        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                          <span style={{ fontSize: "0.75rem", color: "#94a3b8" }}>{Math.round(progressPercentage)}% hoàn thành</span>
                          <span style={{ fontSize: "0.75rem", fontWeight: 600, color: "#64748b" }}>
                            {exercise.submitted}/{exercise.total}
                          </span>
                        </div>
                      </div>

                      {/* Status Cards */}
                      <div style={{ padding: "0 1.25rem", display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "1rem", marginBottom: "1.25rem" }}>
                        {/* Chưa nộp */}
                        <div style={{ backgroundColor: "#fef2f2", padding: "0.625rem", borderRadius: "0.5rem", display: "flex", alignItems: "center", gap: "0.5rem", border: "1px solid #fee2e2" }}>
                          <div style={{ width: "20px", height: "20px", borderRadius: "50%", backgroundColor: "#fca5a5", display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0 }}>
                            <X size={10} color="white" />
                          </div>
                          <div>
                            <div style={{ fontSize: "0.625rem", color: "#94a3b8" }}>Chưa nộp</div>
                            <div style={{ fontSize: "0.75rem", fontWeight: 600, color: "#64748b" }}>{exercise.notSubmitted}</div>
                          </div>
                        </div>

                        {/* Đã nộp */}
                        <div style={{ backgroundColor: "#f0fdf4", padding: "0.625rem", borderRadius: "0.5rem", display: "flex", alignItems: "center", gap: "0.5rem", border: "1px solid #dcfce7" }}>
                          <div style={{ width: "20px", height: "20px", borderRadius: "50%", backgroundColor: "#86efac", display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0 }}>
                            <Check size={10} color="white" />
                          </div>
                          <div>
                            <div style={{ fontSize: "0.625rem", color: "#94a3b8" }}>Đã nộp</div>
                            <div style={{ fontSize: "0.75rem", fontWeight: 600, color: "#64748b" }}>{exercise.submitted}</div>
                          </div>
                        </div>

                        {/* Số lần làm */}
                        <div style={{ backgroundColor: "#eff6ff", padding: "0.625rem", borderRadius: "0.5rem", display: "flex", alignItems: "center", gap: "0.5rem", border: "1px solid #dbeafe" }}>
                          <BarChart3 size={16} color="#60a5fa" style={{ flexShrink: 0 }} />
                          <div>
                            <div style={{ fontSize: "0.625rem", color: "#94a3b8" }}>Số lần làm</div>
                            <div style={{ fontSize: "0.75rem", fontWeight: 600, color: "#64748b" }}>{exercise.attempts}</div>
                          </div>
                        </div>
                      </div>
``
                      {/* Button */}
                      <div style={{ padding: "0 1.25rem 1.25rem 1.25rem" }}>
                        <button
                          onClick={handleViewDetails}
                          style={{
                            width: "100%",
                            padding: "0.75rem",
                            borderRadius: "0.5rem",
                            border: "none",
                            background: "linear-gradient(to right, #93c5fd, #60a5fa)",
                            color: "white",
                            fontWeight: 500,
                            cursor: "pointer",
                            fontSize: "0.75rem",
                            boxShadow: "0 2px 8px rgba(96, 165, 250, 0.2)",
                          }}
                        >
                          Xem chi tiết
                        </button>
                      </div>
                    </div>
                  )
                  })
                )}
              </div>
            ) : (
              <TaiLieuPage onBackToBaitap={() => setActiveTab("baitap")} />
            )}
            </div>
          </div>
        </>
      )}
    </>
  )
}

