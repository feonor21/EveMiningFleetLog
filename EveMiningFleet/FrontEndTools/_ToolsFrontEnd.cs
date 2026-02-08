using System;
using System.Globalization;


public class _ToolsFrontEnd
{

    public static string DisplayPrice(double value)
    {
        var f = new NumberFormatInfo();

        f.NumberGroupSeparator = " ";// separateur des milliers
        f.NumberDecimalDigits = 2;// nombre apres la virgule

        if (value >= Math.Pow(10, 12))
            return Math.Round(value / Math.Pow(10, 12), f.NumberDecimalDigits).ToString("N", f) + "T" + " Isk";
        else if (value >= Math.Pow(10, 9))
            return Math.Round(value / Math.Pow(10, 9), f.NumberDecimalDigits).ToString("N", f) + "B" + " Isk";
        else if (value >= Math.Pow(10, 6))
            return Math.Round(value / Math.Pow(10, 6), f.NumberDecimalDigits).ToString("N", f) + "M" + " Isk";
        else if (value >= Math.Pow(10, 3))
            return Math.Round(value / Math.Pow(10, 3), f.NumberDecimalDigits).ToString("N", f) + "k" + " Isk";
        else
            return Math.Round(value, f.NumberDecimalDigits).ToString("N", f) + "";
    }
    public static string DisplayVolume(double value)
    {
        var f = new NumberFormatInfo();

        f.NumberGroupSeparator = " ";// separateur des milliers
        f.NumberDecimalDigits = 2;// nombre apres la virgule

        if (value >= Math.Pow(10, 12))
            return Math.Round(value / Math.Pow(10, 12), f.NumberDecimalDigits).ToString("N", f) + "T" + " m³";
        else if (value >= Math.Pow(10, 9))
            return Math.Round(value / Math.Pow(10, 9), f.NumberDecimalDigits).ToString("N", f) + "B" + " m³";
        else if (value >= Math.Pow(10, 6))
            return Math.Round(value / Math.Pow(10, 6), f.NumberDecimalDigits).ToString("N", f) + "M" + " m³";
        else if (value >= Math.Pow(10, 3))
            return Math.Round(value / Math.Pow(10, 3), f.NumberDecimalDigits).ToString("N", f) + "k" + " m³";
        else
        {
            return Math.Round(value, f.NumberDecimalDigits).ToString("N", f) + "" + " m³";
        }
    }
    public static string DisplayQuantity(double value)
    {
        var f = new NumberFormatInfo();

        f.NumberGroupSeparator = " ";// separateur des milliers
        f.NumberDecimalDigits = 2;// nombre apres la virgule


        if (value >= Math.Pow(10, 12))
            return Math.Round(value / Math.Pow(10, 12), f.NumberDecimalDigits).ToString("N", f) + "T";
        else if (value >= Math.Pow(10, 9))
            return Math.Round(value / Math.Pow(10, 9), f.NumberDecimalDigits).ToString("N", f) + "B";
        else if (value >= Math.Pow(10, 6))
            return Math.Round(value / Math.Pow(10, 6), f.NumberDecimalDigits).ToString("N", f) + "M";
        else if (value >= Math.Pow(10, 3))
            return Math.Round(value / Math.Pow(10, 3), f.NumberDecimalDigits).ToString("N", f) + "k";
        else
        {
            f.NumberDecimalDigits = 0;
            return Math.Round(value, f.NumberDecimalDigits).ToString("N", f) + "";
        }
    }
    public static string DisplayFullQuantity(double value)
    {
        var f = new NumberFormatInfo();

        f.NumberGroupSeparator = " ";// separateur des milliers

        f.NumberDecimalDigits = 0;
        return Math.Round(value, f.NumberDecimalDigits).ToString("N", f) + "";
    }
    public static string DisplayDateTimeBasic(DateTime value, int timeZone)
    {
        return value.AddMinutes(timeZone * -1).ToString("yyyy/MM/dd HH:mm");
    }



}

