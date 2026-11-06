using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using CornerSpider;
static class UpdateTests {
    static int checks;
    static void Check(bool ok,string why) { checks++;if(!ok) throw new Exception("Updater: "+why); }
    static void Reject(Action call,string why) { bool rejected=false;try { call(); }catch(InvalidDataException) { rejected=true; }Check(rejected,why); }
    static string Release(string version,bool preview,string repository=UpdateCore.Repository) {
        return "{\"tag_name\":\"v"+version+"\",\"published_at\":\"2026-11-05T00:00:00Z\",\"draft\":false,\"prerelease\":"+(preview?"true":"false")+",\"assets\":[{\"name\":\"CornerSpider-v"+version+"-Windows.zip\",\"browser_download_url\":\""+repository+"/releases/download/v"+version+"/CornerSpider-v"+version+"-Windows.zip\"},{\"name\":\"SHA256SUMS.txt\",\"browser_download_url\":\""+repository+"/releases/download/v"+version+"/SHA256SUMS.txt\"}]}";
    }
    static void Zip(string file,string exe,string extra) {
        using(var archive=ZipFile.Open(file,ZipArchiveMode.Create)) {
            archive.CreateEntryFromFile(exe,"CornerSpider/JumpingSpider.exe");using(var s=new StreamWriter(archive.CreateEntry("CornerSpider/JumpingSpider.exe.config").Open())) s.Write("<configuration/>");
            if(extra!=null) using(var s=new StreamWriter(archive.CreateEntry(extra).Open())) s.Write("malformed fixture");
        }
    }
    public static void Run() {
        string[] ordered={"1.0.0","1.1.0-alpha.1","1.1.0-alpha.2","1.1.0-alpha.10","1.1.0-beta.1","1.1.0","1.2.0","1.10.0","2.0.0"};
        for(int i=0;i<ordered.Length;i++) for(int j=0;j<ordered.Length;j++) Check(Math.Sign(ReleaseVersion.Parse(ordered[i]).CompareTo(ReleaseVersion.Parse(ordered[j])))==Math.Sign(i-j),"semantic version ordering including numeric prereleases");
        foreach(string bad in new[]{"../1.0.0","1.1","01.0.0","1.1.0-alpha.01","1.2.3.exe",null}) Check(ReleaseVersion.Parse(bad)==null,"invalid version rejected");
        string feed="["+Release("1.0.0",false)+","+Release("1.2.0-alpha.2",true)+","+Release("1.1.0",false)+"]";
        Check(UpdateCore.SelectRelease(feed,false).Version.Text=="1.1.0","stable channel ignores previews");Check(UpdateCore.SelectRelease(feed,true).Version.Text=="1.2.0-alpha.2","preview channel considers newer prereleases");
        string stable=Release("1.2.0",false),draft=Release("1.3.0-alpha.1",true).Replace("\"draft\":false","\"draft\":true");
        string incomplete=Release("1.3.0-alpha.2",true).Replace("SHA256SUMS.txt","other.txt");
        string mixed="["+Release("1.2.0-alpha.10",true)+","+stable+","+Release("1.2.0-alpha.2",true)+","+draft+","+incomplete+"]";
        Check(UpdateCore.SelectRelease(mixed,ReleaseChannel.PrereleasesOnly).Version.Text=="1.2.0-alpha.10","one-time check finds the newest complete published prerelease even with a newer stable release");
        Check(UpdateCore.SelectRelease(mixed,true).Version.Text=="1.2.0","regular checks that include prereleases still prefer a newer stable release");
        Check(UpdateCore.SelectRelease("["+stable+"]",ReleaseChannel.PrereleasesOnly)==null,"a prerelease check does not return a stable package");
        Check(UpdateCore.SelectRelease("["+Release("9.0.0",false,"https://example.com")+"]",true)==null,"foreign download links are not trusted");
        Check(UpdateCore.SelectRelease("["+Release("1.0.0",false)+"]",false).Version.CompareTo(ReleaseVersion.Parse("1.1.0-alpha.2"))<0,"published 1.0 is not an update for this preview");
        string hash=new string('a',64);Check(UpdateCore.ExpectedHash(hash+"  app.zip\n","app.zip")==hash,"exact checksum selected");Reject(delegate { UpdateCore.ExpectedHash(hash+"  app.zip\n"+hash+"  app.zip","app.zip"); },"ambiguous checksums rejected");
        string folder=Path.Combine(Path.GetTempPath(),"CornerSpider-update-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        try {
            foreach(string bad in new[]{"../outside","/absolute","C:/absolute","a\\b","dir/../x","NUL.txt","file.","a//b","file:stream"}) Reject(delegate { UpdateCore.SafePath(folder,bad); },"unsafe extraction path rejected: "+bad);
            string source=Path.Combine(folder,"Fixture.cs"),exe=Path.Combine(folder,"fixture.exe");File.WriteAllText(source,"[assembly:System.Reflection.AssemblyInformationalVersion(\"1.2.3\")] class Fixture {static void Main() {}}");
            string compiler=Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),"csc.exe");
            using(var process=Process.Start(new ProcessStartInfo(compiler,"/nologo /target:winexe /out:\""+exe+"\" \""+source+"\"") { UseShellExecute=false,CreateNoWindow=true })) { process.WaitForExit();Check(process.ExitCode==0,"build a local versioned package fixture"); }
            string zip=Path.Combine(folder,"valid.zip");Zip(zip,exe,null);string expected=UpdateCore.HashFile(zip),payload=Path.Combine(folder,"payload");UpdateCore.ExtractVerified(zip,expected,payload,"1.2.3");Check(File.Exists(Path.Combine(payload,"JumpingSpider.exe")),"valid checksummed package extracts");
            Reject(delegate { UpdateCore.ExtractVerified(zip,hash,Path.Combine(folder,"bad-hash"),"1.2.3"); },"modified downloads rejected before installation");
            Reject(delegate { UpdateCore.ExtractVerified(zip,expected,Path.Combine(folder,"bad-version"),"1.2.4"); },"package version mismatch rejected");
            int count=0;foreach(string bad in new[]{"CornerSpider/../escape.txt","CornerSpider/JumpingSpider.EXE","Other/app.exe","CornerSpider/settings.txt"}) { string malformed=Path.Combine(folder,"bad"+(count++)+".zip");Zip(malformed,exe,bad);Reject(delegate { UpdateCore.ExtractVerified(malformed,UpdateCore.HashFile(malformed),Path.Combine(folder,"ex"+count),"1.2.3"); },"malformed archive rejected: "+bad); }
            string install=Path.Combine(folder,"installed"),files=Path.Combine(folder,"files"),backup=Path.Combine(folder,"backup");Directory.CreateDirectory(install);Directory.CreateDirectory(files);
            File.WriteAllText(Path.Combine(install,"app.txt"),"original");File.WriteAllText(Path.Combine(install,"personal.txt"),"untouched");File.WriteAllText(Path.Combine(files,"app.txt"),"updated");File.WriteAllText(Path.Combine(files,"new.txt"),"new");
            bool failed=false;try { UpdateCore.ApplyPayload(files,install,backup,delegate(int n) { if(n==2) throw new IOException("Simulated interrupted installation"); }); }catch(IOException) { failed=true; }
            Check(failed && File.ReadAllText(Path.Combine(install,"app.txt"))=="original" && !File.Exists(Path.Combine(install,"new.txt")),"a partial copy rolls back old files and removes new ones");
            UpdateCore.ApplyPayload(files,install,Path.Combine(folder,"backup2"));Check(File.ReadAllText(Path.Combine(install,"app.txt"))=="updated" && File.ReadAllText(Path.Combine(install,"personal.txt"))=="untouched","successful installation preserves unrelated files");
        } finally { Directory.Delete(folder,true); }
        Console.WriteLine("PASS: "+checks+" update checks (stable/preview versions, downgrade prevention, trusted asset links, checksums, hostile paths, package versions and rollback).");
    }
}
