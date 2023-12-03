// See https://aka.ms/new-console-template for more information
using System.IO.Compression;
using System.Text.Json;

class Program
{
    static void Main(FileInfo? file = null)
    {
        if (file == null)
        {
            Console.WriteLine("Please specify a file name.");
            return;
        }

        if (!file.Exists)
        {
            Console.WriteLine("File does not exist.");
            return;
        }

        Console.WriteLine("Validating file: " + file.Name);

        if (!IsZipValid(file.FullName, out var zipFile))
        {
            Console.WriteLine("Not a zip file.");
            return;
        }

        var configEntry = zipFile.GetEntry("Config.json");

        if (configEntry == null)
        {
            Console.WriteLine("Config.json not found.");
            return;
        }

        if (TryParseConfig(configEntry, out var config))
        {
            Console.WriteLine("Parsed Config.json successfully.");
        }
        else
        {
            Console.WriteLine("Failed to parse Config.json.");
            return;
        }

        if (!ValidateValidatorType(config!, zipFile))
        {
            Console.WriteLine("Failed to validate ValidatorType.");
            return;
        }

        var inputPathPrefix = "Input";
        var outputPathPrefix = "Output";

        var inputEntries = zipFile.Entries.Where(e => e.FullName.StartsWith(inputPathPrefix)).ToArray();
        var outputEntries = zipFile.Entries.Where(e => e.FullName.StartsWith(outputPathPrefix)).ToArray();

        if (inputEntries.Length == 0)
        {
            Console.WriteLine("No input files found in Input folder.");
            return;
        }

        if (outputEntries.Length == 0)
        {
            Console.WriteLine("No output files found in Output folder.");
            return;
        }

        if (inputEntries.Length != outputEntries.Length)
        {
            Console.WriteLine("Not the same number of Input/Output files. {0} != {1}", inputEntries.Length, outputEntries.Length);
            return;
        }

        if (config!.MemoryLimit <= 0)
        {
            Console.WriteLine("MemoryLimit must be greater than 0.");
            return;
        }

        if (config.TimeLimit <= 0)
        {
            Console.WriteLine("TimeLimit must be greater than 0.");
            return;
        }

        if (config.TestCases.Length == 0)
        {
            Console.WriteLine("No test cases found.");
            return;
        }

        int totalScore = 0;

        foreach (var testcase in config.TestCases)
        {
            var inputEntry = inputEntries.FirstOrDefault(e => e.FullName == inputPathPrefix + "/" + testcase.Input);
            var outputEntry = outputEntries.FirstOrDefault(e => e.FullName == outputPathPrefix + "/" + testcase.Output);

            if (inputEntry == null)
            {
                Console.WriteLine("Input file not found. Input file is {0}", testcase.Input);
                return;
            }

            if (outputEntry == null)
            {
                Console.WriteLine("Output file not found. Output file is {0}", testcase.Output);
                return;
            }

            if (testcase.Score < 0)
            {
                Console.WriteLine("Score must be greater than or equal to 0.");
                return;
            }

            totalScore += testcase.Score;
        }

        if (totalScore != 100)
        {
            Console.WriteLine("Total score must be 100. Total score is {0}", totalScore);
            return;
        }
    }

    private static bool ValidateValidatorType(TestCaseConfig config, ZipArchive zipFile)
    {
        if (config.ValidatorType == ValidatorType.CustomValidator)
        {
            if (string.IsNullOrWhiteSpace(config.ValidatorSourceCode))
            {
                Console.WriteLine("ValidatorType is CustomValidator but ValidatorSourceCode is null or whitespace.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.ValidatorLanguage))
            {
                Console.WriteLine("ValidatorType is CustomValidator but ValidatorLanguage is null or whitespace.");
                return false;
            }

            var validatorSourceCodeEntry = zipFile.GetEntry(config.ValidatorSourceCode!);

            if (validatorSourceCodeEntry == null)
            {
                Console.WriteLine("ValidatorType is CustomValidator but ValidatorSourceCode file not found. ValidatorSourceCode file is {0}", config.ValidatorSourceCode);
                return false;
            }
        }

        if (config.ValidatorType == ValidatorType.FloatValidator)
        {
            if (!config.Epsilon.HasValue)
            {
                Console.WriteLine("ValidatorType is FloatValidator but Epsilon is null.");
                return false;
            }
        }

        return true;
    }

    private static bool TryParseConfig(ZipArchiveEntry configEntry, out TestCaseConfig? config)
    {
        try
        {
            using var stream = configEntry.Open();
            config = JsonSerializer.Deserialize<TestCaseConfig>(stream)!;
            return true;
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to parse Config.json.");
            config = null;
            return false;
        }
    }

    public static bool IsZipValid(string path, out ZipArchive? zipFile)
    {
        try
        {
            zipFile = ZipFile.OpenRead(path);
            return true;
        }
        catch (InvalidDataException)
        {
            zipFile = null;
            return false;
        }
    }
}