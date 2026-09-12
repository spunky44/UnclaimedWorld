using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.SimSide.Allegiances;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.ClientSide.Log;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.ClientSide.HelpTopics;

namespace UWGame.ClientSide.GameEvents
{

    public class ShowTutorialAction : EventActionType
    {

        public string TutorialPageKey;


        public ShowTutorialAction(string keyName): base(keyName)
        {

        }

        public ShowTutorialAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EntityID? triggeringEntity, EntityID? targetEntity, ref string failReason)
        {
            HelpTopic tutorialPage = GameData.Instance.AllTutorialTopics[this.TutorialPageKey];

            HelpTopicDialog dialog = The.InGameUI.HelpTopicDialogs[tutorialPage];

            dialog.ShowInScreenSpace(0, 200);
           // dialog.DisplayWindow.CenterWindow();


            return true;
           
        }

       

       
    }
}
