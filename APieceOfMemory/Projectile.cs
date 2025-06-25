namespace APieceOfMemory
{
    public enum ProjectileType { Player, Enemy } 

    public class Projectile
    {
        public PointF Position { get; set; }
        public Size Size { get; private set; }
        public Color Color { get; private set; }
        public PointF Velocity { get; private set; }
        public ProjectileType Type { get; private set; }

        public RectangleF Bounds => new RectangleF(Position, Size);

        public Projectile(float x, float y, int size, Color color, PointF velocity, ProjectileType type)
        {
            Position = new PointF(x, y);
            Size = new Size(size, size);
            Color = color;
            Velocity = velocity;
            Type = type;
        }

        public bool Update(Rectangle clientBounds)
        {
            Position = new PointF(Position.X + Velocity.X, Position.Y + Velocity.Y);

            // Check if projectile is off-screen
            if (Position.X < -Size.Width || Position.X > clientBounds.Width ||
                Position.Y < -Size.Height || Position.Y > clientBounds.Height)
            {
                return false;
            }
            return true; 
        }

        public void Draw(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color))
            {
                g.FillEllipse(brush, Bounds); 
            }
        }
    }
}