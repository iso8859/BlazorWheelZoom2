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
    public class Rect
    {
        public double? left, top, right, bottom;

        public Rect()
        {

        }

        public Rect(double _left, double _top, double _right, double _bottom)
        {
            if (_right < _left)
            {
                double t = _right;
                _right = _left;
                _left = t;
            }
            if (_bottom < _top)
            {
                double t = _bottom;
                _bottom = _top;
                _top = t;
            }
            left = _left;
            top = _top;
            right = _right;
            bottom = _bottom;
        }

        public Rect NomalizedRect()
        {
            if (left.HasValue && top.HasValue && right.HasValue && bottom.HasValue)
                return new Rect(left.Value, top.Value, right.Value, bottom.Value);
            else
                return new Rect(0, 0, 0, 0);
        }

        public bool IsEmpty() => !left.HasValue || !top.HasValue || !right.HasValue || !bottom.HasValue;
        public double? width
        {
            get => (right.HasValue && left.HasValue) ? (right.Value - left.Value) : null;
            set
            {
                if (left.HasValue && value.HasValue)
                    right = left.Value + value.Value;
            }
        }

        public double? height
        {
            get => (bottom.HasValue && top.HasValue) ? (bottom.Value - top.Value) : null;
            set
            {
                if (top.HasValue && value.HasValue)
                    bottom = top.Value + value.Value;
            } 
        }

        public static Rect Intersect(Rect a, Rect b)
        {
            double x1 = Math.Max(a.left.Value, b.top.Value);
            double x2 = Math.Min(a.left.Value + a.width.Value, b.left.Value + b.width.Value);
            double y1 = Math.Max(a.top.Value, b.top.Value);
            double y2 = Math.Min(a.top.Value + a.height.Value, b.top.Value + b.height.Value);

            if (x2 >= x1 && y2 >= y1)
            {
                return new Rect(x1, y1, x2 - x1, y2 - y1);
            }

            return new Rect();
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
        public Rect viewSize; // Size of the visible part zoom 1
        public Rect areaSize; // Size of the drawing part zoom 1

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
        public void EnsureIsVisible(Rect area)
        {
            x_offset = Clip(Size(-area.left.Value + 10), viewSize.width.Value - Size(areaSize.width.Value));
            y_offset = Clip(Size(-area.top.Value + 10), viewSize.height.Value - Size(areaSize.height.Value));
        }
    }

    public class SVGObject
    {
        static public readonly string _white = "#FFFFFF";
        static public readonly string _black = "#000000";
        static public readonly string _none = "none";
        static public readonly string _fillopacity = "1";

        public string id;
        public Rect rect = new Rect();
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
            // To avoi negative size rect
            var tmp = $"<rect {GetId()} x='{context.TranslateX(rect.left.Value)}' y='{context.TranslateY(rect.top.Value)}' width='{context.Size_s(Math.Abs(rect.width.Value))}' height='{context.Size_s(Math.Abs(rect.height.Value))}' fill='{fill}' fill-opacity='{fill_opacity}' stroke='{color}' stroke-width='1'/>";
            //Console.WriteLine("SVGRectangle zoom =" + context.zoom);
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
                if (rect.width.HasValue)
                    tmp += $"width='{context.Size_s(Math.Abs(rect.width.Value))}' ";
                if (rect.height.HasValue)
                    tmp += $"height='{context.Size_s(Math.Abs(rect.height.Value))}'";
                tmp += "/>";
                //Console.WriteLine("SVGImage zoom =" + context.zoom);
                return tmp;
            }
            else
                return "";
        }
    }
}
