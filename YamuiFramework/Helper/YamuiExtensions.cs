﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YamuiFramework.Helper {
    public static class YamuiExtensions {

        /// <summary>
        /// Returns a collection of all the values of a given Enum
        /// </summary>
        public static IEnumerable<T> GetEnumValues<T>(this Enum value) {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        #region Math lol

        /// <summary>
        /// Forces a value between a minimum and a maximum
        /// </summary>
        public static int Clamp(this int value, int min, int max) {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        /// <summary>
        /// Forces a value above a minimum
        /// </summary>
        public static int ClampMin(this int value, int min) {
            if (value < min)
                return min;
            return value;
        }

        /// <summary>
        /// Forces a value under a maximum
        /// </summary>
        public static int ClampMax(this int value, int max) {
            if (value > max)
                return max;
            return value;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts a string to an object of the given type
        /// </summary>
        public static object ConvertFromStr(this string value, Type destType) {
            try {
                if (destType == typeof(string))
                    return value;
                return TypeDescriptor.GetConverter(destType).ConvertFromInvariantString(value);
            } catch (Exception) {
                return destType.IsValueType ? Activator.CreateInstance(destType) : null;
            }
        }

        /// <summary>
        /// Converts an object to a string
        /// </summary>
        public static string ConvertToStr(this object value) {
            if (value == null)
                return string.Empty;
            return TypeDescriptor.GetConverter(value).ConvertToInvariantString(value);
        }

        #endregion

        #region GetRoundedRect

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <returns>A round cornered rectagle path</returns>   
        public static GraphicsPath GetRoundedRect(float x, float y, float width, float height, float diameter) {
            return new RectangleF(x, y, width, height).GetRoundedRect(diameter);
        }

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="diameter">The diameter of the corners</param>
        /// <returns>A round cornered rectagle path</returns>
        public static GraphicsPath GetRoundedRect(this RectangleF rect, float diameter) {
            GraphicsPath path = new GraphicsPath();

            if (diameter > 0) {
                RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
            } else {
                path.AddRectangle(rect);
            }

            return path;
        }

        #endregion

        #region Colors extensions

        /// <summary>
        /// Replace all the occurences of @alias thx to the aliasDictionnary
        /// </summary>
        /// <param name="value"></param>
        /// <param name="aliasDictionnary"></param>
        /// <returns></returns>
        public static string ReplaceAliases(this string value, Dictionary<string, string> aliasDictionnary) {
            while (true) {
                if (value.Contains("@")) {
                    // try to replace a variable name by it's html color value
                    var regex = new Regex(@"@([a-zA-Z]*)", RegexOptions.IgnoreCase);
                    value = regex.Replace(value, match => {
                        if (aliasDictionnary.ContainsKey(match.Groups[1].Value))
                            return aliasDictionnary[match.Groups[1].Value];
                        throw new Exception("Couldn't find the color " + match.Groups[1].Value + "!");
                    });
                    continue;
                }
                return value;
            }
        }

        /// <summary>
        /// Allows to have the syntax : 
        /// lighten(#000000, 35%)
        /// darken(#FFFFFF, 35%)
        /// </summary>
        /// <param name="htmlColor"></param>
        /// <returns></returns>
        public static string ApplyColorFunctions(this string htmlColor) {
            if (htmlColor.Contains("(")) {
                var functionName = htmlColor.Substring(0, htmlColor.IndexOf("(", StringComparison.CurrentCultureIgnoreCase));
                var splitValues = htmlColor.GetBetweenMostNested("(", ")").Split(',');
                float ratio;
                if (!float.TryParse(splitValues[1].Trim().Replace("%", ""), out ratio))
                    ratio = 0;

                // Apply the color function to the base color (in case this is another function embedded in this one)
                var baseColor = splitValues[0].Trim().ApplyColorFunctions();

                if (functionName.StartsWith("dark"))
                    return baseColor.ModifyColorLuminosity(-1 * ratio / 100);
                if (functionName.StartsWith("light"))
                    return baseColor.ModifyColorLuminosity(ratio / 100);

                return baseColor;
            }
            return htmlColor;
        }

        /// <summary>
        /// Lighten or darken a color, ratio + to lighten, - to darken
        /// </summary>
        public static string ModifyColorLuminosity(this string htmlColor, float ratio) {
            var color = ColorTranslator.FromHtml(htmlColor);
            return ColorTranslator.ToHtml(ratio > 0 ? ControlPaint.Light(color, ratio) : ControlPaint.Dark(color, ratio));
            /*
            var isBlack = color.R == 0 && color.G == 0 && color.B == 0;
            var red = (int)Math.Min(Math.Max(0, color.R + ((isBlack ? 255 : color.R) * ratio)), 255);
            var green = (int)Math.Min(Math.Max(0, color.G + ((isBlack ? 255 : color.G) * ratio)), 255);
            var blue = (int)Math.Min(Math.Max(0, color.B + ((isBlack ? 255 : color.B) * ratio)), 255);
            return ColorTranslator.ToHtml(Color.FromArgb(red, green, blue));
             */
        }

        /// <summary>
        /// Get string value between [first] a and [last] b (not included)
        /// </summary>
        public static string GetBetweenMostNested(this string value, string a, string b, StringComparison comparer = StringComparison.CurrentCultureIgnoreCase) {
            int posA = value.LastIndexOf(a, comparer);
            int posB = value.IndexOf(b, comparer);
            return posB == -1 ? value.Substring(posA + 1) : value.Substring(posA + 1, posB - posA - 1);
        }

        #endregion

        #region image manipulation

        /// <summary>
        /// Returns the image in grey scale...
        /// </summary>
        public static Image MakeGreyscale3(this Image original) {

            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
                new[] {
                    new[] {.3f, .3f, .3f, 0, 0},
                    new[] {.59f, .59f, .59f, 0, 0},
                    new[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            var attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        #endregion
    }
}
