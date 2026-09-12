using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowSystem
{
    public enum SortingMethod
    {
        /// <summary>
        /// use the static sort order value
        /// </summary>
        StaticSortOrder,

        /// <summary>
        /// use the number result as is
        /// </summary>
        NumberResult,

        /// <summary>
        /// sort by the absolute distance to 0.5
        /// used for status icons to move the most extreme items to the left
        /// </summary>
        NumberResultMiddleDistance
    }

    public interface IKeyedEntryComponent
    {
        void BeginAddingEntries();
        void EndAddingEntries();

        bool TryRemoveEntry(object key);
        bool TryGetEntry(object key, out UIComponent entry);

        List<UIComponent> Entries { get; set; }

        void RemoveEntry(object key, UIComponent item);

        bool CanProcessEntryData(string entryTerm, string entryIconName, float? normalizedValue);

        UIComponent AddEntry(object key, string term, string caption, //string entryTermTooltip, string captionTooltip,
            Action<UIComponent, bool> tooltipDisplayedCallback, //UIComponent tooltipCallback,  //Action<UIComponent, bool> tooltipDisplayedCallback, 
            string showTooltipSetProperty, bool? disableTooltipExpiry, int? tooltipWidth,
            string entryIconName, Color? iconColor, Color? termColor,
            int? orderingNumber,
            float? entryValue,
            bool showBar = false, float? normalizedValue = null, float? normalizedValue2 = null, int? barWidth = null, //we don't support max value...
            bool clickable = false, uint? entityID = null,
            UIComponent customComponent = null, //  Func<UIComponent> createCustomControlCallback = null,
            int? height = null, int? paddingLeft = null, bool useIconBackground = false);

        void UpdateEntry(UIComponent existingEntry, string term, string caption, 
            string entryTermTooltip, string captionTooltip, //string showTooltipSetProperty,
            string entryIconName, Color? iconColor, Color? termColor,
            float? entryValue, float? normalizedValue1, float? normalizedValue2 = null, 
            int? paddingLeft = null, int? paddingRight = null, bool useIconBackground = false, bool showFaded = false, bool rightAdjustTerm = false); 
        
        /// <summary>
        /// not implemented..?
        /// </summary>
        /// <param name="key"></param>
        /// <param name="orderingNumber"></param>
        /// <param name="entryValue"></param>
        /// <returns></returns>
        bool AddSeparator(object key, int? orderingNumber, float? entryValue);

        bool AddSubHeader(string subHeaderName, int? orderingNumber, float? entryValue);

        /// <summary>
        /// adds a grid inside a panel
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        UIComponent AddGroup(object key, bool hasBorder, int? marginLeft = null);

        object GetKeyFromIndex(int index);

        /// <summary>
        /// some components only allow a max number of entries
        /// </summary>
        void CapNoOfEntries();

        void Sort(Grid.Sorting sortType, bool useFirstTag, Func<float, float> transformation = null);

      
        void Clear();

        int Count
        {
            get;
        }
    }
}
