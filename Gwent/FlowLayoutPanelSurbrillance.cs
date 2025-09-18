using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Gwent
{
    public class FlowLayoutPanelSurbrillance : FlowLayoutPanel
    {
        public bool Surbrillance { get; set; } = false;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Surbrillance)
            {
                using (Pen pen = new Pen(Color.Yellow, 5))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }    
            }
        }
    }
}
