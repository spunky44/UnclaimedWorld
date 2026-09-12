using System;
using System.Collections.Generic;
using System.Text;
using WindowSystem;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
namespace UWGame.ClientSide.Interface
{
    public class SidePanelEmpty : RosterPanel
    {
        
        public SidePanelEmpty(): base(The.InGameUI.sidePanelHeight, true)
        {
            HasStatusCRT = false;
         
        }
              

       
        
    }
}
