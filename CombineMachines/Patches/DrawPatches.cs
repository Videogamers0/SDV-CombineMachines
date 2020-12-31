using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;
using CombineMachines.Helpers;
using Microsoft.Xna.Framework;
using StardewValley;

namespace CombineMachines.Patches
{
    [HarmonyPatch(typeof(SObject), nameof(SObject.draw))]
    public static class DrawPatch
    {
        public static void Postfix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            try
            {
                if (__instance.TryGetCombinedQuantity(out int CombinedQuantity) && CombinedQuantity > 1)
                {
                    float Transparency = alpha * ModEntry.UserConfig.NumberOpacity;
                    if (Transparency > 0f)
                    {
                        Color RenderColor = Color.White * Transparency;

                        float draw_layer = GetTileDrawLayerDepth(x, y);
                        float DrawLayerOffset = 1E-05f; // The SpriteBatch LayerDepth needs to be slightly larger than the layer depth used for the bigCraftable texture to avoid z-fighting

                        Vector2 TopLeftTilePosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize, y * Game1.tileSize));
                        Vector2 BottomRightTilePosition = Game1.GlobalToLocal(Game1.viewport, new Vector2((x + 1) * Game1.tileSize - 1, (y + 1) * Game1.tileSize - 1));

                        float Scale = 3.0f;
                        float QuantityWidth = DrawHelpers.MeasureNumber(CombinedQuantity, Scale);
                        Vector2 QuantityTopLeftPosition = new Vector2(BottomRightTilePosition.X - QuantityWidth, BottomRightTilePosition.Y - DrawHelpers.TinyDigitBaseHeight - Game1.tileSize / 8);
                        Utility.drawTinyDigits(CombinedQuantity, spriteBatch, QuantityTopLeftPosition, Scale, draw_layer + DrawLayerOffset, RenderColor);
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.Logger.Log(string.Format("Unhandled Error in {0}.{1}:\n{2}", nameof(DrawPatch), nameof(Postfix), ex), LogLevel.Error);
            }
        }

        /// <summary>Returns the layerDepth value to use when calling <see cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, float, SpriteEffects, float)"/> so that 
        /// the drawn texture at the given tile appears at the correct layer. (Objects placed on tiles at higher Y-values should appear over-top of this tile's textures)</summary>
        private static float GetTileDrawLayerDepth(int TileX, int TileY)
        {
            // Copied from decompiled code: StardewValley.Object.cs
            // public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
            // when it's drawing the bigCraftable spritesheet's texture
            float draw_layer = Math.Max(0f, (float)((TileY + 1) * 64 - 24) / 10000f) + (float)TileX * 1E-05f;
            return draw_layer;
        }
    }

    [HarmonyPatch(typeof(SObject), nameof(SObject.drawInMenu))]
    public static class DrawInMenuPatch
    {
        public static void Postfix(SObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            try
            {
                DrawCombinedStack(__instance, spriteBatch, location, scaleSize, transparency, color);
            }
            catch (Exception ex)
            {
                ModEntry.Logger.Log(string.Format("Unhandled Error in {0}.{1}:\n{2}", nameof(DrawInMenuPatch), nameof(Postfix), ex), LogLevel.Error);
            }
        }

        /// <param name="location">The top-left position of the inventory slot</param>
        public static void DrawCombinedStack(SObject instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, Color color)
        {
            if (instance.TryGetCombinedQuantity(out int CombinedQuantity) && CombinedQuantity > 1)
            {
                float Transparency = ModEntry.UserConfig.NumberOpacity;
                if (Transparency >= 0f)
                {
                    float Scale = 3.0f * scaleSize;
                    Color RenderColor = Color.Gold * transparency;
                    Vector2 RenderPosition = location + new Vector2((64 - Utility.getWidthOfTinyDigitString(CombinedQuantity, Scale)) + Scale, 64f - 18f * scaleSize + 2f);
                    Utility.drawTinyDigits(CombinedQuantity, spriteBatch, RenderPosition, Scale, Transparency, RenderColor);
                }
            }
        }
    }
}
