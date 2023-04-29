using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Security.Cryptography;
using bmcsdl.Model;
namespace bmcsdl.ViewModel
{
    public class QuanLiLopVM: BaseViewModel
    {
        private string _txtManv;
        public string txtManv { get => _txtManv; set { _txtManv = value; OnPropertyChanged(nameof(txtManv)); } }
        private string _txtHoten;
        public string txtHoten { get => _txtHoten; set { _txtHoten = value; OnPropertyChanged(nameof(txtHoten)); } }
        private string _txtEmail;
        public string txtEmail { get => _txtEmail; set { _txtEmail = value; OnPropertyChanged(nameof(txtEmail)); } }
        private string _txtLuong;
        public string txtLuong { get => _txtLuong; set { _txtLuong = value; OnPropertyChanged(nameof(txtLuong)); } }
        private string _txtTendn;
        public string txtTendn { get => _txtTendn; set { _txtTendn = value; OnPropertyChanged(nameof(txtTendn)); } }
        private string _txtMatkhau;
        public string txtMatkhau { get => _txtMatkhau; set { _txtMatkhau = value; OnPropertyChanged(nameof(txtMatkhau)); } }
        private string _txtPublicKey;
        public string txtPublicKey { get => _txtPublicKey; set { _txtPublicKey = value; OnPropertyChanged(nameof(txtPublicKey)); } }
        private string _txtManvDec;
        public string txtManvDec { get => _txtManvDec; set { _txtManvDec = value; OnPropertyChanged(nameof(txtManvDec)); } }
        private string _txtPrivateKey;
        public string txtPrivateKey { get => _txtPrivateKey; set { _txtPrivateKey = value; OnPropertyChanged(nameof(txtPrivateKey));   } }
        public ICommand addNV { get; set; }
        public ICommand detail { get; set; }
        public ICommand cmdDecrypt { get; set; }
        public static ObservableCollection<NhanVien> listNV { get; set; }
        public static ObservableCollection<lopHoc> listLop { get; set; }
        public static UserControl cur { get; set; }
        private dataBase data;
        public QuanLiLopVM()
        {
            data = dataBase.get_dataBase();
            listLop = new ObservableCollection<lopHoc>();
            detail = new RelayCommand<object>((p) => { return p == null ? false : true; }, (p) => { cmdDetail(p as string); });
            listNV = new ObservableCollection<NhanVien>();
            addNV = new RelayCommand<object>((p) => { return true; }, (p) => { cmdAddNV(); });
            cmdDecrypt = new RelayCommand<object>((p) => { return true; }, (p) => { cmdDecryptNV(); });
        }
        private void cmdDetail(string p)
        {
            Window w = GetWindowParent(cur) as Window;
            Grid quanli = w.FindName("quanlilophoc") as Grid;
            Grid detail = w.FindName("detailLop") as Grid;
            quanli.Visibility = Visibility.Collapsed;
            detail.Visibility = Visibility.Visible;
            ObservableCollection<sinhvien> temp = data.detailLop(p);
            detailLopVM.list.Clear();
            for (int i = 0;  i < temp.Count; i++)
            {
                detailLopVM.list.Add(temp[i]);
            }
            detailLopVM.MaLop = p;
        }
        private void cmdAddNV()
        {
            data.addNV(txtManv, txtHoten, txtEmail, txtLuong, txtTendn, txtMatkhau, txtPublicKey);
            txtManv = txtHoten = txtEmail = txtLuong = txtTendn = txtMatkhau = txtPublicKey = "";
            ObservableCollection<NhanVien> temp = data.QuanLiNhanVien();
            listNV.Clear();
            for (int i = 0; i < temp.Count; i++)
            {
                listNV.Add(temp[i]);
            }
        }
        private void cmdDecryptNV()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(txtPrivateKey);
            ObservableCollection<NhanVien> temp = new ObservableCollection<NhanVien>();
            for(int i = 0; i < listNV.Count; i++)
            {
                if (listNV[i].manv == txtManvDec)
                {
                    listNV[i].luong = Encoding.ASCII.GetString(rsa.Decrypt(listNV[i].luongEnc, false));
                }
                temp.Add(listNV[i]);
            }
            listNV.Clear();
            for(int i = 0;i < temp.Count;i++)
            {
                listNV.Add(temp[i]);
            }
            txtManvDec = txtPrivateKey = "";
        }
    }
}
