using System;
using System.Drawing;
using System.Windows.Forms;

namespace Synless_Engine
{
    class Engine
    {
        #region Variables
        Bitmap screen = new Bitmap(640, 360);
        Caracter chell;
        Map carte = new Map();
        Caracter portal_blue;
        Caracter portal_orange;

        const int G = 50;
        const int PRESCALE = 25;
        const int JUMPFORCE = -12000;
        const int MOVINGFORCE = 8600;
        const int SPEEDCALLBACK = 450;
        const int PORTAL_X = 10000;
        const int PORTAL_Y = 500;
        const int GND = 2;
        const int SPEED = 1;
        const int CAPSPEED = 9;

        int portalShot_X = 0;
        int portalShot_Y = 0;
        int f_friction = 2;
        int f_frottement_Y = 0;
        int f_frottement_X = 0;
        int f_frottement_GND = 0;
        int f_ext_X = 0;
        int f_ext_Y = 0;
        int f_portal_X = 0;
        int f_portal_Y = 0;
        int f_poids = 0;
        int closeTick = 0;
        int lvl = 0;
        long tick = 0;

        bool KeyPressed_RIGHT = false;
        bool KeyPressed_LEFT = false;
        bool KeyPressed_UP = false;
        bool once = true;
        bool portalOnce = true;
        bool blueNotOrange = true;
        bool canCast = true;
        bool updatePortalPosBool = false;
        bool tobackup = false;
        bool displayBlue = true;
        bool displayOrange = true;
        bool begining = true;
        bool end = false;
        bool reverse = false;
        #endregion

        public Engine()
        {
            lvl = carte.first_lvl;
            resetLevel();
        }

        public Bitmap getScreen()
        {
            calculate();
            display();
            return screen;
        }

        #region Physic
        private void calculate()
        {
            chell.rememberPosition();
            kinematic();
            moveChell();
            checkChellBorder();
            chell.updateSprite();
            portal_blue.updateSprite();
            portal_orange.updateSprite();
            if (updatePortalPosBool == true)
            {
                updatePortalPosBool = false;
                updatePortalPos();
            }
        }

        /// <summary>
        /// Check the behavior of the engine regarding the character driving,
        /// CPU driven (enterring and leaving the level) or manual driven (user inputs)
        /// </summary>
        private void kinematic()
        {
            if (begining)
            {
                if (tick < 25)
                {
                    if (tick < 5)
                    {
                        chell.isFalling = false;
                        chell.acceleration_X = 1;
                        chell.speed_X = 1;
                        chell.X_pp();
                    }
                    else
                    {
                        if (tick > 15)
                        {
                            chell.acceleration_X = 10;
                            chell.speed_X = 0;
                        }
                        closeBarrier();
                    }
                }
                else
                {
                    begining = false;
                }
            }
            else if (end)
            {
                if (tick < 5)
                {
                    chell.isFalling = false;
                    chell.acceleration_X = 1;
                    chell.speed_X = 1;
                    chell.X_pp();
                }
                else
                {
                    lvl = (++lvl) % 10;
                    resetLevel();
                    end = false;
                }
            }
            else
            {
                end = begining = false;
                begining = false;
                kinematicEquations();
            }
            tick++;
        }

        /// <summary>
        /// Calculate the acceleration and speed of Chell
        /// </summary>
        private void kinematicEquations()
        {
            f_poids = G * chell.weight;
            //PROCESS Y AXIS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (KeyPressed_UP)
            {
                if (once && !chell.isFalling)
                {
                    f_ext_Y = JUMPFORCE;
                    once = false;
                }
                else
                {
                    f_ext_Y >>= 1;
                }
            }
            else
            {
                f_ext_Y = 0;
            }

            f_frottement_Y = chell.speed_Y * Math.Abs(chell.speed_Y) * f_friction;
            chell.acceleration_Y = ((f_poids - f_frottement_Y + f_ext_Y + f_portal_Y) / chell.weight) / PRESCALE;
            chell.speed_Y = chell.acceleration_Y + chell.last_speed_Y;
            f_portal_Y = f_portal_Y >> 2;
            //END PROCESS Y AXIS

            //PROCESS X AXIS // CHECK WITH A MAX SPEED
            f_frottement_X = chell.speed_X * Math.Abs(chell.speed_X) * f_friction;
            chell.acceleration_X = ((f_frottement_X + f_frottement_GND + f_ext_X + f_portal_X) / chell.weight) / PRESCALE;
            chell.speed_X = chell.acceleration_X + chell.last_speed_X;
            f_portal_X = f_portal_X >> 4;

            if (!chell.isFalling) //CONSTANT f_friction WITH GROUND
            {
                if (chell.speed_X > GND) { chell.speed_X = chell.speed_X - GND; }
                else if (chell.speed_X < -GND) { chell.speed_X = chell.speed_X + GND; }
                else { chell.speed_X >>= SPEED; }

                if (KeyPressed_RIGHT && !KeyPressed_LEFT/* && canGoRigth == 0*/) { f_ext_X = MOVINGFORCE - (SPEEDCALLBACK * chell.speed_X); }
                else if (!KeyPressed_RIGHT && KeyPressed_LEFT/* && canGoLeft == 0*/) { f_ext_X = -MOVINGFORCE - (SPEEDCALLBACK * chell.speed_X); }
                else 
                {
                    f_ext_X = 0;
                }
            }
            else //SAME -> TOTAL AIR CONTROL
            {
                if (chell.speed_X > GND) { chell.speed_X = chell.speed_X - GND; }
                else if (chell.speed_X < -GND) { chell.speed_X = chell.speed_X + GND; }
                else { chell.speed_X >>= SPEED; }

                if (KeyPressed_RIGHT && !KeyPressed_LEFT) { f_ext_X = MOVINGFORCE - (SPEEDCALLBACK * chell.speed_X); }
                else if (!KeyPressed_RIGHT && KeyPressed_LEFT) { f_ext_X = -MOVINGFORCE - (SPEEDCALLBACK * chell.speed_X); }
                else 
                {
                    f_ext_X = 0; 
                }

            }
            if (chell.speed_X < 2 && chell.speed_X > -2 && !chell.isFalling) //STOP IF TO SLOW, USEFULL WITH FLOAT
            {
                chell.speed_X = 0;
            }
            //END PROCESS X AXIS ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        /// <summary>
        /// Move chell according her speed and current position
        /// Also check if she bump into some portal
        /// </summary>
        /// <param name="_px">Number of pixel to move to the char</param>
        private void moveChell()
        {
            for (int n = 0; n < chell.speed_Y; n++) //  ↓
            {
                chell.Y_pp();

                if (chell.posBorder_Y >= portal_blue.pos_Y && chell.pos_Y < portal_blue.posBorder_Y && chell.pos_X >= portal_blue.pos_X && chell.posBorder_X <= portal_blue.posBorder_X)
                {   //TEST BLUE TO ORANGE JUMP
                    if (chell.posMid_Y > portal_blue.pos_Y && portal_blue.orientation == "bot")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(true); portalOnce = true; }
                        break;
                    }
                }
                else if (chell.posBorder_Y >= portal_orange.pos_Y && chell.pos_Y < portal_orange.posBorder_Y && chell.pos_X >= portal_orange.pos_X && chell.posBorder_X <= portal_orange.posBorder_X)
                {   //TEST ORANGE TO BLUE JUMP
                    if (chell.posMid_Y > portal_orange.pos_Y && portal_orange.orientation == "bot")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(false); portalOnce = true; }
                        break;
                    }
                }
                else
                {   //BORDER OF THE MAP TEST
                    if (carte.level[lvl].tile_type[chell.pos_X / carte.tile_size, chell.posBorder_Y / carte.tile_size] != 0 || carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, (chell.posBorder_Y / carte.tile_size)] != 0)
                    {
                        chell.Y_mm();
                        while (carte.level[lvl].tile_type[chell.pos_X / carte.tile_size, chell.posBorder_Y / carte.tile_size] != 0 && carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, (chell.posBorder_Y / carte.tile_size)] != 0)
                        {
                            chell.Y_mm();
                        }
                        chell.speed_Y = 0;
                        chell.acceleration_Y = 0;
                        chell.isFalling = false;
                        chell.canJump = true;
                        if (!KeyPressed_UP) { once = true; }
                        break;
                    }
                }
            }
            for (int n = 0; n > chell.speed_Y; n--) //  ↑  JOB TO DO HERE
            {
                chell.Y_mm();
                if (chell.pos_X >= portal_blue.pos_X && chell.posBorder_X <= portal_blue.posBorder_X && chell.pos_Y <= portal_blue.pos_Y && chell.posBorder_Y >= portal_blue.posBorder_Y)
                {
                    if (chell.posMid_Y < portal_blue.posBorder_Y && portal_blue.orientation == "top")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(true); portalOnce = true; }
                        break;
                    }
                }
                else if (chell.pos_X >= portal_orange.pos_X && chell.posBorder_X <= portal_orange.posBorder_X && chell.pos_Y <= portal_orange.pos_Y && chell.posBorder_Y >= portal_orange.posBorder_Y)
                {
                    if (chell.posMid_Y < portal_orange.posBorder_Y && portal_orange.orientation == "top")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(false); portalOnce = true; }
                        break;
                    }
                }
                else
                {
                    if (carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, (chell.pos_Y / carte.tile_size)] != 0 || carte.level[lvl].tile_type[chell.pos_X / carte.tile_size, chell.pos_Y / carte.tile_size] != 0)
                    {
                        chell.Y_pp();
                        chell.speed_Y = 0;
                        chell.acceleration_Y = 0;
                        chell.isFalling = true;
                        break;
                    }
                }
            }
            for (int n = 0; n < chell.speed_X; n++) //  →
            {
                chell.X_pp();

                if (chell.posBorder_X >= portal_blue.pos_X && chell.pos_X <= portal_blue.posBorder_X && chell.pos_Y >= portal_blue.pos_Y && chell.posBorder_Y <= portal_blue.posBorder_Y)
                {
                    if (chell.posMid_X > portal_blue.pos_X && portal_blue.orientation == "rigth")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(true); portalOnce = true; }
                        break;
                    }
                }
                else if (chell.pos_Y >= portal_orange.pos_Y && chell.posBorder_Y <= (portal_orange.posBorder_Y + 1) && chell.posBorder_X >= portal_orange.pos_X && chell.posMid_X <= portal_orange.posBorder_X)
                {
                    if (chell.posMid_X >= portal_orange.pos_X && portal_orange.orientation == "rigth")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(false); portalOnce = true; }
                        break;
                    }
                }
                else
                {
                    if (carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, chell.pos_Y / carte.tile_size] == 4 && carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, chell.posBorder_Y / carte.tile_size] == 4)
                    {
                        if (carte.level[lvl].tile_type[chell.posMid_X / carte.tile_size, chell.pos_Y / carte.tile_size] == 4 && carte.level[lvl].tile_type[chell.posMid_X / carte.tile_size, chell.posBorder_Y / carte.tile_size] == 4)
                        {
                            chell.X_mm();
                            chell.speed_X = 0;
                            chell.acceleration_X = 0;
                            if (!end) { tick = 0; }
                            end = true;
                            break;
                        }
                    }
                    else if (carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, chell.pos_Y / carte.tile_size] != 0 || carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, chell.posMid_Y / carte.tile_size] != 0 || carte.level[lvl].tile_type[chell.posBorder_X / carte.tile_size, chell.posBorder_Y / carte.tile_size] != 0)
                    {
                        chell.X_mm();
                        chell.speed_X = 0;
                        chell.acceleration_X = 0;
                        break;
                    }
                }
            }
            for (int n = 0; n > chell.speed_X; n--) //  ←  
            {
                chell.X_mm();
                if (chell.pos_X <= portal_blue.posBorder_X && chell.posBorder_X >= portal_blue.pos_X && chell.pos_Y >= portal_blue.pos_Y && chell.posBorder_Y <= portal_blue.posBorder_Y)
                {
                    if (chell.posMid_X <= portal_blue.posBorder_X && portal_blue.orientation == "left")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(true); portalOnce = true; }
                        break;
                    }
                }
                else if (chell.pos_X <= portal_orange.posBorder_X && chell.posBorder_X >= portal_orange.pos_X && chell.pos_Y >= portal_orange.pos_Y && chell.posBorder_Y <= portal_orange.posBorder_Y)
                {
                    if (chell.posMid_X <= portal_orange.posBorder_X && portal_orange.orientation == "left")
                    {
                        if (portalOnce) { portalOnce = false; }
                        else { BlueToOrange(false); portalOnce = true; }
                        break;
                    }
                }
                else
                {
                    if (carte.level[lvl].tile_type[chell.pos_X / carte.tile_size, chell.pos_Y / carte.tile_size] != 0 || carte.level[lvl].tile_type[chell.pos_X / carte.tile_size, chell.posMid_Y / carte.tile_size] != 0 || carte.level[lvl].tile_type[chell.pos_X / carte.tile_size, chell.posBorder_Y / carte.tile_size] != 0)
                    {
                        chell.X_pp();
                        chell.speed_X = 0;
                        chell.acceleration_X = 0;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Teleport Chell from one portal to another
        /// Re-affect forces
        /// </summary>
        /// <param name="_b2o">True : Jumping from blue to orange | False : jumping from orange to blue</param>
        private void BlueToOrange(bool _b2o)
        {
            #region blue
            if (_b2o)
            {
                if (portal_blue.orientation == "rigth")
                {
                    if (portal_orange.orientation == "rigth")
                    {
                        chell.acceleration_X = -chell.acceleration_X;
                        chell.last_speed_X = -chell.speed_X;
                        chell.speed_X = chell.last_speed_X = -chell.speed_X;

                        chell.setPosMid_Y(portal_orange.posMid_Y + chell.posMid_Y - portal_blue.posMid_Y);
                        chell.setPosMid_X(portal_orange.posMid_X);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_orange.orientation == "left")
                    {
                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y + chell.posMid_Y - portal_blue.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_orange.orientation == "top")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_Y(portal_orange.posMid_Y);
                        chell.setPosMid_X(portal_orange.posMid_X);

                        f_portal_Y = PORTAL_Y;
                    }
                    else if (portal_orange.orientation == "bot")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = -chell.speed_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_Y = -chell.acceleration_X;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_Y = -PORTAL_Y;
                    }
                }
                else if (portal_blue.orientation == "left")
                {
                    if (portal_orange.orientation == "rigth")
                    {
                        chell.setPosMid_X(portal_orange.posMid_X - (chell.Width >> 1));
                        chell.setPosMid_Y(portal_orange.posMid_Y + chell.posMid_Y - portal_blue.posMid_Y);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_orange.orientation == "left")
                    {
                        chell.acceleration_X = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -chell.speed_X;
                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y + chell.posMid_Y - portal_blue.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_orange.orientation == "top")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = -chell.speed_X;
                        chell.acceleration_Y = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_Y = PORTAL_Y;
                    }
                    else if (portal_orange.orientation == "bot")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = -chell.speed_X;
                        chell.acceleration_Y = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_Y = -PORTAL_Y;
                    }
                }
                else if (portal_blue.orientation == "top")
                {
                    if (portal_orange.orientation == "rigth")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -tmp1;
                        chell.acceleration_X = -tmp2;

                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_orange.orientation == "left")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -tmp1;
                        chell.acceleration_X = -tmp2;

                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_orange.orientation == "top")
                    {
                        chell.speed_Y = -chell.speed_Y;
                        chell.acceleration_Y = -chell.acceleration_Y;

                        chell.setPos_X(portal_orange.pos_X + chell.pos_X - portal_blue.pos_X);
                        chell.setPos_Y(portal_orange.pos_Y - (chell.Height >> 1));

                        f_portal_Y = PORTAL_Y;
                    }
                    else if (portal_orange.orientation == "bot")
                    {
                        chell.setPos_X(portal_orange.pos_X + chell.pos_X - portal_blue.pos_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_Y = -PORTAL_Y /** Math.Max(1, (15 - chell.speed_Y))*/;
                        //f_portal_Y = -PORTAL_Y * 6;
                    }
                }
                else if (portal_blue.orientation == "bot")
                {
                    if (portal_orange.orientation == "rigth")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -tmp1;
                        chell.acceleration_X = -tmp2;

                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_orange.orientation == "left")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_orange.orientation == "top")
                    {
                        f_portal_Y = PORTAL_Y;

                        chell.setPosMid_Y(portal_orange.posMid_Y - (chell.Height >> 1));
                        chell.setPosMid_X(portal_orange.posMid_X + chell.posMid_X - portal_blue.posMid_X);
                    }
                    else if (portal_orange.orientation == "bot")
                    {
                        chell.speed_Y = -chell.speed_Y;
                        chell.acceleration_Y = -chell.acceleration_Y;

                        chell.setPosMid_X(portal_orange.posMid_X + chell.posMid_X - portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_orange.posMid_Y);

                        f_portal_Y = -PORTAL_Y;
                    }
                }

            }
            #endregion
            #region orange
            else
            {
                if (portal_orange.orientation == "rigth")
                {
                    if (portal_blue.orientation == "rigth")
                    {
                        chell.acceleration_X = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -chell.speed_X;

                        chell.setPos_Y(chell.pos_Y + portal_blue.pos_Y - portal_orange.pos_Y);
                        chell.setPosMid_X(portal_blue.posMid_X);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_blue.orientation == "left")
                    {
                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPos_Y(chell.pos_Y + portal_blue.pos_Y - portal_orange.pos_Y);
                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_blue.orientation == "top")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_Y = PORTAL_Y;
                    }
                    else if (portal_blue.orientation == "bot")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = -chell.speed_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_Y = -chell.acceleration_X;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_Y = -PORTAL_Y;
                    }
                }
                else if (portal_orange.orientation == "left")
                {
                    if (portal_blue.orientation == "rigth")
                    {
                        chell.setPosMid_X(portal_blue.posMid_X - (chell.Width >> 1));
                        chell.setPos_Y(portal_blue.pos_Y + chell.pos_Y - portal_orange.pos_Y);

                        f_portal_X = -PORTAL_X;

                    }
                    else if (portal_blue.orientation == "left")
                    {
                        chell.acceleration_X = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -chell.speed_X;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(chell.posMid_Y + portal_blue.posMid_Y - portal_orange.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_blue.orientation == "top")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = -chell.speed_X;
                        chell.acceleration_Y = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_Y = PORTAL_Y;
                    }

                    else if (portal_blue.orientation == "bot")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_Y = -PORTAL_Y;
                    }
                }
                else if (portal_orange.orientation == "top")
                {
                    if (portal_blue.orientation == "rigth")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -tmp1;
                        chell.acceleration_X = -tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_blue.orientation == "left")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = -chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -tmp1;
                        chell.acceleration_X = -tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_blue.orientation == "top")
                    {
                        chell.speed_Y = -chell.speed_Y;
                        chell.acceleration_Y = -chell.acceleration_Y;

                        chell.setPos_X(portal_blue.pos_X + chell.pos_X - portal_orange.pos_X);
                        chell.setPos_Y(portal_blue.pos_Y - (chell.Height >> 1));

                        f_portal_Y = PORTAL_Y;
                    }
                    else if (portal_blue.orientation == "bot")
                    {
                        chell.setPos_X(portal_blue.pos_X + chell.pos_X - portal_orange.pos_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_Y = -PORTAL_Y;
                    }
                }
                else if (portal_orange.orientation == "bot")
                {
                    if (portal_blue.orientation == "rigth")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -tmp1;
                        chell.acceleration_X = -tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_blue.orientation == "left")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_blue.orientation == "top")
                    {
                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_Y = PORTAL_Y;
                    }
                    else if (portal_blue.orientation == "bot")
                    {
                        chell.speed_Y = -chell.speed_Y;
                        chell.acceleration_Y = -chell.acceleration_Y;

                        chell.setPosMid_X(portal_blue.posMid_X + (chell.posMid_X - portal_orange.posMid_X));
                        chell.setPosMid_Y(portal_blue.posMid_Y);
                        f_portal_Y = -PORTAL_Y;
                    }
                }
                else if (portal_orange.orientation == "top")
                {
                    if (portal_blue.orientation == "rigth")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = -tmp1;
                        chell.acceleration_X = -tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_X = -PORTAL_X;
                    }
                    else if (portal_blue.orientation == "left")
                    {
                        int tmp1 = chell.speed_Y;
                        int tmp2 = chell.acceleration_Y;
                        chell.speed_Y = chell.speed_X;
                        chell.acceleration_Y = chell.acceleration_X;
                        chell.speed_X = chell.last_speed_X = tmp1;
                        chell.acceleration_X = tmp2;

                        chell.setPosMid_X(portal_blue.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_X = PORTAL_X;
                    }
                    else if (portal_blue.orientation == "top")
                    {
                        chell.speed_Y = -chell.speed_Y;
                        chell.acceleration_Y = -chell.acceleration_Y;

                        chell.setPosMid_X(portal_blue.posMid_X + chell.posMid_X - portal_orange.posMid_X);
                        chell.setPosMid_Y(portal_blue.posMid_Y);

                        f_portal_Y = PORTAL_Y;
                    }
                    else if (portal_blue.orientation == "bot")
                    {
                        chell.setPos_X(portal_blue.pos_X + chell.pos_X - portal_orange.pos_X);
                        chell.setPos_Y(portal_blue.pos_Y - (chell.Height >> 1));

                        f_portal_Y = -PORTAL_Y;
                    }
                }
            }
            #endregion
            chell.isFalling = true;
            chell.canJump = false;
        }

        /// <summary>
        /// Set Chell viewing border according to her position around portals
        /// </summary>
        private void checkChellBorder()
        {
            if (chell.pos_X <= portal_orange.pos_X && chell.posBorder_X >= portal_orange.pos_X && chell.posBorder_Y > portal_orange.pos_Y && chell.pos_Y < portal_orange.posBorder_Y && portal_orange.orientation == "rigth")
            {
                chell.pixelToSee_X = 0;
                chell.pixelToSeeBorder_X = chell.Width - Math.Min((chell.posBorder_X - portal_orange.pos_X), chell.Width);
            }
            if (chell.pos_X <= portal_blue.pos_X && chell.posBorder_X >= portal_blue.pos_X && chell.posBorder_Y > portal_blue.pos_Y && chell.pos_Y < portal_blue.posBorder_Y && portal_blue.orientation == "rigth")
            {
                chell.pixelToSee_X = 0;
                chell.pixelToSeeBorder_X = chell.Width - Math.Min((chell.posBorder_X - portal_blue.pos_X), chell.Width);
            }
            if (chell.pos_X <= portal_orange.pos_X && chell.posBorder_X >= portal_orange.pos_X && chell.posBorder_Y > portal_orange.pos_Y && chell.pos_Y < portal_orange.posBorder_Y && portal_orange.orientation == "left")
            {
                chell.pixelToSee_X = Math.Min((portal_orange.pos_X - chell.pos_X), chell.Width);
                chell.pixelToSeeBorder_X = chell.Width;
            }
            if (chell.pos_X <= portal_blue.pos_X && chell.posBorder_X >= portal_blue.pos_X && chell.posBorder_Y > portal_blue.pos_Y && chell.pos_Y < portal_blue.posBorder_Y && portal_blue.orientation == "left")
            {
                chell.pixelToSee_X = Math.Min((portal_blue.pos_X - chell.pos_X), chell.Width);
                chell.pixelToSeeBorder_X = chell.Width;
            }

            if (chell.pos_Y < portal_orange.pos_Y && chell.posBorder_Y > portal_orange.pos_Y && chell.pos_X >= portal_orange.pos_X && chell.posBorder_X <= portal_orange.posBorder_X && portal_orange.orientation == "bot")
            {
                chell.pixelToSee_Y = 0;
                chell.pixelToSeeBorder_Y = chell.Height - Math.Min((chell.posBorder_Y - portal_orange.pos_Y), chell.Height);
            }
            if (chell.pos_Y < portal_blue.pos_Y && chell.posBorder_Y > portal_blue.pos_Y && chell.pos_X >= portal_blue.pos_X && chell.posBorder_X <= portal_blue.posBorder_X && portal_blue.orientation == "bot")
            {
                chell.pixelToSee_Y = 0;
                chell.pixelToSeeBorder_Y = chell.Height - Math.Min((chell.posBorder_Y - portal_blue.pos_Y), chell.Height);
            }
            if (chell.pos_Y < portal_orange.pos_Y && chell.posBorder_Y > portal_orange.pos_Y && chell.pos_X >= portal_orange.pos_X && chell.posBorder_X <= portal_orange.posBorder_X && portal_orange.orientation == "top")
            {
                chell.pixelToSee_Y = portal_orange.pos_Y - chell.pos_Y;
                chell.pixelToSeeBorder_Y = chell.Height;
            }
            if (chell.pos_Y < portal_blue.pos_Y && chell.posBorder_Y > portal_blue.pos_Y && chell.pos_X >= portal_blue.pos_X && chell.posBorder_X <= portal_blue.posBorder_X && portal_blue.orientation == "top")
            {
                chell.pixelToSee_Y = portal_blue.pos_Y - chell.pos_Y;
                chell.pixelToSeeBorder_Y = chell.Height;
            }
        }

        /// <summary>
        /// Set the positions and orientation of the portals around the map according to the shots
        /// </summary>
        private void updatePortalPos()
        {
            if (canCast && chell.pixelToSee_X == 0 && chell.pixelToSee_Y == 0 && chell.pixelToSeeBorder_X == chell.Width && chell.pixelToSeeBorder_Y == chell.Height)
            {
                wipePortal(blueNotOrange); // OR WHIPE THE BACKUP
                #region blue
                if (blueNotOrange)
                {   // PORTAL BLUE LOCATION PROCESSING
                    tobackup = false;
                    Caracter backup = new Caracter(portal_blue, true);
                    if (chell.isFalling)
                    {
                        portal_blue.setPosBorder_Y(chell.posBorder_Y - 2);
                    }
                    else
                    {
                        portal_blue.setPosBorder_Y(chell.posBorder_Y + 2);
                    }
                    portal_blue.setPosMid_X(chell.posMid_X);
                    portal_blue.setPosMid_Y(chell.posMid_X, chell.posMid_Y, "bot");
                    if (portalShot_X > 0 && portalShot_Y == 0)
                    {   // 6
                        portal_blue.rotation("rigth");
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_mm(); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.Y_mm(); }
                        if (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_X < 0 && portalShot_Y == 0)
                    {   // 4

                        portal_blue.rotation("left");
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0/* || carte.tile_type[lvl,portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 3*/) { portal_blue.X_pp(); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.Y_mm(); }
                        if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y > 0 && portalShot_X == 0)
                    {   // 2
                        portal_blue.rotation("bot");
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_Y(portalShot_Y); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0 /*|| carte.tile_type[lvl,portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 3*/) { portal_blue.Y_mm(); }
                        while (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_pp(); }
                        while (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_mm(); }
                        if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y < 0 && portalShot_X == 0)
                    {   // 8
                        portal_blue.rotation("top");
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_Y(portalShot_Y); portal_blue.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0 /*|| carte.tile_type[lvl,portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 3*/) { portal_blue.Y_pp(); }
                        while (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_pp(); }
                        while (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_mm(); }
                        if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y > 0 && portalShot_X < 0)
                    {   // 1
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_Y(portalShot_Y); portal_blue.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0 /*|| carte.tile_type[lvl,portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 3*/) { portal_blue.Y_mm(); portal_blue.X_pp(); }
                        if (carte.level[lvl].tile_type[(portal_blue.posMid_X - 1) / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("left");
                            while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); }
                            while (carte.level[lvl].tile_type[(portal_blue.posMid_X + 1) / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.Y_mm(); } // TRICKY TRICKY TRICKY
                            if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, (portal_blue.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("bot");
                            while (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y > 0 && portalShot_X > 0)
                    {   // 3
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_Y(portalShot_Y); portal_blue.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.Y_mm(); portal_blue.X_mm(); }
                        if (carte.level[lvl].tile_type[(portal_blue.posMid_X + 1) / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("rigth");
                            while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); }
                            while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.Y_mm(); }
                            if (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, (portal_blue.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("bot");
                            while (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y < 0 && portalShot_X > 0)
                    {   // 9
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_Y(portalShot_Y); portal_blue.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); portal_blue.X_mm(); }
                        if (carte.level[lvl].tile_type[(portal_blue.posMid_X + 1) / carte.tile_size, (portal_blue.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("rigth");
                            while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); }
                            while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.Y_mm(); }
                            if (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, (portal_blue.posMid_Y - 1) / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("top");
                            while (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y < 0 && portalShot_X < 0)
                    {   // 7
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] == 3) { portal_blue.add_Y(portalShot_Y); portal_blue.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); portal_blue.X_pp(); }
                        if (carte.level[lvl].tile_type[(portal_blue.posMid_X - 1) / carte.tile_size, (portal_blue.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("left");
                            while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 0) { portal_blue.Y_pp(); }
                            while (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 0) { portal_blue.Y_mm(); }
                            if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_blue.posMid_X / carte.tile_size, (portal_blue.posMid_Y - 1) / carte.tile_size] != 0)
                        {
                            portal_blue.rotation("top");
                            while (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.posMid_Y / carte.tile_size] != 0) { portal_blue.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_blue.pos_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_blue.posBorder_X / carte.tile_size, portal_blue.pos_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }
                    if (portal_blue.posBorder_X > portal_orange.pos_X && portal_blue.pos_X < portal_orange.posBorder_X && portal_blue.posMid_Y > portal_orange.pos_Y && portal_blue.posMid_Y < portal_orange.posBorder_Y && portal_blue.orientation == portal_orange.orientation)
                    {
                        tobackup = true;
                    }
                    if (portal_blue.posBorder_Y > portal_orange.pos_Y && portal_blue.pos_Y < portal_orange.posBorder_Y && portal_blue.posMid_X > portal_orange.pos_X && portal_blue.posMid_X < portal_orange.posBorder_X && portal_blue.orientation == portal_orange.orientation)
                    {
                        tobackup = true;
                    }

                    if (tobackup)
                    {
                        portal_blue = backup;
                        displayBlue = false;
                        tobackup = false;
                    }
                    else
                    {
                        displayBlue = true;
                        //wipePortal(blueNotOrange);
                    }
                }
                #endregion
                #region orange
                else
                {   // PORTAL ORANGE LOCATION PROCESSING                    
                    Caracter backup = new Caracter(portal_orange, false);
                    portal_orange.setPosMid_X(chell.posMid_X);
                    if (chell.isFalling)
                    {
                        portal_orange.setPosBorder_Y(chell.posBorder_Y);
                    }
                    else
                    {
                        portal_orange.setPosBorder_Y(chell.posBorder_Y + 2);
                    }
                    portal_orange.setPosMid_Y(chell.posMid_X, chell.posMid_Y, "bot");
                    if (portalShot_X > 0 && portalShot_Y == 0)
                    {   // 6
                        portal_orange.rotation("rigth");
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_mm(); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); }
                        int a = carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size];
                        if (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_X < 0 && portalShot_Y == 0)
                    {   // 4

                        portal_orange.rotation("left");
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_pp(); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); }
                        if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y > 0 && portalShot_X == 0)
                    {   // 2
                        portal_orange.rotation("bot");
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_Y(portalShot_Y); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); }
                        while (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_pp(); }
                        while (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_mm(); }
                        if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y < 0 && portalShot_X == 0)
                    {   // 8
                        portal_orange.rotation("top");
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_Y(portalShot_Y); portal_orange.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); }
                        while (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_pp(); }
                        while (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_mm(); }
                        if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1)
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y > 0 && portalShot_X < 0)
                    {   // 1
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_Y(portalShot_Y); portal_orange.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); portal_orange.X_pp(); }
                        if (carte.level[lvl].tile_type[(portal_orange.posMid_X - 1) / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("left");
                            while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); }
                            while (carte.level[lvl].tile_type[(portal_orange.posMid_X + 1) / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); } // TRICKY TRICKY TRICKY
                            if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, (portal_orange.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("bot");
                            while (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y > 0 && portalShot_X > 0)
                    {   // 3
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_Y(portalShot_Y); portal_orange.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); portal_orange.X_mm(); }
                        if (carte.level[lvl].tile_type[(portal_orange.posMid_X + 1) / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("rigth");
                            while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); }
                            while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); }
                            if (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, (portal_orange.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("bot");
                            while (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y < 0 && portalShot_X > 0)
                    {   // 9
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_Y(portalShot_Y); portal_orange.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); portal_orange.X_mm(); }
                        if (carte.level[lvl].tile_type[(portal_orange.posMid_X + 1) / carte.tile_size, (portal_orange.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("rigth");
                            while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); }
                            while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); }
                            if (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, (portal_orange.posMid_Y - 1) / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("top");
                            while (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }
                    if (portalShot_Y < 0 && portalShot_X < 0)
                    {   // 7
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 0 || carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] == 3) { portal_orange.add_Y(portalShot_Y); portal_orange.add_X(portalShot_X); }
                        while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); portal_orange.X_pp(); }
                        if (carte.level[lvl].tile_type[(portal_orange.posMid_X - 1) / carte.tile_size, (portal_orange.posMid_Y + 1) / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("left");
                            while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 0) { portal_orange.Y_pp(); }
                            while (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 0) { portal_orange.Y_mm(); }
                            if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posBorder_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else if (carte.level[lvl].tile_type[portal_orange.posMid_X / carte.tile_size, (portal_orange.posMid_Y - 1) / carte.tile_size] != 0)
                        {
                            portal_orange.rotation("top");
                            while (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_pp(); }
                            while (carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.posMid_Y / carte.tile_size] != 0) { portal_orange.X_mm(); }
                            if (carte.level[lvl].tile_type[portal_orange.pos_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1 || carte.level[lvl].tile_type[portal_orange.posBorder_X / carte.tile_size, portal_orange.pos_Y / carte.tile_size] != 1)
                            {
                                tobackup = true;
                            }
                        }
                        else
                        {
                            tobackup = true;
                        }
                    }

                    if (portal_orange.posBorder_X > portal_blue.pos_X && portal_orange.pos_X < portal_blue.posBorder_X && portal_orange.posMid_Y > portal_blue.pos_Y && portal_orange.posMid_Y < portal_blue.posBorder_Y && portal_orange.orientation == portal_blue.orientation)
                    {
                        tobackup = true;
                    }
                    if (portal_orange.posBorder_Y > portal_blue.pos_Y && portal_orange.pos_Y < portal_blue.posBorder_Y && portal_orange.posMid_X > portal_blue.pos_X && portal_orange.posMid_X < portal_blue.posBorder_X && portal_orange.orientation == portal_blue.orientation)
                    {
                        tobackup = true;
                    }
                    if (tobackup)
                    {
                        portal_orange = backup;
                        displayBlue = false;
                        tobackup = false;
                    }
                    else
                    {
                        displayBlue = true;
                        //wipePortal(blueNotOrange);
                    }
                }
                #endregion
            }
            tobackup = false;
            displayBlue = true;
            portalShot_X = 0;
            portalShot_Y = 0;
        }

        /// <summary>
        /// Reset the current level
        /// </summary>
        private void resetLevel()
        {   // RESET THE SCREEN
            chell = new Caracter(carte.level[lvl%carte.level.Length].perso_start_X, carte.level[lvl].perso_start_Y);
            portal_blue = new Caracter(true, carte.level[lvl].portal_blue_start_X, carte.level[lvl].portal_blue_start_Y, carte.level[lvl].portal_blue_orientation);
            portal_orange = new Caracter(false, carte.level[lvl].portal_orange_start_X, carte.level[lvl].portal_orange_start_Y, carte.level[lvl].portal_orange_orientation);
            reverse = carte.level[lvl].reverse_map;
            begining = true;
            closeTick = 0;
            tick = 0;
            resetScreen();
            KeyPressed_UP = false;
            KeyPressed_RIGHT = false;
            KeyPressed_LEFT = false;
        }
        #endregion

        #region Render
        /// <summary>
        /// Create and display a barrier in front of the enter of the level
        /// Each call add 2 pixel to the botton
        /// </summary>
        private void closeBarrier()
        {
            Bitmap tmp = new Bitmap(2, 2);
            tmp.SetPixel(0, 0, Color.Black);
            tmp.SetPixel(1, 0, Color.Black);
            tmp.SetPixel(0, 1, Color.Black);
            tmp.SetPixel(1, 1, Color.Black);
            addToScreen(tmp, carte.level[lvl].barrier_start_X, closeTick + carte.level[lvl].barrier_start_Y);
            closeTick += 2;
        }

        /// <summary>
        /// Main display fonction
        /// Display chell and the two portals
        /// </summary>
        private void display()
        {
            if (chell.pos_X != chell.last_pos_X || chell.pos_Y != chell.last_pos_Y || true)
            {   // COVER THE CHARACTER AND REDRAW IT IN THE NEW POSITION
                wipeChar();
                for (int y = chell.pixelToSee_Y; y < chell.pixelToSeeBorder_Y; y++)
                {
                    for (int x = chell.pixelToSee_X; x < chell.pixelToSeeBorder_X; x++)
                    {
                        if (chell.cur_spr.GetPixel(x, y) != Color.FromArgb(0, 0, 0))
                        {
                            if (x + chell.pos_X < screen.Width && y + chell.pos_Y < screen.Height)
                            {
                                if (!reverse)
                                {
                                    screen.SetPixel(x + chell.pos_X, y + chell.pos_Y, chell.cur_spr.GetPixel(x, y));
                                }
                                else
                                {
                                    screen.SetPixel(639 - (x + chell.pos_X), 359 - (y + chell.pos_Y), chell.cur_spr.GetPixel(x, y));
                                }

                            }
                        }
                        // ELSE BACKGROUND COLOR
                    }
                }
                chell.resetBorder();
            }
            // DRAWS THE PORTALS
            if (displayBlue)
            {
                for (int y = 0; y < portal_blue.cur_spr.Height; y++)
                {
                    for (int x = 0; x < portal_blue.cur_spr.Width; x++)
                    {
                        if (portal_blue.cur_spr.GetPixel(x, y) != Color.FromArgb(0, 0, 0))
                        {
                            if (portal_blue.orientation == "left" || portal_blue.orientation == "rigth")
                            {
                                if (!reverse)
                                {
                                    screen.SetPixel(x + portal_blue.pos_X, y + portal_blue.pos_Y, portal_blue.cur_spr.GetPixel(x, y));
                                }
                                else
                                {
                                    screen.SetPixel(639 - (x + portal_blue.pos_X), 359 - (y + portal_blue.pos_Y), portal_blue.cur_spr.GetPixel(x, y));
                                }
                            }
                            else if (portal_blue.orientation == "top" || portal_blue.orientation == "bot")
                            {
                                if (!reverse)
                                {
                                    screen.SetPixel(y + portal_blue.pos_X, x + portal_blue.pos_Y, portal_blue.cur_spr.GetPixel(x, y));
                                }
                                else
                                {
                                    screen.SetPixel(639 - (y + portal_blue.pos_X), 359 - (x + portal_blue.pos_Y), portal_blue.cur_spr.GetPixel(x, y));
                                }
                            }
                        }
                    }
                }
            }
            if (displayOrange)
            {
                for (int y = 0; y < portal_orange.cur_spr.Height; y++)
                {
                    for (int x = 0; x < portal_orange.cur_spr.Width; x++)
                    {
                        if (portal_orange.cur_spr.GetPixel(x, y) != Color.FromArgb(0, 0, 0))
                        {
                            if (portal_orange.orientation == "left" || portal_orange.orientation == "rigth")
                            {
                                if (!reverse)
                                {
                                    screen.SetPixel(x + portal_orange.pos_X, y + portal_orange.pos_Y, portal_orange.cur_spr.GetPixel(x, y));
                                }
                                else
                                {
                                    screen.SetPixel(639 - (x + portal_orange.pos_X), 359 - (y + portal_orange.pos_Y), portal_orange.cur_spr.GetPixel(x, y));
                                }
                            }
                            else if (portal_orange.orientation == "top" || portal_orange.orientation == "bot")
                            {
                                if (!reverse)
                                {
                                    screen.SetPixel(y + portal_orange.pos_X, x + portal_orange.pos_Y, portal_orange.cur_spr.GetPixel(x, y));
                                }
                                else
                                {
                                    screen.SetPixel(639 - (y + portal_orange.pos_X), 359 - (x + portal_orange.pos_Y), portal_orange.cur_spr.GetPixel(x, y));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Soft reset of the screen - redraw the map
        /// </summary>
        private void resetScreen()
        {
            for (int y = 0; y < carte.tile_Height; y++)
            {
                for (int x = 0; x < carte.tile_Width; x++)
                {
                    //addToScreen(carte.level[lvl].spr tile_type[x, y], (x * carte.tile_size), (y * carte.tile_size));
                    addToScreen(carte.level[lvl].getBlock(x, y), x * carte.tile_size, y * carte.tile_size);
                    //Bitmap tmp = new Bitmap(carte.level[lvl].background.GetSprite[carte.level[lvl].background.tiles_type[x,y]]);
                    //addToScreen(carte.background.tiles[carte.tile_type[lvl,x, y]], (x * carte.tile_size), (y * carte.tile_size));
                }
            }
        }

        /// <summary>
        /// Wipe the char (last_position)
        /// </summary>
        private void wipeChar()
        {   // CALCULATE THE POINT WHERE THE MAP SHOULD BE REDRAW TO COVER THE OLD CHARACTER POSITION
            for (int x = chell.last_pos_X / carte.tile_size; x <= chell.last_posBorder_X / carte.tile_size; x++)
            {
                for (int y = chell.last_pos_Y / carte.tile_size; y <= chell.last_posBorder_Y / carte.tile_size; y++)
                {
                    addToScreen(carte.level[lvl].getBlock(x, y), x * carte.tile_size, y * carte.tile_size);
                    //addToScreen(carte.background.tiles[carte.tile_type[lvl,x, y]], (x * carte.tile_size), (y * carte.tile_size));
                }
            }
        }

        /// <summary>
        /// Wipe the portal (last_position)
        /// </summary>
        /// /// <param name="_blue">True : wipe the blue portal | False : wipe the orange portal</param>
        private void wipePortal(bool _blue)
        {
            if (_blue)
            {
                for (int x = portal_blue.pos_X / carte.tile_size; x <= portal_blue.posBorder_X / carte.tile_size; x++)
                {
                    for (int y = portal_blue.pos_Y / carte.tile_size; y <= portal_blue.posBorder_Y / carte.tile_size; y++)
                    {
                        addToScreen(carte.level[lvl].getBlock(x, y), x * carte.tile_size, y * carte.tile_size);
                        //addToScreen(carte.background.tiles[carte.tile_type[lvl,x, y]], (x * carte.tile_size), (y * carte.tile_size));
                    }
                }
            }
            else
            {
                for (int x = portal_orange.pos_X / carte.tile_size; x <= portal_orange.posBorder_X / carte.tile_size; x++)
                {
                    for (int y = portal_orange.pos_Y / carte.tile_size; y <= portal_orange.posBorder_Y / carte.tile_size; y++)
                    {
                        addToScreen(carte.level[lvl].getBlock(x, y), x * carte.tile_size, y * carte.tile_size);
                        //addToScreen(carte.background.tiles[carte.tile_type[lvl,x, y]], (x * carte.tile_size), (y * carte.tile_size));
                    }
                }
            }
        }

        /// <summary>
        /// Add a bitmap to the screen
        /// Re-affect forces
        /// </summary>
        /// <param name="_tp">True : Bitmap to be added to the screen</param>
        /// <param name="_x">Starting/Left position in X of the Bitmap</param>
        /// <param name="_y">Starting/top position in Y of the Bitmap</param>
        private void addToScreen(Bitmap _tp, int _x, int _y)
        {
            if (!reverse)
            {
                for (int y = 0; y < _tp.Height; y++)
                {
                    for (int x = 0; x < _tp.Width; x++)
                    {
                        screen.SetPixel(x + _x, y + _y, _tp.GetPixel(x, y));
                    }
                }
            }
            else
            {
                for (int y = 0; y < _tp.Height; y++)
                {
                    for (int x = 0; x < _tp.Width; x++)
                    {
                        screen.SetPixel(639 - (x + _x), 359 - (y + _y), _tp.GetPixel(x, y));
                    }
                }
            }
        }
        #endregion

        #region Intput / Output
        public void KeyUp(KeyEventArgs e)
        {
            if (!begining && !end)
            {
                if (e.KeyCode == Keys.W) { KeyPressed_UP = true; }
                if (e.KeyCode == Keys.D) { KeyPressed_RIGHT = true; }
                if (e.KeyCode == Keys.A) { KeyPressed_LEFT = true; }
                if (e.KeyValue == 82) { resetLevel(); }
            }
        }

        public void KeyDown(KeyEventArgs e)
        {
            if (!begining && !end)
            {
                // Movement controls remain unaffected
                if (e.KeyCode == Keys.W) { KeyPressed_UP = false; }
                if (e.KeyCode == Keys.D) { KeyPressed_RIGHT = false; }
                if (e.KeyCode == Keys.A) { KeyPressed_LEFT = false; }

                // Portal shooting using arrow keys or numeric keypad
                if ((e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left) && !updatePortalPosBool)
                {
                    portalShot_X = -1;
                    portalShot_Y = 1;
                    updatePortalPosBool = true;
                }
                if ((e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.Down) && !updatePortalPosBool)
                {
                    portalShot_Y = 1;
                    updatePortalPosBool = true;
                }
                if ((e.KeyCode == Keys.NumPad3 || e.KeyCode == Keys.Down || e.KeyCode == Keys.Right) && !updatePortalPosBool)
                {
                    portalShot_X = 1;
                    portalShot_Y = 1;
                    updatePortalPosBool = true;
                }
                if ((e.KeyCode == Keys.NumPad4 || e.KeyCode == Keys.Left) && !updatePortalPosBool)
                {
                    portalShot_X = -1;
                    updatePortalPosBool = true;
                }
                if (e.KeyCode == Keys.NumPad5 || e.KeyCode == Keys.LControlKey)
                {
                    blueNotOrange = !blueNotOrange;
                }
                if ((e.KeyCode == Keys.NumPad6 || e.KeyCode == Keys.Right) && !updatePortalPosBool)
                {
                    portalShot_X = 1;
                    updatePortalPosBool = true;
                }
                if ((e.KeyCode == Keys.NumPad7 || e.KeyCode == Keys.Up || e.KeyCode == Keys.Left) && !updatePortalPosBool)
                {
                    portalShot_X = -1;
                    portalShot_Y = -1;
                    updatePortalPosBool = true;
                }
                if ((e.KeyCode == Keys.NumPad8 || e.KeyCode == Keys.Up) && !updatePortalPosBool)
                {
                    portalShot_Y = -1;
                    updatePortalPosBool = true;
                }
                if ((e.KeyCode == Keys.NumPad9 || e.KeyCode == Keys.Up || e.KeyCode == Keys.Right) && !updatePortalPosBool)
                {
                    portalShot_X = 1;
                    portalShot_Y = -1;
                    updatePortalPosBool = true;
                }

                // Potential portal swap action (commented logic retained for reference)
                if (e.KeyCode == Keys.T && !updatePortalPosBool && false)
                {
                    int a = portal_blue.pos_X;
                    int b = portal_blue.pos_Y;
                    string c = portal_blue.orientation;
                    portal_blue.pos_X = portal_orange.pos_X;
                    portal_blue.pos_Y = portal_orange.pos_Y;
                    portal_blue.orientation = portal_orange.orientation;
                    portal_orange.pos_X = a;
                    portal_orange.pos_Y = b;
                    portal_orange.orientation = c;
                }
            }            
        }

        private void getHz() { }
        public int getAccX() { return chell.acceleration_X; }
        public int getSpeedX() { return chell.last_speed_X; }
        public int getPosX() { return chell.pos_X; }
        public int getFricX() { return f_frottement_X; }
        public int getExtX() { return f_ext_X; }
        public int getAccY() { return chell.acceleration_Y; }
        public int getSpeedY() { return chell.speed_Y; }
        public int getPosY() { return chell.pos_Y; }
        public int getFricY() { return f_frottement_Y; }
        public int getExtY() { return f_ext_Y; }
        public int getG() { return G; }
        public int getP() { return f_poids; }
        //public void setG(int _g) { G = Math.Max(Math.Min(_g, 100), 25); }
        #endregion
    }
}
