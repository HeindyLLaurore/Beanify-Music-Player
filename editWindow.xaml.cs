using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace Beanify_Playlist
{
    /// <summary>
    /// Interaction logic for editWindow.xaml
    /// </summary>
    public partial class editWindow : Window
    {
        public string rootPath = "C:\\Beanify";
        string currentUser = "";
        string playlist = ((MainWindow)System.Windows.Application.Current.MainWindow).TblCurrentPlaylist.Text;
        string copyPath = "";

        public editWindow()
        {
            InitializeComponent();
            currentUser = ((MainWindow)System.Windows.Application.Current.MainWindow).TblCurrentUser.Text.Substring(9);
            rootPath = rootPath + currentUser;
            CurrentPlaylist.Text = ((MainWindow)System.Windows.Application.Current.MainWindow).TblCurrentPlaylist.Text;
        }
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
            Close();
        }
        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            string oldPlaylistPath = rootPath + "\\" + playlist + "\\";
            bool inUse = false;
            if (TbxTitle.Text != null && TbxTitle.Text != "" && TbxTitle.Text != playlist)
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).LvPlaylist.SelectedItem = null;
                ((MainWindow)System.Windows.Application.Current.MainWindow).LvSonglist.SelectedItem = null;
                ((MainWindow)System.Windows.Application.Current.MainWindow).PlaylistCover.Source = null;
                if (File.Exists(String.Format("{0}\\{1}.txt", rootPath, playlist)))
                {
                    try
                    {
                        File.Copy(String.Format("{0}\\{1}.txt", rootPath, playlist), String.Format("{0}\\{1}.txt", rootPath, TbxTitle.Text));
                        File.Delete(String.Format("{0}\\{1}.txt", rootPath, playlist));
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Playlist name already in use!");
                        inUse = true;
                    }

                    string imgPath = File.ReadAllText(String.Format("{0}\\{1}.txt", rootPath, TbxTitle.Text));
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PlaylistCover.Source = new BitmapImage(new Uri(imgPath));
                }
                if (inUse == false)
                {
                    Directory.CreateDirectory(rootPath + "\\" + TbxTitle.Text);
                    string[] toCopy = Directory.GetFiles(oldPlaylistPath);
                    foreach (string file in toCopy)
                    {
                        string name = file.Substring(oldPlaylistPath.Length);
                        File.Copy(file, String.Format("{0}\\{1}\\{2}", rootPath, TbxTitle.Text, name));
                        File.Delete(file);
                    }
                    Directory.Delete(oldPlaylistPath);
                }
                ((MainWindow)System.Windows.Application.Current.MainWindow).PlaylistCover.Source = null;
                ((MainWindow)System.Windows.Application.Current.MainWindow).CurrentPlaylistSongCount.Text = null;
                ((MainWindow)System.Windows.Application.Current.MainWindow).PlaylistOptions.Visibility = Visibility.Hidden;
                ((MainWindow)System.Windows.Application.Current.MainWindow).LvSonglist.Visibility = Visibility.Hidden;
            }
            
        

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
        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            string copyPath = "";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = String.Format("New Playlist Cover");
            openFileDialog.Filter = "JPG files (*.jpg)|*.jpg|PNG files (*.png)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                copyPath = openFileDialog.FileName;
            }
            if (copyPath != null && copyPath != "")
            {
                Cover.Text = copyPath;
                if (!File.Exists(String.Format("{0}\\{1}.txt", rootPath, playlist)))
                {
                    File.WriteAllText(String.Format("{0}\\{1}.txt", rootPath, playlist), copyPath);
                    string imgPath = File.ReadAllText(String.Format("{0}\\{1}.txt", rootPath, playlist));
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PlaylistCover.Source = new BitmapImage(new Uri(imgPath));
                }
                else
                {
                    File.Delete(String.Format("{0}\\{1}.txt", rootPath, playlist));
                    File.WriteAllText(String.Format("{0}\\{1}.txt", rootPath, playlist), copyPath);
                    string imgPath = File.ReadAllText(String.Format("{0}\\{1}.txt", rootPath, playlist));
                    ((MainWindow)System.Windows.Application.Current.MainWindow).PlaylistCover.Source = new BitmapImage(new Uri(imgPath));
                }
            }
        }
    }
}