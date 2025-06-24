using System.Drawing;

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

        public Player(float x, float y, Image sprite, Color color, int speed)
        {
            Position = new PointF(x, y);
            Size = new Size(sprite.Size.Width - 50, sprite.Size.Height - 50);
            Color = color;
            Speed = speed;
            CanMoveFreely = false;
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
            Image SpriteToUse = AnimatedSpriteManager.PlayerSprite?.CurrentFrameImage;
            
            if (SpriteToUse != null) {
                g.DrawImage(SpriteToUse, Bounds);
            } else {
                using (SolidBrush brush = new SolidBrush(Color))
                {
                    g.FillEllipse(brush, Bounds);
                }
            }
        }
    }
}