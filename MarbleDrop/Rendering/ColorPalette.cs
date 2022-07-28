using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Rendering
{
	public class ColorPalette
	{
		public Dictionary<string, Color> Colors;

		public ColorPalette()
		{
			Colors = new Dictionary<string, Color>();
		}

		public void Add(Color color)
		{
			Colors.Add(color.PackedValue.ToString(), color);
		}

		public void Add(Color color, string name)
		{
			if (name == null)
			{
				Add(color);
			}
			else
			{
				Colors.Add(name, color);
			}
		}

		public Color Get(int index)
		{
			return Colors.Values.ElementAt(index);
		}

		public Color Get(string name)
		{
			var matchingColor = Colors[name];

			if (matchingColor != null)
			{
				return matchingColor;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("couldn't find a matching color for name: " + name + ", returning default!");
				Console.ResetColor();

				return Get("white");
			}
		}


		public static ColorPalette Load(string assetName)
		{
			var file = File.ReadAllText("./Content/" + assetName);

			var allLines = file.Split('\n');
			var paletteLines = allLines.Where(line => !line.StartsWith(";") && line.Trim().Length > 0);

			var palette = new ColorPalette();

			foreach (var line in paletteLines)
			{
				var pieces = line.Split(' ');

				Console.WriteLine(String.Join(", ", pieces));

				var colorString = pieces[0];

				// get rgb values differently if AARRGGBB or RRGGBB
				var colorIndexOffset = 0;
				if (colorString.Length == 8) colorIndexOffset = 2;

				var rString = colorString.Substring(colorIndexOffset, 2);
				var gString = colorString.Substring(colorIndexOffset + 2, 2);
				var bString = colorString.Substring(colorIndexOffset + 4, 2);

				var r = int.Parse(rString, System.Globalization.NumberStyles.HexNumber);
				var g = int.Parse(gString, System.Globalization.NumberStyles.HexNumber);
				var b = int.Parse(bString, System.Globalization.NumberStyles.HexNumber);

				if (pieces.Length > 1)
				{
					var name = pieces[1].Trim();
					palette.Add(new Color(r, g, b), name);
				}
				else
				{
					palette.Add(new Color(r, g, b));
				}

			}

			return palette;
		}
	}

	//class ColorPaletteEntry
	//{
	//    public Color Color;
	//    public string Name;

	//    public ColorPaletteEntry(Color color) : this(color, null) { }

	//    public ColorPaletteEntry(Color color, string name)
	//    {
	//        Color = color;
	//        Name = name;
	//    }
	//}
}
