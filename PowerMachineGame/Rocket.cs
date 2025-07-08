using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WordShooter
{
    public class Rocket
    {
        public Vector2 Position { get; private set; }
        public bool WasShot { get; set; } = false;
        public bool IsExploding { get; private set; } = false;
        public float ExplosionTimer { get; private set; } = 0f;
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - 16,  // Half of 32 width
            (int)Position.Y - 16,  // Half of 32 height
            32, 32);  // Matching plane size
            
        private readonly Texture2D _texture;
        private readonly float _speed;
        private const float TARGET_SIZE = 32f;
        private Vector2 _playerPosition;
        private Vector2 _direction;

        public Rocket(Texture2D texture, Vector2 position, float speed, Vector2 playerPosition) 
        {
            _texture = texture;
            Position = position;
            _speed = speed;
            _playerPosition = playerPosition;
            _direction = _playerPosition - Position;
            _direction.Normalize();
        }

        public void Update(float deltaTime)
        {
            if (WasShot && !IsExploding)
            {
                IsExploding = true;
                ExplosionTimer = 0.25f;
            }

            if (IsExploding)
            {
                ExplosionTimer -= deltaTime;
            }
            else
            {
                // Move toward player position
                Position += _direction * _speed * deltaTime;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float scaleX = TARGET_SIZE / _texture.Width;
            float scaleY = TARGET_SIZE / _texture.Height;
            
            if (IsExploding)
            {
                float explosionScale = 1 + (1 - ExplosionTimer * 4);
                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.Red,
                    MathHelper.Pi,
                    new Vector2(_texture.Width / 2, _texture.Height / 2),
                    new Vector2(scaleX, scaleY) * explosionScale,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                // Calculate rotation to face player
                float rotation = (float)Math.Atan2(_direction.Y, _direction.X) + MathHelper.PiOver2;
                
                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    rotation,
                    new Vector2(_texture.Width / 2, _texture.Height / 2),
                    new Vector2(scaleX, scaleY),
                    SpriteEffects.None,
                    0f);
            }
        }

        public bool ShouldRemove()
        {
            return IsExploding && ExplosionTimer <= 0;
        }
    }
}