using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Body
{
    /// <summary>
    /// this interface enables tree methods on both Body and BodyPart
    /// </summary>
    public interface IHasBodyParts
    {
        List<BodyPart> BodyParts { get; set; }
       
    }
}
