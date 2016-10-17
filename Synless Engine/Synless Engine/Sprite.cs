using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synless_Engine
{
    /// <summary>
    /// Sprites to be used for the character and the map
    /// </summary>
    class Sprite
    {
        #region Variables
        private const string bmpPath = "frames/";
        public Bitmap[] tiles    = new Bitmap[32];
        public int[] tiles_type  = new int[32];
        public int Width;
        public int Height;
        public int nbSprite = 0;
        #endregion 

        public Sprite(string _type)
        {
            if (_type == "chell")
            {   
                string path = "";
                for (int n = 0; n < tiles_type.Length; n++)
                {   try
                    {
                        path = bmpPath + "chell" + n.ToString() + ".bmp";
                        tiles[n] = new Bitmap(path);
                        nbSprite++;
                    }
                    catch { break; }
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
                try
                {
                    tiles[0] = new Bitmap(bmpPath + "portal_blue_rigth_backup.bmp");
                    tiles[1] = new Bitmap(bmpPath + "portal_blue_top_backup.bmp");
                }
                catch
                {

                }         
            }
            else if (_type == "portal_orange")
            {
                try
                { 
                    tiles[0] = new Bitmap(bmpPath + "portal_orange_rigth_backup.bmp");
                    tiles[1] = new Bitmap(bmpPath + "portal_orange_top_backup.bmp");
                }
                catch
                {

                }
            }
            Width = tiles[0].Width;
            Height = tiles[0].Height;
        }

        /// <summary>
        /// Get the 2D array of pixel
        /// </summary>
        /// <param name="index">Sprite to be returned</param>
        public Bitmap GetSprite(int _index)
        {
            return tiles[_index];
        }

    }
}
