using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.Unicode;
using System.Windows;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace bmcsdl.Model
{
    public class dataBase
    {
        private static SqlConnection con = new SqlConnection(@"Data Source=TIENHOANGG;Initial Catalog=QLSVNhom;User ID=lab4;Password=123456789;");
        private static dataBase single;
        private static string token { get; set; }
        private RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(512);
        private dataBase() { }
        public static dataBase get_dataBase()
        {
            if (single == null)
            {
                single = new dataBase();
            }
            return single;
        }
        private void open()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
        }
        private void close()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }
        public bool login(string username, string password)
        {
            byte[] inputPass = Encoding.ASCII.GetBytes(password);
            SHA1 sha1 = SHA1.Create();
            byte[] hashPassSHA1 = sha1.ComputeHash(inputPass);
            SqlCommand sqlcmd = new SqlCommand("select dbo.DANGNHAP(@username, @passwordSHA1)", con);
            sqlcmd.Parameters.AddWithValue("@username", username);
            sqlcmd.Parameters.AddWithValue("@passwordSHA1", hashPassSHA1);
            open();
            SqlDataReader reader = sqlcmd.ExecuteReader();
            if (reader.HasRows && reader.Read() && reader[0].ToString() == "400")
            {
                close();
                return false;
            }
            else
            {
                token = reader[0].ToString();
                try
                {
                    StreamReader r = new StreamReader($"publickey\\{username}.txt");
                    rsa.FromXmlString(r.ReadToEnd());
                    r.Close();
                }
                catch { }
                close();
                return true;
            }
        }
        public ObservableCollection<lopHoc> QuanLiLopHoc()
        {
            ObservableCollection<lopHoc> list = new ObservableCollection<lopHoc>();
            SqlCommand sqlcmd = new SqlCommand("exec dsLop @token", con);
            sqlcmd.Parameters.AddWithValue("@token", token);
            open();
            SqlDataReader reader = sqlcmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    lopHoc lop = new lopHoc();
                    lop.malop = reader[0].ToString();
                    lop.tenlop = reader[1].ToString();
                    lop.manv = reader[2].ToString();
                    list.Add(lop);
                }
            }
            close();
            return list;
        }
        public ObservableCollection<sinhvien> detailLop(string malop)
        {
            ObservableCollection<sinhvien> list = new ObservableCollection<sinhvien>();
            SqlCommand sqlcmd = new SqlCommand("exec dsSinhVien @token, @MALOP", con);
            sqlcmd.Parameters.AddWithValue("@token", token);
            sqlcmd.Parameters.AddWithValue("@MALOP", malop);
            open();
            SqlDataReader reader = sqlcmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    sinhvien sv = new sinhvien();
                    sv.masv = reader[0].ToString();
                    sv.hoten = reader[1].ToString();
                    sv.ngaysinh = DateTime.Parse(reader[2].ToString());
                    sv.diachi = reader[3].ToString();
                    list.Add(sv);
                }
            }
            close();
            return list;
        }
        public ObservableCollection<NhanVien> QuanLiNhanVien()
        {
            ObservableCollection<NhanVien> list = new ObservableCollection<NhanVien>();
            SqlCommand sqlcmd = new SqlCommand("exec SP_SEL_PUBLIC_ENCRYPT_NHANVIEN", con);
            open();
            SqlDataReader reader = sqlcmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    NhanVien nv = new NhanVien();
                    nv.manv = reader[0].ToString();
                    nv.HoTen = reader[1].ToString();
                    nv.email = reader[2].ToString();
                    nv.luong = "";
                    nv.luongEnc = reader[3] is DBNull ? new byte[] { } : (byte[])reader[3];
                    list.Add(nv);
                }
            }
            close();
            return list;
        }
        public void edit(string masv, string hoten, DateTime ngaysinh, string diachi)
        {
            SqlCommand sqlcmd = new SqlCommand("exec editSinhVien @token, @MASV, @HOTEN, @NGAYSINH, @DIACHI", con);
            sqlcmd.Parameters.AddWithValue("@token", token);
            sqlcmd.Parameters.AddWithValue("@MASV", masv);
            sqlcmd.Parameters.AddWithValue("@HOTEN", hoten);
            sqlcmd.Parameters.AddWithValue("@NGAYSINH", ngaysinh);
            sqlcmd.Parameters.AddWithValue("@DIACHI", diachi);
            open();
            sqlcmd.ExecuteNonQuery();
            close();
        }
        public void addBangDiem(string masv, string hocphan, string diem)
        {
            SqlCommand sqlcmd = new SqlCommand("exec addBangDiem @token, @MASV, @HOCPHAN, @DIEM", con);
            sqlcmd.Parameters.AddWithValue("@token", token);
            sqlcmd.Parameters.AddWithValue("@MASV", masv);
            sqlcmd.Parameters.AddWithValue("@HOCPHAN", hocphan);
            sqlcmd.Parameters.AddWithValue("@DIEM", rsa.Encrypt(Encoding.ASCII.GetBytes(diem),false));
            open();
            sqlcmd.ExecuteNonQuery();
            close();
        }
        public void addNV(string manv, string hoten, string email, string luong, string tendn, string matkhau, string publicKey)
        {
            byte[] inputPass = Encoding.ASCII.GetBytes(matkhau);
            SHA1 sha1 = SHA1.Create();
            byte[] hashPassSHA1 = sha1.ComputeHash(inputPass);
            RSACryptoServiceProvider rsaTemp = new RSACryptoServiceProvider(512);
            rsaTemp.FromXmlString(publicKey);
            StreamWriter writer = new StreamWriter($"publickey\\{tendn}.txt");
            writer.Write(publicKey);
            writer.Close();
            byte[] encLuong = rsaTemp.Encrypt(Encoding.ASCII.GetBytes(luong), false);
            SqlCommand sqlcmd = new SqlCommand("exec SP_INS_PUBLIC_ENCRYPT_NHANVIEN @manv, @hoten, @email, @luong, @tendn, @matkhau", con);
            sqlcmd.Parameters.AddWithValue("@manv", manv);
            sqlcmd.Parameters.AddWithValue("@hoten", hoten);
            sqlcmd.Parameters.AddWithValue("@email", email);
            sqlcmd.Parameters.AddWithValue("@luong", encLuong);
            sqlcmd.Parameters.AddWithValue("@tendn", tendn);
            sqlcmd.Parameters.AddWithValue("@matkhau", hashPassSHA1);
            open();
            sqlcmd.ExecuteNonQuery();
            close();
        }
    }
}