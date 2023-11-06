using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NovelBuilder
{
    public static class HtmlExtensions
    {
        public static string ToHtmlGrafs(this string s)
        {
            var grafs = s.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            grafs = grafs.Select(x => $"<p>{x}</p>").ToArray();
            var joinedGrafs = string.Join("\n", grafs);

            return joinedGrafs;
        }

        public static string GetContainerXml()
        {
            XNamespace ns = "urn:oasis:names:tc:opendocument:xmlns:container";
            var doc = new XDocument(
                    new XElement(
                        ns + "container",
                        new XAttribute("version", "1.0"),
                        new XElement(
                            ns + "rootfiles",
                            new XElement(
                                ns + "rootfile",
                                new XAttribute("full-path", "OEBPS/content.opf"),
                                new XAttribute("media-type", "application/oebps-package+xml")
                            )
                        )
                    )
                );

            doc.Declaration = new XDeclaration("1.0", null, null);

            return doc.Declaration.ToString() + "\n" + doc.ToString();
        }

        public static string GetContentOpf(Manifest manifest)
        {
            XNamespace idpfNamespace = "http://www.idpf.org/2007/opf";
            string dcNamespace = "http://purl.org/dc/elements/1.1/";

            var doc = new XDocument(
                new XElement(idpfNamespace + "package",
                    new XAttribute(XNamespace.Xmlns + "dc", dcNamespace), 
                    new XAttribute("unique-identifier", "db-id"), 
                    new XAttribute("version","3.0"),
                    new XElement(idpfNamespace + "metadata",
                        new XElement($"{{{ dcNamespace }}}title",
                            new XAttribute("id", "t1"),
                            "Book Title"
                        ),
                        new XElement($"{{{ dcNamespace }}}creator",
                            "Tom Goldthwait"
                        ),
                        new XElement($"{{{ dcNamespace }}}language",
                            "en"
                        ),
                        new XElement(idpfNamespace + "meta",
                            new XAttribute("property", "dcterms:modified"),
                            DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ")
                        ),
                        new XElement($"{{{dcNamespace}}}identifier",
                            new XAttribute("id", "db-id"),
                            Guid.NewGuid()
                        )
                    ),
                    new XElement(idpfNamespace + "manifest",
                        new XElement(idpfNamespace + "item",
                            new XAttribute("id", "toc"),
                            new XAttribute("properties", "nav"),
                            new XAttribute("href", "toc.xhtml"),
                            new XAttribute("media-type", "application/xhtml+xml")
                        ),
                        new XElement(idpfNamespace + "item",
                            new XAttribute("id", "ncx"),
                            new XAttribute("href", "toc.ncx"),
                            new XAttribute("media-type", "application/x-dtbncx+xml")
                        )
                    ),
                    new XElement(idpfNamespace + "spine",
                        new XAttribute("toc", "ncx"),
                        new XElement(idpfNamespace + "itemref",
                            new XAttribute("idref", "toc")
                        )
                    )
                )
            );

            var manifestNode = doc.Element(idpfNamespace + "package").Element(idpfNamespace + "manifest");

            foreach(var chapter in manifest.Chapters)
            {
                manifestNode.Add(
                    new XElement(idpfNamespace + "item",
                        new XAttribute("id", chapter.Id),
                        new XAttribute("href", $"{chapter.Id}.xhtml"),
                        new XAttribute("media-type", "application/xhtml+xml")
                    )
                );
            }

            var spineNode = doc.Element(idpfNamespace + "package").Element(idpfNamespace + "spine");

            foreach(var chapter in manifest.Chapters)
            {
                spineNode.Add(
                    new XElement(idpfNamespace + "itemref",
                        new XAttribute("idref", chapter.Id)
                    )
                );
            }

            doc.Declaration = new XDeclaration("1.0", "UTF-8", null);
            return doc.Declaration.ToString() + "\n" + doc.ToString();
        }

        public static string GetTocNcx(Manifest manifest)
        {
            XNamespace ncxNamespace = "http://www.daisy.org/z3986/2005/ncx/";
            var doc = new XDocument(
                new XElement(ncxNamespace + "ncx",
                    new XAttribute("version", "2005-1"),
                    new XElement(ncxNamespace + "head",
                        new XElement(ncxNamespace + "meta",
                            new XAttribute("name", "dtb:depth"),
                            new XAttribute("content", "1")
                        )
                    ),
                    new XElement(ncxNamespace + "docTitle",
                        new XElement(ncxNamespace + "text",
                            "Title"
                        )
                    ),
                    new XElement(ncxNamespace + "navMap")
                )
            );

            var navMapNode = doc.Element(ncxNamespace + "ncx").Element(ncxNamespace + "navMap");

            var i = 0;
            foreach(var chapter in manifest.Chapters)
            {
                i++;
                navMapNode.Add(
                    new XElement(ncxNamespace + "navPoint",
                        new XAttribute("id", chapter.Id),
                        new XAttribute("playOrder", i),
                        new XElement(ncxNamespace + "navLabel",
                            new XElement(ncxNamespace + "text", chapter.Title)
                        ),
                        new XElement(ncxNamespace + "content",
                            new XAttribute("src", $"{chapter.Id}.xhtml")
                        )
                    )
                );
            }

            doc.Declaration = new XDeclaration("1.0", "UTF-8", null);
            return doc.Declaration.ToString() + "\n" + doc.ToString();
        }

        public static string GetTocHtml(Manifest manifest)
        {
            var listHtml = string.Join("", manifest.Chapters.Select(x => $"<li><a href=\"{x.Id}.xhtml\">{x.Title}</a></li>"));
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"">
                <head>
                <title>toc.xhtml</title>
                </head>
                <body>
                    <nav id=""toc"" epub:type=""toc"">
                        <h1 class=""frontmatter"">Table of Contents</h1>
                        <ol class=""contents"">
                            {listHtml}
                        </ol>
                    </nav>
                </body>
                </html>";
        }

        public static string GetChapterHtml(Chapter chapter, string fileText)
        {
            var innerHtml = fileText.ToHtmlGrafs();
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"">
                <head>
                <title>{chapter.Title}</title>
                </head>

                <body>

                    <h1>{chapter.Title}</h1>

	                {innerHtml}

                </body>
                </html>";
        }
    }
}
