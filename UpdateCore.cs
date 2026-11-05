using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace CornerSpider {
    public sealed class ReleaseVersion : IComparable<ReleaseVersion> {
        public int Major,Minor,Patch;public string Preview="",Text;
        public static ReleaseVersion Parse(string value) {
            if(value==null || value.Length>80) return null;
            var m=Regex.Match(value,@"^v?(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$");
            int a,b,c;if(!m.Success || !int.TryParse(m.Groups[1].Value,out a) || !int.TryParse(m.Groups[2].Value,out b) || !int.TryParse(m.Groups[3].Value,out c)) return null;
            string pre=m.Groups[4].Value;foreach(string part in pre.Split('.')) if(Regex.IsMatch(part,@"^0[0-9]+$")) return null;
            return new ReleaseVersion { Major=a,Minor=b,Patch=c,Preview=pre,Text=a+"."+b+"."+c+(pre.Length>0?"-"+pre:"") };
        }
        public int CompareTo(ReleaseVersion other) {
            int n=Major.CompareTo(other.Major);if(n!=0) return n;n=Minor.CompareTo(other.Minor);if(n!=0) return n;n=Patch.CompareTo(other.Patch);if(n!=0) return n;
            if(Preview==other.Preview) return 0;if(Preview.Length==0) return 1;if(other.Preview.Length==0) return -1;
            string[] a=Preview.Split('.'),b=other.Preview.Split('.');
            for(int i=0;i<Math.Min(a.Length,b.Length);i++) { bool x=Regex.IsMatch(a[i],@"^[0-9]+$"),y=Regex.IsMatch(b[i],@"^[0-9]+$");
                n=x && y?(a[i].Length!=b[i].Length?a[i].Length.CompareTo(b[i].Length):string.CompareOrdinal(a[i],b[i])):x!=y?(x?-1:1):string.CompareOrdinal(a[i],b[i]);if(n!=0) return n; }
            return a.Length.CompareTo(b.Length);
        }
    }
    public sealed class ReleaseInfo {
        public string Tag,ZipName,ZipUrl,ChecksumsUrl,PageUrl;
        public ReleaseVersion Version;
    }
    public static class UpdateCore {
        public const string Repository="https://github.com/tedparkes/corner-spider";
        public const string Feed="https://api.github.com/repos/tedparkes/corner-spider/releases?per_page=100";
        public const long MaxArchive=64L*1024*1024,MaxExpanded=200L*1024*1024;
        static JavaScriptSerializer Json() { return new JavaScriptSerializer { MaxJsonLength=2*1024*1024,RecursionLimit=40 }; }
        static string Field(IDictionary<string,object> d,string key) { object v;return d.TryGetValue(key,out v)?v as string:null; }
        public static ReleaseInfo SelectRelease(string json,bool previews) {
            var list=Json().DeserializeObject(json) as object[];if(list==null) throw new InvalidDataException("GitHub returned an unexpected release list.");ReleaseInfo best=null;
            foreach(object item in list) {
                var d=item as IDictionary<string,object>;object draft,pre,assets;if(d==null || !d.TryGetValue("draft",out draft) || !(draft is bool) || (bool)draft || !d.TryGetValue("prerelease",out pre) || !(pre is bool) || string.IsNullOrEmpty(Field(d,"published_at"))) continue;
                string tag=Field(d,"tag_name");var version=ReleaseVersion.Parse(tag);if(version==null || (!previews && ((bool)pre || version.Preview.Length>0))) continue;
                var candidate=new ReleaseInfo { Tag=tag,Version=version,ZipName="CornerSpider-v"+version.Text+"-Windows.zip",PageUrl=Repository+"/releases/tag/"+tag };
                if(!d.TryGetValue("assets",out assets) || !(assets is object[])) continue;
                foreach(object asset in (object[])assets) { var a=asset as IDictionary<string,object>;if(a==null) continue;string name=Field(a,"name"),url=Field(a,"browser_download_url");
                    if(url!=Repository+"/releases/download/"+tag+"/"+name) continue;
                    if(name==candidate.ZipName) candidate.ZipUrl=url;if(name=="SHA256SUMS.txt") candidate.ChecksumsUrl=url;
                }
                if(candidate.ZipUrl!=null && candidate.ChecksumsUrl!=null && (best==null || version.CompareTo(best.Version)>0)) best=candidate;
            }return best;
        }
        public static string HashFile(string path) { using(var s=File.OpenRead(path)) using(var sha=SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(s)).Replace("-","").ToLowerInvariant(); }
        public static string ExpectedHash(string checksums,string file) {
            string found=null;foreach(string line in checksums.Split('\n')) { var match=Regex.Match(line.Trim(),@"^([0-9a-fA-F]{64})\s+\*?(.+)$");if(!match.Success || match.Groups[2].Value!=file) continue;if(found!=null) throw new InvalidDataException("Duplicate checksum entry.");found=match.Groups[1].Value.ToLowerInvariant(); }
            if(found==null) throw new InvalidDataException("The Windows package has no SHA-256 checksum.");return found;
        }
        static bool DownloadHost(Uri uri) { return uri.Scheme=="https" && uri.IsDefaultPort && uri.UserInfo.Length==0 && (uri.Host=="api.github.com" || uri.Host=="github.com" || uri.Host=="release-assets.githubusercontent.com" || uri.Host=="objects.githubusercontent.com"); }
        public static void Download(string url,Stream output,long maximum,CancellationToken cancel) {
            Uri uri=new Uri(url);ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
            for(int redirects=0;redirects<5;redirects++) {
                if(!DownloadHost(uri)) throw new InvalidDataException("Unexpected download host.");cancel.ThrowIfCancellationRequested();
                var request=(HttpWebRequest)WebRequest.Create(uri);request.UserAgent="CornerSpider-Updater";request.Accept="application/vnd.github+json";request.Timeout=20000;request.ReadWriteTimeout=20000;request.AllowAutoRedirect=false;
                using(cancel.Register(request.Abort)) using(var response=(HttpWebResponse)request.GetResponse()) {
                    int status=(int)response.StatusCode;if(status>=300 && status<400) { uri=new Uri(uri,response.Headers["Location"]);continue; }
                    if(status!=200 || response.ContentLength>maximum) throw new InvalidDataException("Unexpected or oversized download.");
                    using(var input=response.GetResponseStream()) { byte[] buffer=new byte[65536];long total=0;int count;var timer=Stopwatch.StartNew();
                        while((count=input.Read(buffer,0,buffer.Length))>0) { cancel.ThrowIfCancellationRequested();total+=count;if(total>maximum || timer.Elapsed.TotalMinutes>3) throw new InvalidDataException("Download exceeded its limit.");output.Write(buffer,0,count); }
                    }return;
                }
            }throw new InvalidDataException("Too many download redirects.");
        }
        public static string Text(string url,CancellationToken token) { using(var stream=new MemoryStream()) { Download(url,stream,2*1024*1024,token);return Encoding.UTF8.GetString(stream.ToArray()); } }
        public static string SafePath(string root,string relative) {
            if(string.IsNullOrEmpty(relative) || relative.Length>220 || relative.IndexOf('\\')>=0 || relative.IndexOf(':')>=0 || relative.StartsWith("/")) throw new InvalidDataException("Unsafe package path.");
            foreach(string part in relative.Split('/')) if(part.Length==0 || part=="." || part==".." || part.EndsWith(".") || part.EndsWith(" ") || part.IndexOfAny(Path.GetInvalidFileNameChars())>=0 || Regex.IsMatch(part,@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)",RegexOptions.IgnoreCase)) throw new InvalidDataException("Unsafe package name.");
            string full=Path.GetFullPath(Path.Combine(root,relative.Replace('/',Path.DirectorySeparatorChar))),prefix=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
            if(!full.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Package escaped its directory.");return full;
        }
        public static void ExtractVerified(string zip,string expected,string destination,string version) {
            if(!Regex.IsMatch(expected??"",@"^[0-9a-fA-F]{64}$") || !string.Equals(HashFile(zip),expected,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("The download checksum does not match. Nothing was installed.");
            if(new FileInfo(zip).Length>MaxArchive || Directory.Exists(destination)) throw new InvalidDataException("Invalid staging destination or archive size.");
            Directory.CreateDirectory(destination);var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);long total=0;
            using(var archive=ZipFile.OpenRead(zip)) {
                if(archive.Entries.Count>500) throw new InvalidDataException("Too many package entries.");
                foreach(var entry in archive.Entries) {
                    if(entry.FullName.EndsWith("/")) continue;
                    if(!entry.FullName.StartsWith("CornerSpider/",StringComparison.Ordinal) || ((entry.ExternalAttributes>>16)&0xf000)==0xa000) throw new InvalidDataException("Unexpected package layout.");
                    string relative=entry.FullName.Substring(13);if(!seen.Add(relative) || relative.Equals("settings.txt",StringComparison.OrdinalIgnoreCase) || relative.StartsWith(".git",StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unexpected or duplicate package file.");
                    string target=SafePath(destination,relative);total+=entry.Length;if(total>MaxExpanded) throw new InvalidDataException("Expanded package is too large.");
                    Directory.CreateDirectory(Path.GetDirectoryName(target));using(var from=entry.Open()) using(var to=File.Create(target)) { byte[] buffer=new byte[65536];long size=0;int n;while((n=from.Read(buffer,0,buffer.Length))>0) { size+=n;if(size>entry.Length) throw new InvalidDataException("Invalid entry length.");to.Write(buffer,0,n); } }
                }
            }
            string exe=Path.Combine(destination,"JumpingSpider.exe");if(!File.Exists(exe) || !File.Exists(Path.Combine(destination,"JumpingSpider.exe.config"))) throw new InvalidDataException("The package is missing the Windows app.");
            var actual=ReleaseVersion.Parse(FileVersionInfo.GetVersionInfo(exe).ProductVersion);var wanted=ReleaseVersion.Parse(version);
            if(actual==null || wanted==null || actual.CompareTo(wanted)!=0) throw new InvalidDataException("The executable version does not match the release.");
        }
        public static string Prepare(ReleaseInfo release,string settingsFolder,CancellationToken token) {
            string updates=Path.Combine(settingsFolder,"updates");
            if(Directory.Exists(updates)) { var old=new List<DirectoryInfo>();foreach(var dir in new DirectoryInfo(updates).GetDirectories()) if(Regex.IsMatch(dir.Name,@"^[a-f0-9]{32}$")) old.Add(dir);
                old.Sort(delegate(DirectoryInfo a,DirectoryInfo b) { return b.CreationTimeUtc.CompareTo(a.CreationTimeUtc); });
                for(int i=2;i<old.Count;i++) try { if((old[i].Attributes&FileAttributes.ReparsePoint)==0) old[i].Delete(true); }catch(IOException) { }catch(UnauthorizedAccessException) { }
            }
            string stage=Path.Combine(settingsFolder,"updates",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(stage);
            try { string hash=ExpectedHash(Text(release.ChecksumsUrl,token),release.ZipName),zip=Path.Combine(stage,"download.zip");
                using(var output=File.Create(zip)) Download(release.ZipUrl,output,MaxArchive,token);
                ExtractVerified(zip,hash,Path.Combine(stage,"payload"),release.Version.Text);
                File.WriteAllLines(Path.Combine(stage,"verified.txt"),new[]{release.Version.Text,hash});return stage;
            } catch { Directory.Delete(stage,true);throw; }
        }
        static void RejectLinks(string root,string path) {
            string current=path;while(current!=null && current.Length>=root.Length) { if((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0) throw new IOException("The install folder contains a linked path; use a normal writable folder.");if(string.Equals(current,root,StringComparison.OrdinalIgnoreCase)) break;current=Path.GetDirectoryName(current); }
        }
        public static void ApplyPayload(string payload,string target,string backup,Action<int> afterCopy=null) {
            target=Path.GetFullPath(target);var changed=new List<string>();var created=new HashSet<string>(StringComparer.OrdinalIgnoreCase);Directory.CreateDirectory(backup);
            try {
                foreach(string source in Directory.GetFiles(payload,"*",SearchOption.AllDirectories)) {
                    string relative=source.Substring(Path.GetFullPath(payload).Length+1).Replace('\\','/'),dest=SafePath(target,relative),old=SafePath(backup,relative);
                    RejectLinks(target,dest);Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    if(File.Exists(dest)) { Directory.CreateDirectory(Path.GetDirectoryName(old));File.Copy(dest,old,false); }else created.Add(relative);
                    changed.Add(relative);File.Copy(source,dest,true);if(afterCopy!=null) afterCopy(changed.Count);
                }
            } catch {
                for(int i=changed.Count-1;i>=0;i--) { string relative=changed[i],dest=SafePath(target,relative);if(created.Contains(relative)) File.Delete(dest);else File.Copy(SafePath(backup,relative),dest,true); }throw;
            }
        }
        public static void StartInstaller(string stage,bool restart) {
            string exe=Process.GetCurrentProcess().MainModule.FileName,target=Path.GetDirectoryName(exe);
            string[] verified=File.ReadAllLines(Path.Combine(stage,"verified.txt"));
            if(ReleaseVersion.Parse(verified[0]).CompareTo(ReleaseVersion.Parse(FileVersionInfo.GetVersionInfo(exe).ProductVersion))<=0) throw new InvalidDataException("An update must be newer than the installed version.");
            File.WriteAllText(Path.Combine(stage,"ticket.json"),Json().Serialize(new Dictionary<string,object> { {"target",target},{"pid",Process.GetCurrentProcess().Id},{"restart",restart} }));
            string helper=Path.Combine(stage,"CornerSpider.Update.exe");File.Copy(exe,helper,false);
            Process.Start(new ProcessStartInfo(helper,"--apply-update") { WorkingDirectory=stage,UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden });
        }
        public static int RunInstaller() {
            string stage=Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),result=Path.Combine(stage,"result.txt");
            string restartTarget=null;bool restartOnFailure=false;
            try {
                var ticket=Json().Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(stage,"ticket.json")));string target=Path.GetFullPath((string)ticket["target"]);int pid=Convert.ToInt32(ticket["pid"]);bool restart=(bool)ticket["restart"];
                restartTarget=target;restartOnFailure=restart;
                if(pid<=0 || target==stage) throw new InvalidDataException("Invalid update ticket.");
                try { using(var parent=Process.GetProcessById(pid)) { if(!string.Equals(parent.MainModule.FileName,Path.Combine(target,"JumpingSpider.exe"),StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("The running app changed.");if(!parent.WaitForExit(30000)) throw new IOException("The app has not closed; update postponed."); } }catch(ArgumentException) { }
                using(var mutex=new Mutex(false,"Local\\CornerSpiderPet_v1")) {
                    if(!mutex.WaitOne(0)) throw new IOException("Another pet is running; update postponed.");
                    try {
                        var verified=File.ReadAllLines(Path.Combine(stage,"verified.txt"));var installed=ReleaseVersion.Parse(FileVersionInfo.GetVersionInfo(Path.Combine(target,"JumpingSpider.exe")).ProductVersion);
                        if(installed==null || ReleaseVersion.Parse(verified[0]).CompareTo(installed)<=0) throw new InvalidDataException("Update is no longer newer.");
                        string payload=Path.Combine(stage,"apply");ExtractVerified(Path.Combine(stage,"download.zip"),verified[1],payload,verified[0]);ApplyPayload(payload,target,Path.Combine(stage,"backup"));
                        File.WriteAllText(result,"Installed "+verified[0]+". The previous app files are in this folder's backup directory.");
                    } finally { mutex.ReleaseMutex(); }
                }
                if(restart) Process.Start(new ProcessStartInfo(Path.Combine(target,"JumpingSpider.exe")) { WorkingDirectory=target,UseShellExecute=false,CreateNoWindow=true });return 0;
            } catch(Exception ex) {
                File.WriteAllText(result,"Update failed: "+ex.Message+" Replaced files were backed up in this update folder; check the backup if recovery is needed.");
                if(restartOnFailure && File.Exists(Path.Combine(restartTarget,"JumpingSpider.exe"))) try { Process.Start(new ProcessStartInfo(Path.Combine(restartTarget,"JumpingSpider.exe")) { WorkingDirectory=restartTarget,UseShellExecute=false,CreateNoWindow=true }); }catch { }
                return 1;
            }
        }
    }
}
