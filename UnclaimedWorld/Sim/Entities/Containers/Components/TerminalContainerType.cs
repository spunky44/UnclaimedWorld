using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class TerminalContainerType : ContainerType, IHasItemStorageType
    {
        /// <summary>
        /// optional - TODO!! needs IExit. 
        /// </summary>
        public Vector2[] Doors;
        public bool HasRallyPointInCourtyard = false;


        /// <summary>
        /// regular storage - must be filled
        /// </summary>
        public ItemStorageType ItemStorageType { get; set; }


        /// <summary>
        /// holds items offered for trade - must be filled
        /// </summary>
        public ItemStorageType OfferedForTradeStorageType;


        public string DefaultStorageSettings;

        [XmlIgnore]
        public DefaultStorageSettings DefaultStorageSettingsFinal
        {
            get;
            private set;
        }

        public bool AllowStockpiling = true;

        public bool AllowsStockpiling
        {
            get
            {
                return AllowStockpiling;
            }
        }


        public override Container CreateContainer(Entity parent)
        {
            return new TerminalContainer(parent);
        }

        public override float? FullStatePercentage
        {
            get
            {
                return OfferedForTradeStorageType.FullStatePercentage;
            }
        }

        public override float? HalfFullStatePercentage
        {
            get
            {
                return OfferedForTradeStorageType.HalfFullStatePercentage;
            }
        }

       
        public TerminalContainerType()
        {
            
        }

      /*  public ToolContainerType(string condition1, float capacity1, Storage.Conditions? condition2 = null, 
                                float? capacity2 = null, Storage.Conditions? condition3 = null, float? capacity3 = null)
        {
            
        }*/

        public override DefaultStorageSettings GetDefaultStorageSettings()
        {
            return DefaultStorageSettingsFinal;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (ItemStorageType != null)
            {
                ItemStorageType.Initialize();
            }

            if (DefaultStorageSettings != null)
                DefaultStorageSettingsFinal = GameData.Instance.AllDefaultStorageSettings[DefaultStorageSettings];

        }


        public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
        {
            base.PostInitValidate(parent, ref listOfErrors);

            if (Doors == null &&
                CanBeEnteredByTags != null)
            {
                EntityType.CreateValidationError(ref listOfErrors, "CanBeEnteredByTags requires Doors to be specified also.");

            }
        }
       

        public override void PostLoadContentInitialize(EntityType parent)
        {
            base.PostLoadContentInitialize(parent);
           
        }
    }
}
