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
    public class rect
    {
        public double? left, top, right, bottom;

        public rect()
        {
                
        }

        public rect(double _left, double _top, double _right, double _bottom)
        {
            left = _left;
            top = _top;
            right = _right;
            bottom = _bottom;
        }

        public bool IsEmpty() => !left.HasValue || !top.HasValue || !right.HasValue || !bottom.HasValue;
        public double width
        {
            get => right.Value - left.Value;
            set => right = left.Value + value;
        }

        public double height
        {
            get => bottom.Value - top.Value;
            set => bottom = top.Value + value;
        }

        public static rect Intersect(rect a, rect b)
        {
            double x1 = Math.Max(a.left.Value, b.top.Value);
            double x2 = Math.Min(a.left.Value + a.width, b.left.Value + b.width);
            double y1 = Math.Max(a.top.Value, b.top.Value);
            double y2 = Math.Min(a.top.Value + a.height, b.top.Value + b.height);

            if (x2 >= x1 && y2 >= y1)
            {
                return new rect(x1, y1, x2 - x1, y2 - y1);
            }

            return new rect();
        }

        public override string ToString()
        {
            return $"left={left};top={top};right={right};bottom={bottom}";
        }
    }
    public class SVGContext
    {
        public string clip_name = "clip-path";
        public double x_offset = 0, y_offset = 0, zoom = 1;
        public rect viewSize; // Size of the visible part zoom 1
        public rect areaSize; // Size of the drawing part zoom 1

        public override string ToString()
        {
            return $"x_offset={x_offset};y_offset={y_offset};zoom={zoom};viewSize={viewSize};areaSize={areaSize}";
        }
        public double Clip(double i, double minOffset)
        {
            return Math.Min(0, Math.Max(i, minOffset));
        }

        public void Reset()
        {
            x_offset = y_offset = 0;
            zoom = 1;
        }

        public bool IsValid()
        {
            return viewSize != null && areaSize != null;
        }

        public double ScreenToViewX(double i) => (i - x_offset) / zoom;
        public double ScreenToViewY(double i) => (i - y_offset) / zoom;
        public string TranslateX(double x) => (x_offset + (x * zoom)).ToStringInvariant();
        public string TranslateY(double y) => (y_offset + (y * zoom)).ToStringInvariant();
        public double Size(double i) => i * zoom;
        public string Size_s(double i) => (i * zoom).ToStringInvariant();
        public void EnsureIsVisible(rect area)
        {
            x_offset = Clip(Size(-area.left.Value+10), viewSize.width - Size(areaSize.width));
            y_offset = Clip(Size(-area.top.Value+10), viewSize.height - Size(areaSize.height));
        }
    }

    public class SVGObject
    {
        static public readonly string _white = "#FFFFFF";
        static public readonly string _black = "#000000";
        static public readonly string _none = "none";
        static public readonly string _fillopacity = "1";

        public string id;
        public rect rect = new rect();
        public bool scrollIntoView = false;
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
        public string color = _black;
        public string fill = _none;
        public string fill_opacity = _fillopacity;
        public override string GetSVG(SVGContext context)
        {
            var tmp = $"<rect {GetId()} x='{context.TranslateX(rect.left.Value)}' y='{context.TranslateY(rect.top.Value)}' width='{context.Size_s(rect.width)}' height='{context.Size_s(rect.height)}' fill='{fill}' fill-opacity='{fill_opacity}' stroke='{color}' stroke-width='1'/>";
            //Console.WriteLine(tmp);
            return tmp;
        }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions() { IncludeFields = true });
        }
    }

    public class SVGImage : SVGObject
    {
        public string href;
        // https://stackoverflow.com/questions/11390830/is-it-possible-to-listen-image-load-event-in-svg
        // https://stackoverflow.com/questions/6575159/get-image-dimensions-with-javascript-before-image-has-fully-loaded
        public override string GetSVG(SVGContext context)
        {
            if (!string.IsNullOrEmpty(href))
            {
                var tmp = $"<image {GetId()} xlink:href='{href}' ";
                if (rect.left.HasValue)
                    tmp += $"x='{context.TranslateX(rect.left.Value)}' ";
                if (rect.top.HasValue)
                    tmp += $"y='{context.TranslateY(rect.top.Value)}' ";
                if (rect.left.HasValue && rect.right.HasValue)
                    tmp += $"width='{context.Size_s(rect.width)}' ";
                if (rect.top.HasValue && rect.bottom.HasValue)
                    tmp += $"height='{context.Size_s(rect.height)}'";
                tmp += "/>";
                //Console.WriteLine(tmp);
                return tmp;
            }
            else
                return "";
        }
    }
}
