using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.PropertyPresentation
{
    /// <summary>
    /// defines how a property's values should be presented in the user client
    /// </summary>
    public class PresentationTypeCategory : IGameData, ICategoryType
    {
        public bool Collapsable = true;
        public bool StartsAsExpanded = true;

        public int PanelSortOrder;

        public Sorting PrimarySortingOfItems;
        public Sorting SecondarySortingOfItems;
       

        /// <summary>
        /// a tree with presentation leafs and groups
        /// </summary>
        public PresentationNode[] Nodes;

        public bool SetCountAsSummary = false;

        public string Name
        {
            get;
            set;
        }

        public string KeyName
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }


        public void Initialize()
        {
            if (Nodes != null)
            {
                foreach (var item in Nodes)
                {
                    GroupNode group = item as GroupNode;
                    if (group != null)
                    {
                        group.IsOuterGroup = true;
                    }

                }
            }


        }

        public void PreInitValidate(ref List<string> listOfErrors) 
        { 
            if (Nodes != null)
            {
                foreach (var item in Nodes)
                {
                    item.PreInitValidate(ref listOfErrors);
                }

            }
        }


        public void PostInitValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
    }
}
