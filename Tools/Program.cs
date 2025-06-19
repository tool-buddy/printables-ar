using System.Text.RegularExpressions;
using ToolBuddy.PrintablesAR.Application;

namespace UpdateStateDiagram;

internal class Program
{
    private const string _mermaidSectionRegex = @"(```mermaid)(.*?)(```\s*)";
    private static string _targetFile = "../README.md";


    private static void Main(
        string[] args)
    {
        try
        {
            Console.WriteLine("Updating ApplicationStateMachine diagram...");

            string newDiagram = GetUpToDateDiagram();

            string readmeFilePath = GetTargetFileContent(out string content);

            string updatedContent;
            if (Regex.IsMatch(
                    content,
                    _mermaidSectionRegex,
                    RegexOptions.Singleline
                ))
            {
                updatedContent = Regex.Replace(
                    content,
                    _mermaidSectionRegex,
                    $"$1\n{newDiagram}\n$3",
                    RegexOptions.Singleline
                );

                File.WriteAllText(
                    readmeFilePath,
                    updatedContent
                );

                Console.WriteLine($"Successfully updated {_targetFile} with state machine diagram");
            }
            else
            {
                Console.Error.WriteLine($"Error: Couldn't find the mermaid section to fill");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    private static string GetTargetFileContent(
        out string content)
    {
        if (!File.Exists(_targetFile))
        {
            Console.Error.WriteLine($"Error: File not found at {_targetFile}");
            Environment.Exit(1);
        }

        content = File.ReadAllText(_targetFile);
        return _targetFile;
    }

    private static string GetUpToDateDiagram()
    {
        ApplicationStateMachine stateMachine = new ApplicationStateMachine();
        string newDiagram = stateMachine.GetMermaidDiagram();
        return newDiagram;
    }
}