// Collectible.cs
using System; // Required for DateTime and TimeSpan
using System.Drawing;

namespace APieceOfMemory
{
    public enum CollectibleType
    {
        Water,
        Fertilizer
    }

    public class Collectible
    {
        public PointF Position { get; set; }
        public Size Size { get; private set; }
        public CollectibleType Type { get; private set; }
        public Color Color { get; private set; }
        public RectangleF Bounds => new RectangleF(Position, Size);

        public bool IsExpired { get; private set; } // To mark for removal by GameForm
        private DateTime creationTime;
        private static readonly TimeSpan Lifespan = TimeSpan.FromSeconds(7); // Collectibles last for 7 seconds

        public static readonly int DefaultSize = 18; // Made static const for easier access

        // Sprite placeholder:
        // public Image WaterSprite { get; set; }
        // public Image FertilizerSprite { get; set; }

        public Collectible(float x, float y, CollectibleType type)
        {
            Position = new PointF(x, y);
            Type = type;
            Size = new Size(DefaultSize, DefaultSize);
            creationTime = DateTime.Now;
            IsExpired = false;

            switch (type)
            {
                case CollectibleType.Water:
                    Color = Color.FromArgb(135, 206, 250); // LightSkyBlue
                    break;
                case CollectibleType.Fertilizer:
                    Color = Color.FromArgb(139, 69, 19);  // SaddleBrown (brownish-orange)
                    break;
            }
        }

        public void Update()
        {
            if (!IsExpired && DateTime.Now > creationTime + Lifespan)
            {
                IsExpired = true;
            }
            // Optional: Add slight bobbing, fading animation, or slow fall here
            // Example: Position = new PointF(Position.X, Position.Y + 0.3f); // Gentle fall
        }

        public void Draw(Graphics g)
        {
            // Optional: Could reduce alpha as it nears expiry for a fading effect
            // float remainingLifeRatio = 1f - (float)((DateTime.Now - creationTime).TotalSeconds / Lifespan.TotalSeconds);
            // if (remainingLifeRatio < 0) remainingLifeRatio = 0;
            // int alpha = IsExpired ? 0 : (int)(255 * Math.Pow(remainingLifeRatio, 0.5)); // Apply a curve for better fade
            // alpha = Math.Max(0, Math.Min(255, alpha));

            // using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, this.Color)))
            using (SolidBrush brush = new SolidBrush(this.Color)) // Simple draw for now
            {
                if (Type == CollectibleType.Water)
                {
                    PointF[] dropletPoints = {
                        new PointF(Position.X + Size.Width / 2f, Position.Y),
                        new PointF(Position.X + Size.Width, Position.Y + Size.Height * 0.7f),
                        new PointF(Position.X + Size.Width / 2f, Position.Y + Size.Height),
                        new PointF(Position.X, Position.Y + Size.Height * 0.7f)
                    };
                    g.FillPolygon(brush, dropletPoints);
                }
                else // Fertilizer
                {
                    g.FillRectangle(brush, Bounds);
                }
            }
        }
    }
}