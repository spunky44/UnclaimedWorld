using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Overland;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Actions
{
    public class DetectAction : EventActionType
    {
        public TargetObject TargetObject;
        public string EntityName;
                
       // public AllegianceAndExpedition Detector = null;

        public string DetectorAllegianceKey;
        public EvalNode DynamicDetectorAllegianceKey;


         public DetectAction(string keyName): base(keyName)
        {

        }

         public DetectAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public bool Execute(EventAction eventAction, ref string failReason)
        {

            Entity entity = null;
            if (!EventActionType.GetEntity(EntityName, TargetObject, action, out entity, ref failReason))
            {
                return false;
            }

            Allegiance allegiance;
           // Expedition expedition;
            if (!AllegianceAndExpedition.ResolveAllegiance(DynamicDetectorAllegianceKey, DetectorAllegianceKey, action, out allegiance, ref failReason))
            {
                return false;
            }
          

            allegiance.SharedKnowledge.SeeDetectableIfRelevant(entity, suppressClientFeedback: true);

            Point tile = entity.PlaySiteMapPosition;
            if (!The.Map.GetTile(tile).AllegiancesThatSeeThisTile.Contains(allegiance))
            {
                allegiance.SharedKnowledge.UnSeeEntity(entity); // if in FOW
            }
                                 
            return true;               
            
        }


      
        public override string ToString()
        {
            return "Detect : " + EntityName ?? "";
        }
    }
}
