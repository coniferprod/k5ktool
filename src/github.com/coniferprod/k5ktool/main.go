package main

import (
	"flag"
	"fmt"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"

	"github.com/coniferprod/k5ktool/bank"
)

var (
	command       string
	inputFileName string
	patchNumber   int

	usage = "Usage: k5ktool <command> -i <infile> -p <patchnum>\n" +
		"<command> = list or convert\n" +
		"<infile> = input file (.kaa or .syx)\n" +
		"<patchnum> = patch number in bank (1...128, default is all)"
)

// Guidance for command line argument handling:
// https://blog.rapid7.com/2016/08/04/build-a-simple-cli-tool-with-golang/

func main() {
	listCommand := flag.NewFlagSet("list", flag.ExitOnError)
	patchNumberPtr := listCommand.Int("p", 0, "Patch number 1 – 128 (default is all patches in the bank)")

	convertCommand := flag.NewFlagSet("convert", flag.ExitOnError)

	// Flags for the list command:
	listInputFileName := listCommand.String("i", "", "Name of Kawai K5000 bank file (.kaa) or System Exclusive file (.syx)")

	// Flags for the convert command
	convertInputFileName := convertCommand.String("i", "", "Name of Kawai K5000 bank file (.kaa) or System Exclusive file (.syx)")

	if len(os.Args) < 2 {
		fmt.Println(usage)
		os.Exit(1)
	}

	switch os.Args[1] {
	case "list":
		listCommand.Parse(os.Args[2:])
	case "convert":
		convertCommand.Parse(os.Args[2:])
	default:
		flag.PrintDefaults()
		os.Exit(1)
	}

	patchNumber := 0 // default to 0 = all patches

	if listCommand.Parsed() {
		if *listInputFileName == "" {
			listCommand.PrintDefaults()
			os.Exit(1)
		}

		command = "list"
		inputFileName = *listInputFileName
		patchNumber = *patchNumberPtr
	}

	if convertCommand.Parsed() {
		if *convertInputFileName == "" {
			convertCommand.PrintDefaults()
			os.Exit(1)
		}

		command = "convert"
		inputFileName = *convertInputFileName
	}

	extension := strings.ToLower(filepath.Ext(inputFileName))

	fmt.Fprintf(os.Stdout, "command = '%v', input file name = '%v', extension = '%v', patchNumber = %d\n", command, inputFileName, extension, patchNumber)

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

	switch command {
	case "list":
		if patchNumber == 0 {
			listAllPatches(b)
		} else {
			listPatch(b, patchNumber-1)
		}

	case "convert":
		fmt.Println("Convert patches")
		fmt.Println("Not implemented yet")

	default:
		fmt.Println(usage)
		os.Exit(1)
	}
}

func listPatch(b bank.Bank, i int) {
	p := b.Patches[i]
	fmt.Printf("%3d  %s\n", i+1, p.Common)
	fmt.Printf("reverb: %s\n", p.Common.Reverb)
	fmt.Printf("effect 1: %s\n", p.Common.Effect1)
	fmt.Printf("effect 2: %s\n", p.Common.Effect2)
	fmt.Printf("effect 3: %s\n", p.Common.Effect3)
	fmt.Printf("effect 4: %s\n", p.Common.Effect4)
	fmt.Printf("GEQ = %s\n", p.Common.GEQ)
	fmt.Printf("AM = %d\n", p.Common.AmplitudeModulation)
	fmt.Printf("portamento = %t, speed = %d\n", p.Common.PortamentoEnabled, p.Common.PortamentoSpeed)
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

func listAllPatches(b bank.Bank) {
	fmt.Println("List all patches in the bank")

	for i := 0; i < bank.NumPatches; i++ {
		listPatch(b, i)
		fmt.Println()
	}
}
