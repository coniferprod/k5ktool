using CommandLine;

namespace K5KTool
{
    [Verb("create", HelpText="Create a new patch.")]
    public class CreateOptions
    {
        [Option]
        public string PatchType { get; set; }
        
        [Option]
        public string OutputFileName { get; set; }
    }

    [Verb("list", HelpText = "List contents of bank.")]
    public class ListOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file format ('text' or 'html')")]
        public string Output { get; set; }
    }

    [Verb("dump", HelpText = "Dump contents of bank.")]
    public class DumpOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }
    }

    [Verb("report", HelpText = "Report on the specified bank.")]
    public class ReportOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }
    }

    [Verb("init", HelpText = "Initialize a new bank.")]
    public class InitOptions
    {
        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFileName { get; set; }
    }
}
