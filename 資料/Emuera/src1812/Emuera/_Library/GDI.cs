using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using MinorShift._Library;
using System.Windows.Forms;

namespace MinorShift._Library
{
	//http://www.pinvoke.net/default.aspx/gdi32.BitBlt からコピペ
	/// <summary>
	///     Specifies a raster-operation code. These codes define how the color data for the
	///     source rectangle is to be combined with the color data for the destination
	///     rectangle to achieve the final color.
	/// </summary>
	/// 
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum TernaryRasterOperations : uint
	{
		/// <summary>dest = source</summary>
		SRCCOPY = 0x00CC0020,
		/// <summary>dest = source OR dest</summary>
		SRCPAINT = 0x00EE0086,
		/// <summary>dest = source AND dest</summary>
		SRCAND = 0x008800C6,
		/// <summary>dest = source XOR dest</summary>
		SRCINVERT = 0x00660046,
		/// <summary>dest = source AND (NOT dest)</summary>
		SRCERASE = 0x00440328,
		/// <summary>dest = (NOT source)</summary>
		NOTSRCCOPY = 0x00330008,
		/// <summary>dest = (NOT src) AND (NOT dest)</summary>
		NOTSRCERASE = 0x001100A6,
		/// <summary>dest = (source AND pattern)</summary>
		MERGECOPY = 0x00C000CA,
		/// <summary>dest = (NOT source) OR dest</summary>
		MERGEPAINT = 0x00BB0226,
		/// <summary>dest = pattern</summary>
		PATCOPY = 0x00F00021,
		/// <summary>dest = DPSnoo</summary>
		PATPAINT = 0x00FB0A09,
		/// <summary>dest = pattern XOR dest</summary>
		PATINVERT = 0x005A0049,
		/// <summary>dest = (NOT dest)</summary>
		DSTINVERT = 0x00550009,
		/// <summary>dest = BLACK</summary>
		BLACKNESS = 0x00000042,
		/// <summary>dest = WHITE</summary>
		WHITENESS = 0x00FF0062
	}
	internal static class GDI
	{
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
		static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);
		[DllImport("gdi32.dll")]
		static extern uint SetTextColor(IntPtr hdc, int crColor);
		[DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
		static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
		[DllImport("gdi32.dll")]
		static extern bool DeleteObject(IntPtr hObject);
		[DllImport("gdi32.dll")]
		static extern uint SetBkColor(IntPtr hdc, int crColor);
		[DllImport("gdi32.dll")]
		static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
		[DllImport("gdi32.dll")]
		static extern IntPtr CreateSolidBrush(int crColor);
		[DllImport("gdi32.dll")]
		static extern IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
		static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int cbString, out Size lpSize);
		[DllImport("gdi32.dll")]
		static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
		   int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
        [DllImport("user32", EntryPoint = "TabbedTextOut", CharSet = CharSet.Auto)]
        static extern int TabbedTextOutW(IntPtr hdc, int x, int y, string lpString, int nCount, int nTabPositions, ref int lpnTabStopPositions, int nTabOrigin);
        [DllImport("user32", EntryPoint = "GetTabbedTextExtent", CharSet = CharSet.Auto)]
        static extern int GetTabbedTextExtentW(IntPtr hdc, string lpString, int nCount, int nTabPositions, ref int lpnTabStopPositions);

		public static uint SetTextColor(Color color)
		{
			return SetTextColor(hDC, ColorTranslator.ToWin32(color));
		}

        static bool isNt = (System.Environment.OSVersion.Platform == PlatformID.Win32NT) ? true : false;
		static IntPtr hDC;
		static Font lastFont = null;
		static IntPtr defaulthFont;
		static IntPtr defaulthBrush;
		static IntPtr defaulthPen;
        static Size fontMetrics;
		public static void GDIStart(Graphics g, Color backGroundColor)
		{
			GDI.hDC = g.GetHdc();
			IntPtr hBrush = CreateSolidBrush(ColorTranslator.ToWin32(backGroundColor));
			defaulthBrush = SelectObject(hDC, hBrush);
			IntPtr hPen = CreatePen(0, 0, ColorTranslator.ToWin32(backGroundColor));
			defaulthPen = SelectObject(hDC, hPen);
			SetBkColor(hDC, ColorTranslator.ToWin32(backGroundColor));
			lastFont = null;
		}

		public static void SetFont(Font font)
		{
			if (lastFont == font)
				return;
			IntPtr hFont = font.ToHfont();
			IntPtr hOldFont = SelectObject(hDC, hFont);
			if (lastFont == null)
				defaulthFont = hOldFont;
			else
				DeleteObject(hOldFont);
			lastFont = font;
            GetTextExtentPoint32(hDC, "あ", "あ".Length, out fontMetrics);
		}

		public static void GDIEnd(Graphics g)
		{
			if (lastFont != null)
				DeleteObject(SelectObject(hDC, defaulthFont));
			DeleteObject(SelectObject(hDC, defaulthBrush));
			DeleteObject(SelectObject(hDC, defaulthPen));
			g.ReleaseHdc(hDC);
			lastFont = null;
		}

		public static void TextOut(string str, Point p)
		{
			//TextOut(hDC, p.X, p.Y, str, str.Length);
            int a = 0;
            if (isNt)
                TabbedTextOutW(hDC, p.X, p.Y, str, str.Length, 0, ref a, 0);
            else
                TabbedTextOutW(hDC, p.X, p.Y, str, LangManager.GetStrlenLang(str), 0, ref a, 0);
		}
		public static void TextOut(string str, int x, int y)
		{
            //TextOut(hDC, x, y, str, str.Length);
            int a = 0;
            if (isNt)
                TabbedTextOutW(hDC, x, y, str, str.Length, 0, ref a, 0);
            else
                TabbedTextOutW(hDC, x, y, str, LangManager.GetStrlenLang(str), 0, ref a, 0);
        }

		public static void FillRect(Rectangle rect)
		{
			Rectangle(hDC, rect.X, rect.Y, rect.Right, rect.Bottom);
		}

		public static Size MeasureText(string str, Font font)
		{
			SetFont(font);
			Size size;
            int a = 0;
            int ret;
			//GetTextExtentPoint32(hDC, str, str.Length, out size);
            if (isNt)
                ret = GetTabbedTextExtentW(hDC, str, str.Length, 0, ref a);
            else
                ret = GetTabbedTextExtentW(hDC, str, LangManager.GetStrlenLang(str), 0, ref a);
            size = new Size(ret & 0xffff, (ret >> 16) & 0xffff);
			return size;
		}

        public static void FillGap(int lineHeight, int length, Point pt)
        {
            if (lineHeight <= fontMetrics.Height)
                return;
            Rectangle rect = new Rectangle(pt.X, pt.Y + fontMetrics.Height, length, lineHeight - fontMetrics.Height);
            FillRect(rect);
        }

		public static void DrawImage(Rectangle destRect, Bitmap srcImg, Point srcPoint)
		{
			IntPtr hSrcImg = srcImg.GetHbitmap();
			Graphics g = Graphics.FromImage(srcImg);
			IntPtr srchDC = g.GetHdc();
			IntPtr hDefaultImg = SelectObject(srchDC, hSrcImg);
			BitBlt(hDC, destRect.X, destRect.Y, destRect.Width, destRect.Height, srchDC, srcPoint.X, srcPoint.Y, TernaryRasterOperations.SRCCOPY);
			DeleteObject(SelectObject(srchDC, hDefaultImg));
			g.ReleaseHdc(srchDC);
			g.Dispose();
		}

	}
}
