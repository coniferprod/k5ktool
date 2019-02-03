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

func (r Reverb) String() string {
	return fmt.Sprintf("%s, %d%% wet, %s = %d, %s = %d, %s = %d, %s = %d",
		r.Description(),
		r.ReverbDryWet,
		r.ParamDescription(1),
		r.ReverbParam1,
		r.ParamDescription(2),
		r.ReverbParam2,
		r.ParamDescription(3),
		r.ReverbParam3,
		r.ParamDescription(4),
		r.ReverbParam4)
}

func getReverb(data []byte) Reverb {
	return Reverb{
		ReverbType:   int(data[0]),
		ReverbDryWet: int(data[1]),
		ReverbParam1: int(data[2]),
		ReverbParam2: int(data[3]),
		ReverbParam3: int(data[4]),
		ReverbParam4: int(data[5]),
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

func (e Effect) String() string {
	return fmt.Sprintf("%s, depth = %d, %s = %d, %s = %d, %s = %d, %s = %d",
		e.Description(),
		e.EffectDepth,
		e.ParamDescription(1),
		e.EffectParam1,
		e.ParamDescription(2),
		e.EffectParam2,
		e.ParamDescription(3),
		e.EffectParam3,
		e.ParamDescription(4),
		e.EffectParam4)
}

func (e Effect) ParamDescription(paramNumber int) string {
	switch e.EffectType {
	case 1, 2: // Early Reflection 1 and 2
		switch paramNumber {
		case 1:
			return "Slope"
		case 2:
			return "Predelay Time"
		case 3:
			return "Feedback"
		default:
			return "?"
		}
	case 3, 4: // Tap Delay 1 and 2
		switch paramNumber {
		case 1:
			return "Delay Time 1"
		case 2:
			return "Tap Level"
		case 3:
			return "Delay Time 2"
		default:
			return "?"
		}
	case 5: // Single Delay
		switch paramNumber {
		case 1:
			return "Delay Time Fine"
		case 2:
			return "Delay Time Coarse"
		case 3:
			return "Feedback"
		default:
			return "?"
		}
	case 6: // Dual Delay
		switch paramNumber {
		case 1:
			return "Delay Time Left"
		case 2:
			return "Feedback Left"
		case 3:
			return "Delay Time Right"
		case 4:
			return "Feedback Right"
		default:
			return "?"
		}
	case 7, 8: // Stereo Delay, Cross Delay
		switch paramNumber {
		case 1:
			return "Delay Time"
		case 2:
			return "Feedback"
		default:
			return "?"
		}
	case 9, 11, 12, 23: // Auto Pan; Chorus 1; Chorus 2; Tremolo
		switch paramNumber {
		case 1:
			return "Speed"
		case 2:
			return "Depth"
		case 3:
			return "Predelay Time"
		case 4:
			return "Wave"
		default:
			return "?"
		}
	case 10, 13, 14, 24: // Auto Pan & Delay; Chorus 1 & Delay; Chorus 2 & Delay; Tremolo & Delay
		switch paramNumber {
		case 1:
			return "Speed"
		case 2:
			return "Depth"
		case 3:
			return "Delay Time"
		case 4:
			return "Wave"
		default:
			return "?"
		}
	case 15, 16, 25, 26: // Flanger 1 & 2, Phaser 1 & 2
		switch paramNumber {
		case 1:
			return "Speed"
		case 2:
			return "Depth"
		case 3:
			return "Predelay Time"
		case 4:
			return "Feedback"
		default:
			return "?"
		}
	case 17, 18, 27, 28: // Flanger 1 & Delay; Flanger 2 & Delay, Phaser 1 & Delay, Phaser 2 & Delay
		switch paramNumber {
		case 1:
			return "Speed"
		case 2:
			return "Depth"
		case 3:
			return "Delay Time"
		case 4:
			return "Feedback"
		default:
			return "?"
		}
	case 19: // Ensemble
		switch paramNumber {
		case 1:
			return "Depth"
		case 2:
			return "Predelay Time"
		default:
			return "?"
		}
	case 20: // Ensemble & Delay
		switch paramNumber {
		case 1:
			return "Depth"
		case 2:
			return "Delay Time"
		default:
			return "?"
		}
	case 21: // Celeste
		switch paramNumber {
		case 1:
			return "Speed"
		case 2:
			return "Depth"
		case 3:
			return "Predelay Time"
		default:
			return "?"
		}
	case 22: // Celeste & Delay
		switch paramNumber {
		case 1:
			return "Speed"
		case 2:
			return "Depth"
		case 3:
			return "Delay Time"
		default:
			return "?"
		}
	case 29: // Rotary
		switch paramNumber {
		case 1:
			return "Slow Speed"
		case 2:
			return "Fast Speed"
		case 3:
			return "Acceleration"
		case 4:
			return "Slow/Fast Switch"
		default:
			return "?"
		}
	case 30: // Auto Wah
		switch paramNumber {
		case 1:
			return "Sense"
		case 2:
			return "Frequency Bottom"
		case 3:
			return "Frequency Top"
		case 4:
			return "Resonance"
		default:
			return "?"
		}
	case 31: // Bandpass
		switch paramNumber {
		case 1:
			return "Center Frequency"
		case 2:
			return "Bandwidth"
		default:
			return "?"
		}
	case 32, 33: // Exciter, Enhancer
		switch paramNumber {
		case 1:
			return "EQ Low"
		case 2:
			return "EQ High"
		case 3:
			return "Intensity"
		default:
			return "?"
		}
	case 34, 35: // Overdrive, Distortion
		switch paramNumber {
		case 1:
			return "EQ Low"
		case 2:
			return "EQ High"
		case 3:
			return "Output Level"
		case 4:
			return "Drive"
		default:
			return "?"
		}
	case 36, 37: // Overdrive & Delay; Distortion & Delay
		switch paramNumber {
		case 1:
			return "EQ Low"
		case 2:
			return "EQ High"
		case 3:
			return "Delay Time"
		case 4:
			return "Drive"
		default:
			return "?"
		}
	default:
		return "?"
	}
}

func getEffect(data []byte) Effect {
	effectType := 0
	if data[0] != 0 {
		effectType = int(data[0]) - 11
	}

	return Effect{
		EffectType:   effectType,
		EffectDepth:  int(data[1]),
		EffectParam1: int(data[2]),
		EffectParam2: int(data[3]),
		EffectParam3: int(data[4]),
		EffectParam4: int(data[5]),
	}
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

func (g GEQ) String() string {
	return fmt.Sprintf("%d %d %d %d %d %d %d",
		g.Freq1,
		g.Freq2,
		g.Freq3,
		g.Freq4,
		g.Freq5,
		g.Freq6,
		g.Freq7)
}

// EnvelopeSettings describes the parameters of an envelope.
type EnvelopeSettings struct {
	AttackTime  int
	Decay1Time  int
	Decay1Level int
	Decay2Time  int
	Decay2Level int
	ReleaseTime int
}

type Modulation struct {
	Destination int
	Depth       int
}

type ModulationTarget struct {
	Target1 Modulation
	Target2 Modulation
}

type AssignableModulationTarget struct {
	Source           int
	ModulationTarget // NOTE: embedded struct
}

type PanSettings struct {
	PanType  int
	PanValue int
}

type FilterKeyScalingToEnvelope struct {
	AttackTime int
	Decay1Time int
}

type FilterVelocityToEnvelope struct {
	EnvelopeDepth int
	AttackTime    int
	Decay1Time    int
}

type FilterSettings struct {
	Bypass                bool
	Mode                  int
	VelocityCurve         int
	Resonance             int
	Level                 int
	Cutoff                int
	CutoffKeyScalingDepth int
	CutoffVelocityDepth   int
	EnvelopeDepth         int
	Envelope              EnvelopeSettings
	KeyScalingToEnvelope  FilterKeyScalingToEnvelope
	VelocityToEnvelope    FilterVelocityToEnvelope
}

type AmplifierKeyScalingToEnvelope struct {
	Level       int
	AttackTime  int
	Decay1Time  int
	ReleaseTime int
}

type AmplifierVelocitySensitivity struct {
	Level       int
	AttackTime  int
	Decay1Time  int
	ReleaseTime int
}

// Note that AmplifierKeyScalingToEnvelope and AmplifierVelocitySensitivity are essentially the same type.

type AmplifierSettings struct {
	VelocityCurve        int
	Envelope             EnvelopeSettings
	KeyScalingToEnvelope AmplifierKeyScalingToEnvelope
	VelocitySensitivity  AmplifierVelocitySensitivity
}

type LFOFadeInSettings struct {
	Time    int
	ToSpeed int
}

type LFOModulationSettings struct {
	Depth      int
	KeyScaling int
}

type LFOSettings struct {
	Waveform   int
	Speed      int
	DelayOnset int
	FadeIn     LFOFadeInSettings
	Vibrato    LFOModulationSettings
	Growl      LFOModulationSettings
	Tremolo    LFOModulationSettings
}

// Source represents the data of one of the up to six patch sources.
type Source struct {
	ZoneLow           int
	ZoneHigh          int
	VelocitySwitching int // bits 5-6: 0=off, 1=loud, 2=soft. bits 0-4: velo 0=4 ... 31=127 (?)
	EffectPath        int
	Volume            int
	BenderPitch       int
	BenderCutoff      int
	Pressure          ModulationTarget
	Wheel             ModulationTarget
	Expression        ModulationTarget
	Assignable1       AssignableModulationTarget
	Assignable2       AssignableModulationTarget
	KeyOnDelay        int
	Pan               PanSettings

	Filter    FilterSettings
	Amplifier AmplifierSettings
	LFO       LFOSettings
}

// Use struct embedding to avoid clash between member and type name.
// See https://notes.shichao.io/gopl/ch4/#struct-embedding-and-anonymous-fields
// It's probably a good idea to use unique names for the fields in the struct-to-embed.

// Common stores the common parameters for a patch.
type Common struct {
	EffectAlgorithm int
	Reverb          // this means that there is a Reverb struct as a part of Common ("struct embedding")
	Effect1         Effect
	Effect2         Effect
	Effect3         Effect
	Effect4         Effect
	GEQ
	Name        string
	Volume      int
	Polyphony   int
	SourceCount int
	SourceMutes [numSources]bool // "Go does not provide a set type, but since the keys of a map are distinct, a map can serve this pur pose." TGPL p. 96

	// AM: Selects sources for Amplitude Modulation. One source can be set to modulate an adjacent source, i.e., 1>2.
	AmplitudeModulation int
}

func (c Common) String() string {
	return fmt.Sprintf("%8s  vol=%3d  poly=%d\nnumber of sources = %d\neffect algorithm = %d", c.Name, c.Volume, c.Polyphony, c.SourceCount, c.EffectAlgorithm)
}

// Patch represents the parameters of a sound.
type Patch struct {
	Common
	Sources [numSources]Source
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
	gEQOffset             = 32
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

		reverb := getReverb(d[reverbOffset : reverbOffset+6])

		effect1 := getEffect(d[effect1Offset : effect1Offset+6])
		effect2 := getEffect(d[effect2Offset : effect2Offset+6])
		effect3 := getEffect(d[effect3Offset : effect3Offset+6])
		effect4 := getEffect(d[effect4Offset : effect4Offset+6])

		geq := GEQ{
			Freq1: int(d[gEQOffset] - 64),
			Freq2: int(d[gEQOffset+1] - 64),
			Freq3: int(d[gEQOffset+2] - 64),
			Freq4: int(d[gEQOffset+3] - 64),
			Freq5: int(d[gEQOffset+4] - 64),
			Freq6: int(d[gEQOffset+5] - 64),
			Freq7: int(d[gEQOffset+6] - 64),
		}

		sourceOffset := commonDataSize // source data starts after common data
		var sources [numSources]Source
		for sourceIndex := 0; sourceIndex < numSources; sourceIndex++ {
			if sourceIndex < sourceCount {
				newSource := Source{
					ZoneLow:           int(d[sourceOffset]),
					ZoneHigh:          int(d[sourceOffset+1]),
					VelocitySwitching: int(d[sourceOffset+2]),
					EffectPath:        int(d[sourceOffset+3]),
					Volume:            int(d[sourceOffset+4]),
					BenderPitch:       int(d[sourceOffset+5]),
					BenderCutoff:      int(d[sourceOffset+6]),
					Pressure: ModulationTarget{
						Target1: Modulation{
							Destination: int(d[sourceOffset+7]),
							Depth:       int(d[sourceOffset+8]),
						},
						Target2: Modulation{
							Destination: int(d[sourceOffset+9]),
							Depth:       int(d[sourceOffset+10]),
						},
					},
					Wheel: ModulationTarget{
						Target1: Modulation{
							Destination: int(d[sourceOffset+11]),
							Depth:       int(d[sourceOffset+12]),
						},
						Target2: Modulation{
							Destination: int(d[sourceOffset+13]),
							Depth:       int(d[sourceOffset+14]),
						},
					},
					Expression: ModulationTarget{
						Target1: Modulation{
							Destination: int(d[sourceOffset+15]),
							Depth:       int(d[sourceOffset+16]),
						},
						Target2: Modulation{
							Destination: int(d[sourceOffset+17]),
							Depth:       int(d[sourceOffset+18]),
						},
					},
					Assignable1: AssignableModulationTarget{
						Source: int(d[sourceOffset+19]),
						ModulationTarget: ModulationTarget{
							Target1: Modulation{
								Destination: int(d[sourceOffset+20]),
								Depth:       int(d[sourceOffset+21]),
							},
							Target2: Modulation{
								Destination: int(d[sourceOffset+22]),
								Depth:       int(d[sourceOffset+23]),
							},
						},
					},
					Assignable2: AssignableModulationTarget{
						Source: int(d[sourceOffset+24]),
						ModulationTarget: ModulationTarget{
							Target1: Modulation{
								Destination: int(d[sourceOffset+25]),
								Depth:       int(d[sourceOffset+26]),
							},
							Target2: Modulation{
								Destination: int(d[sourceOffset+27]),
								Depth:       int(d[sourceOffset+28]),
							},
						},
					},
					KeyOnDelay: int(d[sourceOffset+29]),
					Pan: PanSettings{
						PanType:  int(d[sourceOffset+30]),
						PanValue: int(d[sourceOffset+31]),
					},
					Filter: FilterSettings{
						Bypass:                int(d[sourceOffset+32]) == 1,
						Mode:                  int(d[sourceOffset+33]),
						VelocityCurve:         int(d[sourceOffset+33]),
						Resonance:             int(d[sourceOffset+34]),
						Level:                 int(d[sourceOffset+35]),
						Cutoff:                int(d[sourceOffset+36]),
						CutoffKeyScalingDepth: int(d[sourceOffset+37]),
						CutoffVelocityDepth:   int(d[sourceOffset+38]),
						EnvelopeDepth:         int(d[sourceOffset+39]),
						Envelope: EnvelopeSettings{
							AttackTime:  int(d[sourceOffset+40]),
							Decay1Time:  int(d[sourceOffset+41]),
							Decay1Level: int(d[sourceOffset+42]),
							Decay2Time:  int(d[sourceOffset+43]),
							Decay2Level: int(d[sourceOffset+44]),
							ReleaseTime: int(d[sourceOffset+45]),
						},
						KeyScalingToEnvelope: FilterKeyScalingToEnvelope{
							AttackTime: int(d[sourceOffset+46]),
							Decay1Time: int(d[sourceOffset+47]),
						},
						VelocityToEnvelope: FilterVelocityToEnvelope{
							EnvelopeDepth: int(d[sourceOffset+48]),
							AttackTime:    int(d[sourceOffset+49]),
							Decay1Time:    int(d[sourceOffset+50]),
						},
					},
					Amplifier: AmplifierSettings{
						VelocityCurve: int(d[sourceOffset+51]),
						Envelope: EnvelopeSettings{
							AttackTime:  int(d[sourceOffset+52]),
							Decay1Time:  int(d[sourceOffset+53]),
							Decay1Level: int(d[sourceOffset+54]),
							Decay2Time:  int(d[sourceOffset+55]),
							Decay2Level: int(d[sourceOffset+56]),
							ReleaseTime: int(d[sourceOffset+57]),
						},
						KeyScalingToEnvelope: AmplifierKeyScalingToEnvelope{
							Level:       int(d[sourceOffset+58]),
							AttackTime:  int(d[sourceOffset+59]),
							Decay1Time:  int(d[sourceOffset+60]),
							ReleaseTime: int(d[sourceOffset+61]),
						},
						VelocitySensitivity: AmplifierVelocitySensitivity{
							Level:       int(d[sourceOffset+62]),
							AttackTime:  int(d[sourceOffset+63]),
							Decay1Time:  int(d[sourceOffset+64]),
							ReleaseTime: int(d[sourceOffset+65]),
						},
					},
					LFO: LFOSettings{
						Waveform:   int(d[sourceOffset+66]),
						Speed:      int(d[sourceOffset+67]),
						DelayOnset: int(d[sourceOffset+68]),
						FadeIn: LFOFadeInSettings{
							Time:    int(d[sourceOffset+69]),
							ToSpeed: int(d[sourceOffset+70]),
						},
						Vibrato: LFOModulationSettings{
							Depth:      int(d[sourceOffset+71]),
							KeyScaling: int(d[sourceOffset+72]),
						},
						Growl: LFOModulationSettings{
							Depth:      int(d[sourceOffset+73]),
							KeyScaling: int(d[sourceOffset+74]),
						},
						Tremolo: LFOModulationSettings{
							Depth:      int(d[sourceOffset+75]),
							KeyScaling: int(d[sourceOffset+76]),
						},
					},
				}
				sources[sourceIndex] = newSource
				sourceOffset += sourceDataSize
			}
		}

		// With struct embedding, the literal must follow the shape of the type declaration. (TGPL, p. 106)
		c := Common{
			Name:            name,
			SourceCount:     sourceCount,
			Reverb:          reverb,
			Effect1:         effect1,
			Effect2:         effect2,
			Effect3:         effect3,
			Effect4:         effect4,
			Volume:          volume,
			Polyphony:       polyphony,
			EffectAlgorithm: effectAlgorithm,
			GEQ:             geq,
		}

		patch := Patch{Common: c, Sources: sources}
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
