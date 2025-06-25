namespace APieceOfMemory
{
    public class Player
    {
        public PointF Position { get; set; }
        public Size Size { get; private set; }
        public Color Color { get; set; }
        public int Speed { get; set; }
        public bool CanMoveFreely { get; set; }

        public RectangleF Bounds => new RectangleF(Position, Size);
        
        private SpriteManager currentSprite;
        private int shootingDuration = 500; // ms
        private DateTime? shootingStartTime = null;

        public void TriggerShootAnimation()
        {
            if (shootingStartTime.HasValue) return;
            
            shootingStartTime = DateTime.Now;
            currentSprite = AnimatedSpriteManager.PlayerShootingSprite;
        }

        public void Update()
        {
            if (shootingStartTime.HasValue)
            {
                var elapsed = (DateTime.Now - shootingStartTime.Value).TotalMilliseconds;
                
                if (elapsed >= shootingDuration)
                {
                    shootingStartTime = null;
                    currentSprite = AnimatedSpriteManager.PlayerSprite;
                }
            }
        }

        public Player(float x, float y, Image sprite, Color color, int speed)
        {
            Position = new PointF(x, y);
            Size = new Size(sprite.Size.Width - 125, sprite.Size.Height - 125);
            Color = color;
            Speed = speed;
            CanMoveFreely = false;
            
            currentSprite = AnimatedSpriteManager.PlayerSprite;
        }

        public void Move(float dx, float dy, Rectangle clientBounds)
        {
            PointF newPosition = new PointF(Position.X + dx, Position.Y + dy);

            // Keep player within bounds
            if (newPosition.X < 0) newPosition.X = 0;
            if (newPosition.Y < 0) newPosition.Y = 0;
            if (newPosition.X + Size.Width > clientBounds.Width) newPosition.X = clientBounds.Width - Size.Width;
            if (newPosition.Y + Size.Height > clientBounds.Height) newPosition.Y = clientBounds.Height - Size.Height;

            Position = newPosition;
        }

        public void Draw(Graphics g)
        {
            Image SpriteToUse = currentSprite?.CurrentFrameImage;
            
            if (SpriteToUse != null) 
            {
                g.DrawImage(SpriteToUse, Bounds);
            } 
            else 
            {
                using (SolidBrush brush = new SolidBrush(Color))
                {
                    g.FillEllipse(brush, Bounds);
                }
            }
        }
    }
}