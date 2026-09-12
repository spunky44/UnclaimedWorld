using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps.MapEditor
{
     public class StringChanceSet
     {
         public StringChance[] Chances;
     }


    public class StringChance : IEdge, ISnapshot
    {
        public float Edge { get; set; }

        public string String;


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Edge = sn.DoFloat(Edge);
            this.String = sn.DoString(String);
           
            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }


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

        #endregion

    }


    public class Person
    {
        public string FirstName;
        public string LastName;

        public string PersonalityType;

        public string Portrait;

        /// <summary>
        /// if set true, there will be a duration where the agent will not want to emigrate. 
        /// Most useful to set this true for the player allegiance, so he does not see people emigrate for a little while after game start
        /// </summary>
        public bool SimulateJoinedExpeditionNow = false;

        public void FillEntity(Entity entity)
        {
            Entities.Person personComponent = entity.PersonEntity;
            if (!string.IsNullOrEmpty(PersonalityType))
            {               
                personComponent.Personality = new Personality(entity, GameData.Instance.AllPersonalityTypes[PersonalityType]);              
            }
            else
            {
                // will be created with a random personality type later
                //personComponent.Personality = 
            }
            
            if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
            {
                entity.Intelligence.SetName(FirstName, LastName);
            }

        }


        

    }
}
