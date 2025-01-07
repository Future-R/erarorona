using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MinorShift.Emuera.Forms
{
	internal sealed class EraPictureBox:PictureBox
	{
		public EraPictureBox()
		{
			//紆余曲折の残骸。
			//ConfigのUseImageBufferがfalseの場合、
			//OptimizedDoubleBufferをtrueにするとリサイズした時に再描画がうまくいかなかった。
			//Opaqueをtrue、OptimizedDoubleBufferをfalseにするとゴミが残る。
			//OnPaintBackgroundを上書きしてもゴミが残る。
			//this.SetStyle(ControlStyles.Opaque, true);
			//this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//this.SetStyle(ControlStyles.UserPaint, true);
			//this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			////this.DoubleBuffered = true;
			//this.SetStyle(ControlStyles.ResizeRedraw, true);
		}

		public void SetStyle()
		{
			//if (StaticConfig.UseImageBuffer)
			//{
			//    this.SetStyle(ControlStyles.Opaque, true);
			//    this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//    this.SetStyle(ControlStyles.UserPaint, true);
			//    this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			//    this.SetStyle(ControlStyles.ResizeRedraw, false);
			//}
			//else
			//{
			//    this.SetStyle(ControlStyles.Opaque, false);
			//    this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//    this.SetStyle(ControlStyles.UserPaint, true);
			//    this.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
			//    this.SetStyle(ControlStyles.ResizeRedraw, false);
			//}
			this.SetStyle(ControlStyles.Opaque, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.ResizeRedraw, false);
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			base.OnPaintBackground(pevent);
		}
	}
}
