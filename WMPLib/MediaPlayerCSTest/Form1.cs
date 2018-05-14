using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaPlayerCSTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.Size = new Size(4200, 1050);
            this.Location = new Point(0, 0);

            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = Directory.GetParent(path).ToString();
            }

            //axWindowsMediaPlayer1.URL = path + @"\Documents\GitHub\MediaPlayer\Endszene.mp4";
            //axWindowsMediaPlayer1.URL = @"E:\_cs\mp4\VideoPlayer-master\Endszene.mp4";
            //axWindowsMediaPlayer1.URL = @"E:\_cs\mp4\VideoPlayer-master\testVideo.mp4";


            axWindowsMediaPlayer1.URL = @"C:\testout.wav";
            //axWindowsMediaPlayer1.URL = @"https://drive.google.com/uc?export=download&id=1u2wJYTB-hVWeZOLLd9CxcA9KCLuEanYg";
            //axWindowsMediaPlayer1.URL = @"https://r6---sn-8qj-i5oed.googlevideo.com/videoplayback?clen=3721767&itag=140&c=WEB&key=yt6&keepalive=yes&ipbits=0&initcwndbps=872500&dur=234.289&fvip=2&signature=96485C2FE1ADF51EC11A6A1E3664A64A983004E1.46E9E2D9737EF7D8B3153D5C06AFFAC1AA6A67AC&ei=AZ_5WrS_L5eSgAPr97WQCA&pcm2cms=yes&gir=yes&sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Ckeepalive%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2cms%2Cpl%2Crequiressl%2Csource%2Cexpire&lmt=1510741503111392&id=o-APWKqSJwgaglnnpKnznw4xZcxn1XrytPMHgnEp_enZQv&ms=au%2Crdu&mt=1526308496&mv=m&expire=1526330209&requiressl=yes&mime=audio%2Fmp4&ip=14.177.123.70&mm=31%2C29&source=youtube&pl=20&mn=sn-8qj-i5oed%2Csn-i3b7kn7d";


            ///axWindowsMediaPlayer1.URL = @"https://r6---sn-8qj-i5oed.googlevideo.com/videoplayback?itag=22&c=WEB&key=yt6&ratebypass=yes&ipbits=0&initcwndbps=872500&dur=234.289&fvip=2&signature=A55AD6D616624565B8322EE3EED2DC91FD9F293F.1B913DA07845E3A27911608104D77E3C02462E6B&ei=AZ_5WrS_L5eSgAPr97WQCA&pcm2cms=yes&sparams=dur%2Cei%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2cms%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&lmt=1510741625396835&id=o-APWKqSJwgaglnnpKnznw4xZcxn1XrytPMHgnEp_enZQv&ms=au%2Crdu&mt=1526308496&mv=m&expire=1526330209&requiressl=yes&mime=video%2Fmp4&ip=14.177.123.70&mm=31%2C29&source=youtube&pl=20&mn=sn-8qj-i5oed%2Csn-i3b7kn7d";
            axWindowsMediaPlayer1.uiMode = "none";
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {

            /**** Don't add this if you want to play it on multiple screens***** /
             * 
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                axWindowsMediaPlayer1.fullScreen = true;
            }
            /********************************************************************/

            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsStopped)
            {
                Application.Exit();
            }

        }
    }
}
