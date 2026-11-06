using System;
using System.Collections.Generic;

namespace CornerSpider {
    public enum HabitatSilkRole { Support, Frame, Capture, Retreat, Gumfoot }

    public struct HabitatSilkPoint {
        public readonly V Position;
        public readonly double Depth;

        public HabitatSilkPoint(V position, double depth) {
            Position = position;
            Depth = depth;
        }

        public static HabitatSilkPoint Lerp(HabitatSilkPoint a, HabitatSilkPoint b, double amount) {
            return new HabitatSilkPoint(a.Position + (b.Position - a.Position) * amount,
                a.Depth + (b.Depth - a.Depth) * amount);
        }
    }

    public struct HabitatSilkSegment {
        public readonly HabitatSilkPoint Start, End;
        public readonly HabitatSilkRole Role;
        public readonly double Weight;

        internal HabitatSilkSegment(HabitatSilkPoint start, HabitatSilkPoint end, HabitatSilkRole role, double weight) {
            Start = start;
            End = end;
            Role = role;
            Weight = weight;
        }
    }

    public sealed class HabitatSilkPlan {
        public readonly WebKind Kind;
        public readonly HabitatSilkPoint Landing, Perch;
        public readonly IList<HabitatSilkPoint> Anchors;
        public readonly IList<HabitatSilkSegment> Segments;
        public readonly double Duration;
        internal readonly double Aspect;

        internal HabitatSilkPlan(WebKind kind, HabitatSilkPoint landing, HabitatSilkPoint perch,
            List<HabitatSilkPoint> anchors, List<HabitatSilkSegment> segments, double duration, double aspect) {
            Kind = kind;
            Landing = landing;
            Perch = perch;
            Anchors = anchors.AsReadOnly();
            Segments = segments.AsReadOnly();
            Duration = duration;
            Aspect = aspect;
        }
    }

    public sealed class HabitatSilkConstruction {
        public readonly HabitatSilkPlan Plan;
        readonly double[] strandEnds;
        double elapsed;

        public HabitatSilkConstruction(HabitatSilkPlan plan) {
            if (plan == null) throw new ArgumentNullException("plan");
            Plan = plan;
            strandEnds = new double[plan.Segments.Count];
            double total = 0;
            for (int i = 0; i < strandEnds.Length; i++) {
                HabitatSilkSegment strand = plan.Segments[i];
                V delta = strand.End.Position - strand.Start.Position;
                double depth = (strand.End.Depth - strand.Start.Depth) * .32;
                total += .004 + Math.Sqrt(delta.X * delta.X * plan.Aspect * plan.Aspect + delta.Y * delta.Y + depth * depth);
                strandEnds[i] = total;
            }
            for (int i = 0; i < strandEnds.Length; i++) strandEnds[i] = strandEnds[i] / total;
        }

        public double Progress { get { return elapsed / Plan.Duration; } }
        public bool Complete { get { return elapsed >= Plan.Duration; } }

        public int VisibleCount {
            get {
                if (elapsed == 0) return 0;
                int index = Array.BinarySearch(strandEnds, Progress);
                return index >= 0 ? index + 1 : Math.Min(strandEnds.Length, ~index + 1);
            }
        }

        public void Advance(double seconds) {
            if (seconds > 0 && !Double.IsInfinity(seconds)) elapsed = Math.Min(Plan.Duration, elapsed + seconds);
        }

        public double SegmentProgress(int index) {
            double start = index == 0 ? 0 : strandEnds[index - 1];
            return HabitatSilk.Clamp01((Progress - start) / (strandEnds[index] - start));
        }
    }

    public static class HabitatSilk {
        const double FullTurn = Math.PI * 2;

        sealed class Anchor {
            public HabitatSilkPoint Site;
            public V Opening;
            public double Relief;
        }

        sealed class Site {
            public readonly List<Anchor> Anchors = new List<Anchor>();
            public HabitatSilkPoint Center;
            public double Score;
        }

        public static HabitatSilkPlan Plan(HabitatDepth field, string species, V origin) {
            if (field == null) throw new ArgumentNullException("field");
            SkinProfile skin = SkinCatalog.Get(species);
            if ((skin.Male && skin.Category == "Orb weavers") || !field.HasFooting(origin)) return null;
            double aspect = Math.Max(.55, Math.Min(2.6, (double)field.Width / field.Height));
            double depthRange;
            List<Anchor> candidates = FindAnchors(field, aspect, out depthRange);
            if (candidates.Count < 4 || depthRange < .12) return null;

            bool compact = skin.WebKind == WebKind.Retreat;
            int sectors = compact ? 5 : 7;
            double radius = compact ? .125 : .21;
            var sites = new List<Site>();
            for (int y = 1; y <= 8; y++) {
                for (int x = 1; x <= 12; x++) {
                    V center = new V(x / 13.0, y / 9.0);
                    for (int size = 0; size < 3; size++) {
                        for (int turn = 0; turn < 2; turn++) {
                            Site site = Evaluate(field, candidates, center, origin, radius * (.77 + size * .24),
                                aspect, sectors, turn * .5, depthRange);
                            if (site != null) sites.Add(site);
                        }
                    }
                }
            }
            sites.Sort(delegate(Site a, Site b) { return b.Score.CompareTo(a.Score); });
            Site best = null;
            HabitatSilkPoint landing = new HabitatSilkPoint();
            var reachable = new Dictionary<Anchor, bool>();
            foreach (Site site in sites) {
                if (FindLanding(field, site, origin, aspect, reachable, out landing)) { best = site; break; }
            }
            if (best == null) return null;

            var anchors = new List<HabitatSilkPoint>();
            int first = best.Anchors.FindIndex(delegate(Anchor anchor) { return anchor.Site.Position.X == landing.Position.X
                && anchor.Site.Position.Y == landing.Position.Y; });
            for (int i = 0; i < best.Anchors.Count; i++) anchors.Add(best.Anchors[(first + i) % best.Anchors.Count].Site);

            HabitatSilkPoint perch = best.Center;
            var lines = new List<HabitatSilkSegment>();
            double duration;
            switch (skin.WebKind) {
                case WebKind.Orb:
                case WebKind.Crystal:
                    BuildOrb(lines, anchors, perch, aspect, skin);
                    duration = 42;
                    break;
                case WebKind.Tangle:
                    perch = BuildTangle(lines, anchors, perch, skin, field);
                    duration = 32;
                    break;
                case WebKind.Sheet:
                    perch = BuildFunnel(lines, anchors, perch, landing, aspect, field);
                    duration = 36;
                    break;
                default:
                    BuildRetreat(lines, anchors, perch, aspect, skin);
                    duration = 22;
                    break;
            }
            return new HabitatSilkPlan(skin.WebKind, landing, perch, anchors, lines, duration, aspect);
        }

        static bool FindLanding(HabitatDepth field, Site site, V origin, double aspect, Dictionary<Anchor, bool> reachable,
            out HabitatSilkPoint landing) {
            landing = new HabitatSilkPoint();
            double best = Double.MaxValue;
            foreach (Anchor anchor in site.Anchors) {
                double distance = Metric(anchor.Site.Position - origin, aspect).Length;
                if (distance >= best) continue;
                bool canReach;
                if (!reachable.TryGetValue(anchor, out canReach)) {
                    List<HabitatStep> route;
                    canReach = HabitatRoute.TryPlan(field, origin, anchor.Site.Position, out route);
                    reachable.Add(anchor, canReach);
                }
                if (canReach) { best = distance; landing = anchor.Site; }
            }
            return best < Double.MaxValue;
        }

        static List<Anchor> FindAnchors(HabitatDepth field, double aspect, out double depthRange) {
            const int rows = 38;
            int columns = Math.Max(25, (int)(rows * aspect));
            double minimum = 1, maximum = 0;
            var samples = new List<Anchor>();
            for (int row = 1; row < rows; row++) {
                for (int column = 1; column < columns; column++) {
                    V position = new V(column / (double)columns, row / (double)rows);
                    double depth = field.DepthAt(position);
                    minimum = Math.Min(minimum, depth);
                    maximum = Math.Max(maximum, depth);
                    V opening = new V();
                    double best = .066;
                    for (int i = 0; i < 8; i++) {
                        V direction = Polar(i * FullTurn / 8, aspect);
                        V behind = position - direction * .017;
                        V near = position + direction * .025;
                        V far = position + direction * .046;
                        if (!Inside(behind) || !Inside(far)) continue;
                        // Keep the root on a traversable surface next to a sustained depth drop.
                        if (Math.Abs(field.DepthAt(behind) - depth) > .065) continue;
                        double relief = depth - Math.Max(field.DepthAt(near), field.DepthAt(far));
                        if (relief > best) { opening = direction; best = relief; }
                    }
                    if (opening.Length > 0 && field.HasFooting(position))
                        samples.Add(new Anchor { Site = new HabitatSilkPoint(position, depth), Opening = opening, Relief = best });
                }
            }
            depthRange = maximum - minimum;
            samples.Sort(delegate(Anchor a, Anchor b) {
                int relief = b.Relief.CompareTo(a.Relief);
                if (relief != 0) return relief;
                int row = a.Site.Position.Y.CompareTo(b.Site.Position.Y);
                return row != 0 ? row : a.Site.Position.X.CompareTo(b.Site.Position.X);
            });

            var distinct = new List<Anchor>();
            foreach (Anchor candidate in samples) {
                bool close = false;
                foreach (Anchor other in distinct) {
                    V delta = Metric(other.Site.Position - candidate.Site.Position, aspect);
                    if (Dot(delta, delta) < .037 * .037) { close = true; break; }
                }
                if (!close) distinct.Add(candidate);
                if (distinct.Count >= 150) break;
            }
            return distinct;
        }

        static Site Evaluate(HabitatDepth field, List<Anchor> candidates, V center, V origin,
            double radius, double aspect, int sectors, double turn, double depthRange) {
            var picks = new Anchor[sectors];
            var quality = new double[sectors];
            double centerDepth = field.DepthAt(center);
            foreach (Anchor anchor in candidates) {
                V delta = Metric(anchor.Site.Position - center, aspect);
                double distance = delta.Length;
                if (distance < radius * .42 || distance > radius * 1.9 || anchor.Site.Depth < centerDepth - .06) continue;
                double facing = -Dot(delta, Metric(anchor.Opening, aspect)) / distance;
                if (facing < -.27) continue;
                double angle = Math.Atan2(delta.Y, delta.X);
                int sector = (int)Math.Floor(angle / FullTurn * sectors - turn + .5 + sectors) % sectors;
                double fit = .60 * (1 - Math.Abs(distance / radius - 1)) + .44 * facing
                    + .43 * Math.Min(.55, anchor.Relief);
                if (picks[sector] == null || fit > quality[sector]) {
                    picks[sector] = anchor;
                    quality[sector] = fit;
                }
            }

            double minX = 1, minY = 1, maxX = 0, maxY = 0, near = 0, far = 1, totalDepth = 0, totalFit = 0;
            var site = new Site();
            for (int i = 0; i < sectors; i++) {
                Anchor anchor = picks[i];
                if (anchor == null) continue;
                site.Anchors.Add(anchor);
                V p = anchor.Site.Position;
                minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
                far = Math.Min(far, anchor.Site.Depth); near = Math.Max(near, anchor.Site.Depth);
                totalDepth += anchor.Site.Depth;
                totalFit += quality[i];
            }
            int count = site.Anchors.Count;
            double spanX = (maxX - minX) * aspect, spanY = maxY - minY;
            if (count < 4 || spanX < radius || spanY < radius || near - far < Math.Min(.11, depthRange * .30)) return null;

            double depth = Clamp01(Math.Max(centerDepth + .037, totalDepth / count + .014));
            site.Center = new HabitatSilkPoint(center, depth);
            if (field.VisibilityAt(center, depth) < .65) return null;
            double coverage = 0;
            for (int i = 0; i < count; i++) {
                V a = Metric(site.Anchors[i].Site.Position - center, aspect);
                V b = Metric(site.Anchors[(i + 1) % count].Site.Position - center, aspect);
                double gap = Math.Atan2(Cross(a, b), Dot(a, b));
                if (gap < 0) gap += FullTurn;
                if (gap > 2.84) return null;
                coverage += VisibleSpan(field, site.Anchors[i].Site, site.Center);
            }
            coverage /= count;
            if (coverage < .53) return null;
            double fromOrigin = Metric(origin - center, aspect).Length;
            site.Score = count * .22 + coverage * 2.3 + totalFit / count + (near - far) * .70
                + Math.Min(spanX, spanY) * .9 - fromOrigin * .10;
            return site;
        }

        static double VisibleSpan(HabitatDepth field, HabitatSilkPoint from, HabitatSilkPoint to) {
            double visible = 0;
            for (int i = 1; i <= 6; i++) {
                HabitatSilkPoint point = HabitatSilkPoint.Lerp(from, to, i / 7.0);
                visible += field.VisibilityAt(point.Position, point.Depth);
            }
            return visible / 6;
        }

        static HabitatSilkPoint[] SupportedFrame(List<HabitatSilkSegment> lines, List<HabitatSilkPoint> anchors,
            HabitatSilkPoint center, double size, double sag) {
            var frame = new HabitatSilkPoint[anchors.Count];
            for (int i = 0; i < frame.Length; i++) {
                HabitatSilkPoint node = HabitatSilkPoint.Lerp(center, anchors[i], size * (.94 + .06 * Math.Cos(i * 2.3)));
                frame[i] = Offset(node, 0, sag * Math.Sin(i * 2.4), .009 * Math.Sin(i * 1.8));
                Add(lines, anchors[i], frame[i], HabitatSilkRole.Support, 1);
            }
            for (int i = 0; i < frame.Length; i++) {
                HabitatSilkPoint next = frame[(i + 1) % frame.Length];
                HabitatSilkPoint middle = FrameBend(center, frame[i], next, sag);
                Add(lines, frame[i], middle, HabitatSilkRole.Frame, .78);
                Add(lines, middle, next, HabitatSilkRole.Frame, .78);
            }
            return frame;
        }

        static void BuildOrb(List<HabitatSilkSegment> lines, List<HabitatSilkPoint> anchors,
            HabitatSilkPoint center, double aspect, SkinProfile skin) {
            HabitatSilkPoint[] frame = SupportedFrame(lines, anchors, center, .80, .006);
            int count = OrbRadials.Count(skin);
            var frameTips = new HabitatSilkPoint[count];
            var lengths = new double[count];
            for (int i = 0; i < count; i++) {
                V ray = OrbRadials.Ray(i, count, OrbRadials.Phase(skin));
                frameTips[i] = RayToFrame(center, new V(ray.X / aspect, ray.Y), frame, .006);
                lengths[i] = Metric(frameTips[i].Position - center.Position, aspect).Length;
                Add(lines, center, frameTips[i], HabitatSilkRole.Frame, .64);
            }
            // The capture spiral has its own shape inside the irregular attachment frame.
            // A long support on one side of the clearing does not stretch every spiral turn.
            double[] sorted = (double[])lengths.Clone();
            Array.Sort(sorted);
            double captureRadius = (sorted[count / 4] + sorted[count / 2]) * .5;
            var spokes = new HabitatSilkPoint[count];
            for (int i = 0; i < count; i++) {
                V ray = OrbRadials.Ray(i, count, OrbRadials.Phase(skin));
                double elongation = skin.Pattern == "Golden" ? .84 + .26 * Math.Abs(ray.Y) : .95 + .07 * Math.Abs(ray.Y);
                double reach = Math.Min(.91, captureRadius * elongation / Math.Max(.0001, lengths[i]));
                spokes[i] = HabitatSilkPoint.Lerp(center, frameTips[i], reach);
            }

            int turns = skin.Pattern == "Golden" ? 10 : skin.WebKind == WebKind.Crystal ? 6 : 8;
            HabitatSilkPoint last = HabitatSilkPoint.Lerp(center, spokes[0], .98);
            for (int i = 1; i <= count * turns; i++) {
                int spoke = i % count;
                double fraction = .98 - .76 * i / (count * turns) + .009 * Math.Sin(i * .51);
                HabitatSilkPoint next = HabitatSilkPoint.Lerp(center, spokes[spoke], fraction);
                Add(lines, last, next, HabitatSilkRole.Capture, .65);
                last = next;
            }
            for (int ring = 1; ring <= 2; ring++) {
                double fraction = ring * .060;
                for (int i = 0; i < count; i++) {
                    Add(lines, HabitatSilkPoint.Lerp(center, spokes[i], fraction),
                        HabitatSilkPoint.Lerp(center, spokes[(i + 1) % count], fraction), HabitatSilkRole.Capture, .55);
                }
            }
        }

        static HabitatSilkPoint FrameBend(HabitatSilkPoint center, HabitatSilkPoint a, HabitatSilkPoint b, double sag) {
            return HabitatSilkPoint.Lerp(HabitatSilkPoint.Lerp(a, b, .5), center, sag * 7);
        }

        static HabitatSilkPoint RayToFrame(HabitatSilkPoint center, V ray, HabitatSilkPoint[] frame, double sag) {
            for (int i = 0; i < frame.Length; i++) {
                HabitatSilkPoint a = frame[i], b = frame[(i + 1) % frame.Length];
                HabitatSilkPoint middle = FrameBend(center, a, b, sag), point;
                if (RayToEdge(center, ray, a, middle, out point) || RayToEdge(center, ray, middle, b, out point)) return point;
            }
            return center;
        }

        static bool RayToEdge(HabitatSilkPoint center, V ray, HabitatSilkPoint a, HabitatSilkPoint b, out HabitatSilkPoint point) {
            point = center;
            V edge = b.Position - a.Position, offset = a.Position - center.Position;
            double determinant = Cross(ray, edge);
            if (Math.Abs(determinant) < .000001) return false;
            double along = Cross(offset, ray) / determinant;
            if (along < 0 || along > 1 || Cross(offset, edge) / determinant <= 0) return false;
            point = HabitatSilkPoint.Lerp(a, b, along);
            return true;
        }

        static HabitatSilkPoint BuildTangle(List<HabitatSilkSegment> lines, List<HabitatSilkPoint> anchors,
            HabitatSilkPoint center, SkinProfile skin, HabitatDepth field) {
            HabitatSilkPoint bed = ClearPerch(field, center, Offset(center, 0, .028, .018));
            HabitatSilkPoint[] frame = SupportedFrame(lines, anchors, bed, .79, .013);
            var canopy = new HabitatSilkPoint[frame.Length];
            var mesh = new HabitatSilkPoint[frame.Length];
            for (int i = 0; i < frame.Length; i++) {
                HabitatSilkPoint node = HabitatSilkPoint.Lerp(bed, frame[i], .50 + .12 * Math.Sin(i * 2.1));
                canopy[i] = Offset(node, 0, -.037 - .016 * Math.Cos(i * 2.7), -.035 * Math.Cos(i * 1.6));
                mesh[i] = Offset(node, 0, .011 * Math.Sin(i * 3.2), .025 * Math.Cos(i * 1.2));
                Add(lines, frame[i], canopy[i], HabitatSilkRole.Frame, .75);
            }
            for (int i = 0; i < frame.Length; i++) {
                int j = (i + 1) % frame.Length;
                int k = (i + 2) % frame.Length;
                Add(lines, canopy[i], mesh[j], HabitatSilkRole.Capture, .52);
                Add(lines, canopy[i], canopy[k], HabitatSilkRole.Capture, .48);
                Add(lines, mesh[i], frame[j], HabitatSilkRole.Capture, .55);
                Add(lines, mesh[i], mesh[k], HabitatSilkRole.Capture, .55);
                Add(lines, mesh[i], bed, HabitatSilkRole.Capture, .61);
                if (anchors[i].Position.Y > bed.Position.Y + .035 && skin.Pattern != "House") {
                    HabitatSilkPoint knot = HabitatSilkPoint.Lerp(anchors[i], mesh[i], .20);
                    Add(lines, mesh[i], knot, HabitatSilkRole.Gumfoot, .58);
                    Add(lines, knot, anchors[i], HabitatSilkRole.Gumfoot, .90);
                }
            }
            return bed;
        }

        static HabitatSilkPoint BuildFunnel(List<HabitatSilkSegment> lines, List<HabitatSilkPoint> anchors,
            HabitatSilkPoint center, HabitatSilkPoint landing, double aspect, HabitatDepth field) {
            HabitatSilkPoint[] frame = SupportedFrame(lines, anchors, center, .85, .010);
            HabitatSilkPoint mouth = ClearPerch(field, center, HabitatSilkPoint.Lerp(center, landing, .39));
            const int count = 14;
            var rim = new HabitatSilkPoint[count];
            var panel = new HabitatSilkPoint[count];
            V axis = Unit(Metric(landing.Position - center.Position, aspect));
            for (int i = 0; i < count; i++) {
                V direction = Polar(i * FullTurn / count, aspect);
                panel[i] = RayToFrame(center, direction, frame, .010);
                V ring = Metric(direction, aspect);
                V ellipse = ring * .022 + axis * (Dot(ring, axis) * -.008);
                rim[i] = Offset(mouth, ellipse.X / aspect, ellipse.Y, Math.Sin(i * FullTurn / count) * .025);
                if (i % 2 == 0) Add(lines, panel[i], rim[i], HabitatSilkRole.Frame, .61);
            }
            var previous = panel;
            for (int row = 1; row <= 7; row++) {
                var current = new HabitatSilkPoint[count];
                for (int i = 0; i < count; i++) {
                    double fraction = row == 7 ? 1 : (row + .20 * Math.Sin(i * 2.3 + row)) / 7;
                    current[i] = HabitatSilkPoint.Lerp(panel[i], rim[i], fraction);
                }
                for (int i = 0; i < count; i++) {
                    int j = (i + 1) % count;
                    Add(lines, current[i], current[j], HabitatSilkRole.Capture, row == 7 ? .82 : .49);
                    if ((i + row) % 3 == 0) Add(lines, previous[j], current[i], HabitatSilkRole.Capture, .42);
                }
                previous = current;
            }

            HabitatSilkPoint back = HabitatSilkPoint.Lerp(mouth, landing, .55);
            back = Offset(back, 0, 0, -.058);
            for (int i = 0; i < count; i++) {
                int next = (i + 1) % count;
                HabitatSilkPoint tip = HabitatSilkPoint.Lerp(back, rim[i], .40);
                HabitatSilkPoint nextTip = HabitatSilkPoint.Lerp(back, rim[next], .40);
                Add(lines, rim[i], tip, HabitatSilkRole.Retreat, .67);
                Add(lines, tip, nextTip, HabitatSilkRole.Retreat, .55);
            }
            return mouth;
        }

        static HabitatSilkPoint ClearPerch(HabitatDepth field, HabitatSilkPoint fallback, HabitatSilkPoint proposed) {
            return field.VisibilityAt(proposed.Position, proposed.Depth) >= .65 ? proposed : fallback;
        }

        static void BuildRetreat(List<HabitatSilkSegment> lines, List<HabitatSilkPoint> anchors,
            HabitatSilkPoint center, double aspect, SkinProfile skin) {
            SilkProfile silk = SilkProfile.Get(skin.Id);
            if (silk.Scaffold) {
                HabitatSilkPoint[] platform = SupportedFrame(lines, anchors, center, skin.Male ? .40 : .62, .008);
                for (int i = 0; i < platform.Length; i++) {
                    Add(lines, platform[i], center, HabitatSilkRole.Retreat, .55);
                    if (!skin.Male) {
                        HabitatSilkPoint upper = Offset(HabitatSilkPoint.Lerp(center, platform[i], .55), 0, -.034, -.035);
                        Add(lines, platform[i], upper, HabitatSilkRole.Retreat, .48);
                        Add(lines, upper, platform[(i + 1) % platform.Length], HabitatSilkRole.Retreat, .48);
                    }
                }
                return;
            }

            V axis = new V(1, 0);
            double longest = 0;
            for (int i = 0; i < anchors.Count; i++) {
                for (int j = i + 1; j < anchors.Count; j++) {
                    V delta = Metric(anchors[j].Position - anchors[i].Position, aspect);
                    double length = Dot(delta, delta);
                    if (length > longest) { longest = length; axis = Unit(delta); }
                }
            }
            if (axis.X < 0) axis = axis * -1;
            V side = new V(-axis.Y, axis.X);
            const int ribs = 8, around = 10;
            var sleeve = new HabitatSilkPoint[ribs, around];
            for (int rib = 0; rib < ribs; rib++) {
                double t = rib / (double)(ribs - 1);
                for (int i = 0; i < around; i++) {
                    double angle = i * FullTurn / around + .11 * Math.Sin(i * 2.2 + rib * 1.7);
                    double cross = Math.Cos(angle);
                    double width = Math.Abs(silk.Edge(t, cross >= 0 ? 1 : -1)) * .00083 * silk.Width;
                    double ripples = .0018 * Math.Sin(i * 2.5 + rib * 2.0) * Math.Sin(Math.PI * t);
                    V offset = axis * (silk.Along(t) * .00110 * silk.Length + ripples) + side * (cross * width);
                    sleeve[rib, i] = Offset(center, offset.X / aspect, offset.Y, Math.Sin(angle) * width * 1.45);
                }
            }
            foreach (HabitatSilkPoint anchor in anchors) {
                HabitatSilkPoint closest = sleeve[0, 0];
                double nearest = Double.MaxValue;
                for (int rib = 0; rib < ribs; rib++) {
                    for (int i = 0; i < around; i++) {
                        HabitatSilkPoint node = sleeve[rib, i];
                        double distance = Metric(anchor.Position - node.Position, aspect).Length + .40 * Math.Abs(anchor.Depth - node.Depth);
                        if (distance < nearest) { nearest = distance; closest = node; }
                    }
                }
                Add(lines, anchor, closest, HabitatSilkRole.Support, .95);
            }
            for (int i = 0; i < around; i += 2)
                Add(lines, sleeve[0, i], sleeve[ribs - 1, i], HabitatSilkRole.Frame, .67);
            for (int rib = 0; rib < ribs; rib++) {
                for (int i = 0; i < around; i++) {
                    int j = (i + 1) % around;
                    double weight = rib == 0 || rib == ribs - 1 ? .75 : i < around / 2 ? .60 : .43;
                    Add(lines, sleeve[rib, i], sleeve[rib, j], HabitatSilkRole.Retreat, weight);
                    if (rib > 0 && (silk.Density >= 1 || (i + rib) % 2 == 0))
                        Add(lines, sleeve[rib - 1, j], sleeve[rib, i], HabitatSilkRole.Retreat, .45);
                }
            }
        }

        static void Add(List<HabitatSilkSegment> lines, HabitatSilkPoint a, HabitatSilkPoint b, HabitatSilkRole role, double weight) {
            if ((a.Position - b.Position).Length > .0004)
                lines.Add(new HabitatSilkSegment(a, b, role, weight));
        }

        static HabitatSilkPoint Offset(HabitatSilkPoint p, double x, double y, double depth) {
            return new HabitatSilkPoint(new V(Clamp01(p.Position.X + x), Clamp01(p.Position.Y + y)), Clamp01(p.Depth + depth));
        }

        static V Metric(V v, double aspect) { return new V(v.X * aspect, v.Y); }
        static V Polar(double angle, double aspect) { return new V(Math.Cos(angle) / aspect, Math.Sin(angle)); }
        static V Unit(V v) { return v * (1 / Math.Max(.0001, v.Length)); }
        static double Dot(V a, V b) { return a.X * b.X + a.Y * b.Y; }
        static double Cross(V a, V b) { return a.X * b.Y - a.Y * b.X; }
        static bool Inside(V p) { return p.X >= 0 && p.X <= 1 && p.Y >= 0 && p.Y <= 1; }
        internal static double Clamp01(double value) { return Math.Max(0, Math.Min(1, value)); }
    }
}
