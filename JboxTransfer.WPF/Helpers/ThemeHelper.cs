﻿using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JboxTransfer.Helpers
{
    public static class ThemeHelper
    {
        static PaletteHelper _paletteHelper;

        public static void ApplyBase(object isDark)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme((bool)isDark ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        public static void ChangeHue2(object c)
        {
            var hue = StringToColor(c.ToString());
            _paletteHelper = new PaletteHelper();
            _paletteHelper.ChangePrimaryColor(hue);
        }

        public static void ChangeHue3(object c)
        {
            var hue = StringToColor(c.ToString());
            _paletteHelper = new PaletteHelper();
            ITheme theme = _paletteHelper.GetTheme();

            theme.PrimaryLight = new ColorPair(hue.Lighten(), theme.Body);
            theme.PrimaryMid = new ColorPair(hue, theme.Body);
            theme.PrimaryDark = new ColorPair(hue.Darken(), theme.Body);
            if (theme is Theme internalTheme)
            {
                internalTheme.ColorAdjustment = new ColorAdjustment() { Colors = ColorSelection.All, DesiredContrastRatio = 4.5f, Contrast = Contrast.Medium };
            }
            _paletteHelper.SetTheme(theme);
        }

        public static void ChangeHue(object c)
        {
            if (c == null) return;
            var hue = StringToColor(c.ToString());
            _paletteHelper = new PaletteHelper();
            ITheme theme = _paletteHelper.GetTheme();

            theme.SetPrimaryColor(hue);

            if (theme is Theme internalTheme)
            {
                internalTheme.ColorAdjustment = new ColorAdjustment() { Colors = ColorSelection.All, DesiredContrastRatio = 4.5f, Contrast = Contrast.Medium };
            }
            _paletteHelper.SetTheme(theme);
        }

        public static void ChangeHue1(string pri, string sec)
        {
            var hue1 = StringToColor(pri);
            var hue2 = StringToColor(sec);
            _paletteHelper = new PaletteHelper();
            ITheme theme = _paletteHelper.GetTheme();

            theme.PrimaryLight = new ColorPair(hue1.Lighten(), Colors.White);
            theme.PrimaryMid = new ColorPair(hue1, Colors.White);
            theme.PrimaryDark = new ColorPair(hue1.Darken(), Colors.White);
            theme.SecondaryLight = new ColorPair(hue2.Lighten(), Colors.White);
            theme.SecondaryMid = new ColorPair(hue2, Colors.White);
            theme.SecondaryDark = new ColorPair(hue2.Darken(), Colors.White);
            if (theme is Theme internalTheme)
            {
                internalTheme.ColorAdjustment = new ColorAdjustment() { Colors = ColorSelection.All, DesiredContrastRatio = 2.5f, Contrast = Contrast.Medium };
            }
            _paletteHelper.SetTheme(theme);
        }

        public static Color StringToColor(this string colorStr)
        {
            TypeConverter cc = TypeDescriptor.GetConverter(typeof(Color));
            var result = (Color)cc.ConvertFromString(colorStr);
            return result;
        }

        public static string ColorToHex(this Color color)
        {
            string hex = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
            return hex;
        }
    }
    public static class PaletteHelperExtensions
    {
        public static void ChangePrimaryColor(this PaletteHelper paletteHelper, Color color)
        {
            ITheme theme = paletteHelper.GetTheme();

            theme.PrimaryLight = new ColorPair(color.Lighten(), theme.PrimaryLight.ForegroundColor);
            theme.PrimaryMid = new ColorPair(color, theme.PrimaryMid.ForegroundColor);
            theme.PrimaryDark = new ColorPair(color.Darken(), theme.PrimaryDark.ForegroundColor);

            if (theme is Theme internalTheme)
            {
                internalTheme.ColorAdjustment = new ColorAdjustment() { Colors = ColorSelection.All, DesiredContrastRatio = 4.5f, Contrast = Contrast.Medium };
            }
            paletteHelper.SetTheme(theme);
        }

        public static void ChangeSecondaryColor(this PaletteHelper paletteHelper, Color color)
        {
            ITheme theme = paletteHelper.GetTheme();

            theme.SecondaryLight = new ColorPair(color.Lighten(), theme.SecondaryLight.ForegroundColor);
            theme.SecondaryMid = new ColorPair(color, theme.SecondaryMid.ForegroundColor);
            theme.SecondaryDark = new ColorPair(color.Darken(), theme.SecondaryDark.ForegroundColor);

            paletteHelper.SetTheme(theme);
        }
    }

}
