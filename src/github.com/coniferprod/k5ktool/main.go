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
	patches []patchData
	data    [poolSize]byte
}

type header struct {
	patches      [numPatches]patchData
	displacement int32
}

func readBankData(bs []byte) bankData {
	buf := bytes.NewReader(bs)

	var b bankData

	for patchIndex := 0; patchIndex < numPatches; patchIndex++ {
		var tonePtr int32
		err := binary.Read(buf, binary.BigEndian, &tonePtr)
		if err != nil {
			fmt.Println("binary read failed:", err)
		}
		p := patchData{index: patchIndex, tonePtr: tonePtr}
		//p.index = patchIndex
		//p.tonePtr = tonePtr
		//fmt.Printf("%03d: %08x\n", p.index+1, p.tonePtr)

		for sourceIndex := 0; sourceIndex < numSources; sourceIndex++ {
			var sourcePtr int32
			err := binary.Read(buf, binary.BigEndian, &sourcePtr)
			if err != nil {
				fmt.Println("binary read failed:", err)
			}

			s := sourceData{additiveKitPtr: sourcePtr, isAdditive: sourcePtr != 0}
			//s.additiveKitPtr = sourcePtr
			//s.isAdditive = s.additiveKitPtr != 0
			if s.isAdditive {
				p.additiveKitCount++
			}
			p.sources[sourceIndex] = s
			//fmt.Printf("  %d: %08x\n", sourceIndex+1, sourcePtr)
		}

		if p.tonePtr != 0 {
			b.patches = append(b.patches, p)
		}
	}

	var displacement int32
	err := binary.Read(buf, binary.BigEndian, &displacement)
	if err != nil {
		fmt.Println("binary read failed:", err)
	}
	fmt.Printf("displacement = %08x\n", displacement)

	var data [poolSize]byte
	err = binary.Read(buf, binary.BigEndian, &data)
	if err != nil {
		fmt.Println("binary read failed:", err)
	}
	b.data = data

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
}
