using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide.Renderables
{
    public class Effect<T>: IUpdatable
    {
        List<Envelope<T>> activeEnvelopes = new List<Envelope<T>>();

        private T defaultElement;


        private Renderable parent;

        public Effect(Renderable parent, T elementAsDefault)
        {
            this.parent = parent;

            // TODO: Complete member initialization
            this.defaultElement = elementAsDefault;
            
        }

        public void SetDefaultValue(T element)
        {
            defaultElement = element;
        }

        public int GetNumberOfEnvelopes()
        {
            return activeEnvelopes.Count;
        }
        /// <summary>
        /// Returns the newest activeenvelope value if there is any, else it returns the default value
        /// </summary>
        /// <returns></returns>
        public T GetValue()
        {

            //As it is setup now it will return the latest added envelope if there exists one
            //Else it will return the default element that the effect uses in all other cases
            if (activeEnvelopes.Count > 0)
            {
                return activeEnvelopes[activeEnvelopes.Count - 1].GetValue();
            }
            else
            {
                return defaultElement;
            }
        }

        /// <summary>
        /// Adds a new element that will be returned when GetValue is called on Effect.cs for the duration of time or untill a newer element has been added.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="element"></param>
        public void AddNewEnvelope(float duration, T element)
        {
            Envelope<T> envelope = new Envelope<T>(duration, element);
            activeEnvelopes.Add(envelope);
        }

        /// <summary>
        /// Calls update on all current active envelopes and removes them if they return false.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime) //, out bool requiresUpdate)
        {
            //requiresUpdate = false;

            for (int i = activeEnvelopes.Count - 1; i >= 0; i--)
            {
              //  requiresUpdate = true;

                if (activeEnvelopes[i].Update(gameTime) == false)
                {
                    activeEnvelopes.RemoveAt(i);
                }
            }
           
        }

       

        public double? GetUpdateInterval()
        {
            if (activeEnvelopes.Count > 0)
            {
                return 0;
            }

            // check the default player:
            
            if (typeof(T) == typeof(Animation2DPlayer)) // probably not best practice in a generic method to do this..
            {
                if (parent.IsOnScreen // this should cull Updates to off-screen renderables...
                    && ((Animation2DPlayer)((object)defaultElement)).RequiresUpdate) // for resource overlays, this will call Update on 30.000 renderables every frame, just to set the isDirtyFlag that gets the Tint for the few on-screen resources...
                {
                    // reset the flag if needed:
                    if (!The.MapUI.TileIsOnScreen(parent.MapPosition.X, parent.MapPosition.Y, GameWorldRenderer.GutterSize))
                    {
                        parent.IsOnScreen = false;

                        return null; // go back to sleep. The.Client will wake us when we are on screen again.
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            return null;
        }
    }
}
