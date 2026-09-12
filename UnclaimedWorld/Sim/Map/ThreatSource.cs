using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using System.Xml;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.Maps
{
    public class ThreatSource
    {
        public Point MapPosition;
        public Rectangle ThreatArea;

        public int Radius;
        public float LifeTime;

        private float elapsedTime = 0f;
        public byte[,] ThreatMapping;

        public byte MaxValue = 255;
        public float PropagationConstant = 0.8f;

        public bool IsTransient;

        public Entity EntitySource;

        public ThreatSource(Point pos, int radius, float lifetime)
        {
            MapPosition = pos;
            Radius = radius;
            LifeTime = lifetime;
            int minX, maxX, minY, maxY;

            ThreatArea = MapManager.GetClampedMapAreaUsingTiles(new TilePos(pos.X, pos.Y), radius, out minX, out maxX, out minY, out maxY);

          /*  int minX = Math.Max(0, MapPosition.X - Radius);
            int minY = Math.Max(0, MapPosition.Y - Radius);
            int maxX = Math.Min(UWGame.SimSide.Instance.map.TileMap.GetLength(0), MapPosition.X + Radius);
            int maxY = Math.Min(UWGame.SimSide.Instance.map.TileMap.GetLength(1), MapPosition.Y + Radius);

            ThreatArea = new Rectangle(minX, minY, maxX - minX, maxY - minY);*/

            ThreatMapping = new byte[maxX - minX, maxY - minY];

            int dist;
            double res;
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {   // 0.8 to the power of DistManhattan:
                    dist = (Math.Abs(x - MapPosition.X) + Math.Abs(y - MapPosition.Y));
                    res = MaxValue * Math.Pow(PropagationConstant, dist);
                    ThreatMapping[x - minX, y - minY] = /*(byte)(ThreatMapping[x, y] +*/ (byte)Common.ClampTop(res, 255);
                }
            }
        }


        public ThreatSource()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

        public void Update(GameTime elapsed)
        {
            elapsedTime += (float)elapsed.ElapsedGameTime.TotalSeconds;


        }

        public float Decay()
        {
            return Common.ClampBottom((LifeTime - elapsedTime) / LifeTime, 0f);
        }

        public bool IsExpired()
        {
            return elapsedTime > LifeTime;
        }

    }
}
