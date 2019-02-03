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

	usage = "Usage: k5ktool <command> -i <infile>\n" +
		"<command> = list or convert\n" +
		"<infile> = input file (.kaa or .syx)"
)

// Guidance for command line argument handling:
// https://blog.rapid7.com/2016/08/04/build-a-simple-cli-tool-with-golang/

func main() {
	listCommand := flag.NewFlagSet("list", flag.ExitOnError)
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

	if listCommand.Parsed() {
		if *listInputFileName == "" {
			listCommand.PrintDefaults()
			os.Exit(1)
		}

		command = "list"
		inputFileName = *listInputFileName
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

	fmt.Fprintf(os.Stdout, "command = '%v', input file name = '%v', extension = '%v'\n", command, inputFileName, extension)

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
		fmt.Println("List patches in the bank")
		listPatches(b)

	case "convert":
		fmt.Println("Convert patches")
		fmt.Println("Not implemented yet")

	default:
		fmt.Println(usage)
		os.Exit(1)
	}
}

func listPatches(b bank.Bank) {
	for i := 0; i < bank.NumPatches; i++ {
		p := b.Patches[i]
		fmt.Printf("%3d  %s\n", i+1, p.Common)
		fmt.Println(p.Common.Reverb)
		fmt.Printf("effect 1: %s\n", p.Common.Effect1)
		fmt.Printf("effect 2: %s\n", p.Common.Effect1)
		fmt.Printf("effect 3: %s\n", p.Common.Effect1)
		fmt.Printf("effect 4: %s\n", p.Common.Effect1)
		fmt.Printf("GEQ: %s\n", p.Common.GEQ)
		for s := 0; s < p.Common.SourceCount; s++ {
			fmt.Printf("S%d: %#v\n", s+1, p.Sources[s])
		}
		fmt.Println()
	}
}
