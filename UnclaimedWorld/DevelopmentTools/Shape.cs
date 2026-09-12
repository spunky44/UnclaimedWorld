using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kensei
{
    namespace Dev
    {
        /// <summary>
        /// Class for drawing very simple shapes on screen in 2D and 3D to aid debugging.
        /// </summary>
        public static class Shape
        {
            #region Class behaviour

            internal static void Initialise(ContentManager content, GraphicsDevice device)
            {
                // XXX this path should NOT be hardcoded at this level. But I leave it to the user to change
                // it if they need to. Ideally you would embed the shader as a resource in your project; see
                // http://blogs.msdn.com/shawnhar/archive/2007/06/12/embedding-content-as-resources.aspx.
                //s_effect = content.Load<Effect>("\\Content\\DebugShape");
                //s_effect = content.Load<Effect>("\\Content\\DevShape");
                effect = content.Load<Effect>("DevShape");
                //  s_vertexDeclaration = new VertexDeclaration( device, VertexPositionColor.VertexElements ); // XNA 3
            }

            /// <summary>
            /// Render all shapes added this frame
            /// </summary>
            /// <param name="device">Graphics device to render with</param>
            /// <param name="renderMatrix">World * View * Projection</param>
            /// <param name="width">Screen width</param>
            /// <param name="height">Screen height</param>
            internal static void Draw(GraphicsDevice device, Matrix renderMatrix, float width, float height)
            {
                if ((Kensei.Dev.Options.GetOption("Overlays.ShowPrimitiveCount"))
                    )
                {
                    int numPrimitives = s_line2DVertices.Count + s_line3DVertices.Count + s_triangle2DVertices.Count + s_triangle3DVertices.Count;
                    Kensei.Dev.DevText.Print("Drawing " + numPrimitives.ToString() + " Debug Primitives", Color.White);
                }


                //device.RenderState.CullMode = CullMode.None;
                device.RasterizerState = RasterizerState.CullNone;

                /* device.RenderState.AlphaBlendEnable = true; // XNA 3
                 device.RenderState.SourceBlend = Blend.SourceAlpha;
                 device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                 */

                device.BlendState = BlendState.AlphaBlend;

                // device.VertexDeclaration = s_vertexDeclaration;

                Render3DLines(device, renderMatrix);
                Render3DTriangles(device, renderMatrix);
                Render2DLines(device, width, height);
                Render2DTriangles(device, width, height);
            }

            #endregion

            // TODO I have noted some performance issues with this code when drawing very large
            // numbers of shapes, but have not had time to profile it and fit it up yet, sorry!

            #region 2D primitives

            /// <summary>
            /// 2D line (ie. in screen space).
            /// </summary>
            /// <param name="start">Line start point</param>
            /// <param name="end">Line end point</param>
            /// <param name="startColour">Line start colour</param>
            /// /// <param name="endColour">Line start colour</param>
            static public void Line(Vector2 start, Vector2 end, Color startColour, Color endColour)
            {
                // Add vertices... note that we keep them in their original format for now as we don't know the screen size
                s_line2DVertices.Add(new VertexPositionColor(new Vector3(start, 0), startColour));
                s_line2DVertices.Add(new VertexPositionColor(new Vector3(end, 0), endColour));
            }

            /// <summary>
            /// 2D line (ie. in screen space).
            /// </summary>
            /// <param name="start">Line start point</param>
            /// <param name="end">Line end point</param>
            /// <param name="colour">Line colour</param>
            static public void Line(Vector2 start, Vector2 end, Color colour)
            {
                Line(start, end, colour, colour);
            }

            /// <summary>
            /// 2D box (ie. in screen space).
            /// </summary>
            /// <param name="topLeft">One corner of the box (doesn't actually need to be top left)</param>
            /// <param name="bottomRight">Opposite corner of the box</param>
            /// <param name="colour">Colour for the box</param>
            static public void Box(Vector2 topLeft, Vector2 bottomRight, Color colour, bool solid)
            {
                Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
                Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);

                if (solid)
                {
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomLeft, 0), colour));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topRight, 0), colour));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topLeft, 0), colour));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topRight, 0), colour));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomLeft, 0), colour));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomRight, 0), colour));
                }
                else
                {
                    Line(topLeft, topRight, colour);
                    Line(topRight, bottomRight, colour);
                    Line(bottomRight, bottomLeft, colour);
                    Line(bottomLeft, topLeft, colour);
                }
            }

            /// <summary>
            /// 2D box (ie. in screen space).
            /// </summary>
            /// <param name="topLeft">One corner of the box (doesn't actually need to be top left)</param>
            /// <param name="bottomRight">Opposite corner of the box</param>
            /// <param name="colour">Colour for the box</param>
            static public void Box(Vector2 topLeft, Vector2 bottomRight,
                Color colour0, Color colour1, Color colour2, Color colour3, bool solid)
            {
                Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
                Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);

                if (solid)
                {
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomLeft, 0), colour2));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topRight, 0), colour1));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topLeft, 0), colour0));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topRight, 0), colour1));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomLeft, 0), colour2));
                    s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomRight, 0), colour3));
                }
                else
                {
                    Line(topLeft, topRight, colour1);
                    Line(topRight, bottomRight, colour3);
                    Line(bottomRight, bottomLeft, colour2);
                    Line(bottomLeft, topLeft, colour0);
                }
            }



            /// <summary>
            /// Triangle (in screen space).
            /// </summary>
            /// <param name="point1">Position for corner one.</param>
            /// <param name="colour1">Colour for corner one.</param>
            /// <param name="point2">Position for corner two.</param>
            /// <param name="colour2">Colour for corner two.</param>
            /// <param name="point3">Position for corner three.</param>
            /// <param name="colour3">Colour for corner three.</param>
            static public void Triangle(Vector2 point1, Color colour1, Vector2 point2, Color colour2, Vector2 point3, Color colour3)
            {
                s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(point1, 0), colour1));
                s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(point2, 0), colour2));
                s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(point3, 0), colour3));
            }

            /// <summary>
            /// Triangle (in screen space).
            /// </summary>
            /// <param name="point1">Position for corner one.</param>
            /// <param name="point2">Position for corner two.</param>
            /// <param name="point3">Position for corner three.</param>
            /// <param name="colour">Colour of triangle.</param>
            static public void Triangle(Vector2 point1, Vector2 point2, Vector2 point3, Color colour)
            {
                Triangle(point1, colour, point2, colour, point3, colour);
            }



            /// <summary>
            /// Circle (in screen space).
            /// </summary>
            /// <param name="center">Position for center.</param>
            /// <param name="radius">Distance from center to circle.</param>
            /// <param name="colour">Colour of triangle.</param>
            static public void Circle(Vector2 center, float radius, Color color, bool dashed = false)
            {
                float numSteps = 0.1f * (float)Math.PI * radius;
                float angleStep = 2f * (float)Math.PI / numSteps;

                if (angleStep > Math.PI / 6d)
                    angleStep = (float)(Math.PI / 6d); // clamp to a minimum of twelve segments per circle

                Vector2 from = center;
                Vector2 to = from;
                Vector2? first = null;
 
                for (float ang = 0; ang < Math.PI * 2f; ang += angleStep)
                {
                    to.X = center.X + (float)Math.Sin(ang) * radius;
                    to.Y = center.Y + (float)Math.Cos(ang) * radius;

                    if (first == null)
                        first = to;
                    else
                    {
                        if (dashed)
                            from = (from + to) * .5f;

                        Line(from, to, color);
                    }

                    from = to;
                }
                to = (Vector2)first;

                if (dashed)
                    from = (from + to) * .5f;

                Line(from, to, color);
            }

            #endregion

            #region 3D primitives

            /// <summary>
            /// 3D line ie. in worldspace. Z-buffered.
            /// </summary>
            /// <param name="start">Start point.</param>
            /// <param name="end">End point.</param>
            /// <param name="startColour">Start colour.</param>
            /// <param name="endColour">End colour.</param>
            static public void Line(Vector3 start, Vector3 end, Color startColour, Color endColour)
            {
                // Add vertices... note that we keep them in their original format for now as we don't know the screen size
                s_line3DVertices.Add(new VertexPositionColor(start, startColour));
                s_line3DVertices.Add(new VertexPositionColor(end, endColour));
            }

            /// <summary>
            /// 3D line ie. in worldspace. Z-buffered.
            /// </summary>
            /// <param name="start">Start point.</param>
            /// <param name="end">End point.</param>
            /// <param name="colour">Colour.</param>
            static public void Line(Vector3 start, Vector3 end, Color colour)
            {
                Line(start, end, colour, colour);
            }

            /// <summary>
            /// 3D line ie. in worldspace. Z-buffered.
            /// </summary>
            /// <param name="ray">Ray.</param>
            /// <param name="colour">Colour.</param>
            static public void Line(Ray ray, Color colour)
            {
                Line(ray.Position, ray.Position + ray.Direction, colour);
            }

            /// <summary>
            /// Triangle (in world space).
            /// </summary>
            /// <param name="point1">Position for corner one.</param>
            /// <param name="colour1">Colour for corner one.</param>
            /// <param name="point2">Position for corner two.</param>
            /// <param name="colour2">Colour for corner two.</param>
            /// <param name="point3">Position for corner three.</param>
            /// <param name="colour3">Colour for corner three.</param>
            static public void Triangle(Vector3 point1, Color colour1, Vector3 point2, Color colour2, Vector3 point3, Color colour3)
            {
                s_triangle3DVertices.Add(new VertexPositionColor(point1, colour1));
                s_triangle3DVertices.Add(new VertexPositionColor(point2, colour2));
                s_triangle3DVertices.Add(new VertexPositionColor(point3, colour3));
            }

            /// <summary>
            /// Triangle (in world space).
            /// </summary>
            /// <param name="point1">Position for corner one.</param>
            /// <param name="point2">Position for corner two.</param>
            /// <param name="point3">Position for corner three.</param>
            /// <param name="colour">Colour of triangle.</param>
            static public void Triangle(Vector3 point1, Vector3 point2, Vector3 point3, Color colour)
            {
                Triangle(point1, colour, point2, colour, point3, colour);
            }

            /// <summary>
            /// A cross (always axis aligned) at a specified point.
            /// </summary>
            /// <param name="position">The point to draw the cross at.</param>
            /// <param name="size">The size of the cross.</param>
            /// <param name="colour">The colour of the cross.</param>
            static public void Cross(Vector3 position, float size, Color colour)
            {
                Line(new Vector3(position.X - size, position.Y, position.Z), new Vector3(position.X + size, position.Y, position.Z), colour);
                Line(new Vector3(position.X, position.Y - size, position.Z), new Vector3(position.X, position.Y + size, position.Z), colour);
                Line(new Vector3(position.X, position.Y, position.Z - size), new Vector3(position.X, position.Y, position.Z + size), colour);
            }

            /// <summary>
            /// Point showing X, Y, and Z axes rotated
            /// </summary>
            /// <param name="position">Position to draw the axes</param>
            /// <param name="size">Size to draw the axes</param>
            /// <param name="rotation">How much to rotate the axes by</param>
            /// <param name="xColour">Colour of X axis</param>
            /// <param name="yColour">Colour of Y axis</param>
            /// <param name="zColour">Colour of Z axis</param>
            static public void Axes(Vector3 position, float size, Quaternion rotation, Color xColour, Color yColour, Color zColour)
            {
                Line(position, position + Vector3.Transform(Vector3.UnitX * size, rotation), xColour);
                Line(position, position + Vector3.Transform(Vector3.UnitY * size, rotation), yColour);
                Line(position, position + Vector3.Transform(Vector3.UnitZ * size, rotation), zColour);
            }

            /// <summary>
            /// Point showing X, Y, and Z axes rotated
            /// </summary>
            /// <param name="position">Position to draw the axes</param>
            /// <param name="size">Size to draw the axes</param>
            /// <param name="rotation">How much to rotate the axes by</param>
            static public void Axes(Vector3 position, float size, Quaternion rotation)
            {
                Axes(position, size, rotation, Color.Blue, Color.Red, Color.Green);
            }

            /// <summary>
            /// Point showing X, Y, and Z axes rotated
            /// </summary>
            /// <param name="position">Position to draw the axes</param>
            /// <param name="size">Size to draw the axes</param>
            static public void Axes(Vector3 position, float size)
            {
                Axes(position, size, Quaternion.Identity);
            }

            /// <summary>
            /// AAB in 3D space. Useful for drawing AABBs.
            /// </summary>
            /// <param name="min">One corner of box (need not actually be minimum).</param>
            /// <param name="max">Opposite corner of box.</param>
            /// <param name="colour">Colour of box.</param>
            /// <param name="solid">Should the box be solid, otherwise wireframe.</param>
            static public void Box(Vector3 min, Vector3 max, Color colour, bool solid)
            {
                Vector3 minXY = new Vector3(min.X, min.Y, max.Z);
                Vector3 minXZ = new Vector3(min.X, max.Y, min.Z);
                Vector3 minYZ = new Vector3(max.X, min.Y, min.Z);
                Vector3 minX = new Vector3(min.X, max.Y, max.Z);
                Vector3 minY = new Vector3(max.X, min.Y, max.Z);
                Vector3 minZ = new Vector3(max.X, max.Y, min.Z);

                if (solid)
                {
                    Triangle(min, minY, minXY, colour);
                    Triangle(min, minY, minYZ, colour);
                    Triangle(min, minX, minXY, colour);
                    Triangle(min, minX, minXZ, colour);
                    Triangle(min, minZ, minYZ, colour);
                    Triangle(min, minZ, minXZ, colour);
                    Triangle(max, minYZ, minZ, colour);
                    Triangle(max, minYZ, minY, colour);
                    Triangle(max, minXY, minX, colour);
                    Triangle(max, minXY, minY, colour);
                    Triangle(max, minXZ, minX, colour);
                    Triangle(max, minXZ, minZ, colour);
                }
                else
                {
                    Line(min, minXY, colour);
                    Line(minXY, minY, colour);
                    Line(minY, minYZ, colour);
                    Line(minYZ, min, colour);
                    Line(min, minXZ, colour);
                    Line(minXY, minX, colour);
                    Line(minY, max, colour);
                    Line(minYZ, minZ, colour);
                    Line(max, minZ, colour);
                    Line(minZ, minXZ, colour);
                    Line(minXZ, minX, colour);
                    Line(minX, max, colour);
                }
            }

            /// <summary>
            /// A non-axis-aligned box, centred at a point and rotated by a quaternion. Useful for drawing OBBs.
            /// </summary>
            /// <param name="centre">The centre of the box.</param>
            /// <param name="size">The size of the box.</param>
            /// <param name="rotation">How the box should be rotated.</param>
            /// <param name="colour">The colour of the box.</param>
            static public void Box(Vector3 centre, Vector3 size, Quaternion rotation, Color colour, bool solid)
            {
                Vector3 min = centre + Vector3.Transform(-size, rotation);
                Vector3 minXY = centre + Vector3.Transform(new Vector3(-size.X, -size.Y, size.Z), rotation);
                Vector3 minXZ = centre + Vector3.Transform(new Vector3(-size.X, size.Y, -size.Z), rotation);
                Vector3 minYZ = centre + Vector3.Transform(new Vector3(size.X, -size.Y, -size.Z), rotation);
                Vector3 minX = centre + Vector3.Transform(new Vector3(-size.X, size.Y, size.Z), rotation);
                Vector3 minY = centre + Vector3.Transform(new Vector3(size.X, -size.Y, size.Z), rotation);
                Vector3 minZ = centre + Vector3.Transform(new Vector3(size.X, size.Y, -size.Z), rotation);
                Vector3 max = centre + Vector3.Transform(size, rotation);

                // NOTE once the vertices have been calculated, this version of Box shares all the same drawing
                // code as Box above (and indeed frustum!), should probably factor it out into a function

                if (solid)
                {
                    Triangle(min, minY, minXY, colour);
                    Triangle(min, minY, minYZ, colour);
                    Triangle(min, minX, minXY, colour);
                    Triangle(min, minX, minXZ, colour);
                    Triangle(min, minZ, minYZ, colour);
                    Triangle(min, minZ, minXZ, colour);
                    Triangle(max, minYZ, minZ, colour);
                    Triangle(max, minYZ, minY, colour);
                    Triangle(max, minXY, minX, colour);
                    Triangle(max, minXY, minY, colour);
                    Triangle(max, minXZ, minX, colour);
                    Triangle(max, minXZ, minZ, colour);
                }
                else
                {
                    Line(min, minXY, colour);
                    Line(minXY, minY, colour);
                    Line(minY, minYZ, colour);
                    Line(minYZ, min, colour);
                    Line(min, minXZ, colour);
                    Line(minXY, minX, colour);
                    Line(minY, max, colour);
                    Line(minYZ, minZ, colour);
                    Line(max, minZ, colour);
                    Line(minZ, minXZ, colour);
                    Line(minXZ, minX, colour);
                    Line(minX, max, colour);
                }
            }

            /// <summary>
            /// AABB in 3D space.
            /// </summary>
            /// <param name="box">AABB.</param>
            /// <param name="colour">Colour.</param>
            /// <param name="solid">Solid or wireframe.</param>
            static public void Box(BoundingBox boundingBox, Color colour, bool solid)
            {
                Box(boundingBox.Min, boundingBox.Max, colour, solid);
            }

            /// <summary>
            /// Draws a frustum in world space, using the same values as are likely to be easily obtained from a camera view.
            /// </summary>
            /// <param name="focalPoint">The tip of the pyramid (camera position).</param>
            /// <param name="rotation">Rotation of (0,0,1) toward the base of the pyramid (camera direction).</param>
            /// <param name="verticalAngle">Angle between the top and bottom faces (field of view Y).</param>
            /// <param name="aspectRatio">Angle between the left and right faces (aspect ratio).</param>
            /// <param name="nearClip">Distance to top of frustum (near clip distance).</param>
            /// <param name="farClip">Distance to base of pyramid (far clip frustum).</param>
            /// <param name="colour">Colour of frustum.</param>
            /// <param name="solid">Draw solid, otherwise wire frame.</param>
            static public void Frustum(Vector3 focalPoint, float yaw, float pitch, float roll, float verticalAngle, float aspectRatio, float nearClip, float farClip, Color colour, bool solid)
            {
                // TBD would any overloads be useful here? Rotated by Quaternion instead 
                // of PYR, and with target/look point instead of rotation?
                // This is probably not a very pretty way of drawing it!
                // And there's lots of calculations that could be cached here too

                // First construct the frustum facing along the Z-axis.
                float horizontalHalfAngle = (float)Math.Atan((float)Math.Tan(verticalAngle * 0.5f) * aspectRatio);

                float planeHeight = nearClip * (float)Math.Tan(verticalAngle * 0.5f);
                float planeWidth = nearClip * (float)Math.Tan(horizontalHalfAngle);
                Vector3 nearTopLeft = new Vector3(-planeWidth, planeHeight, nearClip);
                Vector3 nearTopRight = new Vector3(planeWidth, planeHeight, nearClip);
                Vector3 nearBottomLeft = new Vector3(-planeWidth, -planeHeight, nearClip);
                Vector3 nearBottomRight = new Vector3(planeWidth, -planeHeight, nearClip);

                planeHeight = farClip * (float)Math.Tan(verticalAngle * 0.5f);
                planeWidth = farClip * (float)Math.Tan(horizontalHalfAngle);
                Vector3 farTopLeft = new Vector3(-planeWidth, planeHeight, farClip);
                Vector3 farTopRight = new Vector3(planeWidth, planeHeight, farClip);
                Vector3 farBottomLeft = new Vector3(-planeWidth, -planeHeight, farClip);
                Vector3 farBottomRight = new Vector3(planeWidth, -planeHeight, farClip);

                // Then transform the frustum to match the input orientation and position
                Quaternion rotate = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                nearTopLeft = Vector3.Transform(nearTopLeft, rotate) + focalPoint;
                nearTopRight = Vector3.Transform(nearTopRight, rotate) + focalPoint;
                nearBottomLeft = Vector3.Transform(nearBottomLeft, rotate) + focalPoint;
                nearBottomRight = Vector3.Transform(nearBottomRight, rotate) + focalPoint;
                farTopLeft = Vector3.Transform(farTopLeft, rotate) + focalPoint;
                farTopRight = Vector3.Transform(farTopRight, rotate) + focalPoint;
                farBottomLeft = Vector3.Transform(farBottomLeft, rotate) + focalPoint;
                farBottomRight = Vector3.Transform(farBottomRight, rotate) + focalPoint;

                // Finally, draw the calculated points
                // TODO separate this code into a private function, it's identical to Box
                if (solid)
                {
                    Triangle(nearBottomLeft, farBottomRight, farBottomLeft, colour);
                    Triangle(nearBottomLeft, farBottomRight, nearBottomRight, colour);
                    Triangle(nearBottomLeft, farTopLeft, farBottomLeft, colour);
                    Triangle(nearBottomLeft, farTopLeft, nearTopLeft, colour);
                    Triangle(nearBottomLeft, nearTopRight, nearBottomRight, colour);
                    Triangle(nearBottomLeft, nearTopRight, nearTopLeft, colour);
                    Triangle(farTopRight, nearBottomRight, nearTopRight, colour);
                    Triangle(farTopRight, nearBottomRight, farBottomRight, colour);
                    Triangle(farTopRight, farBottomLeft, farTopLeft, colour);
                    Triangle(farTopRight, farBottomLeft, farBottomRight, colour);
                    Triangle(farTopRight, nearTopLeft, farTopLeft, colour);
                    Triangle(farTopRight, nearTopLeft, nearTopRight, colour);
                }
                else
                {
                    Line(nearTopLeft, nearTopRight, colour);
                    Line(nearTopRight, nearBottomRight, colour);
                    Line(nearBottomRight, nearBottomLeft, colour);
                    Line(nearBottomLeft, nearTopLeft, colour);
                    Line(nearTopLeft, farTopLeft, colour);
                    Line(nearTopRight, farTopRight, colour);
                    Line(nearBottomRight, farBottomRight, colour);
                    Line(nearBottomLeft, farBottomLeft, colour);
                    Line(farTopLeft, farTopRight, colour);
                    Line(farTopRight, farBottomRight, colour);
                    Line(farBottomRight, farBottomLeft, colour);
                    Line(farBottomLeft, farTopLeft, colour);
                }
            }

            /// <summary>
            /// Draws a frustum in world space.
            /// </summary>
            /// <param name="frustum">The frustum.</param>
            /// <param name="colour">Colour.</param>
            /// <param name="solid">Solid or wireframe.</param>
            static public void Frustum(BoundingFrustum boundingFrustum, Color colour, bool solid)
            {
                Vector3[] corners = boundingFrustum.GetCorners();	// XXX GC!

                if (solid)
                {
                    Triangle(corners[3], corners[6], corners[7], colour);
                    Triangle(corners[3], corners[6], corners[2], colour);
                    Triangle(corners[3], corners[4], corners[7], colour);
                    Triangle(corners[3], corners[4], corners[0], colour);
                    Triangle(corners[3], corners[1], corners[2], colour);
                    Triangle(corners[3], corners[1], corners[0], colour);
                    Triangle(corners[5], corners[2], corners[1], colour);
                    Triangle(corners[5], corners[2], corners[6], colour);
                    Triangle(corners[5], corners[7], corners[4], colour);
                    Triangle(corners[5], corners[7], corners[6], colour);
                    Triangle(corners[5], corners[0], corners[4], colour);
                    Triangle(corners[5], corners[0], corners[1], colour);
                }
                else
                {
                    Line(corners[0], corners[1], colour);
                    Line(corners[1], corners[2], colour);
                    Line(corners[2], corners[3], colour);
                    Line(corners[3], corners[0], colour);
                    Line(corners[0], corners[4], colour);
                    Line(corners[1], corners[5], colour);
                    Line(corners[2], corners[6], colour);
                    Line(corners[3], corners[7], colour);
                    Line(corners[4], corners[5], colour);
                    Line(corners[5], corners[6], colour);
                    Line(corners[6], corners[7], colour);
                    Line(corners[7], corners[4], colour);
                }
            }

            /// <summary>
            /// Draws a sphere in 3D space.
            /// </summary>
            /// <param name="centre">Position of the centre of the sphere.</param>
            /// <param name="size">Radius of the sphere.</param>
            /// <param name="colour">Colour of the sphere.</param>
            /// <param name="solid">Should the sphere be solid, otherwise wireframe.</param>
            /// <param name="numSegments">How many segments (vertical lines).</param>
            /// <param name="numSlices">How many slices (horizontal lines).</param>
            static public void Sphere(Vector3 centre, float radius, Color colour, bool solid, int numSegments, int numSlices)
            {
                int numRequiredVertices = numSegments * (numSlices + 1);

                // Avoid GC by avoiding creating a new array each frame
                if (numRequiredVertices > m_numSphereVertices)
                {
                    m_sphereVertices = new Vector3[numRequiredVertices];
                    m_numSphereVertices = numRequiredVertices;
                }

                // Work out the vertex positions for one slice (horizontal divisor) at a time...
                for (int slice = 0; slice <= numSlices; ++slice)
                {
                    float sliceAngle = ((float)Math.PI * slice) / numSlices;
                    float sliceRadius = radius * (float)Math.Sin(sliceAngle);
                    float vertexY = centre.Y + (radius * (float)Math.Cos(sliceAngle));

                    // ...and one segment at a time
                    for (int segment = 0; segment < numSegments; ++segment)
                    {
                        int index = slice * numSegments + segment;
                        float segmentAngle = ((float)Math.PI * 2 * segment) / numSegments;

                        m_sphereVertices[index].X = centre.X + (sliceRadius * (float)Math.Sin(segmentAngle));
                        m_sphereVertices[index].Y = vertexY;
                        m_sphereVertices[index].Z = centre.Z + (sliceRadius * (float)Math.Cos(segmentAngle));
                    }
                }

                if (solid)
                {
                    for (int segment = 0; segment < numSegments; ++segment)
                    {
                        for (int slice = 0; slice < numSlices; ++slice)
                        {
                            int topLeft = slice * numSegments + segment;
                            int bottomLeft = (slice + 1) * numSegments + segment;
                            int topRight = slice * numSegments + (segment + 1) % numSegments;
                            int bottomRight = (slice + 1) * numSegments + (segment + 1) % numSegments;

                            Triangle(m_sphereVertices[topLeft], m_sphereVertices[topRight], m_sphereVertices[bottomLeft], colour);
                            Triangle(m_sphereVertices[bottomRight], m_sphereVertices[topRight], m_sphereVertices[bottomLeft], colour);
                        }
                    }
                }
                else
                {
                    // Render the slices
                    for (int slice = 1; slice < numSlices; ++slice)
                    {
                        for (int segment = 0; segment < numSegments; ++segment)
                        {
                            int startVertex = slice * numSegments + segment;
                            int endVertex = slice * numSegments + (segment + 1) % numSegments;
                            Line(m_sphereVertices[startVertex], m_sphereVertices[endVertex], colour);
                        }
                    }

                    // Render the segments
                    for (int segment = 0; segment < numSegments; ++segment)
                    {
                        for (int slice = 0; slice < numSlices; ++slice)
                        {
                            int startVertex = slice * numSegments + segment;
                            int endVertex = (slice + 1) * numSegments + segment;
                            Line(m_sphereVertices[startVertex], m_sphereVertices[endVertex], colour);
                        }
                    }
                }
            }

            /// <summary>
            /// Draws a sphere in 3D space.
            /// </summary>
            /// <param name="centre">Position of the centre of the sphere.</param>
            /// <param name="size">Radius of the sphere.</param>
            /// <param name="colour">Colour of the sphere.</param>
            /// <param name="solid">Should the sphere be solid, otherwise wireframe.</param>
            static public void Sphere(Vector3 centre, float radius, Color colour, bool solid)
            {
                Sphere(centre, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
            }

            /// <summary>
            /// Draws a sphere in 3D space.
            /// </summary>
            /// <param name="sphere">The sphere.</param>
            /// <param name="colour">Colour of the sphere.</param>
            /// <param name="solid">Should the sphere be solid, otherwise wireframe.</param>
            static public void Sphere(BoundingSphere boundingSphere, Color colour, bool solid)
            {
                Sphere(boundingSphere.Center, boundingSphere.Radius, colour, solid);
            }

            /// <summary>
            /// Draws a cylinder in world space.
            /// </summary>
            /// <param name="start">The centre of one end of the cylinder.</param>
            /// <param name="end">The centre of the other end of the cylinder.</param>
            /// <param name="radius">The radius of the cylinder.</param>
            /// <param name="colour">The colour to draw the cylinder.</param>
            /// <param name="solid">Draw the cylinder solid, else wireframe.</param>
            /// <param name="numSegments">The number of segments to use to draw the cylinder.</param>
            static public void Cylinder(Vector3 start, Vector3 end, float radius, Color colour, bool solid, int numSegments)
            {
                // Avoid GC
                if (numSegments > m_numCylinderSegments)
                {
                    m_cylinderBaseVertices = new Vector3[numSegments];
                    m_cylinderTopVertices = new Vector3[numSegments];
                    m_numCylinderSegments = numSegments;
                }

                // Probably not the fastest way of doing it, but it seems like conceptually the easiest...
                // Work out the rotation for the cylinder from the Y-axis

                // Create a y-axis-aligned cylinder and rotate it into place
                Quaternion rotation = GetRotateFromYAxis(start, end);

                for (int i = 0; i < numSegments; ++i)
                {
                    float angle = ((float)Math.PI * 2 * i) / numSegments;

                    m_cylinderBaseVertices[i].X = radius * (float)Math.Sin(angle);
                    m_cylinderBaseVertices[i].Y = 0.0f;
                    m_cylinderBaseVertices[i].Z = radius * (float)Math.Cos(angle);

                    Vector3.Transform(ref m_cylinderBaseVertices[i], ref rotation, out m_cylinderBaseVertices[i]);
                    m_cylinderTopVertices[i] = m_cylinderBaseVertices[i] + end;
                    m_cylinderBaseVertices[i] += start;
                }

                if (solid)
                {
                    for (int i = 0; i < numSegments; ++i)
                    {
                        int nextI = (i + 1) % numSegments;

                        // Draw the sides of the cylinder
                        Triangle(m_cylinderBaseVertices[i], m_cylinderTopVertices[nextI], m_cylinderBaseVertices[nextI], colour);
                        Triangle(m_cylinderBaseVertices[i], m_cylinderTopVertices[nextI], m_cylinderTopVertices[i], colour);

                        // Draw the ends of the cylinder (could definitely be done with fewer triangles, oh well this is simpler)
                        Triangle(start, m_cylinderBaseVertices[i], m_cylinderBaseVertices[nextI], colour);
                        Triangle(end, m_cylinderTopVertices[i], m_cylinderTopVertices[nextI], colour);
                    }
                }
                else
                {
                    for (int i = 0; i < numSegments; ++i)
                    {
                        int nextI = (i + 1) % numSegments;

                        // Draw the sides of the cylinder
                        Line(m_cylinderBaseVertices[i], m_cylinderTopVertices[i], colour);

                        // Draw the ends of the cylinder
                        Line(start, m_cylinderBaseVertices[i], colour);
                        Line(end, m_cylinderTopVertices[i], colour);

                        // Draw round the edge of the ends of the cylinder
                        Line(m_cylinderBaseVertices[i], m_cylinderBaseVertices[nextI], colour);
                        Line(m_cylinderTopVertices[i], m_cylinderTopVertices[nextI], colour);
                    }
                }
            }

            /// <summary>
            /// Draws a cylinder in world space.
            /// </summary>
            /// <param name="start">The centre of one end of the cylinder.</param>
            /// <param name="end">The centre of the other end of the cylinder.</param>
            /// <param name="radius">The radius of the cylinder.</param>
            /// <param name="colour">The colour to draw the cylinder.</param>
            /// <param name="solid">Draw the cylinder solid, else wireframe.</param>
            static public void Cylinder(Vector3 start, Vector3 end, float radius, Color colour, bool solid)
            {
                Cylinder(start, end, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
            }

            /// <summary>
            /// Draw a cone in 3D space.
            /// </summary>
            /// <param name="baseCentre">The centre point of the base of the cone.</param>
            /// <param name="point">The tip of the cone.</param>
            /// <param name="radius">The radius of the base of the cone.</param>
            /// <param name="colour">The colour to draw the cone.</param>
            /// <param name="solid">Should the cone be solid, else wireframe.</param>
            /// <param name="numSegments">The number of radial segments for the cone.</param>
            static public void Cone(Vector3 baseCentre, Vector3 point, float radius, Color colour, bool solid, int numSegments)
            {
                // Avoid GC
                if (numSegments > m_numCylinderSegments)
                {
                    m_cylinderBaseVertices = new Vector3[numSegments];
                    m_cylinderTopVertices = new Vector3[numSegments];
                    m_numCylinderSegments = numSegments;
                }

                // Create a y-axis-aligned cone and rotate it into place
                Quaternion rotation = GetRotateFromYAxis(baseCentre, point);

                for (int i = 0; i < numSegments; ++i)
                {
                    float angle = ((float)Math.PI * 2 * i) / numSegments;

                    m_cylinderBaseVertices[i].X = radius * (float)Math.Sin(angle);
                    m_cylinderBaseVertices[i].Y = 0.0f;
                    m_cylinderBaseVertices[i].Z = radius * (float)Math.Cos(angle);

                    Vector3.Transform(ref m_cylinderBaseVertices[i], ref rotation, out m_cylinderBaseVertices[i]);
                    m_cylinderBaseVertices[i] += baseCentre;
                }

                if (solid)
                {
                    for (int i = 0; i < numSegments; ++i)
                    {
                        int nextI = (i + 1) % numSegments;

                        // Draw the sides of the cylinder
                        Triangle(m_cylinderBaseVertices[i], m_cylinderBaseVertices[nextI], point, colour);

                        // Draw the base of the cone (could definitely be done with fewer triangles, oh well this is simpler)
                        Triangle(baseCentre, m_cylinderBaseVertices[i], m_cylinderBaseVertices[nextI], colour);
                    }
                }
                else
                {
                    for (int i = 0; i < numSegments; ++i)
                    {
                        int nextI = (i + 1) % numSegments;

                        // Draw the sides of the cone
                        Line(m_cylinderBaseVertices[i], point, colour);

                        // Draw the base of the cone
                        Line(m_cylinderBaseVertices[i], baseCentre, colour);

                        // Draw round the edge of the base of the cylinder
                        Line(m_cylinderBaseVertices[i], m_cylinderBaseVertices[nextI], colour);
                    }
                }

            }

            /// <summary>
            /// Draw a cone in 3D space.
            /// </summary>
            /// <param name="baseCentre">The centre point of the base of the cone.</param>
            /// <param name="point">The tip of the cone.</param>
            /// <param name="radius">The radius of the base of the cone.</param>
            /// <param name="colour">The colour to draw the cone.</param>
            /// <param name="solid">Should the cone be solid, else wireframe.</param>
            static public void Cone(Vector3 baseCentre, Vector3 point, float radius, Color colour, bool solid)
            {
                Cone(baseCentre, point, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
            }

            /// <summary>
            /// Draw an arrow in world space.
            /// </summary>
            /// <param name="begin">The starting point for the arrow.</param>
            /// <param name="end">The tip of the arrowhead.</param>
            /// <param name="shaftRadius">The width of the shaft of the arrow.</param>
            /// <param name="headProportion">How much of the arrow the head should take up (0-1).</param>
            /// <param name="headRadiusFactor">How wide the head should be relative to the shaft (1 or larger).</param>
            /// <param name="colour">The colour to draw the arrow in.</param>
            /// <param name="solid">Should be solid, otherwise wireframe.</param>
            /// <param name="numSegments">Number of segments to draw with.</param>
            static public void Arrow(Vector3 begin, Vector3 end, float shaftRadius, float headProportion, float headRadiusFactor, Color colour, bool solid, int numSegments)
            {
                Vector3 middle = Vector3.Lerp(end, begin, headProportion);
                Cylinder(begin, middle, shaftRadius, colour, solid, numSegments);
                Cone(middle, end, shaftRadius * headRadiusFactor, colour, solid, numSegments);
                // NOTE technically we don't need to draw one end of the cylinder, it will never be seen!
            }

            /// <summary>
            /// Draw an arrow in world space.
            /// </summary>
            /// <param name="begin">The starting point for the arrow.</param>
            /// <param name="end">The tip of the arrowhead.</param>
            /// <param name="shaftRadius">The width of the shaft of the arrow.</param>
            /// <param name="headProportion">How much of the arrow the head should take up (0-1).</param>
            /// <param name="headRadiusFactor">How wide the head should be relative to the shaft (1 or larger).</param>
            /// <param name="colour">The colour to draw the arrow in.</param>
            /// <param name="solid">Should be solid, otherwise wireframe.</param>
            static public void Arrow(Vector3 begin, Vector3 end, float shaftRadius, float headProportion, float headRadiusFactor, Color colour, bool solid)
            {
                Arrow(begin, end, shaftRadius, headProportion, headRadiusFactor, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
            }

            /// <summary>
            /// Draw an arrow in world space.
            /// </summary>
            /// <param name="begin">The starting point for the arrow.</param>
            /// <param name="end">The tip of the arrowhead.</param>
            /// <param name="shaftRadius">The width of the shaft of the arrow.</param>
            /// <param name="colour">The colour to draw the arrow in.</param>
            /// <param name="solid">Should be solid, otherwise wireframe.</param>
            /// <param name="numSegments">Number of segments to draw with.</param>
            static public void Arrow(Vector3 begin, Vector3 end, float shaftRadius, Color colour, bool solid, int numSegments)
            {
                Arrow(begin, end, shaftRadius, DefaultArrowHeadProportion, DefaultArrowHeadRadiusFactor, colour, solid, numSegments);
            }

            /// <summary>
            /// Draw an arrow in world space.
            /// </summary>
            /// <param name="begin">The starting point for the arrow.</param>
            /// <param name="end">The tip of the arrowhead.</param>
            /// <param name="shaftRadius">The width of the shaft of the arrow.</param>
            /// <param name="colour">The colour to draw the arrow in.</param>
            /// <param name="solid">Should be solid, otherwise wireframe.</param>
            static public void Arrow(Vector3 begin, Vector3 end, float shaftRadius, Color colour, bool solid)
            {
                Arrow(begin, end, shaftRadius, DefaultArrowHeadProportion, DefaultArrowHeadRadiusFactor, colour, solid);
            }

            /// <summary>
            /// Draw a capsule (also known as sausage or sometimes, incorrectly, lozenge) in world space.
            /// </summary>
            /// <param name="begin">The centre point of one end of the capsule.</param>
            /// <param name="end">The centre point of the other end of the capsule.</param>
            /// <param name="radius">The width of the capsule.</param>
            /// <param name="colour">The color in which to draw the capsule.</param>
            /// <param name="solid">Should it be solid, otherwise wireframe.</param>
            /// <param name="numSegments">Number of segments to use to draw the capsule.</param>
            static public void Capsule(Vector3 begin, Vector3 end, float radius, Color colour, bool solid, int numSegments)
            {
                // TODO draw only the sides of the cylinder and the hemisphere on either end - this draws quite a bit
                // that won't be seen (especially when solid).
                Sphere(begin, radius, colour, solid, numSegments, numSegments);
                Cylinder(begin, end, radius, colour, solid, numSegments);
                Sphere(end, radius, colour, solid, numSegments, numSegments);
            }

            /// <summary>
            /// Draw a capsule (also known as sausage or sometimes, incorrectly, lozenge) in world space.
            /// </summary>
            /// <param name="begin">The centre point of one end of the capsule.</param>
            /// <param name="end">The centre point of the other end of the capsule.</param>
            /// <param name="radius">The width of the capsule.</param>
            /// <param name="colour">The color in which to draw the capsule.</param>
            /// <param name="solid">Should it be solid, otherwise wireframe.</param>
            static public void Capsule(Vector3 begin, Vector3 end, float radius, Color colour, bool solid)
            {
                Capsule(begin, end, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
            }

            #endregion

            #region Constants

            private static readonly float DefaultArrowHeadProportion = 0.3f;
            private static readonly float DefaultArrowHeadRadiusFactor = 1.5f;
            private static readonly int DefaultNumSegmentsLine = 12;
            private static readonly int DefaultNumSegmentsSolid = 12;

            #endregion

            #region Variables

            // Class static variables
            static private Effect effect;
            // static private VertexDeclaration s_vertexDeclaration; // XNA 3

            // Shape types
            static private List<VertexPositionColor> s_line2DVertices = new List<VertexPositionColor>();
            static private List<VertexPositionColor> s_triangle2DVertices = new List<VertexPositionColor>();
            static private List<VertexPositionColor> s_line3DVertices = new List<VertexPositionColor>();
            static private List<VertexPositionColor> s_triangle3DVertices = new List<VertexPositionColor>();

            // Needed to reduce garbage collection, can't just use ToArray() as it always creates a new one
            static private VertexPositionColor[] s_triangle2DVerticesArray;
            static private int s_triangle2DVerticesArraySize;
            static private VertexPositionColor[] s_line2DVerticesArray;
            static private int s_line2DVerticesArraySize;
            static private VertexPositionColor[] s_triangle3DVerticesArray;
            static private int s_triangle3DVerticesArraySize;
            static private VertexPositionColor[] s_line3DVerticesArray;
            static private int s_line3DVerticesArraySize;

            // More GC avoidance, don't create temporary arrays just to calculate positions that get placed in the static arrays
            static int m_numSphereVertices = DefaultNumSegmentsLine * (DefaultNumSegmentsLine + 1);
            static Vector3[] m_sphereVertices = new Vector3[m_numSphereVertices];
            static int m_numCylinderSegments = DefaultNumSegmentsLine;
            static Vector3[] m_cylinderBaseVertices = new Vector3[m_numCylinderSegments];
            static Vector3[] m_cylinderTopVertices = new Vector3[m_numCylinderSegments];

            #endregion

            #region Private functions

            static private void Render2DLines(GraphicsDevice device, float width, float height)
            {
                if (s_line2DVertices.Count > 0)
                {
                    // Reduce GC, see notes in Render3DTriangles
                    if (s_line2DVertices.Count > s_line2DVerticesArraySize)
                    {
                        s_line2DVerticesArraySize = s_line2DVertices.Count;
                        s_line2DVerticesArray = new VertexPositionColor[s_line2DVerticesArraySize];
                    }

                    s_line2DVertices.CopyTo(s_line2DVerticesArray);

                    // Refit lines to screen space
                    for (int i = 0; i < s_line2DVertices.Count; ++i)
                    {
                        s_line2DVerticesArray[i].Position.X = -1.0f + 2.0f * s_line2DVerticesArray[i].Position.X / width;
                        s_line2DVerticesArray[i].Position.Y = -(-1.0f + 2.0f * s_line2DVerticesArray[i].Position.Y / height);
                    }

                    //device.RenderState.DepthBufferEnable = false; // xna 3
                    device.DepthStencilState = DepthStencilState.None;

                    effect.CurrentTechnique = effect.Techniques["LineRendering2D"];
                    // s_effect.Begin();

                    for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
                    {
                        EffectPass pass = effect.CurrentTechnique.Passes[num];
                        pass.Apply();
                        device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, s_line2DVerticesArray, 0, s_line2DVertices.Count / 2);
                        //pass.End();
                    }

                    //s_effect.End();
                    s_line2DVertices.Clear();
                }
            }

            static private void Render2DTriangles(GraphicsDevice device, float width, float height)
            {
                if (s_triangle2DVertices.Count > 0)
                {
                    if (s_triangle2DVertices.Count > s_triangle2DVerticesArraySize)
                    {
                        s_triangle2DVerticesArraySize = s_triangle2DVertices.Count;
                        s_triangle2DVerticesArray = new VertexPositionColor[s_triangle2DVerticesArraySize];
                    }

                    s_triangle2DVertices.CopyTo(s_triangle2DVerticesArray);

                    // Refit lines to screen space
                    for (int i = 0; i < s_triangle2DVertices.Count; ++i)
                    {
                        s_triangle2DVerticesArray[i].Position.X = -1.0f + 2.0f * s_triangle2DVerticesArray[i].Position.X / width;
                        s_triangle2DVerticesArray[i].Position.Y = -(-1.0f + 2.0f * s_triangle2DVerticesArray[i].Position.Y / height);
                    }

                    //device.RenderState.DepthBufferEnable = false;// xna 3
                    device.DepthStencilState = DepthStencilState.None;

                    effect.CurrentTechnique = effect.Techniques["LineRendering2D"];
                    // s_effect.Begin();

                    for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
                    {
                        EffectPass pass = effect.CurrentTechnique.Passes[num];
                        pass.Apply();
                        device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, s_triangle2DVerticesArray, 0, s_triangle2DVertices.Count / 3);
                        //pass.End();
                    }

                    //s_effect.End();
                    s_triangle2DVertices.Clear();
                }
            }

            static private void Render3DLines(GraphicsDevice device, Matrix renderMatrix)
            {
                if (s_line3DVertices.Count > 0)
                {
                    // Reduce GC, see notes in Render3DTriangles
                    if (s_line3DVertices.Count > s_line3DVerticesArraySize)
                    {
                        s_line3DVerticesArraySize = s_line3DVertices.Count;
                        s_line3DVerticesArray = new VertexPositionColor[s_line3DVerticesArraySize];
                    }

                    s_line3DVertices.CopyTo(s_line3DVerticesArray);

                    //device.RenderState.DepthBufferEnable = true; // xna 3
                    device.DepthStencilState = DepthStencilState.Default;

                    effect.Parameters["renderMatrix"].SetValue(renderMatrix);
                    effect.CurrentTechnique = effect.Techniques["LineRendering3D"];
                    //   s_effect.Begin();

                    for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
                    {
                        EffectPass pass = effect.CurrentTechnique.Passes[num];
                        pass.Apply();
                        device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, s_line3DVerticesArray, 0, s_line3DVertices.Count / 2);
                        // TODO check device.MaxPrimitiveCount and if necessary split up into several DUP calls.
                        // When we draw ridiculously many triangles we exceed this and it draws nothing.
                    }

                    s_line3DVertices.Clear();
                }
            }

            static private void Render3DTriangles(GraphicsDevice device, Matrix renderMatrix)
            {
                if (s_triangle3DVertices.Count > 0)
                {
                    // Copy to a constant array. Can't use the ToArray() method as that creates a new array
                    // every time and the garbage collection penalty is horrific. Instead will keep a single
                    // array and grow it as necessary. This vastly reduces the GC penalty.
                    if (s_triangle3DVertices.Count > s_triangle3DVerticesArraySize)
                    {
                        s_triangle3DVerticesArraySize = s_triangle3DVertices.Count;
                        s_triangle3DVerticesArray = new VertexPositionColor[s_triangle3DVerticesArraySize];
                    }

                    s_triangle3DVertices.CopyTo(s_triangle3DVerticesArray);

                    //device.RenderState.DepthBufferEnable = true; // xna 3
                    device.DepthStencilState = DepthStencilState.Default;

                    effect.Parameters["renderMatrix"].SetValue(renderMatrix);
                    effect.CurrentTechnique = effect.Techniques["LineRendering3D"];
                    //  s_effect.Begin();

                    for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
                    {
                        EffectPass pass = effect.CurrentTechnique.Passes[num];
                        pass.Apply();
                        device.DrawUserPrimitives<VertexPositionColor>(
                            PrimitiveType.TriangleList, s_triangle3DVerticesArray, 0, s_triangle3DVertices.Count / 3);
                        //pass.End();
                        // TODO check device.MaxPrimitiveCount and if necessary split up into several DUP calls.
                        // I think when we draw ridiculously many triangles we exceed this and it draws nothing.
                    }

                    //s_effect.End();
                    s_triangle3DVertices.Clear();
                }
            }

            static private Quaternion GetRotateFromYAxis(Vector3 start, Vector3 end)
            {
                Vector3 difference = end - start;
                difference.Normalize();

                Quaternion rotation;

                if (difference == Vector3.UnitY)
                {
                    rotation = Quaternion.Identity;
                }
                else if (difference == -Vector3.UnitY)
                {
                    rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI);
                }
                else
                {
                    Matrix rotationMatrix = new Matrix();
                    rotationMatrix.Up = difference;
                    rotationMatrix.Right = Vector3.Cross(Vector3.UnitY, difference);
                    rotationMatrix.Forward = Vector3.Cross(difference, rotationMatrix.Right);
                    rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);
                }

                return rotation;
            }

            #endregion
        }
    }
}
