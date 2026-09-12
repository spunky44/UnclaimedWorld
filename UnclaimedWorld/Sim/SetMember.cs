using System;
using System.Collections.Generic;
using System.Text;

namespace UWGame.SimSide
{
    public class SetMember
    {
        private SetMember representative;
        private SetMember next;
        private int listLength = 1;

        public SetMember()
        {   // make a set with just this member:
            this.representative = this;
        }

        /// <summary>
        /// Find the set that this member belongs to
        /// </summary>
        /// <returns></returns>
        public SetMember FindSet()
        { 
            return representative;
        }

        public void Union(SetMember setToAdd)
        {
            Union(this, setToAdd);
            
            /*
            if (listLength > setToAdd.listLength)
            {   // add the shortest list to the longest
                Union(this, this, setToAdd);
            }
            else
            {
                Union(this, setToAdd, this);
            }*/
        }

        private void Union(SetMember set1, SetMember set2)
        {
            set1.representative.listLength = set1.representative.listLength + set2.representative.listLength;

            SetMember endOfList, memberToAdd;           
            endOfList = set1;
            while (endOfList.next != null)
            {                
                endOfList = endOfList.next;
            }

            //start adding:
            memberToAdd = set2.representative;
            do
            {
                endOfList.next = memberToAdd;
                memberToAdd.representative = endOfList.representative;

                endOfList = memberToAdd;
                memberToAdd = memberToAdd.next;
            }
            while (memberToAdd != null);

        }

        private void Union(SetMember representative, SetMember set1, SetMember set2)
        {            
            representative.listLength = set1.representative.listLength + set2.representative.listLength;

            SetMember endOfList, memberToAdd;
            // start from the beginning:
            endOfList = set1.representative;
            while (endOfList.next != null)
            {
                endOfList.representative = representative;
                endOfList = endOfList.next;
            }

            //start adding:
            memberToAdd = set2.representative;
            do
            {
                endOfList.next = memberToAdd;
                memberToAdd.representative = representative;

                endOfList = memberToAdd;
                memberToAdd = memberToAdd.next;
            }
            while (memberToAdd != null);           
            
        }
    }
}
