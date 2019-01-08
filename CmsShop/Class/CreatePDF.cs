using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Fonts;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;

namespace CmsShop.Class
{
    public class CreatePDF
    {
        public Document CreateDocument()
        {
            double suma = 0;
            List<string> nazwy = new List<string>();
            List<int> ilosc = new List<int>();
            List<double> cena = new List<double>();
            int dana_nr_zamowienia;


            ////////////////////////////////////////////////////////////////////////////////////////
            dana_nr_zamowienia = 7866;

            nazwy.Add("Telewizor");
            nazwy.Add("Komputer");
            nazwy.Add("Coś");
            nazwy.Add("Coś2");


            ilosc.Add(1);
            ilosc.Add(1);
            ilosc.Add(2);
            ilosc.Add(6);


            cena.Add(666.66);
            cena.Add(700);
            cena.Add(30);
            cena.Add(4.07);
            /////////////////////////////////////////////////////////////////////////////////////////


            Document document = new Document();

            Section section = document.AddSection();

            Paragraph p_naglowek = section.AddParagraph();
            p_naglowek.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            p_naglowek.Format.Font.Size = 35;
            p_naglowek.AddFormattedText("Zamówienie nr " + dana_nr_zamowienia.ToString(), TextFormat.Bold);


            Paragraph p_podziekowanie = section.AddParagraph();
            p_podziekowanie.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            p_podziekowanie.Format.SpaceBefore =Unit.FromPoint(10);
            p_podziekowanie.Format.SpaceAfter = Unit.FromPoint(20);
            p_podziekowanie.AddFormattedText("Dziękujemy za wybranie produktów z naszego sklepu", TextFormat.Italic);


            Paragraph p_produkty = section.AddParagraph();
            p_produkty.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            p_produkty.AddFormattedText("Dane produktów", TextFormat.NotBold);


            Table table = new Table();
            table.Borders.Width = 0.75;
            table.Format.Alignment = ParagraphAlignment.Center;

            Column column = table.AddColumn(Unit.FromCentimeter(10));

            table.AddColumn(Unit.FromCentimeter(2));

            table.AddColumn(Unit.FromCentimeter(4));

            Row row = table.AddRow();
            row.Shading.Color = Colors.PaleGoldenrod;
            Cell cell = row.Cells[0];
            cell.AddParagraph("Nazwa produktu");
            cell = row.Cells[1];
            cell.AddParagraph("Ilość");
            cell = row.Cells[2];
            cell.AddParagraph("Cena");


            document.LastSection.Add(table);

            for (int i = 0; i < nazwy.Count; i++)
            {
                row = table.AddRow();
                cell = row.Cells[0];
                cell.AddParagraph(nazwy[i]);
                cell = row.Cells[1];
                cell.AddParagraph(ilosc[i].ToString());
                cell = row.Cells[2];
                cell.AddParagraph(cena[i].ToString() + " zł");

                suma = suma + cena[i] * ilosc[i];
            }

            Paragraph p_suma = section.AddParagraph();
            p_suma.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            p_suma.Format.SpaceBefore = Unit.FromPoint(10);
            p_suma.Format.SpaceAfter = Unit.FromPoint(20);
            p_suma.Format.Alignment = ParagraphAlignment.Right;
            p_suma.AddFormattedText("Do zapłaty: " + suma.ToString(), TextFormat.NotBold);

            return document;
        }
    }
}