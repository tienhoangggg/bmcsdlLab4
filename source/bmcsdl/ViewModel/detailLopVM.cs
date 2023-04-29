using bmcsdl.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using bmcsdl.Model;
namespace bmcsdl.ViewModel
{
    public class detailLopVM: BaseViewModel
    {
        public static ObservableCollection<sinhvien> list { get; set; }
        public static string MaLop { get; set; }
        private string _txtMaSV;
        private string _txtDiaChi;
        private string _txtHoTen;
        private DateTime _NgaySinh;
        private string _txtHocPhan;
        private string _txtDiem;
        private string _txtMaSV2;
        public string txtHocPhan { get { return _txtHocPhan; } set { _txtHocPhan = value; OnPropertyChanged(nameof(txtHocPhan)); } }
        public string txtDiem { get { return _txtDiem; } set { _txtDiem = value; OnPropertyChanged(nameof(txtDiem)); } }
        public string txtMaSV2 { get { return _txtMaSV2; } set { _txtMaSV2 = value; OnPropertyChanged(nameof(txtMaSV2)); } }
        public string txtMaSV { get { return _txtMaSV; } set {  _txtMaSV = value; OnPropertyChanged(nameof(txtMaSV)); } }
        public string txtDiaChi { get { return _txtDiaChi; } set { _txtDiaChi = value; OnPropertyChanged(nameof(txtDiaChi)); } }
        public string txtHoTen { get { return _txtHoTen; } set { _txtHoTen = value; OnPropertyChanged(nameof(txtHoTen)); } }
        public DateTime NgaySinh { get { return _NgaySinh; } set { _NgaySinh = value; OnPropertyChanged(nameof(NgaySinh)); } }
        public ICommand edit { get; set; }
        public ICommand add { get; set; }
        private dataBase data { get; set; }
        public detailLopVM()
        {
            list = new ObservableCollection<sinhvien>();
            NgaySinh = DateTime.Now;
            data = dataBase.get_dataBase();
            edit = new RelayCommand<object>((p) => { return true; }, (p) => { editSV(); });
            add = new RelayCommand<object>((p) => { return true; }, (p) => { addBangDiem(); });
        }
        private void editSV()
        {
            data.edit(txtMaSV, txtHoTen, NgaySinh, txtDiaChi);
            ObservableCollection<sinhvien> temp = data.detailLop(MaLop);
            list.Clear();
            for (int i = 0;  i < temp.Count; i++)
            {
                list.Add(temp[i]);
            }
        }
        private void addBangDiem()
        {
            data.addBangDiem(txtMaSV2, txtHocPhan, txtDiem);
            txtMaSV2 = txtHocPhan = txtDiem = "";
        }
    }
}
