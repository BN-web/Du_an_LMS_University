"use client"

import { useState } from "react"
import { Download, Edit, Trash2, Folder, AlertCircle, Search } from "lucide-react"
import DownloadMaterials from "./dowtailieu"
import TaoBaiTap from "./taobaitap"

const documentGroups = [
  {
    id: "1",
    title: "Lập trình Web",
    count: 2,
    documents: [
      {
        id: "1",
        name: "Hướng dẫn sử dụng Firebase",
        category: "Tài liệu tham khảo",
        categoryColor: "bg-blue-100 text-blue-700",
        fileType: "DOCX",
        uploadDate: "10/11/2025",
        size: "1.2 MB",
      },
      {
        id: "2",
        name: "Slide bài giảng JavaScript",
        category: "Bài giảng",
        categoryColor: "bg-pink-100 text-pink-700",
        fileType: "SLIDE",
        uploadDate: "05/11/2025",
        size: "5.8 MB",
      },
    ],
  },
  {
    id: "2",
    title: "Lập trình hướng đối tượng",
    count: 3,
    documents: [
      {
        id: "3",
        name: "Slide bài giảng JavaScript",
        category: "Bài giảng",
        categoryColor: "bg-pink-100 text-pink-700",
        fileType: "SLIDE",
        uploadDate: "05/11/2025",
        size: "5.8 MB",
      },
      {
        id: "4",
        name: "Bài giảng OOP trong Java",
        category: "Bài giảng",
        categoryColor: "bg-pink-100 text-pink-700",
        fileType: "PDF",
        uploadDate: "20/11/2025",
        size: "3.2 MB",
      },
      {
        id: "5",
        name: "Bài tập thực hành Java",
        category: "Bài tập",
        categoryColor: "bg-green-100 text-green-700",
        fileType: "DOCX",
        uploadDate: "18/11/2025",
        size: "0.8 MB",
      },
    ],
  },
]

export default function TaiLieuPage({ onBackToBaitap }) {
  const [documents, setDocuments] = useState(documentGroups)
  const [showDownloadModal, setShowDownloadModal] = useState(false)
  const [notification, setNotification] = useState(null)
  const [deleteDialog, setDeleteDialog] = useState(null)
  const [editingDoc, setEditingDoc] = useState(null)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [searchTerm, setSearchTerm] = useState("")

  const handleDownload = (docId, docName) => {
    setNotification({ type: "success", message: `Đang tải: ${docName}` })
    setTimeout(() => setNotification(null), 2000)
  }

  const handleEdit = (docId, docName) => {
    setEditingDoc({ show: true, docId, docName })
  }

  const handleDeleteClick = (docId, docName) => {
    setDeleteDialog({ show: true, docId, docName })
  }

  const handleConfirmDelete = () => {
    if (!deleteDialog) return

    const newDocuments = documents.map((group) => ({
      ...group,
      documents: group.documents.filter((doc) => doc.id !== deleteDialog.docId),
      count: group.documents.filter((doc) => doc.id !== deleteDialog.docId).length,
    }))

    setDocuments(newDocuments)
    setNotification({ type: "success", message: `Đã xóa: ${deleteDialog.docName}` })
    setDeleteDialog(null)
    setTimeout(() => setNotification(null), 2000)
  }

  const handleEditSave = () => {
    if (!editingDoc) return

    setNotification({ type: "success", message: `Đã cập nhật: ${editingDoc.docName}` })
    setEditingDoc(null)
    setTimeout(() => setNotification(null), 2000)
  }

  // Filter documents based on search term
  const filteredDocuments = documents.map((group) => ({
    ...group,
    documents: group.documents.filter(
      (doc) =>
        doc.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        doc.fileType.toLowerCase().includes(searchTerm.toLowerCase())
    ),
  }))

  return (
    <>
      <style>{`
        .tailieu-container {
          flex: 1;
          overflow: auto;
          background: linear-gradient(to bottom right, #dbeafe, #eff6ff, #dbeafe);
          padding: 1rem;
        }

        @media (min-width: 768px) {
          .tailieu-container {
            padding: 2rem;
          }
        }

        .tailieu-header {
          margin-bottom: 2rem;
        }

        .tailieu-title {
          font-size: 1.875rem;
          font-weight: bold;
          color: #1f2937;
          margin-bottom: 1.5rem;
        }

        @media (min-width: 768px) {
          .tailieu-title {
            font-size: 2.25rem;
          }
        }

        .tailieu-search-filters {
          display: flex;
          flex-direction: column;
          gap: 1rem;
          flex-wrap: wrap;
        }

        @media (min-width: 768px) {
          .tailieu-search-filters {
            flex-direction: row;
            align-items: center;
            gap: 0.75rem;
          }
        }

        .tailieu-search-wrapper {
          position: relative;
          flex: 1;
        }

        @media (min-width: 768px) {
          .tailieu-search-wrapper {
            min-width: 320px;
          }
        }

        .tailieu-search-icon {
          position: absolute;
          left: 0.75rem;
          top: 50%;
          transform: translateY(-50%);
          color: #9ca3af;
          pointer-events: none;
        }

        .tailieu-search-input {
          width: 100%;
          padding: 0.5rem 1rem 0.5rem 2.5rem;
          border-radius: 0.5rem;
          border: 1px solid #d1d5db;
          background: white;
          color: #1f2937;
          font-size: 1rem;
        }

        .tailieu-search-input::placeholder {
          color: #9ca3af;
        }

        .tailieu-search-input:focus {
          outline: none;
          ring: 2px;
          ring-color: #60a5fa;
        }

        .tailieu-content {
          display: flex;
          flex-direction: column;
          gap: 2rem;
        }

        .tailieu-group {
          border-radius: 0.75rem;
          overflow: hidden;
          box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
        }

        .tailieu-group-header {
          background: linear-gradient(to right, #2563eb, #1d4ed8);
          color: white;
          padding: 1rem 1.5rem;
          display: flex;
          align-items: center;
          justify-content: space-between;
          font-weight: 600;
          font-size: 1.125rem;
        }

        .tailieu-group-header-left {
          display: flex;
          align-items: center;
          gap: 0.75rem;
        }

        .tailieu-group-count {
          background: rgba(255, 255, 255, 0.2);
          padding: 0.25rem 0.75rem;
          border-radius: 0.25rem;
          font-size: 0.875rem;
        }

        .tailieu-group-body {
          background: white;
          overflow-x: auto;
        }

        .tailieu-table {
          width: 100%;
          border-collapse: collapse;
          font-size: 0.875rem;
        }

        .tailieu-table thead {
          background: #f9fafb;
          border-bottom: 1px solid #e5e7eb;
        }

        .tailieu-table th {
          padding: 1rem;
          text-align: left;
          font-weight: 600;
          color: #374151;
          text-transform: uppercase;
          font-size: 0.75rem;
          letter-spacing: 0.05em;
        }

        .tailieu-table td {
          padding: 1rem;
          border-bottom: 1px solid #e5e7eb;
          color: #374151;
        }

        .tailieu-table tbody tr:hover {
          background: #f9fafb;
        }

        .tailieu-doc-name {
          display: flex;
          align-items: center;
          gap: 0.75rem;
          color: #1f2937;
          font-weight: 500;
        }

        .tailieu-category-tag {
          display: inline-block;
          padding: 0.25rem 0.75rem;
          border-radius: 0.25rem;
          font-size: 0.75rem;
          font-weight: 600;
        }

        .tailieu-file-type {
          display: inline-block;
          padding: 0.25rem 0.75rem;
          border-radius: 0.25rem;
          font-weight: 600;
          font-size: 0.75rem;
        }

        .tailieu-file-type-docx {
          background: #dbeafe;
          color: #0284c7;
        }

        .tailieu-file-type-pdf {
          background: #fecaca;
          color: #dc2626;
        }

        .tailieu-file-type-slide {
          background: #fed7aa;
          color: #ea580c;
        }

        .tailieu-actions {
          display: flex;
          gap: 0.75rem;
          align-items: center;
        }

        .tailieu-action-btn {
          background: none;
          border: none;
          cursor: pointer;
          color: #6b7280;
          transition: color 0.2s;
          padding: 0.25rem;
          display: flex;
          align-items: center;
          justify-content: center;
        }

        .tailieu-action-btn:hover {
          color: #374151;
        }

        .tailieu-action-btn.delete:hover {
          color: #dc2626;
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
          z-index: 50;
        }

        .modal-content {
          background: white;
          border-radius: 0.75rem;
          padding: 2rem;
          max-width: 400px;
          width: 90%;
          box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
        }

        .modal-header {
          display: flex;
          align-items: center;
          gap: 1rem;
          margin-bottom: 1rem;
        }

        .modal-title {
          font-size: 1.25rem;
          font-weight: 600;
          color: #1f2937;
        }

        .modal-body {
          color: #4b5563;
          margin-bottom: 1.5rem;
          line-height: 1.5;
        }

        .modal-footer {
          display: flex;
          gap: 0.75rem;
          justify-content: flex-end;
        }

        .modal-btn {
          padding: 0.5rem 1.5rem;
          border-radius: 0.5rem;
          border: none;
          cursor: pointer;
          font-weight: 500;
          transition: all 0.2s;
        }

        .modal-btn-cancel {
          background: #e5e7eb;
          color: #374151;
        }

        .modal-btn-cancel:hover {
          background: #d1d5db;
        }

        .modal-btn-confirm {
          background: #ef4444;
          color: white;
        }

        .modal-btn-confirm:hover {
          background: #dc2626;
        }

        .modal-btn-save {
          background: #3b82f6;
          color: white;
        }

        .modal-btn-save:hover {
          background: #2563eb;
        }

        .notification {
          position: fixed;
          bottom: 2rem;
          right: 2rem;
          padding: 1rem 1.5rem;
          border-radius: 0.5rem;
          display: flex;
          align-items: center;
          gap: 0.75rem;
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
          z-index: 40;
          animation: slideIn 0.3s ease-out;
        }

        @keyframes slideIn {
          from {
            transform: translateX(400px);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }

        .notification-success {
          background: #dcfce7;
          color: #166534;
        }

        .notification-error {
          background: #fee2e2;
          color: #991b1b;
        }

        .notification-info {
          background: #dbeafe;
          color: #0c4a6e;
        }

        .modal-input {
          width: 100%;
          padding: 0.75rem;
          border: 1px solid #d1d5db;
          border-radius: 0.5rem;
          font-size: 1rem;
          margin-bottom: 1rem;
        }

        .modal-input:focus {
          outline: none;
          border-color: #3b82f6;
          box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }
      `}</style>

      {notification && (
        <div className={`notification notification-${notification.type}`}>{notification.message}</div>
      )}

      {deleteDialog?.show && (
        <div className="modal-overlay" onClick={() => setDeleteDialog(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <AlertCircle size={24} style={{ color: "#ef4444" }} />
              <div className="modal-title">Xác nhận xóa</div>
            </div>
            <div className="modal-body">
              Bạn có chắc chắn muốn xóa tài liệu "{deleteDialog.docName}"? Hành động này không thể
              hoàn tác.
            </div>
            <div className="modal-footer">
              <button
                className="modal-btn modal-btn-cancel"
                onClick={() => setDeleteDialog(null)}
              >
                Hủy
              </button>
              <button className="modal-btn modal-btn-confirm" onClick={handleConfirmDelete}>
                Xóa
              </button>
            </div>
          </div>
        </div>
      )}

      {editingDoc?.show && (
        <div className="modal-overlay" onClick={() => setEditingDoc(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <Edit size={24} style={{ color: "#3b82f6" }} />
              <div className="modal-title">Chỉnh sửa tài liệu</div>
            </div>
            <div className="modal-body">
              <input
                type="text"
                className="modal-input"
                defaultValue={editingDoc.docName}
                placeholder="Tên tài liệu"
              />
            </div>
            <div className="modal-footer">
              <button className="modal-btn modal-btn-cancel" onClick={() => setEditingDoc(null)}>
                Hủy
              </button>
              <button className="modal-btn modal-btn-save" onClick={handleEditSave}>
                Lưu
              </button>
            </div>
          </div>
        </div>
      )}

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
            <TaoBaiTap onClose={() => setShowCreateForm(false)} />
          </div>
        </div>
      )}

      <DownloadMaterials isOpen={showDownloadModal} onClose={() => setShowDownloadModal(false)} />

      <div>
        {/* Documents Content */}
        <div className="tailieu-content">
          {filteredDocuments.map((group) => {
            if (group.documents.length === 0) return null
            return (
              <div key={group.id} className="tailieu-group">
                {/* Group Header */}
                <div className="tailieu-group-header">
                  <div className="tailieu-group-header-left">
                    <Folder size={24} />
                    <span>{group.title}</span>
                  </div>
                  <span className="tailieu-group-count">{group.documents.length} tài liệu</span>
                </div>

                {/* Group Body */}
                <div className="tailieu-group-body">
                  <table className="tailieu-table">
                    <thead>
                      <tr>
                        <th>TÊN TÀI LIỆU</th>
                        <th>LOẠI TÀI LIỆU</th>
                        <th>NGÀY UPLOAD</th>
                        <th>KÍCH THƯỚC</th>
                        <th>THAO TÁC</th>
                      </tr>
                    </thead>
                    <tbody>
                      {group.documents.map((doc) => (
                        <tr key={doc.id}>
                          <td>
                            <div className="tailieu-doc-name">
                              <Folder size={18} className="text-blue-500" />
                              <span>{doc.name}</span>
                            </div>
                          </td>
                          <td>
                            <span
                              className={`tailieu-file-type tailieu-file-type-${doc.fileType.toLowerCase()}`}
                            >
                              {doc.fileType}
                            </span>
                          </td>
                          <td>{doc.uploadDate}</td>
                          <td>{doc.size}</td>
                          <td>
                            <div className="tailieu-actions">
                              <button
                                className="tailieu-action-btn"
                                title="Download"
                                onClick={() => handleDownload(doc.id, doc.name)}
                              >
                                <Download size={18} />
                              </button>
                              <button
                                className="tailieu-action-btn"
                                title="Edit"
                                onClick={() => handleEdit(doc.id, doc.name)}
                              >
                                <Edit size={18} />
                              </button>
                              <button
                                className="tailieu-action-btn delete"
                                title="Delete"
                                onClick={() => handleDeleteClick(doc.id, doc.name)}
                              >
                                <Trash2 size={18} />
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )
          })}
        </div>
      </div>
    </>
  )
}

