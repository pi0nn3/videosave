using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace videosave
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
            
        }


        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(textBox1.Text, UriKind.Absolute))
            {
                await DownloadVideoAsync(textBox1.Text);
            }
        }

        private async Task DownloadVideoAsync(string videoUrl)
        {
            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(videoUrl);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

                string cleanFileName = CleanFileName(video.Title);

                if (comboBox1.SelectedItem.ToString() == "MP4")
                {
                    var streamInfo = streamManifest
                        .GetMuxedStreams()
                        .Where(s => s.Container.Name == "mp4")
                        .OrderByDescending(s => s.VideoResolution.Height >= 1080 ? s.VideoResolution.Height : 0)
                        .ThenByDescending(s => s.VideoResolution.Height)
                        .FirstOrDefault();

                    if (streamInfo != null)
                    {
                        var fileName = $"{cleanFileName}.mp4";
                        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        var filePath = Path.Combine(desktopPath, fileName);

                        await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                        MessageBox.Show($"Video baþarýyla masaüstüne indirildi: {filePath}");
                    }
                    else
                    {
                        MessageBox.Show("Video indirme sýrasýnda bir sorun oluþtu.");
                    }
                }
                else if (comboBox1.SelectedItem.ToString() == "MP3")
                {
                    var streamInfo = streamManifest
                        .GetAudioOnlyStreams()
                        .OrderByDescending(s => s.Bitrate)
                        .FirstOrDefault();

                    if (streamInfo != null)
                    {
                        var fileName = $"{cleanFileName}.mp3";
                        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        var filePath = Path.Combine(desktopPath, fileName);

                        await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                        MessageBox.Show($"Ses baþarýyla masaüstüne indirildi: {filePath}");
                    }
                    else
                    {
                        MessageBox.Show("Ses indirme sýrasýnda bir sorun oluþtu.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        private string CleanFileName(string fileName)
        {
            // Geçersiz karakterleri temizleme
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_'); 
            }
            return fileName;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
