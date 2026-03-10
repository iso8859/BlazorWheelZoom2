using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            
            SetLock(_left, _top, _right, _bottom);
        }

        public void Set(double _left, double _top, double _right, double _bottom)
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

        public void SetLock(double _left, double _top, double _right, double _bottom)
        {
            lock (this)
            {
                Set(_left, _top, _right, _bottom);
            }
        }

        public Rect NomalizedRect()
        {
            lock (this)
            {
                if (left.HasValue && top.HasValue && right.HasValue && bottom.HasValue)
                    return new Rect(left.Value, top.Value, right.Value, bottom.Value);
                else
                    return new Rect(0, 0, 0, 0);
            }
        }

        public bool IsEmpty()
        {
            lock (this)
                return !left.HasValue || !top.HasValue || !right.HasValue || !bottom.HasValue;
        }
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
        public double x_offset = 0, y_offset = 0;
        public double zoom = 1;
        public double rotation = 0; // Degrees
        public Rect viewSize; // Size of the visible part zoom 1
        public Rect areaSize; // Size of the drawing part zoom 1

        public override string ToString()
        {
            return $"x_offset={x_offset};y_offset={y_offset};zoom={zoom};rotation={rotation};viewSize={viewSize};areaSize={areaSize}";
        }
        
        public double Clip(double i, double minOffset)
        {
            return Math.Min(0, Math.Max(i, minOffset));
        }

        public void Reset()
        {
            x_offset = y_offset = 0;
            zoom = 1;
            rotation = 0;
        }

        public bool IsValid()
        {
            return viewSize != null && areaSize != null;
        }

        /// <summary>
        /// Définit la rotation en degrés
        /// </summary>
        public void SetRotation(double degrees)
        {
            rotation = degrees % 360;
            if (rotation < 0)
                rotation += 360;
        }

        /// <summary>
        /// Fait pivoter de 90 degrés dans le sens horaire
        /// </summary>
        public void RotateClockwise()
        {
            SetRotation(rotation + 90);
        }

        /// <summary>
        /// Fait pivoter de 90 degrés dans le sens antihoraire
        /// </summary>
        public void RotateCounterClockwise()
        {
            SetRotation(rotation - 90);
        }

        /// <summary>
        /// Convertit des degrés en radians
        /// </summary>
        private double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

        /// <summary>
        /// Obtient le centre de rotation basé sur areaSize
        /// </summary>
        private (double centerX, double centerY) GetRotationCenter()
        {
            if (areaSize != null && !areaSize.IsEmpty())
            {
                return (areaSize.width.Value / 2.0, areaSize.height.Value / 2.0);
            }
            return (0, 0);
        }

        /// <summary>
        /// Applique la transformation de rotation à un point
        /// </summary>
        private (double x, double y) ApplyRotation(double x, double y)
        {
            if (rotation == 0)
                return (x, y);

            var (centerX, centerY) = GetRotationCenter();
            double radians = DegreesToRadians(rotation);
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            // Translate to origin
            double translatedX = x - centerX;
            double translatedY = y - centerY;

            // Rotate
            double rotatedX = translatedX * cos - translatedY * sin;
            double rotatedY = translatedX * sin + translatedY * cos;

            // Translate back
            return (rotatedX + centerX, rotatedY + centerY);
        }

        /// <summary>
        /// Applique la transformation inverse de rotation à un point
        /// </summary>
        private (double x, double y) ApplyInverseRotation(double x, double y)
        {
            if (rotation == 0)
                return (x, y);

            var (centerX, centerY) = GetRotationCenter();
            double radians = DegreesToRadians(-rotation); // Inverse rotation
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            // Translate to origin
            double translatedX = x - centerX;
            double translatedY = y - centerY;

            // Rotate
            double rotatedX = translatedX * cos - translatedY * sin;
            double rotatedY = translatedX * sin + translatedY * cos;

            // Translate back
            return (rotatedX + centerX, rotatedY + centerY);
        }

        public double ScreenToViewX(double i)
        {
            double viewX = (i - x_offset) / zoom;
            if (rotation != 0)
            {
                var (x, _) = ApplyInverseRotation(viewX, 0);
                return x;
            }
            return viewX;
        }

        public double ScreenToViewY(double i)
        {
            double viewY = (i - y_offset) / zoom;
            if (rotation != 0)
            {
                var (_, y) = ApplyInverseRotation(0, viewY);
                return y;
            }
            return viewY;
        }

        public string TranslateX(double x)
        {
            if (rotation != 0)
            {
                var (rotatedX, _) = ApplyRotation(x, 0);
                return (x_offset + (rotatedX * zoom)).ToStringInvariant();
            }
            return (x_offset + (x * zoom)).ToStringInvariant();
        }

        public string TranslateY(double y)
        {
            if (rotation != 0)
            {
                var (_, rotatedY) = ApplyRotation(0, y);
                return (y_offset + (rotatedY * zoom)).ToStringInvariant();
            }
            return (y_offset + (y * zoom)).ToStringInvariant();
        }

        /// <summary>
        /// Génère la chaîne de transformation SVG complète (zoom + rotation)
        /// </summary>
        public string GetTransformString()
        {
            if (rotation == 0)
                return string.Empty;

            var (centerX, centerY) = GetRotationCenter();
            double scaledCenterX = x_offset + (centerX * zoom);
            double scaledCenterY = y_offset + (centerY * zoom);

            return $"rotate({rotation.ToStringInvariant()} {scaledCenterX.ToStringInvariant()} {scaledCenterY.ToStringInvariant()})";
        }

        public double Size(double i) => i * zoom;
        
        public string Size_s(double i) => (i * zoom).ToStringInvariant();
        
        public void EnsureIsVisible(Rect area)
        {
            if (rotation != 0)
            {
                // Appliquer la rotation à la zone avant de calculer la visibilité
                var (rotatedLeft, rotatedTop) = ApplyRotation(area.left.Value, area.top.Value);
                x_offset = Clip(Size(-rotatedLeft + 10), viewSize.width.Value - Size(areaSize.width.Value));
                y_offset = Clip(Size(-rotatedTop + 10), viewSize.height.Value - Size(areaSize.height.Value));
            }
            else
            {
                x_offset = Clip(Size(-area.left.Value + 10), viewSize.width.Value - Size(areaSize.width.Value));
                y_offset = Clip(Size(-area.top.Value + 10), viewSize.height.Value - Size(areaSize.height.Value));
            }
        }
    }

    public class SVGObject
    {
        static public readonly string _white = "#FFFFFF";
        static public readonly string _black = "#000000";
        static public readonly string _none = "none";
        static public readonly string _fillopacity = "1";

        public string? id;
        public Rect rect = new Rect();
        public bool scrollIntoView = false;
        public bool visible = true;
        public virtual string GetSVG(SimpleSVG instance, SVGContext context) { return ""; }
        public virtual void SetImageSize(string data) { }

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
        public override string GetSVG(SimpleSVG instance, SVGContext context)
        {
            // To avoid negative size rect
            var tmp = $"<rect {GetId()} x='{context.TranslateX(rect.left.Value)}' y='{context.TranslateY(rect.top.Value)}' width='{context.Size_s(Math.Abs(rect.width.Value))}' height='{context.Size_s(Math.Abs(rect.height.Value))}' fill='{fill}' fill-opacity='{fill_opacity}' stroke='{color}' stroke-width='1'/>";
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
        public override string GetSVG(SimpleSVG instance, SVGContext context)
        {
            if (!string.IsNullOrEmpty(href))
            {
                SimpleSVG.objectMapper.AddOrUpdate(new ObjectRef<SVGObject>() { Id = id, Instance = this });
                var tmp = $"<image {GetId()} href='{href}' ";
                if (rect.left.HasValue)
                    tmp += $"x='{context.TranslateX(rect.left.Value)}' ";
                if (rect.top.HasValue)
                    tmp += $"y='{context.TranslateY(rect.top.Value)}' ";
                if (rect.width.HasValue)
                    tmp += $"width='{context.Size_s(Math.Abs(rect.width.Value))}' ";
                if (rect.height.HasValue)
                    tmp += $"height='{context.Size_s(Math.Abs(rect.height.Value))}'";
                tmp += $"onload='BlazorSimpleSVG.imageLoaded(\"{id}\")' />";
                return tmp;
            }
            else
                return "";
        }

        public void ClearSize()
        {
            rect = new Rect();
        }

        public override void SetImageSize(string data)
        {
            if (rect == null || rect.IsEmpty())
            {
                rect = new Rect();
                lock (rect)
                {
                    var jimage = System.Text.Json.JsonDocument.Parse(data);
                    rect.Set(0, 0, jimage.RootElement.GetProperty("width").GetDouble(), jimage.RootElement.GetProperty("height").GetDouble());
                }
            }
        }
    }
}
