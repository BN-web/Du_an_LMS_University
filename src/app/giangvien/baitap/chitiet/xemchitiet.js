"use client"

import { useState } from "react"
import { Download, Clock, CheckCircle, AlertCircle, ArrowLeft } from "lucide-react"
import CauHoiComponent from "./cauhoi"
import XemBaiLamPage from "./xembailam"

const students = [
  {
    maSv: "SV001",
    hoTen: "Nguyễn Văn An",
    thoiGianNop: "10:30 12/11/2025",
    soLanLam: 1,
    trangThai: "Đã nộp",
    diem: 8.5,
  },
  {
    maSv: "SV002",
    hoTen: "Trần Thị Bình",
    thoiGianNop: "14:20 12/11/2025",
    soLanLam: 1,
    trangThai: "Đã nộp",
    diem: 9,
  },
  {
    maSv: "SV003",
    hoTen: "Lê Hoàng Cường",
    thoiGianNop: "09:15 13/11/2025",
    soLanLam: 1,
    trangThai: "Đã nộp",
    diem: 8.5,
  },
  {
    maSv: "SV004",
    hoTen: "Phạm Thị Dung",
    thoiGianNop: "23:55 13/11/2025",
    soLanLam: 2,
    trangThai: "Đã nộp",
    diem: 7.5,
  },
  {
    maSv: "SV005",
    hoTen: "Hoàng Văn Em",
    thoiGianNop: "16:30 12/11/2025",
    soLanLam: 1,
    trangThai: "Đã nộp",
    diem: 7.0,
  },
]

const XemChiTietPage = ({ onBack }) => {
  const [activeTab, setActiveTab] = useState("students")
  const [showSubmission, setShowSubmission] = useState(false)
  const [selectedStudent, setSelectedStudent] = useState(null)

  const handleViewSubmission = (student) => {
    setSelectedStudent(student)
    setShowSubmission(true)
  }

  if (showSubmission && selectedStudent) {
    return (
      <XemBaiLamPage
        onBack={() => setShowSubmission(false)}
        studentId={selectedStudent.maSv}
      />
    )
  }

  return (
    <>
      <style>{`
        .xemchitiet-container {
          flex: 1;
          overflow: auto;
          background: linear-gradient(to bottom right, #dbeafe, #eff6ff, #dbeafe);
          padding: 1rem;
        }

        @media (min-width: 768px) {
          .xemchitiet-container {
            padding: 2rem;
          }
        }

        .xemchitiet-box {
          background: white;
          border-radius: 0.75rem;
          padding: 1.5rem;
          box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }

        @media (min-width: 768px) {
          .xemchitiet-box {
            padding: 2rem;
          }
        }

        .xemchitiet-header {
          display: flex;
          flex-direction: column;
          gap: 1rem;
          margin-bottom: 1.5rem;
        }

        @media (min-width: 768px) {
          .xemchitiet-header {
            flex-direction: row;
            align-items: flex-start;
            justify-content: space-between;
          }
        }

        .xemchitiet-title-section h1 {
          font-size: 1.5rem;
          font-weight: bold;
          color: #1f2937;
          margin-bottom: 0.5rem;
        }

        @media (min-width: 768px) {
          .xemchitiet-title-section h1 {
            font-size: 1.875rem;
          }
        }

        .xemchitiet-description {
          color: #4b5563;
          font-size: 0.875rem;
        }

        @media (min-width: 768px) {
          .xemchitiet-description {
            font-size: 1rem;
          }
        }

        .xemchitiet-download-btn {
          display: inline-flex;
          align-items: center;
          gap: 0.5rem;
          padding: 0.5rem 1rem;
          border-radius: 0.5rem;
          background: white;
          border: 1px solid #d1d5db;
          color: #374151;
          cursor: pointer;
          font-weight: 500;
          white-space: nowrap;
          transition: background 0.2s;
        }

        .xemchitiet-download-btn:hover {
          background: #f9fafb;
        }

        .xemchitiet-tags {
          display: flex;
          flex-wrap: wrap;
          gap: 0.5rem;
          margin-bottom: 1.5rem;
        }

        .xemchitiet-tag {
          display: inline-block;
          padding: 0.25rem 0.75rem;
          border-radius: 0.25rem;
          font-size: 0.75rem;
          font-weight: 600;
        }

        .xemchitiet-tag-gray {
          background: #e5e7eb;
          color: #374151;
        }

        .xemchitiet-tag-purple {
          background: #ede9fe;
          color: #7c3aed;
        }

        .xemchitiet-stats {
          display: grid;
          grid-template-columns: repeat(2, 1fr);
          gap: 1rem;
          margin-bottom: 1.5rem;
        }

        @media (min-width: 768px) {
          .xemchitiet-stats {
            grid-template-columns: repeat(4, 1fr);
          }
        }

        .xemchitiet-stat-box {
          border-radius: 0.5rem;
          padding: 1rem;
        }

        .xemchitiet-stat-box-blue {
          background: #eff6ff;
        }

        .xemchitiet-stat-box-purple {
          background: #faf5ff;
        }

        .xemchitiet-stat-box-green {
          background: #f0fdf4;
        }

        .xemchitiet-stat-box-orange {
          background: #fffbeb;
        }

        .xemchitiet-stat-header {
          display: flex;
          align-items: center;
          gap: 0.5rem;
          margin-bottom: 0.5rem;
        }

        .xemchitiet-stat-icon-blue {
          color: #2563eb;
        }

        .xemchitiet-stat-icon-purple {
          color: #9333ea;
        }

        .xemchitiet-stat-icon-green {
          color: #16a34a;
        }

        .xemchitiet-stat-icon-orange {
          color: #ea580c;
        }

        .xemchitiet-stat-label {
          font-weight: 600;
          font-size: 0.875rem;
        }

        .xemchitiet-stat-label-blue {
          color: #2563eb;
        }

        .xemchitiet-stat-label-purple {
          color: #9333ea;
        }

        .xemchitiet-stat-label-green {
          color: #16a34a;
        }

        .xemchitiet-stat-label-orange {
          color: #ea580c;
        }

        .xemchitiet-stat-value {
          color: #1f2937;
          font-weight: bold;
          font-size: 0.875rem;
        }

        .xemchitiet-max-attempts {
          margin-bottom: 1.5rem;
          padding-bottom: 1.5rem;
          border-bottom: 1px solid #e5e7eb;
        }

        .xemchitiet-max-attempts-text {
          color: #4b5563;
          font-size: 0.875rem;
        }

        .xemchitiet-tabs {
          display: flex;
          gap: 0.5rem;
          margin-bottom: 1.5rem;
          border-bottom: 1px solid #e5e7eb;
        }

        .xemchitiet-tab-btn {
          padding: 0.5rem 1rem;
          font-weight: 600;
          font-size: 0.875rem;
          border: none;
          background: transparent;
          cursor: pointer;
          border-bottom: 2px solid transparent;
          color: #4b5563;
          transition: all 0.2s;
          border-radius: 0.5rem 0.5rem 0 0;
        }

        .xemchitiet-tab-btn.active {
          background: #f3f4f6;
          color: #1f2937;
          border-bottom-color: #1f2937;
        }

        .xemchitiet-tab-content h3 {
          font-weight: 600;
          color: #1f2937;
          margin-bottom: 1rem;
        }

        .xemchitiet-table {
          width: 100%;
          font-size: 0.875rem;
          overflow-x: auto;
        }

        .xemchitiet-table table {
          width: 100%;
          border-collapse: collapse;
        }

        .xemchitiet-table thead tr {
          border-bottom: 1px solid #e5e7eb;
        }

        .xemchitiet-table th {
          text-align: left;
          padding: 0.75rem 1rem;
          font-weight: 600;
          color: #374151;
        }

        .xemchitiet-table tbody tr {
          border-bottom: 1px solid #f3f4f6;
        }

        .xemchitiet-table tbody tr:hover {
          background: #f9fafb;
        }

        .xemchitiet-table td {
          padding: 0.75rem 1rem;
          color: #374151;
        }

        .xemchitiet-table td.font-medium {
          font-weight: 500;
        }

        .xemchitiet-status-badge {
          display: inline-block;
          background: #dbeafe;
          color: #1e40af;
          font-size: 0.75rem;
          font-weight: 600;
          padding: 0.25rem 0.75rem;
          border-radius: 0.25rem;
        }

        .xemchitiet-score {
          color: #16a34a;
          font-weight: 600;
        }

        .xemchitiet-action-link {
          color: #2563eb;
          text-decoration: none;
          font-weight: 600;
          font-size: 0.875rem;
          cursor: pointer;
          transition: color 0.2s;
        }

        .xemchitiet-action-link:hover {
          color: #1d4ed8;
        }

        .xemchitiet-questions-list {
          display: flex;
          flex-direction: column;
          gap: 1rem;
        }

        .xemchitiet-question-item {
          background: #f9fafb;
          border-radius: 0.5rem;
          padding: 1rem;
          border: 1px solid #e5e7eb;
        }

        .xemchitiet-question-title {
          font-weight: 600;
          color: #1f2937;
          margin-bottom: 0.5rem;
        }

        .xemchitiet-question-desc {
          color: #4b5563;
          font-size: 0.875rem;
        }

        .xemchitiet-back-btn {
          display: inline-flex;
          align-items: center;
          gap: 0.5rem;
          padding: 0.5rem 1rem;
          background: transparent;
          border: none;
          color: #2563eb;
          cursor: pointer;
          font-weight: 600;
          font-size: 0.875rem;
          transition: color 0.2s;
          margin-bottom: 1.5rem;
        }

        .xemchitiet-back-btn:hover {
          color: #1d4ed8;
        }
      `}</style>

      <div className="xemchitiet-container">
        {onBack && (
          <button onClick={onBack} className="xemchitiet-back-btn">
            <ArrowLeft size={20} />
            Quay lại
          </button>
        )}

        <div className="xemchitiet-box">
          {/* Header with Title and Download Button */}
          <div className="xemchitiet-header">
            <div className="xemchitiet-title-section">
              <h1>Kiểm tra giữa kỳ - Web cơ bản</h1>
              <p className="xemchitiet-description">
                Bài kiểm tra kiến thức HTML, CSS và Firebase có bản. Sinh viên cần hoàn thành trong
                thời gian 60 phút.
              </p>
            </div>
            <button className="xemchitiet-download-btn">
              <Download size={16} />
              Xuất báo cáo
            </button>
          </div>

          {/* Tags */}
          <div className="xemchitiet-tags">
            <span className="xemchitiet-tag xemchitiet-tag-gray">HTML</span>
            <span className="xemchitiet-tag xemchitiet-tag-gray">Firebase</span>
            <span className="xemchitiet-tag xemchitiet-tag-purple">Bài kiểm tra</span>
          </div>

          {/* Stats Section */}
          <div className="xemchitiet-stats">
            {/* Deadline */}
            <div className="xemchitiet-stat-box xemchitiet-stat-box-blue">
              <div className="xemchitiet-stat-header">
                <Clock size={20} className="xemchitiet-stat-icon-blue" />
                <span className="xemchitiet-stat-label xemchitiet-stat-label-blue">Hạn nộp</span>
              </div>
              <p className="xemchitiet-stat-value">23:59 13/11/2025</p>
            </div>

            {/* Time Limit */}
            <div className="xemchitiet-stat-box xemchitiet-stat-box-purple">
              <div className="xemchitiet-stat-header">
                <Clock size={20} className="xemchitiet-stat-icon-purple" />
                <span className="xemchitiet-stat-label xemchitiet-stat-label-purple">
                  Thời gian làm bài
                </span>
              </div>
              <p className="xemchitiet-stat-value">60 phút</p>
            </div>

            {/* Submitted */}
            <div className="xemchitiet-stat-box xemchitiet-stat-box-green">
              <div className="xemchitiet-stat-header">
                <CheckCircle size={20} className="xemchitiet-stat-icon-green" />
                <span className="xemchitiet-stat-label xemchitiet-stat-label-green">Đã nộp</span>
              </div>
              <p className="xemchitiet-stat-value">15/45</p>
            </div>

            {/* Total Questions */}
            <div className="xemchitiet-stat-box xemchitiet-stat-box-orange">
              <div className="xemchitiet-stat-header">
                <AlertCircle size={20} className="xemchitiet-stat-icon-orange" />
                <span className="xemchitiet-stat-label xemchitiet-stat-label-orange">
                  Tổng số câu
                </span>
              </div>
              <p className="xemchitiet-stat-value">20 câu hỏi</p>
            </div>
          </div>

          {/* Max Attempts Info */}
          <div className="xemchitiet-max-attempts">
            <p className="xemchitiet-max-attempts-text">
              <span style={{ fontWeight: 600 }}>Số lần làm tối đa:</span> 2 lần
            </p>
          </div>

          {/* Tabs */}
          <div className="xemchitiet-tabs">
            <button
              onClick={() => setActiveTab("students")}
              className={`xemchitiet-tab-btn ${activeTab === "students" ? "active" : ""}`}
            >
              Danh sách sinh viên
            </button>
            <button
              onClick={() => setActiveTab("questions")}
              className={`xemchitiet-tab-btn ${activeTab === "questions" ? "active" : ""}`}
            >
              Câu hỏi bài tập
            </button>
          </div>

          {/* Tab Content */}
          <div className="xemchitiet-tab-content">
            {activeTab === "students" && (
              <div>
                <h3>Danh sách sinh viên đã nộp bài</h3>

                {/* Table */}
                <div className="xemchitiet-table">
                  <table>
                    <thead>
                      <tr>
                        <th>Mã SV</th>
                        <th>Họ và tên</th>
                        <th>Thời gian nộp</th>
                        <th>Số lần làm</th>
                        <th>Trạng thái</th>
                        <th>Điểm</th>
                        <th>Thao tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      {students.map((student, idx) => (
                        <tr key={idx}>
                          <td className="font-medium">{student.maSv}</td>
                          <td>{student.hoTen}</td>
                          <td>{student.thoiGianNop}</td>
                          <td>{student.soLanLam}</td>
                          <td>
                            <span className="xemchitiet-status-badge">{student.trangThai}</span>
                          </td>
                          <td className="xemchitiet-score">{student.diem}</td>
                          <td>
                            <a
                              href="#"
                              className="xemchitiet-action-link"
                              onClick={(e) => {
                                e.preventDefault()
                                handleViewSubmission(student)
                              }}
                            >
                              Xem bài làm
                            </a>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {activeTab === "questions" && (
              <div>
                <h3>Danh sách câu hỏi bài tập</h3>
                <CauHoiComponent />
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  )
}

export default XemChiTietPage

