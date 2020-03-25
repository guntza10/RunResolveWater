using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Test
{
    class Lamps
    {
        public Dictionary<string, bool> CreateLamps()
        {
            var dic = new Dictionary<string, bool>();
            for (int i = 0; i < 10; i++)
            {
                dic.Add($"{i + 1}", false);
            }
            return dic;
        }

        public String ShowLamps(Dictionary<string, bool> dic)
        {
            var strBd = new StringBuilder();
            strBd.Clear();
            foreach (var item in dic)
            {
                strBd.Append("[");
                var isOn = item.Value == true ? "_" : " ";
                strBd.Append(isOn).Append("]").Append(" ");
            }
            return strBd.ToString();
        }

        public void GeneratePDF()
        {
            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 10, 10, 10, 10);

                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                Chunk chunk = new Chunk("This is from chunk. ");
                document.Add(chunk);

                Phrase phrase = new Phrase("This is from Phrase.");
                document.Add(phrase);

                Paragraph para = new Paragraph("This is from paragraph.");
                document.Add(para);

                string text = @"you are successfully created PDF file.";
                Paragraph paragraph = new Paragraph();
                paragraph.SpacingBefore = 10;
                paragraph.SpacingAfter = 10;
                paragraph.Alignment = Element.ALIGN_LEFT;
                paragraph.Font = FontFactory.GetFont(FontFactory.HELVETICA, 12f, BaseColor.GREEN);
                paragraph.Add(text);
                document.Add(paragraph);

                document.Close();
                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();
            }
        }
    }
}