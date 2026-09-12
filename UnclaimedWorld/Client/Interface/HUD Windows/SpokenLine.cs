using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;

namespace UWGame.ClientSide.Interface.HUD_Windows
{
   
    public class SpokenLine : HUDWindow
    {

        public EntityID? Speaker;
        private TextArea textArea;
        public MarkerWindow Parent;
    //    private float timeLeft = 4f;


        public string Text {
            set
            {
                textArea.Text = value;
                //textArea.FitToText();

                textArea.X = sideMargin;
                // lblExpedition.Text = "CAMP CENTER";
                //textArea.FitToText();
                DisplayWindow.Width = textArea.Width + 2 * sideMargin;// correctedSideMargin;
                DisplayWindow.Height = textArea.Height;
                DisplayWindow.CenterChildVertically(textArea);
            }
        }

        public void Init(Entity speaker, /*float timeLeft,*/ string text, MarkerWindow parent)
        {
            textArea.Width = DisplayWindow.Width;

            Parent = parent;
            Parent.ShowWhileLineIsSpoken = true;
            Speaker = speaker.EntityID;
           // this.timeLeft = timeLeft;
            speaker.Intelligence.TalkActionEnded += new Action(Intelligence_TalkActionEnded);

            string name = "";
            string division = "";
            if (speaker.EntityType.Person != null)
            {
                //name = speaker.PersonEntity.ShortName;
                /*name = speaker.PersonEntity.LastName;

                division = ": ";*/
            }
            else
            {
                /*name = speaker.Name;

                division = ": ";*/
            }

            Text = name + division + text;
        }

        void Intelligence_TalkActionEnded()
        {
            // hide the speech bubble:
            The.InGameUI.RemoveSpokenLine(this);
        }

        public SpokenLine(/*EntityID speaker*/)
            : base(120, 24, true, false, false, "HUD_windowCharacter_base")
        {

           // this.Speaker = speaker;


            textArea = new TextArea(gui, ListBoxType.HUDAndLCD);
            Add(textArea);
            textArea.Init(Label.LabelType.HUDWindow);  //TextArea.TextAreaType.HUD);//.Init(Label.LabelType.HUDWindow);                      

        }

     

        public void Show()
        {
            Entity speakerEntity;
            Point? screenPos = UpdatePosition(out speakerEntity);

            if (screenPos.HasValue)
            {
                base.ShowOnPlayfield(screenPos.Value.X, screenPos.Value.Y);
            }
            else
            {
              //  Hide();

                The.InGameUI.RemoveSpokenLine(this);
            }

           /* base.ShowOnPlayfield((int)screenPos.X - MapManager.tileSizeOver2,
                        (int)screenPos.Y + MapManager.tileSizeOver2);
            */
        }

        private Point? UpdatePosition(out Entity speakingEntity)
        {
            Point? screenPos = null;
            speakingEntity = null;

            IKnownEntityData data;
            EntityResult result = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(Speaker.Value, out data);

            if (result == EntityResult.SeenDirectly)
            {
                speakingEntity = (Entity)data;

                if (speakingEntity.Location == null)
                {
                    return null;
                }

                //Vector2 location;
                /*if (speakingEntity.Renderable.RenderAsModel != null)
                {
                    location = speakingEntity.Renderable.RenderAsModel.Location.ToVector2();
                }
                else
                {
                    location = speakingEntity.Renderable.Location.ToVector2();
                }*/
                //location = Parent.GetScreenPosition();//new Vector2(Parent.DisplayWindow.Right, Parent.DisplayWindow.Y);
                Point pos = Parent.GetScreenPosition();//location.ToPoint();//The.MapUI.WorldPosToScreenPoint(location); // The.MapUI.TilePosToScreen(new Point(tile.X, tile.Y));
                pos.X += Parent.DisplayWindow.Width;

                //pos.Y -= 50;

                screenPos = pos;
            }

            return screenPos;
        }


        public override void Update(GameTime elapsed)
        {
            base.Update(elapsed);

            if (Speaker.HasValue)
            {
                Entity speakingEntity;
                Point? screenPos = UpdatePosition(out speakingEntity);

                if (speakingEntity != null && screenPos.HasValue) // is the speaker still seen?
                {
                    SetScreenPosition(new Point(screenPos.Value.X, screenPos.Value.Y));
                    SetWorldPosition(new Point(screenPos.Value.X, screenPos.Value.Y));
                }
                else
                {
                    The.InGameUI.RemoveSpokenLine(this);
                }
            }
        }
       
        public override void Hide()
        {
            if (Speaker == null)//As long as we still have a speaker we should be shown
            {
                base.Hide();
            }
        }
    }
}
