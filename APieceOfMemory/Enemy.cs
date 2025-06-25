namespace APieceOfMemory
{
    public enum EnemyType { Slow, Fast, Faster }

    public class Enemy
    {
        public PointF Position { get; set; }
        public Size Size { get; private set; }
        public Color Color { get; set; }
        public float Speed { get; set; }
        public EnemyType Type { get; private set; }
        public int Health { get; set; }

        public RectangleF Bounds => new RectangleF(Position, Size);

        public Enemy(float x, float y, Image sprite, EnemyType type, int initialHealth = 1)
        {
            Position = new PointF(x, y);
            Type = type;
            Health = initialHealth;
            float baseSpeedMagnitude = 0f;

            switch (type)
            {
                case EnemyType.Slow:
                    Size = new Size(sprite.Size.Width - 150, sprite.Size.Height - 150);
                    Color = Color.FromArgb(220, 20, 60); // Crimson
                    // baseSpeedMagnitude = 1.5f;
                    baseSpeedMagnitude = 1.5f;
                    break;
                case EnemyType.Fast:
                    Size = new Size(sprite.Size.Width - 140, sprite.Size.Height - 140); 
                    Color = Color.FromArgb(255, 140, 0); // DarkOrange
                    baseSpeedMagnitude = 2.5f;
                    break;
                case EnemyType.Faster:
                    Size = new Size(sprite.Size.Width - 130, sprite.Size.Height - 130); 
                    Color = Color.FromArgb(255, 215, 0); // Gold
                    baseSpeedMagnitude = 3.5f;
                    break;
                default:
                    Size = new Size(sprite.Size.Width, sprite.Size.Height);
                    Color = Color.Gray;
                    baseSpeedMagnitude = 1.0f;
                    break;
            }
            this.Speed = baseSpeedMagnitude;
        }

        public void Update(PointF targetPosition)
        {
            Position = new PointF(Position.X + this.Speed, Position.Y); 
        }

        public void Draw(Graphics g)
        {
            Image spriteToUse = AnimatedSpriteManager.EnemySprite?.CurrentFrameImage;

            if (spriteToUse != null)
            {
                g.DrawImage(spriteToUse, Bounds);
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(this.Color))
                {
                    g.FillRectangle(brush, this.Bounds);
                }
            }
        }
    }
}