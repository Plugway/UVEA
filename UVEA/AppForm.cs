using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Accord.Video.FFMPEG;
using Timer = System.Windows.Forms.Timer;

namespace UVEA
{
    public class AppForm : Form
    {
        public static string VideoPath;
        public static string VideoName;
        public static string VideoExtension;
        public static VideoFileReader Reader = new VideoFileReader();
        public static VideoFileWriter Writer = new VideoFileWriter();
        public static readonly OneFrameDistorter Distorter = new OneFrameDistorter();
        public static double MultiplierFrom;
        public static double MultiplierTo;
        public static EffectClass EffectClass = EffectClass.SingleFrame;
        public static Functions Function = Functions.Polar;
        public static MultiFrameFunctions MFFunction = MultiFrameFunctions.PhotoFinish;
        public static bool ShowRendered = true;
        public static Bitmap PreviewBitmap;

        private const int UpdateTimeout = 500;
        private static Timer _scrollingTimer;
        private static BackgroundWorker _renderWorker = new BackgroundWorker();
        private static Progress _progressTimer = new Progress(1000); //1000 because ProgressVisualBar.Maximum = 1000

        private static readonly TextBox ChosenFile = new TextBox();
        private static readonly OpenFileDialog OpenFile = new OpenFileDialog();
        private static readonly Button OpenFileButton = new Button();
        private static readonly PictureBox PreviewPictureBox = new PictureBox();
        private static readonly Label PreviewLabel = new Label();
        private static readonly Label TimeLineLabel = new Label();
        private static readonly Label TimeLineStart = new Label();
        private static readonly Label TimeLineEnd = new Label();
        private static readonly TrackBar TimeLine = new TrackBar();
        private static readonly Label ProgressLabel = new Label();
        private static readonly Label ProgressTimeElapsed = new Label();
        private static readonly Label ProgressTimeRemaining = new Label();
        private static readonly Label ProgressPercent = new Label();
        private static readonly ProgressBar ProgressVisualBar = new ProgressBar();
        private static readonly Label EffectsPanelLabel = new Label();
        private static readonly ComboBox EpEffectClassChoose = new ComboBox();
        private static readonly ComboBox EpOneFrameFuncChoose = new ComboBox();
        private static readonly Label MultiplierFromLabel = new Label();
        private static readonly TextBox MultiplierFromTextBox = new TextBox();
        private static readonly Label MultiplierToLabel = new Label();
        private static readonly TextBox MultiplierToTextBox = new TextBox();
        private static readonly ComboBox EpMultiFrameFuncChoose = new ComboBox();
        private static readonly Button StartButton = new Button();
        private static readonly CheckBox ShowRenderedCheckBox = new CheckBox();
        public AppForm()
        {
            //
            //Main window settings
            //
            Size = new Size(800, 450);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Text = "UVEC"; //???
            Icon = new Icon(@"mmfiles\icon.ico");
            
            //
            //Top subspace of main window
            //
            ChosenFile.Enabled = false;
            ChosenFile.Location = new Point(10, 10);
            ChosenFile.Size = new Size(ClientSize.Width - 5 - 30 - 20,// 10*2 - отступ справа и слева
                Size.Height);
            ChosenFile.TextAlign = HorizontalAlignment.Center;
            ChosenFile.Text = "Choose file ->";
            ChosenFile.ReadOnly = true;

            OpenFile.Title = "Open file";
            OpenFile.Filter = "Video Files|*.mp4;*.avi";
            OpenFile.RestoreDirectory = true;

            OpenFileButton.Size = new Size(30, ChosenFile.Size.Height + 2);
            OpenFileButton.Text = "...";
            OpenFileButton.Location = new Point(ChosenFile.Right + 5, ChosenFile.Top - 1);
            
            Controls.Add(ChosenFile);
            Controls.Add(OpenFileButton);
            
            //
            //Left middle subspace
            //
            PreviewPictureBox.Location = new Point(10, ChosenFile.Bottom + 10);
            PreviewPictureBox.Size =
                new Size(
                    (ClientRectangle.Height - ChosenFile.Bottom + 10 - 100) / 9 * 16, // /9 & *16 - соотношение сторон 16 на 9 
                    ClientRectangle.Height - ChosenFile.Bottom + 10 - 100); // 50 - отступ снизу
            PreviewPictureBox.Image = new Bitmap(PreviewPictureBox.Width, PreviewPictureBox.Height);
            using (Graphics g = Graphics.FromImage(PreviewPictureBox.Image)){g.Clear(Color.Black);} //заливка черным

            PreviewLabel.Text = "Preview";
            PreviewLabel.ForeColor = Color.White;
            PreviewLabel.BackColor = Color.Transparent;
            
            Controls.Add(PreviewPictureBox);
            PreviewPictureBox.Controls.Add(PreviewLabel);
            
            //
            //Left bottom subspace
            //
            TimeLineLabel.Text = "Time Line";
            TimeLineLabel.BackColor = Color.Transparent;
            TimeLineLabel.TextAlign = ContentAlignment.TopCenter;
            TimeLineLabel.Location = new Point(10, PreviewPictureBox.Bottom + 10);
            TimeLineLabel.Size = new Size(PreviewPictureBox.Width, ClientSize.Height - PreviewPictureBox.Bottom - 10 - 10);

            TimeLineStart.Location = new Point(0, 0);
            TimeLineStart.BackColor = Color.Transparent;
            TimeLineStart.Text = "00.00";

            TimeLineEnd.Location = new Point(TimeLineLabel.Width - 100, 0); //100 - отступ справа
            TimeLineEnd.BackColor = Color.Transparent;
            TimeLineEnd.Text = "00.00";
            TimeLineEnd.TextAlign = ContentAlignment.TopRight;

            TimeLine.TickStyle = TickStyle.Both;
            TimeLine.TickFrequency = 5;
            TimeLine.Maximum = 100;
            TimeLine.Minimum = 0;
            TimeLine.Location = new Point(0, 20);
            TimeLine.Width = TimeLineLabel.Width;
            TimeLine.Enabled = false;
            
            Controls.Add(TimeLineLabel);
            TimeLineLabel.Controls.Add(TimeLine);
            TimeLineLabel.Controls.Add(TimeLineStart);
            TimeLineLabel.Controls.Add(TimeLineEnd);
            
            //
            //Progress second layout
            //
            ProgressLabel.Text = "Progress";
            ProgressLabel.BackColor = Color.Transparent;
            ProgressLabel.TextAlign = ContentAlignment.TopLeft;
            ProgressLabel.Location = new Point(10, PreviewPictureBox.Bottom + 10);
            ProgressLabel.Size = new Size(PreviewPictureBox.Width, ClientSize.Height - PreviewPictureBox.Bottom - 10 - 5);

            ProgressVisualBar.Location = new Point(0, 25);
            ProgressVisualBar.Width = ProgressLabel.Width;
            ProgressVisualBar.Maximum = 1000;
            ProgressVisualBar.Minimum = 0;
            
            ProgressTimeRemaining.Location = new Point(ProgressVisualBar.Right - 150, ProgressVisualBar.Top - 15);
            ProgressTimeRemaining.BackColor = Color.Transparent;
            ProgressTimeRemaining.Size = new Size(150, 15);
            ProgressTimeRemaining.Text = "Time remaining: 00:00:00.00";

            ProgressTimeElapsed.Location = new Point(ProgressTimeRemaining.Left - 10 - 150, ProgressTimeRemaining.Top);
            ProgressTimeElapsed.BackColor = Color.Transparent;
            ProgressTimeElapsed.Size = ProgressTimeRemaining.Size;
            ProgressTimeElapsed.Text = "Time elapsed: 00:00:00.00";

            ProgressPercent.Location = new Point(ProgressVisualBar.Right - 145, ProgressVisualBar.Bottom);
            ProgressPercent.BackColor = Color.Transparent;
            ProgressPercent.Text = "Percent completed: 0%";
            ProgressPercent.Size = new Size(145, 15);
            
            ProgressLabel.Controls.Add(ProgressVisualBar);
            ProgressLabel.Controls.Add(ProgressTimeElapsed);
            ProgressLabel.Controls.Add(ProgressTimeRemaining);
            ProgressLabel.Controls.Add(ProgressPercent);
            
            //
            //Right subspace
            //
            EffectsPanelLabel.Location = new Point(PreviewPictureBox.Right + 10, PreviewPictureBox.Top);
            EffectsPanelLabel.Size = new Size(ClientSize.Width - PreviewPictureBox.Right - 10 - 10,
                    ClientSize.Height-PreviewPictureBox.Top-10-10);//10 - отступы
            EffectsPanelLabel.BackColor = Color.Transparent;
            EffectsPanelLabel.TextAlign = ContentAlignment.TopCenter;
            EffectsPanelLabel.Text = "Effects Control";

            EpEffectClassChoose.Location = new Point(10, 20);
            EpEffectClassChoose.Width = (EffectsPanelLabel.Width - 30) / 2;
            EpEffectClassChoose.DataSource = Enum.GetValues(typeof(EffectClass));

            //
            //One frame settings
            EpOneFrameFuncChoose.Location = new Point(EpEffectClassChoose.Right + 10, 20);
            EpOneFrameFuncChoose.Width = EpEffectClassChoose.Width;
            EpOneFrameFuncChoose.DataSource = Enum.GetValues(typeof(Functions));

            MultiplierFromLabel.Location = new Point(EpEffectClassChoose.Left, EpEffectClassChoose.Bottom + 10);
            MultiplierFromLabel.Size = new Size(EffectsPanelLabel.Width - 20, 20);
            MultiplierFromLabel.Text = "Multiplier from:";
            MultiplierFromLabel.TextAlign = ContentAlignment.MiddleLeft;

            MultiplierFromTextBox.Location = new Point(EpEffectClassChoose.Width - 20, 0);
            MultiplierFromTextBox.Width = EpEffectClassChoose.Width + 30;
            MultiplierFromTextBox.Text = "0.0";
            MultiplierFromTextBox.BackColor = Color.White;

            MultiplierToLabel.Location = new Point(EpEffectClassChoose.Left, MultiplierFromLabel.Bottom + 10);
            MultiplierToLabel.Size = new Size(EffectsPanelLabel.Width - 20, 20);
            MultiplierToLabel.Text = "Multiplier to:";
            MultiplierToLabel.TextAlign = ContentAlignment.MiddleLeft;

            MultiplierToTextBox.Location = new Point(EpEffectClassChoose.Width - 20, 0);
            MultiplierToTextBox.Width = EpEffectClassChoose.Width + 30;
            MultiplierToTextBox.Text = "0.0";
            MultiplierToTextBox.BackColor = Color.White;

            //
            //Multi frame settings
            EpMultiFrameFuncChoose.Location = new Point(EpEffectClassChoose.Right + 10, 20);
            EpMultiFrameFuncChoose.Width = EpEffectClassChoose.Width;
            EpMultiFrameFuncChoose.DataSource = Enum.GetValues(typeof(MultiFrameFunctions));

            //
            //Start
            StartButton.Location = new Point(EffectsPanelLabel.Width - 70, EffectsPanelLabel.Height - 30);
            StartButton.Size = new Size(60, 30);
            StartButton.Text = "Start";
            StartButton.Enabled = false;

            _renderWorker.WorkerReportsProgress = true;

            ShowRenderedCheckBox.Location = new Point(10, EffectsPanelLabel.Height - 27);
            ShowRenderedCheckBox.Text = "Show rendered(slower)";
            ShowRenderedCheckBox.Checked = ShowRendered;
            ShowRenderedCheckBox.Width = 140;
            
            Controls.Add(EffectsPanelLabel);
            EffectsPanelLabel.Controls.Add(EpEffectClassChoose);
            EffectsPanelLabel.Controls.Add(EpOneFrameFuncChoose);
            EffectsPanelLabel.Controls.Add(MultiplierFromLabel);
            MultiplierFromLabel.Controls.Add(MultiplierFromTextBox);
            EffectsPanelLabel.Controls.Add(MultiplierToLabel);
            MultiplierToLabel.Controls.Add(MultiplierToTextBox);
            EffectsPanelLabel.Controls.Add(StartButton);
            EffectsPanelLabel.Controls.Add(ShowRenderedCheckBox);
            //effectsPanelLabel.Controls.Add(epMultiFrameFuncChoose);

            //
            //Handlers
            //
            //Top
            //
            OpenFileButton.Click += (sender, args) => { OpenFileButtonHandler(); };
            
            //
            //Left bottom subspace
            //
            TimeLine.ValueChanged += (sender, args) => { TimeLineHandler(); };
            
            //
            //Right subspace
            //
            EpEffectClassChoose.SelectionChangeCommitted += (sender, args) => { EpEffectClassChooseHandler(); };
            
            //
            //One frame settings
            EpOneFrameFuncChoose.SelectionChangeCommitted += (sender, args) =>
            {
                Function = (Functions)EpOneFrameFuncChoose.SelectedItem;
                Distorter.ResetMaxMinValues();
                if (StartButton.Enabled)
                {
                    DrawImage(PreviewPictureBox, Distorter.RunOneFrame(Reader, TimeLine.Value,
                        MultiplierFrom, MultiplierTo, Function, PreviewPictureBox.Width, PreviewPictureBox.Height));
                }
            };
            MultiplierFromTextBox.TextChanged += (sender, args) => { MultiplierTextBoxHandler(MultiplierFromTextBox, ref MultiplierFrom); };
            MultiplierToTextBox.TextChanged += (sender, args) => { MultiplierTextBoxHandler(MultiplierToTextBox, ref MultiplierTo); };
            
            //
            //Multi frame settings
            EpMultiFrameFuncChoose.SelectionChangeCommitted += (sender, args) =>
            {
                MFFunction = (MultiFrameFunctions)EpMultiFrameFuncChoose.SelectedItem;
            };
            
            //
            //Start
            ShowRenderedCheckBox.CheckedChanged += (sender, args) => { ShowRendered = ShowRenderedCheckBox.Checked; };
            _renderWorker.DoWork += (sender, args) =>
            {
                if (EffectClass == EffectClass.SingleFrame)
                {
                    OneFrameDistorter.Run(Reader, Writer, MultiplierFrom, MultiplierTo, Function, _renderWorker);                    
                }
                else if (EffectClass == EffectClass.MultiFrame)
                {
                    MultiFrameDistorter.Run(Reader, Writer, MFFunction, _renderWorker);
                }
            };
            _renderWorker.ProgressChanged += (sender, args) =>
            {
                if (ShowRendered)
                {
                    DrawImage(PreviewPictureBox, PreviewBitmap);
                }
                ProgressVisualBar.Value = args.ProgressPercentage;
                ProgressPercent.Text = $"Percent completed: {args.ProgressPercentage/10.0}%";
                var (timeRemaining, timeElapsed) = _progressTimer.GetTimeElapsed(args.ProgressPercentage);
                ProgressTimeRemaining.Text = $"Time remaining: {timeRemaining:hh\\:mm\\:ss\\.ff}";
                ProgressTimeElapsed.Text = $"Time elapsed: {timeElapsed:hh\\:mm\\:ss\\.ff}";
            };
            _renderWorker.RunWorkerCompleted += (sender, args) =>
            {
                Writer.Close();
                Writer.Dispose();
                Writer = new VideoFileWriter();
                Writer.BitRate = Reader.BitRate;        //конфигурируем writer
                Writer.VideoCodec = VideoCodec.Mpeg4; //Reader videoCodec
                Writer.FrameRate = Reader.FrameRate;
                Writer.Height = Reader.Height;
                Writer.Width = Reader.Width;
                Writer.Open(GetNameToSave(VideoPath, VideoName, VideoExtension));
                OpenFileButton.Enabled = true;
                EpEffectClassChoose.Enabled = true;
                EpMultiFrameFuncChoose.Enabled = true;
                EpOneFrameFuncChoose.Enabled = true;
                MultiplierFromTextBox.Enabled = true;
                MultiplierToTextBox.Enabled = true;
                ShowRenderedCheckBox.Enabled = true;
                StartButton.Enabled = true;
                _progressTimer.ResetAverageTime();
                ProgressVisualBar.Value = 0;
                ProgressTimeRemaining.Text = "Time remaining: 00:00:00.00";
                ProgressTimeElapsed.Text = "Time elapsed: 00:00:00.00";
                ProgressPercent.Text = "Percent completed: 0%";
                Controls.Remove(ProgressLabel);
                Controls.Add(TimeLineLabel);
            };
            StartButton.Click += (sender, args) =>
            {
                OpenFileButton.Enabled = false;
                EpEffectClassChoose.Enabled = false;
                EpMultiFrameFuncChoose.Enabled = false;
                EpOneFrameFuncChoose.Enabled = false;
                MultiplierFromTextBox.Enabled = false;
                MultiplierToTextBox.Enabled = false;
                ShowRenderedCheckBox.Enabled = false;
                StartButton.Enabled = false;
                Controls.Remove(TimeLineLabel);
                Controls.Add(ProgressLabel);
                _renderWorker.RunWorkerAsync();
            };
        }

        private static string GetNameToSave(string videoPath, string videoName, string extenstion)
        {
            var counter = 0;
            var res = videoPath + videoName + "_edited" + counter + ".avi";//extenstion;
            while (File.Exists(res))
            {
                res = videoPath + videoName + "_edited" + ++counter + ".avi";//extenstion;
            }
            return res;
        }
        private static void DrawImage(PictureBox pictureBox, Bitmap image)
        {
            var res = new Bitmap(pictureBox.Width, pictureBox.Height);
            var graphics = Graphics.FromImage(res);
            graphics.DrawImage(FastUtils.ScaleImage(image, pictureBox.Width, pictureBox.Height, false, true), 0, 0);
            pictureBox.Image = res;
        }

        private static void EnableStartButton(Button startButton, TrackBar timeline)
        {
            if (EffectClass == EffectClass.SingleFrame)
            {
                startButton.Enabled = MultiplierFromTextBox.BackColor == Color.White &&
                                      MultiplierToTextBox.BackColor == Color.White && Reader.IsOpen;
            }
            else if (EffectClass == EffectClass.MultiFrame)
            {
                startButton.Enabled = true;
            }
            timeline.Enabled = startButton.Enabled;
        }

        private static void OpenFileButtonHandler()
        {
            if (OpenFile.ShowDialog() != DialogResult.OK) return;
            Reader.Close(); // close prev videos
            Reader.Dispose();
            Reader = new VideoFileReader();
            Writer.Close();
            Writer.Dispose();
            Writer = new VideoFileWriter();
                
            VideoName = (""+OpenFile.SafeFileName).Replace(""+Path.GetExtension(OpenFile.FileName), "");
            VideoPath = Path.GetDirectoryName(OpenFile.FileName)+@"\";//задаем название и путь к файлу
            VideoExtension = Path.GetExtension(OpenFile.FileName);
            ChosenFile.Text = OpenFile.FileName;
                
            Reader.Open(OpenFile.FileName); //открываем файл
                
            Writer.BitRate = Reader.BitRate;        //конфигурируем writer
            Writer.VideoCodec = VideoCodec.Mpeg4; //Reader videoCodec
            Writer.FrameRate = Reader.FrameRate;
            Writer.Height = Reader.Height;
            Writer.Width = Reader.Width;
            Writer.Open(GetNameToSave(VideoPath, VideoName, VideoExtension));
                
            TimeLine.Value = 0;                                                        //настраиваем временную линию
            TimeLine.Maximum = (int)Reader.FrameCount;
            TimeLine.TickFrequency = FastUtils.FastRoundInt((double)Reader.FrameRate);
            TimeLineEnd.Text = TimeSpan.FromSeconds(Reader.FrameCount / (double)Reader.FrameRate).ToString(@"hh\:mm\:ss\.ff");
                
            Distorter.ResetMaxMinValues();
            DrawImage(PreviewPictureBox,            //if enable start button? because multiplierFrom can be bad
                Distorter.RunOneFrame(Reader, 0, MultiplierFrom, MultiplierTo, Function
                    , PreviewPictureBox.Width, PreviewPictureBox.Height)); //рисуем 0 кадр
                
            EnableStartButton(StartButton, TimeLine); //пробуем включить кнопку старт
        }

        private static void TimeLineHandler()
        {
            if (_scrollingTimer != null) return;
            _scrollingTimer = new Timer 
            {
                Enabled = false,
                Interval = UpdateTimeout,
                Tag = TimeLine.Value
            };
            _scrollingTimer.Tick += (s, ea) =>
            {
                if (TimeLine.Value == (int)_scrollingTimer.Tag)
                {
                    _scrollingTimer.Stop();
                        
                    DrawImage(PreviewPictureBox, Distorter.RunOneFrame(Reader, TimeLine.Value,
                        MultiplierFrom, MultiplierTo, Function, PreviewPictureBox.Width, PreviewPictureBox.Height));

                    _scrollingTimer.Dispose();
                    _scrollingTimer = null;
                }
                else
                {
                    _scrollingTimer.Tag = TimeLine.Value;
                }
            };
            _scrollingTimer.Start();
        }

        private static void EpEffectClassChooseHandler()
        {
            if ((EffectClass)EpEffectClassChoose.SelectedItem == EffectClass.SingleFrame)
            {
                EffectsPanelLabel.Controls.Remove(EpMultiFrameFuncChoose);//убираем multiframe menu
                    
                EffectsPanelLabel.Controls.Add(EpOneFrameFuncChoose);
                EffectsPanelLabel.Controls.Add(MultiplierFromLabel);
                EffectsPanelLabel.Controls.Add(MultiplierToLabel);
            }
            else if ((EffectClass)EpEffectClassChoose.SelectedItem == EffectClass.MultiFrame)
            {
                EffectsPanelLabel.Controls.Remove(EpOneFrameFuncChoose);
                EffectsPanelLabel.Controls.Remove(MultiplierFromLabel);
                EffectsPanelLabel.Controls.Remove(MultiplierToLabel);
                    
                EffectsPanelLabel.Controls.Add(EpMultiFrameFuncChoose);
            }
            EffectClass = (EffectClass) EpEffectClassChoose.SelectedItem;
        }

        private static void MultiplierTextBoxHandler(TextBox multiplierTextBox, ref double multiplier)
        {
            if (double.TryParse(multiplierTextBox.Text, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double res) && Math.Abs(res) < 1000) // ограничение в 1000
            {
                multiplier = res;
                Distorter.ResetMaxMinValues();
                multiplierTextBox.BackColor = Color.White;
            }
            else
            {
                multiplierTextBox.BackColor = Color.FromArgb(255, 170, 152);
            }
            EnableStartButton(StartButton, TimeLine);
            if (StartButton.Enabled)
            {
                DrawImage(PreviewPictureBox, Distorter.RunOneFrame(Reader, TimeLine.Value,
                    MultiplierFrom, MultiplierTo, Function, PreviewPictureBox.Width, PreviewPictureBox.Height));
            }
        }
    }
}