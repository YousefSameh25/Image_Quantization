using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }
            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }
        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
    }
    class cgraph
    {
        public List<RGBPixel> mycolors = new List<RGBPixel>();
        public int find_colors(RGBPixel[,] image)
        {
            bool[,,] done = new bool[257, 257, 257];
            for (int i = 0; i < ImageOperations.GetHeight(image); i++)
            {
                for (int j = 0; j < ImageOperations.GetWidth(image); j++)
                {
                    int r = image[i, j].red;
                    int g = image[i, j].green;
                    int b = image[i, j].blue;
                    RGBPixel thisone = image[i, j];
                    if (done[r, g, b])
                    {
                        continue;
                    }
                    else
                    {
                        mycolors.Add(thisone);
                        done[r, g, b] = true;
                    }
                }
            }
            int count = mycolors.Count;
            return count;
        }
        public double weight(RGBPixel x, RGBPixel y)
        {
            double firstparahalf = x.red - y.red;
            double firstpara = firstparahalf * firstparahalf;
            double secparahalf = x.green - y.green;
            double secpara = secparahalf * secparahalf;
            double thirdparahalf = x.blue - y.blue;
            double thirdpara = thirdparahalf * thirdparahalf;
            double wholepara = firstpara + secpara + thirdpara;
            return Math.Sqrt(wholepara);
        }
    }
    class PriorityQueue
    {
        public List<KeyValuePair<double, int>> pq;
        public PriorityQueue()
        {
            pq = new List<KeyValuePair<double, int>>();
            pq.Add(new KeyValuePair<double, int>(-1, -1));
        }
        public void push(double x, int node)
        {
            pq.Add(new KeyValuePair<double, int>(x, node));
            int child_idx = pq.Count - 1;
            int parent_idx = child_idx / 2;
            while (child_idx > 1)
            {
                if (pq[child_idx].Key < pq[parent_idx].Key)
                {
                    KeyValuePair<double, int> tmp = pq[parent_idx];
                    pq[parent_idx] = pq[child_idx];
                    pq[child_idx] = tmp;
                    child_idx = parent_idx;
                    parent_idx = child_idx / 2;
                }
                else
                {
                    break;
                }
            }
        }
        public KeyValuePair<double, int> pop()
        {
            KeyValuePair<double, int> top = pq[1];
            pq[1] = pq[pq.Count - 1];
            pq.RemoveAt(pq.Count - 1);
            int parent_idx = 1;
            int RC = parent_idx * 2 + 1;
            int LC = parent_idx * 2;

            while (RC < pq.Count || LC < pq.Count)
            {
                //Have L & R
                if (RC < pq.Count && LC < pq.Count)
                {
                    if (pq[RC].Key >= pq[LC].Key && pq[LC].Key <= pq[parent_idx].Key)
                    {
                        KeyValuePair<double, int> tmp = pq[parent_idx];
                        pq[parent_idx] = pq[LC];
                        pq[LC] = tmp;
                        parent_idx = LC;
                    }
                    else if (pq[RC].Key < pq[LC].Key && pq[RC].Key <= pq[parent_idx].Key)
                    {
                        KeyValuePair<double, int> tmp = pq[parent_idx];
                        pq[parent_idx] = pq[RC];
                        pq[RC] = tmp;
                        parent_idx = RC;
                    }
                    else
                    {
                        break;
                    }
                    RC = parent_idx * 2 + 1;
                    LC = parent_idx * 2;
                }
                //Have L only
                else
                {
                    if (pq[LC].Key <= pq[parent_idx].Key)
                    {
                        KeyValuePair<double, int> tmp = pq[parent_idx];
                        pq[parent_idx] = pq[LC];
                        pq[LC] = tmp;
                    }
                    break;
                }
            }
            return top;
        }
        public int count()
        {
            return pq.Count;
        }
    }
    class mst
    {
        public List<KeyValuePair<int, double>>[] New_adj_list = new List<KeyValuePair<int, double>>[100000 + 10];
        public List<KeyValuePair<double, KeyValuePair<int, int>>> edges = new List<KeyValuePair<double, KeyValuePair<int, int>>>();
        public bool[] visited = new bool[100000];
        public int[] connection = new int[100000];
        public double[] value = new double[100000];
        public mst()
        {
            for (int i = 0; i < 100000; i++)
            {             
                connection[i] = -1;
                value[i] = double.MaxValue;
                New_adj_list[i] = new List<KeyValuePair<int, double>>();
            }
        }
        public void prims(int count, ref List<RGBPixel> mycolors)
        {
            cgraph g = new cgraph();
            PriorityQueue que = new PriorityQueue();
            //weight 0 - node 1
            que.push(0, 1);
            value[1] = 0;
            while (que.count() != 1)
            {
                int node = que.pop().Value;
                //To skip the remaining elements in the pq.
                if (visited[node] == true)
                    continue;
                visited[node] = true;
                for (int i = 1; i <= count; i++)
                {
                    //To avoid taking node as child when that node is me.
                    if (i == node)
                        continue;
                    int vertex = i;
                    double weight = g.weight(mycolors[node - 1], mycolors[i - 1]);
                    //To avoid add node in already in mst. 
                    if (!visited[vertex] && value[vertex] > weight)
                    {
                        value[vertex] = weight;
                        connection[vertex] = node;
                        que.push(weight, vertex);
                    }
                }
            }
        }
        public double generate_edges_list(int n)
        {
            double cost = 0;
            for (int i = 2; i <= n; i++)
            {
                KeyValuePair<int, int> nodes = new KeyValuePair<int, int>(i, connection[i]);
                KeyValuePair<double, KeyValuePair<int, int>> temp = new KeyValuePair<double, KeyValuePair<int, int>>(value[i], nodes);
                edges.Add(temp);
                cost += value[i];
            }
            //Sort edges.
            edges.Sort((x, y) => (x.Key.CompareTo(y.Key)));
            return cost;
        }
        public void put_infinty(ref List<KeyValuePair<double, KeyValuePair<int, int>>> edges, int k)
        {
            k--;
            int idx = edges.Count() - 1;
            for (int i = 1; i <= k; i++)
            {
                int node1 = edges[idx].Value.Key;
                int node2 = edges[idx].Value.Value;
                KeyValuePair<int, int> nodes = new KeyValuePair<int, int>(node1, node2);
                KeyValuePair<double, KeyValuePair<int, int>> temp = new KeyValuePair<double, KeyValuePair<int, int>>(-1, nodes);
                edges[idx--] = temp;
            }
        }
        public void create_adj_list(ref List<KeyValuePair<double, KeyValuePair<int, int>>> edges)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                New_adj_list[edges[i].Value.Key].Add(new KeyValuePair<int, double>(edges[i].Value.Value, edges[i].Key));
                New_adj_list[edges[i].Value.Value].Add(new KeyValuePair<int, double>(edges[i].Value.Key, edges[i].Key));
            }
        }
    }
    class cluster
    {
        public RGBPixel[] cent = new RGBPixel[50000];
        public int[,,] cton = new int[257, 257, 257];
        public List<int>[] clusters = new List<int>[60000];
        public bool[] visited = new bool[60000];
        public cluster()
        {
            for (int i = 0; i < 60000; i++)
            {
                clusters[i] = new List<int>();
                visited[i] = false;
            }
        }
        public void dfs(int node, int row, ref List<KeyValuePair<int, double>>[] New_adj_list, ref List<int>[] clusters)
        {
            visited[node] = true;
            clusters[row].Add(node);
            foreach (KeyValuePair<int, double> child in New_adj_list[node])
            {
                if (visited[child.Key] == true)
                    continue;

                if (child.Value != -1)
                {
                    dfs(child.Key, row, ref New_adj_list, ref clusters);
                }
            }
        }
        public void create_clusters (int dist, ref List<KeyValuePair<int, double>>[] New_adj_list, ref List<int>[] clusters)
        {
            int r = 0;
            for (int i = 1; i <= dist; i++)
            {
                if (visited[i] == false)
                {
                    dfs(i, r, ref New_adj_list, ref clusters);
                    r++;
                }
            }
        }
        public void get_cent(ref List<int>[] clusters, ref List<RGBPixel> mycolors, int k)
        {
            for (int i = 0; i < k; i++)
            {

                int r = 0, b = 0, g = 0;
                for (int j = 0; j < clusters[i].Count; j++)
                {
                    r += mycolors[clusters[i][j] - 1].red;
                    b += mycolors[clusters[i][j] - 1].blue;
                    g += mycolors[clusters[i][j] - 1].green;
                    cton[mycolors[clusters[i][j] - 1].red, mycolors[clusters[i][j] - 1].green, mycolors[clusters[i][j] - 1].blue] = i;
                }

                int cent_r = r / clusters[i].Count;
                int cent_g = g / clusters[i].Count;
                int cent_b = b / clusters[i].Count;

                RGBPixel col = new RGBPixel();

                col.red = Convert.ToByte(cent_r);
                col.green = Convert.ToByte(cent_g);
                col.blue = Convert.ToByte(cent_b);

                cent[i] = col;

            }
        }
        public void quantize(ref RGBPixel[] cent, ref RGBPixel[,] ImageMatrix)
        {
            for (int i = 0; i < ImageOperations.GetHeight(ImageMatrix); i++)
            {
                for (int j = 0; j < ImageOperations.GetWidth(ImageMatrix); j++)
                {
                    ImageMatrix[i, j] = cent[cton[ImageMatrix[i, j].red, ImageMatrix[i, j].green, ImageMatrix[i, j].blue]];
                }
            }
        }
    }
}
