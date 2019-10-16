using CommandLine;

namespace K5KTool
{
    [Verb("create", HelpText="Create a new patch.")]
    class CreateOptions
    {
        [Option]
        public string PatchType { get; set; }
        
        [Option]
        public string OutputFileName { get; set; }
    }
}
