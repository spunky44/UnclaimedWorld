using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Xclna.Xna.Animation.Content;
using System.ComponentModel;
using System.IO;

namespace VehiclePipeline
{
    [ContentProcessor]
    public class VehicleMaterialProcessor : MaterialProcessor
    {
        private string environmentMap = "landscape_real.png";
        [DisplayName("Environment Map")]
        [DefaultValue("landscape_real.png")]
        [Description("The environment map applied to the model.")]
        public string EnvironmentMap
        {
            get { return environmentMap; }
            set { environmentMap = value; }
        }

        private string dirtMap;
        public string DirtMap
        {
            get { return dirtMap; }
            set { dirtMap = value; }
        }


        private string detailsMap = "";       
        /// <summary>
        /// the details map texture is excluded from color replacement operations (and dirt too?)
        /// </summary>
        public string DetailsMap
        {
            get { return detailsMap; }
            set { detailsMap = value; }
        }


        /// <summary>
        /// Converts a material.
        /// </summary>
        public override MaterialContent Process(MaterialContent input,
                                                ContentProcessorContext context)
        {
            // Create a new effect material.
            EffectMaterialContent customMaterial = new EffectMaterialContent();

            // Point the new material at our custom effect file.
            string effectFile = Path.GetFullPath("Vehicle.fx"); //Path.GetFullPath("EnvironmentMap.fx");

            customMaterial.Effect = new ExternalReference<EffectContent>(effectFile);

            // Copy texture data across from the original material.
            BasicMaterialContent basicMaterial = (BasicMaterialContent)input;

            //basicMaterial.

            //  System.Diagnostics.Debugger.Launch();
            context.Logger.LogImportantMessage("Unskinned material being handled.");

            if (basicMaterial.Texture != null)
            {
                context.Logger.LogImportantMessage(
                    "Unskinned texture found: {0}", basicMaterial.Texture.Filename);

                customMaterial.Textures.Add("BasicTexture", basicMaterial.Texture);
                customMaterial.OpaqueData.Add("TextureEnabled", true);
            }

            // Add the reflection texture.
            string envmap = Path.GetFullPath(EnvironmentMap);


            if (basicMaterial.SpecularPower.HasValue)
            {
                context.Logger.LogImportantMessage("Unskinned material specular power: " + basicMaterial.SpecularPower);              
                customMaterial.OpaqueData.Add("SpecularPower", basicMaterial.SpecularPower);
             
                //customMaterial.OpaqueData.Add("SpecularPower", basicMaterial.SpecularPower);
                //   customMaterial.OpaqueData.Add("SpecularPower", basicMaterial.SpecularPower);

                //  customMaterial.Effect.OpaqueData.Add("SpecularPower", 32); // basicMaterial.SpecularPower);
            }
         

            if (basicMaterial.SpecularColor.HasValue)
            {
                context.Logger.LogImportantMessage("Unskinned material specular color: {0}", basicMaterial.SpecularColor.Value.ToString());
                customMaterial.OpaqueData.Add("SpecularColor", basicMaterial.SpecularColor);
            }

            if (basicMaterial.EmissiveColor.HasValue)
            {
                context.Logger.LogImportantMessage("Unskinned material emissive color: {0}", basicMaterial.EmissiveColor.Value.ToString());

                customMaterial.OpaqueData.Add("EmissiveColor", basicMaterial.EmissiveColor);
            }

           /* if (basicMaterial.OpaqueData.Count > 0)
            {
                foreach (string key in basicMaterial.OpaqueData.Keys)
                {
                    context.Logger.LogImportantMessage("Unskinned material opaque data: {0} {1}", key, basicMaterial.OpaqueData[key].ToString());
                }
            }*/
            /*
            if (basicMaterial.OpaqueData.ContainsKey("Reflectivity"))
            {
                context.Logger.LogImportantMessage("Unskinned material reflectivity: {0}", basicMaterial.OpaqueData["Reflectivity"].ToString());
                customMaterial.OpaqueData.Add("Reflectivity", basicMaterial.OpaqueData["Reflectivity"]);
            }*/

            customMaterial.Textures.Add("EnvironmentMap", new ExternalReference<TextureContent>(envmap));
            customMaterial.OpaqueData.Add("EnvironmentMapEnabled", true);

            if (!string.IsNullOrEmpty(DirtMap))
            {
                string dirtmap = Path.GetFullPath(DirtMap);
                customMaterial.Textures.Add("DirtMap", new ExternalReference<TextureContent>(dirtmap));
                customMaterial.OpaqueData.Add("DirtMapEnabled", true);
            }

            if (!string.IsNullOrEmpty(DetailsMap))
            {
                string detailsMap = Path.GetFullPath(DetailsMap);
                customMaterial.Textures.Add("DetailsMap", new ExternalReference<TextureContent>(detailsMap));
                customMaterial.OpaqueData.Add("DetailsMapEnabled", true);
            }


            // Chain to the base material processor.
            return base.Process(customMaterial, context);
        }
    }
}
