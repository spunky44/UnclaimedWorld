using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.ClientSide.Interface.Tasks
{
    public class JobsMessage
    {
        public string Text;

        private double timeElapsed = 0;


        public void Update(GameTime gameTime)
        {
            timeElapsed += gameTime.ElapsedGameTime.TotalSeconds;

            /*  if (timeElapsed > The.Client.ScreenManager.UserSettings.AlertLifetime)
              {
                  Hide();

              }*/
        }

        public bool IsExpired()
        {
            return timeElapsed > 5f; // The.Client.Controller.Options.AlertLifetime;
        }
    }
}
