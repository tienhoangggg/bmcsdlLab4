using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using bmcsdl.Model;
namespace bmcsdl.ViewModel
{
    public class loginVM: BaseViewModel
    {
        public ICommand login { get; set; }
        public ICommand exit { get; set; }
        private dataBase data;
        private string _dangnhap;
        public string dangnhap { get => _dangnhap; set { _dangnhap = value; OnPropertyChanged(nameof(dangnhap)); } }
        public loginVM()
        {
            login = new RelayCommand<object>((p) => { return p == null ? false : true; }, (p) => { cmdlogin(p as UserControl); });
            exit = new RelayCommand<object>((p) => { return p == null ? false : true; }, (p) => { cmdexit(p as UserControl); });
            data = dataBase.get_dataBase();
        }
        private void cmdlogin(UserControl p)
        {
            Window w = GetWindowParent(p) as Window;
            if (data.login(dangnhap, (p.FindName("passWord") as PasswordBox).Password))
            {
                Grid login = w.FindName("login") as Grid;
                Grid quanli = w.FindName("quanlilophoc") as Grid;
                login.Visibility = Visibility.Collapsed;
                quanli.Visibility = Visibility.Visible;
                ObservableCollection<lopHoc> temp = data.QuanLiLopHoc();
                QuanLiLopVM.listLop.Clear();
                for (int i = 0; i < temp.Count; i++)
                {
                    QuanLiLopVM.listLop.Add(temp[i]);
                }
                ObservableCollection<NhanVien> temp2 = data.QuanLiNhanVien();
                QuanLiLopVM.listNV.Clear();
                for (int i = 0; i < temp2.Count; i++)
                {
                    QuanLiLopVM.listNV.Add(temp2[i]);
                }
            }
            else
            {
                MessageBox.Show("tên đăng nhập và mật khẩu không hợp lệ");
            }
        }
        private void cmdexit(UserControl p)
        {
            Window w = GetWindowParent(p) as Window;
            w.Close();
        }
    }
}
