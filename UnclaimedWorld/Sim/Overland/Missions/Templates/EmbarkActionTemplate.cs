using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.XmlCollections;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.Overland.Missions.Templates
{
   /// <summary>
   /// use this for no vehicle case too...
   /// </summary>
    public class EmbarkActionTemplate : MissionActionTemplate
    {

        public PassengerListTemplate PassengerListTemplate;

        public override string Name
        {
            get { return TemplateName; }
        }

        public static string TemplateName
        {
            get
            {
                return "Embark";
            }
        }

        public EmbarkActionTemplate() 
        { 
            // needed for XmlSerializer 
        }

        public EmbarkActionTemplate(MissionStopTemplate missionStopTemplate, List<EntityID> passengers, bool allowDeleting)
            : base(missionStopTemplate, allowDeleting)
        {

            PassengerListTemplate = new PassengerListTemplate()
            {
                Passengers = passengers.Select(p => (long)p).ToList(),
                StartingLocation = base.MissionStopTemplate.TravelLocation // needed..?
            };
         
        }

        public static bool ValidateEmbarkTerminal(ref List<string> errors, IKnownEntityData terminalData)
        {
            if (!ValidateWorkingTerminal(terminalData, ref errors))
            {
                return false;
            }


            return true;
        }

        public static bool ValidateEmbark(Allegiance thisAllegiance, IKnownEntityData terminal, Allegiance allegiance, ref List<string> errors)
        {
            if (!ValidateEmbarkTerminal(ref errors, terminal))
            {
                return false;
            }

            if (thisAllegiance == allegiance)
            {
                Common.AddToList(ref errors, "For now, we can only embark passengers at other sites.");
                return false;
            }

            return true;

        }

        public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
        {
            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);            

            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            Site site;
            if (thisAllegiance == null 
                || !MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                return false;
            }
            
            if(!ValidateEmbark(thisAllegiance, terminal, allegiance, ref errors))
            {
                return false;
            }
           

            if (PassengerListTemplate != null)
            {
                if (PassengerListTemplate.Passengers.Count > 0)
                {
                    hasMeaning = true;
                }
            }

            return true;
        }

        public override ActionTypes ActionType
        {
            get { return ActionTypes.Embark; }
        }

        public override MissionAction CreateMissionAction(Mission mission)
        {
            return new EmbarkAction(mission, this);
        }

        public override float ComputeTotalCargoBulk()
        {
            float totalBulk = 0;
            float? bulk;

                     
            if (PassengerListTemplate != null)
            {
                foreach (var item in PassengerListTemplate.Passengers)
                {
                    Entity entity = Entity.FindByID((EntityID)item);
                    if (entity != null)
                    {
                        totalBulk += entity.Bulk;
                    }
                    else
                    {
                        totalBulk += 1f; // hack... handle destroyed passengers somewhere else...
                    }
                }
            }

            return totalBulk;
        }

        public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
        {
            decimal totalPrice = 0;
            decimal? price;
            soldItemsCost = 0;
            boughtItemsCost = 0;

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);


            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            Site site;
            if (!MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                return 0;
            }


            return totalPrice;
        }

        public override void AssignIDs()
        {
            base.AssignIDs();

           // ContractTemplate.AssignIDs();
        }

        public override Snapshots.ISnapshot DoSnapshot(Snapshots.Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.PassengerListTemplate = (PassengerListTemplate)sn.DoISnapshot(PassengerListTemplate);
           

            return this;
        }

        public override void LoadPostProcess(Snapshots.Snapshotter sn)
        {
            base.LoadPostProcess(sn);

           // PassengerListTemplate = LookUp<ContractTemplate, ContractTemplateID>.FindByID(snapshotContractTemplate);

        }
    }
}
