#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
////using Microsoft.Xna.Framework.Storage;
using System.Xml;
using System.Linq;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Jobs;
using Xclna.Xna.Animation;
using WindowSystem;
using GameStateManagement;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Allegiances;
#endregion

namespace UWGame.SimSide.Entities
{

    public class Person : Component, IHasEntityGroup, IOwner //, IHasAllegiance
    {
         /*
        public string FirstName
        {
            get;
            private set;
        }

       
        public string LastName
        {
            get;
            private set;
        }

        public string ShortName
        {
            get;
            private set;
        }
      
        */
       

        /// <summary>
        /// TODO: move this to Static Entity Condition framework
        /// </summary>
        private bool portraitRectIsDirty = true;

        /// <summary>
        /// This is the sprite rectangle we use for the portrait on the status screen (upper right)
        /// </summary>
        public Rectangle portraitRectStatus;
   

        /// <summary>
        /// This is the cropped sprite rectangle we use for the portrait on the entity panel
        /// </summary>
        private Rectangle portraitRectEntityPanel;

        public int PortraitFlavour = 1;
 
        public Personality Personality;

        private decimal? tradeCredits = 0;
        public decimal? TradeCredits
        {
            get { return tradeCredits; }
            set { tradeCredits = value; }
        }

        /*
        public void SetName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;

            string totalName;
            if (lastName != null)
            {
                totalName = FirstName + " " + LastName;
            }
            else
            {
                totalName = FirstName;
            }

            totalName = totalName.Trim();
            Parent.Name = totalName;
        }
       */

        /// <summary>
        /// Don't keep references to this! It can merge or split. Keep a reference to the entity instead.
        /// </summary>
        public Household Household;
        HouseholdID? snapshotHousehold;


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        #region IHasEntityGroup

        public Allegiances.Allegiance Allegiance
        {
            get
            {           
                return Parent.Intelligence.Allegiance; 
            }
        
        }

        private EntityGroup ownedEntities;
        EntityGroupID snapshotOwnedEntities; 
        public EntityGroup OwnedEntities
        {
            get
            {
                return ownedEntities;
            }
        }

      /*  public Vector3 Location
        {
            get { return Parent.PlaySiteLocation; }
        }*/

        public Vector3? Location
        {
            get { return Parent.Location; }
        }

        public bool IsEatable(EntityType entityType)
        {   
            if (Parent.EntityType.BiologicalType != null)
            {
                BiologicalEntity bioEntity;
                Parent.Find(out bioEntity);
                if (bioEntity.IsEatable(entityType)) 
                {
                    return true;
                }
            }

            return false;
        }

        public int NoOfWorkers
        {
            get
            {
                return 1;
            }
        }


        #endregion

           
        
      /*  public PersonType.HairColors HairColor;
        public PersonType.SkinColors SkinColor;
        */

       
       

        public Rectangle GetPortraitForTalkDisplay(GUIManager gui)
        {
            Rectangle rect = GetPortraitForStatusDisplay(gui);

            rect.Inflate(-6, -6);

            return rect;
        }

        public Rectangle GetPortraitForStatusDisplay(GUIManager gui)
        {
            if (portraitRectIsDirty)
            {
                UpdatePortrait(gui);
            }

            return portraitRectStatus;
        }

        /// <summary>
        /// TEASER HACK
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="portrait"></param>
        public void UpdatePortrait(GUIManager gui, string portrait)
        {
            portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle(portrait);

            portraitRectEntityPanel = portraitRectStatus;
            portraitRectEntityPanel.X = portraitRectStatus.X + 95;//portraitRectStatus.Width / 3;
            portraitRectEntityPanel.Width = portraitRectStatus.Width - 106;

            portraitRectIsDirty = false;
        }

        public string ComposePortraitKey()
        {

           string portraitName = string.Format("human_{0}_{1}_{2}_{3}",
                MapRaceToPortraitRaces(Parent.BiologicalEntity.RaceType),
                MapCasteToPortraitSex(Parent.BiologicalEntity.CasteType),
                MapAgeGroupToPortraitAge(Parent.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup),
                PortraitFlavour);

           return portraitName;
        }

        private void UpdatePortrait(GUIManager gui)
        {
            /*
             * Naming convention:

                human_race_sex_age_flavour   race: w,b,a,h   sex: m,f 
                human_b_m_adult_1
                */
          
            if (!GetPortraitRectangle(gui))
            {
                PortraitFlavour = 1; // reset to default and try again:
                if (!GetPortraitRectangle(gui))
                {
                    GetDefaultPortrait(gui);
                }              
            }           

            portraitRectEntityPanel = portraitRectStatus;
            portraitRectEntityPanel.X = portraitRectStatus.X + 95;//portraitRectStatus.Width / 3;
            portraitRectEntityPanel.Width = portraitRectStatus.Width - 106;

            portraitRectIsDirty = false;
        }

        private bool GetPortraitRectangle(GUIManager gui)
        {
            string portraitName = ComposePortraitKey();

            if (gui.GUI_CRT_SpriteSheet.spriteNames.ContainsKey(portraitName))
            {
                portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle(portraitName);

                return true;
            }

            return false;
        }

        private void GetDefaultPortrait(GUIManager gui)
        {
            string sex = MapCasteToPortraitSex(Parent.BiologicalEntity.CasteType);
            if (sex == "m")
            {
                portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("human_w_m_adult_1");
            }
            else
            {
                portraitRectStatus = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("human_b_f_adult_1");
            }
        }

        public Rectangle GetPortraitForEntityPanel(GUIManager gui)
        {
            if (portraitRectIsDirty)
            {
                UpdatePortrait(gui);
            }

            return portraitRectEntityPanel;
        }

        public Person(Entity parent)
            : base(parent, GameData.Instance.Constants.UpdateIntervalForEntityComponents)
        {                   
           
            ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();
            ((ILookUp<IOwner, OwnerID>)this).AddToLookup();

           /* FoodExtraction foodExtractionInfo = null;
            if (Parent.EntityType.BiologicalType != null)
            {
                foodExtractionInfo = parent.BiologicalEntity.FoodExtraction;               
            }*/

            ownedEntities = new EntityGroup(this, true, true); 
            
            ownedEntities.HaulingJobManager = new HaulingJobManager(OwnedEntities);

            ownedEntities.OtherJobManager = new OtherJobManager(OwnedEntities);


           /* Biological.BiologicalEntity bioEntity;
            if (Parent.Find(out bioEntity))
            {
                bioEntity.FoodProcessesChanged += new BiologicalEntity.FoodProcessesChangedHandler(bioEntity_FoodProcessesChanged);
            }*/
        }

        public void NotifyFoodProcessesChanged()        
        {
            if (Household != null)
            {
                Household.FoodExtraction.SetIsDirty();
            }
        }


        public Person()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");

            
        }
        

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
            ownerID = sn.DoEnum(ownerID);
            
          
            snapshotHousehold = sn.SnapshotID<Household, HouseholdID>(Household);
            snapshotOwnedEntities = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(ownedEntities);
            
            //this.PersonalityType = sn.DoGameData(Personality.PersonalityType);
            /*
            this.FirstName = sn.DoString(FirstName);
            this.LastName = sn.DoString(LastName);
            this.ShortName = sn.DoString(ShortName);
             * */

            this.Personality = (Personality)sn.DoISnapshot(Personality);
            this.tradeCredits = sn.DoDecimalNullable(tradeCredits);

            // TODO: move these to new Static Info when done
            this.portraitRectEntityPanel = sn.DoRectangle(portraitRectEntityPanel);
            this.portraitRectIsDirty = sn.DoBool(portraitRectIsDirty);
            this.portraitRectStatus = sn.DoRectangle(portraitRectStatus);
            this.PortraitFlavour = sn.DoInt32(PortraitFlavour);

        
            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            /*
            if (snapshotExpedition.HasValue)
                CurrentExpedition = Expedition.FindByID(snapshotExpedition.Value);
            */

            if (snapshotHousehold.HasValue)
                Household = LookUp<Household, HouseholdID>.FindByID(snapshotHousehold.Value);

          
            ownedEntities = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnedEntities);

            Personality.Parent = Parent;
            
            Personality.LoadPostProcess(sn);

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public void Initialize()
        {
           // ShortName = FirstName[0] + ". " + LastName;


            if (Personality == null)
            {
                // if not set from data, choose a random personality...
                PersonalityType type = Common.GetRandomListMember(GameData.Instance.AllPersonalityTypes.Values.ToList(), The.Sim.GameplayRandomGenerator);
                Personality = new Personality(base.Parent, type); //GameData.Instance.AllPersonalityTypes.First().Value);      
         
            }


            Personality.Initialize();
        }

        public bool CanDrive()
        {
            AIAgeGroup aiAgeGroup = Parent.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup;
            
            return aiAgeGroup != AIAgeGroup.Child &&
                aiAgeGroup != AIAgeGroup.Baby && aiAgeGroup != AIAgeGroup.YoungAdult;

        }

        public override double? GetUpdateInterval()
        {
           /* if (Parent.Intelligence.Allegiance.UsesDecisionPoints)
            {*/
                return GameData.Instance.Constants.UpdateIntervalForEntityComponents;
           // }
            
           // return null;
        }

        //protected override void UpdateRegulated(double? timeSinceLastUpdate)
        protected override void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
        {
            base.UpdatePlaySiteRegulated(timeSinceLastUpdate);

            Personality.Update(timeSinceLastUpdate);
        }

        private string MapRaceToPortraitRaces(RaceType raceType)
        //mp jan 2016 this links the PortraitSkinType defined in creatureloader ...PortraitSkinType="whitePortrait", under new RaceType()..
            // with the portrait image names. (if there's no match, the default portrait is 'w')
        {
            //if (!string.isnul)
            switch (raceType.PortraitSkinType) //.Name)
            {
                case "whitePortrait":
                    return "w"; //search the file folder for:  human_w_m_adult_1
                case "hispanicPortrait":
                    return "h";
                case "blackPortrait":
                    return "b";
                case "asianPortrait":
                    return "a";
            }
            return "w";

        }

        private string MapCasteToPortraitSex(CasteType casteType)
        {
            switch (casteType.Reproduction)
            {
                case Reproduction.Male:
                    return "m";
                case Reproduction.Female:
                    return "f";
                default:
                    return "m";
            }
        }

        /// <summary>
        /// move this to static conditions - same as for trees
        /// </summary>
        /// <param name="ageGroup"></param>
        /// <returns></returns>
        private string MapAgeGroupToPortraitAge(AIAgeGroup ageGroup)
        {
            switch (ageGroup)
            {
                case AIAgeGroup.Baby:
                    return "baby";
                case AIAgeGroup.Child:
                    return "child";
                case AIAgeGroup.YoungAdult:
                    return "youngadult";
                case AIAgeGroup.Adult:
                    return "adult";
                case AIAgeGroup.Old:
                    return "old";
                default: return "adult";
               
            }

            
        }

        public void UpdateAgeGroup()
        {
            portraitRectIsDirty = true;            

        }

        private void AddHeirIfAlive(List<Entity> listOfHeirs, Entity potentialHeir)
        {
            if (EntityExistsAndAlive(potentialHeir))
            {
                if (!listOfHeirs.Contains(potentialHeir)){
                    listOfHeirs.Add(potentialHeir);
                }
            }
        }

        public bool IsHeadOfHousehold()
        {
            return Household.HeadOfHousehold1 == Parent || Household.HeadOfHousehold2 == Parent;
        }

        public bool IsChild()
        {           
            BiologicalEntity bioEntity = Parent.BiologicalEntity;
            AIAgeGroup aiAgeGroup = bioEntity.AgeGroup.AgeGroupType.AIAgeGroup;

            return aiAgeGroup == AIAgeGroup.Baby || aiAgeGroup == AIAgeGroup.Child
                || aiAgeGroup == AIAgeGroup.YoungAdult;
        }

        private static bool EntityExistsAndAlive(Entity entity)
        {
            if (entity != null)
            {
                return !entity.IsDead; // && (entity.PersonEntity == null || entity.PersonEntity.IsInColony);
            }
            else return false;
        }

        /// <summary>
        /// When there is no spouse, only blood relatives are considered, but adopted children are treated as blood relatives.
        /// </summary>
        /// <returns></returns>
        public List<Entity> GetHeirs()
        {
            Entity thisEntity = Parent;
            Entity BiologicalMother = Parent.BiologicalEntity.BiologicalMother;
            Entity BiologicalFather = Parent.BiologicalEntity.BiologicalFather;

            BiologicalEntity bioEntity = Parent.BiologicalEntity;
            List<Entity> listOfHeirs = new List<Entity>();
            if (EntityExistsAndAlive(bioEntity.Mate))
            {
                listOfHeirs.Add(bioEntity.Mate);
            }
            else if (bioEntity.BiologicalChildren.Count > 0 || (IsHeadOfHousehold() && Household.NoOfChildren() > 0)) // children in household are considered 'adopted' children
            {
                foreach (Entity child in bioEntity.BiologicalChildren)
                {
                    AddHeirIfAlive(listOfHeirs, child);                    
                }

                if (IsHeadOfHousehold())
                {
                    Household.IterateMembers(member =>
                    {
                        if (member.PersonEntity.IsChild() && member != thisEntity)
                        {
                            AddHeirIfAlive(listOfHeirs, member);
                        }
                    });

                   /* foreach (Entity member in Household.Members) // include other (adopted?) children in household...
                    {
                        if (member.PersonEntity.IsChild() && member != thisEntity)
                        {
                            AddHeirIfAlive(listOfHeirs, member);
                        }
                    }*/
                }
            }
            else if (EntityExistsAndAlive(BiologicalMother) || EntityExistsAndAlive(BiologicalFather))
            {
                AddHeirIfAlive(listOfHeirs, BiologicalMother);
                AddHeirIfAlive(listOfHeirs, BiologicalFather);                 
            }
            else if ((BiologicalMother != null && BiologicalMother.BiologicalEntity.BiologicalChildren.Count > 0) ||
                    (BiologicalFather != null && BiologicalFather.BiologicalEntity.BiologicalChildren.Count > 0)) // only biological siblings!
            {
                foreach (Entity child in BiologicalMother.BiologicalEntity.BiologicalChildren)
                {
                    if (child != thisEntity)
                    {
                        AddHeirIfAlive(listOfHeirs, child);  
                    }
                }
                foreach (Entity child in BiologicalFather.BiologicalEntity.BiologicalChildren) 
                {
                    if (child != thisEntity)
                    {
                        AddHeirIfAlive(listOfHeirs, child);  
                    }
                }
            }
            else if (HasLiveGrandChildren()) // grandchildren
            {
                foreach (Entity child in thisEntity.BiologicalEntity.BiologicalChildren)
                {
                    foreach (Entity grandChild in child.BiologicalEntity.BiologicalChildren)
                    {
                        AddHeirIfAlive(listOfHeirs, grandChild); 
                    }
                }
            }
            else if (HasLiveGrandParents()) // grandparents
            {
                if (BiologicalMother != null)
                {
                    AddHeirIfAlive(listOfHeirs, BiologicalMother.BiologicalEntity.BiologicalMother);
                    AddHeirIfAlive(listOfHeirs, BiologicalMother.BiologicalEntity.BiologicalFather); 
                }
                if (BiologicalFather != null)
                {
                    AddHeirIfAlive(listOfHeirs, BiologicalFather.BiologicalEntity.BiologicalMother);
                    AddHeirIfAlive(listOfHeirs, BiologicalFather.BiologicalEntity.BiologicalFather);
                }
            }
            else if (HasLiveNiecesOrNephews()) // Nieces/Nephews 
            {
                if (BiologicalMother != null)
                {
                    foreach (Entity sibling in BiologicalMother.BiologicalEntity.BiologicalChildren)
                    {
                        if (sibling != thisEntity)
                        {
                            foreach (Entity nephewOrNiece in sibling.BiologicalEntity.BiologicalChildren)
                            {
                                AddHeirIfAlive(listOfHeirs, nephewOrNiece);  
                            }
                        }
                    }
                }

                if (BiologicalFather != null)
                {
                    foreach (Entity sibling in BiologicalFather.BiologicalEntity.BiologicalChildren)
                    {
                        if (sibling != thisEntity)
                        {
                            foreach (Entity nephewOrNiece in sibling.BiologicalEntity.BiologicalChildren)
                            {
                                AddHeirIfAlive(listOfHeirs, nephewOrNiece);  
                            }
                        }
                    }
                }
            }

            return listOfHeirs;
        }

      /*  public List<Entity> GetSiblings(List<Entity> emptyList)
        {


        }*/

        private bool HasLiveGrandParents()
        {
            Entity BiologicalMother = Parent.BiologicalEntity.BiologicalMother;
            Entity BiologicalFather = Parent.BiologicalEntity.BiologicalFather;

            return 
                (BiologicalMother != null && EntityExistsAndAlive(BiologicalMother.BiologicalEntity.BiologicalMother)) ||
                (BiologicalMother != null && EntityExistsAndAlive(BiologicalMother.BiologicalEntity.BiologicalFather)) ||
                (BiologicalFather != null && EntityExistsAndAlive(BiologicalFather.BiologicalEntity.BiologicalMother)) ||
                (BiologicalMother != null && EntityExistsAndAlive(BiologicalFather.BiologicalEntity.BiologicalFather));
        }

        private bool HasLiveNiecesOrNephews()
        {
            Entity BiologicalMother = Parent.BiologicalEntity.BiologicalMother;
            Entity BiologicalFather = Parent.BiologicalEntity.BiologicalFather;
            Entity thisEntity = Parent;

            if (BiologicalMother != null) 
            {
                foreach (Entity sibling in BiologicalMother.BiologicalEntity.BiologicalChildren)
                {
                    if (sibling != thisEntity)
                    {
                        foreach (Entity nephewOrNiece in sibling.BiologicalEntity.BiologicalChildren)
                        {
                            if (EntityExistsAndAlive(nephewOrNiece))
                            {
                                return true;
                            }
                        }
                       
                    }
                }                  
            }
            else if (BiologicalFather != null)
            {
                foreach (Entity sibling in BiologicalFather.BiologicalEntity.BiologicalChildren)
                {
                    if (sibling != thisEntity)
                    {
                        foreach (Entity nephewOrNiece in sibling.BiologicalEntity.BiologicalChildren)
                        {
                            if (EntityExistsAndAlive(nephewOrNiece))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool HasLiveGrandChildren()
        {
            foreach (Entity child in Parent.BiologicalEntity.BiologicalChildren)
            {
                foreach (Entity grandChild in child.BiologicalEntity.BiologicalChildren)
                {
                    if (EntityExistsAndAlive(grandChild))
                    {
                        return true;
                    }
                }    
            }

            return false;
        }


     

        public void Destroy()
        {
            DistributeBelongingsOnDeath();

            ownedEntities.Destroy();

            ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();
            ((ILookUp<IOwner, OwnerID>)this).RemoveIDEntry();

            if (Household != null)
            {
                Household.RemoveMember(Parent); // will trigger destruction of the household if no more members
            }

            /*
            if (CurrentExpedition != null)
            {
                CurrentExpedition.RemoveMember(Parent);
            }          
           */
        }

        private void DistributeBelongingsOnDeath()
        {
            // divide possessions:
            // get heirs           

            List<Entity> heirs;
            List<IOwner> owners;
            GetHeirsAsOwners(out heirs, out owners);

            // we can change ownership of non-existing entities, but only if the knowledge about their status is the same among the owners..

            DivideItems(ownedEntities, owners);            
        }

        public void GetHeirsAsOwners(out List<Entity> heirs, out List<IOwner> owners)
        {
            heirs = GetHeirs();
            owners = new List<IOwner>();
            if (heirs.Count > 0)
            {
                foreach (Entity heir in heirs)
                {   // the items go to the new owners' private stash:
                    owners.Add(heir.PersonEntity);
                }
            }
            else
            {
                // belongings go to the state...               
                if (Parent.Intelligence.CurrentExpedition != null)
                {
                    owners.Add(Parent.Intelligence.CurrentExpedition);
                }
            }
        }

        /// <summary>
        /// Divide private? items among heirs - there must be 1 or more new owners.
        /// </summary>
        /// <param name="itemsToDivide"></param>
        /// <param name="newOwners"></param>
        public static void DivideItems(/*Dictionary<EntityType, List<EntityID>> itemsToDivide,*/ EntityGroup itemsToDivide, List<IOwner> newOwners)
        {
            // TODO: consider value of items...

              int noOfPortions = newOwners.Count;
              // use round-robin method to divide items
              int currentTurn = The.Sim.GameplayRandomGenerator.Next(0, noOfPortions, "DivideItems");

              itemsToDivide.IterateEntities(e =>
                  {
                      if (e.PartOf == null)
                      {
                          e.ChangeOwnership(newOwners[currentTurn], Entity.GiveNewOwnerKnowledge.Yes);
                          currentTurn++;
                          currentTurn = currentTurn % noOfPortions;
                      }
                  });            
        }

        /*     public void DecidePlaceToEat()
             {
                 if (Household.Home != null)
                 {
                     if (Household.CookingJob.Count > 0)
                     {
                         ((CookingJob)Household.CookingJob[0]).WillBeEating.Add(this);
                     }
                     else
                     {
                         CookingJob cJob = new CookingJob(Household.Home, Household.Ownership, Household.CookingJob);
                         ((CookingJob)Household.CookingJob[0]).WillBeEating.Add(this);
                     }
                 }

             }*/



        #region HasEntityGroupID ILookup

        HasEntityGroupID hasEntityGroupID;
        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID
        {
            get
            {
                return hasEntityGroupID;
            }
        }

        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.GetUniqueID()
        {
            return HasEntityGroup.GetUniqueID();
        }

       
        void ILookUp<IHasEntityGroup, HasEntityGroupID>.AddToLookup()
        {
            hasEntityGroupID = ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).GetUniqueID();

            if (hasEntityGroupID != HasEntityGroupID.Invalid)
            {
                LookUpHasEntityGroup.Add(hasEntityGroupID, this); // uses special class!
            }
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.RemoveIDEntry()
        {
            LookUpHasEntityGroup.Remove(this);  // uses special class!
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.CreateLookupCollection() // interface method - does nothing...
        {

        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.SetInvalid()
        {
            hasEntityGroupID = HasEntityGroupID.Invalid;
        }

        #endregion

        #region OwnerID ILookup

        OwnerID ownerID;
        OwnerID ILookUp<IOwner, OwnerID>.ID
        {
            get
            {
                return ownerID;
            }
        }

        OwnerID ILookUp<IOwner, OwnerID>.GetUniqueID()
        {
            return Owner.GetUniqueID();
        }

       
        void ILookUp<IOwner, OwnerID>.AddToLookup()
        {
            ownerID = ((ILookUp<IOwner, OwnerID>)this).GetUniqueID();

            if (ownerID != OwnerID.Invalid)
            {
                LookUpOwners.Add(ownerID, this); // uses special class!
            }
        }

        void ILookUp<IOwner, OwnerID>.RemoveIDEntry()
        {
            LookUpOwners.Remove(this);  // uses special class!
        }

        void ILookUp<IOwner, OwnerID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IOwner, OwnerID>.SetInvalid()
        {
            ownerID = OwnerID.Invalid;
        }

        void ILookUp<IOwner, OwnerID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

       

        #endregion
    }
}
