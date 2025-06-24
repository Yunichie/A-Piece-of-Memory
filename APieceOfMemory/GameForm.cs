// GameForm.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // For SmoothingMode
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;     // For Debug.WriteLine

// Ensure GameScreenState enum is defined (e.g., at the top of this file or in a shared file)
public enum GameScreenState { Playing, LevelTransition, GameOver }
// (StartScreen state is handled by the separate StartScreen.cs)

namespace APieceOfMemory
{
    public partial class GameForm : Form
    {
        private GameScreenState currentScreenState = GameScreenState.Playing; // GameForm starts directly into Playing

        // Core Game Objects
        private Player player;
        private Flower flower;
        private List<Enemy> enemies;
        private Boss boss;
        private List<Projectile> playerProjectiles;
        private List<Projectile> enemyProjectiles;
        private List<Collectible> activeCollectibles;

        // Level and Game State
        private int currentLevel = 1;
        private System.Windows.Forms.Timer gameTimer;
        private Random random = new Random();
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private string gameMessage = ""; // For "Level Complete", "Game Over" overlays

        // Game Parameters
        private const int PlayerSize = 30;
        private const int FlowerPotSize = 60;
        private const int EnemyBaseSize = 28;
        private const int BossSize = 70;
        private const int ProjectileSize = 8;
        private const int PlayerSpeed = 5;
        private const float PlayerProjectileSpeed = 10f;
        private const float EnemyProjectileSpeed = 6f;

        // Cooldowns and Timers
        private DateTime lastPlayerShootTime = DateTime.MinValue;
        private TimeSpan playerShootCooldown = TimeSpan.FromMilliseconds(300);
        private DateTime lastBossShootTime = DateTime.MinValue;
        private TimeSpan bossShootCooldown = TimeSpan.FromSeconds(1.0); // Slightly faster regular boss shots
        private DateTime lastEnemySpawnTime = DateTime.MinValue;
        private TimeSpan enemySpawnInterval;
        private DateTime lastPlayerCareActionTime = DateTime.MinValue;
        private TimeSpan playerCareActionCooldown = TimeSpan.FromMilliseconds(700);
        
        // Boss Special Attack Fields
        private DateTime lastBossSpecialAttackTime = DateTime.MinValue;
        private TimeSpan bossSpecialAttackInterval = TimeSpan.FromSeconds(10); 
        private TimeSpan bossSpecialAttackDuration = TimeSpan.FromSeconds(3.5); 
        private DateTime bossSpecialAttackEndTime = DateTime.MinValue;
        private bool isBossPerformingSpecialAttack = false;

        // UI and Feedback
        private Font gameFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        private Font titleFont = new Font("Georgia", 36F, FontStyle.Bold | FontStyle.Italic); // Matched StartScreen Title Font
        private Font smallFont = new Font("Segoe UI", 9F);
        private Font objectiveFont = new Font("Segoe UI", 10F, FontStyle.Italic);
        private string temporaryFeedbackMessage = "";
        private DateTime feedbackMessageExpiry = DateTime.MinValue;

        // Level Progress
        private int waterGoal;
        private int fertilizerGoal;
        private int waterProgress;
        private int fertilizerProgress;
        private const float COLLECTIBLE_DROP_CHANCE = 0.40f; // 40% chance

        // Debug (can be removed or adjusted for release)
        public float inflation = 25f; // Moderate inflation for collection zone

        public GameForm()
        {
            this.Text = "A Piece of Memory";
            this.ClientSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            // this.BackColor = Color.FromArgb(15, 15, 30); // Darker blue
            this.DoubleBuffered = true;
            this.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, "Resources", "Space_Background.png"));
            this.BackgroundImageLayout = ImageLayout.Center;

            this.Paint += GameForm_Paint;
            this.KeyDown += GameForm_KeyDown;
            this.KeyUp += GameForm_KeyUp;
            this.MouseClick += GameForm_MouseClick;
            this.FormClosed += GameForm_FormClosed;

            InitializeAndStartGame();
        }

        private void InitializeAndStartGame() 
        {
            currentScreenState = GameScreenState.Playing;
            currentLevel = 1;    
            
            StartGameplayLevel(currentLevel); 

            if (gameTimer == null)
            {
                gameTimer = new System.Windows.Forms.Timer();
                gameTimer.Interval = 16; 
                gameTimer.Tick += GameTimer_Tick;
            }
            if (!gameTimer.Enabled)
            {
                gameTimer.Start(); 
            }
        }
        
        private void StartGameplayLevel(int levelNumber)
        {
            currentLevel = levelNumber;
            currentScreenState = GameScreenState.Playing;

            // player = new Player(this.ClientSize.Width / 2f - PlayerSize / 2f, this.ClientSize.Height - PlayerSize - 50, PlayerSize, Color.Cyan, PlayerSpeed);
            player = new Player(this.ClientSize.Width / 2f - PlayerSize / 2f, this.ClientSize.Height - PlayerSize - 50, AnimatedSpriteManager.PlayerSprite?.CurrentFrameImage, Color.Cyan, PlayerSpeed);
            flower = new Flower(30, this.ClientSize.Height / 2f - FlowerPotSize / 2f, FlowerPotSize); // Flower resets each level start
            
            enemies = new List<Enemy>();
            playerProjectiles = new List<Projectile>();
            enemyProjectiles = new List<Projectile>();
            activeCollectibles = new List<Collectible>();
            boss = null; 

            waterProgress = 0;
            fertilizerProgress = 0;
            temporaryFeedbackMessage = "";
            isBossPerformingSpecialAttack = false; 
            lastBossSpecialAttackTime = DateTime.Now; 
            pressedKeys.Clear();

            SetupLevelSpecifics(currentLevel); 
        }

        private void SetupLevelSpecifics(int level)
        {
            player.CanMoveFreely = (level == 5);
            if (level >= 1 && level <= 4) { 
                player.CanMoveFreely = false;
                player.Position = new PointF(flower.Bounds.Right + 15, flower.Bounds.Top + flower.Size.Height / 2f - player.Size.Height / 2f);
            } else if (level == 5) { 
                 player.CanMoveFreely = true; 
                 player.Position = new PointF(this.ClientSize.Width * 0.2f, this.ClientSize.Height - PlayerSize - 50);
            }
            else { 
                player.Position = new PointF(this.ClientSize.Width / 2f - PlayerSize / 2f, this.ClientSize.Height - PlayerSize - 50);
            }

            this.Text = $"A Piece of Memory - Level {level}";
            gameMessage = "";
            
            switch (level)
            {
                // case 1:
                //     waterGoal = 7; fertilizerGoal = 5;
                //     flower.State = FlowerState.Healthy; 
                //     enemySpawnInterval = TimeSpan.FromSeconds(9999);
                //     lastEnemySpawnTime = DateTime.Now;
                //     break;
                case 1: case 2: case 3: case 4:
                    if (level == 1) { waterGoal = 3; fertilizerGoal = 2; enemySpawnInterval = TimeSpan.FromSeconds(2.0); }
                    if (level == 2) { waterGoal = 5; fertilizerGoal = 3; enemySpawnInterval = TimeSpan.FromSeconds(1.5); } 
                    if (level == 3) { waterGoal = 8; fertilizerGoal = 5; enemySpawnInterval = TimeSpan.FromSeconds(1.0); }
                    if (level == 4) { waterGoal = 12; fertilizerGoal = 8; enemySpawnInterval = TimeSpan.FromSeconds(0.7); } // Faster
                    SpawnInitialEnemiesForLevel(level); 
                    lastEnemySpawnTime = DateTime.Now.AddSeconds(-enemySpawnInterval.TotalSeconds + 0.5);
                    break;
                case 5:
                    waterGoal = 0; fertilizerGoal = 0;
                    enemySpawnInterval = TimeSpan.FromSeconds(9999);
                    boss = new Boss( this.ClientSize.Width + BossSize + 20, this.ClientSize.Height / 2f - BossSize / 2f, BossSize, // Spawn further off
                        Color.DarkMagenta, 1.2f, 80, this.ClientSize.Width, this.ClientSize.Height );
                    lastEnemySpawnTime = DateTime.Now;
                    lastBossSpecialAttackTime = DateTime.Now.AddSeconds(-bossSpecialAttackInterval.TotalSeconds + 5);
                    isBossPerformingSpecialAttack = false;
                    break;
                // case 6:
                //     waterGoal = 0; fertilizerGoal = 0;
                //     flower.State = FlowerState.Broken; 
                //     enemySpawnInterval = TimeSpan.FromSeconds(1.5); 
                //     if (!enemies.Any()) SpawnInitialEnemiesForLevel(6); 
                //     lastEnemySpawnTime = DateTime.Now.AddSeconds(-enemySpawnInterval.TotalSeconds + 1.0);
                //     break;
                default: 
                    HandleGameOver($"You've pieced together all the memories.\nThanks for playing!");
                    break;
            }
        }

        private void SpawnInitialEnemiesForLevel(int gameLevel)
        {
            int count = 0; EnemyType type = EnemyType.Slow;
            if (gameLevel == 1)
            {
                count = 2;
                type = EnemyType.Slow;
            }
            else if (gameLevel == 2) { count = 3; type = EnemyType.Slow; }
            else if (gameLevel == 3) { count = 3; type = EnemyType.Fast; } 
            else if (gameLevel == 4) { count = 4; type = EnemyType.Faster; }
            else if (gameLevel == 6) { count = 4; type = EnemyType.Slow;}

            for (int i = 0; i < count; i++) {
                float spawnY;
                 if (gameLevel >= 1 && gameLevel <= 4 && player != null) 
                    spawnY = player.Position.Y + random.Next(-(int)(player.Size.Height*1.5), (int)(player.Size.Height*1.5));
                 else 
                    spawnY = random.Next(EnemyBaseSize, (int)(this.ClientSize.Height * 0.85f));
                spawnY = Math.Max(EnemyBaseSize, Math.Min(this.ClientSize.Height - EnemyBaseSize * 2, spawnY));
                float spawnX = this.ClientSize.Width + 30 + (i * (EnemyBaseSize + 35));
                Enemy newEnemy = new Enemy(spawnX, spawnY, AnimatedSpriteManager.EnemySprite?.CurrentFrameImage, type);
                newEnemy.Speed = -Math.Abs(newEnemy.Speed); 
                enemies.Add(newEnemy);
            }
        }
        
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            switch (currentScreenState)
            {
                case GameScreenState.Playing:
                    UpdateGame();
                    break;
            }
            this.Invalidate();
        }
        
        private void UpdateGame()
        {
            if (currentScreenState != GameScreenState.Playing) return;

            // Player Movement
            float dx = 0; float dy = 0;
            if (!(currentLevel >= 1 && currentLevel <= 4)) 
            {
                if (currentLevel != 5) 
                {
                    if (pressedKeys.Contains(Keys.Left) || pressedKeys.Contains(Keys.A)) dx = -player.Speed;
                    if (pressedKeys.Contains(Keys.Right) || pressedKeys.Contains(Keys.D)) dx = player.Speed;
                }
                if (player.CanMoveFreely) 
                {
                    if (pressedKeys.Contains(Keys.Up) || pressedKeys.Contains(Keys.W)) dy = -player.Speed;
                    if (pressedKeys.Contains(Keys.Down) || pressedKeys.Contains(Keys.S)) dy = player.Speed;
                }
                if (dx != 0 || dy != 0) player.Move(dx, dy, this.ClientRectangle);
            }

            // Updates
            for (int i = playerProjectiles.Count - 1; i >= 0; i--) { if (!playerProjectiles[i].Update(ClientRectangle)) playerProjectiles.RemoveAt(i); }
            for (int i = enemyProjectiles.Count - 1; i >= 0; i--) { if (!enemyProjectiles[i].Update(ClientRectangle)) enemyProjectiles.RemoveAt(i); }
            for (int i = activeCollectibles.Count - 1; i >= 0; i--) { activeCollectibles[i].Update(); if (activeCollectibles[i].IsExpired) activeCollectibles.RemoveAt(i); }

            // Timed Enemy Spawning
            if ((currentLevel >= 1 && currentLevel <= 4) &&
                (DateTime.Now - lastEnemySpawnTime) > enemySpawnInterval &&
                 enemies.Count < (currentLevel == 4 ? 12 : currentLevel == 6 ? 10 : 8)) // Adjusted max enemies 
            {
                EnemyType typeToSpawn = currentLevel switch {
                    1 => EnemyType.Slow, 2 => EnemyType.Slow, 3 => EnemyType.Fast, 4 => EnemyType.Faster, _ => EnemyType.Slow
                };
                float spawnY;
                if ((currentLevel >= 1 && currentLevel <= 4) && player != null) { spawnY = player.Position.Y + random.Next(-player.Size.Height*2, player.Size.Height*2); } 
                else { spawnY = random.Next(EnemyBaseSize, this.ClientSize.Height - EnemyBaseSize); }
                spawnY = Math.Max(EnemyBaseSize, Math.Min(this.ClientSize.Height - EnemyBaseSize*2, spawnY)); float spawnX;
                if (currentLevel >= 1 && currentLevel <= 4) { spawnX = this.ClientSize.Width + random.Next(20, 120); } 
                else { spawnX = (random.Next(0,2) == 0) ? -EnemyBaseSize - random.Next(20,100) : this.ClientSize.Width + random.Next(20,150); }
                Enemy newEnemy = new Enemy(spawnX, spawnY, AnimatedSpriteManager.EnemySprite?.CurrentFrameImage, typeToSpawn);
                PointF targetForEnemy = (currentLevel >= 1 && currentLevel <= 4 && player != null) ? player.Position : flower.Position;
                if (spawnX > targetForEnemy.X) newEnemy.Speed = -Math.Abs(newEnemy.Speed); else newEnemy.Speed = Math.Abs(newEnemy.Speed);
                enemies.Add(newEnemy); lastEnemySpawnTime = DateTime.Now;
            }

            // Enemy Updates
            for (int i = enemies.Count - 1; i >= 0; i--) {
                PointF targetPos = (currentLevel >= 1 && currentLevel <= 4 && player != null) ? player.Position : flower.Position;
                enemies[i].Update(targetPos); 
                if (enemies[i].Bounds.IntersectsWith(flower.Bounds)) {
                    flower.TakeDamage(); enemies.RemoveAt(i);
                    if (flower.State == FlowerState.Dead) { HandleGameOver("The memory faded... The flower is gone."); return;}
                } else if (enemies[i].Position.X < -enemies[i].Size.Width * 1.5f || enemies[i].Position.X > this.ClientSize.Width + enemies[i].Size.Width * 1.5f) {
                    enemies.RemoveAt(i);
                }
            }

            // Boss Logic
            if (boss != null && currentLevel == 5) {
                boss.Update(this.ClientSize.Width, this.ClientSize.Height);
                PointF bossCenter = new PointF(boss.Position.X + boss.Size.Width / 2f, boss.Position.Y + boss.Size.Height / 2f);
                if (isBossPerformingSpecialAttack) {
                    if (DateTime.Now < bossSpecialAttackEndTime) {
                        if (random.Next(0, 3) == 0) { // Increased chance/rate of special projectiles
                            for(int r=0; r < random.Next(2,5) ; r++) { 
                                float randomAngleRad = (float)(random.NextDouble() * 2 * Math.PI);
                                float velX = (float)Math.Cos(randomAngleRad) * (EnemyProjectileSpeed * 0.8f); 
                                float velY = (float)Math.Sin(randomAngleRad) * (EnemyProjectileSpeed * 0.8f);
                                enemyProjectiles.Add(new Projectile(bossCenter.X - ProjectileSize / 2f, bossCenter.Y - ProjectileSize / 2f, ProjectileSize, Color.OrangeRed, new PointF(velX, velY), ProjectileType.Enemy));
                            }
                        }
                    } else { isBossPerformingSpecialAttack = false; lastBossSpecialAttackTime = DateTime.Now; }
                } else {
                    if ((DateTime.Now - lastBossSpecialAttackTime) > bossSpecialAttackInterval) {
                        isBossPerformingSpecialAttack = true;
                        bossSpecialAttackEndTime = DateTime.Now.Add(bossSpecialAttackDuration);
                        temporaryFeedbackMessage = "Boss Attack!"; feedbackMessageExpiry = DateTime.Now.AddSeconds(1.5);
                        FireCircularBurst(random.Next(12,25), bossCenter, EnemyProjectileSpeed * 0.65f, Color.MediumVioletRed); // Randomized burst
                    } else if ((DateTime.Now - lastBossShootTime) > bossShootCooldown) {
                        int projectilesInSpread = random.Next(3,8); float totalSpreadAngleDegrees = random.Next(25,75); 
                        PointF playerCenter = (player != null) ? new PointF(player.Position.X + player.Size.Width / 2f, player.Position.Y + player.Size.Height / 2f) : bossCenter;
                        float angleToPlayerRad = (float)Math.Atan2(playerCenter.Y - bossCenter.Y, playerCenter.X - bossCenter.X);
                        float startAngleRad = angleToPlayerRad - (totalSpreadAngleDegrees / 2f * (float)Math.PI / 180f);
                        float angleIncrementRad = (projectilesInSpread > 1) ? (totalSpreadAngleDegrees * (float)Math.PI / 180f) / (projectilesInSpread - 1) : 0;
                        for (int k = 0; k < projectilesInSpread; k++) {
                            float currentAngleRad = startAngleRad + (k * angleIncrementRad);
                            float velocityX = (float)Math.Cos(currentAngleRad) * EnemyProjectileSpeed; float velocityY = (float)Math.Sin(currentAngleRad) * EnemyProjectileSpeed;
                            enemyProjectiles.Add(new Projectile(bossCenter.X - ProjectileSize / 2f, bossCenter.Y - ProjectileSize / 2f, ProjectileSize, Color.HotPink, new PointF(velocityX, velocityY), ProjectileType.Enemy));
                        }
                        lastBossShootTime = DateTime.Now;
                    }
                }
            }
            
            if (DateTime.Now > feedbackMessageExpiry) temporaryFeedbackMessage = "";
            CheckCollisions();
            if(currentScreenState == GameScreenState.Playing) CheckLevelCompletion();
        }
        
        private void FireCircularBurst(int count, PointF origin, float speed, Color projColor) {
            for (int k = 0; k < count; k++) {
                float angleRad = (float)(k * (2 * Math.PI / count));
                float velX = (float)Math.Cos(angleRad) * speed; float velY = (float)Math.Sin(angleRad) * speed;
                enemyProjectiles.Add(new Projectile( origin.X - ProjectileSize / 2f, origin.Y - ProjectileSize / 2f, ProjectileSize, projColor, new PointF(velX, velY), ProjectileType.Enemy));
            }
        }

        private void HandleGameOver(string message) {
            if (currentScreenState == GameScreenState.GameOver) return;
            currentScreenState = GameScreenState.GameOver;
            gameMessage = message;
        }
        
        private void GameForm_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                if (currentScreenState == GameScreenState.Playing) {
                    // if (currentLevel == 1 && !enemies.Any()) return; 
                    if ((DateTime.Now - lastPlayerShootTime) > playerShootCooldown && player != null) {
                        PointF playerCenter = new PointF(player.Position.X + player.Size.Width / 2f, player.Position.Y + player.Size.Height / 2f);
                        float dirX = e.X - playerCenter.X; float dirY = e.Y - playerCenter.Y; float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
                        if (length > 0) {
                            float normX = dirX/length; float normY = dirY/length;
                            playerProjectiles.Add(new Projectile( playerCenter.X + normX * (player.Size.Width/2f + 3) - ProjectileSize/2f, playerCenter.Y + normY * (player.Size.Height/2f + 3) - ProjectileSize/2f, ProjectileSize, Color.LightSkyBlue, new PointF(normX * PlayerProjectileSpeed, normY * PlayerProjectileSpeed), ProjectileType.Player));
                            lastPlayerShootTime = DateTime.Now;
                        }
                    }
                }
            }
        }

        private void CheckCollisions() {
            if (player == null) return;
            // Player Projectiles vs Enemies
            for (int i = playerProjectiles.Count - 1; i >= 0; i--) {
                for (int j = enemies.Count - 1; j >= 0; j--) {
                    if (i < playerProjectiles.Count && playerProjectiles[i].Bounds.IntersectsWith(enemies[j].Bounds)) { // Check i validity
                        enemies[j].Health--;
                        if (enemies[j].Health <= 0) {
                            if (currentLevel >= 1 && currentLevel <= 4 && random.NextDouble() < COLLECTIBLE_DROP_CHANCE) {
                                CollectibleType dropType = (random.Next(0, 2) == 0) ? CollectibleType.Water : CollectibleType.Fertilizer;
                                activeCollectibles.Add(new Collectible(
                                    enemies[j].Position.X + enemies[j].Size.Width / 2f - Collectible.DefaultSize / 2f, 
                                    enemies[j].Position.Y + enemies[j].Size.Height / 2f - Collectible.DefaultSize / 2f, dropType));
                            }
                            enemies.RemoveAt(j);
                        }
                        playerProjectiles.RemoveAt(i); break; 
                    }
                }
            }
            // Player Projectiles vs Boss
            if (boss != null && currentLevel == 5) {
                for (int i = playerProjectiles.Count - 1; i >= 0; i--) {
                    if (i < playerProjectiles.Count && playerProjectiles[i].Bounds.IntersectsWith(boss.Bounds)) { // Check i validity
                        Debug.WriteLine($"BOSS HIT! HP Before: {boss.Health}");
                        boss.Health--;
                        temporaryFeedbackMessage = $"Boss Hit! HP: {boss.Health}"; feedbackMessageExpiry = DateTime.Now.AddSeconds(1);
                        playerProjectiles.RemoveAt(i);
                        if (boss.Health <= 0) { Debug.WriteLine("BOSS DEFEATED!"); boss = null; /* Completion handled by CheckLevelCompletion */ }
                        break; 
                    }
                }
            }
            // Player vs Collectibles
            if (currentLevel >= 1 && currentLevel <= 4) {
                RectangleF collectionZone = player.Bounds; float actualInflation = player.Size.Width * inflation; collectionZone.Inflate(actualInflation, actualInflation); 
                for (int k = activeCollectibles.Count - 1; k >= 0; k--) {
                    // Debug.WriteLine($"Checking collectible [{k}] Bounds: {activeCollectibles[k].Bounds} against Zone: {collectionZone}. Intersects: {collectionZone.IntersectsWith(activeCollectibles[k].Bounds)}");
                    if (collectionZone.IntersectsWith(activeCollectibles[k].Bounds)) {
                        // Debug.WriteLine($"    COLLECTED! Collectible {k}");
                        Collectible collected = activeCollectibles[k];
                        if (collected.Type == CollectibleType.Water) { waterProgress++; temporaryFeedbackMessage = "+1 Water!"; }
                        else { fertilizerProgress++; temporaryFeedbackMessage = "+1 Fertilizer!"; }
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(1.5);
                        activeCollectibles.RemoveAt(k);
                    }
                }
            }
            // Enemy Projectiles vs Player
            for (int i = enemyProjectiles.Count - 1; i >= 0; i--) {
                if (i < enemyProjectiles.Count && enemyProjectiles[i].Bounds.IntersectsWith(player.Bounds)) { // Check i validity
                    enemyProjectiles.RemoveAt(i); flower.TakeDamage(); 
                    if (flower.State == FlowerState.Dead) { HandleGameOver("You couldn't protect the memory... It shattered."); return; }
                    else { temporaryFeedbackMessage = "Ouch! Be careful!"; feedbackMessageExpiry = DateTime.Now.AddSeconds(2); }
                }
            }
            // Player vs Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i < enemies.Count && player.Bounds.IntersectsWith(enemies[i].Bounds)) { // Check i validity
                    enemies.RemoveAt(i); flower.TakeDamage(); 
                    if (flower.State == FlowerState.Dead) { HandleGameOver("The chaos was too much for the fragile memory."); return; }
                    else { temporaryFeedbackMessage = "They got too close to you!"; feedbackMessageExpiry = DateTime.Now.AddSeconds(2); }
                }
            }
            // Player Projectiles vs Enemy Projectiles
            for (int i = playerProjectiles.Count - 1; i >= 0; i--)
            {
                for (int j = enemyProjectiles.Count - 1; j >= 0; j--)
                {
                    if (i < playerProjectiles.Count && j < enemyProjectiles.Count) {
                        if (playerProjectiles[i].Bounds.IntersectsWith(enemyProjectiles[j].Bounds)) {
                            Debug.WriteLine("PROJECTILE CLASH!");
                            playerProjectiles.RemoveAt(i); enemyProjectiles.RemoveAt(j);
                            goto nextPlayerProjectileClashCheck; 
                        }
                    }
                } nextPlayerProjectileClashCheck:;
            }
        }

        private void CheckLevelCompletion() {
            if (currentScreenState != GameScreenState.Playing) return;
            bool conditionsMet = false;
            switch (currentLevel) {
                // case 1: if (waterProgress >= waterGoal && fertilizerProgress >= fertilizerGoal) conditionsMet = true; break;
                case 1: case 2: case 3: case 4: if (waterProgress >= waterGoal && fertilizerProgress >= fertilizerGoal) conditionsMet = true; break;
                case 5: if (boss == null) conditionsMet = true; break;
                // case 6: if (enemies.Count == 0 && !AnyEnemiesSpawningSoon() && flower.State != FlowerState.Dead) { /* conditionsMet = true; */ } break;
            }
            if (conditionsMet) { currentScreenState = GameScreenState.LevelTransition; gameMessage = $"Level {currentLevel} Complete!"; temporaryFeedbackMessage = ""; }
        }

        private bool AnyEnemiesSpawningSoon()
        {
            return (currentLevel >= 1 && currentLevel <= 4) && 
                   (DateTime.Now - lastEnemySpawnTime) <= enemySpawnInterval && 
                   enemies.Count < (currentLevel == 4 ? 12 : currentLevel == 6 ? 8 : 8) ; // Adjusted max enemies
        }
        
        private void DrawGamePlayingState(Graphics g) {
            if (flower == null || player == null) return;
            flower.Draw(g); player.Draw(g);
            foreach (var p in playerProjectiles) p.Draw(g); foreach (var c in activeCollectibles) c.Draw(g);
            foreach (var en in enemies) en.Draw(g); if (boss != null) boss.Draw(g);
            foreach (var ep in enemyProjectiles) ep.Draw(g);
            // UI Text
            string lt = $"Level: {currentLevel}"; g.DrawString(lt, gameFont, Brushes.LightGray, 10,10);
            string fst = $"Flower: {flower.State}"; g.DrawString(fst, gameFont, Brushes.LightGray, 10, 10+gameFont.Height+2);
            string ot = ""; if (currentLevel==1) ot = $"Care (E/R): W:{waterProgress}/{waterGoal} F:{fertilizerProgress}/{fertilizerGoal}"; else if (currentLevel >=2 && currentLevel <=4) ot = $"Collect: W:{waterProgress}/{waterGoal} F:{fertilizerProgress}/{fertilizerGoal}";
            if (!string.IsNullOrEmpty(ot)) g.DrawString(ot, objectiveFont, Brushes.Gold, 10, 10+(gameFont.Height+2)*2);
            
            if (!string.IsNullOrEmpty(temporaryFeedbackMessage) && DateTime.Now < feedbackMessageExpiry) {
                SizeF feedbackSize = g.MeasureString(temporaryFeedbackMessage, gameFont);
                float feedbackX = (this.ClientSize.Width - feedbackSize.Width) / 2;
                float feedbackY = (player != null ? player.Position.Y : this.ClientSize.Height / 2f) - PlayerSize - 15; 
                if (feedbackY < 10) feedbackY = this.ClientSize.Height - 40; 
                g.DrawString(temporaryFeedbackMessage, gameFont, Brushes.Lime, feedbackX, feedbackY);
            }

            // if (currentLevel == 6)
            // {
            //     string condition = $"Flower Condition: {flower.State}";
            //     SizeF conditionSize = g.MeasureString(condition, gameFont);
            //     using (SolidBrush conditionBrush = new SolidBrush(flower.GetCurrentColor())) {
            //         g.DrawString(condition, gameFont, conditionBrush, this.ClientSize.Width - conditionSize.Width - 10, 10);
            //     }
            //     g.DrawString("Press 'Space' to Save Screen (Mock)", smallFont, Brushes.LightGray, this.ClientSize.Width - 200, 10 + conditionSize.Height + 5);
            // }
            // Debug drawing for collection zone
            if (currentLevel >= 1 && currentLevel <= 4 && currentScreenState == GameScreenState.Playing) {
                using (Pen playerPen = new Pen(Color.FromArgb(100, Color.LightGreen), 1)) {
                    if(player != null) g.DrawRectangle(playerPen, Rectangle.Round(player.Bounds));
                }
                if(player != null) {
                    RectangleF cz = player.Bounds;
                    cz.Inflate(player.Size.Width * inflation, player.Size.Height * inflation);
                    using (Pen zonePen = new Pen(Color.FromArgb(150, Color.Yellow), 2)) {
                        RectangleF clippedCz = RectangleF.Intersect(cz, this.ClientRectangle);
                        if(!clippedCz.IsEmpty) g.DrawRectangle(zonePen, Rectangle.Round(clippedCz));
                    }
                }
                foreach (var collectible in activeCollectibles) {
                    using (Pen collectiblePen = new Pen(Color.FromArgb(150, Color.Cyan), 1)) {
                        g.DrawRectangle(collectiblePen, Rectangle.Round(collectible.Bounds));
                    }
                }
            }
        }
        
        private void DrawOverlayMessage(Graphics g, string messageToDisplay) {
            using (SolidBrush overlayBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0))) { g.FillRectangle(overlayBrush, 0, ClientSize.Height / 3.5f, ClientSize.Width, ClientSize.Height / 2.5f); }
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
            Rectangle rectMessage = new Rectangle(0, (int)(ClientSize.Height / 3.5f), ClientSize.Width, (int)(ClientSize.Height / 2.5f));
            TextRenderer.DrawText(g, messageToDisplay, titleFont, rectMessage, Color.White, flags);
            string prompt = "";
            if (currentScreenState == GameScreenState.LevelTransition && currentLevel < 6) prompt = "Press Enter to Proceed.";
            else if (currentScreenState == GameScreenState.LevelTransition && currentLevel == 6) prompt = "Final memory preserved...\nPress Enter.";
            else if (currentScreenState == GameScreenState.GameOver) prompt = "Press Enter to Return to Start Screen or Escape to Quit.";
            if (!string.IsNullOrEmpty(prompt)) { Rectangle rectPrompt = new Rectangle(0, (int)(rectMessage.Bottom - 70), ClientSize.Width, 60); TextRenderer.DrawText(g, prompt, gameFont, rectPrompt, Color.LightYellow, flags); }
        }

        private void GameForm_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; 
            // g.Clear(this.BackColor); 
            switch (currentScreenState) {
                case GameScreenState.Playing: DrawGamePlayingState(g); break;
                case GameScreenState.LevelTransition: DrawGamePlayingState(g); DrawOverlayMessage(g, gameMessage); break;
                case GameScreenState.GameOver: DrawGamePlayingState(g); DrawOverlayMessage(g, gameMessage); break;
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e) {
            switch (currentScreenState) {
                case GameScreenState.Playing:
                    if (!pressedKeys.Contains(e.KeyCode)) pressedKeys.Add(e.KeyCode);
                    if (player == null) return;
                    // if (currentLevel == 1 && (DateTime.Now - lastPlayerCareActionTime) > playerCareActionCooldown) {
                    //     RectangleF iBounds = flower.Bounds; iBounds.Inflate(player.Size.Width * 0.75f, player.Size.Height * 0.75f);
                    //     if (player.Bounds.IntersectsWith(iBounds)) {
                    //         if (e.KeyCode == Keys.E) { if (waterProgress < waterGoal) waterProgress++; temporaryFeedbackMessage = (waterProgress<waterGoal)?"Watered!":"Flower has enough water."; lastPlayerCareActionTime = DateTime.Now; feedbackMessageExpiry = DateTime.Now.AddSeconds(1.5); }
                    //         else if (e.KeyCode == Keys.R) { if (fertilizerProgress < fertilizerGoal) fertilizerProgress++; temporaryFeedbackMessage = (fertilizerProgress<fertilizerGoal)?"Fertilized!":"Flower has enough fertilizer."; lastPlayerCareActionTime = DateTime.Now; feedbackMessageExpiry = DateTime.Now.AddSeconds(1.5); }
                    //     }
                    // }
                    // if (currentLevel == 6 && e.KeyCode == Keys.S) { MessageBox.Show("Screenshot 'saved'! (Placeholder)", "A Piece of Memory", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    if (e.KeyCode == Keys.Q) { currentLevel++; if (currentLevel > 6) { HandleGameOver("DEBUG: All levels skipped."); } else { StartGameplayLevel(currentLevel); } }
                    break;
                case GameScreenState.LevelTransition:
                    if (e.KeyCode == Keys.Enter) {
                        currentLevel++;
                        if (currentLevel > 5) { HandleGameOver("You've pieced together all the memories.\nThank you for playing."); }
                        else { StartGameplayLevel(currentLevel); }
                    }
                    break;
                case GameScreenState.GameOver:
                    if (e.KeyCode == Keys.Enter) { InitializeAndStartGame(); }
                    else if (e.KeyCode == Keys.Escape) { this.Close(); }
                    break;
            }
        }
        
        private void GameForm_KeyUp(object sender, KeyEventArgs e) { pressedKeys.Remove(e.KeyCode); }
        
        private void GameForm_FormClosed(object sender, FormClosedEventArgs e) {
            Application.Exit(); 
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { 
            base.OnFormClosed(e); 
            gameTimer?.Stop(); gameTimer?.Dispose();
            gameFont?.Dispose(); titleFont?.Dispose(); smallFont?.Dispose(); objectiveFont?.Dispose();
        }
    }
}