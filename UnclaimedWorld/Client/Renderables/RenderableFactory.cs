using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;

namespace UWGame.ClientSide.Renderables
{
    // FACTORY /////////////////////////////////////////////////////////////
    public class RenderableFactory
    {
        //public members
       /* public static bool isProducing()
        {
            return producing;
        }*/

      /*  public static bool isRemoving()
        {
            return removing;
        }*/

      /*  public Renderable FindRenderableByID(ulong id)
        {
            return FindRenderableByID((RenderableID)id);
        }



        public static Renderable FindRenderableByID(RenderableID id)
        {
            if (id == RenderableID.Invalid)
                return null;

            Renderable val;
            if (collection.TryGetValue(id, out val))
                return val;

            return null;
        }*/

        public static void Remove(Renderable e)
        {
           /* removing = true;
            collection.Remove(e.ID);
            removing = false;
            */
            /*if (e.Entity == null)
            {*/
                The.Client.RemoveRenderable(e);
           // }

            /*
            producing = true; // not really, just permitting the setting of ID here
            e.ID = RenderableID.Invalid;
            producing = false;*/
        }

        /// <summary>
        /// for new Entity renderables.
        /// remember to call Initialize() on the renderable after...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Renderable Produce(Entity entity, RenderableType type, Renderable.SnapshotRenderable snapshotRenderable = null, double? lifetimeInSeconds = null)
        {
            //producing = true;

            Renderable renderable = new Renderable(entity, type);

            InitRenderable(lifetimeInSeconds, renderable, snapshotRenderable);   

           // producing = false;
            return renderable;
        }

        /// <summary>
        /// for carcass
        /// </summary>
        /// <param name="original"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Renderable Produce(Renderable original, Entity entity)
        {
          //  producing = true;

            Renderable renderable = new Renderable(original, entity);

            InitRenderable(null, renderable);

         //   producing = false;
            return renderable;
        }

       

        /// <summary>
        /// for memory fact
        /// </summary>
        /// <param name="original"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Renderable Produce(Renderable original, MemoryFact memoryFact)
        {
            //producing = true;

            Renderable renderable = new Renderable(original, memoryFact);

            InitRenderable(null, renderable);

         //   producing = false;
            return renderable;
        }


        /// <summary>
        /// for loaded memoryfact
        /// </summary>
        /// <param name="snapshotRenderable"></param>
        /// <param name="memoryFact"></param>
        /// <returns></returns>
        public static Renderable Produce(Renderable.SnapshotRenderable snapshotRenderable, MemoryFact memoryFact)
        {
           // producing = true;

            Renderable renderable = new Renderable(memoryFact);

            InitRenderable(null, renderable, snapshotRenderable);

          //  producing = false;
            return renderable;
        }


        private static void InitRenderable(double? lifetimeInSeconds, Renderable renderable, Renderable.SnapshotRenderable snapshotRenderable = null)
        {
            /*AssignUniqueID(renderable);

            if (renderable.ID != RenderableID.Invalid)
                collection.Add(renderable.ID, renderable);*/

            if (snapshotRenderable != null)
            {
                snapshotRenderable.LoadRenderableWithSnapshotData(renderable);
            }

            if (lifetimeInSeconds.HasValue)
            {
                // set the time for death:
                renderable.ExpiryTimePointInSeconds = UpdateTimePoints.ComputeTimePointFromInterval(lifetimeInSeconds.Value);
            }

            bool intervalChanged;
            renderable.RecomputeUpdateInterval(out intervalChanged); // for emitter renderables, this call is enough... for entity renderables, we need to call it again after Initialize, because thats where RenderAsModel gets created

            The.Client.AddRenderable(renderable); // hook up so it may receive updates
        }


       /* public static void Clear()
        {
            collection.Clear();
            ResetIDCounter();
        }*/

       /* public static void ResetIDCounter()
        {
            idCounter = (RenderableID)1;
        }*/

        //private members
      //  private static bool producing = false;
       // private static bool removing = false;
      //  private static Dictionary<RenderableID, Renderable> collection = new Dictionary<RenderableID, Renderable>();
     //   private static RenderableID idCounter = (RenderableID)1;
      /*  private static void AssignUniqueID(Renderable e)
        {
            e.ID = idCounter;

            idCounter++;

            if (idCounter >= RenderableID.Max)
            {
                throw new Exception("Astounding, RenderableID just exceeded 64 bits. Something seriously wrong has happened.");
            }
        }*/

    }

}
