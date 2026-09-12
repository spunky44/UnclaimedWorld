using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Overlays
{
    public enum EntityGrouping 
    { 
        Structures, Animals, 
        ColonyMembers, // Keep this for grouping map entities, but display its setting in the single item grid
        Interest 
    }


    public enum OverlayTypes { Threats, BuildAreas, ColonyMembers }

    public enum EditorOverlayTypes { BuildAreas, EntityIDs, Coords, TerrainDivision }


    public class OverlaySettings : ISnapshot
    {
        #region Colors for resources and entities as shown on the mini-map (=mini map) and display resource menu
        // Faded colors are used for memory facts on the minimap

        public static Color threatColor = Common.ColorFromHex("#FF4C58");
        public static Color buildableColor = Common.ColorFromHex("#BCFF59");
        public static Color personsColor = Common.ColorFromHex("#009cff"); //blue. corresponding "faded" color could be: #0170b6


        public static Color animalsColor = Common.ColorFromHex("#ff0066"); //burgundy
        public static Color animalsFaded = Common.ColorFromHex("#b50048");

        public static Color structuresColor = Common.ColorFromHex("#FFFFFF"); //white
        public static Color structuresFaded = Common.ColorFromHex("#c5c5c5");

       
        public static Color interestColor = Common.ColorFromHex("#ff8400"); //orange
        public static Color interestFaded = Common.ColorFromHex("#cd6b02");
        //raw materials and food colors not set here. instead defined in Basedataloader, search for: category = new ResourceCategory()
        //    public static Color rawmaterials = Common.ColorFromHex("#0000FF");//color not set here - instead defined in Basedataloader, search for: category = new ResourceCategory()
        //    public static Color food = Common.ColorFromHex("#009933"); //color not set here - instead defined in Basedataloader, search for: category = new ResourceCategory()

        #endregion

      
        #region Single item settings

        public Dictionary<OverlayTypes, bool> OverlayTypeSettings = new Dictionary<OverlayTypes, bool>();

        public Dictionary<EditorOverlayTypes, bool> EditorOverlayTypeSettings = new Dictionary<EditorOverlayTypes, bool>();
      
        #endregion

        #region Resources

        public Dictionary<ResourceCategory, bool> ResourceCategoriesToDisplay = new Dictionary<ResourceCategory, bool>();
        public Dictionary<ResourceType, bool> ResourceTypesToDisplay = new Dictionary<ResourceType, bool>();

        #endregion

        #region Entities
        public Dictionary<EntityGrouping, bool> EntityTypeGroupingsToDisplay = new Dictionary<EntityGrouping, bool>();
        public Dictionary<EntityType, bool> EntityTypesToDisplay = new Dictionary<EntityType, bool>();


        #endregion

        /// <summary>
        /// computed from the other dicts.
        /// </summary>      
        private Dictionary<EntityGrouping, List<EntityType>> entityCategoryItemSettings = new Dictionary<EntityGrouping, List<EntityType>>();
        private Dictionary<ResourceCategory, List<ResourceType>> resourceCategoryItemSettings = new Dictionary<ResourceCategory, List<ResourceType>>();


        public bool ShowOverlaysOnGameArea = false;


        public OverlaySettings()
        {            
            if (!Snapshotter.IsSnapshotting)
            {
                LoadDefaultSettings();
            }
        }

        private void LoadDefaultSettings()
        {
            // load defaults:
            var groupings = Enum.GetValues(typeof(EntityGrouping));
            foreach (var item in groupings) //
            {
                EntityGrouping grouping = (EntityGrouping)item;
                bool displayByDefault = false;
                GameData.Instance.GUIConstants.OverlayDefaultDisplaySettings.TryGetValue(grouping, out displayByDefault);
                EntityTypeGroupingsToDisplay.Add(grouping, displayByDefault);
            }

            foreach (var item in GameData.Instance.AllEntityTypes)
            {
                bool displayByDefault = false;
                if (GameData.Instance.GUIConstants.OverlayDefaultEntityTypeDisplaySettings.TryGetValue(item.Key, out displayByDefault))
                {
                    EntityTypesToDisplay[item.Value] = displayByDefault;                
                }
                else 
                {
                    bool value = false;
                    // set to grouping's value:
                    EntityGrouping? grouping = GetGrouping(item.Value);
                    if (grouping.HasValue)
                    {
                        value = EntityTypeGroupingsToDisplay[grouping.Value];
                    }

                    EntityTypesToDisplay[item.Value] = value;
                }
            }

            foreach (var item in GameData.Instance.AllResourceCategories)
            {
                bool displayByDefault = false;
                GameData.Instance.GUIConstants.OverlayDefaultResourceCategoryDisplaySettings.TryGetValue(item.Key, out displayByDefault);
                ResourceCategoriesToDisplay.Add(item.Value, displayByDefault);
            }

            foreach (var item in GameData.Instance.AllResourceTypes)
            {
                bool displayByDefault = false;
                if (GameData.Instance.GUIConstants.OverlayDefaultResourceTypeDisplaySettings.TryGetValue(item.Key, out displayByDefault))
                {
                    ResourceTypesToDisplay[item.Value] = displayByDefault;
                }
                else
                {
                    bool value = ResourceCategoriesToDisplay[item.Value.Category];

                    ResourceTypesToDisplay[item.Value] = value;
                }
            }

            foreach (var item in GameData.Instance.GUIConstants.OverlayDefaultTypeDisplaySettings)
            {
                OverlayTypeSettings[item.Key] = item.Value;
            }

            InitEditorOverlaySettings();


            ShowOverlaysOnGameArea = GameData.Instance.GUIConstants.ShowOverlaysOnGameAreaDefault;

        }

        private void InitEditorOverlaySettings()
        {
            foreach (var item in GameData.Instance.GUIConstants.OverlayDefaultTypeEditorDisplaySettings)
            {
                EditorOverlayTypeSettings[item.Key] = item.Value;
            }
        }

        public bool EntityCategoryHasDifferentItemSetting(bool categorySetting, EntityGrouping grouping)
        {
            //if (entitySettingsAreDirty)
            //    {
            RecomputeEntityCategoryHasItemSettings(grouping);
            //     }

            List<EntityType> list;
            if (entityCategoryItemSettings.TryGetValue(grouping, out list))
            {
                return list.Exists(e => ItemSettingDiffers(EntityTypesToDisplay, e, categorySetting));
            }
            return false;
        }

        public static bool ItemSettingDiffers( Dictionary<EntityType, bool> mayStockpileItem, EntityType entityType, bool setting)      
        {
            bool value;
            if (mayStockpileItem.TryGetValue(entityType, out value))
            {
                return value != setting;
            }

            return false;
        }

        private void RecomputeEntityCategoryHasItemSettings(EntityGrouping grouping)
        {
            entityCategoryItemSettings.Clear();

            foreach (var item in EntityTypesToDisplay)
            {
                if (grouping == GetGrouping(item.Key))
                {
                    Common.AddToMultiList(entityCategoryItemSettings, GetGrouping(item.Key).Value, item.Key);
                }
            }
            //entitySettingsAreDirty = false;
        }

        /// <summary>
        /// item setting will override category setting
        /// </summary>
        public bool DisplayEntityType(EntityType entityType)
        {
            bool itemSetting;
            if (EntityTypesToDisplay.TryGetValue(entityType, out itemSetting))
            {
                return itemSetting;
            }
            else
            {
                bool entityCategorySetting;
                EntityGrouping? grouping = GetGrouping(entityType);

                if (grouping.HasValue
                    && EntityTypeGroupingsToDisplay.TryGetValue(grouping.Value, out entityCategorySetting))
                {
                    return entityCategorySetting;
                }
                else
                {
                    // if category has not been set, and item neither, then default is Allow:
                    return false;
                }
            }

        }

        public bool DisplayAnyResources()
        {
            return ResourceTypesToDisplay.Any(r => r.Value == true)
                || ResourceCategoriesToDisplay.Any(c => c.Value == true);
        }

        public bool DisplayAnyEntities()
        {
            return EntityTypesToDisplay.Any(r => r.Value == true)
                || EntityTypeGroupingsToDisplay.Any(c => c.Value == true);
        }

        public bool ResourceCategoryHasDifferentItemSetting(bool categorySetting, ResourceCategory category)
        {
            // if (resourceSettingsAreDirty)
            //   {
            RecomputeResourceCategoryHasItemSettings(category);
            //  }

            List<ResourceType> list;
            if (resourceCategoryItemSettings.TryGetValue(category, out list))
            {
                return list.Exists(e => ResourceItemSettingDiffers(ResourceTypesToDisplay, e, categorySetting));
            }
            return false;
        }

        private bool ResourceItemSettingDiffers(Dictionary<ResourceType, bool> mayStockpileItem, ResourceType resourceType, bool setting)
        {
            bool value;
            if (mayStockpileItem.TryGetValue(resourceType, out value))
            {
                return value != setting;
            }

            return false;
        }

        /// <summary>
        /// compute a mapping from category to item types that have a setting (mayStockpileItem)
        /// </summary>
        private void RecomputeResourceCategoryHasItemSettings(ResourceCategory resourceCategory)
        {
            resourceCategoryItemSettings.Clear();

            foreach (var item in ResourceTypesToDisplay)
            {
                if (item.Key.Category == resourceCategory)
                {
                    Common.AddToMultiList(resourceCategoryItemSettings, item.Key.Category, item.Key);
                }
            }
            //resourceSettingsAreDirty = false;
        }

        /// <summary>
        /// item setting will override category setting
        /// </summary>     
        public bool DisplayResourceType(ResourceType resourceType)
        {
            bool itemSetting;
            if (ResourceTypesToDisplay.TryGetValue(resourceType, out itemSetting))
            {
                return itemSetting;
            }
            else
            {
                bool resourceCategorySetting;
                if (ResourceCategoriesToDisplay.TryGetValue(resourceType.Category, out resourceCategorySetting))
                {
                    return resourceCategorySetting;
                }
                else
                {
                    // if category has not been set, and item neither, then default is Allow:
                    return false;
                }
            }
        }

        public bool DrawResource(ResourceContainer container, bool isInGodMode)
        {
            SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
           
            if (DisplayResourceType(container.ResourceType)
                    && (isInGodMode || sharedKnowledge.AllDetectedEntities.Contains(((IDetectable)container).ID))
                    && container.NoOfHarvestableItems > 0)
            {
                return true;
                
            }
            else return false;
        }

        public static string GetName(EntityGrouping grouping)
        {
            switch (grouping)
            {
                case EntityGrouping.Interest: return "INTEREST";
                case EntityGrouping.ColonyMembers: return "COLONY MEMBERS";
                case EntityGrouping.Animals: return "ANIMALS";
                case EntityGrouping.Structures: return "STRUCTURES";
                default: return null;
            }
        }

       

        public static string GetIconFromOverlayType(EditorOverlayTypes type)
        {
            switch (type)
            {
                case EditorOverlayTypes.BuildAreas: return "HUD_icon_structure";
                case EditorOverlayTypes.EntityIDs: return null; // "HUD_icon_status_skull";
                case EditorOverlayTypes.Coords: return null; // "HUD_icon_person";
                case EditorOverlayTypes.TerrainDivision: return null;
            }

            return null;
        }


        public static string GetTextFromOverlayType(EditorOverlayTypes type)
        {
            switch (type)
            {
                case EditorOverlayTypes.BuildAreas: return "BUILDABLE AREA";
                case EditorOverlayTypes.Coords: return "COORDINATES";
                case EditorOverlayTypes.EntityIDs: return "IDs";
                case EditorOverlayTypes.TerrainDivision: return "TERRAIN DIVISION";
            }

            return null;
        }

        const string buildTooltip = "Shows the areas that can be built on, and where characters can go. Structures cannot be built on the red and yellow areas.";
        public static string GetTooltipFromOverlayType(EditorOverlayTypes type)
        {
            switch (type)
            {
                case EditorOverlayTypes.BuildAreas: return buildTooltip;
                case EditorOverlayTypes.Coords: return "Shows tile coordinates and world coordinates on each tile";
                case EditorOverlayTypes.EntityIDs: return "Shows entity IDs";
                case EditorOverlayTypes.TerrainDivision: return "Shows terrain division";
            }

            return null;
        }

        public static Color? GetColorFromOverlayType(EditorOverlayTypes type)
        {
            switch (type)
            {
                case EditorOverlayTypes.BuildAreas: return buildableColor; 
                case EditorOverlayTypes.EntityIDs: return threatColor; // Color.Red;
                case EditorOverlayTypes.Coords: return personsColor; // return Color.Blue;  
                case EditorOverlayTypes.TerrainDivision: return animalsColor;
            }

            return null;
        }

        public static string GetIconFromOverlayType(OverlayTypes type)
        {
            switch (type)
            {
                case OverlayTypes.BuildAreas: return "HUD_icon_structure";
                case OverlayTypes.Threats: return "HUD_icon_status_skull";
                case OverlayTypes.ColonyMembers: return "HUD_icon_person";
            }

            return null;
        }

        public static string GetTextFromOverlayType(OverlayTypes type)
        {
            switch (type)
            {
                case OverlayTypes.BuildAreas: return "BUILDABLE AREA";
                case OverlayTypes.Threats: return "THREATS";
                case OverlayTypes.ColonyMembers: return "COLONY MEMBERS";  
            }

            return null;
        }

        public static string GetTooltipFromOverlayType(OverlayTypes type)
        {
            switch (type)
            {
                case OverlayTypes.BuildAreas: return buildTooltip;
                case OverlayTypes.Threats: return "Shows the areas considered dangerous by the colonists because threats have been spotted there. Threats can sometimes be removed with the ATTACK action.";
                case OverlayTypes.ColonyMembers: return "Shows where the colony members are on the minimap";
            }

            return null;
        }

        public static Color? GetColorFromOverlayType(OverlayTypes type)
        {
            switch (type)
            {
                case OverlayTypes.BuildAreas: return buildableColor; // Color.Yellow;
                case OverlayTypes.Threats: return threatColor; // Color.Red;
                case OverlayTypes.ColonyMembers: return personsColor; // return Color.Blue;  
            }

            return null;
        }
        
      

        public static EntityGrouping? GetGrouping(IKnownEntityData entityData)
        {
            Entity entity = entityData as Entity;

            if (entity != null && The.InGameUI.UIAllegiance.Members.Contains(entity))
            {
                return EntityGrouping.ColonyMembers;
            }
            else
            {
                return GetGrouping(entityData.EntityType);
            }

        }

        public static EntityGrouping? GetGrouping(EntityType entityType)
        {
            if (entityType.StructureType != null)
            {
                return EntityGrouping.Structures;
            }
            /*  else if (entityType.IntelligenceType != null/*
                  entityType.IntelligenceT)
              {
                  return EntityGrouping.ColonyMembers;
              }*/
            else if (entityType.BiologicalType != null && entityType.Person == null)
            {
                return EntityGrouping.Animals;
            }
            else if (entityType.TerrainType != null && entityType.TerrainType.IsSpecialInterestFeature) // farmplots, nests and fishing spots
            {
                return EntityGrouping.Interest;
            }

            return null;
        }

        public static Color? GetGroupingColorFromEntity(IKnownEntityData entityData, bool faded)
        {
            switch (GetGrouping(entityData))
            {
                case EntityGrouping.Animals: if (faded) return animalsFaded; else return animalsColor;
                case EntityGrouping.Structures: if (faded) return structuresFaded; else return structuresColor;
                case EntityGrouping.ColonyMembers: return personsColor;
                case EntityGrouping.Interest: if (faded) return interestFaded; else return interestColor;
            }

            return null;
        }

        public static Color? GetGroupingColorFromEntityType(EntityType entityType)
        {
            switch (GetGrouping(entityType))
            {
                case EntityGrouping.Animals: return animalsColor;
                case EntityGrouping.Structures: return structuresColor;
              //  case EntityGrouping.ColonyMembers: return persons;
                case EntityGrouping.Interest: return interestColor;
            }

            return null;
        }

       /* public static Color? GetGroupingColorFromEntityType(EntityType entityType, bool faded)
        {
            switch (GetGrouping(entityType))
            {
                case EntityGrouping.Animals: if (faded) return animalsFaded; else return animals;
                case EntityGrouping.Structures: if (faded) return structuresFaded; else return structures;
                case EntityGrouping.ColonyMembers: return persons;
                case EntityGrouping.Interest: if (faded) return interestFaded; else return interest;
            }

            return null;
        }*/

        public static Color? GetGroupingColor(EntityGrouping entityGrouping, bool faded)
        {
            switch (entityGrouping)
            {
                case EntityGrouping.Animals: if (faded) return animalsFaded; else return animalsColor;
                case EntityGrouping.Structures: if (faded) return structuresFaded; else return structuresColor;
                case EntityGrouping.ColonyMembers: return personsColor;
                case EntityGrouping.Interest: if (faded) return interestFaded; else return interestColor;
            }

            return null;
        }



        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.EntityTypesToDisplay = sn.DoDictionary(EntityTypesToDisplay);
            this.ResourceTypesToDisplay = sn.DoDictionary(ResourceTypesToDisplay);
            this.OverlayTypeSettings = sn.DoDictionary(OverlayTypeSettings);

            this.ResourceCategoriesToDisplay = sn.DoDictionary(ResourceCategoriesToDisplay);
            this.EntityTypeGroupingsToDisplay = sn.DoDictionary(EntityTypeGroupingsToDisplay);

            this.entityCategoryItemSettings = sn.DoMultiMap(entityCategoryItemSettings);
            this.resourceCategoryItemSettings = sn.DoMultiMap(resourceCategoryItemSettings);

            this.ShowOverlaysOnGameArea = sn.DoBool(ShowOverlaysOnGameArea);

            sn.Ignore(interestColor);
            sn.Ignore(interestFaded);
            sn.Ignore(animalsColor);
            sn.Ignore(animalsFaded);
            sn.Ignore(structuresColor);
            sn.Ignore(structuresFaded);
            sn.Ignore(personsColor);
            sn.Ignore(threatColor);
            sn.Ignore(buildableColor);
            sn.Ignore(EditorOverlayTypeSettings);


            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            InitEditorOverlaySettings();
        }

        #endregion

    }
}
