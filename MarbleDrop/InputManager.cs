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
		KeyboardState previousKeyboardState;
		KeyboardState currentKeyboardState;
		MouseState previousMouseState;
		MouseState currentMouseState;

		public InputManager(Game1 game)
		{
			this.game = game;

			previousKeyboardState = Keyboard.GetState();
			currentKeyboardState = Keyboard.GetState();

			previousMouseState = Mouse.GetState(game.Window);
			currentMouseState = Mouse.GetState(game.Window);
		}

		public void Update()
		{
			previousKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();

			previousMouseState = currentMouseState;
			currentMouseState = Mouse.GetState(game.Window);
		}

		public bool IsKeyPressed(Keys key)
		{
			return previousKeyboardState.IsKeyUp(key) && currentKeyboardState.IsKeyDown(key);
		}

		public MouseState GetMouse()
		{
			return currentMouseState;
		}

		public bool IsLeftMouseButtonPressed()
		{
			return previousMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
		}
	}
}
