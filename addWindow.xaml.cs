using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Beanify_Playlist
{
    /// <summary>
    /// Interaction logic for addWindow.xaml
    /// </summary>
    public partial class addWindow : Window
    {
        public string rootPath = "C:\\Beanify";
        string currentUser = "";

        public addWindow()
        {
            InitializeComponent();
            currentUser = ((MainWindow)System.Windows.Application.Current.MainWindow).TblCurrentUser.Text.Substring(9);
            rootPath = rootPath + currentUser;
        }
        // (string cover, string title, int songcount, int totalplays)

        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }         //minimize window
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }         //close window
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void addPlaylist_Click(object sender, RoutedEventArgs e)
        {
            string playlistName = Tbx_playlisttitle.Text;

            if (Directory.Exists(rootPath + "\\" + playlistName))
            {
                MessageBox.Show("Playlist name already in use! Please choose another name");
            }
            else
            {
                Directory.CreateDirectory(rootPath + "\\" + playlistName);
                Close();
                List<string> playlists = new List<string>();
                int rootLength = rootPath.Length;
                string[] playlistPaths = Directory.GetDirectories(rootPath);
                foreach (string playlistPath in playlistPaths)
                {
                    string Name = playlistPath.Substring(rootLength + 1);
                    playlists.Add(Name);
                }
                ((MainWindow)System.Windows.Application.Current.MainWindow).LvPlaylist.ItemsSource = playlists;
                ((MainWindow)System.Windows.Application.Current.MainWindow).LvPlaylist.Items.Refresh();
            }
        }

    }
}
