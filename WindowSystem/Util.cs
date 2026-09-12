using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace WindowSystem
{
    /// <summary>
    /// contains code copied from Common since we don't have a reference...
    /// </summary>
    public static class Util
    {

        /// <summary>
        /// creates the collection if not instantiated, and adds the element
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        public static void AddToList<T>(ref List<T> list, T value)
        {
            if (list == null)
                list = new List<T>();

            list.Add(value);

        }


        public static int Clamp(int f1, int bottom, int top)
        {
            return (f1 > bottom ? (f1 < top ? f1 : top) : bottom);
        }

        public static float Clamp(float f1, float bottom, float top)
        {
            return (f1 > bottom ? (f1 < top ? f1 : top) : bottom);
        }

        public static Color? ColorFromHex(this string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);

          //  uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            uint hex;
            if (uint.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hex))
            {
                Color color = Color.White;

                if (hexString.Length == 8)
                {
                    color.A = (byte)(hex >> 24);
                    color.R = (byte)(hex >> 16);
                    color.G = (byte)(hex >> 8);
                    color.B = (byte)(hex);
                }
                else if (hexString.Length == 6)
                {
                    color.R = (byte)(hex >> 16);
                    color.G = (byte)(hex >> 8);
                    color.B = (byte)(hex);
                }
                else
                {
                    throw new InvalidOperationException("Invalid hex representation of an ARGB or RGB color value.");
                }  
                
                return color;
            }

            return null;
          
        }

        public static string ToHex(this Color color)
        {
            return String.Format("#{0}{1}{2}"              
                , color.R.ToString("X").Length == 1 ? String.Format("0{0}", color.R.ToString("X")) : color.R.ToString("X")
                , color.G.ToString("X").Length == 1 ? String.Format("0{0}", color.G.ToString("X")) : color.G.ToString("X")
                , color.B.ToString("X").Length == 1 ? String.Format("0{0}", color.B.ToString("X")) : color.B.ToString("X"));
        }

      /*  public static string ToHex(this Color color)
        {
            return String.Format("#{0}{1}{2}{3}"
                , color.A.ToString("X").Length == 1 ? String.Format("0{0}", color.A.ToString("X")) : color.A.ToString("X")
                , color.R.ToString("X").Length == 1 ? String.Format("0{0}", color.R.ToString("X")) : color.R.ToString("X")
                , color.G.ToString("X").Length == 1 ? String.Format("0{0}", color.G.ToString("X")) : color.G.ToString("X")
                , color.B.ToString("X").Length == 1 ? String.Format("0{0}", color.B.ToString("X")) : color.B.ToString("X"));
        }*/

        /*
        public static string ColorToHex(this string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);

            uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            Color color = Color.White;

            if (hexString.Length == 8)
            {
                color.A = (byte)(hex >> 24);
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else if (hexString.Length == 6)
            {
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else
            {
                throw new InvalidOperationException("Invalid hex representation of an ARGB or RGB color value.");
            }

            return color;
        }*/

    }
}
