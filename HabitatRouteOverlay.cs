using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    internal sealed class HabitatRouteOverlay {
        static readonly Pen Outline = MakePen(Colors.Black, 5);
        static readonly Pen Walking = MakePen(Color.FromRgb(82, 255, 215), 2.8);
        static readonly Pen Hopping = MakePen(Color.FromRgb(255, 210, 105), 3, DashStyles.Dash);
        static readonly Pen Blocked = MakePen(Color.FromRgb(255, 112, 124), 2.8);
        static readonly Pen Start = MakePen(Colors.White, 2.6);
        readonly HabitatDepth field;
        List<HabitatStep> steps;
        V origin, target;
        bool hasProbe, reachable;

        public HabitatRouteOverlay(HabitatDepth field) { this.field = field; Clear(); }

        public string Status { get; private set; }
        internal bool Reachable { get { return reachable; } }

        public void Clear() {
            hasProbe = reachable = false;
            steps = null;
            Status = "Click inside the habitat image to test a route.";
        }

        public void Inspect(V from, V to, bool atSpider) {
            origin = from;
            target = to;
            hasProbe = true;
            reachable = false;
            steps = null;
            if (!field.HasFooting(target)) {
                Status = "The selected point has no stable footing.";
                return;
            }
            string start = atSpider ? "the spider" : "the last surface foothold";
            if (!HabitatRoute.TryPlan(field, origin, target, out steps)) {
                Status = "No route reaches the selected point from " + start + ".";
                return;
            }
            reachable = true;
            if ((target - origin).Length < .003) {
                Status = "This is the starting point.";
                return;
            }
            int hops = 0;
            foreach (var step in steps) if (step.Hop) hops++;
            Status = hops == 0 ? "Reachable on foot from " + start + "." :
                "Reachable from " + start + " with " + hops + (hops == 1 ? " hop." : " hops.");
        }

        public void Draw(DrawingContext drawing, Rect viewport) {
            if (!hasProbe) return;
            Point from = HabitatArt.Project(origin, viewport);
            if (steps != null) {
                foreach (var step in steps) {
                    Point to = HabitatArt.Project(step.Destination, viewport);
                    if (step.Hop) DrawHop(drawing, from, to, viewport.Height);
                    else {
                        drawing.DrawLine(Outline, from, to);
                        drawing.DrawLine(Walking, from, to);
                    }
                    from = to;
                }
            }
            DrawRing(drawing, HabitatArt.Project(origin, viewport), 6, Start);
            Point end = HabitatArt.Project(target, viewport);
            if (!reachable) {
                DrawCross(drawing, end, Outline);
                DrawCross(drawing, end, Blocked);
            } else if ((target - origin).Length >= .003) DrawRing(drawing, end, 5, Walking);
        }

        static void DrawHop(DrawingContext drawing, Point from, Point to, double height) {
            var arc = new StreamGeometry();
            using (var path = arc.Open()) {
                path.BeginFigure(from, false, false);
                double controlLift = 2 * height * HabitatDepth.HopHeight;
                path.QuadraticBezierTo(new Point((from.X + to.X) * .5, (from.Y + to.Y) * .5 - controlLift), to, true, false);
            }
            drawing.DrawGeometry(null, Outline, arc);
            drawing.DrawGeometry(null, Hopping, arc);
        }

        static void DrawRing(DrawingContext drawing, Point point, double radius, Pen pen) {
            drawing.DrawEllipse(null, Outline, point, radius, radius);
            drawing.DrawEllipse(null, pen, point, radius, radius);
        }

        static void DrawCross(DrawingContext drawing, Point point, Pen pen) {
            var down = new Vector(5, 5);
            var up = new Vector(5, -5);
            drawing.DrawLine(pen, point - down, point + down);
            drawing.DrawLine(pen, point - up, point + up);
        }

        static Pen MakePen(Color color, double thickness, DashStyle dash = null) {
            var pen = new Pen(new SolidColorBrush(color), thickness) {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                DashCap = PenLineCap.Round
            };
            if (dash != null) pen.DashStyle = dash;
            pen.Freeze();
            return pen;
        }
    }
}
