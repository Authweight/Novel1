using NovelBuilder;
using System.IO.Compression;

Console.WriteLine("Let's do this");
var basePath = args[0];

if (string.IsNullOrEmpty(basePath))
{
    throw new Exception("Need a base path to work with");
}

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
File.WriteAllText(Path.Combine(oebpsPath, "content.opf"), HtmlExtensions.GetContentOpf());
File.WriteAllText(Path.Combine(oebpsPath, "toc.ncx"), HtmlExtensions.GetTocNcx());
File.WriteAllText(Path.Combine(oebpsPath, "toc.xhtml"), HtmlExtensions.GetTocHtml());
File.WriteAllText(Path.Combine(oebpsPath, "chapter_1.xhtml"), HtmlExtensions.GetChapter1Html());

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