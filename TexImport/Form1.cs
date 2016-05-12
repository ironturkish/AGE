using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TexImport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var toimport = (Bitmap)Bitmap.FromFile("toimport.png");
            var idgaf = Color.White;
            var output = "";
            var currentcolor = Color.White;

            for (int i = 0; i < toimport.Height; i++)
            {
                for (int a = 0; a < toimport.Width; a++)
                {
                    var pixel = toimport.GetPixel(a, i);
                    if (pixel == idgaf)
                    {
                        output += "0-";
                    }
                    else if (pixel == currentcolor)
                    {
                        output += "1-";
                    }
                    else
                    {
                        output += "sc" + ColorTranslator.ToHtml(pixel) + "-";
                        output += "1-";
                    }
                }
                output += "nl-";
            }
            File.WriteAllText("output.texture", output);
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
