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
	command  string
	fileName string

	usage = "Usage: k5ktool <command> -i <infile>\n" +
		"<command> = one of: list, convert\n" +
		"<infile> = input file (.kaa or .syx)"
)

func init() {
	flag.StringVar(&command, "c", "list", "Command - currently only 'list'")
	flag.StringVar(&fileName, "i", "", "Name of Kawai K5000 bank file (.kaa) or System Exclusive file (.syx)")
}

func main() {
	flag.Parse()
	// TODO: Check for missing file name argument
	extension := strings.ToLower(filepath.Ext(fileName))

	fmt.Fprintf(os.Stdout, "command = %v, fileName = %v, extension = %v\n", command, fileName, extension)

	data, err := ioutil.ReadFile(fileName) // read the whole file into memory
	if err != nil {
		fmt.Printf("error opening %s: %s\n", fileName, err)
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
	listPatches(b)

	switch command {
	case "list":
		fmt.Println("List patches in the bank")

	case "convert":
		fmt.Println("Convert patches")

	default:
		fmt.Println(usage)
		os.Exit(1)
	}
}

func listPatches(b bank.Bank) {
	for i := 0; i < bank.NumPatches; i++ {
		p := b.Patches[i]
		fmt.Printf("%3d  %8s  vol=%3d  poly=%d\n", i+1, p.Common.Name, p.Common.Volume, p.Common.Polyphony)
		fmt.Printf("sources = %d\n", p.Common.SourceCount)
		fmt.Printf("effect algorithm = %d\n", p.Common.EffectAlgorithm)
		fmt.Printf("reverb: %s, %d%% wet, %s = %d, %s = %d, %s = %d, %s = %d\n",
			p.Common.Reverb.Description(),
			p.Common.ReverbDryWet,
			p.Common.Reverb.ParamDescription(1),
			p.Common.ReverbParam1,
			p.Common.Reverb.ParamDescription(2),
			p.Common.ReverbParam2,
			p.Common.Reverb.ParamDescription(3),
			p.Common.ReverbParam3,
			p.Common.Reverb.ParamDescription(4),
			p.Common.ReverbParam4)
		fmt.Printf("effect 1: %s, depth = %d, param1 = %d, param2 = %d, param3 = %d, param4 = %d\n",
			p.Common.Effect1.Description(),
			p.Common.Effect1.EffectDepth,
			p.Common.Effect1.EffectParam1,
			p.Common.Effect1.EffectParam2,
			p.Common.Effect1.EffectParam3,
			p.Common.Effect1.EffectParam3)
		fmt.Printf("effect 2: %s, depth = %d, param1 = %d, param2 = %d, param3 = %d, param4 = %d\n",
			p.Common.Effect2.Description(),
			p.Common.Effect2.EffectDepth,
			p.Common.Effect2.EffectParam1,
			p.Common.Effect2.EffectParam2,
			p.Common.Effect2.EffectParam3,
			p.Common.Effect2.EffectParam3)
		fmt.Printf("effect 3: %s, depth = %d, param1 = %d, param2 = %d, param3 = %d, param4 = %d\n",
			p.Common.Effect3.Description(),
			p.Common.Effect3.EffectDepth,
			p.Common.Effect3.EffectParam1,
			p.Common.Effect3.EffectParam2,
			p.Common.Effect3.EffectParam3,
			p.Common.Effect3.EffectParam3)
		fmt.Printf("effect 4: %s, depth = %d, param1 = %d, param2 = %d, param3 = %d, param4 = %d\n",
			p.Common.Effect4.Description(),
			p.Common.Effect4.EffectDepth,
			p.Common.Effect4.EffectParam1,
			p.Common.Effect4.EffectParam2,
			p.Common.Effect4.EffectParam3,
			p.Common.Effect4.EffectParam3)
		fmt.Printf("GEQ: %d %d %d %d %d %d %d\n",
			p.Common.GEQ.Freq1,
			p.Common.GEQ.Freq2,
			p.Common.GEQ.Freq3,
			p.Common.GEQ.Freq4,
			p.Common.GEQ.Freq5,
			p.Common.GEQ.Freq6,
			p.Common.GEQ.Freq7)

		fmt.Println()
	}
}
