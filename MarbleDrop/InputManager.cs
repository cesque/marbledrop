using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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

		public Vector2 RawMousePosition
		{
			get { return currentMouseState.Position.ToVector2(); }
		}

		public Vector2 MousePosition
		{
			get { return RawMousePosition / game.screenScale; }
		}

		public Vector2 RawMouseDelta
		{
			get { return currentMouseState.Position.ToVector2() - previousMouseState.Position.ToVector2(); }
		}

		public Vector2 MouseDelta
		{
			get { return MousePosition - (previousMouseState.Position.ToVector2() / game.screenScale); }
		}

		public float MouseScrollWheelDelta
		{
			get { return currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;  }
		}

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

		public bool IsKeyUntouched(Keys key)
		{
			return previousKeyboardState.IsKeyUp(key) && currentKeyboardState.IsKeyUp(key);
		}

		public bool IsKeyReleased(Keys key)
		{
			return previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyUp(key);
		}

		public bool IsKeyHeld(Keys key)
		{
			return previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
		}

		public MouseState GetMouse()
		{
			return currentMouseState;
		}

		public bool IsLeftMouseButtonPressed()
		{
			return previousMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
		}
		
		public bool IsLeftMouseButtonUntouched()
		{
			return previousMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Released;
		}
		
		public bool IsLeftMouseButtonReleased()
		{
			return previousMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released;
		}
		
		public bool IsLeftMouseButtonHeld()
		{
			return previousMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Pressed;
		}
		
		public bool IsRightMouseButtonPressed()
		{
			return previousMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
		}
		
		public bool IsRightMouseButtonUntouched()
		{
			return previousMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Released;
		}
		
		public bool IsRightMouseButtonReleased()
		{
			return previousMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released;
		}
		
		public bool IsRightMouseButtonHeld()
		{
			return previousMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Pressed;
		}
		
		public bool IsMiddleMouseButtonPressed()
		{
			return previousMouseState.MiddleButton == ButtonState.Released && currentMouseState.MiddleButton == ButtonState.Pressed;
		}
		
		public bool IsMiddleMouseButtonUntouched()
		{
			return previousMouseState.MiddleButton == ButtonState.Released && currentMouseState.MiddleButton == ButtonState.Released;
		}
		
		public bool IsMiddleMouseButtonReleased()
		{
			return previousMouseState.MiddleButton == ButtonState.Pressed && currentMouseState.MiddleButton == ButtonState.Released;
		}
		
		public bool IsMiddleMouseButtonHeld()
		{
			return previousMouseState.MiddleButton == ButtonState.Pressed && currentMouseState.MiddleButton == ButtonState.Pressed;
		}
	}
}
