using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmcsdl.Model
{
    public class NhanVien
    {
        public string manv { get; set; }
        public string HoTen { get; set; }
        public string email { get; set; }
        public string luong { get; set; }
        public byte[] luongEnc { get; set; }
    }
}
