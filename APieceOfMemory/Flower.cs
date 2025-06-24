using System.Drawing;

namespace APieceOfMemory
{
    public enum FlowerState { Healthy, Damaged, Broken, Dead }

    public class Flower
    {
        public PointF Position { get; private set; }
        public Size Size { get; private set; }
        public FlowerState State { get; set; }

        public RectangleF Bounds => new RectangleF(Position, Size);

        public Flower(float x, float y, int size)
        {
            Position = new PointF(x, y);
            Size = new Size(size, size);
            State = FlowerState.Healthy;
        }

        public Color GetCurrentColor()
        {
            return State switch
            {
                FlowerState.Healthy => Color.FromArgb(144, 238, 144), // LightGreen
                FlowerState.Damaged => Color.FromArgb(255, 255, 150), // LightYellow
                FlowerState.Broken => Color.FromArgb(210, 180, 140),  // Tan
                FlowerState.Dead => Color.FromArgb(100, 100, 100),    // DarkGray
                _ => Color.Gray,
            };
        }

        public void Draw(Graphics g)
        {
            Image spriteToDraw = null;
            switch (State)
            {
                case FlowerState.Healthy: spriteToDraw = AnimatedSpriteManager.FlowerSprite?.CurrentFrameImage; break;
                case FlowerState.Damaged: spriteToDraw = AnimatedSpriteManager.FlowerSprite?.CurrentFrameImage; break;
                case FlowerState.Broken: spriteToDraw = AnimatedSpriteManager.FlowerSprite?.CurrentFrameImage; break;
                case FlowerState.Dead: spriteToDraw = AnimatedSpriteManager.FlowerSprite?.CurrentFrameImage; break;
            }
            if (spriteToDraw != null) {
                g.DrawImage(spriteToDraw, Bounds);
            } else {
            using (SolidBrush brush = new SolidBrush(GetCurrentColor()))
                {
                    float stemWidth = Size.Width / 5f;
                    float stemHeight = Size.Height * 0.6f;
                    RectangleF stemRect = new RectangleF(Position.X + Size.Width / 2f - stemWidth / 2f, Position.Y + Size.Height * 0.4f, stemWidth, stemHeight);
                    g.FillRectangle(brush, stemRect);

                    float headRadius = Size.Width / 2.5f;
                    RectangleF headRect = new RectangleF(Position.X + Size.Width / 2f - headRadius, Position.Y, headRadius * 2, headRadius * 2);
                    using (SolidBrush headBrush = new SolidBrush(State == FlowerState.Healthy ? Color.HotPink : State == FlowerState.Damaged ? Color.OrangeRed : State == FlowerState.Broken ? Color.SaddleBrown : Color.DarkSlateGray))
                    {
                        g.FillEllipse(headBrush, headRect);
                    }
                }
            }
        }

        public void TakeDamage()
        {
            if (State == FlowerState.Healthy) State = FlowerState.Damaged;
            else if (State == FlowerState.Damaged) State = FlowerState.Broken;
            else if (State == FlowerState.Broken) State = FlowerState.Dead;
        }
    }
}