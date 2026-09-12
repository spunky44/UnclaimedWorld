using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AI
{
    public class MemoryModelRenderData
    {
        private static Pool<MemoryModelRenderData> memoryModelRenderDataPool = new Pool<MemoryModelRenderData>(60);

        public float? VehiclePitch;

        public float? VehicleRoll;

        public Vector3? FacingDirection;


        public static MemoryModelRenderData Get(Vector3 facingDirection, float? vehicleRoll, float? vehiclePitch) 
        {
            MemoryModelRenderData memoryModelRenderData = memoryModelRenderDataPool.Get();

            memoryModelRenderData.Init(facingDirection, vehicleRoll, vehiclePitch);

            return memoryModelRenderData;
        }

        public void Retire()
        {

            memoryModelRenderDataPool.Retire(this);
        }

        public void Init(Vector3 facingDirection, float? vehicleRoll, float? vehiclePitch)
        {
            FacingDirection = facingDirection;
            VehicleRoll = vehicleRoll;
            VehiclePitch = vehiclePitch;
        }
    }
}
