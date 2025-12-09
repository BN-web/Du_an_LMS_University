"use client"

const questions = [
  {
    id: 1,
    title: "HTML là viết tắt của gì?",
    description: "",
    answers: [
      { key: "A", text: "Hypertext Markup Language", isCorrect: true },
      { key: "B", text: "High Text Machine Language", isCorrect: false },
      { key: "C", text: "Hyper Text Machine Language", isCorrect: false },
      { key: "D", text: "Hypertext Markup Language", isCorrect: false },
    ],
  },
  {
    id: 2,
    title: "CSS được sử dụng để làm gì?",
    description: "",
    answers: [
      { key: "A", text: "Tạo cấu trúc trang web", isCorrect: false },
      { key: "B", text: "Tạo logic cho trang web", isCorrect: false },
      { key: "C", text: "Tạo kiểu dáng cho trang web", isCorrect: true },
      { key: "D", text: "Quản lý cơ sở dữ liệu", isCorrect: false },
    ],
  },
  {
    id: 3,
    title: "Firebase là gì?",
    description: "",
    answers: [
      { key: "A", text: "Một ngôn ngữ lập trình", isCorrect: false },
      { key: "B", text: "Một nền tảng phát triển ứng dụng", isCorrect: true },
      { key: "C", text: "Một trình biên dịch web", isCorrect: false },
      { key: "D", text: "Một tiêu chuẩn web mới", isCorrect: false },
    ],
  },
]

export default function CauHoiComponent({ onBack }) {
  return (
    <>
      <style>{`
        .cauhoi-container {
          display: flex;
          flex-direction: column;
          gap: 1rem;
        }

        .cauhoi-header {
          margin-bottom: 1rem;
        }

        .cauhoi-header-title {
          color: #1f2937;
          font-weight: 600;
          font-size: 0.875rem;
        }

        .cauhoi-header-info {
          color: #6b7280;
          font-size: 0.75rem;
        }


        .cauhoi-questions-list {
          display: flex;
          flex-direction: column;
          gap: 1.5rem;
        }

        .cauhoi-question {
          background: white;
          border: 1px solid #e5e7eb;
          border-radius: 0.5rem;
          padding: 1.5rem;
        }

        .cauhoi-question-number {
          font-weight: 600;
          color: #1f2937;
          margin-bottom: 1rem;
          font-size: 1rem;
        }

        .cauhoi-answers {
          display: flex;
          flex-direction: column;
          gap: 0.75rem;
        }

        .cauhoi-answer {
          display: flex;
          align-items: flex-start;
          gap: 0.75rem;
          padding: 0.75rem;
          border-radius: 0.375rem;
          cursor: pointer;
          transition: background 0.2s;
        }

        .cauhoi-answer:hover {
          background: #f9fafb;
        }

        .cauhoi-answer-key {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          width: 1.5rem;
          height: 1.5rem;
          border: 2px solid #d1d5db;
          border-radius: 0.25rem;
          font-weight: 600;
          font-size: 0.875rem;
          color: #374151;
          flex-shrink: 0;
        }

        .cauhoi-answer.correct .cauhoi-answer-key {
          background: #10b981;
          border-color: #10b981;
          color: white;
        }

        .cauhoi-answer.correct {
          background: #f0fdf4;
        }

        .cauhoi-answer-text {
          flex: 1;
          color: #374151;
          font-size: 0.875rem;
        }

        .cauhoi-answer-check {
          width: 1.25rem;
          height: 1.25rem;
          color: #10b981;
          flex-shrink: 0;
        }

        .cauhoi-answer-check svg {
          width: 100%;
          height: 100%;
        }

        .cauhoi-back-btn {
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

        .cauhoi-back-btn:hover {
          color: #1d4ed8;
        }

        @media (max-width: 768px) {
          .cauhoi-header {
            flex-direction: column;
            gap: 1rem;
            align-items: flex-start;
          }

          .cauhoi-question {
            padding: 1rem;
          }
        }
      `}</style>

      {onBack && (
        <button onClick={onBack} className="cauhoi-back-btn">
          ← Quay lại
        </button>
      )}

      <div className="cauhoi-container">
        <div className="cauhoi-header">
          <div>
            <h3 className="cauhoi-header-title">Câu hỏi bài tập</h3>
            <p className="cauhoi-header-info">Tổng số: {questions.length} câu hỏi</p>
          </div>
        </div>

        <div className="cauhoi-questions-list">
          {questions.map((question) => (
            <div key={question.id} className="cauhoi-question">
              <div className="cauhoi-question-number">
                Câu {question.id}: {question.title}
              </div>

              <div className="cauhoi-answers">
                {question.answers.map((answer) => (
                  <div
                    key={answer.key}
                    className={`cauhoi-answer ${answer.isCorrect ? "correct" : ""}`}
                  >
                    <div className="cauhoi-answer-key">{answer.key}</div>
                    <span className="cauhoi-answer-text">{answer.text}</span>
                    {answer.isCorrect && (
                      <svg
                        className="cauhoi-answer-check"
                        viewBox="0 0 20 20"
                        fill="currentColor"
                      >
                        <path
                          fillRule="evenodd"
                          d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                          clipRule="evenodd"
                        />
                      </svg>
                    )}
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  )
}
