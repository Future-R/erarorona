using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MinorShift.Emuera.Sub;
using MinorShift._Library;
using System.Windows.Forms;

namespace MinorShift.Emuera.GameView
{
	internal sealed class StringMeasure:IDisposable
	{
		public StringMeasure(Graphics g, Color bgColor, TextDrawingMode textDrawingMode)
		{
            mode = textDrawingMode;
			if (mode == TextDrawingMode.WINAPI)
                GDI.GDIStart(g, bgColor);
			graph = g;
		}
		Graphics graph = null;
		readonly TextDrawingMode mode;

        //ここらへん一部は再起動時に値が変わらないとずれが生じるのでreadonlyにはできない
		static float fontDisplaySize = Config.Font.Size / 2 * 1.04f;//実際には指定したフォントより若干幅をとる？
		static readonly StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
		static readonly CharacterRange[] ranges = new CharacterRange[] { new CharacterRange(0, 1) };

		static Size layoutSize = new Size(Config.WindowX * 2, Config.LineHeight);
		static RectangleF layoutRect = new RectangleF(0, 0, Config.WindowX * 2, Config.LineHeight);

		public int GetDisplayLength(string s, Font font)
		{
            //nullになるケースだと呼び出されない…はず
            //if (graph == null)
            //    throw new ExeEE("Graphicsが設定されていない");
			return StringMeasure.GetDisplayLength(graph, s, font, mode);
		}

		public static int GetDisplayLength(Graphics g, string s, Font font, TextDrawingMode textDrawingMode)
		{
			if (string.IsNullOrEmpty(s))
				return 0;
			if (textDrawingMode == TextDrawingMode.GRAPHICS)
			{
                if (s.Contains("\t"))
                    s = s.Replace("\t", "        ");
                ranges[0].Length = s.Length;
				//CharacterRange[] ranges = new CharacterRange[] { new CharacterRange(0, s.Length) };
				sf.SetMeasurableCharacterRanges(ranges);
				Region[] regions = g.MeasureCharacterRanges(s, font, layoutRect, sf);
				RectangleF rectF = regions[0].GetBounds(g);
				//return (int)rectF.Width;//プロポーショナルでなくても数ピクセルずれる
                return (int)((int)((rectF.Width - 1) / fontDisplaySize + 0.95f) * fontDisplaySize);
			}
			else if (textDrawingMode == TextDrawingMode.TEXTRENDERER)
			{
				Size size = TextRenderer.MeasureText(g, s, font, layoutSize, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
				//Size size = TextRenderer.MeasureText(g, s, StaticConfig.Font);
				return size.Width;
			}
			else// if (StaticConfig.TextDrawingMode == TextDrawingMode.WINAPI)
			{
				Size size = GDI.MeasureText(s, font);
				return size.Width;
			}
            //来るわけがない
            //else
            //    throw new ExeEE("描画モード不明");
		}

        public static void setStaticMember(int x)
        {
            fontDisplaySize = Config.Font.Size / 2 * 1.04f;
            layoutSize = new Size(x * 2, Config.LineHeight);
            layoutRect = new RectangleF(0, 0, x * 2, Config.LineHeight);
        }
		#region IDisposable メンバ

		public void Dispose()
		{
			if (graph != null)
			{
				if (mode == TextDrawingMode.WINAPI)
					GDI.GDIEnd(graph);
				graph.Dispose();
			}
			graph = null;
		}

		#endregion
	}
}
