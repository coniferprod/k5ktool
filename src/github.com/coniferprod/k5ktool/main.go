package main

import (
	"flag"
	"fmt"
	"io/ioutil"
	"os"
	"path/filepath"
	s "strings"

	"github.com/coniferprod/k5ktool/bank"
	"github.com/coniferprod/k5ktool/sound"
)

var (
	command       string
	inputFileName string
	patchNumber   int
	waveformName  string

	usage = "Usage: k5ktool <command> -i <infile> -p <patchnum>\n" +
		"<command> = list, convert, generate\n" +
		"<infile> = input file (.kaa or .syx)\n" +
		"<patchnum> = patch number in bank (1...128, default is all)" +
		"<sections> = which patch sections to show (default is 'ncsa' for all (n = name, c = common, s = sources, a = additive kits))"
)

// Guidance for command line argument handling:
// https://blog.rapid7.com/2016/08/04/build-a-simple-cli-tool-with-golang/

func main() {
	listCommand := flag.NewFlagSet("list", flag.ExitOnError)
	patchNumberPtr := listCommand.Int("p", 0, "Patch number 1 – 128 (default is all patches in the bank)")
	sectionsPtr := listCommand.String("s", "ncsa", "What patch sections to show (default is 'ncsa' for all)")

	convertCommand := flag.NewFlagSet("convert", flag.ExitOnError)

	generateCommand := flag.NewFlagSet("generate", flag.ExitOnError)

	// Flags for the list command:
	listInputFileName := listCommand.String("i", "", "Name of Kawai K5000 bank file (.kaa) or System Exclusive file (.syx)")

	// Flags for the convert command
	convertInputFileName := convertCommand.String("i", "", "Name of Kawai K5000 bank file (.kaa) or System Exclusive file (.syx)")

	// Flas for the generate command
	generateWaveformName := generateCommand.String("w", "", "Waveform to generate: saw, square, or triangle")

	if len(os.Args) < 2 {
		fmt.Println(usage)
		os.Exit(1)
	}

	switch os.Args[1] {
	case "list":
		listCommand.Parse(os.Args[2:])
	case "convert":
		convertCommand.Parse(os.Args[2:])
	case "generate":
		generateCommand.Parse(os.Args[2:])
	default:
		flag.PrintDefaults()
		os.Exit(1)
	}

	patchNumber := 0   // default to 0 = all patches
	sections := "ncsa" // default to showing all information about a patch

	if listCommand.Parsed() {
		if *listInputFileName == "" {
			listCommand.PrintDefaults()
			os.Exit(1)
		}

		command = "list"
		inputFileName = *listInputFileName
		patchNumber = *patchNumberPtr
		sections = *sectionsPtr

		list(inputFileName, patchNumber, sections)
	}

	if convertCommand.Parsed() {
		if *convertInputFileName == "" {
			convertCommand.PrintDefaults()
			os.Exit(1)
		}

		command = "convert"
		inputFileName = *convertInputFileName

		fmt.Println("Convert patches")
		fmt.Println("Not implemented yet")
	}

	if generateCommand.Parsed() {
		if *generateWaveformName == "" {
			generateCommand.PrintDefaults()
			os.Exit(1)
		}

		command = "generate"
		waveformName = *generateWaveformName

		fmt.Printf("Generate, waveform = %s\n", waveformName)
		lowHarmonics := sound.NewHarmonicLevels(waveformName)
		fmt.Printf("%s\n", lowHarmonics)
	}
}

func list(inputFileName string, patchNumber int, sections string) {
	extension := s.ToLower(filepath.Ext(inputFileName))

	//fmt.Fprintf(os.Stdout, "command = '%v', input file name = '%v', extension = '%v', patchNumber = %d\n", command, inputFileName, extension, patchNumber)

	data, err := ioutil.ReadFile(inputFileName) // read the whole file into memory
	if err != nil {
		fmt.Printf("error opening %s: %s\n", inputFileName, err)
		os.Exit(1)
	}

	// Now we should have contents of the whole file in `data`.

	var b bank.Bank
	if extension == ".kaa" {
		b = bank.ParseBankFile(data)
	} else if extension == ".syx" {
		b = bank.ParseSysExFile(data)
	} else {
		fmt.Printf("Don't know how to handle %s files\n", extension)
		os.Exit(1)
	}

	if patchNumber == 0 {
		listAllPatches(b, sections)
	} else {
		listPatch(b, patchNumber-1, sections)
	}
}

func printName(p bank.Patch, i int) {
	fmt.Printf("%3d  %s\n", i+1, p.Common.Name)
}

func printCommon(p bank.Patch) {
	fmt.Printf("%s\n", p.Common)
	fmt.Printf("Reverb: %s\n", p.Common.Reverb)
	fmt.Printf("Effect 1: %s\n", p.Common.Effect1)
	fmt.Printf("Effect 2: %s\n", p.Common.Effect2)
	fmt.Printf("Effect 3: %s\n", p.Common.Effect3)
	fmt.Printf("Effect 4: %s\n", p.Common.Effect4)
	fmt.Printf("GEQ: %s\n", p.Common.GEQ)
	fmt.Printf("AM: %d\n", p.Common.AmplitudeModulation)
	fmt.Printf("Portamento: %t  Speed: %d\n", p.Common.PortamentoEnabled, p.Common.PortamentoSpeed)
}

func printSources(p bank.Patch) {
	fmt.Printf("Patch has %d sources\n", p.SourceCount)
	for s := 0; s < p.Common.SourceCount; s++ {
		fmt.Printf("Source %d:\n", s+1)
		source := p.Sources[s]

		fmt.Printf("DCO: %s\n", source.Oscillator)
		//fmt.Printf("S%d: %#v\n", s+1, p.Sources[s])
		filter := source.Filter
		fmt.Printf("    Filter: %s\n", filter)
		fmt.Printf("    Filter envelope: depth = %d, %s\n", filter.EnvelopeDepth, filter.Envelope)
	}
}

func printAdditiveKits(p bank.Patch) {
	for k := 0; k < len(p.Kits); k++ {
		fmt.Printf("Additive Kit #%d:\n", k+1)
		fmt.Printf("%s\n", p.Kits[k])
	}
}

func listPatch(b bank.Bank, i int, sections string) {
	p := b.Patches[i]
	if s.Contains(sections, "n") {
		printName(p, i)
	}

	if s.Contains(sections, "c") {
		printCommon(p)
	}

	if s.Contains(sections, "s") {
		printSources(p)
	}

	if s.Contains(sections, "a") {
		printAdditiveKits(p)
	}
}

func listAllPatches(b bank.Bank, sections string) {
	for i := 0; i < bank.NumPatches; i++ {
		listPatch(b, i, sections)
		//fmt.Println()
	}
}
