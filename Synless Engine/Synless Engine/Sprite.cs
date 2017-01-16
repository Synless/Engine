using System.Drawing;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Linq;
using System.Resources;

namespace Synless_Engine
{
    /// <summary>
    /// Sprites to be used for the character and the map
    /// </summary>
    class Sprite
    {
        #region Variables
        private const string bmpPath = "frames/";
        private const int maxChellFrame = 14;

        public Bitmap[] tiles    = new Bitmap[32];
        public int[] tiles_type  = new int[32];
        public int width;
        public int height;
        public int nbSprite = 0;
        #endregion 

        public Sprite(string _type)
        {
            if (_type == "chell")
            {   
                string path = "";
                for (int n = 0; n < maxChellFrame; n++)
                {
                    path = bmpPath + "chell" + n.ToString() + ".bmp";
                    //ResourceManager rm = Properties.Resources.ResourceManager;
                    //tiles[n] = (Bitmap)rm.GetObject(path);
                    tiles[n] = new Bitmap(path);
                    nbSprite++;                    
                }
                tiles_type[0] = 0;
                tiles_type[1] = 1;
                tiles_type[2] = 2;
                tiles_type[3] = 3;
                tiles_type[4] = 4;
                tiles_type[5] = 5;
                tiles_type[6] = 6;
                tiles_type[7] = 7;
                tiles_type[8] = 8;
                tiles_type[9] = 9;
                tiles_type[10]= 10;
            }
            else if(_type == "map")
            {
                tiles[0] = new Bitmap(bmpPath + "map_background.bmp");
                tiles[1] = new Bitmap(bmpPath + "map_block.bmp");
                tiles[2] = new Bitmap(bmpPath + "map_block_dark.bmp");
                tiles[3] = new Bitmap(bmpPath + "map_block_glass.bmp");
                tiles[4] = new Bitmap(bmpPath + "map_in.bmp");
                tiles[5] = new Bitmap(bmpPath + "map_out.bmp");
                tiles[6] = new Bitmap(bmpPath + "map_zap.bmp");
                tiles_type[0] = 0;
                tiles_type[1] = 1;
                tiles_type[2] = 2;
                tiles_type[3] = 3;
                tiles_type[4] = 4;
                tiles_type[5] = 5;
                tiles_type[6] = 6;
                tiles_type[7] = 7;
                tiles_type[8] = 8;
                tiles_type[9] = 9;
                tiles_type[10]= 10;
            }
            else if (_type == "portal_blue")
            {
                tiles[0] = new Bitmap(bmpPath + "portal_blue_rigth.bmp");
                tiles[1] = new Bitmap(bmpPath + "portal_blue_top.bmp");
            }
            else if (_type == "portal_orange")
            { 
                tiles[0] = new Bitmap(bmpPath + "portal_orange_rigth.bmp");
                tiles[1] = new Bitmap(bmpPath + "portal_orange_top.bmp");
            }
            width = tiles[0].Width;
            height = tiles[0].Height;
        }

        /// <summary>
        /// Get the 2D array of pixel
        /// </summary>
        /// <param name="index">Sprite to be returned</param>
        public Bitmap getSprite(int _index)
        {
            return tiles[_index];
        }
    }
}
