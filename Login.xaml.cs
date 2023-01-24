using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace Beanify_Playlist
{
    public partial class Login : Window
    {
        string azConnect = ConfigurationManager.AppSettings["AzureDBConnection"] + "&J3b&T8h;" + ConfigurationManager.AppSettings["AzureDBConnection2"];
        public Login()
        {
            InitializeComponent();
            
            
        }

        

        //Draging the window anywhere
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        //minimize window
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //close window
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxName.Text.Length == 0)
            {
                MessageBox.Show("Enter a Username.");
                textBoxName.Focus();
            }
            else
            {
                string username = textBoxName.Text;
                string password = passwordBox1.Password;
                SqlConnection con = new SqlConnection(azConnect);
                con.Open();
                SqlCommand cmd = new SqlCommand("Select * from Users where Username='" + username + "'  and Password='" + password + "'", con);
                cmd.CommandType = CommandType.Text;
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    Close();
                    ((MainWindow)System.Windows.Application.Current.MainWindow).TblCurrentUser.Text = "Welcome, " + username;
                }
                else
                {
                    MessageBox.Show("Invalid account information.");
                }
                con.Close();
            }
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Register register = new Register();
            register.ShowDialog();
        }
    }
}
