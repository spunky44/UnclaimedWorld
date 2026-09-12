using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Research
{
    public class KnowledgeType : IGameData
    {
        public string KeyName { get; set; }

        public string Name { get; set; }

        public bool DeleteRecord
        {
            get;
            set;
        }

        public string SummaryDescription;

        public string Description;


        public void Initialize()
        {
        }
        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors)
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
