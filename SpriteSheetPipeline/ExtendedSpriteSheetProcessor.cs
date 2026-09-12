#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetProcessor.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using SpriteSheetRuntime;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
#endregion

namespace SpriteSheetPipeline
{
    /// <summary>
    /// Custom content processor takes an array of individual sprite filenames (which
    /// will typically be imported from an XML file), reads them all into memory,
    /// arranges them onto a single larger texture, and returns the resulting sprite
    /// sheet object.
    /// </summary>
    [ContentProcessor]
    public class ExtendedSpriteSheetProcessor : ContentProcessor<string[], ExtendedSpriteSheetContent>
    {
        // Controls how extreme the output normalmap should be.
        const float bumpSize = 10f; //4f;

        bool doDebugLogging = false; //true; // false;

        Dictionary<string, string> treeFlavours;
        ExtendedSpriteSheetContent spriteSheet; 

        public SpriteSheetPipeline.SpriteSheetProcessor.Actions Action
        {
            get { return action; }
            set { action = value; }
        }

        SpriteSheetPipeline.SpriteSheetProcessor.Actions action = SpriteSheetPipeline.SpriteSheetProcessor.Actions.None;

        ContentProcessorContext context;

        /// <summary>
        /// Converts an array of sprite filenames into a sprite sheet object.
        /// </summary>
        public override ExtendedSpriteSheetContent Process(string[] input,
                                                ContentProcessorContext context)
        {
            this.context = context;

            context.Logger.LogImportantMessage("Extended sprite sheet processor: Start processing sprites.");

            spriteSheet = new ExtendedSpriteSheetContent();

            List<BitmapContent> sourceSprites = new List<BitmapContent>();

            List<PixelBitmapContent<Vector4>> sourceSpritesDepthMaps = new List<PixelBitmapContent<Vector4>>();

            treeFlavours = new Dictionary<string, string>();

            //Debugger.Launch();

            // Loop over each input sprite filename.
            foreach (string inputFilename in input)
            {
                if (inputFilename.EndsWith("\\"))
                {
                    context.Logger.LogImportantMessage("Processing sprites in folder {0}", inputFilename);
                    string folderName = inputFilename.Substring(0, inputFilename.Length - 1);

                    string[] filesInFolder = System.IO.Directory.GetFiles(folderName);
                    foreach (string filename in filesInFolder)
                    {
                        string spriteName = Path.GetFileNameWithoutExtension(filename);

                        context.Logger.LogImportantMessage("Look at file {0}, suffix: {1}, extension: {2}, spritename: {3}",
                            filename, spriteName.Substring(spriteName.Length - 2, 1), Path.GetExtension(filename), spriteName);

                        // new...
                        string suffix = spriteName.Substring(spriteName.Length - 2, 2);

                        if ((Path.GetExtension(filename) == ".tga" || Path.GetExtension(filename) == ".png")
                            && suffix != "_u" && suffix != "_d" && suffix != "_g")
                        { // we found a 'base' sprite - no suffix.
                            // Store the name of this sprite.

                            DoSpriteNoNormalize(context, sourceSprites, sourceSpritesDepthMaps, filename, spriteName);

                        }
                    }
                }
                else
                {
                    // load single sprite
                    // Store the name of this sprite.
                    string spriteName = Path.GetFileNameWithoutExtension(inputFilename);

                    spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

                    DoSpriteNoNormalize(context, sourceSprites, sourceSpritesDepthMaps, inputFilename, spriteName);

                }
            }

            // Pack all the sprites into a single large texture.

            PixelBitmapContent<Vector4> packedSpritesNormalMaps = null;
            BitmapContent packedSprites = null;

            SpritePacker.PackSpritesNoNormalize(sourceSprites, spriteSheet.SpriteNames, sourceSpritesDepthMaps,
                                     spriteSheet.SpriteRectangles, context,
                                     out packedSprites, out packedSpritesNormalMaps); //, false);

            spriteSheet.Texture.Mipmaps.Add(packedSprites);
            context.Logger.LogImportantMessage("Added diffuse sprite sheet texture.");
            spriteSheet.NormalTexture.Mipmaps.Add(packedSpritesNormalMaps);
            context.Logger.LogImportantMessage("Added normal sprite sheet texture.");

            return spriteSheet;
        }

        /* Normalizes:
        public override ExtendedSpriteSheetContent Process(string[] input,
                                                   ContentProcessorContext context)
        {
            this.context = context;

            context.Logger.LogImportantMessage("Extended sprite sheet processor: Start processing sprites.");

            spriteSheet = new ExtendedSpriteSheetContent();
            
            List<BitmapContent> sourceSprites = new List<BitmapContent>();
       
            List<PixelBitmapContent<NormalizedByte4>> sourceSpritesDepthMaps = new List<PixelBitmapContent<NormalizedByte4>>();

            treeFlavours = new Dictionary<string, string>();


            // Loop over each input sprite filename.
            foreach (string inputFilename in input)
            {
                if (inputFilename.EndsWith("\\"))
                {
                    context.Logger.LogImportantMessage("Processing sprites in folder {0}", inputFilename);
                    string folderName = inputFilename.Substring(0, inputFilename.Length - 1);

                    string[] filesInFolder = System.IO.Directory.GetFiles(folderName);
                    foreach (string filename in filesInFolder)
                    {                        
                        string spriteName = Path.GetFileNameWithoutExtension(filename);

                        context.Logger.LogImportantMessage("Look at file {0}, suffix: {1}, extension: {2}, spritename: {3}",
                            filename, spriteName.Substring(spriteName.Length - 2, 1), Path.GetExtension(filename), spriteName);

                        // new...
                        string suffix = spriteName.Substring(spriteName.Length - 2, 2);

                        if ((Path.GetExtension(filename) == ".tga" || Path.GetExtension(filename) == ".png")
                            && suffix != "_u" && suffix != "_d" && suffix != "_g")    
                        { // we found a 'base' sprite - no suffix.
                            // Store the name of this sprite.

                            DoSprite(context, sourceSprites, sourceSpritesDepthMaps, filename, spriteName);

                        }
                    }
                }
                else
                {
                    // load single sprite
                    // Store the name of this sprite.
                    string spriteName = Path.GetFileNameWithoutExtension(inputFilename);

                    spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

                    DoSprite(context, sourceSprites, sourceSpritesDepthMaps, inputFilename, spriteName);

                }    
            }

            // Pack all the sprites into a single large texture.
            
            PixelBitmapContent<NormalizedByte4> packedSpritesNormalMaps = null;
            BitmapContent packedSprites = null;
            
            SpritePacker.PackSprites(sourceSprites, spriteSheet.SpriteNames, sourceSpritesDepthMaps, 
                                     spriteSheet.SpriteRectangles, context,
                                     out packedSprites, out packedSpritesNormalMaps); //, false);

            spriteSheet.Texture.Mipmaps.Add(packedSprites);
            context.Logger.LogImportantMessage("Added diffuse sprite sheet texture.");
            spriteSheet.NormalTexture.Mipmaps.Add(packedSpritesNormalMaps);
            context.Logger.LogImportantMessage("Added normal sprite sheet texture.");
         
            return spriteSheet;
        }*/

        private void DoSpriteNoNormalize(ContentProcessorContext context, List<BitmapContent> sourceSprites, List<PixelBitmapContent<Vector4>> sourceSpritesDepthMaps, string filename, string spriteName)
        {
            spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

            context.Logger.LogImportantMessage("Adding diffuse sprite {0}", filename);
            SpriteSheetProcessor.LoadSpriteTexture(context, sourceSprites, filename, false, Action);

            context.Logger.LogImportantMessage("Adding normal map sprite");
            HandleDepthMapNoNormalize(spriteName, filename, sourceSpritesDepthMaps, context);
        }


        
      /*  private void DoSprite(ContentProcessorContext context, List<BitmapContent> sourceSprites, List<PixelBitmapContent<NormalizedByte4>> sourceSpritesDepthMaps, string filename, string spriteName)
        {
            spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

            context.Logger.LogImportantMessage("Adding diffuse sprite {0}", filename);
            SpriteSheetProcessor.LoadSpriteTexture(context, sourceSprites, filename, false, Action);

            context.Logger.LogImportantMessage("Adding normal map sprite");
            HandleDepthMap(spriteName, filename, sourceSpritesDepthMaps, context);
        }*/

        

        

        /*
        private void HandleUtilityMap(string spriteName, string inputFilePath, ContentProcessorContext context)
        {
            // handle utility map, if any is found:
            string utilityMapFileName = spriteName + "_u";
            string utilityMapFilePath = inputFilePath.Replace(spriteName, utilityMapFileName);
                         
            // NEW: use TGA for utility map because of color encoding problems with png.
            utilityMapFilePath = inputFilePath.Replace(spriteName, utilityMapFileName).Replace(".png", ".tga");
            //  System.Diagnostics.Debugger.Launch();
            context.Logger.LogImportantMessage(
                "Looking for utility map on path: {0}", utilityMapFilePath);

            if (File.Exists(utilityMapFilePath))
            {
                context.Logger.LogImportantMessage("Found: {0}", utilityMapFilePath);

               // BuildingTypeData buildingTypeData = new BuildingTypeData();
                
                // Load the sprite texture into memory. we don't need the utility bitmap for anything ingame though... can it be disposed of?
                
                OpaqueDataDictionary data = new OpaqueDataDictionary(); // XNA 4
                data.Add("PremultiplyAlpha", false); // don't premultiply!!! 
                ExternalReference<TextureContent> textureReference = new ExternalReference<TextureContent>(utilityMapFilePath);
                TextureContent texture = context.BuildAndLoadAsset<TextureContent, TextureContent>(textureReference, "TextureProcessor", data, null);
                
                PixelBitmapContent<Color> bitmap = (PixelBitmapContent<Color>)texture.Faces[0][0];
                buildingTypeData.LightSourceOffsets = GetLightMapOffsets(bitmap, spriteName, false, context);
                
                // NEW: Store dimensions of utility map:
                buildingTypeData.Width = bitmap.Width;
                buildingTypeData.Height = bitmap.Height;

                int width, height;

                bool debugBlockedMap = false;
                if (utilityMapFilePath.EndsWith("rockwall1_u.tga")) // == "terrainBillboards\rockwall1_u.tga")
                {
                    context.Logger.LogImportantMessage("Debugging is turned on");

                    debugBlockedMap = true;
                }
                

                buildingTypeData.DiscomfortValues = GetDiscomfortValues(bitmap, spriteName, out width, out height, debugBlockedMap);
                buildingTypeData.WidthInTiles = width;
                buildingTypeData.HeightInTiles = height;

                GetPointsOfInterest(bitmap, buildingTypeData, context, height);

                spriteSheet.BuildingTypeData.Add(spriteName, buildingTypeData);

            }
         
        }

        private void GetPointsOfInterest(PixelBitmapContent<Color> bitmap, 
            BuildingTypeData buildingTypeData,
            ContentProcessorContext context, int heightInTiles)
        {
            // sample center of tiles:
            for (int y = bitmap.Height - 24; y > 0; y -= 48)
            {
                for (int x = 24; x < bitmap.Width; x += 48)
                {
                    Color value = bitmap.GetPixel(x, y);
                    if (value.G >= 170 && value.G < 180)
                    {   
                        context.Logger.LogImportantMessage("Found tiny addon slot at: {0}, {1}", x, y);
                        buildingTypeData.CreateAddonSlotsIfNotExist();
                        buildingTypeData.AddonSlots[AddonSize.Tiny].Add(ConvertTopLeftOffsetToRelativeTilePos(bitmap.Height, new Point(x, y), heightInTiles));
                    }
                    else if (value.G >= 180 && value.G < 190)
                    {   // 26, 121
                        context.Logger.LogImportantMessage("Found small addon slot at: {0}, {1}", x, y);
                        buildingTypeData.CreateAddonSlotsIfNotExist();
                        buildingTypeData.AddonSlots[AddonSize.Small].Add(ConvertTopLeftOffsetToRelativeTilePos(bitmap.Height, new Point(x, y), heightInTiles));
                    }
                    else if (value.G >= 190 && value.G < 200)
                    {
                        context.Logger.LogImportantMessage("Found big addon slot at: {0}, {1}", x, y);
                        buildingTypeData.CreateAddonSlotsIfNotExist();
                        buildingTypeData.AddonSlots[AddonSize.Big].Add(ConvertTopLeftOffsetToRelativeTilePos(bitmap.Height, new Point(x, y), heightInTiles));
                    }
                }
            }       

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color value = bitmap.GetPixel(x, y);

                    if (value.A == 0)
                    {   // skip transparent pixels
                        continue;
                    }
          
                    if (value.G == 255)
                    {
                        context.Logger.LogImportantMessage("Found base center at: {0}, {1}", x, y);
                        buildingTypeData.BaseCenter = new Point(x, y); 
                    }
                    else if (value.G == 240)
                    {
                        buildingTypeData.FrontDoorRelativeLocation = new Point(x, y);
                    }
                    else if (value.G == 200)
                    {
                        buildingTypeData.BackDoorRelativeLocation = new Point(x, y); 
                    }
                    
                }
            }
        }*/

        private Point ConvertTopLeftOffsetToRelativeTilePos(int bitmapHeight, Point offset, int heightInTiles)
        {
            return new Point((int)(offset.X / 48), (heightInTiles * 48 - bitmapHeight + offset.Y) / 48);

        }

        /*
        private byte[] GetDiscomfortValues(PixelBitmapContent<Color> bitmap, string spriteName, out int widthInTiles, out int heightInTiles, bool debugOutput)
        {
           
            // sample the middle of each sub tile (16 * 16 pixels). 9 subtiles per tile.
            // start from the bottom. not all values are used if the billboard extends 'above' ground.
            // they are disregarded on load.
            //for (int y = 8; y < bitmap.Height; y += 16)

            bool hasHitFilledTile = false;
            int x = bitmap.Width / 2 - 10;
            int y;
            widthInTiles = 0;
            heightInTiles = 0;
            
            int heightInSubtiles = 0;
            // NEW: sample each subtile. Round up to get the tile height...
            for (y = bitmap.Height - 8; y > 0; y -= 16)
            {
                Color value = bitmap.GetPixel(x, y);
                if (value.A > 0)
                {
                    hasHitFilledTile = true;
                }
                else if (hasHitFilledTile == true)
                {
                    // beyond "top edge"
                    break;
                }
                heightInSubtiles++;
            }

            // round up:
            heightInTiles = (int)Math.Ceiling((double)heightInSubtiles / 3d);

            // NEW: round up instead of down, to accommodate non-grid dimensions on bitmaps:
            widthInTiles = (int)Math.Ceiling((double)bitmap.Width / 48d);

            // create a discomfort map to fit the dimensions, fill it with 0's where we go outside the bitmap:
            byte[] discomforts = new byte[widthInTiles * 3 * heightInTiles * 3];


            // if the bitmap is smaller than a full tile, bail out now:
            // fix this!!!!!
            if (bitmap.Width < 48 || bitmap.Height < 48)
            {
                return discomforts;
            }


            int discomfortHeightInPixels = heightInTiles * 48;

            int subTileIndex = 0; // one dimensional array...

            if (debugOutput)
            {
                context.Logger.LogImportantMessage("Read discomfort map: width, height in tiles: {0}, {1}", widthInTiles, heightInTiles);
            }

            int pixelX = -1, pixelY = -1;
            try
            {
                for (y = 0; y < heightInTiles * 3; y++)
                {
                    pixelY = (bitmap.Height - discomfortHeightInPixels) + ((y * 16) + 8);

                    for (x = 0; x < widthInTiles * 3; x++)
                    {
                        pixelX = (x * 16) + 8;

                        if (pixelX < bitmap.Width && pixelY < bitmap.Height)
                        {
                            Color value = bitmap.GetPixel(pixelX, pixelY);

                            if (debugOutput)
                            {
                                context.Logger.LogImportantMessage("Read discomfort map: color value at ({0}, {1}) is {2}", pixelX, pixelY, value.ToString());
                            }

                            if (value.A > 0)
                            {
                                discomforts[subTileIndex] = value.R;
                            }
                            else
                            {
                                discomforts[subTileIndex] = 0;
                            }

                        }
                        else
                        {
                            // outside the bitmap... enter a 0:
                            discomforts[subTileIndex] = 0;
                        }

                        subTileIndex++;

                        if (subTileIndex >= discomforts.Length)
                        { // ???
                            return discomforts;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Error reading discomforts for '{5}', length of array: {6}, subtile index : {0}, pixelX,pixelY: {1},{2}, width*height: {3}*{4}", subTileIndex, pixelX, pixelY, bitmap.Width, bitmap.Height, spriteName, discomforts.Length));
            }

         
            return discomforts;
        }*/

        private Dictionary<string, Point> GetLightMapOffsets(PixelBitmapContent<Color> bitmap, string buildingAndFlavourName, bool doDebugLogging, ContentProcessorContext context)
        {
            Dictionary<string, Point> offsets = new Dictionary<string, Point>();
               
            //return offsets;
            string lightSourceName = "";
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color value = bitmap.GetPixel(x, y);

                    // BLUE=200-220 henviser til op til 21 light maps, hver identificeret med et tal fra 0-20,
                    // BLUE=221-255 henviser til generelle light maps som er alm. tilgængelige og kan benyttes på tværs af bygninger (og køretøjer).             
                    if (value.A > 0 && value.B >= 200) 
                    {
                        
                        try
                        {
                            if (value.B < 221) 
                            {
                                // camp_i_0_windows.png -> camp_0
                                lightSourceName = string.Format("{0}_{1}", buildingAndFlavourName, value.B - 200);
                                offsets.Add(lightSourceName, new Point(x, y));
                            }
                            else
                            {
                                // common_21_bigcirclegradient.png  -> common_21
                                lightSourceName = string.Format("common_{0}", value.B - 200);
                                offsets.Add(lightSourceName, new Point(x, y));
                            }
                        }
                        catch (Exception)
                        {
                            throw new Exception(string.Format("Error reading LightMapOffsets for '{0}', light source '{3}', x,y: {1},{2}",
                               buildingAndFlavourName, x, y, lightSourceName));
                        }
                    //    bitmap.SetPixel(x, y, value); //Sample4EdgesOfPixel(bitmap, x, y));
                    //    return offsets; //new Vector2(x, y);
                    }

                }
            }

            return offsets;
        }

        private void HandleDepthMapNoNormalize(string spriteName, string inputFilename, List<PixelBitmapContent<Vector4>> sourceSpritesDepthMaps, ContentProcessorContext context)
        {
            // handle depth map, if any is found:
            string depthMapFileName = spriteName + "_d";
            string depthMapFilePath = inputFilename.Replace(spriteName, depthMapFileName);
            //  System.Diagnostics.Debugger.Launch();
            context.Logger.LogImportantMessage(
                "Looking for Depth map on path: {0}", depthMapFilePath);

            if (System.IO.Path.GetExtension(depthMapFilePath) == ".tga")
            {   // try both tga and png.
                if (!File.Exists(depthMapFilePath))
                {
                    depthMapFileName = depthMapFileName.Replace(".tga", ".png");
                    depthMapFilePath = depthMapFilePath.Replace(".tga", ".png");
                }
            }
            else if (System.IO.Path.GetExtension(depthMapFilePath) == ".png")
            {
                if (!File.Exists(depthMapFilePath))
                {
                    depthMapFileName = depthMapFileName.Replace(".png", ".tga");
                    depthMapFilePath = depthMapFilePath.Replace(".png", ".tga");
                }
            }

            if (!File.Exists(depthMapFilePath))
            {
                if (spriteName.Contains("_construct"))
                {
                    depthMapFilePath = depthMapFilePath.Replace("_construct", "");
                }
                else if (spriteName.Contains("_burnt"))
                {
                    depthMapFilePath = depthMapFilePath.Replace("_burnt", "");
                }
            }

            if (File.Exists(depthMapFilePath))
            {
                context.Logger.LogImportantMessage("Found: {0}", depthMapFilePath);

                ExternalReference<TextureContent> depthMapTextureReference =
                            new ExternalReference<TextureContent>(depthMapFilePath);

                TextureContent depthMapTexture = context.BuildAndLoadAsset<TextureContent,
                                              TextureContent>(depthMapTextureReference, null);

                // convert to normal map:
                // Convert the input bitmap to Vector4 format, for ease of processing.
                depthMapTexture.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));

                PixelBitmapContent<Vector4> bitmap;
                bitmap = (PixelBitmapContent<Vector4>)depthMapTexture.Faces[0][0];

                // Calculate normalmap vectors.

                float[,] depthValues = new float[bitmap.Width, bitmap.Height];

                GetDepthValues(bitmap, depthValues, doDebugLogging, context);
                ConvertDepthToNormals(bitmap, depthValues, doDebugLogging, context);

                // Convert the result into NormalizedByte4 format.
               
                //*** TESTING CODE:
                /*  PixelBitmapContent<NormalizedByte4> convertedBitmap;
                  convertedBitmap = (PixelBitmapContent<NormalizedByte4>)depthMapTexture.Faces[0][0];
                  NormalizedByte4 pixelValue = convertedBitmap.GetPixel(43, 10);
                  context.Logger.LogImportantMessage(
                             "Converted pixel: {0}", pixelValue); 
                    */
                //*****

                sourceSpritesDepthMaps.Add((PixelBitmapContent<Vector4>)depthMapTexture.Faces[0][0]);


            }
            else
            {
                // add a placeholder:
                sourceSpritesDepthMaps.Add(null);
            }
        }

      /*  private void HandleDepthMap(string spriteName, string inputFilename, List<PixelBitmapContent<NormalizedByte4>> sourceSpritesDepthMaps, ContentProcessorContext context)
        {
            // handle depth map, if any is found:
            string depthMapFileName = spriteName + "_d";
            string depthMapFilePath = inputFilename.Replace(spriteName, depthMapFileName);
            //  System.Diagnostics.Debugger.Launch();
            context.Logger.LogImportantMessage(
                "Looking for Depth map on path: {0}", depthMapFilePath);

            if (System.IO.Path.GetExtension(depthMapFilePath) == ".tga")
            {   // try both tga and png.
                if (!File.Exists(depthMapFilePath))
                {
                    depthMapFileName = depthMapFileName.Replace(".tga", ".png");
                    depthMapFilePath = depthMapFilePath.Replace(".tga", ".png");
                }
            }
            else if (System.IO.Path.GetExtension(depthMapFilePath) == ".png")
            {
                if (!File.Exists(depthMapFilePath))
                {
                    depthMapFileName = depthMapFileName.Replace(".png", ".tga");
                    depthMapFilePath = depthMapFilePath.Replace(".png", ".tga");
                }
            }

            if (!File.Exists(depthMapFilePath))
            {
                if (spriteName.Contains("_construct"))
                {
                    depthMapFilePath = depthMapFilePath.Replace("_construct", "");
                }
                else if (spriteName.Contains("_burnt"))
                {
                    depthMapFilePath = depthMapFilePath.Replace("_burnt", "");
                }
            }

            if (File.Exists(depthMapFilePath))
            {
                context.Logger.LogImportantMessage("Found: {0}", depthMapFilePath);

                ExternalReference<TextureContent> depthMapTextureReference =
                            new ExternalReference<TextureContent>(depthMapFilePath);

                TextureContent depthMapTexture = context.BuildAndLoadAsset<TextureContent,
                                              TextureContent>(depthMapTextureReference, null);

                // convert to normal map:
                // Convert the input bitmap to Vector4 format, for ease of processing.
                depthMapTexture.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));

                PixelBitmapContent<Vector4> bitmap;
                bitmap = (PixelBitmapContent<Vector4>)depthMapTexture.Faces[0][0];

                // Calculate normalmap vectors.

                float[,] depthValues = new float[bitmap.Width, bitmap.Height];

                GetDepthValues(bitmap, depthValues, doDebugLogging, context);
                ConvertDepthToNormals(bitmap, depthValues, doDebugLogging, context);

                // Convert the result into NormalizedByte4 format.
                depthMapTexture.ConvertBitmapType(typeof(PixelBitmapContent<NormalizedByte4>));

              

                sourceSpritesDepthMaps.Add((PixelBitmapContent<NormalizedByte4>)depthMapTexture.Faces[0][0]);

                 
            }
            else
            {
                // add a placeholder:
                sourceSpritesDepthMaps.Add(null);
            }
        }*/


        public static void GetDepthValues(PixelBitmapContent<Vector4> bitmap, float[,] depthValues, bool doDebugLogging, ContentProcessorContext context)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Vector4 value = bitmap.GetPixel(x, y);
                    depthValues[x, y] = value.X;
                }
            }

        }

        /// <summary>
        /// Copies greyscale color information into the alpha channel.
        /// </summary>
        static void ConvertGreyToAlpha(PixelBitmapContent<Vector4> bitmap, float[,] alphaChannel, bool doDebugLogging, ContentProcessorContext context)
        {
            
            
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Vector4 value = bitmap.GetPixel(x, y);
                    Vector4 oldValue = value;
                    

                    alphaChannel[x, y] = value.W;

                    // Copy a greyscale version of the RGB data into the RED! channel //alpha channel.
                    float greyscale = (value.X + value.Y + value.Z) / 3;

                  //  value.W = greyscale;
                    value.X =  greyscale;

                    if (doDebugLogging)
                    {
                        context.Logger.LogImportantMessage(
                            "Bitmap value BEFORE at x,y ({0},{1}): {2}, computed greyscale: {3}, AFTER: {4}", x, y, oldValue, greyscale, bitmap.GetPixel(x, y));
                    }

                    bitmap.SetPixel(x, y, value);
                }
            }
        }

        /// <summary>
        /// Copies greyscale color information into the alpha channel.
        /// </summary>
        static void ConvertToGrey(PixelBitmapContent<Vector4> bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Vector4 value = bitmap.GetPixel(x, y);

                    // Copy a greyscale version of the RGB data into the alpha channel.
                    float greyscale = (value.X + value.Y + value.Z) / 3;
                    
                    value.X = greyscale;
                    value.Y = greyscale;
                    value.Z = greyscale;

                    bitmap.SetPixel(x, y, value);
                }
            }
        }




        /// <summary>
        /// Using height data stored in the alpha channel, computes normalmap
        /// vectors and stores them in the RGB portion of the bitmap.
        /// </summary>
        public static void ConvertDepthToNormals(PixelBitmapContent<Vector4> bitmap, float[,] depthValue, bool doDebugLogging, 
                                                   ContentProcessorContext context)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {                 
                    // Look up the heights to either side of this pixel.
                    float left = GetHeight(depthValue, x - 1, y);
                    float right = GetHeight(depthValue, x + 1, y);

                    float top = GetHeight(depthValue, x, y - 1);
                    float bottom = GetHeight(depthValue, x, y + 1);

                    // Compute gradient vectors, then cross them to get the normal.
                    Vector3 dx = new Vector3(1, 0, (right - left) * bumpSize);
                    Vector3 dy = new Vector3(0, 1, (top - bottom) * bumpSize);//(bottom - top) * bumpSize); // invert y - positive is now up.
                  //  Vector3 dz = new Vector3(0, 1, (bottom - top) * bumpSize); 

                    Vector3 normal = Vector3.Cross(dx, dy);
                 //   Vector3 normal = Vector3.Cross(dz, dx);

                    normal.Normalize();

                    // Store the result.
                    float alpha = bitmap.GetPixel(x, y).W;
                    bitmap.SetPixel(x, y, new Vector4(normal.X, normal.Z, -normal.Y, alpha)); // in Conlan's, the z axis points down into the ground.
                  //  bitmap.SetPixel(x, y, new Vector4(normal.X, normal.Z, -normal.Y, bitmap.GetPixel(x, y).W)); // in Conlan's, the z axis points down into the ground.
                   // bitmap.SetPixel(x, y, new Vector4(bitmap.GetPixel(x, y).W, normal.X, normal.Z, -normal.Y)); // shifted to the right

                    if (doDebugLogging)
                    {
                        Vector4 thisPixel = bitmap.GetPixel(x, y);
                        context.Logger.LogImportantMessage(
                              "Normal at x,y ({0},{1}): {2}, pixel value: {3}, (dx, dy): {4}, {5} ", x, y, normal, thisPixel, dx, dy); 
                     /*       "Normal at x,y ({0},{1}): {2}, pixel value: {3}, (left, right): {4}, {5} ", x, y, normal, thisPixel,
                            bitmap.GetPixel(ClampX(bitmap, x - 1), y),
                            bitmap.GetPixel(ClampX(bitmap, x + 1), y));
                                //GetHeight(bitmap, x - 1, y), GetHeight(bitmap, x + 1, y)); */
                    }
                }
            }
        }

        /// <summary>
        /// Using height data stored in the alpha channel, computes normalmap
        /// vectors and stores them in the RGB portion of the bitmap.
        /// </summary>
   /*     static void ConvertAlphaToNormals_OLD(PixelBitmapContent<Vector4> bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Look up the heights to either side of this pixel.
                    float left = GetHeight(bitmap, x - 1, y);
                    float right = GetHeight(bitmap, x + 1, y);

                    float top = GetHeight(bitmap, x, y - 1);
                    float bottom = GetHeight(bitmap, x, y + 1);

                    // Compute gradient vectors, then cross them to get the normal.
                    Vector3 dx = new Vector3(1, 0, (right - left) * bumpSize);
                    Vector3 dy = new Vector3(0, 1, (bottom - top) * bumpSize);

                    Vector3 normal = Vector3.Cross(dx, dy);

                    normal.Normalize();

                    // Store the result.
                     float alpha = GetHeight(bitmap, x, y);

                    bitmap.SetPixel(x, y, new Vector4(normal, alpha));
                }
            }
        }*/

        public static int ClampX(PixelBitmapContent<Vector4> bitmap, int x)
        {
            if (x < 0)
            {
                return 0;
            }
            else if (x >= bitmap.Width)
            {
                return bitmap.Width - 1;
            }
            return x;
        }

        public static int ClampY(PixelBitmapContent<Vector4> bitmap, int y)
        {
            if (y < 0)
            {
                y = 0;
            }
            else if (y >= bitmap.Height)
            {
                y = bitmap.Height - 1;
            }

            return y;
        }
        
        /// <summary>
        /// Helper for looking up height values from the bitmap,
        /// clamping if the specified position is off the edge of the bitmap.
        /// </summary>
        static float GetHeight(/*PixelBitmapContent<Vector4> bitmap*/ float[,] depthValue, int x, int y)
        {
            if (x < 0)
            {
                x = 0;
            }
            else if (x >= depthValue.GetLength(0))
            {
                x = depthValue.GetLength(0) - 1;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y >= depthValue.GetLength(1))
            {
                y = depthValue.GetLength(1) - 1;
            }

            return depthValue[x, y];
            // NEW: look at Red component:
           // return bitmap.GetPixel(x, y).X; // X; // W;
        }
    }
}
