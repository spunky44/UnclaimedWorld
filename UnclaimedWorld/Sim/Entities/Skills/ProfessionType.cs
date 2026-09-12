using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Skills
{
    /// <summary>
    /// the highest skill (over a limit) determines the Profession...
    /// </summary>
    public class ProfessionType : IGameData
    {
        public string Name { get; set; }

        public string Icon;

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
        public void Initialize() { }
        public void PreInitValidate(ref List<string> listOfErrors) { }
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
