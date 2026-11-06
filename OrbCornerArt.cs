using System;
using System.Windows;
using System.Windows.Media;

namespace CornerSpider {
    public partial class PetCanvas {
        // A hub at the pet's resting position, with two real mounting surfaces
        // in local space. The long open span is pulled in by radial tension.
        static void DrawCornerOrb(DrawingContext drawing, SkinProfile skin, double ex, double ey,
            double growth, int layers, bool resident, double threadWidth) {
            bool golden = skin.Pattern == "Golden";
            bool garden = skin.Pattern == "Garden";
            bool marble = skin.Pattern == "Marble";
            bool cross = skin.Pattern == "Cross";
            bool crystal = skin.WebKind == WebKind.Crystal;
            bool swap = resident && ex < ey;
            if (swap) {
                double oldX = ex;
                ex = ey;
                ey = oldX;
                drawing.PushTransform(new MatrixTransform(0, 1, 1, 0, 0, 0));
            }

            double upper = Math.Min(World.OrbCornerInset, ey);
            double side = resident ? 105 : Math.Min(World.OrbCornerInset, ex);
            if (skin.Male) {
                DrawOrbTravelSilk(drawing, ex, ey, side, upper, resident, golden, layers);
                if (swap) drawing.Pop();
                return;
            }
            double farX = resident ? 108 : side * (golden ? 1.43 : marble ? 1.53 : cross ? 1.88 : 1.63);
            double farY = resident ? 94 : upper * (golden ? 1.82 : marble ? 1.74 : cross ? 1.96 : 1.71);
            double topGap = upper * .045;
            double sideGap = side * .05;
            var rim = resident ? new[] {
                new V(-side * .95, -ey + topGap), new V(-side * .47, -ey + topGap * 1.32),
                new V(side * .18, -ey + topGap * 1.62), new V(side * .79, -ey + topGap),
                new V(farX, -ey + topGap * .55), new V(side * .86, farY * .62),
                new V(-side * .21, farY), new V(-side, farY * .37), new V(-side * 1.13, -upper * .37)
            } : new[] {
                new V(-ex * .80, -ey + topGap * 1.30), new V(-ex * .43, -ey + topGap * 1.65),
                new V(side * .14, -ey + topGap * 1.11), new V(side * .75, -ey + topGap * 1.50),
                new V(farX, -ey + topGap * .66), new V(-ex + sideGap * .70, farY),
                new V(-ex + sideGap * 1.49, ey * .59), new V(-ex + sideGap * 1.12, -ey * .02),
                new V(-ex + sideGap * 1.35, -ey * .59), new V(-ex + sideGap * .90, -ey * .85)
            };

            string frameColor = golden ? "#c6edd9a2" : crystal ? "#b7c8f9eb" : "#b4f2f5ec";
            string radialColor = golden ? "#99e7d1a0" : skin.Pattern == "Nebula" ? "#9cc9c5ef" : "#a1e7eee8";
            string catchColor = golden ? "#9aedd8a0" : crystal ? "#a0a4dfd7" : skin.Pattern == "Nebula" ? "#a1c3b9ec" : "#a4e8eee7";
            var frame = new StreamGeometry();
            using (var path = frame.Open()) {
                path.BeginFigure(OrbPoint(rim[0]), false, false);
                for (int i = 0; i < rim.Length; i++) {
                    V a = rim[i], b = rim[(i + 1) % rim.Length];
                    if (!resident && i == 4) {
                        path.BeginFigure(OrbPoint(b), false, false);
                        continue;
                    }
                    V c, d;
                    OrbFrameControls(a, b, i, resident, crystal, out c, out d);
                    path.BezierTo(OrbPoint(c), OrbPoint(d), OrbPoint(b), true, false);
                }
            }
            OrbStroke(drawing, frame, frameColor, .59, "#54394748", threadWidth);
            if (!resident) {
                var bridge = new StreamGeometry();
                using (var path = bridge.Open()) {
                    V a = rim[4], b = rim[5], c, d;
                    OrbFrameControls(a, b, 4, resident, crystal, out c, out d);
                    path.BeginFigure(OrbPoint(a), false, false);
                    path.BezierTo(OrbPoint(c), OrbPoint(d), OrbPoint(b), true, false);
                }
                drawing.PushOpacity(.77);
                OrbStroke(drawing, bridge, frameColor, .51, "#48394748", threadWidth);
                drawing.Pop();
            }

            var mounts = new StreamGeometry();
            using (var path = mounts.Open()) {
                int count = resident ? 5 : rim.Length;
                for (int i = 0; i < count; i++) {
                    V joint = rim[i];
                    bool top = i <= 4;
                    double spread = (top ? upper : side) * (.019 + .010 * Math.Cos(i * 2.37));
                    V anchor = top ? new V(joint.X, -ey) : new V(-ex, joint.Y);
                    int first = i % 3 == 0 ? 0 : -1;
                    int last = i == 4 || i == 5 || i % 3 == 0 ? 1 : 0;
                    for (int j = first; j <= last; j++) {
                        V root = anchor + (top ? new V(j * spread, 0) : new V(0, j * spread));
                        OrbLine(path, root, joint);
                    }
                    // Silk swells into a tiny flattened pad at the substrate.
                    V pad = top ? new V(spread * 1.28, 0) : new V(0, spread * 1.28);
                    OrbLine(path, anchor - pad, anchor + pad);
                }
            }
            OrbStroke(drawing, mounts, frameColor, .49, "#49394748", threadWidth);

            int spokes = OrbRadials.Count(skin);
            int turns = cross ? (resident ? 8 : 11) : golden ? 22 : crystal ? 12 : marble ? 14 : garden ? 17 : 16;
            double phase = OrbRadials.Phase(skin);
            V openSpan = rim[5] - rim[4];
            V towardOpening = resident ? new V(0, 1) : new V(openSpan.Y, -openSpan.X) * (1 / openSpan.Length);
            V towardCorner = towardOpening * -1;
            double fore = OrbRimHit(rim, towardOpening, resident, crystal).Length * .94;
            double aft = OrbRimHit(rim, towardCorner, resident, crystal).Length * (resident ? .85 : .71);
            // Keep the Cross orb's oval within the available depth. Fitting a
            // wide ellipse through the short sector compressed its sticky silk
            // into a dense wedge, especially in the small settings preview.
            double halfDepth = cross ? Math.Min(fore, aft) * .88 : (fore + aft) * .5;
            double centerDepth = cross ? halfDepth * .09 : (fore - aft) * .5;
            double halfWidth = cross ? halfDepth * 1.14 : side * (golden ? .82 : marble ? .88 : .96);
            var rays = new V[spokes];
            var tips = new V[spokes];
            var captureEdge = new double[spokes];
            var radii = new StreamGeometry();
            using (var path = radii.Open()) {
                for (int i = 0; i < spokes; i++) {
                    V ray = OrbRadials.Ray(i, spokes, phase);
                    rays[i] = ray;
                    tips[i] = OrbRimHit(rim, ray, resident, crystal);
                    double along = (ray.X * towardOpening.X + ray.Y * towardOpening.Y) / halfDepth;
                    double across = OrbCross(ray, towardOpening) / halfWidth;
                    double offset = centerDepth / halfDepth;
                    double quadratic = along * along + across * across;
                    double ellipse = (along * offset + Math.Sqrt(along * along * offset * offset +
                        quadratic * (1 - offset * offset))) / quadratic;
                    double rimLimit = tips[i].Length * (.912 + .018 * Math.Sin(i * 2.04 + phase));
                    // A soft fit through the frame removes abrupt transitions
                    // where the oval approaches an outside span.
                    double clearance = rimLimit - ellipse;
                    double blend = Math.Min(30, Math.Min(rimLimit, ellipse) * .21);
                    captureEdge[i] = (rimLimit + ellipse - Math.Sqrt(clearance * clearance + blend * blend)) * .5;
                    OrbLine(path, cross ? ray * 3.5 : new V(0, 0), tips[i]);
                }
            }
            OrbStroke(drawing, radii, radialColor, cross ? .34 : .45, cross ? "#303c4a4c" : "#403c4a4c", threadWidth);

            var spiral = new StreamGeometry();
            using (var path = spiral.Open()) {
                int count = Math.Min(turns, Math.Max(4, (int)Math.Ceiling(growth * turns))) * spokes;
                V a = OrbCapturePoint(rays, captureEdge, 0, turns, phase, cross);
                path.BeginFigure(OrbPoint(a), false, false);
                for (int i = 1; i <= count; i++) {
                    V b = OrbCapturePoint(rays, captureEdge, i, turns, phase, cross);
                    int turn = i / spokes, sector = i % spokes;
                    // A small damaged patch ends on intact spokes. It avoids
                    // pristine repeated rows without loose floating fragments.
                    if (turn == 1 && sector == 7 || turn == 2 && (sector == 7 || sector == 8)) {
                        path.BeginFigure(OrbPoint(b), false, false);
                        a = b;
                        continue;
                    }
                    V step = b - a;
                    V bend = (a + b) * -.5;
                    bend = bend * (step.Length / Math.Max(1, bend.Length) *
                        (crystal ? .014 : .053 + .018 * Math.Sin(i * 1.78 + phase)));
                    path.BezierTo(OrbPoint(a + step * .33 + bend), OrbPoint(a + step * .67 + bend),
                        OrbPoint(b), true, false);
                    a = b;
                }
            }
            OrbCaptureStroke(drawing, spiral, catchColor, cross, threadWidth);

            // Nonsticky hub turns are separated from the capture spiral.
            var hub = new StreamGeometry();
            using (var path = hub.Open()) {
                double innerHub = cross ? 3.5 : 2.8;
                int hubSegments = (cross ? 2 : 3) * spokes;
                path.BeginFigure(OrbPoint(rays[0] * innerHub), false, false);
                for (int i = 1; i <= hubSegments; i++) {
                    int ray = i % spokes;
                    double radius = Math.Min(cross ? 7.4 : 8.5, captureEdge[ray] * .36);
                    double r = innerHub + (radius - innerHub) * i / hubSegments;
                    path.LineTo(OrbPoint(rays[ray] * r), true, false);
                }
            }
            OrbStroke(drawing, hub, frameColor, cross ? .30 : .40, cross ? "#30404c4d" : "#42404c4d", threadWidth);

            if (garden) {
                for (int sideOfHub = -1; sideOfHub <= 1; sideOfHub += 2) {
                    int steps = sideOfHub < 0 ? 5 : 10;
                    V a = new V(-2.8, sideOfHub * 13);
                    for (int i = 1; i <= steps; i++) {
                        V b = new V((i % 2 == 0 ? -1 : 1) * (4.4 - i * .19),
                            sideOfHub * (13 + i * 3.15));
                        Line(drawing, "#64c9d2c6", 1.25*threadWidth, a.X, a.Y, b.X, b.Y);
                        Line(drawing, "#dcf4f3e7", .65*threadWidth, a.X, a.Y, b.X, b.Y);
                        a = b;
                    }
                }
            }
            if (marble) {
                // A short off-orb signal line leads toward a crevice at the mounting surface.
                V crevice = new V(rim[1].X, -ey + topGap * .20);
                Line(drawing, frameColor, .68*threadWidth, 0, 0, crevice.X, crevice.Y);
                for (int i = 0; i < 7; i++) {
                    double s = (i - 3) * 1.5;
                    Curve(drawing, "#91e1e6db", .34*threadWidth, new Point(crevice.X + s, -ey),
                        new Point(crevice.X + s - 1, -ey + 6), new Point(crevice.X - s + 2, -ey + 9),
                        new Point(crevice.X - s, -ey + topGap * 1.1));
                }
            }
            if (layers > 0) {
                // Fine reinforcement stays near existing junctions. The base web
                // is identical when additional repair layers become visible.
                var repairs = new StreamGeometry();
                using (var path = repairs.Open()) {
                    for (int i = 0; i < layers * 3; i++) {
                        int k = (i * 7 + 3) % spokes;
                        int next = (k + 1) % spokes;
                        double t = .83 + .11 * Math.Sin(i * 2.23 + phase);
                        OrbLine(path, tips[k] * t, tips[next] * (t - .055));
                        OrbLine(path, tips[k] * (t + .04), tips[k] * Math.Min(1, t + .22));
                    }
                }
                OrbStroke(drawing, repairs, "#78e1e9df", .32, "#2d3c4a4c", threadWidth);
            }
            if (swap) drawing.Pop();
        }

        static void DrawOrbTravelSilk(DrawingContext drawing, double ex, double ey, double side, double upper,
            bool resident, bool golden, int layers) {
            V left = new V(-side * .48, -upper * .19);
            V high = new V(side * .26, -upper * .48);
            V low = new V(-side * .16, upper * .36);
            V center = new V(0, 0);
            var trunks = new StreamGeometry();
            using (var path = trunks.Open()) {
                V[] roots = {
                    new V(-side * .79, -ey), new V(-side * .23, -ey),
                    new V(side * .38, -ey), new V(side * .94, -ey),
                    resident ? new V(-side * .99, -ey) : new V(-ex, -upper * .73),
                    resident ? new V(-side * .61, -ey) : new V(-ex, -upper * .28),
                    resident ? new V(side * .11, -ey) : new V(-ex, upper * .38),
                    resident ? new V(side * .68, -ey) : new V(-ex, upper * .87)
                };
                V[] joins = { left, high, high, low, left, left, low, low };
                for (int i = 0; i < roots.Length; i++) {
                    V root = roots[i], joint = joins[i], step = joint - root;
                    V slack = new V(step.Y, -step.X) * (.017 + .011 * Math.Sin(i * 1.9));
                    path.BeginFigure(OrbPoint(root), false, false);
                    path.BezierTo(OrbPoint(root + step * .31 + slack), OrbPoint(root + step * .68 + slack),
                        OrbPoint(joint), true, false);
                    double size = 3.3 + i % 3;
                    V pad = resident || i < 4 ? new V(size, 0) : new V(0, size);
                    OrbLine(path, root - pad, root + pad);
                }
                OrbLine(path, left, high);
                OrbLine(path, high, center);
                OrbLine(path, left, center);
                OrbLine(path, center, low);
                OrbLine(path, left, low);
                OrbLine(path, high, low);
                for (int i = 0; i < layers * 2; i++) {
                    int k = (i * 3) % roots.Length, neighbor = (k + 1) % roots.Length;
                    double t = .11 + .34 * (.5 + .5 * Math.Sin(i * 1.86));
                    V from = OrbTravelPoint(roots[k], joins[k], k, t);
                    V to = OrbTravelPoint(roots[neighbor], joins[neighbor], neighbor, t + .07);
                    OrbLine(path, from, to);
                }
            }
            OrbStroke(drawing, trunks, golden ? "#a9ebd6a4" : "#a9e9efe6", .50, "#523c4a4c");
        }

        static V OrbTravelPoint(V a, V b, int index, double t) {
            V step = b - a;
            V slack = new V(step.Y, -step.X) * (.017 + .011 * Math.Sin(index * 1.9));
            return OrbFramePoint(a, a + step * .31 + slack, a + step * .68 + slack, b, t);
        }

        static void OrbFrameControls(V a, V b, int index, bool resident, bool crystal, out V c, out V d) {
            V midpoint = (a + b) * .5;
            bool openSpan = resident ? index >= 4 && index <= 7 : index == 4;
            V step = b - a;
            double length = Math.Max(1, step.Length);
            double chordDepth = Math.Abs(OrbCross(a, b)) / length;
            double inward = openSpan ? Math.Min(crystal ? 19 : 27, chordDepth * (resident ? .48 : .30)) :
                (index % 3 == 1 ? 5.1 : 2.5);
            V bow = openSpan ? new V(-step.Y, step.X) * (inward / length) :
                midpoint * (-Math.Min(inward, midpoint.Length * .36) / Math.Max(1, midpoint.Length));
            c = a + step * (1.0 / 3) + bow * (openSpan ? .57 : 1);
            d = a + step * (2.0 / 3) + bow * (openSpan ? 1.43 : 1);
        }

        static V OrbFramePoint(V a, V c, V d, V b, double t) {
            double u = 1 - t;
            return a * (u * u * u) + c * (3 * t * u * u) + d * (3 * t * t * u) + b * (t * t * t);
        }

        static V OrbRimHit(V[] rim, V direction, bool resident, bool crystal) {
            for (int i = 0; i < rim.Length; i++) {
                V a = rim[i], b = rim[(i + 1) % rim.Length];
                double from = OrbCross(direction, a), to = OrbCross(direction, b);
                if (from > 0 || to < 0) continue;
                V c, d;
                OrbFrameControls(a, b, i, resident, crystal, out c, out d);
                double low = 0, high = 1;
                for (int step = 0; step < 21; step++) {
                    double at = (low + high) * .5;
                    V point = OrbFramePoint(a, c, d, b, at);
                    if (OrbCross(direction, point) < 0) low = at; else high = at;
                }
                double t = (low + high) * .5;
                V hit = OrbFramePoint(a, c, d, b, t);
                if (hit.X * direction.X + hit.Y * direction.Y > 0) return hit;
            }
            return direction * 32;
        }

        static V OrbCapturePoint(V[] rays, double[] edge, int index, int turns, double phase, bool cross) {
            int k = index % rays.Length;
            double progress = index / (double)(turns * rays.Length);
            double radial = Math.Pow(Math.Max(0, 1 - progress), cross ? 1.025 : 1.11);
            double freeZone = Math.Min(cross ? 20.5 : 17.5, edge[k] * .58);
            double sway = Math.Sin(index * .89 + phase) * .75 * Math.Sin(progress * Math.PI);
            return rays[k] * (freeZone + (edge[k] - freeZone) * radial + sway);
        }

        static double OrbCross(V a, V b) { return a.X * b.Y - a.Y * b.X; }
        static Point OrbPoint(V point) { return new Point(point.X, point.Y); }
        static void OrbLine(StreamGeometryContext path, V a, V b) {
            path.BeginFigure(OrbPoint(a), false, false);
            path.LineTo(OrbPoint(b), true, false);
        }

        static void OrbStroke(DrawingContext drawing, StreamGeometry geometry, string tint, double width, string shade,
            double threadWidth = 1) {
            geometry.Freeze();
            drawing.DrawGeometry(null, P(shade, (width + .53)*threadWidth), geometry);
            drawing.DrawGeometry(null, P(tint, width*threadWidth), geometry);
        }

        static void OrbCaptureStroke(DrawingContext drawing, StreamGeometry geometry, string tint, bool cross, double threadWidth) {
            geometry.Freeze();
            drawing.DrawGeometry(null, P(cross ? "#323c4a4c" : "#423c4a4c", (cross ? .57 : .74)*threadWidth), geometry);
            var silk = (Color)ColorConverter.ConvertFromString(tint);
            var sheen = new LinearGradientBrush {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new Point(-138, -92), EndPoint = new Point(148, 114)
            };
            double[] brightness = { .78, 1.02, .87, 1.18, .93, 1.10, .81 };
            for (int i = 0; i < brightness.Length; i++) {
                byte alpha = (byte)Math.Min(255, silk.A * brightness[i]);
                sheen.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, silk.R, silk.G, silk.B),
                    i / (double)(brightness.Length - 1)));
            }
            sheen.Freeze();
            var pen = new Pen(sheen, (cross ? .30 : .35)*threadWidth) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            pen.Freeze();
            drawing.DrawGeometry(null, pen, geometry);
        }
    }
}
