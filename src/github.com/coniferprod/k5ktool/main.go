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

	usage = "Usage: k5ktool <command> [arguments]\n" +
		"<command> = one of list"
)

func init() {
	flag.StringVar(&command, "c", "list", "Command - currently only 'list'")
	flag.StringVar(&fileName, "f", "", "Name of Kawai K5000 bank file (.kaa) or System Exclusive file (.syx)")
}

func main() {
	flag.Parse()
	// TODO: Check for missing file name argument
	extension := strings.ToLower(filepath.Ext(fileName))

	fmt.Fprintf(os.Stdout, "command = %v, fileName = %v, extension = %v\n", command, fileName, extension)
	switch command {
	case "list":
		fmt.Println("List patches in the bank")
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

	default:
		fmt.Println(usage)
		os.Exit(1)
	}
}

func listPatches(b bank.Bank) {
	for i := 0; i < bank.NumPatches; i++ {
		p := b.Patches[i]
		fmt.Printf("%3d  %8s\n", i+1, p.Common.Name)
	}
}
