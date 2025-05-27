using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using System.IO;
using NAudio.Wave;

namespace MultiMediaProject
{
    public partial class Form1 : Form
    {
        private clsGameManager gameManager = new clsGameManager();
        private Bitmap originalImage1, originalImage2, originalImage2Clean;
        private bool gameStarted = false;
        private List<Point> foundPoints = new List<Point>();

      
        private string correctSoundPath = "C:/Users/User/source/repos/MultiMediaProject/MultiMediaProject/Resources/correct.wav";
        private string wrongSoundPath = "C:/Users/User/source/repos/MultiMediaProject/MultiMediaProject/Resources/wrong.wav";

        public Form1()
        {
            InitializeComponent();

            try
            {
                string imagePath1 = "C:/Users/User/source/repos/MultiMediaProject/MultiMediaProject/Resources/spot_difference_image_1.jpg";
                string imagePath2 = "C:/Users/User/source/repos/MultiMediaProject/MultiMediaProject/Resources/spot_difference_image_2.jpg";

                if (!File.Exists(imagePath1))
                    throw new FileNotFoundException("Undefined Image 1");
                if (!File.Exists(imagePath2))
                    throw new FileNotFoundException("Undefined Image 2");

             

                originalImage1 = new Bitmap(imagePath1);
                originalImage2 = new Bitmap(imagePath2);
                originalImage2Clean = new Bitmap(originalImage2);

                pictureBox1.Image = originalImage1;
                pictureBox2.Image = originalImage2;

                pictureBox2.MouseClick += pictureBox_MouseClick;
                btnStart.Click += btnStart_Click;
                timer1.Tick += timer1_Tick;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error While Loading Image: {ex.Message}");
            }
            cbMode.SelectedIndex = 0;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cbMode.Text == "Easy")
            {
               gameManager.InitializeManualDifferences();
                gameManager.TimeLeft = 6000;
                gameManager.RemainingAttempts = 20;
            }
            else if (cbMode.Text == "Medium")
            {
                gameManager.InitializeManualDifferences();
                gameManager.TimeLeft = 4000;
                gameManager.RemainingAttempts = 15;
            }
            else if (cbMode.Text == "Hard")
            {
                gameManager.InitializeManualDifferences();
                gameManager.TimeLeft = 2000;
                gameManager.RemainingAttempts = 10;
            }
            else
            {
                MessageBox.Show("Please select a difficulty mode.");
                return;
            }

            gameManager.FoundDifferences = 0;
            foundPoints.Clear();
            pictureBox2.Image = new Bitmap(originalImage2Clean);
            gameStarted = true;
            timer1.Start();
            UpdateStats();
            btnStart.Enabled = false;
            cbMode.Enabled = false;
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (!gameStarted)
            {
                MessageBox.Show("Please start the game first.");
                return;
            }

            PictureBox pb = (PictureBox)sender;
            Point clickPoint = pb.PointToClient(Cursor.Position);

            bool found = false;
            foreach (Point diff in gameManager.Differences)
            {
                if (foundPoints.Any(fp => Math.Abs(fp.X - diff.X) < gameManager.Tolerance &&
                                          Math.Abs(fp.Y - diff.Y) < gameManager.Tolerance))
                {
                    continue;
                }

                if (Math.Abs(clickPoint.X - diff.X) < gameManager.Tolerance &&
                    Math.Abs(clickPoint.Y - diff.Y) < gameManager.Tolerance)
                {
                    using (Graphics g = pb.CreateGraphics())
                    {
                        g.DrawEllipse(new Pen(Color.Green, 3), diff.X - 15, diff.Y - 15, 30, 30);
                    }

                    PlaySound(correctSoundPath); 

                    gameManager.FoundDifferences++;
                    foundPoints.Add(diff);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                using (Graphics g = pb.CreateGraphics())
                {
                    g.DrawEllipse(new Pen(Color.Red, 3), clickPoint.X - 10, clickPoint.Y - 10, 20, 20);
                }

                PlaySound(wrongSoundPath); 

                gameManager.RemainingAttempts--;
            }

            UpdateStats();

            if (gameManager.RemainingAttempts <= 0 || gameManager.FoundDifferences >= gameManager.TotalDifferences)
            {
                EndGame();
            }
        }

        private void UpdateStats()
        {
            lblDifferinces.Text = $"Discovered: {gameManager.FoundDifferences} / {gameManager.TotalDifferences}\nRemaining Attempts: {gameManager.RemainingAttempts}";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            gameManager.TimeLeft--;
            lblTimeLeft.Text = $"Time Left: {gameManager.TimeLeft / 100} seconds";

            if (gameManager.TimeLeft <= 0)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            timer1.Stop();
            gameStarted = false;
            string result = gameManager.FoundDifferences >= gameManager.TotalDifferences ? "Win!" : "Lose!";
            MessageBox.Show(result);
            cbMode.Enabled = true;
            btnStart.Enabled = true;
        }

        private void PlaySound(string filePath)
        {
            try
            {
                using (var audioFile = new AudioFileReader(filePath))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing sound: " + ex.Message);
            }
        }
    }
}
