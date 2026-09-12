using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// does not create an expedition, merely moves its location.
    /// </summary>
    public class PlaceExpedition : Control.Commands.Command
    {
        public Vector3 Location; 

        public bool GiveClientFeedback;

        public long Expedition; 

        public PlaceExpedition()
        { 
        }

        public PlaceExpedition(ExpeditionID expedition, Vector3 location, bool giveClientFeedback)
        {
            this.Expedition = (long)expedition;
            this.Location = location;
            this.GiveClientFeedback = giveClientFeedback;
        }

        public override void Execute(bool giveClientFeedback)
        {
            bool placeExpeditionSuccesful = DoPlaceExpedition();

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (placeExpeditionSuccesful)
                {
                    The.Client.OnPlaceExpedition();
                }
            }   
        }


        private bool DoPlaceExpedition()
        {
            if (!The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtile(Location)))
            {
                The.Sim.PlaySite.GetFirstPlayerExpedition().Center = Location;
                The.Sim.PlaySite.GetFirstPlayerExpedition().RemoveGatheringSite();

                return true;
            }

            return false;
        }
    }
}
