using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;

using ctrl = System.Windows.Controls;
namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            String path = dialog.SelectedPath;
            Debug.WriteLine(path);
            if(path != "") {
                DirectoryInfo d = new DirectoryInfo(@path);
                FileInfo[] Files = d.GetFiles("*.mp4");

                
                //Debug.WriteLine(doc.DocumentNode.SelectNodes("//html//body//div").ToArray()[17].FirstChild.Attributes["href"].Value);
                //Debug.WriteLine(doc.DocumentNode.SelectNodes("//html//body//div[@id='rcnt']").ToArray()[0].InnerHtml);
                //Debug.WriteLine(doc.DocumentNode.SelectNodes("//html//body//div[@id='main']").ToArray()[0].InnerHtml);

                foreach (FileInfo file in Files)
                {

                    String name = String.Join(" ", file.Name.Split('_')[0].Split('.').ToArray());

                    var html = @"https://www.google.com/search?q=imdb+" + String.Join("+", name.Split(' ').ToArray());

                    HtmlWeb web = new HtmlWeb();

                    var doc = web.Load(html);

                    foreach (HtmlNode div in doc.DocumentNode.SelectNodes("//html//body//a").ToArray()) {
                        if (div.Attributes["data-jsarwt"] != null)
                        {
                            if (div.Attributes["data-jsarwt"].Value == "1") { 
                                Debug.WriteLine(div.OuterHtml);
                                break;
                            }
                        }

                    }
                    Debug.WriteLine("********************");
                    //Debug.WriteLine(doc.DocumentNode.SelectNodes("//html//body//div").ToArray()[17].OuterHtml);
                    //Debug.WriteLine((doc.DocumentNode.SelectNodes("//html//body//div[@id='main']").ToArray()[0]).InnerHtml);
                    /*if (doc != null)
                    {
                        // Fetch the stock price from the Web page
                        //string stockprice = doc.GetElementbyId.SelectSingleNode(string.Format(".//*[@class='{0}']", tickerid)).InnerHtml;
                        foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//span[@class='" + tickerid + "']"))
                        {
                            
                            string value = node.InnerHtml;
                            //Console.WriteLine(stockprice);
                            Console.WriteLine(value);
                            // etc...
                        }
                        
                    }*/

                    ImageBrush img= new ImageBrush();
                    //Image finalImage = new Image();
                    //finalImage.Width = 80;
                    //finalImage.Source = new BitmapImage(new Uri("Ressources/Thumbnail.png", UriKind.Relative));
                    img.ImageSource = new BitmapImage(new Uri("C:/Users/manka/source/repos/Media Player/Media Player/Ressources/Thumbnail.jpg", UriKind.Relative));
                    /*var uriSource = new Uri("Ressources/Untitled.png", UriKind.Relative);
                    BitmapImage logo = new BitmapImage(uriSource);
                    foo.Source = uriSource;
                    */
                    Border Video_border = new()
                    {
                        Name = "group_panel",
                        Style = (Style)System.Windows.Application.Current.Resources["video_border"],
                        Background = img
                    };

                    StackPanel Video_panel = new()
                    {
                        Orientation = ctrl.Orientation.Vertical,
                        Margin = new Thickness(10, 0, 10, 10),
                        Background = Brushes.Transparent,
                        Tag = ""
                    };

                    ctrl.Button video_Thumbnail = new()
                    {
                        Width = 200,
                        Height = 120,
                        BorderThickness = new Thickness(0, 0, 0, 0),
                        Background = Brushes.Transparent
                    };

                    ctrl.Label video_name = new()
                    {
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new Thickness(0, 2, 0, 0),
                        Content = name,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.DemiBold
                    };

                    Video_border.Child = video_Thumbnail;
                    Video_panel.Children.Add(Video_border);
                    Video_panel.Children.Add(video_name);

                    panel.Children.Add(Video_panel);

                }
            }
            
        }
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.Write(window.Width + " " + window.Height);
            stack_panel.Width = window.Width - 250;
            stack_panel.Height = window.Height - 155;
        }
    }
}
