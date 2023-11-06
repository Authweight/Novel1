using NovelBuilder;
using System.IO.Compression;
using System.Text.Json;

Console.WriteLine("Let's do this");
var manifestPath = args[0];

if (string.IsNullOrEmpty(manifestPath))
{
    throw new Exception("Need a manifest to work with");
}

var basePath = Path.Join(manifestPath, "../output");

if (Directory.Exists(basePath))
{
    Directory.Delete(basePath, true);
}

Directory.CreateDirectory(basePath);
File.WriteAllText(Path.Combine(basePath, "mimetype"), "application/epub+zip");

var metaInfPath = Path.Join(basePath, "META-INF");
Directory.CreateDirectory(metaInfPath);
File.WriteAllText(Path.Combine(metaInfPath, "container.xml"), HtmlExtensions.GetContainerXml());

var oebpsPath = Path.Join(basePath, "OEBPS");
Directory.CreateDirectory(oebpsPath);

var manifestText = File.ReadAllText(manifestPath);
var manifest = JsonSerializer.Deserialize<Manifest>(manifestText);
manifest = manifest with { Chapters = manifest.Chapters.Select((x, i) => x with { Id = $"chapter_{i}" }).ToList() };

File.WriteAllText(Path.Combine(oebpsPath, "content.opf"), HtmlExtensions.GetContentOpf(manifest));
File.WriteAllText(Path.Combine(oebpsPath, "toc.ncx"), HtmlExtensions.GetTocNcx(manifest));
File.WriteAllText(Path.Combine(oebpsPath, "toc.xhtml"), HtmlExtensions.GetTocHtml(manifest));

foreach(var chapter in manifest.Chapters)
{
    var text = File.ReadAllText(Path.Join(manifestPath, "..", chapter.File));
    File.WriteAllText(Path.Combine(oebpsPath, $"{chapter.Id}.xhtml"), HtmlExtensions.GetChapterHtml(chapter, text));
}

CreateInitialArchive();
AddFoldersToArchive();

void CreateInitialArchive()
{
    using var zipStream = File.Open(Path.Join(basePath, "output.epub"), FileMode.CreateNew);
    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false);
    archive.CreateEntryFromFile(Path.Combine(basePath, "mimetype"), "mimetype", CompressionLevel.NoCompression);
}

void AddFoldersToArchive()
{
    using var zipStream = File.Open(Path.Join(basePath, "output.epub"), FileMode.Open);
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