using System;
using System.Collections.Generic;
using CornerSpider;

internal static class HabitatSilkTests {
    static void Check(bool condition, string message) {
        if (!condition) throw new Exception(message);
    }

    static HabitatDepth MakeDepth(int width, int height, Func<double, double, byte> sample) {
        var values = new byte[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) values[y * width + x] = sample(x / (double)(width - 1), y / (double)(height - 1));
        }
        return new HabitatDepth(width, height, values);
    }

    static HabitatDepth Clearing() {
        return MakeDepth(240, 160, delegate(double x, double y) {
            double left = .37 + .023 * Math.Sin(y * 7), right = .67 + .034 * Math.Cos(y * 6);
            if (x < left) return (byte)(208 - y * 24);
            if (x > right) return (byte)(148 + y * 19);
            if (y < .28 - .20 * Math.Abs(x - .50)) return 164;
            if (y > .77 + .30 * Math.Abs(x - .53)) return 139;
            return (byte)(65 + y * 30);
        });
    }

    static int Count(HabitatSilkPlan plan, HabitatSilkRole role) {
        int total = 0;
        foreach (HabitatSilkSegment segment in plan.Segments) if (segment.Role == role) total++;
        return total;
    }

    static bool Same(HabitatSilkPoint a, HabitatSilkPoint b) {
        return (a.Position - b.Position).Length < .000001 && Math.Abs(a.Depth - b.Depth) < .000001;
    }

    static void VerifyGeometry(HabitatDepth field, V origin, HabitatSilkPlan plan) {
        Check(plan.Anchors.Count >= 4, "Habitat needs enough distinct roots to span a clearing");
        Check(Count(plan, HabitatSilkRole.Support) == plan.Anchors.Count, "Every measured root has one initial support");
        Check(Same(plan.Segments[0].Start, plan.Landing), "Construction begins at the reachable landing anchor");
        List<HabitatStep> route;
        Check(HabitatRoute.TryPlan(field, origin, plan.Landing.Position, out route), "The landing is reachable on the depth map");
        Check(field.VisibilityAt(plan.Perch.Position, plan.Perch.Depth) >= .65, "The resting perch is visible inside the web");

        double min = 1, max = 0;
        for (int i = 0; i < plan.Anchors.Count; i++) {
            HabitatSilkPoint anchor = plan.Anchors[i];
            Check(Math.Abs(anchor.Depth - field.DepthAt(anchor.Position)) < .000001, "The anchor uses measured depth");
            Check(plan.Segments[i].Role == HabitatSilkRole.Support && Same(anchor, plan.Segments[i].Start),
                "Anchors and supports are laid before the frame");
            for (int j = 0; j < i; j++) Check((anchor.Position - plan.Anchors[j].Position).Length > .015, "Roots have distinct projections");
            min = Math.Min(min, anchor.Depth); max = Math.Max(max, anchor.Depth);
        }
        Check(max - min >= .10, "The supports occupy different measured depth levels");
        foreach (HabitatSilkSegment segment in plan.Segments) {
            Check(segment.Start.Depth >= 0 && segment.Start.Depth <= 1 && segment.End.Depth >= 0 && segment.End.Depth <= 1,
                "Silk stays inside the scene's depth range");
            Check((segment.Start.Position - segment.End.Position).Length > .0001, "Silk spans a nonzero projected length");
        }
    }

    static void VerifyBuild(HabitatSilkPlan plan) {
        var build = new HabitatSilkConstruction(plan);
        Check(build.VisibleCount == 0 && !build.Complete, "Silk starts empty");
        build.Advance(.001);
        Check(build.VisibleCount == 1 && build.SegmentProgress(0) > 0 && build.SegmentProgress(0) < 1,
            "Silk grows continuously from the first support");
        int previous = build.VisibleCount;
        for (int i = 0; i < 100; i++) {
            build.Advance(plan.Duration / 100);
            int count = build.VisibleCount;
            Check(count >= previous, "Visible strands accumulate in order");
            if (count > 1) Check(build.SegmentProgress(count - 2) == 1, "Every strand before the growing strand is complete");
            if (count < plan.Segments.Count) Check(build.SegmentProgress(count) == 0, "Later strands wait their turn");
            previous = count;
        }
        Check(build.Complete && build.Progress == 1 && build.VisibleCount == plan.Segments.Count &&
            build.SegmentProgress(plan.Segments.Count - 1) == 1, "Construction finishes the entire web");
    }

    public static void Run() {
        var origin = new V(.23, .60);
        Check(HabitatSilk.Plan(MakeDepth(40, 30, delegate { return 128; }), "CrossOrb", origin) == null,
            "A flat depth image has no floating substitute web");
        Check(HabitatSilk.Plan(MakeDepth(80, 60, delegate(double x, double y) { return (byte)(25 + 170 * y); }),
            "CrossOrb", origin) == null, "A smooth depth gradient alone does not supply anchors");

        HabitatDepth field = Clearing();
        Check(HabitatSilk.Plan(field, "CrossOrbMale", origin) == null, "Male orb-weavers roam without building a capture orb");
        string[] species = { "CrossOrb", "SouthernWidow", "BarnFunnel", "Regal", "PortiaLabiata", "PortiaLabiataMale", "Crystal" };
        var plans = new Dictionary<string, HabitatSilkPlan>();
        foreach (string name in species) {
            HabitatSilkPlan plan = HabitatSilk.Plan(field, name, origin);
            Check(plan != null, "A clearing supports habitat silk for " + name);
            VerifyGeometry(field, origin, plan);
            VerifyBuild(plan);
            plans.Add(name, plan);
        }
        HabitatSilkPlan orb = plans["CrossOrb"];
        Check(orb.Kind == WebKind.Orb && Count(orb, HabitatSilkRole.Capture) > 150, "An orb includes a capture spiral");
        Check(plans["Crystal"].Kind == WebKind.Crystal && Count(plans["Crystal"], HabitatSilkRole.Capture) < Count(orb, HabitatSilkRole.Capture),
            "The fictional crystal uses a sparser radial pattern");
        Check(Count(plans["SouthernWidow"], HabitatSilkRole.Gumfoot) > 0, "A widow's tangle has descending gumfoot lines");
        Check(Count(plans["BarnFunnel"], HabitatSilkRole.Capture) > 70 && Count(plans["BarnFunnel"], HabitatSilkRole.Retreat) > 20,
            "The funnel has both a sheet and a recessed tunnel");
        Check(Count(plans["Regal"], HabitatSilkRole.Retreat) > 50 && Count(plans["Regal"], HabitatSilkRole.Capture) == 0,
            "A jumping spider makes a retreat rather than a capture web");
        Check(Count(plans["PortiaLabiata"], HabitatSilkRole.Retreat) > Count(plans["PortiaLabiataMale"], HabitatSilkRole.Retreat),
            "Female Portia has a layered retreat and the male has an open platform");

        HabitatSilkPlan repeat = HabitatSilk.Plan(field, "CrossOrb", origin);
        Check(repeat.Segments.Count == orb.Segments.Count && Same(repeat.Landing, orb.Landing) && Same(repeat.Perch, orb.Perch),
            "Selecting the same habitat and species produces the same site");
        Console.WriteLine("PASS: depth-aware habitat silk, anchored orb/tangle/funnel/retreat shapes and gradual construction.");
    }
}
