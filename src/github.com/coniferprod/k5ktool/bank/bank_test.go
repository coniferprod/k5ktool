package bank

import "testing"

func TestGetEffectType(t *testing.T) {
	data := []byte{0x11, 0x05, 0x50, 0x1F, 0x00, 0x00}
	effect := newEffect(data)
	if effect.EffectType != 6 {
		t.Error("Effect type should be 6")
	}
}

func TestGetEffectName(t *testing.T) {
	data := []byte{0x11, 0x05, 0x50, 0x1F, 0x00, 0x00}
	effect := newEffect(data)
	name := effect.Description()
	if name != "Stereo Delay" {
		t.Error("Effect name should be 'Stereo Delay'")
	}
}
