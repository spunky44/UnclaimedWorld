using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.Missions;
using UWGame.SimSide.AI;

namespace UWGame.ClientSide.Interface.Personnel
{
    public class PersonnelRosterPanel : RosterPanel
    {
        PersonnelList PersonnelList;

        public PersonnelRosterPanel()
            : base("PERSONNEL",
            615 /*600*/, The.InGameUI.rosterPanelHeight, false,
            panelType: PanelType.RosterPanel) // to avoid using the event archive ctor...            
        {            
            PersonnelList = new PersonnelList(intface, lcdSurface, false, 0, true);
        }

        public override void Show()
        {
            PersonnelList.Fill(GetPeople, false, true);

            base.Show();            
        }


        public static List<IKnownEntityData> GetPeople()
        {
            return The.InGameUI.UIAllegiance.MembersList.Select(e => (IKnownEntityData)e).ToList();             
        }

        public override void Refresh()
        {
            base.Refresh();

            PersonnelList.Populate();
        }
    }
}
