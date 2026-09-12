using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.InGameEvents.PropertyObjects
{
    public class GetList
    {
        /// <summary>
        /// the list function
        /// </summary>
        public string HasPropertiesListKey;


        public FilterCondition FilterCondition;
       // public PropertyCondition FilterCondition;



        /// <summary>
        /// only one may be filled... the usual caveat when using composition instead of inheritance
        /// </summary>
        //public GetObject NextObject;
        public GetList NextList;

        public int? MaxResults; // hmmm

        public List<IHasExposedProperties> GetResult(List<IHasExposedProperties> context, 
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            List<IHasExposedProperties> listOfResults = new List<IHasExposedProperties>();

            if (context == null) // start of a chain
            {
                // call the root = Site:
                The.Sim.PlaySite.GetChildren(HasPropertiesListKey, ref listOfResults, FilterCondition, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);

            }
            else
            {
                IHasExposedProperties firstItem = context[0];
                firstItem.GetChildren(HasPropertiesListKey, ref listOfResults, FilterCondition, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }

            if (listOfResults.Count > 0)
            {
                if (MaxResults.HasValue && MaxResults.Value < listOfResults.Count)
                {
                    for (int i = listOfResults.Count - 1; i >= MaxResults.Value; i--)
                    {
                        listOfResults.RemoveAt(i);
                    }
                }

                // chaining:
               /* if (NextObject != null)
                {
                    return NextObject.GetResult(listOfResults);

                }
                else*/ if (NextList != null)
                {
                    return NextList.GetResult(listOfResults, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                }
            }
                        
            return listOfResults;
            
        }


    }
}
