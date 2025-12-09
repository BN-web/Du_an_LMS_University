"use client"

import { useState } from "react"
import { ArrowLeft } from "lucide-react"

const XemBaiLamPage = ({ onBack, studentId }) => {
  const [submission] = useState({
    maSv: "SV001",
    hoTen: "Nguyễn Văn An",
    thoiGianNop: "10:30 12/11/2025",
    tongCau: 5,
    cauDung: 4,
    doChinhXac: 80,
    diemTong: 8.5,
    diemToiDa: 10,
    questions: [
      {
        id: 1,
        cauHoi: "HTML là viết tắt của gì?",
        loai: "Trắc nghiệm",
        cauTraLoiSV: "HyperText Markup Language",
        cauTraLoiDung: "HyperText Markup Language",
        diem: 0.5,
        diem_toi_da: 0.5,
      },
      {
        id: 2,
        cauHoi: "Thẻ nào được sử dụng để tạo liên kết trong HTML?",
        loai: "Trắc nghiệm",
        cauTraLoiSV: "<a>",
        cauTraLoiDung: "<a>",
        diem: 0.5,
        diem_toi_da: 0.5,
      },
      {
        id: 3,
        cauHoi: "CSS được sử dụng để làm gì?",
        loai: "Trắc nghiệm",
        cauTraLoiSV: "Tạo logic cho trang web",
        cauTraLoiDung: "Tạo kiểu dáng cho trang web",
        diem: 0,
        diem_toi_da: 0.5,
      },
      {
        id: 4,
        cauHoi: "Firebase là gì?",
        loai: "Trắc nghiệm",
        cauTraLoiSV: "Một nền tảng phát triển ứng dụng",
        cauTraLoiDung: "Một nền tảng phát triển ứng dụng",
        diem: 0.5,
        diem_toi_da: 0.5,
      },
      {
        id: 5,
        cauHoi: "Giải thích về DOM trong JavaScript?",
        loai: "Trắc nghiệm",
        cauTraLoiSV: "Mô hình đối tượng của tài liệu",
        cauTraLoiDung: "Mô hình đối tượng của tài liệu",
        diem: 0.5,
        diem_toi_da: 0.5,
      },
    ],
  })

  return (
    <>
      <style>{`
        .xembailam-container {
          flex: 1;
          overflow: auto;
          background: linear-gradient(to bottom right, #dbeafe, #eff6ff, #dbeafe);
          padding: 1rem;
        }

        @media (min-width: 768px) {
          .xembailam-container {
            padding: 2rem;
          }
        }

        .xembailam-back-btn {
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

        .xembailam-back-btn:hover {
          color: #1d4ed8;
        }

        .xembailam-box {
          background: white;
          border-radius: 0.75rem;
          padding: 1.5rem;
          box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }

        @media (min-width: 768px) {
          .xembailam-box {
            padding: 2rem;
          }
        }

        .xembailam-header {
          display: flex;
          flex-direction: column;
          gap: 1rem;
          margin-bottom: 1.5rem;
        }

        @media (min-width: 768px) {
          .xembailam-header {
            flex-direction: row;
            align-items: center;
            justify-content: space-between;
          }
        }

        .xembailam-title-section h1 {
          font-size: 1.5rem;
          font-weight: bold;
          color: #1f2937;
          margin-bottom: 0.5rem;
        }

        .xembailam-student-info {
          color: #4b5563;
          font-size: 0.875rem;
          display: flex;
          gap: 1rem;
          align-items: center;
        }

        .xembailam-score-badge {
          background: #f0fdf4;
          color: #16a34a;
          font-weight: bold;
          font-size: 1.5rem;
          padding: 0.75rem 1.5rem;
          border-radius: 0.5rem;
          text-align: center;
        }

        .xembailam-score-label {
          font-size: 0.75rem;
          color: #4b5563;
        }

        .xembailam-stats {
          display: grid;
          grid-template-columns: repeat(3, 1fr);
          gap: 1rem;
          margin-bottom: 1.5rem;
        }

        @media (min-width: 768px) {
          .xembailam-stats {
            grid-template-columns: repeat(3, 1fr);
            gap: 2rem;
          }
        }

        .xembailam-stat-card {
          text-align: center;
          padding: 1rem;
          border-radius: 0.5rem;
          background: #f9fafb;
          border: 1px solid #e5e7eb;
        }

        .xembailam-stat-value {
          font-size: 1.5rem;
          font-weight: bold;
          color: #1f2937;
        }

        .xembailam-stat-label {
          font-size: 0.75rem;
          color: #4b5563;
          margin-top: 0.25rem;
        }

        .xembailam-questions {
          margin-top: 2rem;
        }

        .xembailam-questions-title {
          font-size: 1rem;
          font-weight: 600;
          color: #1f2937;
          margin-bottom: 1rem;
        }

        .xembailam-question-item {
          border: 1px solid #e5e7eb;
          border-radius: 0.5rem;
          padding: 1.5rem;
          margin-bottom: 1rem;
          background: #f9fafb;
        }

        .xembailam-question-header {
          display: flex;
          align-items: flex-start;
          gap: 1rem;
          margin-bottom: 1rem;
        }

        .xembailam-question-number {
          font-weight: bold;
          color: #1f2937;
          flex-shrink: 0;
        }

        .xembailam-question-meta {
          display: flex;
          gap: 1rem;
          font-size: 0.75rem;
          color: #4b5563;
        }

        .xembailam-question-text {
          font-weight: 600;
          color: #1f2937;
          margin-bottom: 0.75rem;
        }

        .xembailam-answer-section {
          margin-top: 1rem;
          padding-top: 1rem;
          border-top: 1px solid #e5e7eb;
        }

        .xembailam-answer-label {
          font-size: 0.75rem;
          color: #4b5563;
          font-weight: 600;
          margin-bottom: 0.5rem;
        }

        .xembailam-answer-box {
          background: #f0fdf4;
          border: 1px solid #22c55e;
          border-radius: 0.375rem;
          padding: 0.75rem;
          color: #16a34a;
          display: flex;
          align-items: center;
          gap: 0.5rem;
        }

        .xembailam-answer-box.incorrect {
          background: #fee2e2;
          border-color: #ef4444;
          color: #dc2626;
        }

        .xembailam-checkmark {
          color: #16a34a;
          font-weight: bold;
        }

        .xembailam-correct-answer {
          margin-top: 0.75rem;
          padding: 0.75rem;
          background: #f0fdf4;
          border-radius: 0.375rem;
          font-size: 0.875rem;
        }

        .xembailam-correct-answer-label {
          font-weight: 600;
          color: #16a34a;
          margin-bottom: 0.25rem;
        }

        .xembailam-correct-answer-text {
          color: #15803d;
        }

        .xembailam-score-display {
          font-size: 0.875rem;
          color: #4b5563;
          display: flex;
          align-items: center;
          gap: 1rem;
          margin-top: 0.75rem;
          justify-content: flex-end;
        }

        .xembailam-score-display-value {
          color: #16a34a;
          font-weight: 600;
        }
      `}</style>

      <div className="xembailam-container">
        {onBack && (
          <button onClick={onBack} className="xembailam-back-btn">
            <ArrowLeft size={20} />
            Quay lại danh sách
          </button>
        )}

        <div className="xembailam-box">
          {/* Header */}
          <div className="xembailam-header">
            <div className="xembailam-title-section">
              <h1>Bài làm của sinh viên</h1>
              <div className="xembailam-student-info">
                <span>{submission.hoTen}</span>
                <span>({submission.maSv})</span>
                <span>•</span>
                <span>Nộp lúc {submission.thoiGianNop}</span>
              </div>
            </div>
            <div className="xembailam-score-badge">
              {submission.diemTong}/{submission.diemToiDa}
              <div className="xembailam-score-label">Điểm số</div>
            </div>
          </div>

          {/* Stats */}
          <div className="xembailam-stats">
            <div className="xembailam-stat-card">
              <div className="xembailam-stat-value">{submission.tongCau}</div>
              <div className="xembailam-stat-label">Tổng số câu</div>
            </div>
            <div className="xembailam-stat-card">
              <div className="xembailam-stat-value">{submission.cauDung}</div>
              <div className="xembailam-stat-label">Câu trả lời đúng</div>
            </div>
            <div className="xembailam-stat-card">
              <div className="xembailam-stat-value">{submission.doChinhXac}%</div>
              <div className="xembailam-stat-label">Độ chính xác</div>
            </div>
          </div>

          {/* Questions Detail */}
          <div className="xembailam-questions">
            <h2 className="xembailam-questions-title">Chi tiết câu trả lời</h2>

            {submission.questions.map((question) => (
              <div key={question.id} className="xembailam-question-item">
                <div className="xembailam-question-header">
                  <span className="xembailam-question-number">Câu {question.id}</span>
                  <div className="xembailam-question-meta">
                    <span>{question.loai}</span>
                  </div>
                  {question.diem === question.diem_toi_da && (
                    <span style={{ color: "#16a34a", fontWeight: 600 }}>
                      ✓ {question.diem}/{question.diem_toi_da} điểm
                    </span>
                  )}
                  {question.diem < question.diem_toi_da && (
                    <span style={{ color: "#dc2626", fontWeight: 600 }}>
                      ✗ {question.diem}/{question.diem_toi_da} điểm
                    </span>
                  )}
                </div>

                <p className="xembailam-question-text">{question.cauHoi}</p>

                <div className="xembailam-answer-section">
                  <p className="xembailam-answer-label">Câu trả lời của sinh viên:</p>
                  <div
                    className={`xembailam-answer-box ${
                      question.diem < question.diem_toi_da ? "incorrect" : ""
                    }`}
                  >
                    {question.diem === question.diem_toi_da && (
                      <span className="xembailam-checkmark">✓</span>
                    )}
                    {question.cauTraLoiSV}
                  </div>

                  {question.diem < question.diem_toi_da && (
                    <div className="xembailam-correct-answer">
                      <div className="xembailam-correct-answer-label">Câu trả lời đúng:</div>
                      <div className="xembailam-correct-answer-text">{question.cauTraLoiDung}</div>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </>
  )
}

export default XemBaiLamPage
