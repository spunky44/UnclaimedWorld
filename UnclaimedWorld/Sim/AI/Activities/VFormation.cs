using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
namespace UWGame.SimSide.AI.Activities
{
    public class VFormation: Formation
    {
        private static Vector2 vLine = new Vector2(-0.4f, -1f); //.Normalize();

        /// <summary>
        /// this can change so more members can be packed into the same space. Or spread out in an open field...
        /// </summary>
        private float memberSpacing = 256f; //48f;

         // Static constructor
        static VFormation()
        {
            vLine.Normalize();
        }


        public override int MaxMembers
        {
            get { return -1; }
        }

        public override Vector2 ComputePosition()
        {
          //  return new Vector2(0f, -100f);

            // place up/down?
            int yDirection = 1 - (FormationPositions.Count % 2) * 2;

            return memberSpacing * (FormationPositions.Count / 2 + 1) * new Vector2(vLine.X, yDirection * vLine.Y);
                
        }

        /*
        public int MaxMembers;
        private static LooseGroupFormation instance;
        private LooseGroupFormation()
        {
          
        }

        public static LooseGroupFormation Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    instance = new LooseGroupFormation();
                    return instance;
                }
            }
        }*/

    }
}
