using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace WordShooter
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        public enum GameState { MainMenu, CharacterSelect, Playing, Paused, GameOver }
        public GameState CurrentState { get; set; }
        
        private PlayerPlane _player;
        private List<EnemyPlane> _enemies;
        private List<Rocket> _rockets;
        private CharacterSelectScreen _characterSelect;
        private GameHUD _hud;
        
        private Texture2D _planeSprites;
        private Texture2D _rocketTexture;
        private Texture2D _projectileTexture;
        private Texture2D _menuBackground;
        private Texture2D _gameBackground;
        private SpriteFont _font;
        
        private float _enemySpawnTimer = 0f;
        private float _rocketSpawnTimer = 0f;
        private const float BASE_SPAWN_INTERVAL = 3f;
        private const float ROCKET_SPAWN_INTERVAL = 5f;
        private float _currentSpawnInterval;
        private int _score = 0;
        private int _level = 1;
        private const int MAX_LEVEL = 20;
        private int _enemiesPerSpawn = 1;
        private bool _gameWon = false;
        private KeyboardState _previousKeyboardState;
        private List<string> _wordList = new List<string>
        {
            "energy", "system", "machine", "voltage", "current",
            "power", "reactor", "activate", "fusion", "circuit"
        };

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();
            
            CurrentState = GameState.MainMenu;
            _enemies = new List<EnemyPlane>();
            _rockets = new List<Rocket>();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _planeSprites = Content.Load<Texture2D>("PixelPlanesAssetPack");
            _rocketTexture = Content.Load<Texture2D>("rocket");
            _projectileTexture = Content.Load<Texture2D>("projectile");
            _menuBackground = Content.Load<Texture2D>("Background");
            _gameBackground = Content.Load<Texture2D>("Background-2");
            _font = Content.Load<SpriteFont>("DefaultFont");
            
            _characterSelect = new CharacterSelectScreen(_planeSprites, _font);
            _hud = new GameHUD(_font);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                if (CurrentState == GameState.Playing)
                    CurrentState = GameState.Paused;
                else if (CurrentState == GameState.Paused)
                    CurrentState = GameState.MainMenu;
                else if (CurrentState == GameState.MainMenu)
                    Exit();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            switch (CurrentState)
            {
                case GameState.MainMenu:
                    if (keyboardState.IsKeyDown(Keys.Enter))
                        CurrentState = GameState.CharacterSelect;
                    break;
                    
                case GameState.CharacterSelect:
                    _characterSelect.Update(gameTime);
                    if (_characterSelect.IsSelectionComplete)
                    {
                        InitializeGame(_characterSelect.SelectedPlaneIndex);
                        CurrentState = GameState.Playing;
                    }
                    break;
                    
                case GameState.Playing:
                    UpdateGame(deltaTime, keyboardState);
                    break;
                    
                case GameState.Paused:
                    if (keyboardState.IsKeyDown(Keys.Enter))
                        CurrentState = GameState.Playing;
                    break;
                    
                case GameState.GameOver:
                    if (keyboardState.IsKeyDown(Keys.Enter))
                    {
                        CurrentState = GameState.MainMenu;
                        _characterSelect.Reset();
                        _gameWon = false;
                    }
                    break;
            }

            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void InitializeGame(int planeIndex)
        {
            Rectangle playerRect = new Rectangle(planeIndex * 32, 0, 32, 32);
            Vector2 centerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2);
                
            _player = new PlayerPlane(
                _planeSprites,
                _projectileTexture,
                playerRect,
                centerPosition);
                
            _enemies.Clear();
            _rockets.Clear();
            _score = 0;
            _level = 1;
            _currentSpawnInterval = BASE_SPAWN_INTERVAL;
            _enemiesPerSpawn = 1;
        }

        private Vector2 GetSpawnPositionAtEdge()
        {
            int side = new System.Random().Next(4); // 0=top, 1=right, 2=bottom, 3=left
            int width = _graphics.PreferredBackBufferWidth;
            int height = _graphics.PreferredBackBufferHeight;
            
            return side switch
            {
                0 => new Vector2(new System.Random().Next(0, width), -50), // Top
                1 => new Vector2(width + 50, new System.Random().Next(0, height)), // Right
                2 => new Vector2(new System.Random().Next(0, width), height + 50), // Bottom
                _ => new Vector2(-50, new System.Random().Next(0, height)) // Left
            };
        }

        private void SpawnEnemy()
        {
            string word = _wordList[new System.Random().Next(_wordList.Count)];
            Rectangle enemyRect = new Rectangle(64, 0, 32, 32);
            float speed = MathHelper.Clamp(50f + (_level * 5f), 50f, 150f);
            
            Vector2 position = GetSpawnPositionAtEdge();
            
            _enemies.Add(new EnemyPlane(
                _planeSprites, 
                enemyRect, 
                word, 
                position, 
                speed,
                _player.Position));
        }

        private void SpawnRocket()
        {
            float speed = MathHelper.Clamp(70f + (_level * 5f), 70f, 200f);
            
            Vector2 position = GetSpawnPositionAtEdge();
            
            _rockets.Add(new Rocket(
                _rocketTexture, 
                position, 
                speed,
                _player.Position));
        }

        private void UpdateGame(float deltaTime, KeyboardState keyboardState)
        {
            _player.Update(keyboardState, deltaTime, _enemies);
            
            // Update level based on score
            int newLevel = (_score / 500) + 1;
            if (newLevel > _level)
            {
                _level = newLevel;
                _currentSpawnInterval = MathHelper.Clamp(BASE_SPAWN_INTERVAL - (_level * 0.2f), 0.5f, BASE_SPAWN_INTERVAL);
                _enemiesPerSpawn = MathHelper.Clamp(1 + (_level / 2), 1, 4);
            }
            
            // Check for victory
            if (_level >= MAX_LEVEL && !_gameWon)
            {
                _gameWon = true;
                CurrentState = GameState.GameOver;
                return;
            }
            
            // Enemy spawning
            _enemySpawnTimer += deltaTime;
            if (_enemySpawnTimer >= _currentSpawnInterval && _enemies.Count < 10) // Increased max enemies
            {
                for (int i = 0; i < _enemiesPerSpawn; i++)
                {
                    SpawnEnemy();
                }
                _enemySpawnTimer = 0f;
            }
            
            // Rocket spawning (after level 6)
            if (_level >= 6)
            {
                _rocketSpawnTimer += deltaTime;
                if (_rocketSpawnTimer >= ROCKET_SPAWN_INTERVAL && _rockets.Count < 4) // Increased max rockets
                {
                    SpawnRocket();
                    _rocketSpawnTimer = 0f;
                }
            }
            
            // Handle spacebar for rockets
            if (keyboardState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
            {
                foreach (var rocket in _rockets.ToList())
                {
                    if (!rocket.WasShot)
                    {
                        rocket.WasShot = true;
                        _score += 50;
                        break;
                    }
                }
            }
            
            // Update projectiles and check collisions
            foreach (var projectile in _player.GetProjectiles().ToList())
            {
                if (projectile.Position.X < 0 || projectile.Position.X > _graphics.PreferredBackBufferWidth ||
                    projectile.Position.Y < 0 || projectile.Position.Y > _graphics.PreferredBackBufferHeight)
                {
                    _player.RemoveProjectile(projectile);
                    continue;
                }

                foreach (var enemy in _enemies.ToList())
                {
                    if (enemy.Bounds.Intersects(projectile.Bounds) && !enemy.WasShot)
                    {
                        _player.RemoveProjectile(projectile);
                        enemy.WasShot = true;
                        _score += 50;
                        break;
                    }
                }
            }

            // Update enemies
            foreach (var enemy in _enemies.ToList())
            {
                bool isCurrentTarget = _player.CurrentTarget == enemy;
                enemy.Update(deltaTime, isCurrentTarget, _player.ShakeTimer);
                
                if (enemy.WasShot)
                {
                    if (enemy.ShouldRemove())
                    {
                        _enemies.Remove(enemy);
                        _score += 100;
                    }
                }
                else if (Vector2.Distance(enemy.Position, _player.Position) < 20) // Check collision with player
                {
                    _enemies.Remove(enemy);
                    _player.Health -= 15;
                    
                    if (_player.Health <= 0)
                    {
                        CurrentState = GameState.GameOver;
                    }
                }
            }
            
            // Update rockets
            foreach (var rocket in _rockets.ToList())
            {
                rocket.Update(deltaTime);
                
                if (rocket.WasShot)
                {
                    if (rocket.ShouldRemove())
                    {
                        _rockets.Remove(rocket);
                    }
                }
                else if (Vector2.Distance(rocket.Position, _player.Position) < 20) // Check collision with player
                {
                    _rockets.Remove(rocket);
                    _player.Health -= 25;
                    
                    if (_player.Health <= 0)
                    {
                        CurrentState = GameState.GameOver;
                    }
                }
            }
        }

        private void DrawProgressBar()
        {
            float progress = (float)_level / MAX_LEVEL;
            progress = MathHelper.Clamp(progress, 0, 1);
            
            int barHeight = 5;
            Rectangle fillRect = new Rectangle(
                0, 
                _graphics.PreferredBackBufferHeight - barHeight,
                (int)(_graphics.PreferredBackBufferWidth * progress), 
                barHeight);
            
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.LimeGreen });
            
            _spriteBatch.Draw(
                pixel,
                fillRect,
                Color.LimeGreen);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            
            switch (CurrentState)
            {
                case GameState.MainMenu:
                case GameState.Paused:
                case GameState.GameOver:
                    _spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, 800, 600), Color.White);
                    break;
                    
                case GameState.Playing:
                    _spriteBatch.Draw(_gameBackground, new Rectangle(0, 0, 800, 600), Color.White);
                    break;
            }

            switch (CurrentState)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;
                    
                case GameState.CharacterSelect:
                    _characterSelect.Draw(_spriteBatch);
                    break;
                    
                case GameState.Playing:
                    DrawGame();
                    break;
                    
                case GameState.Paused:
                    DrawGame();
                    DrawPauseScreen();
                    break;
                    
                case GameState.GameOver:
                    DrawGameOver();
                    break;
            }
            
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawMainMenu()
        {
            string title = "DEFEND THE CENTER";
            string startText = "Press ENTER to Start";
            string exitText = "Press ESC to Exit";
            
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 startSize = _font.MeasureString(startText);
            Vector2 exitSize = _font.MeasureString(exitText);
            
            _spriteBatch.DrawString(_font, title, 
                new Vector2(400 - titleSize.X / 2, 200), 
                Color.White);
                
            _spriteBatch.DrawString(_font, startText,
                new Vector2(400 - startSize.X / 2, 300),
                Color.White);
                
            _spriteBatch.DrawString(_font, exitText,
                new Vector2(400 - exitSize.X / 2, 350),
                Color.White);
        }

        private void DrawPauseScreen()
        {
            _spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, 800, 600), Color.Black * 0.5f);
            
            string pauseText = "PAUSED";
            string resumeText = "Press ENTER to Resume";
            string menuText = "Press ESC for Main Menu";
            
            Vector2 pauseSize = _font.MeasureString(pauseText);
            Vector2 resumeSize = _font.MeasureString(resumeText);
            Vector2 menuSize = _font.MeasureString(menuText);
            
            _spriteBatch.DrawString(_font, pauseText, 
                new Vector2(400 - pauseSize.X / 2, 200), 
                Color.White);
                
            _spriteBatch.DrawString(_font, resumeText,
                new Vector2(400 - resumeSize.X / 2, 300),
                Color.White);
                
            _spriteBatch.DrawString(_font, menuText,
                new Vector2(400 - menuSize.X / 2, 350),
                Color.White);
        }

        private void DrawGame()
        {
            _player.Draw(_spriteBatch, _font);
            
            foreach (var enemy in _enemies)
            {
                bool isCurrentTarget = _player.CurrentTarget == enemy;
                enemy.Draw(_spriteBatch, _font, _player.CurrentLetterIndex, isCurrentTarget);
            }
            
            foreach (var rocket in _rockets)
            {
                rocket.Draw(_spriteBatch);
            }
            
            _hud.Draw(_spriteBatch, _score, _player.Health);
            DrawProgressBar();
        }

        private void DrawGameOver()
        {
            string gameOverText = _gameWon ? "VICTORY!" : "GAME OVER";
            string scoreText = $"Final Score: {_score}";
            string levelText = $"Level Reached: {_level}";
            string restartText = "Press ENTER to return to menu";
            
            Vector2 gameOverSize = _font.MeasureString(gameOverText);
            Vector2 scoreSize = _font.MeasureString(scoreText);
            Vector2 levelSize = _font.MeasureString(levelText);
            Vector2 restartSize = _font.MeasureString(restartText);
            
            _spriteBatch.DrawString(_font, gameOverText, 
                new Vector2(_graphics.PreferredBackBufferWidth / 2 - gameOverSize.X / 2, 200), 
                _gameWon ? Color.Gold : Color.Red);
                
            _spriteBatch.DrawString(_font, scoreText,
                new Vector2(_graphics.PreferredBackBufferWidth / 2 - scoreSize.X / 2, 250),
                Color.White);
                
            _spriteBatch.DrawString(_font, levelText,
                new Vector2(_graphics.PreferredBackBufferWidth / 2 - levelSize.X / 2, 300),
                Color.White);
                
            _spriteBatch.DrawString(_font, restartText,
                new Vector2(_graphics.PreferredBackBufferWidth / 2 - restartSize.X / 2, 350),
                Color.White);
        }
    }
}