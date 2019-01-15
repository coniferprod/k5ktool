// Package bank contains type definitions and utility functions for a sound bank.
package bank

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
	Type   int // 0...10
	DryWet int
	Param1 int
	Param2 int
	Param3 int
	Param4 int
}

// Description returns a textual description of the reverb.
func (r Reverb) Description() string {
	s := reverbNames[r.Type]
	return s
}

// Effect stores the effect settings of a patch.
type Effect struct {
	Type   int
	Depth  int
	Param1 int
	Param2 int
	Param3 int
	Param4 int
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

// Common stores the common parameters for a patch.
type Common struct {
	Name        string
	SourceCount int
	Reverb
	Volume          int
	Polyphony       int
	EffectAlgorithm int
	Effects         [NumEffects]Effect
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
