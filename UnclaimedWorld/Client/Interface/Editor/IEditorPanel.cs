using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.ClientSide.Interface.Editor.MapTools;

namespace UWGame.ClientSide.Interface.Editor
{
    public interface IEditorPanel
    {

        void AffectMap(List<MapTool.SubTileAndChange> affectedSubtiles);

        void AffectMap(List<MapTool.TileAndChange> affectedTiles);


    }
}
