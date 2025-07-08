using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace WordShooter
{
    public class CharacterSelectScreen
    {
        public bool IsSelectionComplete { get; private set; }
        public int SelectedPlaneIndex { get; private set; }
        
        private readonly Texture2D _planeSprites;
        private readonly SpriteFont _font;
        private readonly List<Rectangle> _planeOptions;
        private readonly List<Vector2> _optionPositions;
        
        public CharacterSelectScreen(Texture2D planeSprites, SpriteFont font)
        {
            _planeSprites = planeSprites;
            _font = font;
            
            _planeOptions = new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),   // Plane 1
                new Rectangle(32, 0, 32, 32),  // Plane 2
                new Rectangle(64, 0, 32, 32)   // Plane 3
            };
            
            _optionPositions = new List<Vector2>
            {
                new Vector2(200, 300),
                new Vector2(400, 300),
                new Vector2(600, 300)
            };
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                SelectedPlaneIndex = (SelectedPlaneIndex - 1 + _planeOptions.Count) % _planeOptions.Count;
            }
            else if (keyboardState.IsKeyDown(Keys.Right))
            {
                SelectedPlaneIndex = (SelectedPlaneIndex + 1) % _planeOptions.Count;
            }
            
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                IsSelectionComplete = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string title = "SELECT YOUR PLANE";
            Vector2 titleSize = _font.MeasureString(title);
            
            spriteBatch.DrawString(
                _font,
                title,
                new Vector2(400 - titleSize.X / 2, 100),
                Color.White);
                
            for (int i = 0; i < _planeOptions.Count; i++)
            {
                Rectangle sourceRect = _planeOptions[i];
                Vector2 position = _optionPositions[i];
                Color color = i == SelectedPlaneIndex ? Color.Yellow : Color.White;
                
                spriteBatch.Draw(
                    _planeSprites,
                    position,
                    sourceRect,
                    color,
                    0f,
                    new Vector2(sourceRect.Width / 2, sourceRect.Height / 2),
                    2f,
                    SpriteEffects.None,
                    0f);
            }
            
            string prompt = "Press LEFT/RIGHT to choose, ENTER to confirm";
            Vector2 promptSize = _font.MeasureString(prompt);
            
            spriteBatch.DrawString(
                _font,
                prompt,
                new Vector2(400 - promptSize.X / 2, 500),
                Color.White);
        }

        public void Reset()
        {
            IsSelectionComplete = false;
            SelectedPlaneIndex = 0;
        }
    }
}