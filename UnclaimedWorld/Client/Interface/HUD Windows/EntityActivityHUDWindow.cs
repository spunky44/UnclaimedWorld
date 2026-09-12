using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
    /// <summary>
    /// has invisible background, contains with icons in it.
    /// This is a child window of Marker Window
    /// can be displayed for all agents of the players allegiance to give info about what they are doing and how well.
    /// </summary>
    public class EntityActivityHUDWindow : HUDWindow
    {
        public EntityActivityHUDWindow(Point offsetFromParent,int maxNumberOfIcons)
            : base(80, 24, false)
        {
            OffsetFromParent = offsetFromParent;
            activityIcons = new HorizontalList(The.InGameUI.gui, maxNumberOfIcons);

        }

        public EntityID? owner;
        HorizontalList activityIcons;
        
        private Point ScreenPosition;
        private Point OffsetFromParent;

        public override void Refresh()
        {
            Remove(activityIcons);
            if (owner.HasValue == true)
            {
                Entity entityWithStatus = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownDataAsEntity(owner.Value);

                if (entityWithStatus != null)
                {
                    if(UpdateIcons(entityWithStatus) == true)
                    {
                        Add(activityIcons);
                    }
                }
            }
        }


        public void SetPosition(Point parentScreenPosition)
        {
            ScreenPosition = parentScreenPosition;
            ScreenPosition.X += OffsetFromParent.X;
            ScreenPosition.Y += OffsetFromParent.Y;

        }

        private bool UpdateIcons(Entity entityWithStatus)
        {
            foreach (PresentationTypeCategory presentationTypeCategory in GameData.Instance.CustomEntityActivityData.FinalPresentationTypeCategories)
            {
                int? numberOfItems = null;
                PresentationTypeCategoryProcessor.DisplayCategory(presentationTypeCategory, entityWithStatus, activityIcons, ref numberOfItems); 
            }
            
            DisplayWindow.Width = activityIcons.Width;
            DisplayWindow.CenterChildVertically(activityIcons);
          
            return true;
        }

        public void Show()
        {
            //base.ShowInScreenSpace(ScreenPosition.X, ScreenPosition.Y);
            base.ShowOnPlayfield(ScreenPosition.X, ScreenPosition.Y);
        }

        public void Update()
        {
            //base.ShowInScreenSpace(ScreenPosition.X, ScreenPosition.Y);
            base.ShowOnPlayfield(ScreenPosition.X, ScreenPosition.Y);
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}
