using System.Drawing;

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

        // Sprite placeholder:
        // public Image ProjectileSprite { get; set; }

        public Projectile(float x, float y, int size, Color color, PointF velocity, ProjectileType type)
        {
            Position = new PointF(x, y);
            Size = new Size(size, size);
            Color = color;
            Velocity = velocity;
            Type = type;
        }

        public void Update()
        {
            Position = new PointF(Position.X + Velocity.X, Position.Y + Velocity.Y);
        }

        public void Draw(Graphics g)
        {
            // if (ProjectileSprite != null) {
            //     g.DrawImage(ProjectileSprite, Bounds);
            // } else {
            using (SolidBrush brush = new SolidBrush(Color))
            {
                g.FillEllipse(brush, Bounds); // Projectiles as small circles
            }
            // }
        }
    }
}