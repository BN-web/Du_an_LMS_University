using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class File
{
    public int FilesId { get; set; }

    public int? BaiHocId { get; set; }

    public string? TenFile { get; set; }

    public string? DuongDan { get; set; }

    public long? KichThuoc { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual BaiHoc? BaiHoc { get; set; }
}
