using NovelBuilder;
using System.IO.Compression;
using System.Text.Json;

Console.WriteLine("Let's do this");
var manifestPath = args[0];

if (string.IsNullOrEmpty(manifestPath))
{
    throw new Exception("Need a manifest to work with");
}

var rootPath = Path.Join(manifestPath, "..");
var outputPath = Path.Join(rootPath, "output");

if (Directory.Exists(outputPath))
{
    Directory.Delete(outputPath, true);
}

Directory.CreateDirectory(outputPath);
File.WriteAllText(Path.Combine(outputPath, "mimetype"), "application/epub+zip");

var metaInfPath = Path.Join(outputPath, "META-INF");
Directory.CreateDirectory(metaInfPath);
File.WriteAllText(Path.Combine(metaInfPath, "container.xml"), HtmlExtensions.GetContainerXml());

var oebpsPath = Path.Join(outputPath, "OEBPS");
Directory.CreateDirectory(oebpsPath);

var manifestText = File.ReadAllText(manifestPath);
var manifest = JsonSerializer.Deserialize<Manifest>(manifestText);
manifest = manifest with { Chapters = manifest.Chapters.Select((x, i) => x with { Id = $"chapter_{i}" }).ToList() };

File.WriteAllText(Path.Combine(oebpsPath, "content.opf"), HtmlExtensions.GetContentOpf(manifest));
File.WriteAllText(Path.Combine(oebpsPath, "toc.ncx"), HtmlExtensions.GetTocNcx(manifest));
File.WriteAllText(Path.Combine(oebpsPath, "toc.xhtml"), HtmlExtensions.GetTocHtml(manifest));
File.WriteAllText(Path.Combine(oebpsPath, manifest.PageStyles), File.ReadAllText(Path.Combine(rootPath, manifest.PageStyles)));

File.Copy(Path.Join(rootPath, manifest.Cover), Path.Combine(oebpsPath, "cover.png"));

File.WriteAllText(Path.Combine(oebpsPath, "cover_page.xhtml"), HtmlExtensions.GetCoverPage());

List<string> chapters = new List<string>();

foreach(var chapter in manifest.Chapters)
{
    var text = File.ReadAllText(Path.Join(rootPath, chapter.File));
    chapters.Add(text);
    File.WriteAllText(Path.Combine(oebpsPath, $"{chapter.Id}.xhtml"), HtmlExtensions.GetChapterHtml(chapter, text, manifest));
}

CreateInitialArchive();
AddFoldersToArchive();

void CreateInitialArchive()
{
    using var zipStream = File.Open(Path.Join(outputPath, "output.epub"), FileMode.CreateNew);
    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false);
    archive.CreateEntryFromFile(Path.Combine(outputPath, "mimetype"), "mimetype", CompressionLevel.NoCompression);
}

void AddFoldersToArchive()
{
    using var zipStream = File.Open(Path.Join(outputPath, "output.epub"), FileMode.Open);
    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Update, false);
    foreach(var file in Directory.EnumerateFiles(metaInfPath))
    {
        archive.CreateEntryFromFile(file, $"META-INF/{Path.GetFileName(file)}");
    }

    foreach(var file in Directory.EnumerateFiles(oebpsPath))
    {
        archive.CreateEntryFromFile(file, $"OEBPS/{Path.GetFileName(file)}");
    }
}