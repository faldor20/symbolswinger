using HtmlAgilityPack;
using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        // Load the HTML file
        HtmlDocument doc = new HtmlDocument();
        doc.Load("website/test.html");

        // Get all the CSS class names in the HTML file
        var classNames = doc.DocumentNode.Descendants()
            .Where(n => n.Attributes["class"] != null)
            .SelectMany(n => n.Attributes["class"].Value.Split(' '))
            .Distinct();

        // Find the content attribute of each CSS class in the CSS files
        foreach (var className in classNames)
        {
            Console.WriteLine ($"name:{className}",className);
            // Create a regular expression to find the CSS class
            string pattern = @"(\." + className + @")\s*{\s*content\s*:\s*(.+)\s*;}";

            // Read each CSS file and search for the pattern
            foreach (string cssFilePath in Directory.GetFiles(".", "*.css", SearchOption.AllDirectories))
            {
                Console.WriteLine ($"file:{cssFilePath}",className);

                string cssContent = File.ReadAllText(cssFilePath);
                Match match = Regex.Match(cssContent, pattern);
                if (match.Success)
                {
                    Console.WriteLine(className + ": " + match.Groups[2].Value);
                }
            }
        }
    }
}
