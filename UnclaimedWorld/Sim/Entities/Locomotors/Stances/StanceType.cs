using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities.Locomotors.Stances
{
    public class StanceType: IGameData
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


        /// <summary>
        /// going from a stance with a higher number to a lower number will set the Reverse anim flag
        /// </summary>
        public int Number;

        public AnimModifier? AnimModifier;

        /// <summary>
        /// if true, the character is permitted to take part in conversation in this stance
        /// </summary>
        public bool? CanStartIdleConversation;

        /// <summary>
        /// used when reacting to interest
        /// </summary>
        public bool? CanTurnBody;

        /// <summary>
        /// used when reacting to interest
        /// </summary>
        public bool? CanTurnHead;

        /// <summary>
        /// prone states take more damage from attacks
        /// </summary>
        public bool? IsProne;


        public float IdleExertionLevel;


        public void PreInitValidate(ref List<string> errors)
        {

        }

        public void Initialize()
        {

        }

        public void PostInitValidate(ref List<string> errors)
        {

        }

        public override string ToString()
        {
            return Name ?? KeyName;
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
