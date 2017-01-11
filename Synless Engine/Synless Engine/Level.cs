using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synless_Engine
{
    class Level
    {
        #region
        public Sprite background = new Sprite("map");
        public int[,] tile_type;
        public int perso_start_X;
        public int perso_start_Y;
        public int portal_blue_start_X;
        public int portal_blue_start_Y;
        public int portal_orange_start_X;
        public int portal_orange_start_Y;
        public string portal_blue_orientation;
        public string portal_orange_orientation;
        public int barrier_start_X;
        public int barrier_start_Y;
        public bool reverse_map;
        #endregion
        public Level()
        {
              perso_start_X = 0;
              perso_start_Y = 0;
              portal_blue_start_X = 0;
              portal_blue_start_Y = 0;
              portal_orange_start_X = 0;
              portal_orange_start_Y = 0;
              portal_blue_orientation = "left";
              portal_orange_orientation = "left";
              barrier_start_X = 0;
              barrier_start_Y = 0;
              reverse_map = false;
        }

        public Bitmap getBlock(int _x, int _y)
        {
            return background.tiles[tile_type[_x,_y]];
        }
    }
}
