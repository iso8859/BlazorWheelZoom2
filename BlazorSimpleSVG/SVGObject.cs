using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSimpleSVG
{

    static public class Extensions
    {
        static public string ToStringInvariant(this double val) => val.ToString(CultureInfo.InvariantCulture);
    }
    public class SVGContext
    {
        public string clip_name = "clip-path";
        public double x_offset = 0, y_offset = 0, zoom = 1;

        public double Clip(double i, double minOffset)
        {
            return Math.Min(0, Math.Max(i, minOffset));
        }

        public double ScreenToViewX(double i) => (i - x_offset) / zoom;
        public double ScreenToViewY(double i) => (i - y_offset) / zoom;
        public string TranslateX(double x) => (x_offset + (x * zoom)).ToStringInvariant();
        public string TranslateY(double y) => (y_offset + (y * zoom)).ToStringInvariant();
        public double Size(double i) => i * zoom;
        public string Size_s(double i) => (i * zoom).ToStringInvariant();
    }

    public class SVGObject
    {
        static public readonly string _white = "#FFFFFF";
        static public readonly string _black = "#000000";
        static public readonly string _none = "none";
        static public readonly string _fillopacity = "1";

        public string id;
        public bool visible = true;
        public virtual string GetSVG(SVGContext context) { return ""; }

        public string GetId()
        {
            if (!string.IsNullOrEmpty(id))
                return $"id='{id}'";
            else
                return "";
        }
    }

    public class SVGRectangle : SVGObject
    {
        public double left, top, right, bottom;
        public string color = _black;
        public string fill = _none;
        public string fill_opacity = _fillopacity;

        public override string GetSVG(SVGContext context)
        {
            var tmp = $"<rect {GetId()} x='{context.TranslateX(left)}' y='{context.TranslateY(top)}' width='{context.Size_s(right - left)}' height='{context.Size_s(bottom - top)}' fill='{fill}' fill-opacity='{fill_opacity}' stroke='{color}' stroke-width='1'/>";
            // Console.WriteLine(tmp);
            return tmp;
        }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions() { IncludeFields = true });
        }
    }

    public class SVGImage : SVGObject
    {
        public double left, top, right = -1, bottom = -1;
        public string href;

        public override string GetSVG(SVGContext context)
        {
            var tmp = $"<image {GetId()} xlink:href='{href}' x='{context.TranslateX(left)}' y='{context.TranslateY(top)}' ";
            if (right != -1)
                tmp += $"width='{context.Size_s(right - left)}' ";
            if (bottom != -1)
                tmp += $"height='{context.Size_s(bottom - top)}'";
            tmp += "/>";
            //Console.WriteLine(tmp);
            return tmp;

        }
    }
}
