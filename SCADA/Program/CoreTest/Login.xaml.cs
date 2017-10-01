using System.Windows;

namespace CoreTest
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        MemberIdentity identity;
        public static bool IsOpen = false;

        public Login()
        {
            IsOpen = true;
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUser.Text;
            if (string.IsNullOrEmpty(user) || user.Length > 10)
            {
                txterr.Text = "用户名不正确!字符数应在0-10之间";
                return;
            }
            //if (string.IsNullOrEmpty(txtring1.Text) || string.IsNullOrEmpty(txtring2.Text))
            //{
            //    txterr.Text = "必须输入两台制粒机的当前环模号！";
            //    return;
            //}
            identity = new MemberIdentity(user);
            int rs = identity.Authenticate(txtPassword.Password);
            switch (rs)
            {
                case 0:
                    App.Principal = new MemberPrincipal(identity);
                    //SystemLog.AddLog(new SystemLog(EventType.Simple, DateTime.Now, App.LogSource, "登录"));
                    //App.Server["P1_DIE"].Write(txtring1.Text);
                    //App.Server["P2_DIE"].Write(txtring2.Text);
                    this.Close();
                    return;
                case -1:
                    txterr.Text = "密码不正确!默认用户:op 默认密码:1";
                    break;
                case -2:
                    txterr.Text = "用户名不正确!默认用户:op 默认密码:1";
                    break;
                case -3:
                    txterr.Text = "用户名不正确!默认用户:op 默认密码:1";
                    break;
                default:
                    txterr.Text = "数据库连接失败!请检查";
                    break;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //DateTime start, end; string team;
            //WindowHelper.GetWorkInfo(DateTime.Now, out start, out end, out team);
            //var tag1 = App.Server["P1_DIE"];
            //txtring1.Text = tag1.ToString();
            //txtring2.Text = App.Server["P2_DIE"].ToString();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //EditUser frm = new EditUser();
            //frm.ShowDialog();
        }
    }
}
