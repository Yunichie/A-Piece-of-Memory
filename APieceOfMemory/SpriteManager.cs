using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace APieceOfMemory
{
    public class SpriteManager
    {
        private Image originalImage;
        private int frameCount;
        private int currentFrame;
        private System.Windows.Forms.Timer animationTimer;
        
        public Image CurrentFrameImage { get; private set; }
        
        public bool IsAnimated { get; private set; }

        public SpriteManager(Image image)
        {
            originalImage = image;
            IsAnimated = IsAnimatedGif(image);
            
            if (IsAnimated)
            {
                frameCount = image.GetFrameCount(FrameDimension.Time);
                currentFrame = 0;
                
                UpdateCurrentFrame();
                
                animationTimer = new System.Windows.Forms.Timer();
                animationTimer.Interval = 100; // 100ms per frame = 10 FPS
                animationTimer.Tick += AnimationTimer_Tick;
                animationTimer.Start();
            }
            else
            {
                CurrentFrameImage = image;
            }
        }

        private bool IsAnimatedGif(Image image)
        {
            return image.RawFormat.Equals(ImageFormat.Gif) && 
                   image.GetFrameCount(FrameDimension.Time) > 1;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            currentFrame = (currentFrame + 1) % frameCount;
            UpdateCurrentFrame();
        }

        private void UpdateCurrentFrame()
        {
            if (IsAnimated)
            {
                originalImage.SelectActiveFrame(FrameDimension.Time, currentFrame);
                
                CurrentFrameImage?.Dispose();
                CurrentFrameImage = new Bitmap(originalImage);
            }
        }

        public void Dispose()
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            CurrentFrameImage?.Dispose();
            originalImage?.Dispose();
        }
    }

    public static class AnimatedSpriteManager
    {
        public static SpriteManager BackgroundImage { get; private set; }
        // Animated sprite properties
        public static SpriteManager PlayerSprite { get; private set; }
        public static SpriteManager EnemySprite { get; private set; }
        
        public static SpriteManager FlowerSprite { get; private set; }
        public static SpriteManager PlayerProjectileSprite { get; private set; }
        public static SpriteManager EnemyProjectileSprite { get; private set; }
        public static SpriteManager BossSprite { get; private set; }

        public static void LoadSprites(string spritesPath = "Resources/")
        {
            try
            {
                BackgroundImage = LoadAnimatedSprite(Path.Combine(spritesPath, "Space_Background.png"));
                PlayerSprite = LoadAnimatedSprite(Path.Combine(spritesPath, "sol.gif"));
                EnemySprite = LoadAnimatedSprite(Path.Combine(spritesPath, "lava_world.gif"));
                FlowerSprite = LoadAnimatedSprite(Path.Combine(spritesPath, "terra.gif"));
                // PlayerProjectileSprite = LoadAnimatedSprite(Path.Combine(spritesPath, "projectile_player.gif"));
                // EnemyProjectileSprite = LoadAnimatedSprite(Path.Combine(spritesPath, "projectile_enemy.gif"));
                BossSprite = LoadAnimatedSprite(Path.Combine(spritesPath, "black_hole.gif"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sprites: {ex.Message}");
            }
        }

        private static SpriteManager LoadAnimatedSprite(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    Image image = Image.FromFile(filePath);
                    return new SpriteManager(image);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading sprite {filePath}: {ex.Message}");
                    return null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Sprite not found: {filePath}");
                return null;
            }
        }

        public static void DisposeSprites()
        {
            BackgroundImage?.Dispose();
            PlayerSprite?.Dispose();
            EnemySprite?.Dispose();
            FlowerSprite?.Dispose();
            PlayerProjectileSprite?.Dispose();
            EnemyProjectileSprite?.Dispose();
            BossSprite?.Dispose();
        }
    }
}