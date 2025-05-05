using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Xml;
using iTextSharp.text.pdf;

class Program
{
    static void Main(string[] args)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Go up two levels: from bin/Debug/netX.X/ to project root
        string projectRoot = Directory.GetParent(baseDir)!.Parent!.Parent!.Parent!.FullName;

        string inputPdfPath = Path.Combine(projectRoot, "docs", "test.pdf");
        string outputPdfPath = Path.Combine(projectRoot, "docs", "modified_test.pdf");

        // Load the PDF
        PdfReader reader = new PdfReader(inputPdfPath);

        // Access the XFA XML DOM
        XmlDocument xfaDom = reader.AcroFields.Xfa.DomDocument;

        // Read data from XFA
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(xfaDom.NameTable);
        nsMgr.AddNamespace("xfa", "http://www.xfa.org/schema/xfa-data/1.0/");

        XmlNode? dataNode = xfaDom.SelectSingleNode("//xfa:data", nsMgr);
        if (dataNode != null)
        {
            Console.WriteLine("Original XFA Data:");
            Console.WriteLine(dataNode.InnerXml);

            UpdateField(xfaDom, nsMgr, "//form1/grantApplication/page1/ProjectTitle", "AI Research Project");
            UpdateField(xfaDom, nsMgr, "//form1/grantApplication/page1/PIName", "Dr. Rafey Husain");
            UpdateField(xfaDom, nsMgr, "//form1/grantApplication/page1/Email", "rafey@example.com");
            UpdateField(xfaDom, nsMgr, "//form1/grantApplication/page1/BudgetTotalCosts", "150000");
            UpdateField(xfaDom, nsMgr, "//form1/grantApplication/page2/Description", "This project aims to explore AI ethics and practical applications.");
        }
        else
        {
            Console.WriteLine("XFA data node not found.");
        }

        // Write updated XFA back into PDF
        using (FileStream output = new FileStream(outputPdfPath, FileMode.Create))
        {
            PdfStamper stamper = new PdfStamper(reader, output);

            XmlNode? datasetsNode = xfaDom.SelectSingleNode("//xfa:datasets", nsMgr);
            if (datasetsNode != null)
            {
                stamper.AcroFields.Xfa.FillXfaForm(datasetsNode);
            }
            else
            {
                Console.WriteLine("datasets node not found.");
            }

            stamper.Close();
        }

        reader.Close();
        Console.WriteLine("PDF updated and saved to " + outputPdfPath);
    }

    // Update form fields
    static void UpdateField(XmlDocument xmlDoc, XmlNamespaceManager nsMgr, string xpath, string value)
    {
        XmlNode? node = xmlDoc.SelectSingleNode(xpath, nsMgr);
        if (node != null)
        {
            node.InnerText = value;
        }
        else
        {
            Console.WriteLine($"Field not found: {xpath}");
        }
    }

}
