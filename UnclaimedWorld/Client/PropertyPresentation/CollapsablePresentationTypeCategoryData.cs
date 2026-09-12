using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.ClientSide;

namespace UWGame.ClientSide.PropertyPresentation
{
   public  class CategoryPanelAndData
    {
        public CollapsablePanel Panel;
        public Grid Grid;
        public PresentationTypeCategory Category;


        public CategoryPanelAndData(CollapsablePanel panel, Grid grid, PresentationTypeCategory category)
        {
            Panel = panel;
            Grid = grid;
            Category = category;
            int index = 0;

            foreach (var propertyPresentation in Category.Nodes)
            {
                propertyPresentation.Index = index;
                index++;
            }
        }

       

    }
}
