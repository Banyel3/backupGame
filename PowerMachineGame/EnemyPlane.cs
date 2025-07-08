using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WordShooter
{
    public class EnemyPlane
    {
        public string Word { get; }
        public Vector2 Position { get; private set; }
        public bool WasShot { get; set; } = false;
        public bool IsExploding { get; private set; } = false;
        public float ExplosionTimer { get; private set; } = 0f;
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - _sourceRect.Width / 2,
            (int)Position.Y - _sourceRect.Height / 2,
            _sourceRect.Width,
            _sourceRect.Height);
            
        private readonly Texture2D _texture;
        private readonly Rectangle _sourceRect;
        private readonly float _speed;
        private float _shakeOffset = 0f;
        private Vector2 _playerPosition;
        private Vector2 _direction;

        public EnemyPlane(Texture2D texture, Rectangle sourceRect, string word, Vector2 position, float speed, Vector2 playerPosition) 
        {
            _texture = texture;
            _sourceRect = sourceRect;
            Word = word;
            Position = position;
            _speed = speed;
            _playerPosition = playerPosition;
            _direction = _playerPosition - Position;
            _direction.Normalize();
        }

        public void Update(float deltaTime, bool isCurrentTarget, float shakeTimer)
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
                _shakeOffset = isCurrentTarget && shakeTimer > 0 ? 
                    (float)Math.Sin(shakeTimer * 50) * 5 : 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, int currentLetterIndex, bool isCurrentTarget)
        {
            if (IsExploding)
            {
                float scale = 1 + (1 - ExplosionTimer * 4);
                spriteBatch.Draw(
                    _texture,
                    Position,
                    _sourceRect,
                    Color.Red,
                    MathHelper.Pi,
                    new Vector2(_sourceRect.Width / 2, _sourceRect.Height / 2),
                    scale,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                // Calculate rotation to face player
                float rotation = (float)Math.Atan2(_direction.Y, _direction.X) + MathHelper.PiOver2;
                
                spriteBatch.Draw(
                    _texture,
                    Position + new Vector2(_shakeOffset, 0),
                    _sourceRect,
                    Color.White,
                    rotation,
                    new Vector2(_sourceRect.Width / 2, _sourceRect.Height / 2),
                    1f,
                    SpriteEffects.None,
                    0f);
                    
                // Draw word with highlighting
                if (!string.IsNullOrEmpty(Word))
                {
                    Vector2 wordPosition = new Vector2(
                        Position.X,
                        Position.Y + _sourceRect.Height);
                    
                    float xOffset = 0;
                    for (int i = 0; i < Word.Length; i++)
                    {
                        char c = Word[i];
                        Color color = isCurrentTarget ? 
                            (i < currentLetterIndex ? Color.LimeGreen : 
                             (i == currentLetterIndex ? Color.Yellow : Color.White)) 
                            : Color.White;
                        
                        Vector2 charSize = font.MeasureString(c.ToString());
                        spriteBatch.DrawString(
                            font,
                            c.ToString(),
                            new Vector2(
                                wordPosition.X - (Word.Length * charSize.X) / 2 + xOffset,
                                wordPosition.Y),
                            color);
                        
                        xOffset += charSize.X;
                    }
                }
            }
        }

        public bool ShouldRemove()
        {
            return IsExploding && ExplosionTimer <= 0;
        }
    }
}