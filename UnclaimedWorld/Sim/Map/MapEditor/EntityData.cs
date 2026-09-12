using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.Maps.MapEditor
{
    public enum OwnerFlag { None, Player }

    /// <summary>
    /// contains data that is used to instantiate an Entity.
    /// Entity class is huge, so this simpler class was created for that purpose.
    /// IT has a template-like format to create randomizations easily
    /// 
    /// NOTE: don't but primitive fields in this class. it will bloat the xml for all the terrain entities on the map. place them in subclasses instead
    /// 
    /// TODO: in SpawnEntityAction, we want dynamic options for all these fields
    /// 
    /// NOTE! Don't change this data (such as Location) after init. The data may be shared to create several entity instances at the same location
    /// 
    /// TODO: make this an IGameData class?
    /// </summary>
    public class EntityData : IGameData 
    {
        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }

      //  public string Name;

        public string EntityKey;

       
        public Vector3? Location;

        public float? Rotation;

        public bool FlipHorizontally;

        /// <summary>
        /// for Biologicals, if not filled, a bulk derived from age will be used
        /// </summary>
        public float? Bulk;

      //  public DateAndTime.TimeDateYear? SpawnDate;


        public Resource[] Resources;

        public Tree Tree;
        public BiologicalEntity BioEntity;
        public Rock Rock;
        public Person Person;
        public Threat Threat;

        public SerializableDictionary<string, NeedData> NeedLevels;
        //public SerializableDictionary<string, float> NeedDaysAtZero;
        

        /// <summary>
        /// only one of these two should be filled in.
        /// 
        /// owned robots/animals will also become members.
        /// </summary>
        public AllegianceAndExpedition OwnedBy;
        public AllegianceAndExpedition MemberOf;

      

        public SerializableDictionary<string, PropertyResult> Properties;

        public string[] EffectProfiles;

        //public float? SpawnTimeOfDay;
        //public int? SpawnDay;

        // these will prevent xsi:nil elements from being output when the value is null, instead omitting the element.
       /* public bool ShouldSerializeSpawnDate()
        {
            return SpawnDate != null;
        }*/

        public bool ShouldSerializeRotation()
        {
            return Rotation != null;
        }

        public bool ShouldSerializeResources()
        {
            return Resources != null;
        }

        public bool ShouldSerializeBulk()
        {
            return Bulk != null;
        }

        public void SetRandomStats()
        {
        }

        public void PostInitValidate(ref List<string> listOfErrors)
        {
            
        }

        public void PostLoadContentValidate(List<string> listOfErrors)
        {
            
            /*
            if (BioEntity != null)
            {
                if (BioEntity.AgeInYears.HasValue == false)
                {
                    EntityType.CreateValidationError(ref listOfErrors, "Age was never set for BioEntity." + EntityKey);
                }
            }*/

          
            EntityType entityType;
            if (!GameData.Instance.AllEntityTypes.TryGetValue(EntityKey, out entityType))
            {
                // fails for spawn action in same scenario as the entity type is defined. InitActions comes before InitEntityTypes... 
           /*       EntityType.CreateValidationError(ref listOfErrors, "Unknown entity type (check if it is added to list in loader.cs): " + EntityKey);
            * */
                return;
            }


            if (entityType != null && entityType.BiologicalType != null && Bulk < entityType.BiologicalType.MinimumBulk)
            {
                EntityType.CreateValidationError(ref listOfErrors, string.Format("Bulk [{0}] is lower than the minimum [{1}] for {2}.", Bulk, entityType.BiologicalType.MinimumBulk, GetName()));
            }

            if (BioEntity != null)
            {
                if (BioEntity.TraitTemplates == null)
                {
                    if (Person != null)
                    {
                        foreach (var item in GameData.Instance.AllSkillTypes)
                        {
                            if (BioEntity.Skills == null || !BioEntity.Skills.ContainsKey(item.Key))
                            {
                                EntityType.CreateValidationError(ref listOfErrors, string.Format("Skill {0} not defined for Person: {1}", item.Key, GetName()));

                            }    
                        }
                    }

                   /* if (BioEntity.AgeInYears.HasValue == false)
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "Age was never set for BioEntity." + EntityKey);
                    }*/
                }                
            }
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (EffectProfiles != null)
            {
                foreach (var item in EffectProfiles)
                {
                    EffectProfileType effect;
                    EntityType.ValidateGameDataTypeExists(ref listOfErrors, item, GameData.Instance.AllEffectProfileTypes, out effect);                    
                }
            }
        }


        private string GetName()
        {
            if (Person != null)
            {
                string totalName = (Person.FirstName ?? "") + " " + (Person.LastName ?? "");
                totalName = totalName.Trim();
                return totalName;
            }
            else
            {
                return Name ?? EntityKey;
            }
        }

        public void Initialize()
        {

        }

        public void PostDataCompleteInitialize()
        {

            if (Resources != null)
            {
                foreach (var item in Resources)
                {
                    item.PostDataCompleteInitialize();
                }
            }

        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }

        public void PreInitValidate(ref List<string> errors)
        {
          //  EntityType.ValidateRequiredValue(ref errors, "KeyName", !string.IsNullOrEmpty(KeyName));

        }
    }

    

    public class NeedData
    {
        public NormalDistribution Level;
        public NormalDistribution DaysAtZero;

    }

    public class AllegianceAndExpedition
    {
        public string AllegianceKey;
        public string ExpeditionKey;
        // enable us to set the owner dynamicly when spawning entities
        public EvalNode DynamicAllegianceKey;
        public EvalNode DynamicExpeditionKey;

        public void Resolve(EventAction action, ref string allegiance, ref string expedition)
        {
            Resolve(action.TriggeringEntity, action.TargetEntity, action.PolledEventSource, action.DynamicTarget, ref allegiance, ref expedition);
        }

        public void Resolve(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string allegiance, ref string expedition)
        {

            allegiance = ResolveAllegiance(DynamicAllegianceKey, AllegianceKey, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            expedition = ResolveExpedition(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
        }

        private string ResolveExpedition(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            string expedition = "";
            if (DynamicExpeditionKey != null)
            {
                PropertyResult? expeditionKey = DynamicExpeditionKey.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (expeditionKey.HasValue)
                {
                    expedition = expeditionKey.Value.StringResult;
                }
            }
            else if (ExpeditionKey != null)
            {
                expedition = ExpeditionKey;
            }
            return expedition;
        }

        public static bool ResolveAllegiance(EvalNode dynamicAllegianceKey, string allegianceKey, EventAction eventAction, out Allegiance allegiance, ref string failReason)
        {
            allegianceKey = ResolveAllegiance(dynamicAllegianceKey, allegianceKey, 
                eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);

            allegiance = The.Sim.World.GetAllegianceFromKey(allegianceKey);
            if (allegiance == null)
            {
                failReason = "Allegiance not found";
                return false;
            }

            return true;
        }

        public static string ResolveAllegiance(EvalNode dynamicAllegianceKey, string allegianceKey, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            string allegiance = "";
            if (dynamicAllegianceKey != null)
            {
                PropertyResult? allegianceResult = dynamicAllegianceKey.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                if (allegianceResult.HasValue)
                {
                    allegiance = allegianceResult.Value.StringResult;
                }
            }
            else if (allegianceKey != null)
            {
                allegiance = allegianceKey;
            }

            return allegiance;
        }

        public bool Resolve(EventAction eventAction, 
            out Allegiance allegiance, out Expedition expedition, ref string failReason)
        {
            allegiance = null;  
            expedition = null;    
       
            if (!ResolveAllegiance(DynamicAllegianceKey, AllegianceKey, eventAction, out allegiance, ref failReason))
            {
                return false;
            }

            /*
            string allegianceKey = "", expeditionKey = "";
            Resolve(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget, ref allegianceKey, ref expeditionKey);
            allegiance = The.Sim.World.GetAllegianceFromKey(allegianceKey);
            if (allegiance == null)
            {
                failReason = "Allegiance not found";
                return false;
            }*/

            string expeditionKey = ResolveExpedition(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);

            expedition = allegiance.GetExpedition(expeditionKey);
            if (expedition == null)
            {
                failReason = "Expedition not found";
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            string text = "";
            if (DynamicAllegianceKey != null)
            {
                text = DynamicAllegianceKey.ToString();
            }
            else if (AllegianceKey != null)
            {
                text = AllegianceKey;
            }

            if (DynamicExpeditionKey != null)
            {
                text += " " + DynamicExpeditionKey.ToString();
            }
            else if (ExpeditionKey != null)
            {
                text += " " + ExpeditionKey.ToString();         
            }

            return text;
        }
    }
}
