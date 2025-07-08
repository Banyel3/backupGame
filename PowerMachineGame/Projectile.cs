using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WordShooter
{
    public class Projectile
    {
        public Vector2 Position { get; private set; }
        public Rectangle Bounds => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            _texture.Width,
            _texture.Height);
            
        private readonly Texture2D _texture;
        private readonly Vector2 _velocity;

        public Projectile(Texture2D texture, Vector2 position, Vector2 velocity)
        {
            _texture = texture;
            Position = position;
            _velocity = velocity;
        }

        public void Update(float deltaTime)
        {
            Position += _velocity * deltaTime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Color.White);
        }
    }
}