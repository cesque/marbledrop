using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop
{
	static class Globals
	{
		public static Random RNG = new Random();

		public static Texture2D DebugTexture;

		public static Vector2 RotateAround(Vector2 position, Vector2 pivot, int quarterTurns)
		{
			//return Vector2.Transform(position - pivot, Matrix.CreateRotationZ(MathHelper.ToRadians(angle))) + pivot;
			var basePosition = position - pivot;

			for (int i = 0; i < quarterTurns % 4; i++)
			{
				var tempY = basePosition.Y;
				basePosition.Y = basePosition.X;
				basePosition.X = -tempY;
			}

			return basePosition + pivot;
		}
	}
}
