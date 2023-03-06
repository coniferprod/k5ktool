using CommandLine;

namespace K5KTool
{
    [Verb("create", HelpText = "Create a new patch.")]
    public class CreateOptions
    {
        [Option('t', "type", Required = true, HelpText = "Patch type (single or multi).")]
        public string PatchType { get; set; }

        [Option('d', "descriptor", Required = false, HelpText = "Descriptor file in JSON format")]
        public string Descriptor { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file")]
        public string OutputFileName { get; set; }

        [Option('b', "bank", Required = true, HelpText = "Bank name (A, D, E, F)")]
        public string BankName { get; set; }

        [Option('n', "number", Required = true, HelpText = "Patch number (1...128)")]
        public int PatchNumber { get; set; }
    }

    [Verb("list", HelpText = "List contents of bank.")]
    public class ListOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed")]
        public string FileName { get; set; }
    }

    [Verb("dump", HelpText = "Dump contents of bank or patch file.")]
    public class DumpOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed")]
        public string FileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file format ('text' or 'json')")]
        public string Output { get; set; }
    }

    [Verb("report", HelpText = "Report on the specified bank or patch file.")]
    public class ReportOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed")]
        public string FileName { get; set; }
    }

    [Verb("init", HelpText = "Initialize a new bank.")]
    public class InitOptions
    {
        [Option('o', "output", Required = true, HelpText = "Output file")]
        public string OutputFileName { get; set; }
    }

    [Verb("edit", HelpText = "Make changes to the edit buffer.")]
    public class EditOptions
    {
        [Option('d', "device", Required = true, HelpText = "MIDI device for `sendmidi` command")]
        public string Device { get; set; }

        [Option('w', "waveform", Required = true, HelpText = "Waveform for harmonic levels")]
        public string Waveform { get; set; }

        [Option('p', "params", Required = false, HelpText = "Parameters for custom waveform, comma-separated")]
        public string Params { get; set; }
    }
}
