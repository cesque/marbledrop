using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Rendering
{
    public class Grid
    {
        Game1 game;

        public int Width;
        public int Height;

        public int CharacterWidth = 8;
        public int CharacterHeight = 8;

        public BitmapFont Font;
        public ColorPalette Palette;

        public GridCharacter[,] Characters;

        public Grid(Game1 game)
        {
            this.game = game;

            Width = 80;
            Height = 40;

            Characters = new GridCharacter[Width, Height];

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    Characters[x, y] = null;
                }
            }

            CharacterWidth = 8;
            CharacterHeight = 8;

            Font = new BitmapFont(game, "zxevolution", CharacterWidth, CharacterHeight);
            Palette = ColorPalette.Load("pico-8.txt");
        }

        public Vector2 GetScreenSize()
        {
            return new Vector2(
                CharacterWidth * Width,
                CharacterHeight * Height
            );
        }

        public void Update(GameTime gameTime)
        {
            Characters = new GridCharacter[Width, Height];

            //for (var x = 0; x < Width; x++)
            //{
            //    for (var y = 0; y < Height; y++)
            //    {
            //        if(Globals.RNG.Next(100) > 90)
            //        {
            //            Characters[x, y] = new GridCharacter(
            //                this,
            //                Globals.RNG.Next(4),
            //                new Vector2(x, y)
            //            );
            //        }
            //    }
            //}
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for(var x = 0; x < Width; x++)
            {
                for(var y = 0; y < Height; y++)
                {
                    var character = Characters[x, y];


                    if(character != null)
                    {
                        character.Position = new Vector2(x, y);
                        character.Draw(spriteBatch);
                    }
                }
            }
        }

        public bool TryAddCharacter(GridCharacter character)
        {

            var x = (int)Math.Round(character.Position.X);
            var y = (int)Math.Round(character.Position.Y);

            if(
                x < 0 
                || x >= Characters.GetLength(0)
                || y < 0
                || y >= Characters.GetLength(1)
            )
            {
                return false;
            }

            var current = Characters[x, y];

            if(current == null || current.Priority <= character.Priority)
            {
                Characters[x, y] = character;
                return true;
            }

            return false;
        } 
    }
}
