using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Substances
{
    public class SubstanceType: IGameData 
    {

        public SubstanceType()
        {
        }

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
            get; set;
        }

        public void PreInitValidate(ref List<string> errors)
        {
           
        }

        public void Initialize()
        {
          
        }

        public void PostInitValidate(ref List<string> errors)
        {
           
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteInitialize()
        {
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

    }
}
