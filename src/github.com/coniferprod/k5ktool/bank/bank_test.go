package bank

import "testing"

func TestGetEffect(t *testing.T) {
	data := []byte{11, 1, 2, 3, 4, 5}
	effect := getEffect(data)
	if effect.EffectType != 0 {
		t.Error("Effect type should be 0")
	}
}
