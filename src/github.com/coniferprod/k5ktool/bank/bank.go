// Package bank contains type definitions and utility functions for a sound bank.
package bank

import (
	"bytes"
	"encoding/binary"
	"fmt"
	"os"
	"sort"
)

const (
	// NumPatches is the number of patches in a sound bank.
	NumPatches = 128

	//NumEffects is the number of effects in a patch.
	NumEffects = 4
)

var reverbNames = []string{"Hall 1", "Hall 2", "Hall 3", "Room 1", "Room 2", "Room 3", "Plate 1", "Plate 2", "Plate 3", "Reverse", "Long Delay"}

/*
type ReverbType int

const (
	Hall1     ReverbType = 0
	Hall2     ReverbType = 1
	Hall3     ReverbType = 2
	Room1     ReverbType = 3
	Room2     ReverbType = 4
	Room3     ReverbType = 5
	Plate1    ReverbType = 6
	Plate2    ReverbType = 7
	Plate3    ReverbType = 8
	Reverse   ReverbType = 9
	LongDelay ReverbType = 10
)
*/

// Reverb stores the reverb parameters of the patch.
type Reverb struct {
	ReverbType   int // 0...10
	ReverbDryWet int
	ReverbParam1 int
	ReverbParam2 int
	ReverbParam3 int
	ReverbParam4 int
}

// Description returns a textual description of the reverb.
func (r Reverb) Description() string {
	s := reverbNames[r.ReverbType]
	return s
}

// ParamDescription returns a textual description of the reverb parameter number i.
func (r Reverb) ParamDescription(i int) string {
	switch i {
	case 1:
		return "Dry/Wet 2"
	case 2:
		switch r.ReverbType {
		case 9, 10:
			return "Feedback"
		default:
			return "Reverb Time"
		}
	case 3:
		switch r.ReverbType {
		case 10:
			return "Predelay Time"
		default:
			return "Delay Time"
		}
	case 4:
		return "High Frequency Damping"
	default:
		return "Unknown"
	}
}

// Effect stores the effect settings of a patch.
type Effect struct {
	EffectType   int
	EffectDepth  int
	EffectParam1 int
	EffectParam2 int
	EffectParam3 int
	EffectParam4 int
}

var effectNames = []string{
	"Early Reflection 1",
	"Early Reflection 2",
	"Tap Delay 1",
	"Tap Delay 2",
	"Single Delay",
	"Dual Delay",
	"Stereo Delay",
	"Cross Delay",
	"Auto Pan",
	"Auto Pan & Delay",
	"Chorus 1",
	"Chorus 2",
	"Chorus 1 & Delay",
	"Chorus 2 & Delay",
	"Flanger 1",
	"Flanger 2",
	"Flanger 1 & Delay",
	"Flanger 2 & Delay",
	"Ensemble",
	"Ensemble & Delay",
	"Celeste",
	"Celeste & Delay",
	"Tremolo",
	"Tremolo & Delay",
	"Phaser 1",
	"Phaser 2",
	"Phaser 1 & Delay",
	"Phaser 2 & Delay",
	"Rotary",
	"Autowah",
	"Bandpass",
	"Exciter",
	"Enhancer",
	"Overdrive",
	"Distortion",
	"Overdrive & Delay",
	"Distortion & Delay",
}

// There seems to be a conflict in the manual: there are 37 effect names,
// but the number of effects is reported to be 36.
// Cross-check this with the actual synth.

// Description returns a textual description of the effect.
func (e Effect) Description() string {
	s := effectNames[e.EffectType]
	return s
}

// GEQ stores the graphical EQ settings of a patch.
type GEQ struct {
	Freq1 int
	Freq2 int
	Freq3 int
	Freq4 int
	Freq5 int
	Freq6 int
	Freq7 int
}

// Use struct embedding to avoid clash between member and type name.
// See https://notes.shichao.io/gopl/ch4/#struct-embedding-and-anonymous-fields
// It's probably a good idea to use unique names for the fields in the struct-to-embed.

// Common stores the common parameters for a patch.
type Common struct {
	Name            string
	SourceCount     int
	Reverb          // this means that there is a Reverb struct as a part of Common ("struct embedding")
	Volume          int
	Polyphony       int
	EffectAlgorithm int
	Effect1         Effect
	Effect2         Effect
	Effect3         Effect
	Effect4         Effect
	GEQ
}

// Patch represents the parameters of a sound.
type Patch struct {
	Common
}

// Bank contains 128 patches.
type Bank struct {
	Patches [NumPatches]Patch
}

const (
	numSources          = 6
	poolSize            = 0x20000
	commonDataSize      = 82
	sourceDataSize      = 86
	additiveWaveKitSize = 806
	nameSize            = 8
	sourceCountOffset   = 51
	nameOffset          = 40
	numGEQBands         = 7
	numEffects          = 4

	commonChecksumOffset  = 0
	effectAlgorithmOffset = 1
	reverbOffset          = 2
	volumeOffset          = 48
	polyphonyOffset       = 49
	effect1Offset         = 8
	effect2Offset         = 14
	effect3Offset         = 20
	effect4Offset         = 26
)

type patchPtr struct {
	index      int
	tonePtr    int32
	sourcePtrs [numSources]int32
}

func (pp patchPtr) additiveWaveKitCount() int {
	count := 0
	for i := 0; i < numSources; i++ {
		if pp.sourcePtrs[i] != 0 {
			count++
		}
	}
	return count
}

// ParseBankFile parses the bank data into a structure.
func ParseBankFile(bs []byte) Bank {
	buf := bytes.NewReader(bs)

	var err error

	// Read the pointer table (128 * 7 pointers). They are 32-bit integers
	// with Big Endian byte ordering, so we need to specify that when reading.
	var patchPtrs [NumPatches]patchPtr
	for patchIndex := 0; patchIndex < NumPatches; patchIndex++ {
		var tonePtr int32
		err = binary.Read(buf, binary.BigEndian, &tonePtr)
		if err != nil {
			fmt.Println("binary read failed: ", err)
			os.Exit(1)
		}

		var sourcePtrs [numSources]int32
		for sourceIndex := 0; sourceIndex < numSources; sourceIndex++ {
			var sourcePtr int32
			err = binary.Read(buf, binary.BigEndian, &sourcePtr)
			if err != nil {
				fmt.Println("binary read failed: ", err)
				os.Exit(1)
			}

			sourcePtrs[sourceIndex] = sourcePtr
		}

		patchPtrs[patchIndex] = patchPtr{index: patchIndex, tonePtr: tonePtr, sourcePtrs: sourcePtrs}
	}

	// Read the 'memory high water mark' pointer
	var highMemPtr int32
	err = binary.Read(buf, binary.BigEndian, &highMemPtr)
	if err != nil {
		fmt.Println("binary read failed: ", err)
		os.Exit(1)
	}

	// Read the data pool
	var data [poolSize]byte
	err = binary.Read(buf, binary.BigEndian, &data)
	if err != nil {
		fmt.Println("binary read failed: ", err)
		os.Exit(1)
	}

	// Adjust any non-zero tone pointers.
	// Must treat tone pointers as int since there is no sort.Int32s.
	tonePtrs := make([]int, 0)
	for i := 0; i < NumPatches; i++ {
		if patchPtrs[i].tonePtr != 0 {
			tonePtrs = append(tonePtrs, int(patchPtrs[i].tonePtr))
		}
	}

	tonePtrs = append(tonePtrs, int(highMemPtr))
	sort.Ints(tonePtrs)
	base := tonePtrs[0]

	for patchIndex := 0; patchIndex < NumPatches; patchIndex++ {
		if patchPtrs[patchIndex].tonePtr != 0 {
			patchPtrs[patchIndex].tonePtr -= int32(base)
		}

		for sourceIndex := 0; sourceIndex < numSources; sourceIndex++ {
			if patchPtrs[patchIndex].sourcePtrs[sourceIndex] != 0 {
				patchPtrs[patchIndex].sourcePtrs[sourceIndex] -= int32(base)
			}
		}
	}

	// Now we have all the adjusted data pointers, so we can start picking up
	// chunks of data from the big pool based on them.

	b := Bank{}

	for _, pp := range patchPtrs {
		dataStart := int(pp.tonePtr)
		sourceCount := int(data[dataStart+sourceCountOffset])
		dataSize := commonDataSize + sourceDataSize*sourceCount + additiveWaveKitSize*pp.additiveWaveKitCount()
		dataEnd := dataStart + dataSize
		d := data[dataStart:dataEnd]

		nameStart := nameOffset
		nameEnd := nameStart + nameSize
		nameData := d[nameStart:nameEnd]
		name := string(bytes.TrimRight(nameData, "\x00"))

		volume := int(d[volumeOffset])
		polyphony := int(d[polyphonyOffset])
		effectAlgorithm := int(d[effectAlgorithmOffset])

		reverbType := int(d[reverbOffset])
		reverbDryWet := int(d[reverbOffset+1])
		reverbParam1 := int(d[reverbOffset+2])
		reverbParam2 := int(d[reverbOffset+3])
		reverbParam3 := int(d[reverbOffset+4])
		reverbParam4 := int(d[reverbOffset+5])

		effect1Type := int(d[effect1Offset]) - 11
		//fmt.Printf("effect 1 type = %d\n", effect1Type)
		effect1Depth := int(d[effect1Offset+1])
		effect1Param1 := int(d[effect1Offset+2])
		effect1Param2 := int(d[effect1Offset+3])
		effect1Param3 := int(d[effect1Offset+4])
		effect1Param4 := int(d[effect1Offset+5])

		// With struct embedding, the literal must follow the shape of the type declaration. (TGPL, p. 106)
		c := Common{
			Name:        name,
			SourceCount: sourceCount,
			Reverb: Reverb{
				ReverbType:   reverbType,
				ReverbDryWet: reverbDryWet,
				ReverbParam1: reverbParam1,
				ReverbParam2: reverbParam2,
				ReverbParam3: reverbParam3,
				ReverbParam4: reverbParam4,
			},
			Effect1: Effect{
				EffectType:   effect1Type,
				EffectDepth:  effect1Depth,
				EffectParam1: effect1Param1,
				EffectParam2: effect1Param2,
				EffectParam3: effect1Param3,
				EffectParam4: effect1Param4,
			},
			Volume:          volume,
			Polyphony:       polyphony,
			EffectAlgorithm: effectAlgorithm,
		}

		patch := Patch{Common: c}
		b.Patches[pp.index] = patch
	}

	return b
}

// ParseSysExFile parses a System Exclusive data file into a bank structure.
func ParseSysExFile(bs []byte) Bank {
	fmt.Println("Parsing from SysEx file")

	buf := bytes.NewReader(bs)

	var err error

	// Read the SysEx header
	var header [8]byte
	err = binary.Read(buf, binary.BigEndian, &header)
	if err != nil {
		fmt.Println("binary read failed: ", err)
		os.Exit(1)
	}

	// Examine the header.
	// Manufacturer list: https://www.midi.org/specifications-old/item/manufacturer-id-numbers
	if header[0] != 0xF0 {
		fmt.Println("Error: SysEx file must start with F0 (hex)")
		os.Exit(1)
	}

	if header[1] != 0x40 {
		fmt.Println("Error: Manufacturer ID for Kawai should be 40 (hex)")
		os.Exit(1)
	}

	channel := header[2]
	fmt.Printf("MIDI channel = %d\n", channel)

	command := header[3:7]
	blockSingleCommand := []byte{0x21, 0x00, 0x0A, 0x00}
	if !bytes.Equal(command, blockSingleCommand) {
		fmt.Println("Error: this is not a block single dump")
	} else {
		fmt.Println("Block single dump")
	}

	bankID := header[7]
	if bankID == 0x00 {
		fmt.Println("For A1-A128")
	} else if bankID == 0x01 {
		fmt.Println("For B70-B116 (only for K5000W)")
	} else if bankID == 0x02 {
		fmt.Println("For D1-D128")
	}

	b := Bank{}

	return b
}
