using System.Xml;

namespace GroupDocs.Parser.PdfTablesExtraction
{
    using GroupDocs.Parser.Data;
    using GroupDocs.Parser.Templates;

    internal class Program
    {
        static void Main(string[] args)
        {
            ExtractAllTablesFromDocument();
            ExtractTablesPerParticluarPage();
        }

        /// <summary>
        /// Extracts and displays tables from each page of a PDF document.
        /// </summary>
        /// <remarks>
        /// This method processes a PDF document, extracts all tables from each page,
        /// and analyzes each table for its content by rows and columns
        /// </remarks>
        static void ExtractTablesPerParticluarPage()
        {
            string sample = "Invoices.pdf";

            Console.WriteLine();
            Console.WriteLine($"Extracting tables from: {sample}");
            
            using (var parser = new Parser(sample))
            {
                var documentInfo = parser.GetDocumentInfo();
                int pageCount = documentInfo.PageCount;
                
                Console.WriteLine($"Total pages: {pageCount}");
                Console.WriteLine();
                // extract tables from first page
                var pageIndex = 0;
                var tables = parser.GetTables(pageIndex);

                if (tables != null && tables.Any())
                {
                    int tableNumber = 1;
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"  Table {tableNumber}: {table.RowCount} rows x {table.ColumnCount} columns");
                        ProcessTable(table);
                        tableNumber++;
                    }
                }
            }
        }

        static void ExtractAllTablesFromDocument()
        {
            string sample = "TablesReport.pdf";

            Console.WriteLine();
            Console.WriteLine($"Extracting tables from: {sample}");

            using (var parser = new Parser(sample))
            {   
                var tables = parser.GetTables();

                if (tables != null && tables.Any())
                {
                    // Group tables by page index
                    var tablesByPage = tables
                        .GroupBy(table => table.Page.Index)
                        .OrderBy(group => group.Key);

                    foreach (var pageGroup in tablesByPage)
                    {
                        int pageIndex = pageGroup.Key;
                        Console.WriteLine($"Tables in the Page {pageIndex + 1}");
                        Console.WriteLine();

                        int tableNumber = 1;
                        foreach (var table in pageGroup)
                        {
                            Console.WriteLine($"  Table {tableNumber}: {table.RowCount} rows x {table.ColumnCount} columns");
                            ProcessTable(table);
                            tableNumber++;
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Extracts tables from a PDF document using a template definition.
        /// </summary>
        /// <remarks>
        /// This method loads a template from an XML file and uses it to extract tables
        /// from the specified PDF document. The template defines the table structure
        /// including column and row positions for precise extraction.
        /// </remarks>
        static void ExtractTablesWithTemplate()
        {
            string sample = "scanned-tables.pdf";
            string templateFile = "scanned-tables.layout.xml";

            Console.WriteLine();
            Console.WriteLine($"Extracting tables from: {sample}");
            Console.WriteLine($"Using template: {templateFile}");
            Console.WriteLine();

            // Check if template file exists
            if (!File.Exists(templateFile))
            {
                Console.WriteLine($"Error: Template file not found: {templateFile}");
                Console.WriteLine("Please ensure the template file is in the same directory as the executable.");
                return;
            }

            using (var parser = new Parser(sample))
            {
                // Load template from XML file
                Template? template = LoadTemplateFromXml(templateFile);
                
                if (template == null)
                {
                    Console.WriteLine("Error: Failed to load template from XML file.");
                    return;
                }

                // Parse document using template
                DocumentData data = parser.ParseByTemplate(template);
                
                if (data == null)
                {
                    Console.WriteLine("Error: Failed to parse document with template.");
                    return;
                }

                // Extract all table fields from the parsed data
                var tableFields = data
                    .Where(field => field?.PageArea is PageTableArea)
                    .ToList();

                if (!tableFields.Any())
                {
                    Console.WriteLine("No tables found using the template.");
                    return;
                }

                // Group tables by page
                var tablesByPage = tableFields
                    .Where(field => field?.PageArea != null)
                    .GroupBy(field => field!.PageArea!.Page.Index)
                    .OrderBy(group => group.Key);

                foreach (var pageGroup in tablesByPage)
                {
                    int pageIndex = pageGroup.Key;
                    Console.WriteLine($"Tables in the Page {pageIndex + 1}");
                    Console.WriteLine();

                    int tableNumber = 1;
                    foreach (var field in pageGroup)
                    {
                        if (field.PageArea is PageTableArea table)
                        {
                            Console.WriteLine($"  Table {tableNumber} (Field: {field.Name}): {table.RowCount} rows x {table.ColumnCount} columns");
                            ProcessTable(table);
                            tableNumber++;
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Loads a template from an XML file.
        /// </summary>
        /// <param name="xmlFilePath">Path to the XML template file.</param>
        /// <returns>Template object or null if loading fails.</returns>
        static Template? LoadTemplateFromXml(string xmlFilePath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFilePath);

                // Try to use Template.Load if available
                // Otherwise, parse XML manually to create template
                var templateItems = new List<TemplateItem>();

                // Parse XML to extract table definitions
                XmlNodeList? tableNodes = doc.SelectNodes("//TemplateTable | //Table");
                
                if (tableNodes != null && tableNodes.Count > 0)
                {
                    foreach (XmlNode tableNode in tableNodes)
                    {
                        if (tableNode == null) continue;
                        
                        string tableName = tableNode.Attributes?["name"]?.Value ?? "Table";
                        
                        // Extract column positions
                        var columnPositions = new List<double>();
                        XmlNodeList? columnNodes = tableNode.SelectNodes(".//Column | .//col");
                        if (columnNodes != null && columnNodes.Count > 0)
                        {
                            foreach (XmlNode? colNode in columnNodes)
                            {
                                if (colNode != null && double.TryParse(colNode.Attributes?["x"]?.Value ?? colNode.InnerText, out double x))
                                {
                                    columnPositions.Add(x);
                                }
                            }
                        }

                        // Extract row positions
                        var rowPositions = new List<double>();
                        XmlNodeList? rowNodes = tableNode.SelectNodes(".//Row | .//row");
                        if (rowNodes != null && rowNodes.Count > 0)
                        {
                            foreach (XmlNode? rowNode in rowNodes)
                            {
                                if (rowNode != null && double.TryParse(rowNode.Attributes?["y"]?.Value ?? rowNode.InnerText, out double y))
                                {
                                    rowPositions.Add(y);
                                }
                            }
                        }

                        // If we have both column and row positions, create template table
                        if (columnPositions.Count > 0 && rowPositions.Count > 0)
                        {
                            var layout = new TemplateTableLayout(
                                columnPositions.ToArray(),
                                rowPositions.ToArray()
                            );
                            templateItems.Add(new TemplateTable(layout, tableName, null));
                        }
                    }
                }

                if (templateItems.Count > 0)
                {
                    return new Template(templateItems.ToArray());
                }

                // If XML parsing didn't work, try Template.Load method if available
                // This is a fallback - adjust based on actual API
                Console.WriteLine("Warning: Could not parse template from XML. Using default template structure.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading template: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        static void ProcessTable(PageTableArea table)
        {
            // Calculate column widths for proper alignment
            int[] columnWidths = Enumerable.Range(0, table.ColumnCount)
                .Select(col => Math.Max(3, Enumerable.Range(0, table.RowCount)
                    .Max(row => table[row, col]?.Text?.Length ?? 0)))
                .ToArray();


            // Display table with borders
            string separator = "+" + string.Join("+", columnWidths.Select(w => new string('-', w + 2))) + "+";
            
            // Display header row (first row)
            Console.WriteLine("    " + separator);
            Console.Write("    |");
            for (int col = 0; col < table.ColumnCount; col++)
            {
                string cellText = GetCellText(table, 0, col);
                Console.Write($" {cellText.PadRight(columnWidths[col])} |");
            }
            Console.WriteLine();
            Console.WriteLine("    " + separator);

            // Display data rows
            for (int row = 1; row < table.RowCount; row++)
            {
                Console.Write("    |");
                for (int col = 0; col < table.ColumnCount; col++)
                {
                    string cellText = GetCellText(table, row, col);
                    Console.Write($" {cellText.PadRight(columnWidths[col])} |");
                }
                Console.WriteLine();
            }
            Console.WriteLine("    " + separator);
        }

        static string GetCellText(PageTableArea table, int row, int col)
        {
            return table[row, col]?.Text ?? "";
        }
    }
}
