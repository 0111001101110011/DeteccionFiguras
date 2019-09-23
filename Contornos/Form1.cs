using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Contornos
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> _imgInput;
        Image<Bgr, byte> _imgGray;
        public Form1()
        {
            InitializeComponent();
        }

        private void cargarImagenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _imgInput = new Image<Bgr, byte>(ofd.FileName);
                pictureBox1.Image = _imgInput.Bitmap;
            }
        }

        private void detectarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Binarizacion
            Image<Gray, byte> _imgOutput = _imgInput.Convert<Gray, byte>().ThresholdBinary(new Gray(200), new Gray(255));
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat hier = new Mat();

            //Encontrat los contornos en la imagen
            CvInvoke.FindContours(_imgOutput, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(_imgOutput, contours, -1, new MCvScalar(255, 0, 0));

            pictureBox2.Image = _imgOutput.Bitmap;
        }

        private void DetectarFormasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_imgInput == null)
            {
                return;
            }

            try
            {
                //Suaviza la imagen y aplica binarizacion y almacena en variable temporal
                var temp = _imgInput.SmoothGaussian(5).Convert<Gray, byte>().ThresholdBinaryInv(new Gray(230), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat n = new Mat();

                //Aplicar funcion FindCountour para detectar los contornos de las figuras.
                CvInvoke.FindContours(temp, contours, n, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                for (int i=0; i < contours.Size; i++)
                {
                    double perimetro = CvInvoke.ArcLength(contours[i], true);
                    VectorOfPoint aprox = new VectorOfPoint();
                    //Deteccion de curvas a una presicion indicada eje(0.04)
                    CvInvoke.ApproxPolyDP(contours[i], aprox, 0.04 * perimetro, true);

                    CvInvoke.DrawContours(_imgInput, contours, i, new MCvScalar(0, 0, 255), 2);

                    //Encontrar centro de la figura
                    var momentos = CvInvoke.Moments(contours[i]);
                    int x = (int)(momentos.M10 / momentos.M00);
                    int y = (int)(momentos.M01 / momentos.M00);  

                    if (aprox.Size == 3)
                    {

                        CvInvoke.PutText(_imgInput, "triangulo", new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);

                    }

                    if (aprox.Size == 4)
                    {

                        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);

                        double ar = (double)rect.Width / rect.Height;

                        if (ar >= 0.95 && ar <= 1.05)
                        {
                            CvInvoke.PutText(_imgInput, "Cuadrado", new Point(x, y), 
                            Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);


                        }
                        else
                        {
                            CvInvoke.PutText(_imgInput, "Rectangulo", new Point(x, y), 
                            Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                        }

                    }


                    if (aprox.Size > 6)
                    {

                        CvInvoke.PutText(_imgInput, "Circulo", new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);

                    }

                    pictureBox2.Image = _imgInput.Bitmap;


                    if (aprox.Size == 6)
                    {

                        CvInvoke.PutText(_imgInput, "Hexagono", new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);

                    }

                    pictureBox2.Image = _imgInput.Bitmap;

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
    }
}
