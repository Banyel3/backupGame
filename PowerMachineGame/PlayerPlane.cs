using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace WordShooter
{
    public class PlayerPlane
    {
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - _sourceRect.Width / 2,
            (int)Position.Y - _sourceRect.Height / 2,
            _sourceRect.Width,
            _sourceRect.Height);
            
        public Vector2 Position { get; }
        public int Health { get; set; } = 100;
        public int CurrentLetterIndex { get; private set; } = 0;
        public float ShakeTimer { get; private set; } = 0f;
        public EnemyPlane CurrentTarget { get; private set; }
        
        private readonly Texture2D _texture;
        private readonly Texture2D _projectileTexture;
        private readonly Rectangle _sourceRect;
        private KeyboardState _previousKeyboardState;
        private float _shootCooldown = 0f;
        private const float SHOOT_COOLDOWN_TIME = 0.2f;
        private List<Projectile> _projectiles = new List<Projectile>();

        public PlayerPlane(Texture2D texture, Texture2D projectileTexture, Rectangle sourceRect, Vector2 position)
        {
            _texture = texture;
            _projectileTexture = projectileTexture;
            _sourceRect = sourceRect;
            Position = position;
            _previousKeyboardState = Keyboard.GetState();
        }

        public void Update(KeyboardState keyboardState, float deltaTime, List<EnemyPlane> enemies)
        {
            if (ShakeTimer > 0)
            {
                ShakeTimer -= deltaTime;
            }

            if (_shootCooldown > 0)
            {
                _shootCooldown -= deltaTime;
            }

            CurrentTarget = enemies.FirstOrDefault(e => !e.WasShot);
            
            if (CurrentTarget != null)
            {
                foreach (var key in keyboardState.GetPressedKeys())
                {
                    if (_previousKeyboardState.IsKeyUp(key) && key >= Keys.A && key <= Keys.Z)
                    {
                        char pressedChar = key.ToString().ToLower()[0];
                        
                        if (CurrentLetterIndex < CurrentTarget.Word.Length && 
                            pressedChar == CurrentTarget.Word[CurrentLetterIndex])
                        {
                            CurrentLetterIndex++;
                            if (CurrentLetterIndex >= CurrentTarget.Word.Length)
                            {
                                CurrentTarget.WasShot = true;
                                CurrentLetterIndex = 0;
                            }
                            
                            if (_shootCooldown <= 0)
                            {
                                Shoot();
                                _shootCooldown = SHOOT_COOLDOWN_TIME;
                            }
                        }
                        else
                        {
                            CurrentLetterIndex = 0;
                            ShakeTimer = 0.5f;
                        }
                    }
                }
            }
            else
            {
                CurrentLetterIndex = 0;
            }

            foreach (var projectile in _projectiles.ToList())
            {
                projectile.Update(deltaTime);
            }

            _previousKeyboardState = keyboardState;
        }

        private void Shoot()
        {
            if (CurrentTarget != null && !CurrentTarget.WasShot)
            {
                Vector2 direction = CurrentTarget.Position - Position;
                direction.Normalize();
                
                _projectiles.Add(new Projectile(
                    _projectileTexture,
                    Position,
                    direction * 300f));
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Draw(
                _texture,
                Position,
                _sourceRect,
                Color.White,
                0f,
                new Vector2(_sourceRect.Width / 2, _sourceRect.Height / 2),
                1f,
                SpriteEffects.None,
                0f);
                
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(spriteBatch);
            }
        }

        public List<Projectile> GetProjectiles()
        {
            return _projectiles;
        }

        public void RemoveProjectile(Projectile projectile)
        {
            _projectiles.Remove(projectile);
        }
    }
}