using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;

namespace MarbleDrop
{
    class InputManager
    {
        Game1 game;
        KeyboardState previousState;
        KeyboardState currentState;

        public InputManager(Game1 game)
        {
            this.game = game;

            previousState = Keyboard.GetState();
            currentState = Keyboard.GetState();
        }

        public void Update()
        {
            previousState = currentState;
            currentState = Keyboard.GetState();
        }

        public bool IsKeyPressed(Keys key)
        {
            return previousState.IsKeyUp(key) && currentState.IsKeyDown(key);
        }
    }
}
