using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide
{
    /// <summary>
    /// use this interface for classes that should be loaded and referred to in general lists in GameData
    /// </summary>
    public interface IGameData
    {
        /// <summary>
        /// naming convention: camelCase
        /// in EntityType: use prefixes such as "structure:", "item:" etc.
        /// </summary>
        string KeyName{ get; }
        string Name { get; }
        
        bool DeleteRecord { get; }

        void PreInitValidate(ref List<string> errors);

        /// <summary>
        /// gets called automatically on types that are added to an All... dictionary.
        /// 
        /// Don't do type lookups here. Not all game data types will have been added/deleted at this point
        /// </summary>
        void Initialize();

        void PostInitValidate(ref List<string> errors);



        void PreDataCompleteValidate(ref List<string> listOfErrors);
        void PostDataCompleteInitialize();
        void PostDataCompleteValidate(ref List<string> listOfErrors);
    }
}
