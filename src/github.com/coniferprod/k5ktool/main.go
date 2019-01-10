package main

import (
	"bytes"
	"encoding/binary"
	"flag"
	"fmt"
	"io/ioutil"
	"os"
	"sort"
)

var (
	command  string
	fileName string

	usage = "Usage: k5ktool <command> [arguments]\n" +
		"<command> = one of list"
)

func check(e error) {
	if e != nil {
		panic(e)
	}
}

func init() {
	flag.StringVar(&command, "c", "list", "Command - currently only 'list'")
	flag.StringVar(&fileName, "f", "", "Name of Kawai K5000 bank file (.KAA)")
}

func main() {
	flag.Parse()
	fmt.Fprintf(os.Stdout, "command = %v, fileName = %v\n", command, fileName)
	switch command {
	case "list":
		fmt.Println("List patches in the bank")
		data, err := ioutil.ReadFile(fileName) // read the whole file into memory
		check(err)

		bd := readBankData(data)

		b := parsePatchData(bd)

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
	toneCommonDataSize  = 82
	sourceDataSize      = 86
	additiveWaveKitSize = 806
	nameSize            = 8
	sourceCountOffset   = 51
	nameOffset          = 40
	numGEQBands         = 7
	numEffects          = 4
)

type patchData struct {
	index            int
	tonePtr          int32
	isUsed           bool
	sources          [numSources]sourceData
	sourceCount      int
	size             int
	additiveKitCount int
	name             string
	sourceTypes      string
}

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

type commonData struct {
	algorithm    effectAlgorithm
	reverb       reverbType
	reverbDryWet int
	reverbPara1  int
	reverbPara2  int
	reverbPara3  int
	reverbPara4  int
	effects      [numEffects]effect
	geq          [numGEQBands]int
	drumMark     bool
	name         string // eight characters max
	volume       int
	poly         polyphony
	numSources   int
	sourceMutes  byte // TODO: check the contents of this setting

	// From here on there are more common parameters, but maybe later...

}

type bank struct {
	patches [numPatches]patch
}

type patch struct {
	common commonData
}

type sourceData struct {
	isAdditive     bool
	additiveKitPtr int32
	data           [sourceDataSize]byte
}

type bankData struct {
	patches     []patchData
	patchCount  int
	data        [poolSize]byte
	usedPatches []patchData
}

type header struct {
	patches      [numPatches]patchData
	displacement int32
}

func readBankData(bs []byte) bankData {
	buf := bytes.NewReader(bs)

	var b bankData

	// Read the pointer table (128 * 7 pointers) and build list of used patches
	patchCount := 0
	for patchIndex := 0; patchIndex < numPatches; patchIndex++ {
		var tonePtr int32
		err := binary.Read(buf, binary.BigEndian, &tonePtr)
		if err != nil {
			fmt.Println("binary read failed:", err)
		}
		p := patchData{index: patchIndex, tonePtr: tonePtr, isUsed: tonePtr != 0, additiveKitCount: 0}

		// Pointers to ADD wave kits
		for sourceIndex := 0; sourceIndex < numSources; sourceIndex++ {
			var sourcePtr int32
			err := binary.Read(buf, binary.BigEndian, &sourcePtr)
			if err != nil {
				fmt.Println("binary read failed:", err)
			}

			s := sourceData{additiveKitPtr: sourcePtr, isAdditive: sourcePtr != 0}
			if s.isAdditive {
				p.additiveKitCount++
			}
			p.sources[sourceIndex] = s
			//fmt.Printf("  %d: %08x\n", sourceIndex+1, sourcePtr)
		}

		if p.isUsed {
			b.usedPatches = append(b.patches, p)
			patchCount++
		}
	}

	// Read the 'memory high water mark' pointer
	var displacement int32
	err := binary.Read(buf, binary.BigEndian, &displacement)
	if err != nil {
		fmt.Println("binary read failed:", err)
	}
	fmt.Printf("displacement = %08x\n", displacement)

	b.patchCount = patchCount

	// Read the data pool
	var data [poolSize]byte
	err = binary.Read(buf, binary.BigEndian, &data)
	if err != nil {
		fmt.Println("binary read failed:", err)
	}
	b.data = data

	// Adjust any non-zero tone pointers
	tonePtrs := make([]int, 0)
	for i := 0; i < len(b.patches); i++ {
		ptr := int(b.patches[i].tonePtr)
		if ptr == 0 {
			fmt.Printf("tonePtr #%d == 0, not adding it\n", i+1)
			continue
		}
		tonePtrs = append(tonePtrs, ptr)
	}
	tonePtrs = append(tonePtrs, int(displacement))

	sort.Ints(tonePtrs)
	fmt.Printf("sorted: len = %d, cap = %d\n", len(tonePtrs), cap(tonePtrs))

	base := tonePtrs[0]
	fmt.Printf("base = %08x\n", base)

	for i := 0; i < len(b.patches); i++ {
		b.patches[i].tonePtr -= int32(base)
		fmt.Printf("%03d: %08x\n", i+1, b.patches[i].tonePtr)
		for j := 0; j < len(b.patches[i].sources); j++ {
			if b.patches[i].sources[j].isAdditive {
				b.patches[i].sources[j].additiveKitPtr -= int32(base)
			}
			fmt.Printf("  %d: %08x\n", j+1, b.patches[i].sources[j].additiveKitPtr)
		}
	}

	displacement -= int32(base)
	fmt.Printf("adjusted displacement = %08x\n", displacement)

	return b
}

func parsePatchData(bd bankData) bank {
	var b bank

	for i := 0; i < numPatches; i++ {
		dataStart := i * 1024
		dataEnd := dataStart + 1024
		d := bd.data[dataStart:dataEnd]
		c := commonData{algorithm: effectAlgorithm(d[0]), reverb: reverbType(d[1])}

		e1 := effect{effectType: d[7], depth: d[8]}
		e2 := effect{effectType: d[13], depth: d[14]}
		e3 := effect{effectType: d[19], depth: d[20]}
		e4 := effect{effectType: d[25], depth: d[26]}

		c.effects = [4]effect{e1, e2, e3, e4}

		nameStart := dataStart + nameOffset
		nameData := d[nameStart : nameStart+nameSize]
		nameData = bytes.TrimRight(nameData, "\x00")
		c.name = string(nameData)

		p := patch{common: c}

		b.patches[i] = p
	}

	return b
}

// Old listPatches code:
/*
	for i := 0; i < numPatches; i++ {
		p := b.patches[i]
		offset := int(p.tonePtr) + int(sourceCountOffset)
		fmt.Printf("len(data) = %d, offset = %d\n", len(b.data), offset)
		sc := int(b.data[offset])
		b.patches[i].sourceCount = sc
		fmt.Printf("source count = %d\n", sc)
		b.patches[i].size = toneCommonDataSize + sourceDataSize*b.patches[i].sourceCount + additiveWaveKitSize*b.patches[i].additiveKitCount
		nameStart := int(p.tonePtr) + int(nameOffset)
		fmt.Printf("nameStart = %d\n", nameStart)
		nameData := b.data[nameStart : nameStart+nameSize]
		nameData = bytes.TrimRight(nameData, "\x00")
		b.patches[i].name = string(nameData)

		sourceTypes := ""
		for j := 0; j < b.patches[i].sourceCount; j++ {
			s := p.sources[j]
			fmt.Printf("source #%d: %v", j+1, s)
			if s.isAdditive {
				sourceTypes += "A"
			} else {
				sourceTypes += "P"
			}
		}
		sn := p.sourceCount
		for sn < numSources {
			sourceTypes += "-"
			sn++
		}
		b.patches[i].sourceTypes = sourceTypes
	}

	for i := 0; i < len(b.patches); i++ {
		p := b.patches[i]
		fmt.Printf("%3d  %8s  %s  %d\n", p.index, p.name, p.sourceTypes, p.size)
	}
*/

func listPatches(b bank) {
	for i := 0; i < numPatches; i++ {
		p := b.patches[i]
		fmt.Printf("%3d  %8s\n", i+1, p.common.name)
	}
}
