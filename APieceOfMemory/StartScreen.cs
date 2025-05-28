using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace APieceOfMemory 
{
    // Start Screen Form
    public partial class StartScreen : Form
    {
        private System.Windows.Forms.Timer animationTimer;
        private float animationTime = 0f;
        private Button startButton;
        private Button exitButton;
        private Label titleLabel; 

        public StartScreen()
        {
            InitializeComponent();
            SetupStartScreen();
        }
        private new void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // StartScreen
            // 
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Name = "StartScreen";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "A Piece of Memory";
            this.BackColor = Color.FromArgb(135, 206, 235); // Sky blue
            this.DoubleBuffered = true;
            this.ResumeLayout(false); // Resume layout
        }

        private void SetupStartScreen()
        {
            // Title Label
            titleLabel = new Label();
            titleLabel.Text = "A Piece of Memory";
            titleLabel.Font = new Font("Georgia", 36, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.AutoSize = true;
            this.Controls.Add(titleLabel);

            // Subtitle
            var subtitleLabel = new Label();
            subtitleLabel.Text = "Nurture Life, Protect Memories";
            subtitleLabel.Font = new Font("Georgia", 14, FontStyle.Italic);
            subtitleLabel.ForeColor = Color.FromArgb(255, 255, 224); // LightYellow
            subtitleLabel.BackColor = Color.Transparent;
            subtitleLabel.AutoSize = true;
            this.Controls.Add(subtitleLabel);

            // Start Button
            startButton = new Button();
            startButton.Text = "🌱 Start Game";
            startButton.Font = new Font("Arial", 16, FontStyle.Bold);
            startButton.Size = new Size(200, 60); // Slightly larger button
            startButton.Location = new Point((this.ClientSize.Width - startButton.Width) / 2, 300); // Centered
            startButton.BackColor = Color.FromArgb(34, 139, 34); // ForestGreen
            startButton.ForeColor = Color.White;
            startButton.FlatStyle = FlatStyle.Flat;
            startButton.FlatAppearance.BorderSize = 1;
            startButton.FlatAppearance.BorderColor = Color.FromArgb(100, Color.White); // Subtle border
            startButton.Cursor = Cursors.Hand;
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

            // Exit Button
            exitButton = new Button();
            exitButton.Text = "Exit";
            exitButton.Font = new Font("Arial", 12);
            exitButton.Size = new Size(120, 40); // Slightly larger
            exitButton.Location = new Point((this.ClientSize.Width - exitButton.Width) / 2, startButton.Location.Y + startButton.Height + 20); // Centered below start
            exitButton.BackColor = Color.FromArgb(200, 20, 60); // Crimson
            exitButton.ForeColor = Color.White;
            exitButton.FlatStyle = FlatStyle.Flat;
            exitButton.FlatAppearance.BorderSize = 1;
            exitButton.FlatAppearance.BorderColor = Color.FromArgb(100, Color.White);
            exitButton.Cursor = Cursors.Hand;
            exitButton.Click += (s, e) => Application.Exit();
            this.Controls.Add(exitButton);
            
            titleLabel.Location = new Point((this.ClientSize.Width - titleLabel.Width) / 2, 150);
            subtitleLabel.Location = new Point((this.ClientSize.Width - subtitleLabel.Width) / 2, titleLabel.Bottom + 10);

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 50;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationTime += 0.1f;
            int offset = (int)(Math.Sin(animationTime * 1.5f) * 5);
            titleLabel.Location = new Point(titleLabel.Left, 150 + offset);

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw floating particles (simple version)
            var particleRandom = new Random(123);
            for (int i = 0; i < 30; i++)
            {
                float particleX = (particleRandom.Next(0, this.Width) + animationTime * 10f * (particleRandom.Next(1,3)-1.5f)) % this.Width;
                float particleY = (this.Height - ((animationTime * (20 + particleRandom.Next(0,20))) + particleRandom.Next(0, this.Height)) % this.Height) ;
                 if (particleX < 0) particleX += this.Width;


                using (var brush = new SolidBrush(Color.FromArgb(particleRandom.Next(50, 150), Color.WhiteSmoke)))
                {
                    g.FillEllipse(brush, particleX, particleY, particleRandom.Next(2,5), particleRandom.Next(2,5));
                }
            }

            using (var plantBrushDark = new SolidBrush(Color.DarkGreen)) 
            using (var plantBrushLight = new SolidBrush(Color.ForestGreen)) 
            {
                Random drawingRandom = (particleRandom != null) ? particleRandom : new Random();

                for (int i = -30; i < this.ClientSize.Width; i += 30)
                {
                    int h1 = drawingRandom.Next(20, 40);
                    int h2 = drawingRandom.Next(15, 35);

                    float y1 = this.ClientSize.Height - h1;
                    float y2 = this.ClientSize.Height - h2;

                    // X-coordinates with sway
                    float x1_offset = (float)Math.Sin(animationTime / 2f + i / 50f) * 5;
                    float x2_offset = (float)Math.Cos(animationTime / 2f + i / 40f) * 5;

                    g.FillEllipse(plantBrushDark, i + x1_offset, y1, 15, h1);
                    g.FillEllipse(plantBrushLight, i + 10 + x2_offset, y2, 12, h2);
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            animationTimer.Stop();
            animationTimer.Dispose();

            var gameForm = new GameForm();
            gameForm.FormClosed += (s2, e2) => this.Close();
            gameForm.Show();
            this.Hide();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (Application.OpenForms.Count == 0)
            {
                Application.Exit();
            }
            else if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
}