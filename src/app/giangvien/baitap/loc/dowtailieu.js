"use client"

import { useState } from "react"
import { Upload, X } from "lucide-react"

export default function DownloadMaterials({ isOpen, onClose }) {
  const [title, setTitle] = useState("")
  const [class_, setClass] = useState("")
  const [documentType, setDocumentType] = useState("")
  const [dragActive, setDragActive] = useState(false)
  const [selectedFile, setSelectedFile] = useState(null)

  const handleDrag = (e) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true)
    } else if (e.type === "dragleave") {
      setDragActive(false)
    }
  }

  const handleDrop = (e) => {
    e.preventDefault()
    e.stopPropagation()
    setDragActive(false)
    
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      const file = e.dataTransfer.files[0]
      const allowedTypes = ['.pdf', '.doc', '.docx', '.ppt', '.pptx']
      const fileExtension = '.' + file.name.split('.').pop().toLowerCase()
      
      if (allowedTypes.includes(fileExtension)) {
        setSelectedFile(file)
      } else {
        alert('Chỉ chấp nhận file: DOC, PDF, SLIDE')
      }
    }
  }

  const handleFileChange = (e) => {
    if (e.target.files && e.target.files.length > 0) {
      const file = e.target.files[0]
      const allowedTypes = ['.pdf', '.doc', '.docx', '.ppt', '.pptx']
      const fileExtension = '.' + file.name.split('.').pop().toLowerCase()
      
      if (allowedTypes.includes(fileExtension)) {
        setSelectedFile(file)
      } else {
        alert('Chỉ chấp nhận file: DOC, PDF, SLIDE')
        e.target.value = ''
      }
    }
  }

  const handleSubmit = (e) => {
    e.preventDefault()
    
    if (!selectedFile) {
      alert('Vui lòng chọn file để tải lên')
      return
    }
    
    // Handle form submission
    console.log("Form submitted:", { title, class_, documentType, file: selectedFile.name })
    
    // Here you would typically upload the file to a server
    // For now, we'll just show a success message
    alert(`Đã tải lên thành công: ${selectedFile.name}`)
    
    // Reset form
    setTitle("")
    setClass("")
    setDocumentType("")
    setSelectedFile(null)
    onClose()
  }

  const handleClose = () => {
    setTitle("")
    setClass("")
    setDocumentType("")
    setSelectedFile(null)
    onClose()
  }

  if (!isOpen) return null

  return (
    <>
      <style>{`
        .dow-overlay {
          position: fixed;
          top: 0;
          left: 0;
          right: 0;
          bottom: 0;
          background: rgba(0, 0, 0, 0.5);
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 9999;
          padding: 1rem;
        }

        .dow-modal {
          background: white;
          border-radius: 0.75rem;
          width: 100%;
          max-width: 500px;
          max-height: 90vh;
          overflow-y: auto;
          position: relative;
          box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
        }

        .dow-header {
          background: linear-gradient(to right, #2563eb, #1d4ed8);
          color: white;
          padding: 1rem 1.5rem;
          display: flex;
          align-items: center;
          justify-content: space-between;
          border-radius: 0.75rem 0.75rem 0 0;
        }

        .dow-header-title {
          font-size: 1.25rem;
          font-weight: 600;
        }

        .dow-close-btn {
          background: none;
          border: none;
          color: white;
          cursor: pointer;
          padding: 0.25rem;
          display: flex;
          align-items: center;
          justify-content: center;
          transition: opacity 0.2s;
        }

        .dow-close-btn:hover {
          opacity: 0.8;
        }

        .dow-form-content {
          padding: 2rem;
        }

        .dow-form-group {
          margin-bottom: 1.5rem;
        }

        .dow-label {
          display: block;
          font-size: 0.875rem;
          font-weight: 600;
          color: #1f2937;
          margin-bottom: 0.5rem;
        }

        .dow-label .required {
          color: #ef4444;
        }

        .dow-input,
        .dow-select,
        .dow-textarea {
          width: 100%;
          padding: 0.75rem;
          border: 1px solid #e5e7eb;
          border-radius: 0.5rem;
          font-size: 0.875rem;
          font-family: inherit;
          color: #1f2937;
          background: white;
        }

        .dow-input:focus,
        .dow-select:focus,
        .dow-textarea:focus {
          outline: none;
          background: white;
          border-color: #3b82f6;
          box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

        .dow-input::placeholder,
        .dow-textarea::placeholder {
          color: #9ca3af;
        }

        .dow-row {
          display: grid;
          grid-template-columns: 1fr 1fr;
          gap: 1rem;
        }

        .dow-textarea {
          resize: vertical;
          min-height: 100px;
        }

        .dow-upload-area {
          border: 2px dashed #d1d5db;
          border-radius: 0.5rem;
          padding: 3rem 2rem;
          text-align: center;
          cursor: pointer;
          transition: all 0.2s;
          background: white;
        }

        .dow-upload-area.drag-active {
          border-color: #3b82f6;
          background: #eff6ff;
        }

        .dow-upload-icon {
          color: #3b82f6;
          margin-bottom: 1rem;
          display: flex;
          justify-content: center;
        }

        .dow-upload-text {
          font-size: 0.875rem;
          color: #3b82f6;
          margin-bottom: 0.5rem;
          font-weight: 500;
        }

        .dow-upload-hint {
          font-size: 0.75rem;
          color: #6b7280;
        }

        .dow-buttons {
          display: flex;
          gap: 1rem;
          margin-top: 2rem;
          justify-content: flex-end;
        }

        .dow-btn {
          padding: 0.75rem 1.5rem;
          border-radius: 0.5rem;
          font-weight: 600;
          font-size: 0.875rem;
          cursor: pointer;
          transition: all 0.2s;
          border: 1px solid #d1d5db;
        }

        .dow-btn-cancel {
          background: white;
          color: #374151;
          border-color: #d1d5db;
        }

        .dow-btn-cancel:hover {
          background: #f9fafb;
        }

        .dow-btn-submit {
          background: #3b82f6;
          color: white;
          border: none;
          display: flex;
          align-items: center;
          gap: 0.5rem;
        }

        .dow-btn-submit:hover {
          background: #2563eb;
        }
      `}</style>

      <div className="dow-overlay" onClick={handleClose}>
        <div className="dow-modal" onClick={(e) => e.stopPropagation()}>
          {/* Header */}
          <div className="dow-header">
            <div className="dow-header-title">Upload Tài liệu</div>
            <button className="dow-close-btn" onClick={handleClose}>
              <X size={20} />
            </button>
          </div>

          {/* Form */}
          <div className="dow-form-content">
            <form onSubmit={handleSubmit}>
              {/* Tên tài liệu */}
              <div className="dow-form-group">
                <label className="dow-label">
                  Tên tài liệu<span className="required">*</span>
                </label>
                <input
                  type="text"
                  className="dow-input"
                  placeholder="Nhập tên tài liệu..."
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  required
                />
              </div>

              {/* Lớp */}
              <div className="dow-form-group">
                <label className="dow-label">
                  Lớp<span className="required">*</span>
                </label>
                <select
                  className="dow-select"
                  value={class_}
                  onChange={(e) => setClass(e.target.value)}
                  required
                >
                  <option value="">Chọn lớp</option>
                  <option value="Lập trình Web">Lập trình Web</option>
                  <option value="Lập trình hướng đối tượng">Lập trình hướng đối tượng</option>
                  <option value="Lập trình Mobile">Lập trình Mobile</option>
                  <option value="Database">Database</option>
                </select>
              </div>

              {/* Loại tài liệu */}
              <div className="dow-form-group">
                <label className="dow-label">
                  Loại tài liệu<span className="required">*</span>
                </label>
                <select
                  className="dow-select"
                  value={documentType}
                  onChange={(e) => setDocumentType(e.target.value)}
                  required
                >
                  <option value="">Chọn loại tài liệu</option>
                  <option value="File DOC">File DOC</option>
                  <option value="File SLIDE">File SLIDE</option>
                  <option value="File PDF">File PDF</option>
                </select>
              </div>

              {/* Upload Area */}
              <div className="dow-form-group">
                <label className="dow-label">
                  Chọn file<span className="required">*</span>
                </label>
                <div
                  className={`dow-upload-area ${dragActive ? "drag-active" : ""}`}
                  onDragEnter={handleDrag}
                  onDragLeave={handleDrag}
                  onDragOver={handleDrag}
                  onDrop={handleDrop}
                  onClick={() => document.getElementById("dow-file-input").click()}
                >
                  <input
                    id="dow-file-input"
                    type="file"
                    style={{ display: "none" }}
                    accept=".pdf,.doc,.docx,.ppt,.pptx"
                    onChange={handleFileChange}
                  />
                  <div className="dow-upload-icon">
                    <Upload size={48} />
                  </div>
                  <div className="dow-upload-text">Chọn file hoặc kéo thả file vào đây</div>
                  <div className="dow-upload-hint">Hỗ trợ: DOC, PDF, SLIDE</div>
                  {selectedFile && (
                    <div className="mt-2 text-sm text-gray-700 font-medium">
                      Đã chọn: {selectedFile.name}
                    </div>
                  )}
                </div>
              </div>

              <div className="dow-buttons">
                <button type="button" className="dow-btn dow-btn-cancel" onClick={handleClose}>
                  Hủy
                </button>
                <button type="submit" className="dow-btn dow-btn-submit">
                  <Upload size={18} />
                  Upload
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  )
}
