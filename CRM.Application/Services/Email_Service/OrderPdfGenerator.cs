using CRM.Application.Services.Order_Service;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CRM.Application.Services.Email_Service
{
    public static class OrderPdfGenerator
    {
        private const int MaxLineLength = 92;
        private const int LinesPerPage = 48;

        public static byte[] GenerateOrderRequestPdf(OrderViewModel order)
        {
            var lines = BuildDocumentLines(order);
            var pages = lines
                .Chunk(LinesPerPage)
                .Select(chunk => chunk.ToList())
                .ToList();

            if (pages.Count == 0)
                pages.Add(new List<string> { "Order information is unavailable." });

            return BuildPdfDocument(pages);
        }

        private static List<string> BuildDocumentLines(OrderViewModel order)
        {
            var lines = new List<string>
            {
                "AGORA FOOD",
                "STOCK FULFILLMENT REQUEST",
                new string('=', MaxLineLength),
                $"Order Reference : #{order.OrderNumber ?? order.Id.ToString(CultureInfo.InvariantCulture)}",
                $"Request Date    : {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC",
                $"Order Date      : {order.OrderDate:dd MMM yyyy HH:mm}",
                $"Status          : {SafeText(order.Status)}",
                string.Empty,
                "CUSTOMER DETAILS",
                new string('-', MaxLineLength),
                $"Customer Name   : {SafeText($"{order.FirstName} {order.LastName}")}",
                $"Phone           : {SafeText(order.Phone)}",
                $"Address         : {SafeText(order.Address)}",
                $"City            : {SafeText(order.City)}",
                $"Zip Code        : {SafeText(order.ZipCode)}",
                $"Country         : {SafeText(order.Country)}",
                string.Empty,
                "ITEM DETAILS",
                new string('-', MaxLineLength),
                FormatTableHeader(),
                new string('-', MaxLineLength)
            };

            foreach (var item in order.Items ?? new List<OrderItemViewModel>())
            {
                lines.Add(FormatTableRow(
                    SafeText(item.Name),
                    item.Quantity.ToString(CultureInfo.InvariantCulture),
                    FormatMoney(item.UnitPrice),
                    FormatMoney(item.UnitPrice * item.Quantity)));
            }

            lines.Add(new string('-', MaxLineLength));
            lines.Add($"Sub Total       : ${FormatMoney(order.SubTotal)}");
            lines.Add($"Shipping Fee    : ${FormatMoney(order.ShippingFee)}");
            lines.Add($"Tax             : ${FormatMoney(order.Tax)}");
            lines.Add($"Grand Total     : ${FormatMoney(order.TotalAmount)}");

            var customerQuery = StripHtml(order.CustomerQuery);
            if (!string.IsNullOrWhiteSpace(customerQuery))
            {
                lines.Add(string.Empty);
                lines.Add("SPECIAL INSTRUCTIONS / CUSTOMER QUERY");
                lines.Add(new string('-', MaxLineLength));

                foreach (var wrappedLine in WrapText(customerQuery, MaxLineLength))
                    lines.Add(wrappedLine);
            }

            lines.Add(string.Empty);
            lines.Add(new string('=', MaxLineLength));
            lines.Add("Authorized Dispatch");
            lines.Add("Mir Mohammad Faruk");
            lines.Add("Founder & CEO");

            return lines;
        }

        private static byte[] BuildPdfDocument(List<List<string>> pages)
        {
            var objectMap = new SortedDictionary<int, byte[]>();
            const int catalogId = 1;
            const int pagesId = 2;
            const int fontId = 3;

            objectMap[fontId] = EncodePdfObject("<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>");

            var pageIds = new List<int>();
            var nextObjectId = 4;

            for (var index = 0; index < pages.Count; index++)
            {
                var pageId = nextObjectId++;
                var contentId = nextObjectId++;

                pageIds.Add(pageId);

                var contentBytes = Encoding.ASCII.GetBytes(BuildPageContent(pages[index], index + 1, pages.Count));
                objectMap[contentId] = EncodePdfStream(contentBytes);
                objectMap[pageId] = EncodePdfObject(
                    $"<< /Type /Page /Parent {pagesId} 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {fontId} 0 R >> >> /Contents {contentId} 0 R >>");
            }

            var kids = string.Join(" ", pageIds.Select(id => $"{id} 0 R"));
            objectMap[pagesId] = EncodePdfObject($"<< /Type /Pages /Count {pageIds.Count} /Kids [ {kids} ] >>");
            objectMap[catalogId] = EncodePdfObject($"<< /Type /Catalog /Pages {pagesId} 0 R >>");

            return AssemblePdf(objectMap, catalogId);
        }

        private static string BuildPageContent(IReadOnlyList<string> lines, int pageNumber, int totalPages)
        {
            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 10 Tf");
            content.AppendLine("14 TL");
            content.AppendLine("40 800 Td");

            foreach (var line in lines)
            {
                content.Append('(').Append(EscapePdfText(line)).AppendLine(") Tj");
                content.AppendLine("T*");
            }

            content.AppendLine("ET");
            content.AppendLine("BT");
            content.AppendLine("/F1 10 Tf");
            content.AppendLine("250 25 Td");
            content.Append('(').Append(EscapePdfText($"Page {pageNumber} of {totalPages}")).AppendLine(") Tj");
            content.AppendLine("ET");

            return content.ToString();
        }

        private static byte[] AssemblePdf(SortedDictionary<int, byte[]> objects, int rootObjectId)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

            writer.Write(Encoding.ASCII.GetBytes("%PDF-1.4\n"));

            var offsets = new List<long> { 0 };

            foreach (var entry in objects)
            {
                offsets.Add(stream.Position);
                writer.Write(Encoding.ASCII.GetBytes($"{entry.Key} 0 obj\n"));
                writer.Write(entry.Value);
                writer.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
            }

            var xrefStart = stream.Position;
            writer.Write(Encoding.ASCII.GetBytes($"xref\n0 {objects.Count + 1}\n"));
            writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));

            for (var index = 1; index < offsets.Count; index++)
                writer.Write(Encoding.ASCII.GetBytes($"{offsets[index]:D10} 00000 n \n"));

            writer.Write(Encoding.ASCII.GetBytes(
                $"trailer\n<< /Size {objects.Count + 1} /Root {rootObjectId} 0 R >>\nstartxref\n{xrefStart}\n%%EOF"));

            writer.Flush();
            return stream.ToArray();
        }

        private static byte[] EncodePdfObject(string content) =>
            Encoding.ASCII.GetBytes(content.Trim());

        private static byte[] EncodePdfStream(byte[] content)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);

            writer.Write(Encoding.ASCII.GetBytes($"<< /Length {content.Length} >>\nstream\n"));
            writer.Write(content);
            writer.Write(Encoding.ASCII.GetBytes("endstream"));
            writer.Flush();

            return stream.ToArray();
        }

        private static string FormatTableHeader() =>
            $"{PadRight("Product", 50)} {PadLeft("Qty", 6)} {PadLeft("Unit", 14)} {PadLeft("Total", 14)}";

        private static string FormatTableRow(string product, string quantity, string unitPrice, string total) =>
            $"{PadRight(product, 50)} {PadLeft(quantity, 6)} {PadLeft($"${unitPrice}", 14)} {PadLeft($"${total}", 14)}";

        private static string PadRight(string value, int width)
        {
            var safe = SafeText(value);
            return safe.Length >= width ? safe[..width] : safe.PadRight(width);
        }

        private static string PadLeft(string value, int width)
        {
            var safe = SafeText(value);
            return safe.Length >= width ? safe[^width..] : safe.PadLeft(width);
        }

        private static string FormatMoney(decimal value) =>
            value.ToString("0.00", CultureInfo.InvariantCulture);

        private static IEnumerable<string> WrapText(string text, int width)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            var normalized = Regex.Replace(SafeText(text), @"\s+", " ").Trim();
            while (normalized.Length > width)
            {
                var splitIndex = normalized.LastIndexOf(' ', width);
                if (splitIndex <= 0)
                    splitIndex = width;

                yield return normalized[..splitIndex].TrimEnd();
                normalized = normalized[splitIndex..].TrimStart();
            }

            if (normalized.Length > 0)
                yield return normalized;
        }

        private static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var withBreaks = Regex.Replace(html, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
            withBreaks = Regex.Replace(withBreaks, "</p\\s*>", "\n", RegexOptions.IgnoreCase);
            withBreaks = Regex.Replace(withBreaks, "</li\\s*>", "\n", RegexOptions.IgnoreCase);

            var noTags = Regex.Replace(withBreaks, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(noTags).Trim();
        }

        private static string SafeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                builder.Append(character switch
                {
                    >= ' ' and <= '~' => character,
                    '\n' => ' ',
                    '\r' => ' ',
                    '\t' => ' ',
                    _ => '?'
                });
            }

            return builder.ToString();
        }

        private static string EscapePdfText(string value) =>
            SafeText(value)
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal);
    }
}
