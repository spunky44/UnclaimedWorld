using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UWGame.Control.Replays
{
    public class ReplayVerificationData
    {
        public Vector3 RepresentativeEntityLocation;

        public Vector2 MapWindowLocation;

        public void Load(BinaryFileReader replayReader)
        {
            RepresentativeEntityLocation.X = replayReader.ReadFloat();
            RepresentativeEntityLocation.Y = replayReader.ReadFloat();

            MapWindowLocation.X = replayReader.ReadFloat();
            MapWindowLocation.Y = replayReader.ReadFloat();
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(RepresentativeEntityLocation.X);
            writer.Write(RepresentativeEntityLocation.Y);

            writer.Write(MapWindowLocation.X);
            writer.Write(MapWindowLocation.Y);
        }

        public bool Verify(ReplayVerificationData compareTo, UWGame.Control.Replays.Replayer.ReplayingMode mode) 
        {
            Vector2 mapWindowLocation = compareTo.MapWindowLocation;
            Vector3 representativeEntityLocation = compareTo.RepresentativeEntityLocation;
            

            if (Common.IsEqual(representativeEntityLocation.X, this.RepresentativeEntityLocation.X) == false)
            {
                return false;
            }
            if (Common.IsEqual(representativeEntityLocation.Y, this.RepresentativeEntityLocation.Y) == false)
            {
                return false;
            }

            if (mode != Replayer.ReplayingMode.CommandMode)
            {
                if (Common.IsEqual(mapWindowLocation.X, this.MapWindowLocation.X) == false)
                {
                    return false;
                }
                if (Common.IsEqual(mapWindowLocation.Y, this.MapWindowLocation.Y) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
