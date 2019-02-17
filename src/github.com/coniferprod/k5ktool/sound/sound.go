package sound

import (
	"math"

	"github.com/coniferprod/k5ktool/bank"
)

type leiterParameters struct {
	A  float64
	B  float64
	C  float64
	Xp float64
	D  float64
	E  float64
	Yp float64
}

var waveformParameters = map[string]leiterParameters{
	"saw": leiterParameters{
		A:  1.0,
		B:  0.0,
		C:  0.0,
		Xp: 0.0,
		D:  0.0,
		E:  0.0,
		Yp: 0.0,
	},
	"square": leiterParameters{
		A:  1.0,
		B:  1.0,
		C:  0.0,
		Xp: 0.5,
		D:  0.0,
		E:  0.0,
		Yp: 0.0,
	},
	"triangle": leiterParameters{
		A:  2.0,
		B:  1.0,
		C:  0.0,
		Xp: 0.5,
		D:  0.0,
		E:  0.0,
		Yp: 0.0,
	},
}

var harmonicTemplates = map[string][]byte{
	"sawsoft":  []byte{127, 124, 121, 118, 115, 112, 109, 106, 103, 100, 97, 94, 91, 88, 85, 82, 79, 76, 73, 70, 67, 64, 61, 58, 55, 52, 49, 46, 43, 40, 37, 34, 31, 28, 24, 22, 19, 16, 13, 10, 7, 4, 1},
	"triangle": []byte{127, 0, 85, 0, 47, 0, 20, 0, 4, 0},
	"vibes":    []byte{127, 0, 0, 115, 0, 0, 0, 0, 0, 0, 103, 0, 0, 0, 0, 0, 0, 0, 106, 0, 0, 0, 0, 0, 0, 106, 0, 0, 0, 0, 0, 0, 0, 97, 0, 0, 0, 0, 0, 0, 101, 0, 0, 0, 0, 0, 0, 0, 73, 0, 0, 0, 0, 0, 0, 61, 0, 0, 0, 0, 0, 0, 0, 32},
}

func leiter(n int, params leiterParameters) float64 {
	a, b, c, xp, d, e, yp := params.A, params.B, params.C, params.Xp, params.D, params.E, params.Yp

	x := float64(n) * math.Pi * xp
	y := float64(n) * math.Pi * yp

	module1 := 1.0 / math.Pow(float64(n), a)
	module2 := math.Pow(math.Sin(x), b) * math.Pow(math.Cos(x), c)
	module3 := math.Pow(math.Sin(y), d) * math.Pow(math.Cos(y), e)

	return module1 * module2 * module3
}

func getHarmonicLevel(harmonic int, params leiterParameters) float64 {
	aMax := 1.0
	a := leiter(harmonic, params)
	v := math.Log2(math.Abs(a / aMax))
	level := 127 + 8*v
	if level < 0 {
		return 0
	}
	return level
}

func NewHarmonicLevels(name string) bank.HarmonicLevels {
	params := waveformParameters[name]
	var levels bank.HarmonicLevels
	for n := 0; n < 64; n++ {
		level := getHarmonicLevel(n+1, params)
		levels[n] = byte(math.Floor(level))
		//fmt.Printf("%2d: %d %f\n", n + 1, int(math.Floor(level)), level)
	}
	return levels
}
