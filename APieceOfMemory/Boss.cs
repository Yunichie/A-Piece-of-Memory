namespace APieceOfMemory
{
    public class Boss
    {
        public PointF Position { get; set; }
        public Size Size { get; private set; }
        public Color Color { get; set; }
        public float BaseSpeed { get; private set; } // For entry
        public int Health { get; set; }
        public int MaxHealth { get; private set; }

        private float entryTargetX;
        private bool hasEntered;
        private float verticalTargetY;
        private float verticalMoveSpeed;
        private Random randomGen;
        private float screenMinY = 0;
        private float screenMaxY = 0;

        public RectangleF Bounds => new RectangleF(Position, Size);

        public Boss(float x, float y, int size, Color color, float speed, int health, float clientWidth, float clientHeight)
        {
            Position = new PointF(x, y); 
            Size = new Size(size * 2, size * 2);
            Color = color;
            BaseSpeed = speed; 
            MaxHealth = health;
            Health = health;
            
            this.randomGen = new Random();
            this.entryTargetX = clientWidth * 0.75f; 
            this.hasEntered = false;
            
            this.screenMinY = clientHeight * 0.1f; 
            this.screenMaxY = clientHeight * 0.9f - Size.Height; 

            this.verticalTargetY = y;
            this.verticalMoveSpeed = BaseSpeed * 0.5f; 
            
            SetNewVerticalTarget(); 
        }

        private void SetNewVerticalTarget()
        {
            verticalTargetY = randomGen.Next((int)screenMinY, (int)screenMaxY);
        }

        public void Update(float clientWidth, float clientHeight)
        {
            if (!hasEntered)
            {
                Position = new PointF(Position.X - BaseSpeed, Position.Y);
                
                if (Position.X <= entryTargetX)
                {
                    Position = new PointF(entryTargetX, Position.Y); // Snap to final X position
                    hasEntered = true;
                    SetNewVerticalTarget();
                }
            }
            else
            {
                if (Math.Abs(Position.Y - verticalTargetY) < verticalMoveSpeed)
                {
                    Position = new PointF(Position.X, verticalTargetY); // Snap to target
                    SetNewVerticalTarget(); // Pick a new random Y target
                }
                else if (Position.Y < verticalTargetY)
                {
                    Position = new PointF(Position.X, Position.Y + verticalMoveSpeed);
                }
                else if (Position.Y > verticalTargetY)
                {
                    Position = new PointF(Position.X, Position.Y - verticalMoveSpeed);
                }

                Position = new PointF(Position.X, Math.Max(screenMinY, Math.Min(Position.Y, screenMaxY)));
            }
        }

        public void Draw(Graphics g)
        {
            Image spriteToUse = AnimatedSpriteManager.BossSprite?.CurrentFrameImage;

            if (spriteToUse != null)
            {
                g.DrawImage(spriteToUse, Bounds);
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(Color))
                {
                    g.FillEllipse(brush, Bounds);
                }

                float healthBarWidth = Size.Width;
                float healthBarHeight = 10;
                float healthBarX = Position.X;
                float healthBarY = Position.Y - healthBarHeight - 5;
                if (healthBarY < 0) healthBarY = Position.Y + Size.Height + 5;

                float currentHealthWidth = healthBarWidth * ((float)Health / MaxHealth);
                if (currentHealthWidth < 0) currentHealthWidth = 0;


                using (SolidBrush backBrush = new SolidBrush(Color.FromArgb(150, Color.DarkRed)))
                {
                    g.FillRectangle(backBrush, healthBarX, healthBarY, healthBarWidth, healthBarHeight);
                }
                using (SolidBrush frontBrush = new SolidBrush(Color.FromArgb(200, Color.LimeGreen)))
                {
                    g.FillRectangle(frontBrush, healthBarX, healthBarY, currentHealthWidth, healthBarHeight);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(180, Color.White)))
                {
                    g.DrawRectangle(borderPen, healthBarX, healthBarY, healthBarWidth, healthBarHeight);
                }
            }
            
        }
    }
}