using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WordShooter
{
    public class GameHUD
    {
        private readonly SpriteFont _font;

        public GameHUD(SpriteFont font)
        {
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch, int score, int health)
        {
            // Draw score
            spriteBatch.DrawString(
                _font,
                $"Score: {score}",
                new Vector2(20, 20),
                Color.White);
                
            // Draw health
            spriteBatch.DrawString(
                _font,
                $"Health: {health}",
                new Vector2(20, 50),
                health > 50 ? Color.Green : Color.Red);
        }
    }
}