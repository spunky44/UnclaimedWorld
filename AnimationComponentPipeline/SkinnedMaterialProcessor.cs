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

namespace Xclna.Xna.Animation.Content
{
    [ContentProcessor]
    public class SkinnedMaterialProcessor : MaterialProcessor
    {
      
        private string detailsMap = "";       
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
            string effectFile = Path.GetFullPath("skinFX.fx"); //Path.GetFullPath("EnvironmentMap.fx");

            customMaterial.Effect = new ExternalReference<EffectContent>(effectFile);

            // Copy texture data across from the original material.
            BasicMaterialContent basicMaterial = (BasicMaterialContent)input;

            if (basicMaterial.SpecularPower.HasValue) // is called "ShininessExponent" in fbx
            {
                context.Logger.LogImportantMessage("Skinned material specular power: " + basicMaterial.SpecularPower);
             //   context.Logger.LogImportantMessage("Skinned material specular color: " + basicMaterial.SpecularColor);
                customMaterial.OpaqueData.Add("SpecularPower", basicMaterial.SpecularPower);
                //basicMaterial.ref
                //customMaterial.OpaqueData.Add("SpecularPower", basicMaterial.SpecularPower);
             //   customMaterial.OpaqueData.Add("SpecularPower", basicMaterial.SpecularPower);

                //  customMaterial.Effect.OpaqueData.Add("SpecularPower", 32); // basicMaterial.SpecularPower);
            }
         /*   else
            {
                customMaterial.OpaqueData.Add("SpecularPower", 1);
            }*/

            if (basicMaterial.EmissiveColor.HasValue)
            {
                context.Logger.LogImportantMessage("Skinned material emissive color: {0}", basicMaterial.EmissiveColor.Value.ToString());
               
                customMaterial.OpaqueData.Add("EmissiveColor", basicMaterial.EmissiveColor);               
            }
            

            if (basicMaterial.SpecularColor.HasValue)
            {
                context.Logger.LogImportantMessage("Skinned material specular color: {0}", basicMaterial.SpecularColor.Value.ToString());
                customMaterial.OpaqueData.Add("SpecularColor", basicMaterial.SpecularColor);
            }

            /*if (basicMaterial.OpaqueData.Count > 0)
            {
                foreach (string key in basicMaterial.OpaqueData.Keys)
                {
                    context.Logger.LogImportantMessage("Unskinned material opaque data: {0} {1}", key, basicMaterial.OpaqueData[key].ToString());
                }
            }*/

            //customMaterial.OpaqueData.Add("SpecularPower", 32);

            //  System.Diagnostics.Debugger.Launch();
            //context.Logger.LogImportantMessage("Vehicle material being handled.");

            if (basicMaterial.Texture != null)
            {
               /* context.Logger.LogImportantMessage(
                    "Vehicle texture found: {0}", basicMaterial.Texture.Filename);
                */
                customMaterial.Textures.Add("BasicTexture", basicMaterial.Texture);
                customMaterial.OpaqueData.Add("TextureEnabled", true);

                
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
