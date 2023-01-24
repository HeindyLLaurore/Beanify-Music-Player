using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Threading;
using System.IO.Compression;
using Azure.Storage.Blobs;
using System.Configuration;
using Path = System.IO.Path;
using File = System.IO.File;

namespace Beanify_Playlist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string rootPath = "C:\\Beanify";
        public string archivePath = "C:\\BeanifyArchive.zip";
        string currentUser = "";
        string newSongFilePath = "";
        string newSongFileName = "";

        private MediaPlayer mediaPlayer = new MediaPlayer();
        private bool sliderDrag = false;
        bool shuffle = false;
        bool loop = true;

        List<string> playlists = new List<string>();
        List<string> currentSongs = new List<string>();
        
        public MainWindow()
        {
            InitializeComponent();
            Login login = new Login();
            login.ShowDialog();
            if(TblCurrentUser.Text.Length > 0)
            {
                currentUser = TblCurrentUser.Text.Substring(9);
                rootPath = rootPath + currentUser;
                archivePath = String.Format("{0}Archive{1}.zip", rootPath, currentUser);
                //Create Beanify root directory & Default playlist
                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                    Directory.CreateDirectory(rootPath + "\\MySongs");
                }
                //Get Playlists from root if it exists
                else
                {
                    int rootLength= rootPath.Length;
                    string[] playlistPaths = Directory.GetDirectories(rootPath);
                    foreach (string playlistPath in playlistPaths)
                    {
                        string playlistName = playlistPath.Substring(rootLength + 1);
                        playlists.Add(playlistName);
                    }
                    LvPlaylist.ItemsSource = playlists;
                    LvPlaylist.Items.Refresh();
                }
                //Set songs from playlist
                LvSonglist.ItemsSource = currentSongs;
                //Create archive
                RebuildArchive();
            }
            else
            {
                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    Environment.Exit(1);
                }
            }
           
        }

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
            Application.Current.Shutdown();
        }
       

/*===================================
    * Archive/Azure
====================================*/
        private void RebuildArchive()
        {
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
                ZipFile.CreateFromDirectory(rootPath, archivePath);
            }
            else
            {
                ZipFile.CreateFromDirectory(rootPath, archivePath);
            }
        }
        public static async Task UploadFile(BlobContainerClient containerClient, string localFilePath)
        {
            string fileName = Path.GetFileName(localFilePath);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(localFilePath, true);
        }
        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoadingWindow w = new LoadingWindow();
            w.Title = "Backing Up to Cloud...";
            w.Show();
            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            string storageContainer = "beanifygeneral";
            BlobContainerClient azContainer = new BlobContainerClient(connectionString, storageContainer);
            await UploadFile(azContainer, archivePath);
            w.Close();
            Application.Current.Shutdown();
        }

/*===================================
    * Playlists
====================================*/
        private void refreshPlaylist()
        {
            currentSongs.Clear();
            int directory = (String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text)).Length + 1;
            string[] playlistSongs = Directory.GetFiles(String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text));
            if (playlistSongs.Length != 0)
            {
                foreach (string song in playlistSongs)
                {
                    currentSongs.Add(song.Substring(directory));
                }
            }
            LvSonglist.Items.Refresh();
        }
        public void Refresh()
        {
            string[] playlistPaths = Directory.GetDirectories(rootPath);
            int rootLength = rootPath.Length;
            foreach (string playlistPath in playlistPaths)
            {
                string playlistName = playlistPath.Substring(rootLength + 1);
                playlists.Add(playlistName);
            }
            LvPlaylist.ItemsSource = playlists;
            LvPlaylist.Items.Refresh();
        }
        private void LvPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSongs.Clear();
            int directory = (String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text)).Length + 1;
            string[] playlistSongs = Directory.GetFiles(String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text));    
            if(playlistSongs.Length != 0)
            {
                foreach(string song in playlistSongs)
                {
                    currentSongs.Add(song.Substring(directory));
                }
            }
            CurrentPlaylistSongCount.Text = currentSongs.Count.ToString() + " Songs";
            PlaylistOptions.Visibility = Visibility.Visible;
            LvSonglist.Visibility = Visibility.Visible;
            LvSonglist.Items.Refresh();

            string imgName = TblCurrentPlaylist.Text + ".txt";
            if (File.Exists(rootPath + "\\" + imgName))
            {
                string imgPath = File.ReadAllText(rootPath + "\\" + imgName);
                PlaylistCover.Source = new BitmapImage(new Uri(imgPath));
            }
            else
            {
                PlaylistCover.Source = null;
            }
            
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            addWindow addWindow = new addWindow();
            addWindow.Title = "Add new Playlist";
            addWindow.Show();
        }
        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {

            LoadingWindow w = new LoadingWindow();
            w.Title = "Delete Playlist";
            if (TblCurrentPlaylist.Text != "" && TblCurrentPlaylist.Text != null && Directory.Exists(String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text)))
            {
                PlaylistCover.Source = null;
                w.Show();
                if (Directory.GetFiles(String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text)).Length > 0)
                {
                    string[] files = Directory.GetFiles(String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text));
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
                Directory.Delete(String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text));
                
                if (File.Exists(String.Format("{0}\\{1}.txt", rootPath, TblCurrentPlaylist.Text)))
                {
                    try
                    {
                        File.Delete(String.Format("{0}\\{1}.txt", rootPath, TblCurrentPlaylist.Text));
                    }
                    catch(IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            List<string> playlists = new List<string>();
            int rootLength = rootPath.Length;
            string[] playlistPaths = Directory.GetDirectories(rootPath);
            foreach (string playlistPath in playlistPaths)
            {
                string Name = playlistPath.Substring(rootLength + 1);
                playlists.Add(Name);
            }
            LvPlaylist.ItemsSource = playlists;
            LvPlaylist.Items.Refresh();
            RebuildArchive();
            w.Close();
          
            if (LvPlaylist.Items.Count == 0)
            {
                PlaylistCover.Source = null;
                CurrentPlaylistSongCount.Text = null;
                PlaylistOptions.Visibility = Visibility.Hidden;
                LvSonglist.Visibility = Visibility.Hidden;
                currentSongs.Clear();
                LvSonglist.Items.Refresh();
            }
            else
            {
                CurrentPlaylistSongCount.Text = null;
                currentSongs.Clear();
                LvSonglist.Items.Refresh();
            }
        }
        private void Options_Click(object sender, RoutedEventArgs e)
        {
            if (LvPlaylist.SelectedItem != null)
            {
                string pl = LvPlaylist.SelectedItem.ToString();
                editWindow editWindow = new editWindow();
                editWindow.Title = "Editing " + TblCurrentPlaylist.Text;
                PlaylistCover.Source = null;
                editWindow.Show();
            }
        }

/*===================================
    * Song List
====================================*/
        private void AddSong_Click(object sender, RoutedEventArgs e)
        {
            if (TblCurrentPlaylist.Text == "" || TblCurrentPlaylist.Text == null)
            {
                MessageBox.Show("Please select a playlist!");
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = String.Format("Add New Song to {0}", TblCurrentPlaylist.Text);
                openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|WAV files (*.wav)|*.wav";

                if (openFileDialog.ShowDialog() == true)
                {
                    newSongFilePath = openFileDialog.FileName;
                    newSongFileName = openFileDialog.SafeFileName;
                }
                if (newSongFilePath != null && newSongFilePath != "")
                {
                    LoadingWindow w = new LoadingWindow();
                    w.Title = "Adding Song...";
                    w.Show();
                    File.Copy(newSongFilePath, String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, newSongFileName), true);
                    refreshPlaylist();
                    RebuildArchive();
                    w.Close();
         
                }
                currentSongs.Clear();
                int directory = (String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text)).Length + 1;
                string[] playlistSongs = Directory.GetFiles(String.Format("{0}\\{1}", rootPath, TblCurrentPlaylist.Text));
                if (playlistSongs.Length != 0)
                {
                    foreach (string song in playlistSongs)
                    {
                        currentSongs.Add(song.Substring(directory));
                    }
                }
                CurrentPlaylistSongCount.Text = currentSongs.Count.ToString() + " Songs";
            }
        }

        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
            if (TblCurrentPlaylist.Text == "" || TblCurrentPlaylist.Text == null)
            {
                MessageBox.Show("Select a playlist!");
            }
            else
            {
                if (LvSonglist.SelectedItem == null)
                {
                    MessageBox.Show("Select a song!");
                }
                else
                {
                    LoadingWindow w = new LoadingWindow();
                    w.Title = "Delete Song";
                    w.Show();
                    File.Delete(String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, LvSonglist.SelectedItem));
                    refreshPlaylist();
                    CurrentPlaylistSongCount.Text = currentSongs.Count.ToString() + " Songs";
                    RebuildArchive();
                    w.Close();
                }
            }
        }

/*===================================
    * Music Player
====================================*/
        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !sliderDrag)
            {
                TblCurrentTime.Text = mediaPlayer.Position.ToString(@"mm\:ss");
                TblSongLength.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
            else
            {
                TblCurrentTime.Text = "00:00";
                TblSongLength.Text = "00:00";
            }
            if (sliProgress.Value >= sliProgress.Maximum)
            {
                if (loop == true)
                {
                    string nextSong = "";
                    if (LvSonglist.SelectedIndex == currentSongs.Count - 1)
                    {
                        nextSong = LvSonglist.Items[0].ToString();
                    }
                    else
                    {
                        nextSong = LvSonglist.Items[LvSonglist.SelectedIndex + 1].ToString();
                    }

                    if (nextSong != "")
                    {
                        LvSonglist.SelectedItem = nextSong;
                        string nextPath = String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, nextSong);
                        mediaPlayer.Open(new Uri(nextPath));
                        mediaPlayer.Play();
                        TblCurrentSong.Text = nextSong;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                }
                else
                {
                    string randomSong = "";
                    Random r = new Random();
                    int randomInt = r.Next(0, currentSongs.Count);
                    while(randomInt == LvSonglist.SelectedIndex)
                    {
                        randomInt = r.Next(0, currentSongs.Count);
                    }
                    randomSong = LvSonglist.Items[randomInt].ToString();
                    if (randomSong != "")
                    {
                        LvSonglist.SelectedItem = randomSong;
                        string randomPath = String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, randomSong);
                        mediaPlayer.Open(new Uri(randomPath));
                        mediaPlayer.Play();
                        TblCurrentSong.Text = randomSong;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                }
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (LvSonglist.SelectedItem != null)
            {
                if (TblCurrentSong.Text != LvSonglist.SelectedItem.ToString() || TblCurrentTime.Text == "00:00")
                {
                    string songPath = String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, LvSonglist.SelectedItem);
                    mediaPlayer.Open(new Uri(songPath));
                }
                
                mediaPlayer.Play();
                TblCurrentSong.Text = LvSonglist.SelectedItem.ToString();
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();
            }
            else
            {
                TblCurrentSong.Text = "Play a song!";
            }
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }
        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (LvSonglist.SelectedItem != null)
            {
                if (loop == true)
                {
                    string prevSong = "";
                    if (LvSonglist.SelectedIndex == 0)
                    {
                        prevSong = LvSonglist.Items[currentSongs.Count - 1].ToString();
                    }
                    else
                    {
                        prevSong = LvSonglist.Items[LvSonglist.SelectedIndex - 1].ToString();
                    }

                    if (prevSong != "")
                    {
                        LvSonglist.SelectedItem = prevSong;
                        string prevPath = String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, prevSong);
                        mediaPlayer.Open(new Uri(prevPath));
                        mediaPlayer.Play();
                        TblCurrentSong.Text = prevSong;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                }
                else
                {
                    string randomSong = "";
                    Random r = new Random();
                    int randomInt = r.Next(0, currentSongs.Count);
                    while (randomInt == LvSonglist.SelectedIndex)
                    {
                        randomInt = r.Next(0, currentSongs.Count);
                    }
                    randomSong = LvSonglist.Items[randomInt].ToString();
                    if (randomSong != "")
                    {
                        LvSonglist.SelectedItem = randomSong;
                        string randomPath = String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, randomSong);
                        mediaPlayer.Open(new Uri(randomPath));
                        mediaPlayer.Play();
                        TblCurrentSong.Text = randomSong;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                }
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (LvSonglist.SelectedItem != null)
            {
                if (loop == true)
                {
                    string nextSong = "";
                    if (LvSonglist.SelectedIndex == currentSongs.Count - 1)
                    {
                        nextSong = LvSonglist.Items[0].ToString();
                    }
                    else
                    {
                        nextSong = LvSonglist.Items[LvSonglist.SelectedIndex + 1].ToString();
                    }

                    if (nextSong != "")
                    {
                        LvSonglist.SelectedItem = nextSong;
                        string nextPath = String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, nextSong);
                        mediaPlayer.Open(new Uri(nextPath));
                        mediaPlayer.Play();
                        TblCurrentSong.Text = nextSong;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                }
                else
                {
                    string randomSong = "";
                    Random r = new Random();
                    int randomInt = r.Next(0, currentSongs.Count);
                    while (randomInt == LvSonglist.SelectedIndex)
                    {
                        randomInt = r.Next(0, currentSongs.Count);
                    }
                    randomSong = LvSonglist.Items[randomInt].ToString();
                    if (randomSong != "")
                    {
                        LvSonglist.SelectedItem = randomSong;
                        string randomPath = String.Format("{0}\\{1}\\{2}", rootPath, TblCurrentPlaylist.Text, randomSong);
                        mediaPlayer.Open(new Uri(randomPath));
                        mediaPlayer.Play();
                        TblCurrentSong.Text = randomSong;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                }
            }
        }

        private void sliProgress_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(sliProgress.Value < sliProgress.Maximum)
            {
                int SliderValue = (int)sliProgress.Value;

                TimeSpan ts = TimeSpan.FromSeconds(sliProgress.Value);
                mediaPlayer.Position = ts;
            }
        }

        private void Element_MediaOpened(object sender, EventArgs e)
        {
            sliProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            myMediaElement.Source = new Uri(rootPath + TblCurrentPlaylist.Text + "\\" + TblCurrentSong.Text);
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffle = true;
            loop = false;
        }

        private void Loop_Click(object sender, RoutedEventArgs e)
        {
            loop = true;
            shuffle = false;
        }


      


    }
}
