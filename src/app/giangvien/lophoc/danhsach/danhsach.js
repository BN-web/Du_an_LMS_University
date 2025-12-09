"use client"

import { useState } from "react"

import { Search, Eye } from "lucide-react"

export default function DanhSachSV({ classData, onSelectStudent }) {

  const [searchTerm, setSearchTerm] = useState("")

  const filteredStudents = classData.students.filter(

    (student) =>

      student.name.toLowerCase().includes(searchTerm.toLowerCase()) ||

      student.mssv.toLowerCase().includes(searchTerm.toLowerCase()) ||

      student.email.toLowerCase().includes(searchTerm.toLowerCase()),

  )

  return (

    <div className="space-y-6">

      {/* Search Bar */}

      <div>

        <div className="relative max-w-md">

          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />

          <input

            type="text"

            placeholder="Tìm kiếm sinh viên...."

            value={searchTerm}

            onChange={(e) => setSearchTerm(e.target.value)}

            className="w-full pl-10 pr-4 py-2 rounded-lg bg-gray-200 text-gray-700 placeholder-gray-500 focus:outline-none"

          />

        </div>

      </div>

      {/* Students Table */}

      <div className="bg-white rounded-lg overflow-hidden">

        <table className="w-full">

          <thead>

            <tr className="bg-gray-50 border-b border-gray-200">

              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">MSSV</th>

              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">HỌ TÊN</th>

              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">NGÀNH</th>

              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">TỔNG ĐIỂM</th>

              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">TRẠNG THÁI</th>

              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">THAO TÁC</th>

            </tr>

          </thead>

          <tbody className="divide-y divide-gray-200">

            {filteredStudents.map((student) => (

              <tr key={student.mssv} className="hover:bg-gray-50 transition-colors">

                <td className="px-6 py-4 text-sm text-gray-600">{student.mssv}</td>

                <td className="px-6 py-4 text-sm text-gray-800 font-medium">{student.name}</td>

                <td className="px-6 py-4 text-sm text-gray-600">{student.major}</td>

                <td className="px-6 py-4 text-sm text-gray-800 font-medium">{student.grade}</td>

                <td className="px-6 py-4 text-sm">

                  <span

                    className={`px-3 py-1 rounded-full text-sm font-medium ${

                      student.status === "Đang học"

                        ? "bg-green-100 text-green-700"

                        : student.status === "Đã thôi học"

                          ? "bg-red-100 text-red-700"

                          : "bg-red-100 text-red-700"

                    }`}

                  >

                    {student.status}

                  </span>

                </td>

                <td className="px-6 py-4 text-sm">

                  <button

                    onClick={() => onSelectStudent(student)}

                    className="p-2 hover:bg-gray-100 rounded-lg transition-colors"

                    title="Xem chi tiết"

                  >

                    <Eye size={20} className="text-gray-600" />

                  </button>

                </td>

              </tr>

            ))}

          </tbody>

        </table>

      </div>

    </div>

  )

}




