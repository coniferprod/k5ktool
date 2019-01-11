// Package bank contains type definitions and utility functions for a sound bank.
package bank

const (
	// NumPatches is the number of patches in a sound bank.
	NumPatches = 128
)

// Patch represents the parameters of a sound.
type Patch struct {
	Name string
}

// Bank contains 128 patches.
type Bank struct {
	patches [NumPatches]Patch
}
