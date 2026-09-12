using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Interface;
using WindowSystem;
using UWGame.SimSide.Trees;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Jobs;

namespace UWGame.ClientSide.Renderables 
{
    public class RenderAsIcon 
    {
       

        public RenderAsIcon()
        {
            
        }

        private Color color = Color.White; 
        public IconToRender IconToRender = IconToRender.Hook;//Get from RenderAsIconType

        public IconToRender GetIconToRender()
        {
            return IconToRender;
        }
        
       
    
    }
}
