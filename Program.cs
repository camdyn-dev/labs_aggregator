/* We have two installed dependencies for this project;
 * 1. PdfPig - Used for text processing within PDFs
 * 2. CSVHelper - Used to take Results and aggregate them into a CSV file
 */
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig;
using labs_aggregator;
using CsvHelper;
using System.Globalization;




class PDFReaderAndCSVWriter
{ // We'll use this to read data out of the PDFs, then to write it back into a CSV file

    IEnumerable<string> GetPDFTextByLine(string path)
    { // Function to read a PDF line by line, then return it in a list
        var pageContentsToList = new List<string>();
        using (var document = PdfDocument.Open(path))
        {
            foreach (var page in document.GetPages())
            {
                var words = page.GetWords(); // grab words

                var recursiveXYCut = new RecursiveXYCut(new RecursiveXYCut.RecursiveXYCutOptions()
                {
                    MinimumWidth = page.Width
                }); // basically cuts the page into lines with the width of the page
                var blockLines = recursiveXYCut.GetBlocks(words)[0].TextLines; // only one block per page

                IEnumerable<string> textOfEachLine = blockLines.Select(line => line.Text);
                pageContentsToList.AddRange(textOfEachLine);

            }
        }
        return pageContentsToList;
    }

    void WriteResultsToCSV(IEnumerable<Result> resultsToWrite, string outputPath)
    { // Function to output results into CSV file; basically a copy of the example code from CsvHelper's website
        var writer = new StreamWriter(outputPath);
        var outputCsv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        outputCsv.WriteRecords(resultsToWrite);
    }
}