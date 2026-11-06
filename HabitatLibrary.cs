using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornerSpider {
    public sealed class HabitatLibraryEntry {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool WhiteIsNear { get; private set; }

        internal HabitatLibraryEntry(string id, string name, bool whiteIsNear) {
            Id = id;
            Name = name;
            WhiteIsNear = whiteIsNear;
        }
        public override string ToString() { return Name; }
    }

    public sealed class HabitatScene {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public BitmapSource Color { get; private set; }
        public HabitatDepth Field { get; private set; }

        internal HabitatScene(string id, string name, BitmapSource color, HabitatDepth field) {
            Id = id;
            Name = name;
            Color = color;
            Field = field;
        }
    }

    public static class HabitatLibrary {
        const int MaxFileBytes = 48 * 1024 * 1024;
        const int MaxInputSide = 8192;
        const long MaxInputPixels = 24000000;
        const int MaxColorSide = 4096;
        const int MaxDepthSide = 512;
        const string PolarityFile = "near.txt";

        public static HabitatLibraryEntry[] List(string profileFolder) {
            var entries = new List<HabitatLibraryEntry>();
            string root = LibraryFolder(profileFolder);
            if (!Directory.Exists(root)) return entries.ToArray();
            foreach (string folder in Directory.EnumerateDirectories(root)) {
                string id = Path.GetFileName(folder);
                if (!IsSceneId(id) || AssetPath(folder, "color") == null || AssetPath(folder, "depth") == null) continue;
                entries.Add(new HabitatLibraryEntry(id, ReadName(folder), ReadPolarity(folder)));
            }
            entries.Sort(delegate(HabitatLibraryEntry left, HabitatLibraryEntry right) {
                int name = StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name);
                return name == 0 ? StringComparer.Ordinal.Compare(left.Id, right.Id) : name;
            });
            return entries.ToArray();
        }

        public static HabitatScene Import(string profileFolder, string colorPath, string depthPath, bool whiteIsNear) {
            string id = Guid.NewGuid().ToString("N");
            using (var colorFile = OpenImage(colorPath, "habitat photo"))
            using (var depthFile = OpenImage(depthPath, "depth map")) {
                string colorExtension, depthExtension;
                string name = CleanName(Path.GetFileNameWithoutExtension(colorPath));
                HabitatScene scene = Decode(id, name, colorFile, depthFile, whiteIsNear, out colorExtension, out depthExtension);
                string root = LibraryFolder(profileFolder);
                string stage = Path.Combine(root, "." + id + ".new");
                string stagedColor = Path.Combine(stage, "color" + colorExtension);
                string stagedDepth = Path.Combine(stage, "depth" + depthExtension);
                string stagedName = Path.Combine(stage, "name.txt");
                string stagedPolarity = Path.Combine(stage, PolarityFile);
                Directory.CreateDirectory(stage);
                try {
                    CopyImage(colorFile, stagedColor);
                    CopyImage(depthFile, stagedDepth);
                    File.WriteAllText(stagedName, name);
                    File.WriteAllText(stagedPolarity, whiteIsNear ? "white" : "black");
                    Directory.Move(stage, Path.Combine(root, id));
                } catch {
                    CleanStage(stage, stagedColor, stagedDepth, stagedName, stagedPolarity);
                    throw;
                }
                return scene;
            }
        }

        public static HabitatScene Load(string profileFolder, string id, bool whiteIsNear) {
            string folder = SavedFolder(profileFolder, id);
            string colorPath = AssetPath(folder, "color"), depthPath = AssetPath(folder, "depth");
            if (colorPath == null || depthPath == null) throw new FileNotFoundException("The saved habitat's photo or depth map is missing. Import the pair again.");
            using (var colorFile = OpenImage(colorPath, "habitat photo"))
            using (var depthFile = OpenImage(depthPath, "depth map")) {
                string colorExtension, depthExtension;
                return Decode(id, ReadName(folder), colorFile, depthFile, whiteIsNear, out colorExtension, out depthExtension);
            }
        }

        public static void SetPolarity(string profileFolder, string id, bool whiteIsNear) {
            string folder = SavedFolder(profileFolder, id);
            string path = Path.Combine(folder, PolarityFile);
            string stage = path + "." + Guid.NewGuid().ToString("N") + ".new";
            try {
                File.WriteAllText(stage, whiteIsNear ? "white" : "black");
                try { File.Move(stage, path); }
                catch (IOException) {
                    if (!File.Exists(path)) throw;
                    File.Replace(stage, path, null);
                }
            } finally { File.Delete(stage); }
        }

        static string LibraryFolder(string profileFolder) { return Path.Combine(profileFolder, "habitats"); }

        static string SavedFolder(string profileFolder, string id) {
            if (!IsSceneId(id)) throw new InvalidDataException("The saved habitat identifier is invalid.");
            return Path.Combine(LibraryFolder(profileFolder), id);
        }

        static bool IsSceneId(string id) {
            Guid value;
            return id != null && Guid.TryParseExact(id, "N", out value) && value.ToString("N") == id;
        }

        static string CleanName(string name) {
            if (string.IsNullOrWhiteSpace(name)) return "Habitat";
            name = name.Trim();
            for (int i = 0; i < name.Length; i++) if (char.IsControl(name[i])) return "Habitat";
            return name.Length <= 80 ? name : name.Substring(0, 80);
        }

        static string ReadName(string folder) {
            string path = Path.Combine(folder, "name.txt");
            try {
                using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var text = new StreamReader(input)) {
                    if (input.Length > 512) return "Habitat";
                    return CleanName(text.ReadToEnd());
                }
            } catch (IOException) { return "Habitat"; }
            catch (UnauthorizedAccessException) { return "Habitat"; }
        }

        static bool ReadPolarity(string folder) {
            try {
                using (var input = new FileStream(Path.Combine(folder, PolarityFile), FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                using (var text = new StreamReader(input)) {
                    return input.Length > 16 || text.ReadToEnd().Trim() != "black";
                }
            } catch (IOException) { return true; }
            catch (UnauthorizedAccessException) { return true; }
        }

        static string AssetPath(string folder, string name) {
            foreach (string extension in new[] { ".png", ".jpg" }) {
                string path = Path.Combine(folder, name + extension);
                if (File.Exists(path)) return path;
            }
            return null;
        }

        static FileStream OpenImage(string path, string label) {
            if (string.IsNullOrWhiteSpace(path)) throw new InvalidDataException("Choose a " + label + " first.");
            var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (file.Length > 0 && file.Length <= MaxFileBytes) return file;
            file.Dispose();
            throw new InvalidDataException("The " + label + " must be a nonempty PNG or JPEG no larger than 48 MB.");
        }

        static void CopyImage(FileStream source, string destination) {
            source.Position = 0;
            using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                source.CopyTo(output);
        }

        static void CleanStage(string folder, params string[] paths) {
            try {
                foreach (string path in paths) File.Delete(path);
                Directory.Delete(folder);
            } catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        static HabitatScene Decode(string id, string name, FileStream colorFile, FileStream depthFile, bool whiteIsNear,
                out string colorExtension, out string depthExtension) {
            try {
                BitmapFrame color = ReadFrame(colorFile, "habitat photo", out colorExtension);
                BitmapFrame depth = ReadFrame(depthFile, "depth map", out depthExtension);
                bool nearlySameSize = Math.Abs(color.PixelWidth - depth.PixelWidth) <= 1 && Math.Abs(color.PixelHeight - depth.PixelHeight) <= 1;
                double aspectRatio = (double)color.PixelWidth * depth.PixelHeight / ((double)color.PixelHeight * depth.PixelWidth);
                if (!nearlySameSize && Math.Abs(1 - aspectRatio) > .0025) {
                    throw new InvalidDataException(string.Format(
                        "The habitat photo is {0} x {1} and the depth map is {2} x {3}. Use a depth map with the same aspect ratio and crop as the photo.",
                        color.PixelWidth, color.PixelHeight, depth.PixelWidth, depth.PixelHeight));
                }
                HabitatDepth field = ReadDepth(depth, whiteIsNear);
                return new HabitatScene(id, name, FreezeColor(color), field);
            } catch (FileFormatException ex) { throw DecodeError(ex); }
            catch (NotSupportedException ex) { throw DecodeError(ex); }
            catch (COMException ex) { throw DecodeError(ex); }
        }

        static InvalidDataException DecodeError(Exception inner) {
            return new InvalidDataException("The habitat photo or depth map could not be read. Use standard PNG or JPEG images.", inner);
        }

        static BitmapFrame ReadFrame(FileStream file, string label, out string extension) {
            var decoder = BitmapDecoder.Create(file, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
            if (decoder is PngBitmapDecoder) extension = ".png";
            else if (decoder is JpegBitmapDecoder) extension = ".jpg";
            else throw new InvalidDataException("The " + label + " must be a PNG or JPEG image.");
            if (decoder.Frames.Count != 1) throw new InvalidDataException("The " + label + " must contain one image.");
            BitmapFrame frame = decoder.Frames[0];
            int width = frame.PixelWidth, height = frame.PixelHeight;
            if (width < 32 || height < 32 || width > MaxInputSide || height > MaxInputSide || (long)width * height > MaxInputPixels) {
                throw new InvalidDataException("The " + label + " must be at least 32 x 32, at most 8192 pixels on either side, and under 24 megapixels.");
            }
            return frame;
        }

        static BitmapSource FreezeColor(BitmapSource frame) {
            double scale = Math.Min(1, (double)MaxColorSide / Math.Max(frame.PixelWidth, frame.PixelHeight));
            BitmapSource scaled = scale < 1 ? new TransformedBitmap(frame, new ScaleTransform(scale, scale)) : frame;
            var color = new FormatConvertedBitmap(scaled, PixelFormats.Bgra32, null, 0);
            int stride = color.PixelWidth * 4;
            byte[] pixels = new byte[stride * color.PixelHeight];
            color.CopyPixels(pixels, stride, 0);
            BitmapSource result = BitmapSource.Create(color.PixelWidth, color.PixelHeight, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
            result.Freeze();
            return result;
        }

        static HabitatDepth ReadDepth(BitmapSource frame, bool whiteIsNear) {
            var depth = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
            int width = depth.PixelWidth, height = depth.PixelHeight, stride = width * 4;
            double scale = Math.Min(1, (double)MaxDepthSide / Math.Max(width, height));
            int columns = Math.Max(2, (int)Math.Round(width * scale));
            int rows = Math.Max(2, (int)Math.Round(height * scale));
            byte[] result = new byte[columns * rows], pixels = new byte[stride];
            var sums = new int[columns];
            var low = new byte[columns];
            var high = new byte[columns];
            var centers = new byte[columns];
            for (int row = 0; row < rows; row++) {
                int top = row * height / rows, bottom = (row + 1) * height / rows;
                for (int column = 0; column < columns; column++) { sums[column] = 0; low[column] = 255; high[column] = 0; }
                for (int y = top; y < bottom; y++) {
                    depth.CopyPixels(new Int32Rect(0, y, width, 1), pixels, stride, 0);
                    for (int column = 0; column < columns; column++) {
                        int left = column * width / columns, right = (column + 1) * width / columns;
                        for (int x = left; x < right; x++) {
                            int index = x * 4;
                            int b = pixels[index], g = pixels[index + 1], r = pixels[index + 2];
                            // RGB exports of grayscale maps can carry a small chroma residue.
                            if (pixels[index + 3] != 255 || Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b)) > 16)
                                throw new InvalidDataException("The depth map must be opaque grayscale. Choose the matching depth map or export it in grayscale.");
                            byte tone = (byte)((77 * r + 150 * g + 29 * b + 128) >> 8);
                            sums[column] += tone;
                            if (tone < low[column]) low[column] = tone;
                            if (tone > high[column]) high[column] = tone;
                            if (y == (top + bottom) / 2 && x == (left + right) / 2) centers[column] = tone;
                        }
                    }
                }
                for (int column = 0; column < columns; column++) {
                    int samples = ((column + 1) * width / columns - column * width / columns) * (bottom - top);
                    // Keep real foreground/background samples at hard edges instead of inventing a ramp.
                    int tone = high[column] - low[column] > 32 ? centers[column] : (sums[column] + samples / 2) / samples;
                    result[row * columns + column] = (byte)(whiteIsNear ? tone : 255 - tone);
                }
            }
            return new HabitatDepth(columns, rows, result);
        }
    }
}
