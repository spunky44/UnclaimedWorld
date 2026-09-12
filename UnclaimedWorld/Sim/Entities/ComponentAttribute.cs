using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{

    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : System.Attribute // Lars: what is this class for..?
    {
        public static int Counter { get; set; }

        private static int counter;

      /*  public string Topic               // Topic is a named parameter
        {
            get
            {
                return topic;
            }
            set
            {

                topic = value;
            }
        }*/



        public ComponentAttribute()  // url is a positional parameter
        {

        }

        static ComponentAttribute()
        {
            Counter++;
        }

    }
}
