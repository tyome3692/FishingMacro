using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIshingMacro
{
    public partial class TestPictureView : Form
    {
        public TestPictureView()
        {
            InitializeComponent();
        }

        internal void SetRect(Rectangle rect)
        {
            this.Size = new Size(rect.Width, rect.Height);
            this.Location = new Point(rect.X, rect.Y);
        }
    }
}
