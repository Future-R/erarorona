using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using MinorShift._Library;
using MinorShift.Emuera;

namespace MinorShift.Emuera.GameView
{
	/// <summary>
	/// 装飾付文字列。stringとStringStyleからなる。
	/// </summary>
	internal sealed class ConsoleStyledString
	{
		private ConsoleStyledString() { }
		public ConsoleStyledString(string str, StringStyle style)
		{
            //if ((StaticConfig.TextDrawingMode != TextDrawingMode.GRAPHICS) && (str.IndexOf('\t') >= 0))
            //    str = str.Replace("\t", "");
			this.Str = str;
			this.StringStyle = style;
			Font = Config.GetFont(style.Fontname, style.FontStyle);
			Color = style.Color;
			BColor = style.ButtonColor;
            colorChanged = style.ColorChanged;
			if (!colorChanged && Color != Config.ForeColor)
				colorChanged = true;
			PointX = -1;
			Width = -1;
		}

		public string Str { get; private set; }
		public int Length { get { return Str.Length; } }
		public Font Font{ get; private set;}
		public Color Color { get; private set; }
		public Color BColor { get; private set; }
		public int PointX { get; set; }
		public int Width { get; private set; }
		public StringStyle StringStyle{ get; private set;}
		bool colorChanged;
		//単一のボタンフラグ
		public bool IsButton { get; set; }
		//indexの文字数の前方文字列とindex以降の後方文字列に分割
		public ConsoleStyledString DivideAt(int index, StringMeasure sm)
		{
			if ((index <= 0)||(index > Str.Length))
				return null;
			ConsoleStyledString ret = DivideAt(index);
			ret.SetWidth(sm);
			this.SetWidth(sm);
			return ret;
		}
		public ConsoleStyledString DivideAt(int index)
		{
			if ((index <= 0) || (index > Str.Length))
				return null;
			string str = Str.Substring(index, Str.Length - index);
			this.Str = Str.Substring(0, index);
			ConsoleStyledString ret = new ConsoleStyledString();
			ret.Font = this.Font;
			ret.Str = str;
			ret.Color = this.Color;
			ret.BColor = this.BColor;
			ret.colorChanged = this.colorChanged;
			ret.StringStyle = this.StringStyle;
			return ret;
		}

		public void SetWidth(StringMeasure sm)
		{
			Width = sm.GetDisplayLength(Str, Font);
		}

		public void DrawTo(Graphics graph, int pointY, bool isSelecting, bool isBackLog, TextDrawingMode mode)
		{
			Color color = this.Color;
			if(isSelecting)
				color = this.BColor;
			else if (isBackLog && !colorChanged)
                color = Config.LogColor;
				
			if (mode == TextDrawingMode.GRAPHICS)
				graph.DrawString(Str, Font, new SolidBrush(color), new Point(PointX, pointY));
			else
				TextRenderer.DrawText(graph, Str, Font, new Point(PointX, pointY), color, TextFormatFlags.NoPrefix);

		}

		public void GDIDrawTo(int pointY, bool isSelecting, bool isBackLog)
		{
			Color color = this.Color;
			if(isSelecting)
                color = this.BColor;
			else if (isBackLog && !colorChanged)
                color = Config.LogColor;
			GDI.SetFont(Font);
			GDI.SetTextColor(color);
			GDI.TextOut(Str, new Point(PointX, pointY));
		}

		public override string ToString()
		{
			if (Str == null)
				return "";
			return Str;
		}
	}
}
