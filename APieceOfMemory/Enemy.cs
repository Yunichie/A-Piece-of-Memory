using System.Drawing;

namespace APieceOfMemory
{
    public enum EnemyType { Slow, Fast, Faster } // Make sure this enum is accessible

    public class Enemy
    {
        public PointF Position { get; set; }
        public Size Size { get; private set; }
        public Color Color { get; set; }
        public float Speed { get; set; }
        public EnemyType Type { get; private set; }
        public int Health { get; set; }

        public RectangleF Bounds => new RectangleF(Position, Size);

        // Sprite placeholder:
        // public Image EnemySprite { get; set; }

        public Enemy(float x, float y, int baseSize, EnemyType type, int initialHealth = 1)
        {
            Position = new PointF(x, y);
            Type = type;
            Health = initialHealth;
            float baseSpeedMagnitude = 0f;

            switch (type)
            {
                case EnemyType.Slow:
                    Size = new Size(baseSize, baseSize);
                    Color = Color.FromArgb(220, 20, 60); // Crimson
                    // baseSpeedMagnitude = 1.5f;
                    baseSpeedMagnitude = 1.5f;
                    break;
                case EnemyType.Fast:
                    Size = new Size(baseSize - 2, baseSize - 2); 
                    Color = Color.FromArgb(255, 140, 0); // DarkOrange
                    baseSpeedMagnitude = 2.5f;
                    break;
                case EnemyType.Faster:
                    Size = new Size(baseSize - 4, baseSize - 4); 
                    Color = Color.FromArgb(255, 215, 0); // Gold
                    baseSpeedMagnitude = 3.5f;
                    break;
                default:
                    Size = new Size(baseSize, baseSize);
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
            // if (EnemySprite != null) {
            //     g.DrawImage(EnemySprite, Bounds);
            // } else {
            using (SolidBrush brush = new SolidBrush(this.Color))
            {
                g.FillRectangle(brush, this.Bounds);
            }
            // }
        }
    }
}