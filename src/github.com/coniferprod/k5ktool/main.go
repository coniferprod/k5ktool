package main

import (
	"flag"
	"fmt"
	"io/ioutil"
	"os"

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
	flag.StringVar(&fileName, "f", "", "Name of Kawai K5000 bank file (.KAA)")
}

func main() {
	flag.Parse()
	// TODO: Check for missing file name argument
	fmt.Fprintf(os.Stdout, "command = %v, fileName = %v\n", command, fileName)
	switch command {
	case "list":
		fmt.Println("List patches in the bank")
		data, err := ioutil.ReadFile(fileName) // read the whole file into memory
		if err != nil {
			fmt.Printf("error opening %s: %s\n", fileName, err)
			os.Exit(1)
		}

		// Now we should have contents of the whole file in `data`.

		b := bank.Parse(data)
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
