using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameView
{
	/// <summary>
	/// ボタン。1つ以上の装飾付文字列（ConsoleStyledString）からなる。
	/// </summary>
	internal sealed class ConsoleButtonString
	{
		public ConsoleButtonString(EmueraConsole console, ConsoleStyledString[] strs)
		{
			this.parent = console;
			this.strArray = strs;
			IsButton = false;
			PointX = -1;
			Width = -1;
            ErrPos = null;
		}
		public ConsoleButtonString(EmueraConsole console, ConsoleStyledString[] strs, Int64 input)
			:this(console, strs)
		{
			this.Input = input;
			Inputs = input.ToString();
			IsButton = true;
			IsInteger = true;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = null;
        }
		public ConsoleButtonString(EmueraConsole console, ConsoleStyledString[] strs, string inputs)
			: this(console, strs)
		{
			this.Inputs = inputs;
			IsButton = true;
			IsInteger = false;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = null;
        }
        
		public ConsoleButtonString(EmueraConsole console, ConsoleStyledString[] strs, Int64 input, string inputs)
			: this(console, strs)
		{
			this.Input = input;
			this.Inputs = inputs;
			IsButton = true;
			IsInteger = true;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = null;
        }
        public ConsoleButtonString(EmueraConsole console, ConsoleStyledString[] strs, string inputs,ScriptPosition pos)
            : this(console, strs)
        {
            this.Inputs = inputs;
            IsButton = true;
            IsInteger = false;
			if (console != null)
			{
				Generation = parent.NewButtonGeneration;
				console.UpdateGeneration();
			}
            ErrPos = pos;
        }
        
        ConsoleStyledString[] strArray;
		public ConsoleStyledString[] StrArray { get { return strArray; } }
		EmueraConsole parent;

		public ConsoleDisplayLine ParentLine { get; set; }
		public bool IsButton { get; private set; }
		public bool IsInteger { get; private set; }
		public Int64 Input { get; private set; }
		public string Inputs { get; private set; }
		public int PointX { get; private set; }
		public int Width { get; private set; }
		public Int64 Generation { get; private set; }
		public ScriptPosition ErrPos { get; set; }
		public string Title { get; set; }

		//indexの文字数の前方文字列とindex以降の後方文字列に分割
		public ConsoleButtonString DivideAt(int divIndex, StringMeasure sm)
		{
			if (divIndex <= 0)
				return null;
			List<ConsoleStyledString> cssListA = new List<ConsoleStyledString>();
			List<ConsoleStyledString> cssListB = new List<ConsoleStyledString>();
			int index = 0;
			int cssIndex = 0;
			bool b = false;
			for (cssIndex = 0; cssIndex < strArray.Length; cssIndex++)
			{
				if (b)
				{
					cssListB.Add(strArray[cssIndex]);
					continue;
				}
				int length = strArray[cssIndex].Str.Length;
				if (divIndex < index + length)
				{
					ConsoleStyledString newCss = strArray[cssIndex].DivideAt(divIndex - index, sm);
					cssListA.Add(strArray[cssIndex]);
					if (newCss != null)
						cssListB.Add(newCss);
					b = true;
					continue;
				}
				else if (divIndex == index + length)
				{
					cssListA.Add(strArray[cssIndex]);
					b = true;
					continue;
				}
				index += length;
				cssListA.Add(strArray[cssIndex]);
			}
			if ((cssIndex >= strArray.Length) && (cssListB.Count == 0))
				return null;
			ConsoleStyledString[] cssArrayA = new ConsoleStyledString[cssListA.Count];
			ConsoleStyledString[] cssArrayB = new ConsoleStyledString[cssListB.Count];
			cssListA.CopyTo(cssArrayA);
			cssListB.CopyTo(cssArrayB);
			this.strArray = cssArrayA;
			ConsoleButtonString ret = new ConsoleButtonString(null, cssArrayB);
			this.SetWidth(sm);
			ret.SetWidth(sm);
			ret.PointX = this.PointX + this.Width;
			ret.parent = this.parent;
			ret.ParentLine = this.ParentLine;
			ret.IsButton = this.IsButton;
			ret.IsInteger = this.IsInteger;
			ret.Input = this.Input;
			ret.Inputs = this.Inputs;
			ret.Generation = this.Generation;
			ret.ErrPos = this.ErrPos;
			ret.Title = this.Title;
			return ret;
		}

		public void SetWidth(StringMeasure sm)
		{
			Width = -1;
			if ((strArray != null) && (strArray.Length > 0))
			{
				Width = 0;
				foreach (ConsoleStyledString css in strArray)
				{
					css.SetWidth(sm);
					Width += css.Width;
				}
				if (Width <= 0)
					Width = -1;
			}
		}

		/// <summary>
		/// 先にSetWidthすること。
		/// </summary>
		/// <param name="sm"></param>
		public void SetPointX(int pointx)
		{
			int px = pointx;
			this.PointX = px;
			for (int i = 0; i < strArray.Length; i++)
			{
				strArray[i].PointX = px;
				px += strArray[i].Width;
			}
		}

		internal void ShiftPositionX(int shiftX)
		{
			PointX += shiftX;
			foreach (ConsoleStyledString css in strArray)
				css.PointX += shiftX;
		}

		public void DrawTo(Graphics graph, int pointY, bool isBackLog, TextDrawingMode mode)
		{
			bool isSelecting = (IsButton) && (parent.ButtonIsSelected(this));
			foreach (ConsoleStyledString css in strArray)
				css.DrawTo(graph, pointY, isSelecting, isBackLog, mode);
		}

		public void GDIDrawTo(int pointY, bool isBackLog)
		{
			bool isSelecting = (IsButton) && (parent.ButtonIsSelected(this));
			foreach (ConsoleStyledString css in strArray)
				css.GDIDrawTo(pointY, isSelecting, isBackLog);
		}
		
		public override string ToString()
		{
			if (strArray == null)
				return "";
			string str = "";
			foreach (ConsoleStyledString css in strArray)
				str += css.ToString();
			return str;
		}

	}
}
