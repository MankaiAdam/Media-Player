using DM.MovieApi.ApiResponse;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;

using ctrl = System.Windows;
using System.Reflection.Metadata;
using System.Net;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Printing;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : ctrl.Window
    {
        FileInfo[] mp4_files;
        Resolution lastRes = new Resolution(0,0);
        int lastresX = 0;
        int lastresY = 0;
        bool fullscreen = false;
        bool nav_toggle = true;
        String[] categories = {"Action", "Adventure", "Animation", "Comedy", "Drama", "Fantasy", "History", "Horror", "Mystery", "Romance", "Sci-Fi", "Thriller", "War", "Western"};

        private class Resolution
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Resolution(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [Serializable]
        private class Movie
        {
            [JsonProperty("Title")]
            public string Title { get; set; }
            [JsonProperty("Year")]
            public string Year { get; set; }
            [JsonProperty("Posterpath")]
            public string Posterpath { get; set; }
            [JsonProperty("Description")]
            public string Description { get; set; }
            public Movie(string t,string y, string p, string d)
            {
                Title = t;
                Year = y;
                Posterpath = p;
                Description = d;
            }
        }

        /*public async Task GetCustomersAsync(WebClient client , string image_url, string local_poster_path)
        {
            await GetCustomers(client, image_url, local_poster_path);
        }
        public void GetCustomers(WebClient client, string image_url, string local_poster_path)
        { 
            client.DownloadFileAsync(new Uri(image_url), local_poster_path);
        }*/
        public MainWindow()
        {
            InitializeComponent();
            foreach (string C in categories)
            {
                ctrl.Controls.Button category_button = new()
                {
                    Content = C,
                    Style = (Style)System.Windows.Application.Current.Resources["category_button"]
                };
                category_button.Click += category_click;
                categories_panel.Children.Add(category_button);
            }
        }

        private void nav_toggle_Click(object sender, RoutedEventArgs e){
            if (nav_toggle)
            {
                movie_btn.Visibility = Visibility.Hidden;
                music_btn.Visibility = Visibility.Hidden;
                favorite_btn.Visibility = Visibility.Hidden;
                playlist_btn.Visibility = Visibility.Hidden;
                search_bar.Visibility = Visibility.Hidden;
                nav_panel.Width = 50;
                nav_toggle = false;
            }
            else {
                movie_btn.Visibility = Visibility.Visible;
                music_btn.Visibility = Visibility.Visible;
                favorite_btn.Visibility = Visibility.Visible;
                playlist_btn.Visibility = Visibility.Visible;
                search_bar.Visibility = Visibility.Visible;
                nav_panel.Width = 200;
                nav_toggle = true;
            }
        }

        private void category_click(object sender, RoutedEventArgs e)
        {
            ctrl.Controls.Button b = (ctrl.Controls.Button)sender;
            Debug.WriteLine(b.Content);

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            string movies_path = dialog.SelectedPath;
            string posters_folder_path = movies_path + "\\_posters";
            string data_folder_path = movies_path + "\\_data";

            if(movies_path != "") {

                if (!Directory.Exists(posters_folder_path))
                {
                    Directory.CreateDirectory(posters_folder_path);
                }
                if (!Directory.Exists(data_folder_path))
                {
                    Directory.CreateDirectory(data_folder_path);
                }

                DirectoryInfo d = new(data_folder_path);
                FileInfo[] data_files = d.GetFiles("*.json");

                DirectoryInfo d1 = new(movies_path);
                mp4_files = d1.GetFiles("*.mp4");

                List<Movie> movie_list = new();

                /*foreach (FileInfo file in data_files)
                {
                    string jsonString = File.ReadAllText(data_folder_path+ "\\" + file.Name);
                    Movie? movie = JsonConvert.DeserializeObject<Movie>(jsonString);
                    Debug.WriteLine(movie?.Title + " " + movie?.Year + " " + movie?.Posterpath + " " + movie?.Description);
                    movie_list.Add(movie);
                }*/

                foreach (FileInfo file in mp4_files)
                {
                    var image_url = posters_folder_path + "\\on_poster.jpg";
                    string movie_title = String.Join(" ", file.Name.Split('_')[0].Split('.').ToArray());
                    string movie_year = file.Name.Split('_')[1].Split('.').ToArray()[0];
                    string name = movie_title + " " + movie_year;
                    string local_poster_path = posters_folder_path + "\\" + movie_title + ".jpg";

                    if (!File.Exists(data_folder_path + "\\" + movie_title + ".json"))
                    {
                        var html = @"https://www.google.com/search?q=imdb+" + String.Join("+", name.Split(' ').ToArray());

                        HtmlWeb web = new();
                        var doc = web.Load(html);
                        var div = doc.DocumentNode.Descendants("a")
                                                  .Where(d => d.Attributes["data-jsarwt"] != null)
                                                  .Where(d => d.Attributes["data-jsarwt"].Value == "1")
                                                  .ToArray()[0];
                        Debug.WriteLine(div.OuterHtml);

                        String url = div.Attributes["href"].Value;
                        html = url[..(url.IndexOf("/tt") + (url[((url.IndexOf("/tt")) + 1)..]).IndexOf('/') + 1)];
                        doc = web.Load(html);
                        var img_html = doc.DocumentNode.Descendants("img")
                                        .Where(d => d.Attributes["class"] != null)
                                        .Where(d => d.Attributes["class"].Value == "ipc-image")
                                        .ToArray()[0];
                        image_url = img_html.OuterHtml[(img_html.OuterHtml.IndexOf("285w,") + 6)..img_html.OuterHtml.IndexOf(" 380w")];

                        WebClient client = new();
                        await client.DownloadFileTaskAsync(new Uri(image_url), local_poster_path);
                        Movie movie = new(movie_title, movie_year, local_poster_path, "description");
                        string json = JsonConvert.SerializeObject(movie);
                        File.WriteAllText(data_folder_path + "\\" + movie_title + ".json", json);
                        movie_list.Add(movie);
                    }
                    else {
                        string jsonString = File.ReadAllText(data_folder_path + "\\" + movie_title + ".json");
                        Movie? movie = JsonConvert.DeserializeObject<Movie>(jsonString);
                        local_poster_path = movie?.Posterpath;
                        Debug.WriteLine(movie?.Title + " " + movie?.Year + " " + movie?.Posterpath + " " + movie?.Description);
                        movie_list.Add(movie);
                    }

                    /*var html = @"https://api.themoviedb.org/3/search/movie?api_key=5f458cc1d1fb42b312cfbacc909e7b31&query=the+edge+of+seventeen";

                    HtmlWeb web = new HtmlWeb();

                    var doc = web.Load(html);
                    HtmlNode div = doc.DocumentNode;
                    Debug.WriteLine(div!=null);

                    foreach (HtmlNode div in doc.DocumentNode.SelectNodes("//html//body//a").ToArray())
                    {
                        if (div.Attributes["data-jsarwt"] != null)
                        {
                            if (div.Attributes["data-jsarwt"].Value == "1")
                            {
                                String url = div.Attributes["href"].Value;
                                Debug.WriteLine(url.Substring(0, url.IndexOf("/tt") + 10));
                                //Debug.WriteLine(div.OuterHtml);
                                break;
                            }
                        }

                    }*/

                    Debug.WriteLine("********************");
                    string link = ""+ image_url;

                    ImageBrush img = new()
                    {
                        //img.ImageSource = new BitmapImage(new Uri("C:/Users/manka/source/repos/Media Player/Media Player/Ressources/Thumbnail.jpg", UriKind.Relative));
                        Stretch = Stretch.UniformToFill,
                        ImageSource = new BitmapImage(new Uri(local_poster_path, UriKind.Relative))
                    };

                    Grid Video_panel = new()
                    {
                        Margin = new Thickness(15, 0, 15, 15),
                        Background = Brushes.Transparent,
                        Tag = ""
                    };

                    Border Video_border = new()
                    {
                        Width = 150,
                        Height = 210,
                        Name = "group_panel",
                        BorderThickness = new Thickness(0, 0, 0, 0),
                        Style = (Style)System.Windows.Application.Current.Resources["video_border"],
                        Background = img,
                        BorderBrush = Brushes.White
                    };
                    Video_border.MouseLeftButtonUp += new MouseButtonEventHandler(this.Video_released);
                    Video_border.MouseMove += new ctrl.Input.MouseEventHandler(this.Video_hovered);
                    Video_border.MouseLeave += new ctrl.Input.MouseEventHandler(this.Video_left);

                    ctrl.Controls.Button video_Thumbnail = new()
                    {
                        Width = 150,
                        Height = 210,
                        BorderThickness = new Thickness(0, 0, 0, 0),
                        Background = Brushes.Transparent
                    };

                    ctrl.Controls.TextBlock video_name = new()
                    {
                        HorizontalAlignment = ctrl.HorizontalAlignment.Center,
                        VerticalAlignment = ctrl.VerticalAlignment.Center,
                        Margin = new Thickness(0, 2, 0, 0),
                        MaxWidth = 140,
                        TextWrapping = TextWrapping.WrapWithOverflow,
                        Text = movie_title + "(" + movie_year + ")",
                        Visibility = Visibility.Hidden,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.DemiBold
                    };
                    Video_panel.Children.Add(Video_border);
                    Video_panel.Children.Add(video_name);

                    panel.Children.Add(Video_panel);

                }
                /*string json = JsonSerializer.Serialize(movie_list);
                File.WriteAllText(@"E:\movies.json", json);*/

            }

        }

        private async void Play_Toggle_Click(object sender, RoutedEventArgs e)
        {
            player.Source = new Uri(mp4_files[0].FullName);
            control_bar.Background = new SolidColorBrush(Color.FromArgb(0x8F, 0x27, 0x27, 0x27));
            video_background.Visibility = Visibility.Visible;
            player.Visibility= Visibility.Visible;
            player.LoadedBehavior = MediaState.Manual;
            player.Play();
            await Task.Delay(1000);
            var duration = player.NaturalDuration.TimeSpan;
            duration_display.Content = new TimeSpan(duration.Hours, duration.Minutes, duration.Seconds).ToString()[1..];
            Debug.WriteLine(mp4_files[0].FullName);
            while (true) {
                play_progress_bar.Value = 100 * (player.Position.TotalSeconds / player.NaturalDuration.TimeSpan.TotalSeconds);
                Current_time_display.Content = new TimeSpan(player.Position.Hours, player.Position.Minutes, player.Position.Seconds).ToString()[1..];
                await Task.Delay(1000);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            player.Position += new TimeSpan(0,0,30);
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            player.Position -= new TimeSpan(0, 0, 30);
        }

        private void fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (fullscreen)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.ToolWindow;
                fullscreen = false;
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                fullscreen = true;
            }
        }

        private void OnKeyDownHandler(object sender, ctrl.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                next_button.RaiseEvent(new RoutedEventArgs(ctrl.Controls.Button.ClickEvent));
            }
            if (e.Key == Key.Left)
            {
                prev_button.RaiseEvent(new RoutedEventArgs(ctrl.Controls.Button.ClickEvent));
            }
            if (e.Key == Key.Up)
            {
                player.Volume += 0.1;
                Debug.WriteLine(player.Volume);
            }
            if (e.Key == Key.Down)
            {
                player.Volume -= 0.1;
                Debug.WriteLine(player.Volume);
            }
            if (e.Key == Key.Escape && fullscreen)
            {
                fullscreen_button.RaiseEvent(new RoutedEventArgs(ctrl.Controls.Button.ClickEvent));
            }
        }

        private void Video_left(object sender, ctrl.Input.MouseEventArgs e)
        {
            Border border = (Border)sender;
            Grid grid = (Grid)border.Parent;
            border.Opacity = 1;
            border.BorderThickness = new Thickness(0, 0, 0, 0);
            grid.Children.OfType<ctrl.Controls.TextBlock>().ToArray()[0].Visibility = Visibility.Hidden;
        }

        private void Video_hovered(object sender, ctrl.Input.MouseEventArgs e)
        {
            Border border = (Border)sender;
            Grid grid = (Grid)border.Parent;
            border.Opacity = 0.3;
            border.BorderThickness = new Thickness(2, 2, 2, 2);
            grid.Children.OfType<ctrl.Controls.TextBlock>().ToArray()[0].Visibility= Visibility.Visible;
        }

        private void Video_released(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            border.Opacity = 1;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            /*if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                lastRes.X = Convert.ToInt32(window.Width);
                lastRes.Y = (int)window.Height;
                
                window.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                window.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

                Debug.WriteLine("1-last reolution:" + lastRes.X + " " + lastRes.Y);

            }else {
                Debug.WriteLine("2-last reolution:" + lastRes.X + " " + lastRes.Y);
                window.Width = lastRes.X;
                window.Height = lastRes.Y;
            }
            Debug.WriteLine(window.Width + " " + window.Height);
            stack_panel.Width = window.Width - 250;
            stack_panel.Height = window.Height - 155;
            scroll_viewer.MaxHeight = window.Height - 260;*/
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("3-" + window.Width + " " + window.Height);
            //stack_panel.Height = window.Height - 155;
            //scroll_viewer.MaxHeight = window.Height - 260;
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;
        }

    }
}
