using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Configuration;
using 

namespace Beanify_Playlist
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        string azConnect = ConfigurationManager.AppSettings["AzureDBConnection"] + "&J3b&T8h;" + ConfigurationManager.AppSettings["AzureDBConnection2"];

        AzureTableEntities con
        
        public Register()
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

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Reset()
        {
            textBoxName.Text = "";
            passwordBox1.Password = "";
            passwordBoxConfirm.Password = "";
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
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
                if (passwordBox1.Password.Length == 0)
                {
                    MessageBox.Show("Enter password.");
                    passwordBox1.Focus();
                }
                else if (passwordBoxConfirm.Password.Length == 0)
                {
                    MessageBox.Show("Enter Confirm password.");
                    passwordBoxConfirm.Focus();
                }
                else if (passwordBox1.Password != passwordBoxConfirm.Password)
                {
                    MessageBox.Show("Confirm password must be same as password.");
                    passwordBoxConfirm.Focus();
                }
                else
                {
                    SqlConnection con = new SqlConnection(azConnect);
                    con.Open();
                    SqlCommand nameTaken = new SqlCommand("Select * from Users WHERE Username = '" + username + "'", con);
                    nameTaken.CommandType = CommandType.Text;
                    nameTaken.ExecuteScalar();
                    if (nameTaken.ExecuteScalar() == null)
                    {
                        int UserID = 0;
                        SqlCommand getID = new SqlCommand("Select TOP 1 UserID from Users order by UserID Desc", con);
                        getID.CommandType = CommandType.Text;
                        getID.ExecuteScalar();
                        if (getID.ExecuteScalar().ToString() == "-1")
                        {
                            UserID = 0;
                        }
                        else
                        {
                            UserID = Int32.Parse(getID.ExecuteScalar().ToString()) + 1;
                        }
                        SqlCommand cmd = new SqlCommand("Insert into Users (UserID, Username, Password) values('" + UserID + "','" + username + "','" + password + "')", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                        con.Close();
                        MessageBox.Show("Thanks for registering, " + username + "!");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Username already taken!");
                    }
                    Reset();
                }
            }
        }


    }
}
