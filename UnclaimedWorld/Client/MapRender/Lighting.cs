using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using UWGame.ClientSide.Map;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide;
using UWGame.SimSide.Snapshots;
using UWGame.ClientSide.Renderables;

namespace UWGame.ClientSide.Map
{
    /// <summary>
    /// TODO: delete this class
    /// </summary>
    public class Lighting 
    {

        public LightSource[] IndoorLightSources;
        public LightSource[] OutdoorLightSources;


        private Renderable Parent;
        LightingType lightingType;


        public Lighting(LightingType lightingType, Renderable parent) 
        {
            this.Parent = parent;
            this.lightingType = lightingType;
        }

      
       

      /*  public bool FlipHorizontally // = false;
        {
            // get { return flipHorizontally; }
            set
            {

                if (IndoorLightSources != null)
                {
                    foreach (LightSource light in IndoorLightSources)
                    {
                        light.FlipHorizontally = value;
                    }
                }
                if (OutdoorLightSources != null)
                {
                    foreach (LightSource light in OutdoorLightSources)
                    {
                        light.FlipHorizontally = value;
                    }
                }

            }
        }*/

        public void TurnOnDesiredShareOfLights(float fractionToTurnOn)
        {
            if (IndoorLightSources != null)
            {
                int indoorLightsOn = CountLightsOn(IndoorLightSources);

                int desiredIndoorLightsOn = (int)Math.Ceiling((double)(IndoorLightSources.Length * fractionToTurnOn));
                if (desiredIndoorLightsOn > indoorLightsOn)
                {
                    TurnOnLights(desiredIndoorLightsOn - indoorLightsOn, IndoorLightSources);
                }

            }

            if (OutdoorLightSources != null)
            {
                int outdoorLightsOn = CountLightsOn(OutdoorLightSources);

                int desiredOutdoorLightsOn = (int)Math.Ceiling((double)(OutdoorLightSources.Length * fractionToTurnOn));
                if (desiredOutdoorLightsOn > outdoorLightsOn)
                {
                    TurnOnLights(desiredOutdoorLightsOn - outdoorLightsOn, OutdoorLightSources);
                }
            }
        }

        public static void TurnOnLights(int noToTurnOn, LightSource[] list)
        {
            int index = The.Sim.GameplayRandomGenerator.Next(0, list.Length, "Structure"); //pick a random item
            int numberTurnedOn = 0;
            int seenTurnedOn = 0;

            while (numberTurnedOn < noToTurnOn && seenTurnedOn < list.Length)
            {
                while (list[index].LightIsOn == true)
                {
                    seenTurnedOn++;
                    index++;
                    index = index % list.Length;
                }

                list[index].LightIsOn = true;
                numberTurnedOn++;
            }

        }

        public void TurnOffTheLight()
        {
            if (IndoorLightSources != null)
            {
                for (int i = 0; i < IndoorLightSources.Length; i++)
                {
                    IndoorLightSources[i].LightIsOn = false;
                }
            }
            if (OutdoorLightSources != null)
            {
                for (int i = 0; i < OutdoorLightSources.Length; i++)
                {
                    OutdoorLightSources[i].LightIsOn = false;
                }
            }
        }

        public void GetLightSourcesForDrawing(List<LightSource> listOfObjectsToDraw)
        {
            if (IndoorLightSources != null)
            {
                for (int i = 0; i < IndoorLightSources.Length; i++)
                {
                    if (IndoorLightSources[i] != null && IndoorLightSources[i].LightIsOn)
                    {
                        listOfObjectsToDraw.Add(IndoorLightSources[i]);
                    }
                }
            }
            if (OutdoorLightSources != null)
            {
                for (int i = 0; i < OutdoorLightSources.Length; i++)
                {
                    if (OutdoorLightSources[i] != null && OutdoorLightSources[i].LightIsOn)
                    {
                        listOfObjectsToDraw.Add(OutdoorLightSources[i]);
                    }
                }
            }
        }


        public float GetOutdoorLightsShare()
        {
            if (OutdoorLightSources != null)
            {
                return (float)CountLightsOn(OutdoorLightSources) / (float)OutdoorLightSources.Length;
            }

            return -1f;
        }

        public int CountLightsOn(LightSource[] lights)
        {
            int lightsOn = 0;
            for (int i = 0; i < IndoorLightSources.Length; i++)
            {
                if (IndoorLightSources[i].LightIsOn)
                {
                    lightsOn++;
                }
            }
            return lightsOn;
        }

        public float GetIndoorLightsShare()
        {
            if (IndoorLightSources != null)
            {
                return (float)CountLightsOn(IndoorLightSources) / (float)IndoorLightSources.Length;
            }

            return -1f;

            // int noOfPeopleInside = GetNoOfPeopleInside();
            //noOfPeopleInside;

        }

      /*  public void PlaceLightSources(Vector2 baseCenter)
        {
            // put lights in:
            IndoorLightSources = new LightSource[lightingType.IndoorLightSourceTypes.Count];
            OutdoorLightSources = new LightSource[lightingType.OutdoorLightSourceTypes.Count];
            int i = 0;
            
          //  Vector2 baseCenter = Parent.EntityType.TileLayoutType.GetBaseCenter(Parent.FlipHorizontally);

            foreach (LightSourceOffset lightOffset in lightingType.IndoorLightSourceTypes)
            {
                IndoorLightSources[i] = PlaceLightSource(baseCenter, lightOffset);
                i++;
            }

            i = 0;
            foreach (LightSourceOffset lightOffset in lightingType.OutdoorLightSourceTypes)
            {
                OutdoorLightSources[i] = PlaceLightSource(baseCenter, lightOffset);
                i++;
            }
        }*/


      /*  private LightSource PlaceLightSource(Vector2 baseCenterOffset, LightSourceOffset lightOffset)
        {
            LightSource newLightSource = new LightSource(Parent.Location.Value);
            newLightSource.FlipHorizontally = Parent.FlipHorizontally;
            newLightSource.LightSourceType = lightOffset.LightSourceType;

            // turn it on for testing - this should be a Goal...
            newLightSource.LightIsOn = true;


            if (Parent.FlipHorizontally)
            {
                // TODO: fix this when WidthInTiles is deprecated
               // lightOffset.Offset.X = Parent.EntityType.StructureType.WidthInTiles * MapManager.tileSize - lightOffset.Offset.X;
            }

            // add the offsets:
            Vector2 offsetWithRespectToBaseOfCenter;
            offsetWithRespectToBaseOfCenter.X = lightOffset.Offset.X - baseCenterOffset.X;
            offsetWithRespectToBaseOfCenter.Y = lightOffset.Offset.Y - baseCenterOffset.Y;

            Vector2 offset = new Vector2(offsetWithRespectToBaseOfCenter.X - newLightSource.LightSourceType.GetOffset(Parent.FlipHorizontally).X,
                 (offsetWithRespectToBaseOfCenter.Y - newLightSource.LightSourceType.GetOffset(Parent.FlipHorizontally).Y));


            newLightSource.SetupQuadVertices(newLightSource.Location, offset, 20f);

            return newLightSource;
        }*/
    }
}
