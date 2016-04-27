using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synless_Engine

{
    class Caracter
    {
        #region Variables
        private Sprite spr;
        public Bitmap cur_spr;
        public string orientation = "";
        public int cur_tile = 0;
        public int pixelToSee_X;
        public int pixelToSeeBorder_X;
        public int pixelToSee_Y;
        public int pixelToSeeBorder_Y;
        public int acceleration_X;
        public int acceleration_Y;        
        public int pos_X;
        public int pos_Y;
        public int posMid_X;
        public int posMid_Y;
        public int posBorder_X;
        public int posBorder_Y;
        public int last_pos_X;
        public int last_pos_Y;
        public int last_posBorder_X;
        public int last_posBorder_Y;
        public int speed_X;
        public int speed_Y;
        public int last_speed_X = 0;
        public int last_speed_Y = 0;
        public int Width;
        public int Height;
        public int weight;
        public bool isFalling;
        public bool canJump;
        #endregion

        public Caracter(int _posx, int _posy)
        {
            spr = new Sprite("chell");
            cur_spr             = spr.GetSprite(0);
            Width               = spr.Width;
            Height              = spr.Height;
            pixelToSee_X        = 0;
            pixelToSeeBorder_X  = Width;
            pixelToSee_Y        = 0;
            pixelToSeeBorder_Y  = Height;
            last_pos_X          = _posx;
            last_pos_Y          = _posy;
            pos_X               = _posx;
            pos_Y               = _posy;
            posMid_X            = pos_X + (Width>>1) - 1;
            posMid_Y            = pos_Y + (Height>>1) - 1;
            posBorder_X         = pos_X + Width  - 1;
            posBorder_Y         = pos_Y + Height - 1;
            speed_X             = 0;
            speed_Y             = 0;
            acceleration_X      = 0;
            acceleration_Y      = 0;
            weight              = 64;
            isFalling           = true;
            canJump             = false;
        } // CHELL
        public Caracter(bool _blue, int _pos_X, int _pos_Y, string _orientation)
        {
            if (_blue) { spr = new Sprite("portal_blue"); }
            else { spr = new Sprite("portal_orange"); }
            Width = spr.Width;
            Height = spr.Height;
            pos_X = _pos_X;
            pos_Y = _pos_Y;
            posMid_X = pos_X + (Width >> 1) - 1;
            posMid_Y = pos_Y + (Height >> 1) - 1;
            posBorder_X = pos_X + Width - 1;
            posBorder_Y = pos_Y + Height - 1;
            orientation = _orientation;
            cur_tile = 0;
            cur_spr = spr.GetSprite(cur_tile);
        } // PORTAL
        public Caracter(Caracter _charac, bool _blue) // MAYBE TO BE ADDED : CHELL COMPATIBILITY
        {
            if (_blue) { spr = new Sprite("portal_blue"); }
            else { spr = new Sprite("portal_orange"); }
            cur_spr = _charac.cur_spr;
            cur_tile = _charac.cur_tile;
            Width = _charac.Width;
            Height = _charac.Height;
            pos_X = _charac.pos_X;
            pos_Y = _charac.pos_Y;
            posMid_X = _charac.pos_X + (Width >> 1) - 1;
            posMid_Y = _charac.pos_Y + (Height >> 1) - 1;
            posBorder_X = _charac.pos_X + Width - 1;
            posBorder_Y = _charac.pos_Y + Height - 1;
            orientation = _charac.orientation;
        }

        #region PositionHandler
        /// <summary>
        /// Rotate the charater from is current orientation to the sent one,
        /// only affects the portals
        /// </summary>
        public void Rotation(string _newOrientation)
        {
            orientation = _newOrientation;
            if (orientation == "rigth" || orientation == "left")
            {
                Width = cur_spr.Width;
                Height = cur_spr.Height;
                SetPosMid_X(posMid_X);
                SetPosMid_Y(posMid_Y);

            }
            else if (orientation == "bot" || orientation == "top")
            {
                Height = cur_spr.Width;
                Width = cur_spr.Height;
                SetPosMid_X(posMid_X);
                SetPosMid_Y(posMid_Y);
            }
        }

        /// <summary>
        /// Used to remember the character position from one tick to another,
        /// usefull to process acceleration and speed
        /// </summary>
        public void Remember()
        {
            last_pos_X      = pos_X;
            last_pos_Y      = pos_Y;
            last_posBorder_X = posBorder_X;
            last_posBorder_Y = posBorder_Y;
            last_speed_X    = speed_X;
            last_speed_Y    = speed_Y;
            posBorder_X     = pos_X + Width -1;
            posBorder_Y     = pos_Y + Height -1;
        }

        /// <summary>
        /// Set a position in the X axis for the caracter starting from the start/left
        /// </summary>
        /// /// /// <param name="_posMid_X">new top position in X axis</param>
        public void SetPos_X(int _pos_X)
        {
            pos_X = _pos_X;
            posMid_X = pos_X + (Width >> 1) - 1;
            posBorder_X = pos_X + Width - 1;
        }

        /// <summary>
        /// Set a position in the Y axis for the caracter starting from the start/top
        /// </summary>
        /// /// <param name="_posMid_Y">new top position in Y axis</param>
        public void SetPos_Y(int _pos_Y)
        {
            pos_Y = _pos_Y;
            posMid_Y = pos_Y + (Height >> 1) - 1;
            posBorder_Y = pos_Y + Height - 1;
        }

        /// <summary>
        /// Set a position in the X axis for the caracter starting from the middle
        /// </summary>
        /// <param name="_posMid_X">new middle position in X axis</param>
        public void SetPosMid_X(int _posMid_X)
        {
            posMid_X = _posMid_X;
            pos_X = posMid_X - (Width >> 1) + 1;
            posBorder_X = pos_X + Width - 1;
        }

        /// <summary>
        /// Set a position in the Y axis for the caracter starting from the middle
        /// </summary>
        /// <param name="_posMid_Y">new middle position in Y axis</param>
        public void SetPosMid_Y(int _posMid_Y)
        {
            posMid_Y = _posMid_Y;
            pos_Y = posMid_Y - (Height>>1)  +1;            
            posBorder_Y = pos_Y + Height - 1;
        }

        /// <summary>
        /// Pre-processing done to a portal to have an even position at the start of the shooting calculation
        /// </summary>
        /// <param name="_posMid_Y">new middle position in Y axis</param>
        public void SetPosMid_Y(int _portal_posMid_X, int _portal_posMid_y, string _orientation)
        {
            SetPosMid_X(_portal_posMid_X);
            SetPosBorder_Y(_portal_posMid_y);
            Rotation(_orientation);
            SetPosMid_X(_portal_posMid_X);
            SetPosBorder_Y(_portal_posMid_y);
        }

        /// <summary>
        /// Set a position in the X axis for the caracter starting from the end/rigth
        /// </summary>
        /// <param name="_posBorder_X">new border position in X axis</param>
        public void SetPosBorder_X(int _posBorder_X)
        {
            posBorder_X = _posBorder_X;
            pos_X = posBorder_X - Width + 1;
            posMid_X = pos_X + (Width >> 1) - 1;
        }

        /// <summary>
        /// Set a position in the Y axis for the caracter starting from the end/bottom
        /// </summary>
        /// <param name="_posBorder_Y">new border position in Y axis</param>
        public void SetPosBorder_Y(int _posBorder_Y)
        {
            posBorder_Y = _posBorder_Y;
            pos_Y = posBorder_Y - Height + 1;
            posMid_Y = pos_Y + (Height >> 1) - 1;
        }

        /// <summary>
        /// Move the character one pixel to the rigth
        /// </summary>
        public void X_pp()
        {
            pos_X++;
            posMid_X++;
            posBorder_X++;
        }

        /// <summary>
        /// Move the character one pixel to the left
        /// </summary>
        public void X_mm()
        {
            pos_X--;
            posMid_X--;
            posBorder_X--;
        }

        /// <summary>
        /// Move the character one pixel to the top
        /// </summary>
        public void Y_pp()
        {
            pos_Y++;
            posMid_Y++;
            posBorder_Y++;
        }

        /// <summary>
        /// Move the character one pixel to the bottom
        /// </summary>
        public void Y_mm()
        {
            pos_Y--;
            posMid_Y--;
            posBorder_Y--;
        }

        /// <summary>
        /// Move the character in the X axis from the sent numer of pixel
        /// </summary>
        /// <param name="_px">Number of pixel to move to the char</param>
        public void AddX(int _px)
        {
            pos_X += _px;
            posMid_X += _px;
            posBorder_X += _px;
        }

        /// <summary>
        /// Move the character in the Y axis from the sent numer of pixel
        /// </summary>
        /// <param name="_py">Number of pixel to move to the char</param>
        public void AddY(int _py)
        {
            pos_Y += _py;
            posMid_Y += _py;
            posBorder_Y += _py;
        }

        #endregion
        #region ViewHandler
        /// <summary>
        /// Set the new sprite according to the acceleration in both X and Y axis
        /// </summary>
        public void UpdateSprite()
        {
            if (acceleration_Y != 0)
            {
                isFalling = true;
            }
            // else false;
            if (speed_X > 0) //acceleration_X
            {
                if (!isFalling) { cur_tile = (cur_tile+1) % 5; }
                else
                {
                    if (speed_Y < 0) { cur_tile = 10; }
                    if (speed_Y > 0) { cur_tile = 11; }
                }
            }
            else if (speed_X < 0) //acceleration_X
            {
                if (!isFalling) { cur_tile = (cur_tile+1) % 5 + 5; }
                else
                {
                    if (speed_Y < 0) { cur_tile = 12; }
                    if (speed_Y > 0) { cur_tile = 13; }
                }
            }
            else
            { ///////////////////////////////////////// TEST Y BEFORE X
                if (isFalling)
                {
                    if (speed_Y < 0)
                    {
                        if  (cur_tile == 0) { cur_tile = 10; }
                        if  (cur_tile == 5) { cur_tile = 12; }
                    }
                    else if (speed_Y > 0)
                    {
                        if  (cur_tile == 0) { cur_tile = 11; }
                        if  (cur_tile == 5) { cur_tile = 13; }
                    }
                }
                else
                {
                    if       (cur_tile == 0)                 {  }
                    else if  (cur_tile > 0 && cur_tile < 5)  { cur_tile = (cur_tile + 1) % 5; }
                    else if  (cur_tile == 5)                 {  }
                    else if  (cur_tile > 5 && cur_tile < 10) { cur_tile = (cur_tile + 1) % 5 + 5; }
                    else
                    {
                        if      (cur_tile == 10 || cur_tile == 11) { cur_tile = 0; }
                        else if (cur_tile == 12 || cur_tile == 13) { cur_tile = 5; }
                    }
                }
            }
            cur_spr = spr.GetSprite(cur_tile);
        }
        /// <summary>
        /// Reset the area of the caracter to be saw to the default value (full view)
        /// </summary>
        public void ResetBorder()
        {
            pixelToSee_X = 0;
            pixelToSee_Y = 0;
            pixelToSeeBorder_X = Width;
            pixelToSeeBorder_Y = Height;
        }
        #endregion
    }
}
