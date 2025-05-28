using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // For SmoothingMode
using System.Linq;
using System.Windows.Forms;

namespace APieceOfMemory
{
    public partial class GameForm : Form
    {
        // ... (Existing fields from the previous correct version)
        private Player player;
        private Flower flower;
        private List<Enemy> enemies;
        private Boss boss;
        private List<Projectile> playerProjectiles;
        private List<Projectile> enemyProjectiles;
        private int currentLevel = 1;
        private System.Windows.Forms.Timer gameTimer;
        private Random random = new Random();
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private const int PlayerSize = 30;
        private const int FlowerPotSize = 60;
        private const int EnemyBaseSize = 28;
        private const int BossSize = 70;
        private const int ProjectileSize = 8; // Keep this, it's projectile visual size
        private const int PlayerSpeed = 5;
        private const float PlayerProjectileSpeed = 10f; // Speed of the projectile
        private const float EnemyProjectileSpeed = 6f;
        private DateTime lastPlayerShootTime = DateTime.MinValue;
        private TimeSpan playerShootCooldown = TimeSpan.FromMilliseconds(300); // Adjusted cooldown slightly
        private DateTime lastBossShootTime = DateTime.MinValue;
        private TimeSpan bossShootCooldown = TimeSpan.FromSeconds(1.2);
        private DateTime lastEnemySpawnTime = DateTime.MinValue;
        private TimeSpan enemySpawnInterval;
        private bool gameOver = false;
        private bool levelComplete = false;
        private string gameMessage = "";
        private Font gameFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        private Font titleFont = new Font("Arial", 24F, FontStyle.Bold | FontStyle.Italic);
        private Font smallFont = new Font("Segoe UI", 9F);
        private Font objectiveFont = new Font("Segoe UI", 10F, FontStyle.Italic);
        private List<Collectible> activeCollectibles;
        private int waterGoal;
        private int fertilizerGoal;
        private int waterProgress;
        private int fertilizerProgress;
        private const float COLLECTIBLE_DROP_CHANCE = 2f;
        private DateTime lastPlayerCareActionTime = DateTime.MinValue;
        private TimeSpan playerCareActionCooldown = TimeSpan.FromMilliseconds(700);
        private string temporaryFeedbackMessage = "";
        private DateTime feedbackMessageExpiry = DateTime.MinValue;
        // Add these fields to GameForm class
        private DateTime lastBossSpecialAttackTime = DateTime.MinValue;
        private TimeSpan bossSpecialAttackInterval = TimeSpan.FromSeconds(12); // How often special attack can occur
        private TimeSpan bossSpecialAttackDuration = TimeSpan.FromSeconds(2.5); // How long the barrage lasts
        private DateTime bossSpecialAttackEndTime = DateTime.MinValue;
        private bool isBossPerformingSpecialAttack = false;
        private int specialAttackProjectilesPerTick = 0; // Counter for projectiles in current special attack tick

        // DEBUG
        public float inflation = 25.0f;

        public GameForm()
        {
            this.Text = "A Piece of Memory";
            this.ClientSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 40);
            this.DoubleBuffered = true;

            this.Paint += GameForm_Paint;
            this.KeyDown += GameForm_KeyDown;
            this.KeyUp += GameForm_KeyUp;
            this.MouseClick += GameForm_MouseClick; // ADDED: Mouse click event handler

            InitializeGame();
        }

        private void InitializeGame()
        {
            gameOver = false;
            levelComplete = false;
            gameMessage = "";
            temporaryFeedbackMessage = "";
            pressedKeys.Clear();

            // Initial player position (will be overridden by SetupLevel if needed)
            player = new Player(this.ClientSize.Width / 2f - PlayerSize / 2f, this.ClientSize.Height - PlayerSize - 50, PlayerSize, Color.Cyan, PlayerSpeed);
            flower = new Flower(30, this.ClientSize.Height / 2f - FlowerPotSize / 2f, FlowerPotSize);

            enemies = new List<Enemy>();
            playerProjectiles = new List<Projectile>();
            enemyProjectiles = new List<Projectile>();
            activeCollectibles = new List<Collectible>();
            boss = null;

            SetupLevel(currentLevel);

            if (gameTimer == null)
            {
                gameTimer = new System.Windows.Forms.Timer();
                gameTimer.Interval = 16; // Approx 60 FPS
                gameTimer.Tick += GameTimer_Tick;
            }
            gameTimer.Start();
        }

        private void SpawnInitialEnemiesForLevel(int gameLevel)
        {
            int count = 0;
            EnemyType type = EnemyType.Slow;
            float initialSpawnDelayFactor = 0; // For staggering initial spawns slightly if needed

            if (gameLevel == 2) { count = 2; type = EnemyType.Slow; } // Spawn 2 slow enemies for level 2
            else if (gameLevel == 3) { count = 2; type = EnemyType.Fast; initialSpawnDelayFactor = 0.5f; } // Spawn 2 fast
            else if (gameLevel == 4) { count = 3; type = EnemyType.Faster; initialSpawnDelayFactor = 0.3f; } // Spawn 3 faster

            for (int i = 0; i < count; i++)
            {
                float spawnY = player.Position.Y + random.Next(-player.Size.Height, player.Size.Height); // Spawn around player's Y for L2-4
                spawnY = Math.Max(EnemyBaseSize, Math.Min(this.ClientSize.Height - EnemyBaseSize * 2, spawnY));
                
                // ALWAYS SPAWN FROM THE RIGHT for levels 2, 3, 4 (and 6 if using this for initial right-only spawn)
                float spawnX = this.ClientSize.Width + 20 + (i * (EnemyBaseSize + 30)); // Staggered off-screen right

                Enemy newEnemy = new Enemy(spawnX, spawnY, EnemyBaseSize, type);
                
                // Since they always spawn from the right (spawnX > player.Position.X is true),
                // their speed should always be negative to move left.
                newEnemy.Speed = -Math.Abs(newEnemy.Speed); 
                
                enemies.Add(newEnemy);
                // System.Diagnostics.Debug.WriteLine($"L{gameLevel} SPAWN_INIT: Added enemy {i}. Pos:({newEnemy.Position.X},{newEnemy.Position.Y}), Speed:{newEnemy.Speed}. Total enemies: {enemies.Count}");
            }
        }


        // In GameForm.cs
        private void SetupLevel(int level)
        {
            // ... (existing clear lists, player setup) ...
            enemies.Clear();
            playerProjectiles.Clear();
            enemyProjectiles.Clear();
            activeCollectibles.Clear();
            boss = null;

            player.CanMoveFreely = (level == 1 || level == 6 || level == 5);
            player.Position = new PointF(this.ClientSize.Width / 2f - PlayerSize / 2f, this.ClientSize.Height - PlayerSize - 50);

            this.Text = $"A Piece of Memory - Level {level}";
            
            waterProgress = 0;
            fertilizerProgress = 0;
            temporaryFeedbackMessage = ""; 

            switch (level)
            {
                case 1:
                    waterGoal = 7;
                    fertilizerGoal = 5;
                    gameMessage = "Level 1: Nurture the flower. Stand near it and press 'E' to Water, 'R' to Fertilize.";
                    flower.State = FlowerState.Healthy;
                    enemySpawnInterval = TimeSpan.FromSeconds(9999); // No timed spawns
                    lastEnemySpawnTime = DateTime.Now;
                    break;
                case 2:
                case 3:
                case 4:
                    player.CanMoveFreely = false;
                    player.Position = new PointF(flower.Bounds.Right + 15, flower.Bounds.Top + flower.Size.Height / 2f - player.Size.Height / 2f);
                    
                    // FASTER SPAWN INTERVALS:
                    if (level == 2) { waterGoal = 5; fertilizerGoal = 3; enemySpawnInterval = TimeSpan.FromSeconds(0.5); gameMessage = "Level 2: Protect the flower! Click to shoot. Collect Water & Fertilizer."; } 
                    if (level == 3) { waterGoal = 8; fertilizerGoal = 5; enemySpawnInterval = TimeSpan.FromSeconds(0.3); gameMessage = "Level 3: They are getting faster! Click to shoot. Gather more resources."; }
                    if (level == 4) { waterGoal = 12; fertilizerGoal = 8; enemySpawnInterval = TimeSpan.FromSeconds(0.1); gameMessage = "Level 4: Overwhelm! Click to shoot. Stock up on essentials."; }
                    
                    SpawnInitialEnemiesForLevel(level); 
                    // Make the first *timed* wave appear faster after initial spawns
                    lastEnemySpawnTime = DateTime.Now.AddSeconds(-enemySpawnInterval.TotalSeconds + 0.5); // e.g., first timed wave after ~0.5 sec
                    break;
                case 5:
                    // ... (level 5 setup) ...
                    waterGoal = 0; fertilizerGoal = 0;
                    gameMessage = "Level 5: The Source of Sorrow! Click to shoot.";
                    enemySpawnInterval = TimeSpan.FromSeconds(9999);
                    boss = new Boss(
                        this.ClientSize.Width + BossSize,  // x
                        this.ClientSize.Height / 2f - BossSize / 2f, // y
                        BossSize,                          // size
                        Color.DarkMagenta,                 // color
                        1f,                                // speed
                        50,                                // health
                        this.ClientSize.Width,             // clientWidth (THE MISSING ARGUMENT)
                        this.ClientSize.Height             // clientHeight
                    );
                    player.CanMoveFreely = true;
                    player.Position = new PointF(this.ClientSize.Width * 0.2f, this.ClientSize.Height - PlayerSize - 50);
                    lastEnemySpawnTime = DateTime.Now;
                    break;
                case 6:
                    // ... (level 6 setup, consider faster spawns here too if needed) ...
                    waterGoal = 0; fertilizerGoal = 0;
                    gameMessage = "Level 6: A fragile memory... Click to shoot. (Flower health is critical). Press 'S' to 'save' (mock).";
                    flower.State = FlowerState.Broken;
                    player.CanMoveFreely = true; 
                    enemySpawnInterval = TimeSpan.FromSeconds(1.8); // Faster spawns for L6 example
                    SpawnInitialEnemiesForLevel(6); // If you decide to have initial spawns for L6
                    lastEnemySpawnTime = DateTime.Now.AddSeconds(-enemySpawnInterval.TotalSeconds + 1.0);
                    break;
                default:
                    // ... (game over logic) ...
                    gameOver = true;
                    gameMessage = $"You've completed all memories! Thanks for playing!\nFinal Flower State: {flower.State}";
                    gameTimer.Stop();
                    break;
            }
            levelComplete = false;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (gameOver || levelComplete)
            {
                this.Invalidate();
                return;
            }
            UpdateGame();
            this.Invalidate();
        }

        private void UpdateGame()
        {
            // Player Movement
            float dx = 0;
            float dy = 0;
            bool allowPlayerMovementInput = true;

            if (currentLevel >= 2 && currentLevel <= 4) // Levels 2, 3, 4: Player is static
            {
                allowPlayerMovementInput = false;
            }
    
            if (allowPlayerMovementInput)
            {
                // Horizontal Movement
                if (currentLevel != 5) // No horizontal movement in Level 5
                {
                    if (pressedKeys.Contains(Keys.Left) || pressedKeys.Contains(Keys.A)) dx = -player.Speed;
                    if (pressedKeys.Contains(Keys.Right) || pressedKeys.Contains(Keys.D)) dx = player.Speed;
                }

                // Vertical Movement (allowed if CanMoveFreely is true, which it is for L1, L5, L6)
                if (player.CanMoveFreely) // player.CanMoveFreely is set to true for Level 5 in SetupLevel
                {
                    if (pressedKeys.Contains(Keys.Up) || pressedKeys.Contains(Keys.W)) dy = -player.Speed;
                    if (pressedKeys.Contains(Keys.Down) || pressedKeys.Contains(Keys.S)) dy = player.Speed;
                }
        
                if (dx != 0 || dy != 0)
                {
                    player.Move(dx, dy, this.ClientRectangle);
                }
            }


            // ... (Player Projectile Update logic - same) ...
            for (int i = playerProjectiles.Count - 1; i >= 0; i--)
            {
                playerProjectiles[i].Update();
                if (playerProjectiles[i].Position.X < -playerProjectiles[i].Size.Width ||
                    playerProjectiles[i].Position.X > this.ClientSize.Width ||
                    playerProjectiles[i].Position.Y < -playerProjectiles[i].Size.Height ||
                    playerProjectiles[i].Position.Y > this.ClientSize.Height)
                {
                    playerProjectiles.RemoveAt(i);
                }
            }


            // Enemy Spawning (timed, after initial wave)
            if ((currentLevel >= 2 && currentLevel <= 4 || currentLevel == 6) && // Condition for which levels spawn timed enemies
                (DateTime.Now - lastEnemySpawnTime) > enemySpawnInterval &&
                 enemies.Count < (currentLevel == 4 ? 10 : currentLevel == 6 ? 8 : 7)) // Max enemies
            {
                EnemyType typeToSpawn = currentLevel switch {
                    2 => EnemyType.Slow, 3 => EnemyType.Fast, 4 => EnemyType.Faster, 6 => EnemyType.Slow, _ => EnemyType.Slow
                };
                
                float spawnY;
                if ((currentLevel >= 2 && currentLevel <= 4)) // For levels where player is static
                {
                    spawnY = player.Position.Y + random.Next(-player.Size.Height*2, player.Size.Height*2);
                    spawnY = Math.Max(EnemyBaseSize, Math.Min(this.ClientSize.Height - EnemyBaseSize*2, spawnY));
                } else { // For other levels like 6 where player might move, or general spawning
                    spawnY = random.Next(EnemyBaseSize, this.ClientSize.Height - EnemyBaseSize);
                }

                // --- MODIFIED spawnX LOGIC FOR TIMED SPAWNS ---
                float spawnX;
                if (currentLevel >= 2 && currentLevel <= 4) // Levels 2, 3, 4: Enemies ONLY from the right
                {
                    spawnX = this.ClientSize.Width + random.Next(20, 150); // Random offset off-screen right
                }
                else // For other levels (e.g., Level 6, or future levels) allow random left/right
                {
                    spawnX = (random.Next(0,2) == 0) ? -EnemyBaseSize - random.Next(20,100) : this.ClientSize.Width + random.Next(20,150);
                }
                                
                Enemy newEnemy = new Enemy(spawnX, spawnY, EnemyBaseSize, typeToSpawn);

                // Set speed direction based on spawn X and general target (flower/player)
                // For levels 2, 3, 4, spawnX will always be > targetForEnemy.X, so speed will be negative.
                PointF targetForEnemy = (currentLevel >= 2 && currentLevel <= 4) ? player.Position : flower.Position;
                if (spawnX > targetForEnemy.X) 
                {
                    newEnemy.Speed = -Math.Abs(newEnemy.Speed);
                }
                else 
                {
                    newEnemy.Speed = Math.Abs(newEnemy.Speed); // This case applies if spawning from left is possible (e.g., Level 6)
                }
                
                enemies.Add(newEnemy);
                System.Diagnostics.Debug.WriteLine($"L{currentLevel} SPAWN_TIMED: Added enemy. Pos:({newEnemy.Position.X},{newEnemy.Position.Y}), Speed:{newEnemy.Speed}. Total enemies: {enemies.Count}");
                lastEnemySpawnTime = DateTime.Now;
                
                // Update Collectibles (and remove if expired)
                for (int i = activeCollectibles.Count - 1; i >= 0; i--)
                {
                    activeCollectibles[i].Update(); // This will set IsExpired if lifespan is up
                    if (activeCollectibles[i].IsExpired)
                    {
                        activeCollectibles.RemoveAt(i);
                    }
                }
            }

            // ... (Update Enemies, Boss Logic, Update Enemy Projectiles, Update Collectibles, Feedback Message Expiry - same) ...
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                PointF targetPos = (currentLevel >=2 && currentLevel <=4) ? player.Position : flower.Position;
                enemies[i].Update(targetPos); 

                if (enemies[i].Bounds.IntersectsWith(flower.Bounds))
                {
                    flower.TakeDamage();
                    enemies.RemoveAt(i);
                    if (flower.State == FlowerState.Dead) { gameOver = true; gameMessage = "The memory faded... The flower is gone.\nPress Enter to Restart."; gameTimer.Stop(); return;}
                }
                else if (enemies[i].Position.X < -enemies[i].Size.Width * 2 || enemies[i].Position.X > this.ClientSize.Width + enemies[i].Size.Width)
                {
                    enemies.RemoveAt(i);
                }
            }

            // ... (Boss Logic, Update Enemy Projectiles, Update Collectibles, Feedback Message Expiry - same as before) ...
            // In GameForm.cs, inside UpdateGame() method, within the if (boss != null) block:

            if (boss != null)
            {
                boss.Update(this.ClientSize.Width, this.ClientSize.Height);

                // Handle Special Attack State
                if (isBossPerformingSpecialAttack)
                {
                    if (DateTime.Now < bossSpecialAttackEndTime)
                    {
                        // Fire projectiles during special attack (e.g., circular burst)
                        // This will fire very rapidly, adjust projectile count or add small tick delay
                        int projectilesInBurst = 24; // Example: 24 projectiles in a full circle
                        PointF bossCenter = new PointF(boss.Position.X + boss.Size.Width / 2f, boss.Position.Y + boss.Size.Height / 2f);
                        
                        // Fire a few projectiles per game tick during the special attack to make it a stream/burst
                        // This needs careful tuning to avoid too many objects at once.
                        // Let's fire a full circle over the duration.
                        // A simpler approach: fire a few aimed shots or a small pattern each tick of special.

                        // For a "barrage from all directions" effect, let's do a circular burst more slowly
                        // over the duration, or one big burst then wait.
                        // Let's try one big burst at the start of the special.
                        // This logic below is for a continuous stream during the special attack.

                        if (specialAttackProjectilesPerTick < 2) // Fire 2 projectiles per game tick (approx 120/sec if 60fps) - DANGEROUS!
                        {                                       // This needs a sub-timer or a counter for the pattern.

                            // Let's simplify: Fire a full circle burst ONCE when special attack starts,
                            // or a few projectiles each tick in random/sweeping directions.

                            // Example: Fire 3 random direction projectiles this tick
                            for(int r=0; r<2; r++) { // Fire 2 random projectiles per tick of special
                                float randomAngleRad = (float)(random.NextDouble() * 2 * Math.PI);
                                float velX = (float)Math.Cos(randomAngleRad) * (EnemyProjectileSpeed * 0.8f); // Slightly slower special projectiles
                                float velY = (float)Math.Sin(randomAngleRad) * (EnemyProjectileSpeed * 0.8f);
                                enemyProjectiles.Add(new Projectile(
                                    bossCenter.X - ProjectileSize / 2f, bossCenter.Y - ProjectileSize / 2f,
                                    ProjectileSize, Color.OrangeRed, new PointF(velX, velY), ProjectileType.Enemy));
                            }
                        }
                        // A better way for a "barrage" might be to spawn a pattern over several ticks.
                        // For now, this is a rapid random fire.
                    }
                    else
                    {
                        isBossPerformingSpecialAttack = false; // Special attack ended
                        lastBossSpecialAttackTime = DateTime.Now; // Reset timer for next special
                    }
                }
                else // Normal shooting logic
                {
                    // Check if it's time to START a special attack
                    if ((DateTime.Now - lastBossSpecialAttackTime) > bossSpecialAttackInterval)
                    {
                        isBossPerformingSpecialAttack = true;
                        bossSpecialAttackEndTime = DateTime.Now.Add(bossSpecialAttackDuration);
                        temporaryFeedbackMessage = "Boss Special Attack!";
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(1);
                        // One large burst at the start of the special could be here:
                        // FireCircularBurst(18, bossCenter, EnemyProjectileSpeed * 0.7f, Color.OrangeRed);

                    } // Normal spread shot (if not in special attack cooldown right after it finishes)
                    else if ((DateTime.Now - lastBossShootTime) > bossShootCooldown) 
                    {
                        // ... (existing spread shot logic from Revision 1)
                        int projectilesInSpread = 3; 
                        float totalSpreadAngleDegrees = 40f; 
                        PointF boss_Center = new PointF(boss.Position.X + boss.Size.Width / 2f, boss.Position.Y + boss.Size.Height / 2f);
                        PointF player_Center = new PointF(player.Position.X + player.Size.Width / 2f, player.Position.Y + player.Size.Height / 2f);
                        float angleToPlayerRad = (float)Math.Atan2(player_Center.Y - boss_Center.Y, player_Center.X - boss_Center.X);
                        float startAngleRad = angleToPlayerRad - (totalSpreadAngleDegrees / 2f * (float)Math.PI / 180f);
                        float angleIncrementRad = 0f;
                        if (projectilesInSpread > 1) angleIncrementRad = (totalSpreadAngleDegrees * (float)Math.PI / 180f) / (projectilesInSpread - 1);

                        for (int k = 0; k < projectilesInSpread; k++)
                        {
                            float currentAngleRad = startAngleRad + (k * angleIncrementRad);
                            float velocityX = (float)Math.Cos(currentAngleRad) * EnemyProjectileSpeed;
                            float velocityY = (float)Math.Sin(currentAngleRad) * EnemyProjectileSpeed;
                            enemyProjectiles.Add(new Projectile(
                                boss_Center.X - ProjectileSize / 2f, boss_Center.Y - ProjectileSize / 2f,
                                ProjectileSize, Color.HotPink, new PointF(velocityX, velocityY), ProjectileType.Enemy));
                        }
                        lastBossShootTime = DateTime.Now;
                    }
                }
            }

            for (int i = enemyProjectiles.Count - 1; i >= 0; i--)
            {
                enemyProjectiles[i].Update();
                if (enemyProjectiles[i].Position.Y > this.ClientSize.Height || enemyProjectiles[i].Position.Y < -enemyProjectiles[i].Size.Height ||
                    enemyProjectiles[i].Position.X > this.ClientSize.Width || enemyProjectiles[i].Position.X < -enemyProjectiles[i].Size.Width)
                {
                    enemyProjectiles.RemoveAt(i);
                }
            }
            for (int i = activeCollectibles.Count - 1; i >= 0; i--)
            {
                activeCollectibles[i].Update();
            }
            if (DateTime.Now > feedbackMessageExpiry)
            {
                temporaryFeedbackMessage = "";
            }


            CheckCollisions();
            CheckLevelCompletion();
        }
        
        // REVISION 2: Mouse Click Shooting
        private void GameForm_MouseClick(object sender, MouseEventArgs e)
        {
            // No shooting if game over, level complete, or in level 1 (no enemies)
            // Or if player is static and shouldn't shoot (but current assumption is static player CAN shoot)
            if (gameOver || levelComplete || currentLevel == 1) return;

            // Only shoot on left click and if cooldown has passed
            if (e.Button == MouseButtons.Left && (DateTime.Now - lastPlayerShootTime) > playerShootCooldown)
            {
                PointF playerCenter = new PointF(player.Position.X + player.Size.Width / 2f, player.Position.Y + player.Size.Height / 2f);
                
                // Calculate direction vector from player center to mouse click
                float dirX = e.X - playerCenter.X;
                float dirY = e.Y - playerCenter.Y;
                float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

                if (length > 0) // Ensure there's a direction (avoid division by zero)
                {
                    // Normalize direction vector
                    float normalizedDirX = dirX / length;
                    float normalizedDirY = dirY / length;

                    // Calculate velocity
                    float velocityX = normalizedDirX * PlayerProjectileSpeed;
                    float velocityY = normalizedDirY * PlayerProjectileSpeed;

                    // Calculate spawn position: slightly outside the player's radius in the direction of the shot
                    // This helps avoid the projectile spawning inside the player
                    float spawnRadiusOffset = player.Size.Width / 2f + 2; // Player radius + small gap
                    float spawnX = playerCenter.X + normalizedDirX * spawnRadiusOffset - ProjectileSize / 2f;
                    float spawnY = playerCenter.Y + normalizedDirY * spawnRadiusOffset - ProjectileSize / 2f;
                    
                    playerProjectiles.Add(new Projectile(
                        spawnX,
                        spawnY,
                        ProjectileSize,
                        Color.LightSkyBlue,
                        new PointF(velocityX, velocityY),
                        ProjectileType.Player));

                    lastPlayerShootTime = DateTime.Now;
                }
            }
        }


        private void CheckCollisions()
        {
            // Player Projectiles vs Enemies (Enemy drop logic is already here)
            for (int i = playerProjectiles.Count - 1; i >= 0; i--)
            {
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (playerProjectiles[i].Bounds.IntersectsWith(enemies[j].Bounds))
                    {
                        enemies[j].Health--;
                        if (enemies[j].Health <= 0)
                        {
                            if (currentLevel >= 2 && currentLevel <= 4)
                            {
                                if (random.NextDouble() < COLLECTIBLE_DROP_CHANCE)
                                {
                                    CollectibleType dropType = (random.Next(0, 2) == 0) ? CollectibleType.Water : CollectibleType.Fertilizer;
                                    // CORRECTED Spawn Offset for Collectible:
                                    float collectibleSpawnOffsetX = enemies[j].Position.X + enemies[j].Size.Width / 2f - Collectible.DefaultSize / 2f;
                                    float collectibleSpawnOffsetY = enemies[j].Position.Y + enemies[j].Size.Height / 2f - Collectible.DefaultSize / 2f;
                                    activeCollectibles.Add(new Collectible(collectibleSpawnOffsetX, collectibleSpawnOffsetY, dropType));
                                }
                            }
                            enemies.RemoveAt(j);
                        }
                        playerProjectiles.RemoveAt(i);
                        break; 
                    }
                }
            }

            // ... (Rest of CheckCollisions: Player Projectiles vs Boss, Enemy Projectiles vs Player, Player vs Enemies, Player vs Boss, Player vs Collectibles - mostly same as before) ...
            // Player Projectiles vs Boss
            if (boss != null)
            {
                for (int i = playerProjectiles.Count - 1; i >= 0; i--)
                {
                    if (playerProjectiles[i] != null && playerProjectiles[i].Bounds.IntersectsWith(boss.Bounds))
                    {
                        // --- ADD DEBUG LINE AND TEMPORARY FEEDBACK ---
                        System.Diagnostics.Debug.WriteLine($"BOSS HIT! Current HP: {boss.Health}, Projectile: {playerProjectiles[i].Position}");
                        boss.Health--;
                        temporaryFeedbackMessage = $"Boss Hit! HP: {boss.Health}"; // Visual feedback
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(1);
                        // --- END DEBUG ---
                        playerProjectiles.RemoveAt(i);
                        if (boss.Health <= 0)
                        {
                            System.Diagnostics.Debug.WriteLine("BOSS DEFEATED!"); // Confirm defeat
                            boss = null; 
                        }
                        break; 
                    }
                }
            }
            // Ensure Player vs Collectibles only happens if player CAN move to them, or if collectibles can move to player.
            // Since player is static in Lvl 2-4, collectibles should ideally fall near player or player has a small collection radius.
            // For now, player needs to be on top of them, which won't happen if static.
            // Let's give player a small collection radius around them for levels 2-4.

            // Player vs Collectibles (MODIFIED for static player in Lvl 2-4)
            if (currentLevel >= 2 && currentLevel <= 4)
            {
                RectangleF collectionZone = player.Bounds;
                if (currentLevel >= 2 && currentLevel <= 4) // If player is static
                {
                    collectionZone.Inflate(player.Size.Width * inflation, player.Size.Height * inflation); // Player has a small aura to collect
                }

                for (int i = activeCollectibles.Count - 1; i >= 0; i--)
                {
                    if (collectionZone.IntersectsWith(activeCollectibles[i].Bounds)) // Use collectionZone
                    {
                        Collectible collected = activeCollectibles[i];
                        if (collected.Type == CollectibleType.Water) { waterProgress++; temporaryFeedbackMessage = "+1 Water!"; }
                        else if (collected.Type == CollectibleType.Fertilizer) { fertilizerProgress++; temporaryFeedbackMessage = "+1 Fertilizer!"; }
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(1.5);
                        activeCollectibles.RemoveAt(i);
                    }
                }
            }
            // Enemy Projectiles vs Player
            for (int i = enemyProjectiles.Count - 1; i >= 0; i--)
            {
                if (enemyProjectiles[i].Bounds.IntersectsWith(player.Bounds))
                {
                    enemyProjectiles.RemoveAt(i);
                    flower.TakeDamage(); 
                    if (flower.State == FlowerState.Dead) {
                        gameOver = true;
                        gameMessage = "You couldn't protect the memory... It shattered.\nPress Enter to Restart.";
                    } else {
                        temporaryFeedbackMessage = "Ouch! Be careful!";
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(2);
                    }
                    if(gameOver) gameTimer.Stop();
                    return;
                }
            }

            // Player vs Enemies (direct collision)
            // This might not happen if player is static and far from where enemies reach flower,
            // but keeping it for edge cases or if enemies pass through player to get to flower.
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (player.Bounds.IntersectsWith(enemies[i].Bounds))
                {
                     enemies.RemoveAt(i); 
                    flower.TakeDamage(); 
                     if (flower.State == FlowerState.Dead) {
                        gameOver = true;
                        gameMessage = "The chaos was too much for the fragile memory.\nPress Enter to Restart.";
                    } else {
                        temporaryFeedbackMessage = "They got too close to you!";
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(2);
                    }
                    if(gameOver) gameTimer.Stop();
                    return;
                }
            }
            
            for (int i = playerProjectiles.Count - 1; i >= 0; i--)
            {
                for (int j = enemyProjectiles.Count - 1; j >= 0; j--)
                {
                    // Important: Check if projectiles still exist in lists, as one might have been removed
                    // by a collision with something else in the same CheckCollisions() call.
                    // However, iterating backwards helps mitigate issues with removal.
                    if (i < playerProjectiles.Count && j < enemyProjectiles.Count) // Ensure valid indices
                    {
                        if (playerProjectiles[i].Bounds.IntersectsWith(enemyProjectiles[j].Bounds))
                        {
                            System.Diagnostics.Debug.WriteLine("PROJECTILE CLASH!");
                            playerProjectiles.RemoveAt(i);
                            enemyProjectiles.RemoveAt(j);
                            // No need for temporaryFeedbackMessage here unless desired, can be spammy
                            goto nextPlayerProjectileCheck; // Break from inner loop (j) and continue outer (i)
                        }
                    }
                }
                nextPlayerProjectileCheck:; // Label for the goto
            }
            
            for (int i = playerProjectiles.Count - 1; i >= 0; i--)
            {
                for (int j = enemyProjectiles.Count - 1; j >= 0; j--)
                {
                    // Important: Check if projectiles still exist in lists, as one might have been removed
                    // by a collision with something else in the same CheckCollisions() call.
                    // However, iterating backwards helps mitigate issues with removal.
                    if (i < playerProjectiles.Count && j < enemyProjectiles.Count) // Ensure valid indices
                    {
                        if (playerProjectiles[i].Bounds.IntersectsWith(enemyProjectiles[j].Bounds))
                        {
                            System.Diagnostics.Debug.WriteLine("PROJECTILE CLASH!");
                            playerProjectiles.RemoveAt(i);
                            enemyProjectiles.RemoveAt(j);
                            // No need for temporaryFeedbackMessage here unless desired, can be spammy
                            goto nextPlayerProjectileCheck; // Break from inner loop (j) and continue outer (i)
                        }
                    }
                }
                nextPlayerProjectileCheck:; // Label for the goto
            }
        }

        // ... (CheckLevelCompletion - same as before) ...
        private void CheckLevelCompletion()
        {
            if (levelComplete) return;
            bool conditionsMet = false;

            switch (currentLevel)
            {
                case 1:
                    if (waterProgress >= waterGoal && fertilizerProgress >= fertilizerGoal) conditionsMet = true;
                    break;
                case 2: case 3: case 4:
                    if (waterProgress >= waterGoal && fertilizerProgress >= fertilizerGoal) conditionsMet = true;
                    break;
                case 5:
                    if (boss == null && !gameOver) conditionsMet = true;
                    break;
                case 6:
                     if (enemies.Count == 0 && !AnyEnemiesSpawningSoon() && flower.State != FlowerState.Dead && !gameOver && !levelComplete)
                    {
                        // For level 6, let's make it complete explicitly via a message or after a wave for simplicity for now
                        // This could be a placeholder for a more specific win condition.
                        // Let's assume if player survives initial spawns and flower isn't dead, then can proceed.
                        // To avoid auto-complete if just one enemy spawned and killed, let's add a small delay or a dummy goal.
                        // For now, it will complete if enemies list is empty and no more are spawning soon.
                    }
                    break;
            }

            if (conditionsMet)
            {
                levelComplete = true;
                gameTimer.Stop();
                gameMessage = $"Level {currentLevel} Complete!\nPress Enter to Continue.";
                temporaryFeedbackMessage = ""; 
            }
        }
        private bool AnyEnemiesSpawningSoon()
        {
             if ((currentLevel >= 2 && currentLevel <= 4 || currentLevel == 6) && 
                 (DateTime.Now - lastEnemySpawnTime) <= enemySpawnInterval && 
                 enemies.Count < (currentLevel == 4 ? 12 : currentLevel == 6 ? 7 : 8) ) // Check against max enemies too
                return true;
            return false;
        }


        // ... (GameForm_Paint - mostly same, ensure game messages are updated if needed) ...
        private void GameForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            flower.Draw(g);
            player.Draw(g);

            foreach (var projectile in playerProjectiles) projectile.Draw(g);
            foreach (var collectible in activeCollectibles) collectible.Draw(g);
            foreach (var enemy in enemies) enemy.Draw(g);
            if (boss != null) boss.Draw(g);
            foreach (var projectile in enemyProjectiles) projectile.Draw(g);
            
            if (currentLevel >= 2 && currentLevel <= 4 && !gameOver && !levelComplete)
            {
                // Draw the player's actual bounds (for reference)
                using (Pen playerPen = new Pen(Color.FromArgb(100, Color.LightGreen), 1)) // Semi-transparent green
                {
                    e.Graphics.DrawRectangle(playerPen, Rectangle.Round(player.Bounds));
                }

                // Calculate and draw the inflated collection zone
                RectangleF cz = player.Bounds;
                cz.Inflate(player.Size.Width * inflation, player.Size.Height * inflation); // Inflation by 30 on each side
                using (Pen zonePen = new Pen(Color.FromArgb(150, Color.Yellow), 2)) // Semi-transparent yellow, thicker
                {
                    e.Graphics.DrawRectangle(zonePen, Rectangle.Round(cz));
                    // Optionally, draw text to confirm zone coords for precise debugging
                    // string zoneInfo = $"CZ:({cz.X:F0},{cz.Y:F0}) W:{cz.Width:F0} H:{cz.Height:F0}";
                    // e.Graphics.DrawString(zoneInfo, smallFont, Brushes.Yellow, cz.Location.X, cz.Location.Y - 15);
                }

                // Draw bounds of active collectibles
                foreach (var collectible in activeCollectibles)
                {
                    using (Pen collectiblePen = new Pen(Color.FromArgb(150, Color.Cyan), 1)) // Semi-transparent cyan
                    {
                        e.Graphics.DrawRectangle(collectiblePen, Rectangle.Round(collectible.Bounds));
                        // string collInfo = $"C:({collectible.Position.X:F0},{collectible.Position.Y:F0})";
                        // e.Graphics.DrawString(collInfo, smallFont, Brushes.Cyan, collectible.Position.X, collectible.Position.Y - 10);
                    }
                }
            }

            string levelText = $"Level: {currentLevel}";
            string flowerStateText = $"Flower: {flower.State}";
            SizeF levelTextSize = g.MeasureString(levelText, gameFont);
            g.DrawString(levelText, gameFont, Brushes.LightGray, 10, 10);
            g.DrawString(flowerStateText, gameFont, Brushes.LightGray, 10, 10 + levelTextSize.Height + 5);

            string objectiveText = "";
            if (currentLevel == 1) objectiveText = $"Care: (E) Water: {waterProgress}/{waterGoal} | (R) Fertilizer: {fertilizerProgress}/{fertilizerGoal}";
            else if (currentLevel >= 2 && currentLevel <= 4) objectiveText = $"Collect: Water: {waterProgress}/{waterGoal} | Fertilizer: {fertilizerProgress}/{fertilizerGoal}";
            
            if (!string.IsNullOrEmpty(objectiveText))
            {
                g.DrawString(objectiveText, objectiveFont, Brushes.Gold, 10, 10 + levelTextSize.Height + 5 + g.MeasureString(flowerStateText, gameFont).Height + 5);
            }
            
            if (!string.IsNullOrEmpty(temporaryFeedbackMessage))
            {
                SizeF feedbackSize = g.MeasureString(temporaryFeedbackMessage, gameFont);
                float feedbackX = (this.ClientSize.Width - feedbackSize.Width) / 2;
                float feedbackY = player.Position.Y - player.Size.Height - 15; 
                if (feedbackY < 10) feedbackY = this.ClientSize.Height - 40; 
                g.DrawString(temporaryFeedbackMessage, gameFont, Brushes.Lime, feedbackX, feedbackY);
            }

            // BUG FIX 1: Adjusted message display logic
            if (gameOver || levelComplete) // Big overlay for game over or level complete
            {
                using (SolidBrush overlayBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                {
                    g.FillRectangle(overlayBrush, 0, this.ClientSize.Height / 3f, this.ClientSize.Width, this.ClientSize.Height / 3f);
                }
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                Rectangle rectMessage = new Rectangle(0, (int)(this.ClientSize.Height / 3f), this.ClientSize.Width, (int)(this.ClientSize.Height / 3f));
                
                TextRenderer.DrawText(g, gameMessage, titleFont, rectMessage, Color.White, flags); // gameMessage is set appropriately by SetupLevel or completion/game over

                // Restart/Continue prompts
                 if (gameOver && currentLevel < 7) { 
                     string restartMsg = "Press Enter to Restart Level or Escape to Quit.";
                     Rectangle rectRestart = new Rectangle(0, (int)(this.ClientSize.Height * 2/3f) - 30 , this.ClientSize.Width, 60);
                     TextRenderer.DrawText(g, restartMsg, gameFont, rectRestart, Color.LightCyan, flags);
                } else if (levelComplete && currentLevel < 6) { // Ensure this doesn't overlap with final message for L6 complete
                     string continueMsg = "Press Enter to Proceed to Next Memory.";
                     Rectangle rectContinue = new Rectangle(0, (int)(this.ClientSize.Height * 2/3f) -30 , this.ClientSize.Width, 60);
                     TextRenderer.DrawText(g, continueMsg, gameFont, rectContinue, Color.LightGreen, flags);
                } else if (currentLevel == 6 && levelComplete){ // Specific message for completing level 6
                     string endMsg = "The final memory piece is fragile but preserved...\nPress Enter to see the outcome.";
                     Rectangle rectEnd = new Rectangle(0, (int)(this.ClientSize.Height * 2/3f) -30 , this.ClientSize.Width, 80);
                     TextRenderer.DrawText(g, endMsg, gameFont, rectEnd, Color.Gold, flags);
                }
            }
            else if (currentLevel == 1 && !levelComplete) // Show small top instruction for Level 1 if active and not complete
            {
                 TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak;
                 Rectangle rectInstruction = new Rectangle(this.ClientSize.Width / 4, 30, this.ClientSize.Width / 2, 60); // Moved down a bit
                 // Use the gameMessage which is already set for Level 1
                 TextRenderer.DrawText(g, gameMessage, gameFont, rectInstruction, Color.Aqua, flags);
            }
            // Level 6 specific drawing
            if (currentLevel == 6)
            {
                string condition = $"Flower Condition: {flower.State}";
                SizeF conditionSize = g.MeasureString(condition, gameFont);
                using (SolidBrush conditionBrush = new SolidBrush(flower.GetCurrentColor()))
                {
                    g.DrawString(condition, gameFont, conditionBrush, this.ClientSize.Width - conditionSize.Width - 10, 10);
                }
                g.DrawString("Press 'S' to Save Screen (Mock)", smallFont, Brushes.LightGray, this.ClientSize.Width - 200, 10 + conditionSize.Height + 5);
            }
        }


        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Add to pressedKeys for continuous movement, handled in UpdateGame
            if (!pressedKeys.Contains(e.KeyCode)) // Avoid redundant additions if already pressed
                 pressedKeys.Add(e.KeyCode);


            if (gameOver)
            {
                if (e.KeyCode == Keys.Enter) { if(currentLevel < 7) InitializeGame(); }
                else if (e.KeyCode == Keys.Escape) { this.Close(); }
                return;
            }

            if (levelComplete)
            {
                if (e.KeyCode == Keys.Enter) { currentLevel++; InitializeGame(); }
                return;
            }

            // Level 1: Water and Fertilize Actions (REVISION 1: New Keys E/R)
            if (currentLevel == 1 && (DateTime.Now - lastPlayerCareActionTime) > playerCareActionCooldown)
            {
                RectangleF interactionBounds = flower.Bounds;
                interactionBounds.Inflate(player.Size.Width, player.Size.Height);

                if (player.Bounds.IntersectsWith(interactionBounds))
                {
                    if (e.KeyCode == Keys.E) // Water (WAS 'W')
                    {
                        if (waterProgress < waterGoal) { waterProgress++; temporaryFeedbackMessage = "Watered!"; }
                        else { temporaryFeedbackMessage = "Flower has enough water."; }
                        lastPlayerCareActionTime = DateTime.Now;
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(1.5);
                    }
                    else if (e.KeyCode == Keys.R) // Fertilize (WAS 'F')
                    {
                         if (fertilizerProgress < fertilizerGoal) { fertilizerProgress++; temporaryFeedbackMessage = "Fertilized!"; }
                         else { temporaryFeedbackMessage = "Flower has enough fertilizer."; }
                        lastPlayerCareActionTime = DateTime.Now;
                        feedbackMessageExpiry = DateTime.Now.AddSeconds(1.5);
                    } else if (e.KeyCode == Keys.Q) { currentLevel++; InitializeGame(); } // DEBUG, REMOVE LATER
                }
            }
            
            if (currentLevel == 6 && e.KeyCode == Keys.S) {  MessageBox.Show("Screenshot 'saved'! (This is a placeholder)", "A Piece of Memory - Save", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
        }

        // ... (OnFormClosed - same as before) ...
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            gameTimer?.Stop();
            gameTimer?.Dispose();
            gameFont?.Dispose();
            titleFont?.Dispose();
            smallFont?.Dispose();
            objectiveFont?.Dispose();
        }
    }
}