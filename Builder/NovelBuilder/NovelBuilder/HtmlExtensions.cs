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
        public static string ToHtmlString(this string s, string head)
        {
            var topBoiler = @$"
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"">
<!DOCTYPE html>
<head>
  <title>{head}</title>
</head>
<body>
<section epub:type=""chapter"" role=""doc-chapter"" aria-label=""{head}"">";
            var grafs = s.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            grafs = grafs.Select(x => $"<p>{x}</p>").ToArray();
            var joinedGrafs = string.Join("", grafs);

            var bottomBoiler = @"
</section>
</body>
</html>";

            return topBoiler + joinedGrafs + bottomBoiler;
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

        public static string GetContentOpf()
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
                            "isbn"
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
                        ),
                        new XElement(idpfNamespace + "item",
                            new XAttribute("id", "chapter_1"),
                            new XAttribute("href", "chapter_1.xhtml"),
                            new XAttribute("media-type", "application/xhtml+xml")
                        )
                    ),
                    new XElement(idpfNamespace + "spine",
                        new XAttribute("toc", "ncx"),
                        new XElement(idpfNamespace + "itemref",
                            new XAttribute("idref", "toc")
                        ),
                        new XElement(idpfNamespace + "itemref",
                            new XAttribute("idref", "chapter_1")
                        )
                    )
                )
            );

            doc.Declaration = new XDeclaration("1.0", "UTF-8", null);
            return doc.Declaration.ToString() + "\n" + doc.ToString();
        }

        public static string GetTocNcx()
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
                    new XElement(ncxNamespace + "navMap",
                        new XElement(ncxNamespace + "navPoint",
                            new XAttribute("id", "chapter_1"),
                            new XAttribute("playOrder", "1"),
                            new XElement(ncxNamespace + "navLabel",
                                new XElement(ncxNamespace + "text", "Chapter 1")
                            ),
                            new XElement(ncxNamespace + "content",
                                new XAttribute("src", "chapter_1.xhtml")
                            )
                        )                    
                    )
                )
            );

            doc.Declaration = new XDeclaration("1.0", "UTF-8", null);
            return doc.Declaration.ToString() + "\n" + doc.ToString();
        }

        public static string GetTocHtml()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"">
<head>
<title>toc.xhtml</title>
</head>
<body>
    <nav id=""toc"" epub:type=""toc"">
        <h1 class=""frontmatter"">Table of Contents</h1>
        <ol class=""contents"">
               <li><a href=""chapter_1.xhtml"">Chapter 1</a></li>
        </ol>
    </nav>
</body>
</html>";
        }

        public static string GetChapter1Html()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"">
<head>
<title>chapter_1.xhtml</title>
</head>

<body>

    <h1>Chapter 1</h1>

	<p>Nullam eros diam, hendrerit vel nibh in, malesuada aliquet massa. Nam blandit egestas massa, sit amet efficitur erat luctus vel. In eu leo at nibh egestas venenatis vel nec eros. Nulla bibendum sapien vel velit iaculis, in feugiat risus vestibulum. Suspendisse placerat laoreet eros, et ullamcorper est ornare eu. Praesent imperdiet lacus vel vehicula accumsan. Donec fringilla odio velit. Cras tempus est in lacus eleifend, cursus pellentesque lorem vehicula. Ut metus turpis, posuere eget imperdiet eget, bibendum in tortor. Mauris id dolor tellus. Suspendisse at nisi tellus. Duis lectus arcu, pulvinar et pretium in, tincidunt facilisis nulla. Nulla ullamcorper aliquam ullamcorper. Nunc ultricies nibh vitae urna hendrerit varius.</p>

</body>
</html>";
        }
    }
}
