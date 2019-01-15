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

const (
	numPatches          = 128
	wordSize            = 4
	numSources          = 6
	poolSize            = 0x20000
	commonDataSize      = 82
	sourceDataSize      = 86
	additiveWaveKitSize = 806
	sourceCountOffset   = 51
	nameOffset          = 40
	nameSize            = 8
	numGEQBands         = 7
	numEffects          = 4

	commonChecksumOffset  = 0
	effectAlgorithmOffset = 1
	volumeOffset          = 48
	polyphonyOffset       = 49
)

// From here on the structs describe the semantic patch data, not the file structure

type effect struct {
	effectType byte
	depth      byte
	para1      byte
	para2      byte
	para3      byte
	para4      byte
}

type polyphony int

const (
	poly  polyphony = 0
	solo1 polyphony = 1
	solo2 polyphony = 2
)

type effectAlgorithm int

const (
	algorithm1 effectAlgorithm = 0
	algorithm2 effectAlgorithm = 1
	algorithm3 effectAlgorithm = 2
	algorithm4 effectAlgorithm = 3
)

var effectNames = []string{"Early Reflection 1", "Early Reflection 2", "Tap Delay 1", "Tap Delay 2", "Single Delay", "Dual Delay", "Stereo Delay", "Cross Delay", "Auto Pan", "Auto Pan & Delay", "Chorus 1", "Chorus 2", "Chorus 1 & Delay", "Chorus 2 & Delay", "Flanger 1", "Flanger 2", "Flanger 1 & Delay", "Flanger 2 & Delay", "Ensemble", "Ensemble & Delay", "Celeste", "Celeste & Delay", "Tremolo", "Tremolo & Delay", "Phaser 1", "Phaser 2", "Phaser 1 & Delay", "Phaser 2 & Delay", "Rotary", "Autowah", "Bandpass", "Exciter", "Enhancer", "Overdrive", "Distortion", "Overdrive & Delay", "Distortion & Delay"}

// There seems to be a conflict in the manual: there are 37 effect names, but the number of effects is reported to be 36.
// Cross-check this with the actual synth.

type reverbType int

const (
	hall1     reverbType = 0
	hall2     reverbType = 1
	hall3     reverbType = 2
	room1     reverbType = 3
	room2     reverbType = 4
	room3     reverbType = 5
	plate1    reverbType = 6
	plate2    reverbType = 7
	plate3    reverbType = 8
	reverse   reverbType = 9
	longDelay reverbType = 10
)

func listPatches(b bank.Bank) {
	for i := 0; i < bank.NumPatches; i++ {
		p := b.Patches[i]
		fmt.Printf("%3d  %8s\n", i+1, p.Common.Name)
	}
}
