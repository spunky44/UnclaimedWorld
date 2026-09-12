using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.PropertyPresentation;

namespace UWGame.SimSide.InGameEvents.PropertyObjects
{
    /// <summary>
    /// this call returns a single IHasExposedProperties object, wrapped in a list... not sure how useful it is.
    /// </summary>
  /*  public class GetObject
    {
        public string PropertyKey;

        /// <summary>
        /// only one may be filled... the usual caveat when using composition instead of inheritance
        /// </summary>
        public GetObject NextGetObject;
        public GetList NextGetList;

        public List<IHasExposedProperties> GetResult(List<IHasExposedProperties> context = null)
        {

            List<IHasExposedProperties> listOfResults = new List<IHasExposedProperties>();

            if (context == null)
            {
                // get the root (site):
                PropertyResult? result = TargetObject.GetRootElement().GetPropertyValue(PropertyKey);
                if (result.HasValue && result.Value.HasExposedPropertiesResult != null)
                {
                    // wrap in a list:
                    listOfResults.Add(result.Value.HasExposedPropertiesResult);
                }
            }
            else
            {
                // the chained case:
                IHasExposedProperties firstItem = context[0];

                PropertyResult? result = firstItem.GetPropertyValue(PropertyKey);
                if (result.HasValue && result.Value.HasExposedPropertiesResult != null)
                {
                    // wrap in a list:
                    listOfResults.Add(result.Value.HasExposedPropertiesResult);
                }
            }


            if (NextGetObject != null)
            {
                return NextGetObject.GetResult(listOfResults);
            }
            else if (NextGetList != null)
            {
                // invoke the child result:
                return NextGetList.GetResult(listOfResults);
            }
            else
            {
                return listOfResults;
            }
        }

       

    }*/
}
