using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Xclna.Xna.Animation.Content;


namespace VehiclePipeline
{
    /// <summary>
    /// 
    ///Subclasses the AnimatedModelProcessor from the Animation Component Pipeline project!
    /// </summary>
    [ContentProcessor(DisplayName = "Model - Animated Vehicle")]
    public class AnimatedVehicleProcessor : AnimatedModelProcessor //ContentProcessor<TInput, TOutput>
    {
        List<Vector3> vertices = new List<Vector3>();

        private string environmentMap = "landscapeSquare.png";
        [DisplayName("Environment Map")]
        [DefaultValue("landscapeSquare.png")]
        [Description("The environment map applied to the model. Please make it power of 2.")]
        public string EnvironmentMap
        {
            get { return environmentMap; }
            set { environmentMap = value; }
        }


        private string dirtMap = "";
        [DisplayName("Dirt map")]        
        [Description("The dirt map applied to the model. Please make it power of 2.")]
        public string DirtMap
        {
            get { return dirtMap; }
            set { dirtMap = value; }
        }


        private string detailsMap = "";
        [DisplayName("Details map")]
        [Description("The details map applied to the model. Please make it power of 2.")]
        public string DetailsMap
        {
            get { return detailsMap; }
            set { detailsMap = value; }
        }

    /*    public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            // Look up the input vertex positions.
            FindVertices(input);

            // Chain to the base ModelProcessor class.
            ModelContent model = base.Process(input, context);

            // You can store any type of object in the model Tag property. This
            // sample only uses built-in types such as string, Vector3, BoundingSphere,
            // dictionaries, and arrays, which the content pipeline knows how to
            // serialize by default. We could also attach custom data types here, but
            // then we would have to provide a ContentTypeWriter and ContentTypeReader
            // implementation to tell the pipeline how to serialize our custom type.
            //
            // We are setting our model Tag to a dictionary that maps strings to
            // objects, and then storing two different kinds of custom data into that
            // dictionary. This is a useful pattern because it allows processors to
            // combine many different kinds of information inside the single Tag value.

            Dictionary<string, object> tagData = new Dictionary<string, object>();

            model.Tag = tagData;

            // Store vertex information in the tag data, as an array of Vector3.
            tagData.Add("Vertices", vertices.ToArray());

            // Also store a custom bounding sphere.
            tagData.Add("BoundingSphere", BoundingSphere.CreateFromPoints(vertices));

            return model;
        }*/

        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
         //   System.Diagnostics.Debugger.Launch();
            context.Logger.LogImportantMessage("Unskinned material encountered: {0}", material.Name);

            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
            processorParameters.Add("EnvironmentMap", EnvironmentMap);

            processorParameters.Add("DirtMap", DirtMap);
            processorParameters.Add("DetailsMap", DetailsMap);

            //NEW:
            processorParameters.Add("GenerateMipmaps", GenerateMipmaps);
            processorParameters.Add("TextureFormat", this.TextureFormat);           
            processorParameters.Add("ResizeTexturesToPowerOfTwo", this.ResizeTexturesToPowerOfTwo);


            return context.Convert<MaterialContent, MaterialContent>(material,
                                                "VehicleMaterialProcessor",
                                                processorParameters);
        }

        /// <summary>
        /// Helper for extracting a list of all the vertex positions in a model.
        /// </summary>
    /*    void FindVertices(NodeContent node)
        {
            // Is this node a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                // Loop over all the pieces of geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // Loop over all the indices in this piece of geometry.
                    // Every group of three indices represents one triangle.
                    foreach (int index in geometry.Indices)
                    {
                        // Look up the position of this vertex.
                        Vector3 vertex = geometry.Vertices.Positions[index];

                        // Transform from local into world space.
                        vertex = Vector3.Transform(vertex, absoluteTransform);

                        // Store this vertex.
                        vertices.Add(vertex);
                    }
                }
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                FindVertices(child);
            }
        }
     * */
    }
}