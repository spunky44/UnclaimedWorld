using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide
{
    /// <summary>
    /// use this for highscores, hint progress etc.
    /// 
    /// Sync with Steam???
    /// </summary>
    public class Progress
    {
        public const string FileName = "Progress.xml";

       // public List<string> DisplayedHints;
        public SerializableHashSet<string> DisplayedHints = new SerializableHashSet<string>();


        public void Write()
        {
            BaseDataLoader.SerializeObject(this, "", FileName, Config.DataType.UserSettings);
        }
    }
}
