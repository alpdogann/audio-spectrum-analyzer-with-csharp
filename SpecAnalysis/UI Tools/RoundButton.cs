using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;

namespace SpecAnalysis.UI_Tools
{
    public class RoundButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            GraphicsPath gPath = GUI.RoundedRectangle(rect, 5, 5, 5, 5);
            this.Region = new System.Drawing.Region(gPath);
            base.OnPaint(e);


        }
    }
}
