# Extract Tables from PDF Documents with C# - GroupDocs.Parser Examples

[![Product Page](https://img.shields.io/badge/Product%20Page-2865E0?style=for-the-badge&logo=appveyor&logoColor=white)](https://products.groupdocs.com/parser/net/) 
[![Docs](https://img.shields.io/badge/Docs-2865E0?style=for-the-badge&logo=Hugo&logoColor=white)](https://docs.groupdocs.com/parser/net/) 
[![Demos](https://img.shields.io/badge/Demos-2865E0?style=for-the-badge&logo=appveyor&logoColor=white)](https://products.groupdocs.app/parser/total) 
[![API](https://img.shields.io/badge/API%20Reference-2865E0?style=for-the-badge&logo=html5&logoColor=white)](https://reference.groupdocs.com/parser/net/) 
[![Blog](https://img.shields.io/badge/Blog-2865E0?style=for-the-badge&logo=WordPress&logoColor=white)](https://blog.groupdocs.com/category/parser/) 
[![Support](https://img.shields.io/badge/Free%20Support-2865E0?style=for-the-badge&logo=Discourse&logoColor=white)](https://forum.groupdocs.com/c/parser) 
[![Temp License](https://img.shields.io/badge/Temporary%20License-2865E0?style=for-the-badge&logo=rocket&logoColor=white)](https://purchase.groupdocs.com/temp-license/100308)

## 📋 Quick Navigation

- [Overview](#-overview)
- [Features](#-features)
- [Getting Started](#-getting-started)
- [Code Examples](#-code-examples)
- [Use Cases](#-use-cases)
- [Supported Formats](#-supported-formats)
- [Resources](#-resources)

---

## 📖 Overview

This repository demonstrates how to **extract tables from PDF documents** using [GroupDocs.Parser for .NET](https://products.groupdocs.com/parser/net/). Learn how to **parse document tables**, **extract table values**, and process structured data from PDF files programmatically with C#.

**Key Capabilities:**
- ✅ **Extract tables** from PDF documents automatically
- ✅ **Parse document** structures without templates
- ✅ **Extract table values** with cell-level precision
- ✅ Process tables from specific pages or entire documents
- ✅ Display extracted tables in formatted console output

> **Note:** This repository currently focuses on **table extraction without templates**. Template-based extraction examples will be added in future updates.

---

## ✨ Features

### Table Extraction Capabilities

- **Automatic Table Detection** – No templates required for basic table extraction
- **Page-Specific Extraction** – Extract tables from specific pages
- **Full Document Processing** – Extract all tables across all pages
- **Structured Output** – Formatted table display with headers and values
- **Cell-Level Access** – Access individual table cells and their content

### What You Can Extract

- Table headers and data rows
- Cell values with precise positioning
- Table dimensions (rows × columns)
- Multi-page table extraction
- Tables organized by page

---

## 🚀 Getting Started

### Prerequisites

- **.NET 6.0** or later (.NET 9.0 recommended)
- **GroupDocs.Parser for .NET** NuGet package
- Valid **GroupDocs.Parser license** (optional for evaluation)

### Installation

**Clone the repository:**
   ```bash
   git clone https://github.com/groupdocs-parser/Pdf-tables-extraction-using-groupdocs-parser.git
   cd Pdf-tables-extraction-using-groupdocs-parser
   ```

### License Setup (optional)

For production use, set your GroupDocs.Parser license:

```csharp
new License().SetLicense(@"path\to\GroupDocs.Parser.NET.lic");
```

For evaluation, you can use a [temporary license](https://purchase.groupdocs.com/temp-license/100308).

---

## 💻 Code Examples

### Example 1: Extract Tables from a Specific Page

This example demonstrates how to **extract tables from a particular page** of a PDF document. The method analyzes the document structure and extracts all tables found on the specified page.

**Source Document:**
![PDF Document Page](document-page-01.png)

**Code:**

```csharp
static void ExtractTablesPerParticluarPage()
{
    string sample = "Invoices.pdf";
    
    // Initialize parser with PDF document
    using (var parser = new Parser(sample))
    {
        // Get document information
        var documentInfo = parser.GetDocumentInfo();
        int pageCount = documentInfo.PageCount;
        
        // Extract tables from first page (pageIndex = 0)
        var pageIndex = 0;
        var tables = parser.GetTables(pageIndex);

        if (tables != null && tables.Any())
        {
            int tableNumber = 1;
            foreach (var table in tables)
            {
                // Process each table
                // Display table dimensions and content
                ProcessTable(table);
                tableNumber++;
            }
        }
    }
}
```

**Console Output:**
![Console Output](console-output-01.png)

### Example 2: Extract All Tables from Document

Extract all tables from all pages of a PDF document, organized by page:

```csharp
static void ExtractAllTablesFromDocument()
{
    string sample = "TablesReport.pdf";

    using (var parser = new Parser(sample))
    {   
        // Get all tables from entire document
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
                
                int tableNumber = 1;
                foreach (var table in pageGroup)
                {
                    Console.WriteLine($"  Table {tableNumber}: {table.RowCount} rows x {table.ColumnCount} columns");
                    ProcessTable(table);
                    tableNumber++;
                }
            }
        }
    }
}
```

### Example 3: Access Individual Table Cells

Access and process individual cells from extracted tables:

```csharp
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
```

---

## 🎯 Use Cases

### Business Document Processing

- **Invoice Processing** – Extract line items, totals, and payment information
- **Financial Reports** – Parse balance sheets, income statements, and financial tables
- **Purchase Orders** – Extract product details, quantities, and pricing
- **Receipts** – Extract itemized lists and transaction details

### Data Migration & Integration

- **Database Import** – Convert PDF tables to database records
- **Excel Conversion** – Extract tables for spreadsheet processing
- **API Integration** – Parse document tables for REST API consumption
- **ETL Pipelines** – Extract, transform, and load table data

### Document Analysis

- **Report Analysis** – Extract structured data from business reports
- **Compliance Documents** – Parse regulatory tables and forms
- **Research Data** – Extract tables from research papers and publications
- **Legal Documents** – Parse tables from contracts and legal filings

### Automation & Workflows

- **Automated Data Entry** – Reduce manual data entry from PDFs
- **Batch Processing** – Process multiple PDF documents automatically
- **Content Indexing** – Extract tables for search engine indexing
- **Data Validation** – Verify table data against business rules

---

## 📄 Supported Formats

### Document Formats

| Format | Extension | Table Extraction |
|--------|-----------|-----------------|
| **PDF** | `.pdf` | ✅ Supported |
| **Microsoft Word** | `.doc`, `.docx` | ✅ Supported |
| **Microsoft Excel** | `.xls`, `.xlsx` | ✅ Supported |
| **Microsoft PowerPoint** | `.ppt`, `.pptx` | ✅ Supported |
| **OpenDocument** | `.odt`, `.ods`, `.odp` | ✅ Supported |

> **Note:** This repository focuses on PDF table extraction. Other formats are supported by GroupDocs.Parser but not demonstrated in these examples.

---

## 🔧 Project Structure

```
Pdf-tables-extraction-using-groupdocs-parser/
│
├── Program.cs                 # Main code examples
├── README.md                  # This file
├── LICENSE                    # License file
│
├── Invoices.pdf              # Sample PDF document
├── TablesReport.pdf          # Sample PDF with tables
├── Operations.pdf            # Sample PDF document
│
├── document-page-01.png      # Document preview image
├── console-output-01.png     # Console output example
│
└── bin/                      # Build output directory
```

---

## 📚 Resources

### Documentation & Learning

- 📖 [GroupDocs.Parser for .NET Documentation](https://docs.groupdocs.com/parser/net/)
- 📚 [API Reference](https://reference.groupdocs.com/parser/net/)
- 🎓 [Working with Tables Guide](https://docs.groupdocs.com/parser/net/working-with-tables/)
- 💡 [Code Samples](https://github.com/groupdocs-parser/GroupDocs.Parser-for-.NET)

### Support & Community

- 💬 [Free Support Forum](https://forum.groupdocs.com/c/parser/)
- 🆘 [Paid Support Helpdesk](https://helpdesk.groupdocs.com/)
- 📝 [Blog Articles](https://blog.groupdocs.com/category/parser/)
- 🎬 [Video Tutorials](https://www.youtube.com/c/groupdocs)

### Product Information

- 🌐 [Product Page](https://products.groupdocs.com/parser/net/)
- 🎮 [Live Demos](https://products.groupdocs.app/parser/total)
- 🔑 [Get Temporary License](https://purchase.groupdocs.com/temp-license/100308)
- 💰 [Pricing Information](https://purchase.groupdocs.com/pricing/parser/net)

---

## 🤝 Contributing

Contributions are welcome! If you'd like to contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Contribution Ideas

- Add more table extraction examples
- Improve error handling
- Add template-based extraction examples
- Enhance documentation
- Add unit tests

---

## 🔮 Roadmap

### Current Features ✅
- Extract tables from PDF documents
- Page-specific table extraction
- Full document table extraction
- Formatted table display

### Coming Soon 🚀
- Template-based table extraction examples
- OCR support for scanned PDFs
- Batch processing multiple documents
- Export to CSV/Excel formats
- Advanced table formatting options

---

## 📊 Keywords & SEO

**Primary Keywords:**
- extract table from PDF
- parse document tables
- extracting table values
- PDF table extraction C#
- GroupDocs.Parser examples
- table parsing .NET
- extract tables from PDF documents
- parse PDF tables programmatically
- C# PDF table extraction
- document table parser

**Related Terms:**
- PDF parser, table extraction, document parsing, data extraction, PDF processing, C# PDF library, .NET PDF parser, table data extraction, structured data extraction, PDF table reader

---

## ⭐ Star History

If you find this repository helpful, please consider giving it a star! ⭐

---

**Made with ❤️ by [GroupDocs](https://www.groupdocs.com/)**
