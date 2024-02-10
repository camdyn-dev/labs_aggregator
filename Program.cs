/* We have two installed dependencies for this project;
 * 1. PdfPig - Used for text processing within PDFs
 * 2. CSVHelper - Used to take Results and aggregate them into a CSV file
 */
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig;
using labs_aggregator;
using CsvHelper;
using System.Globalization;

var repo = new LabsHolder();
var program = new LabsReader(repo);
Console.WriteLine("Enter a path to read from...");
var path = Console.ReadLine();
Console.WriteLine("Enter where you want to save the results...");
var pathToSave = Console.ReadLine();
program.Run(path);
PDFReaderAndCSVWriter.WriteResultsToCSV(repo.Results, pathToSave);

class LabsReader
{
    public LabsReader(LabsHolder repo)
    {
        Repo = repo;
    }

    public LabsHolder Repo { get; set; }
    

    public void Run(string path)
    { // Basic run function; let's us decide whether or not we'll be reading one file or an entire directory
        Console.WriteLine("Is this path a directory? Y/N");
        bool isDirectory = Console.ReadLine().Contains("Y") ? true : false;

        if (isDirectory)
        {
            foreach(var labFilePath in PDFReaderAndCSVWriter.OpenDirectoryOfPDFs(path))
            {
                Repo.SaveLabResult(ReadLab(labFilePath));
            }
        }
        else
        {
            Repo.SaveLabResult(ReadLab(path));
        }
        Repo.Results.Sort();

    }

    LabResult ReadLab(string labfilePath)
    { // this will be the heavy lifting function, basically reading from, organizing, and then saving our lab results into a LabResult object
        var labFileToList = PDFReaderAndCSVWriter.GetPDFTextByLine(labfilePath);

        int i = 0; // basic iteration counter
        bool isDateOfService = false; // Important boolean; check LabResult.cs for more information. In essence, these documents have a different structure
        var labResult = new LabResult();

        foreach (var line in labFileToList)
        { // for each line of text in this list of strings
            foreach (var labName in LabsHolder.LabNames)
            { // for each labName we're trying to find in this document

                // Since files will always start with "Collected:" or "Date of Service:"
                if (line.Contains("Date of Service:"))
                {
                    isDateOfService = true;
                    // Take the first index of "Date of Service:", add the length of "Date of Service:" to it, and take the next 11 characters.
                    // This correlates to "MM/DD/YYYY"
                    var collectedLabResult = line.Substring(line.IndexOf("Date of Service:") + "Date of Service:".Length,
                        11);
                    labResult.StoreResult("Date of Service:", collectedLabResult);

                    break; // this will save us iterations
                }
                else if (line.Contains("Collected:"))
                {
                    // Take the first index of "Collected:", add the length of "Collected:" to it, and take the next 17 characters.
                    // This correlates to "MM/DD/YYYY TT/TT"
                    var collectedLabResult = line.Substring(line.IndexOf("Collected:") + "Collected:".Length,
                        17);
                    labResult.StoreResult("Collected:", collectedLabResult);
                    break; // this will save us iterations
                }
                else
                {
                    if(line.StartsWith(labName) 
                        && line.Substring(labName.Length).Any(char.IsDigit)
                        && !line.StartsWith("ALBUMIN/GLOBULIN"))
                    // If this line starts with a lab name AND any proceeding character is a digit AND doesn't start with specificed lab name
                    {
                        string labValue;
                        /* This is where the whole "Date of Service:" thing comes into play.
                         * For some odd reason, on these types of documents the lab result actually preceeds the lab name.
                         * Because of this, we just take the previous index, and that's the number we roll with.
                         */
                        if (isDateOfService)
                        {
                            labValue = labFileToList.ElementAt(i - 1);
                        }
                        // Otherwise, if it's a normal document, we just take everything after the lab name (which is the result
                        else
                        {
                            labValue = line.Substring(labName.Length);
                        }

                        // Removes some values we don't want to see, namely H, L and whitespace characters.
                        labValue = string.Concat(labValue.Where(c => !char.IsWhiteSpace(c) && c != 'H' && c != 'L'));
                        // Finally, store the lab result
                        labResult.StoreResult(labName, labValue);
                        break;
                    }
                }

            }
            i++; // increase our iteration counter
        }
        return labResult; // return our filled out LabResult
    }
}
class LabsHolder
{ // this will basically handle the lab files by extracting the info we want from each file
    public LabsHolder()
    {
        Results = new List<LabResult>();
    }

    public List<LabResult> Results { get; set; }
    public static string[] LabNames { get; } = [
    "ALBUMIN",
    "BILIRUBIN, TOTAL",
    "BILIRUBIN, DIRECT",
    "ALKALINE PHOSPHATASE",
    "AST",
    "ALT",
    "GGT",
    "INR",
    "SED RATE BY MODIFIED WESTERGREN",
    "WHITE BLOOD CELL COUNT",
    "RED BLOOD CELL COUNT",
    "PLATELET COUNT",
    "C-REACTIVE PROTEIN",
    "HS CRP",
    ];

    public void SaveLabResult(LabResult labResult)
    {
        Results.Add(labResult);
    }
}

public static class PDFReaderAndCSVWriter
{ // We'll use this to read data out of the PDFs, then to write it back into a CSV file
    public static IEnumerable<string> OpenDirectoryOfPDFs(string path)
    { // Function to open a number of PDFs
        var fileNames = Directory.GetFiles(path, "*.pdf");
        return fileNames;
    }

    public static IEnumerable<string> GetPDFTextByLine(string path)
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

    public static void WriteResultsToCSV(IEnumerable<LabResult> resultsToWrite, string outputPath)
    { // Function to output results into CSV file; basically a copy of the example code from CsvHelper's website
        using (var writer = new StreamWriter(outputPath))
        using (var outputCsv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        { 
            outputCsv.WriteRecords(resultsToWrite); 
        }
    }
}