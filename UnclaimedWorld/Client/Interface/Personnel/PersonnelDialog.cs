using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using UWGame.ClientSide.Interface.Controls;

namespace UWGame.ClientSide.Interface.Personnel
{
    public class PersonnelDialog: Panel
    {
        LCDScreen lcdScreen;
        UIComponent lcdSurface;
        Box display; 

        PersonnelList PersonnelList;

        ErrorAndMessagePanel errorAndMessagePanel;


        public event EventHandler OKClick;
        public event EventHandler CancelClick;

        TextButton btOK, btCancel;

        public PersonnelDialog(CommonInterface intf, Point position) :
            base(intf, "", position, new Vector2(468 /* 540*/, 500), Level.StackedDialogs) // Dialogs)
        {

             RosterPanel.CreateRosterStyleLCDPanel(intf, Window, 
                 out display, out lcdSurface, ref lcdScreen);

            errorAndMessagePanel = new ErrorAndMessagePanel(lcdSurface);

            PersonnelList = new PersonnelList(intf, lcdSurface, true, errorAndMessagePanel.Height, false); //base.Window);

            
            btOK = AddLowerButton("OK", "Accepts the order and closes the dialog.", Align.Left);
            btOK.Click += new ClickHandler(btOK_Click);

            btCancel = AddLowerButton("CANCEL", "Cancels and closes the dialog.", Align.Right);
            btCancel.Click += new ClickHandler(btCancel_Click);
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            Window.Hide();

            if (CancelClick != null)
                CancelClick.Invoke(this, null);
        }

        void btOK_Click(UIComponent sender, EventArgs e)
        {
            // clear old errors before validating
            this.errorAndMessagePanel.Clear();

            List<string> errors = null;

            if (ValidateSelectionCanEmbark(ref errors))
            {
                if (this.OKClick != null)
                    OKClick.Invoke(this, null);

                Hide();
            }
            else
            {
                errorAndMessagePanel.ShowErrors(errors);
            }
        }


        public void FillAndShow(Func<List<IKnownEntityData>> getEntities, Point dialogSourceAbsolutePosition, bool showSelectors, bool showMigrateRisk) //, List<EntityID> entities)
        {
            Fill(getEntities, showSelectors, showMigrateRisk); //GetPeople);

            // center dialog over the click source:
            ShowInScreenSpace(dialogSourceAbsolutePosition.X - Window.Width / 2, dialogSourceAbsolutePosition.Y / 2, false);
        }

        private bool ValidateSelectionCanEmbark(ref List<string> errors)
        {
            List<EntityID> selected = GetSelectedEntities();

            int embarkers = 0;
           
          //  IKnownEntityData entityData;
            if (selected != null)
            {
                foreach (var item in selected)
                {
                    Entity entity = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownDataAsEntity(item); //, out entityData)
                    if (entity != null)
                    {
                        if (!entity.Intelligence.IsReadyForEmbark(The.InGameUI.UIAllegiance))
                        {
                            Common.AddToList(ref errors, entity.Name + " is not willing to embark now.");
                        }

                        embarkers++;
                    }
                    else
                    {
                        Common.AddToList(ref errors, "The display is out of date. Please try again.");
                    }
                }
            }

            if (errors == null)
            {
                // exploit: embark from 2 or more sites at once.
                bool isWithinCap = The.InGameUI.UIAllegiance.IsWithinPopulationCap(embarkers);

                if (!isWithinCap)
                {
                    Common.AddToList(ref errors, "Exceeds max population! We do not accept that many newcomers!");
                }
            }

            if (errors != null && errors.Count > 0)
            {
                return false;
            }

            return true;
        }


        public List<EntityID> GetSelectedEntities()
        {
            return PersonnelList.GetSelectedEntities();
        }

        public void Fill(Func<List<IKnownEntityData>> entities, bool showSelectors, bool showMigrationRisk) //Func<List<EntityID>> entities)
        {
            PersonnelList.Fill(entities, showSelectors, showMigrationRisk);

            btOK.Visible = showSelectors;
        }

        public override void Refresh()
        {
            base.Refresh();

            PersonnelList.Populate();
        }
    }
}
