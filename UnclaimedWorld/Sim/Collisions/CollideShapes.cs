using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Collisions
{

    public enum CollidePrim
    {
        Rectangle,
        Circle
    }

    /// <summary>
    /// base class for all collision primitives
    /// also stores a collection of primitives
    /// includes an axis aligned bounding rectangle
    /// all primitive shapes are inside bounding rectangle
    /// 
    /// can this class be made generic so it can be used together with Trigger..?
    /// </summary>
    public class CollideShape2D //: ISnapshot
    {

        private CollidePrim primitiveType = CollidePrim.Rectangle;
        public CollidePrim PrimitiveType
        {
            get
            {
                return primitiveType;
            }
        }

      

        /// <summary>
        /// why couple this class to Entity...?
        /// </summary>
        [XmlIgnore] // Xml???
        public Collidable<Entity> Parent
        {
            get;
            set;
        }



        private Vector2 boundsUpperLeft;
        private Vector2 boundsLowerRight;

        public Vector2 BoundsUpperLeft
        {
            get
            {
                return boundsUpperLeft;
            }
            set
            {
                boundsUpperLeft = value;
                UpdateCenterAndRadius();
            }
        }

        public Vector2 BoundsUpperRight
        {
            get
            {
                return new Vector2(boundsLowerRight.X, boundsUpperLeft.Y);
            }
            set
            {
                boundsLowerRight.X = value.X;
                boundsUpperLeft.Y = value.Y;
                UpdateCenterAndRadius();
            }
        }

        public Vector2 BoundsLowerRight
        {
            get
            {
                return boundsLowerRight;
            }
            set
            {
                boundsLowerRight = value;
                UpdateCenterAndRadius();
            }
        }

        public Vector2 BoundsLowerLeft
        {
            get
            {
                return new Vector2(boundsUpperLeft.X, boundsLowerRight.Y);
            }
            set
            {
                boundsUpperLeft.X = value.X;
                boundsLowerRight.Y = value.Y;
                UpdateCenterAndRadius();
            }
        }

        public float BoundsTop
        {
            get
            {
                return BoundsUpperLeft.Y;
            }
            set
            {
                boundsUpperLeft.Y = value;
                UpdateCenterAndRadius();
            }
        }

        public float BoundsLeft
        {
            get
            {
                return BoundsUpperLeft.X;
            }
            set
            {
                boundsUpperLeft.X = value;
                UpdateCenterAndRadius();
            }
        }

        public float BoundsBottom
        {
            get
            {
                return BoundsLowerRight.Y;
            }
            set
            {
                boundsLowerRight.Y = value;
                UpdateCenterAndRadius();
            }
        }

        public float BoundsRight
        {
            get
            {
                return BoundsLowerRight.X;
            }
            set
            {
                boundsLowerRight.X = value;
                UpdateCenterAndRadius();
            }
        }

        public float BoundsWidth
        {
            get
            {
                return boundsLowerRight.X - boundsUpperLeft.X;
            }
        }

        public float BoundsHeight
        {
            get
            {
                return boundsLowerRight.Y - boundsUpperLeft.Y;
            }
        }

        private Vector2 offset = Vector2.Zero;

        /// <summary>
        /// set this to offset the shape
        /// </summary>
        public Vector2 Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
                UpdateCenterAndRadius();
            }
        }

        private bool hFlipped = false;
        public bool HFlipped
        {
            get
            {
                return hFlipped;
            }
        }
        public void FlipHorizontally()
        {
            offset.X *= -1f;
            hFlipped = !hFlipped;

            UpdateCenterAndRadius();
        }

        /// <summary>
        /// for serializer
        /// </summary>
        public CollideShape2D()
        { }

        /// <summary>
        /// ctor  for rectangles
        /// </summary>
        public CollideShape2D(Vector2 topleft, Vector2 bottomright)
        {
            primitiveType = CollidePrim.Rectangle;
            boundsUpperLeft = topleft;
            boundsLowerRight = bottomright;
            Offset = Vector2.Zero;

            UpdateCenterAndRadius();
        }

        /// <summary>
        /// ctor  for rectangles
        /// </summary>
        public CollideShape2D(float top, float left, float bottom, float right)
        {
            primitiveType = CollidePrim.Rectangle;
            boundsUpperLeft = new Vector2(left, top);
            boundsLowerRight = new Vector2(right, bottom);
            Offset = Vector2.Zero;

            UpdateCenterAndRadius();
        }

        /// <summary>
        /// ctor for circles,
        /// is 13 the minimum for radius to block an area?
        /// 
        /// ALWAYS set center to (0,0)!
        /// </summary>
        public CollideShape2D(Vector2 center, float radius)
        {
            primitiveType = CollidePrim.Circle;
            boundsUpperLeft = new Vector2(center.X - radius, center.Y - radius);
            boundsLowerRight = new Vector2(center.X + radius, center.Y + radius);
            Offset = Vector2.Zero;

            UpdateCenterAndRadius();
        }

        public void BeCircle()
        {
            primitiveType = CollidePrim.Circle;
        }

        public float GetWidthHeightRatio()
        {
            return Math.Abs(boundsUpperLeft.X - boundsLowerRight.X) / Math.Abs(boundsUpperLeft.Y - boundsLowerRight.Y);
        }

        public float GetWidth()
        {
            return Math.Abs(boundsUpperLeft.X - boundsLowerRight.X);
        }


        public float GetHeight()
        {
            return Math.Abs(boundsUpperLeft.Y - boundsLowerRight.Y);
        }


        public void AdjustShape(Vector2 offset, float? radius, float? width, float? height)
        {
            //  primitiveType = CollidePrim.Circle;
            Vector2 center = Center;//local from property, to avoid diagonal creep
            if (primitiveType == CollidePrim.Circle)
            {
                boundsUpperLeft = new Vector2(center.X - radius.Value, center.Y - radius.Value);
                boundsLowerRight = new Vector2(center.X + radius.Value, center.Y + radius.Value);
            }
            else if (primitiveType == CollidePrim.Rectangle)
            {
                // float ratio = GetWidthHeightRatio();

                if (width.HasValue)
                {
                    // adjust width
                    boundsUpperLeft.X = center.X - 0.5f * width.Value;
                    boundsLowerRight.X = center.X + 0.5f * width.Value;

                }
                else if (height.HasValue)
                {   // adjust height

                    boundsUpperLeft.Y = center.Y - 0.5f * height.Value;
                    boundsLowerRight.Y = center.Y + 0.5f * height.Value;

                    /* float radiusChange = radius - this.Radius;

                     boundsUpperLeft.X -= radiusChange * ratio;
                     boundsLowerRight.X += radiusChange * ratio;

                     boundsUpperLeft.Y -= radiusChange / ratio;
                     boundsLowerRight.Y += radiusChange / ratio;*/
                }
            }

            Offset = offset;

            UpdateCenterAndRadius();
        }


        /// <summary>
        /// Copy ctor
        /// </summary>
        /// <param name="other"></param>
        public CollideShape2D(CollideShape2D other)
        {
            primitiveType = other.PrimitiveType;
            boundsUpperLeft = other.BoundsUpperLeft;
            boundsLowerRight = other.BoundsLowerRight;
            Offset = other.Offset;

            UpdateCenterAndRadius();
        }



        /// <summary>
        /// Checks if this shape's bounding rectangle contains a point
        /// </summary>
        public bool ContainsPoint(Vector2 point)
        {
            Vector2 localCenter = Center;
            Vector2 localUL = boundsUpperLeft;
            Vector2 localLR = boundsLowerRight;

            if (Parent != null)//this is a child
            {
                localCenter += Parent.Center + offset;
                localUL += Parent.Center + offset;
                localLR += Parent.Center + offset;
            }


            if (!(localUL.X <= point.X && localLR.X >= point.X &&
                    localUL.Y <= point.Y && localLR.Y >= point.Y))
                return false;

            if (PrimitiveType == CollidePrim.Rectangle)
            {
                return true;
            }

            if ((point - localCenter).LengthSquared() < Radius * Radius)
                return true;



            return false;
        }

        /// <summary>
        /// Does only a bounding rectangle intersection
        /// </summary>
        public bool isBoundsOverlap(CollideShape2D that)
        {
            return (!(this.BoundsBottom < that.BoundsTop ||
                       this.BoundsTop > that.BoundsBottom ||
                       this.BoundsRight < that.BoundsLeft ||
                       this.BoundsLeft > that.BoundsRight));
        }



        public float GetShapeOverlapAmount(CollideShape2D that)
        {
            if (!isBoundsOverlap(that))
                return 0; // no shapes overlap if their bounds do not


            if (this.primitiveType == CollidePrim.Rectangle &&
                that.PrimitiveType == CollidePrim.Rectangle)
                return 1; // a rectangle is a bounds is a rectangle //TODO returning 1 is a hack

            //we're both circles, that is easy
            if (this.primitiveType == CollidePrim.Circle &&
                that.PrimitiveType == CollidePrim.Circle)
            {


                float dist = Common.RoughVectorMagnitude((this.Center - that.Center).ToVector3());
                return (this.Radius + that.Radius) - dist;

                ////measure the distsqr between the centers
                //float distSqr = Vector2.DistanceSquared(this.Center, that.Center);

                ////is it less than the sum of the radii squared?
                //return  (this.RadiusSquared + that.RadiusSquared) - distSqr;
            }

            //if one of us is a circle and the other a rectangle then the test is more complex
            //detect a success case here and return true


            CollideShape2D circ = that;
            CollideShape2D rect = this;
            if (this.primitiveType == CollidePrim.Circle)
            {
                circ = this;
                rect = that;
            }

            if (rect.primitiveType != CollidePrim.Rectangle)
                throw new Exception("oops, flawed logic. the other rect is not a rectangle after all.");
            if (circ.primitiveType != CollidePrim.Circle)
                throw new Exception("oops, flawed logic. the other circ is not a circle after all.");

            //1. (l < cx < r) AND (t - cr < cy < b + cr) then true
            if ((rect.BoundsLeft < circ.Center.X && circ.Center.X < rect.BoundsRight) &&
                (rect.BoundsTop - circ.Radius < circ.Center.Y && circ.Center.Y < rect.BoundsBottom + circ.Radius))
                return 1;//TODO returning 1 is a hack

            //2. (l - cr < cx < r + cr) AND (t < cy < b) then 
            if ((rect.BoundsTop < circ.Center.Y && circ.Center.Y < rect.BoundsBottom) &&
                (rect.BoundsLeft - circ.Radius < circ.Center.X && circ.Center.X < rect.BoundsRight + circ.Radius))
                return 1; //TODO returning 1 is a hack

            //3. Any one of the rectangle's corners lies inside the circle, then true
            if (that.ContainsPoint(this.boundsUpperLeft) ||
                that.ContainsPoint(this.BoundsUpperRight) ||
                that.ContainsPoint(this.boundsLowerRight) ||
                that.ContainsPoint(this.BoundsLowerLeft))
                return 1; //TODO returning 1 is a hack


            return 0;
        }


        private void UpdateCenterAndRadius()
        {
            center = (boundsUpperLeft + boundsLowerRight) * 0.5f;
            radius = (boundsLowerRight.X - boundsUpperLeft.X) * 0.5f;
            radiusSquared = radius * radius;
        }

        private Vector2 center = Vector2.Zero;
        private float radius = 0;
        private float radiusSquared = 0;
        public Vector2 Center
        {
            get
            {
                return center;
            }
        }

        public float Radius
        {
            get
            {
                return radius;
            }
        }
        public float RadiusSquared
        {
            get
            {
                return radiusSquared;
            }
        }

    }



}
