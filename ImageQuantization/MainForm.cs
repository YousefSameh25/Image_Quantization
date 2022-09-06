using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        RGBPixel[,] ImageMatrix;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();           
        }
        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            //Task 1
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            int k = int.Parse(txtGaussSigma.Text);
        
            cgraph c = new cgraph();

            int count = c.find_colors(ImageMatrix);

            //--------------------------------------------------------------------------
            watch.Stop();
            MessageBox.Show("Number of distict colors = " +count.ToString());
            watch.Start();
            //--------------------------------------------------------------------------

            //Task 2
            mst ms = new mst();

            ms.prims(count ,ref c.mycolors);

            double cost = ms.generate_edges_list(count);

            //--------------------------------------------------------------------------
            watch.Stop();
            MessageBox.Show("MST = " + cost.ToString());
            watch.Start();
            //--------------------------------------------------------------------------

            //Task 3
            cluster clus = new cluster();

            ms.put_infinty(ref ms.edges,k);

            ms.create_adj_list(ref ms.edges);

            clus.create_clusters(count, ref ms.New_adj_list , ref clus.clusters);

            clus.get_cent(ref clus.clusters, ref c.mycolors, k);

            clus.quantize(ref clus.cent, ref ImageMatrix);

            //--------------------------------------------------------------------------
            watch.Stop();
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            //--------------------------------------------------------------------------

            MessageBox.Show("Execution Time: " + (watch.ElapsedMilliseconds/1000).ToString() + " seconds");
            MessageBox.Show("Execution Time: " + watch.ElapsedMilliseconds.ToString() + " ms");

            //Save the image.
            var fd = new SaveFileDialog();
            fd.Filter = "Bmp(.BMP;)|.BMP;| Jpg(Jpg)|.jpg";
            fd.AddExtension = true;
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (System.IO.Path.GetExtension(fd.FileName).ToUpper() == ".BMP")
                {
                    pictureBox2.Image.Save(fd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                else if (System.IO.Path.GetExtension(fd.FileName).ToUpper() == ".JPG")
                {
                    pictureBox2.Image.Save(fd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else if (System.IO.Path.GetExtension(fd.FileName).ToUpper() == ".PNG")
                {
                    pictureBox2.Image.Save(fd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }

        }  
    }
}