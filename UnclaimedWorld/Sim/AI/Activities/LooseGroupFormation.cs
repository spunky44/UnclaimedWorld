using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
namespace UWGame.SimSide.AI.Activities
{
    public class LooseGroupFormation: Formation
    {
       
        private float spacingBetweenTurns = 24f / MathHelper.TwoPi;

        private float angleSpacingFactor = MathHelper.PiOver4; // MathHelper.PiOver4; //The REAL value

       // private float spiralStartOffset = MathHelper.PiOver2;

        private double anglePosition = MathHelper.PiOver2;

        public override int MaxMembers
        {
            get { return -1; }
        }

        public override Vector2 ComputePosition()
        {
            //  r = a + b * theta

            float radius = /*FormationPositions.Count **/ (float)anglePosition * spacingBetweenTurns + 48f;
            float dTheta = FormationPositions.Count * angleSpacingFactor;
            //dTheta = Common.ClampBottom(dTheta, MathHelper.PiOver4);

            anglePosition = anglePosition + dTheta;

            return radius * new Vector2((float)Math.Sin(anglePosition), (float)-Math.Cos(anglePosition));
                
                /*(Common.RandomBetween(Globals.Instance.Random, -20f, -10f),
                               Common.RandomBetween(Globals.Instance.Random, -20f, 20f));*/
        }


    }
}
