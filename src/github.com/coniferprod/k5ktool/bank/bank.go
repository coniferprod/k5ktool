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

// Reverb stores the reverb parameters of the patch.
type Reverb struct {
	ReverbType   int // 0...10
	ReverbDryWet int
	ReverbParam1 int
	ReverbParam2 int
	ReverbParam3 int
	ReverbParam4 int
}

type ReverbName struct {
	Name           string
	ParameterNames [4]string
}

var reverbNames = []ReverbName{
	/*  0 */ ReverbName{Name: "Hall 1", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  1 */ ReverbName{Name: "Hall 2", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  2 */ ReverbName{Name: "Hall 3", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  3 */ ReverbName{Name: "Room 1", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  4 */ ReverbName{Name: "Room 2", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  5 */ ReverbName{Name: "Room 3", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  6 */ ReverbName{Name: "Plate 1", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  7 */ ReverbName{Name: "Plate 2", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  8 */ ReverbName{Name: "Plate 3", ParameterNames: [4]string{"Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping"}},
	/*  9 */ ReverbName{Name: "Reverse", ParameterNames: [4]string{"Dry/Wet 2", "Feedback", "Predelay Time", "High Frequency Damping"}},
	/* 10 */ ReverbName{Name: "Long Delay", ParameterNames: [4]string{"Dry/Wet 2", "Feedback", "Delay Time", "High Frequency Damping"}},
}

func (r Reverb) String() string {
	names := reverbNames[r.ReverbType]
	return fmt.Sprintf("%s, %d%% wet, %s = %d, %s = %d, %s = %d, %s = %d",
		names.Name,
		r.ReverbDryWet,
		names.ParameterNames[0], r.ReverbParam1,
		names.ParameterNames[1], r.ReverbParam2,
		names.ParameterNames[2], r.ReverbParam3,
		names.ParameterNames[3], r.ReverbParam4)
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

type EffectName struct {
	Name           string
	ParameterNames [4]string
}

var effectNames = []EffectName{
	/*  0 */ EffectName{Name: "Early Reflection 1", ParameterNames: [4]string{"Slope", "Predelay Time", "Feedback", "?"}},
	/*  1 */ EffectName{Name: "Early Reflection 2", ParameterNames: [4]string{"Slope", "Predelay Time", "Feedback", "?"}},
	/*  2 */ EffectName{Name: "Tap Delay 1", ParameterNames: [4]string{"Delay Time 1", "Tap Level", "Delay Time 2", "?"}},
	/*  3 */ EffectName{Name: "Tap Delay 2", ParameterNames: [4]string{"Delay Time 1", "Tap Level", "Delay Time 2", "?"}},
	/*  4 */ EffectName{Name: "Single Delay", ParameterNames: [4]string{"Delay Time Fine", "Delay Time Coarse", "Feedback", "?"}},
	/*  5 */ EffectName{Name: "Dual Delay", ParameterNames: [4]string{"Delay Time Left", "Feedback Left", "Delay Time Right", "Feedback Right"}},
	/*  6 */ EffectName{Name: "Stereo Delay", ParameterNames: [4]string{"Delay Time", "Feedback", "?", "?"}},
	/*  7 */ EffectName{Name: "Cross Delay", ParameterNames: [4]string{"Delay Time", "Feedback", "?", "?"}},
	/*  8 */ EffectName{Name: "Auto Pan", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Wave"}},
	/*  9 */ EffectName{Name: "Auto Pan & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Wave"}},
	/* 10 */ EffectName{Name: "Chorus 1", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Wave"}},
	/* 11 */ EffectName{Name: "Chorus 2", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Wave"}},
	/* 12 */ EffectName{Name: "Chorus 1 & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Wave"}},
	/* 13 */ EffectName{Name: "Chorus 2 & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Wave"}},
	/* 14 */ EffectName{Name: "Flanger 1", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Feedback"}},
	/* 15 */ EffectName{Name: "Flanger 2", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Feedback"}},
	/* 16 */ EffectName{Name: "Flanger 1 & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Feedback"}},
	/* 17 */ EffectName{Name: "Flanger 2 & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Feedback"}},
	/* 18 */ EffectName{Name: "Ensemble", ParameterNames: [4]string{"Depth", "Predelay Time", "?", "?"}},
	/* 19 */ EffectName{Name: "Ensemble & Delay", ParameterNames: [4]string{"Depth", "Delay Time", "?", "?"}},
	/* 20 */ EffectName{Name: "Celeste", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "?"}},
	/* 21 */ EffectName{Name: "Celeste & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "?"}},
	/* 22 */ EffectName{Name: "Tremolo", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Wave"}},
	/* 23 */ EffectName{Name: "Tremolo & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Wave"}},
	/* 24 */ EffectName{Name: "Phaser 1", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Feedback"}},
	/* 25 */ EffectName{Name: "Phaser 2", ParameterNames: [4]string{"Speed", "Depth", "Predelay Time", "Feedback"}},
	/* 26 */ EffectName{Name: "Phaser 1 & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Feedback"}},
	/* 27 */ EffectName{Name: "Phaser 2 & Delay", ParameterNames: [4]string{"Speed", "Depth", "Delay Time", "Feedback"}},
	/* 28 */ EffectName{Name: "Rotary", ParameterNames: [4]string{"Slow Speed", "Fast Speed", "Acceleration", "Slow/Fast Switch"}},
	/* 29 */ EffectName{Name: "Auto Wah", ParameterNames: [4]string{"Sense", "Frequency Bottom", "Frequency Top", "Resonance"}},
	/* 30 */ EffectName{Name: "Bandpass", ParameterNames: [4]string{"Center Frequency", "Bandwidth", "?", "?"}},
	/* 31 */ EffectName{Name: "Exciter", ParameterNames: [4]string{"EQ Low", "EQ High", "Intensity", "?"}},
	/* 32 */ EffectName{Name: "Enhancer", ParameterNames: [4]string{"EQ Low", "EQ High", "Intensity", "?"}},
	/* 33 */ EffectName{Name: "Overdrive", ParameterNames: [4]string{"EQ Low", "EQ High", "Output Level", "Drive"}},
	/* 34 */ EffectName{Name: "Distortion", ParameterNames: [4]string{"EQ Low", "EQ High", "Output Level", "Drive"}},
	/* 35 */ EffectName{Name: "Overdrive & Delay", ParameterNames: [4]string{"EQ Low", "EQ High", "Delay Time", "Drive"}},
	/* 36 */ EffectName{Name: "Distortion & Delay", ParameterNames: [4]string{"EQ Low", "EQ High", "Delay Time", "Drive"}},
}

// There seems to be a conflict in the manual: there are 37 effect names,
// but the number of effects is reported to be 36.
// Cross-check this with the actual synth.

// Description returns a textual description of the effect.
func (e Effect) Description() string {
	s := effectNames[e.EffectType].Name
	return s
}

func (e Effect) String() string {
	names := effectNames[e.EffectType]
	return fmt.Sprintf("%02d %s, depth = %d, %s = %d, %s = %d, %s = %d, %s = %d",
		e.EffectType,
		names.Name,
		e.EffectDepth,
		names.ParameterNames[0], e.EffectParam1,
		names.ParameterNames[1], e.EffectParam2,
		names.ParameterNames[2], e.EffectParam3,
		names.ParameterNames[3], e.EffectParam4)
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

func getGEQ(d []byte) GEQ {
	return GEQ{
		Freq1: int(d[0] - 64),
		Freq2: int(d[1] - 64),
		Freq3: int(d[2] - 64),
		Freq4: int(d[3] - 64),
		Freq5: int(d[4] - 64),
		Freq6: int(d[5] - 64),
		Freq7: int(d[6] - 64),
	}
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
	PortamentoEnabled   bool
	PortamentoSpeed     int
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
	portamentoOffset      = 60
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
		effectAlgorithm := int(d[effectAlgorithmOffset]) + 1 // value is 0...3, scale to 1...4

		reverb := getReverb(d[reverbOffset : reverbOffset+6])

		effect1 := getEffect(d[effect1Offset : effect1Offset+6])
		effect2 := getEffect(d[effect2Offset : effect2Offset+6])
		effect3 := getEffect(d[effect3Offset : effect3Offset+6])
		effect4 := getEffect(d[effect4Offset : effect4Offset+6])

		geq := getGEQ(d[gEQOffset : gEQOffset+7])

		portamentoFlag := int(d[portamentoOffset]) == 1
		portamentoSpeed := int(d[portamentoOffset+1])

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
			Name:              name,
			SourceCount:       sourceCount,
			Reverb:            reverb,
			Effect1:           effect1,
			Effect2:           effect2,
			Effect3:           effect3,
			Effect4:           effect4,
			Volume:            volume,
			Polyphony:         polyphony,
			EffectAlgorithm:   effectAlgorithm,
			GEQ:               geq,
			PortamentoEnabled: portamentoFlag,
			PortamentoSpeed:   portamentoSpeed,
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
