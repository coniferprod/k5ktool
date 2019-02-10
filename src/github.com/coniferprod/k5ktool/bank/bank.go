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

	// NumEffects is the number of effects in a patch.
	NumEffects = 4

	// NumWaves is the number of PCM waveforms in the wave list.
	NumWaves = 341
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

func newReverb(data []byte) Reverb {
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

func newEffect(data []byte) Effect {
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

func newGEQ(d []byte) GEQ {
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

func (e EnvelopeSettings) String() string {
	return fmt.Sprintf("A T%d, D1 T%d L%d, D2 T%d L%d, R T%d",
		e.AttackTime, e.Decay1Time, e.Decay1Level, e.Decay2Time, e.Decay2Level, e.ReleaseTime)
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
	Source     int
	Modulation // NOTE: embedded struct
}

// PanSettings stores the pan settings of the source
type PanSettings struct {
	PanType  int
	PanValue int
}

// PitchEnvelopeSettings represents a pitch envelope
type PitchEnvelopeSettings struct {
	StartLevel  int
	AttackTime  int
	AttackLevel int
	DecayTime   int
}

// OscillatorSettings stores the DCO parameters of the source
type OscillatorSettings struct {
	WaveKit         int
	Coarse          int
	Fine            int
	FixedKey        int
	KSPitch         int
	PitchEnvelope   PitchEnvelopeSettings
	VelocityToTime  int
	VelocityToLevel int
}

var waveNames = [NumWaves]string{
	/*  1 */ "OldUprit1",
	/*  2 */ "OldUprit2",
	/*  3 */ "Gr.Piano",
	/*  4 */ "WidPiano",
	/*  5 */ "Br.Piano",
	/*  6 */ "Hnkytonk1",
	/*  7 */ "E.Grand1",
	/*  8 */ "Hnkytonk2",
	/*  9 */ "E.Grand2",
	/*  10 */ "E.Grand3",
	/*  11 */ "Metallic1",
	/*  12 */ "E.Piano1",
	/*  13 */ "60's EP",
	/*  14 */ "E.Piano2",
	/*  15 */ "E.Piano3",
	/*  16 */ "E.Piano4",
	/*  17 */ "Clavi 1",
	/*  18 */ "Drawbar1",
	/*  19 */ "Drawbar2",
	/*  20 */ "DetunOr1",
	/*  21 */ "Drawbar3",
	/*  22 */ "PercOrg1",
	/*  23 */ "PercOrg2",
	/*  24 */ "ChrcOrg1",
	/*  25 */ "ChrcOrg2",
	/*  26 */ "Celesta1",
	/*  27 */ "Vibe",
	/*  28 */ "Glocken1",
	/*  29 */ "Marimba",
	/*  30 */ "Glocken2",
	/*  31 */ "NewAge1",
	/*  32 */ "Xylophon",
	/*  33 */ "TubulBel",
	/*  34 */ "Stl Drum",
	/*  35 */ "Timpani1",
	/*  36 */ "CncertBD1",
	/*  37 */ "NylonGt1",
	/*  38 */ "Ukulele",
	/*  39 */ "NylonGt2",
	/*  40 */ "Nyln+Stl",
	/*  41 */ "Atmosphr1",
	/*  42 */ "SteelGt1",
	/*  43 */ "Sci-Fi1",
	/*  44 */ "Mandolin1",
	/*  45 */ "Mandolin2",
	/*  46 */ "SteelGt2",
	/*  47 */ "12strGtr1",
	/*  48 */ "12strGtr2",
	/*  49 */ "Dulcimer1",
	/*  50 */ "JazzGtr1",
	/*  51 */ "CleanGtr1",
	/*  52 */ "Hi.E.Gtr1",
	/*  53 */ "ChorusGt",
	/*  54 */ "TubeBass1",
	/*  55 */ "CleanGtr2",
	/*  56 */ "Hi.E.Gtr2",
	/*  57 */ "MuteGtr1",
	/*  58 */ "OvrDrive1",
	/*  59 */ "ResO.D.1",
	/*  60 */ "OvrDrive2",
	/*  61 */ "ResO.D.2",
	/*  62 */ "Distortd",
	/*  63 */ "Charang1",
	/*  64 */ "Charang2",
	/*  65 */ "FeedbkGt1",
	/*  66 */ "PowerGtr1",
	/*  67 */ "Res.Dist",
	/*  68 */ "RockOrgn1",
	/*  69 */ "PowerGtr2",
	/*  70 */ "Harmnics",
	/*  71 */ "Dulcimer2",
	/*  72 */ "Banjo",
	/*  73 */ "Sitar",
	/*  74 */ "Shamisen",
	/*  75 */ "Koto",
	/*  76 */ "TaishoKt1",
	/*  77 */ "TaishoKt2",
	/*  78 */ "Harp1",
	/*  79 */ "Harp2",
	/*  80 */ "Ac.Bass1",
	/*  81 */ "Ac.Bass2",
	/*  82 */ "Ac.Bass3",
	/*  83 */ "FngBass1",
	/*  84 */ "Ac.Bass4",
	/*  85 */ "FngBass2",
	/*  86 */ "TubeBass2",
	/*  87 */ "PickBass1",
	/*  88 */ "MutePick1",
	/*  89 */ "PickBass2",
	/*  90 */ "MutePick2",
	/*  91 */ "Fretless",
	/*  92 */ "SlapBas1",
	/*  93 */ "FunkGtr1",
	/*  94 */ "FunkGtr2",
	/*  95 */ "SlapBas2",
	/*  96 */ "SlapBas3",
	/*  97 */ "SlapBas4",
	/*  98 */ "SynBass1",
	/*  99 */ "SynBass2",
	/* 100 */ "SynBass3",
	/* 101 */ "SynBass4",
	/* 102 */ "HouseBass1",
	/* 103 */ "HouseBass2",
	/* 104 */ "SynBass5",
	/* 105 */ "Violn",
	/* 106 */ "Fiddle",
	/* 107 */ "SlwVioln",
	/* 108 */ "Viola",
	/* 109 */ "Cello",
	/* 110 */ "Contra1",
	/* 111 */ "Contra2",
	/* 112 */ "Strings1",
	/* 113 */ "Strings2",
	/* 114 */ "Orchstra1",
	/* 115 */ "Strings3",
	/* 116 */ "Strings4",
	/* 117 */ "Bright1",
	/* 118 */ "Atmosphr2",
	/* 119 */ "Sweep1",
	/* 120 */ "Pizzicto1",
	/* 121 */ "Pizzicto2",
	/* 122 */ "SynStrg1",
	/* 123 */ "SynBras1",
	/* 124 */ "SynStrg2",
	/* 125 */ "Poly Syn1",
	/* 126 */ "Rain1",
	/* 127 */ "Soundtrk1",
	/* 128 */ "Soundtrk2",
	/* 129 */ "SynBass5",
	/* 130 */ "SynStrg3",
	/* 131 */ "SynStrg4",
	/* 132 */ "SynBras2",
	/* 133 */ "SynBras3",
	/* 134 */ "Chiff1",
	/* 135 */ "Fifth1",
	/* 136 */ "Fifth2",
	/* 137 */ "Metallic2",
	/* 138 */ "Sci-Fi2",
	/* 139 */ "ChorAah1",
	/* 140 */ "Voice1",
	/* 141 */ "Halo Pad1",
	/* 142 */ "Echoes",
	/* 143 */ "ChorAah2",
	/* 144 */ "ChorAah3",
	/* 145 */ "Sweep2",
	/* 146 */ "RockOrgn2",
	/* 147 */ "Choir1",
	/* 148 */ "Halo Pad2",
	/* 149 */ "Chiff2",
	/* 150 */ "Bright2",
	/* 151 */ "Voi Ooh1",
	/* 152 */ "SynVoice",
	/* 153 */ "NewAge2",
	/* 154 */ "Choir2",
	/* 155 */ "Goblns1",
	/* 156 */ "Voi Ooh2",
	/* 157 */ "Orchstra2",
	/* 158 */ "Oct.Bras1",
	/* 159 */ "BrasSect1",
	/* 160 */ "Brass1",
	/* 161 */ "Oct.Bras2",
	/* 162 */ "Orch Hit1",
	/* 163 */ "Orch Hit2",
	/* 164 */ "WarmTrmp",
	/* 165 */ "Trumpet",
	/* 166 */ "Tuba1",
	/* 167 */ "DublBone",
	/* 168 */ "Tuba2",
	/* 169 */ "TromBone",
	/* 170 */ "BrasSect2",
	/* 171 */ "Mute Tp",
	/* 172 */ "FrenchHr1",
	/* 173 */ "FrenchHr2",
	/* 174 */ "SprnoSax",
	/* 175 */ "Bassoon1",
	/* 176 */ "AltoSax1",
	/* 177 */ "AltoSax2",
	/* 178 */ "TenorSax1",
	/* 179 */ "BrthTenr1",
	/* 180 */ "Brass2",
	/* 181 */ "Bari Sax",
	/* 182 */ "EnglHorn",
	/* 183 */ "Bassoon2",
	/* 184 */ "Oboe",
	/* 185 */ "Winds1",
	/* 186 */ "Winds2",
	/* 187 */ "Shanai1",
	/* 188 */ "Bag Pipe1",
	/* 189 */ "Clarinet1",
	/* 190 */ "Winds3",
	/* 191 */ "Flute1",
	/* 192 */ "Winds4",
	/* 193 */ "Calliope1",
	/* 194 */ "Flute2",
	/* 195 */ "Piccolo1",
	/* 196 */ "PanFlute1",
	/* 197 */ "Bottle",
	/* 198 */ "Calliope2",
	/* 199 */ "Voice2",
	/* 200 */ "Shakhach",
	/* 201 */ "Kalimba1",
	/* 202 */ "Agogo",
	/* 203 */ "WoodBlok",
	/* 204 */ "Melo.Tom",
	/* 205 */ "Syn.Drum",
	/* 206 */ "E.Percus",
	/* 207 */ "Scratch",
	/* 208 */ "E.Tom1",
	/* 209 */ "E.Tom2",
	/* 210 */ "Castanet",
	/* 211 */ "TaikoDrm",
	/* 212 */ "RevCymb1",
	/* 213 */ "WndChime",
	/* 214 */ "BrthNoiz",
	/* 215 */ "Flute3",
	/* 216 */ "Recorder1",
	/* 217 */ "PanFlute2",
	/* 218 */ "Ocarina1",
	/* 219 */ "Flute4",
	/* 220 */ "DrawBar4",
	/* 221 */ "Piccolo2",
	/* 222 */ "TenorSax2",
	/* 223 */ "BrthTenr2",
	/* 224 */ "Seashore",
	/* 225 */ "Wind",
	/* 226 */ "FretNoiz",
	/* 227 */ "GtCtNiz1",
	/* 228 */ "GtCtNiz2",
	/* 229 */ "StrgSlap",
	/* 230 */ "Rain2",
	/* 231 */ "Thunder",
	/* 232 */ "Stream1",
	/* 233 */ "Stream2",
	/* 234 */ "Bubble",
	/* 235 */ "Bird1",
	/* 236 */ "Bird2",
	/* 237 */ "Dog",
	/* 238 */ "HorseGalp",
	/* 239 */ "Tel1",
	/* 240 */ "DoorCreak",
	/* 241 */ "Door",
	/* 242 */ "Helicopter",
	/* 243 */ "CarEngine",
	/* 244 */ "CarStop",
	/* 245 */ "CarPass",
	/* 246 */ "CarCrash",
	/* 247 */ "Siren",
	/* 248 */ "Train",
	/* 249 */ "JetPlan",
	/* 250 */ "StarShip",
	/* 251 */ "Applause1",
	/* 252 */ "Applause2",
	/* 253 */ "Laughing",
	/* 254 */ "Screaming",
	/* 255 */ "Punch",
	/* 256 */ "HeartBeat",
	/* 257 */ "FootStep",
	/* 258 */ "Gun",
	/* 259 */ "MachinGun",
	/* 260 */ "LaserGun",
	/* 261 */ "Explosion",
	/* 262 */ "Omni1",
	/* 263 */ "Omni2",
	/* 264 */ "Rain3",
	/* 265 */ "MuteGtr2",
	/* 266 */ "MusicBox1",
	/* 267 */ "Sine",
	/* 268 */ "Bowed1",
	/* 269 */ "ConcrtBD2",
	/* 270 */ "FngBass3",
	/* 271 */ "FeedbkGt2",
	/* 272 */ "Timpani2",
	/* 273 */ "SawLead1",
	/* 274 */ "Dr.Solo1",
	/* 275 */ "Dr.Solo2",
	/* 276 */ "SawLead2",
	/* 277 */ "DistClav1",
	/* 278 */ "DistClav2",
	/* 279 */ "DstSawLd1",
	/* 280 */ "DstSawLd2",
	/* 281 */ "Bass&Ld1",
	/* 282 */ "Bass&Ld2",
	/* 283 */ "PolySyn2",
	/* 284 */ "SawLead3",
	/* 285 */ "SquarLd1",
	/* 286 */ "SquarLd2",
	/* 287 */ "SquarLd3",
	/* 288 */ "SquarLd4",
	/* 289 */ "Dist.Sqr1",
	/* 290 */ "Dist.Sqr2",
	/* 291 */ "E.Piano5",
	/* 292 */ "E.Piano6",
	/* 293 */ "E.Piano7",
	/* 294 */ "Clavi2",
	/* 295 */ "Hrpschrd1",
	/* 296 */ "Hrpschrd2",
	/* 297 */ "PercOrg3",
	/* 298 */ "Drawbar5",
	/* 299 */ "DetunOr2",
	/* 300 */ "DetunOr3",
	/* 301 */ "60'sOrg",
	/* 302 */ "CheseOrg",
	/* 303 */ "PercOrg4",
	/* 304 */ "ChrchOrg3",
	/* 305 */ "ReedOrgn1",
	/* 306 */ "ReedOrgn2",
	/* 307 */ "Accord.1",
	/* 308 */ "Accord.2",
	/* 309 */ "Accord.3",
	/* 310 */ "Accord.4",
	/* 311 */ "TangoAcd1",
	/* 312 */ "TangoAcd2",
	/* 313 */ "Harmnica",
	/* 314 */ "Celesta2",
	/* 315 */ "MusicBox2",
	/* 316 */ "Crystal1",
	/* 317 */ "Crystal2",
	/* 318 */ "Kalimba2",
	/* 319 */ "TnklBell1",
	/* 320 */ "TnklBell2",
	/* 321 */ "JazzGtr2",
	/* 322 */ "MelowGt1",
	/* 323 */ "Hawaiian",
	/* 324 */ "MelowGt2",
	/* 325 */ "SynBass6",
	/* 326 */ "SynBass7",
	/* 327 */ "SynBass8",
	/* 328 */ "SynBras4",
	/* 329 */ "SynBras5",
	/* 330 */ "Warm1",
	/* 331 */ "Warm2",
	/* 332 */ "Bowed2",
	/* 333 */ "Sweep3",
	/* 334 */ "Sweep4",
	/* 335 */ "Goblns2",
	/* 336 */ "Whistle1",
	/* 337 */ "Whistle2",
	/* 338 */ "Ocarina2",
	/* 339 */ "Recorder2",
	/* 340 */ "Bag Pipe2",
	/* 341 */ "Shanai2",
}

func (o OscillatorSettings) String() string {
	waveKitName := "0"
	if o.WaveKit == 512 {
		waveKitName = "ADD"
	} else {
		waveKitName = fmt.Sprintf("%d", o.WaveKit)
	}
	return waveKitName
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

// FilterSettings represents the DCF settings of the source
type FilterSettings struct {
	Bypassed              bool
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

func (f FilterSettings) String() string {
	filterState := "active"
	if f.Bypassed {
		filterState = "bypassed"
	}

	filterMode := "LP"
	if f.Mode == 1 {
		filterMode = "HP"
	}
	return fmt.Sprintf("%s, %s, L%d, cutoff = %d, velocity curve = %d, resonance = %d, KS to Cut = %d, Velo to Cut = %d",
		filterState, filterMode, f.Level, f.Cutoff, f.VelocityCurve, f.Resonance, f.CutoffKeyScalingDepth, f.CutoffVelocityDepth)
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

type EnvelopeSegment struct {
	Rate  int
	Level int
}

func (e EnvelopeSegment) String() string {
	return fmt.Sprintf("R%d L%d", e.Rate, e.Level)
}

type MORFHarmonicEnvelope struct {
	Time1    int
	Time2    int
	Time3    int
	Time4    int
	LoopType int
}

func (e MORFHarmonicEnvelope) String() string {
	return fmt.Sprintf("T1 = %d, T2 = %d, T3 = %d, T4 = %d, loop = %d", e.Time1, e.Time2, e.Time3, e.Time4, e.LoopType)
}

type HarmonicEnvelope struct {
	Segments [4]EnvelopeSegment
	SRSFlag  bool
	TRTFlag  bool
}

type LoopingEnvelope struct {
	Attack   EnvelopeSegment
	Decay1   EnvelopeSegment
	Decay2   EnvelopeSegment
	Release  EnvelopeSegment
	LoopType int
}

func (e LoopingEnvelope) String() string {
	return fmt.Sprintf("A: %s, D1: %s, D2: %s, R: %s", e.Attack, e.Decay1, e.Decay2, e.Release)
}

type HarmonicCopyParameters struct {
	PatchNumber  int
	SourceNumber int
}

func (h HarmonicCopyParameters) String() string {
	return fmt.Sprintf("P%d S%d", h.PatchNumber, h.SourceNumber)
}

type HarmonicParameters struct {
	TotalGain int

	// Non-MORF parameters
	HarmonicGroup    int // 0 = LO, 1 = HI
	KeyScalingToGain int
	VelocityCurve    int
	VelocityDepth    int

	// MORF parameters
	// Harmonic Copy
	HarmonicCopy1 HarmonicCopyParameters
	HarmonicCopy2 HarmonicCopyParameters
	HarmonicCopy3 HarmonicCopyParameters
	HarmonicCopy4 HarmonicCopyParameters

	// Harmonic envelope
	Envelope MORFHarmonicEnvelope
}

func (h HarmonicParameters) String() string {
	group := "LO"
	if h.HarmonicGroup == 1 {
		group = "HI"
	}

	return fmt.Sprintf("total gain = %d, harmonic group = %s, KS to gain = %d, vel. curve = %d, vel. depth = %d\nHC1 = %s, HC2 = %s, HC3 = %s, HC4 = %s\nMORF HE = %s",
		h.TotalGain, group, h.KeyScalingToGain, h.VelocityCurve, h.VelocityDepth, h.HarmonicCopy1, h.HarmonicCopy2, h.HarmonicCopy3, h.HarmonicCopy4, h.Envelope)
}

type LFOParameters struct {
	Speed int
	Shape int
	Depth int
}

func (l LFOParameters) String() string {
	shapeString := ""
	switch l.Shape {
	case 0:
		shapeString = "TRI"
	case 1:
		shapeString = "SAW"
	case 2:
		shapeString = "RND"
	default:
		shapeString = "?"
	}
	return fmt.Sprintf("S/%d %s D/%d", l.Speed, shapeString, l.Depth)
}

type FormantParameters struct {
	Bias      int  // (-63)1 ... (+63)127
	EnvLFOSel bool // false = env, true = LFO

	// Envelope parameters
	EnvelopeDepth int
	Envelope      LoopingEnvelope

	VelocitySensitivity int // (-63)1 ... (+63)127
	KeyScaling          int // (-63)1 ... (+63)127

	LFO LFOParameters // speed = 0...127; shape = 0/TRI, 1=SAW, 2=RND; depth = 0...63
}

func (f FormantParameters) String() string {
	envLFOSelString := "ENV"
	if f.EnvLFOSel {
		envLFOSelString = "LFO"
	}
	return fmt.Sprintf("bias = %d, %s, envelope = %s (depth=%d), vel.sens = %d, KS = %d, LFO = %s",
		f.Bias, envLFOSelString, f.Envelope, f.EnvelopeDepth, f.VelocitySensitivity, f.KeyScaling, f.LFO)
}

func printable64ByteArray(b [64]byte) string {
	s := ""
	for i := 0; i < 64; i++ {
		s += fmt.Sprintf("%d/%d ", i+1, b[i])
	}
	return s
}

type AdditiveKit struct {
	MorfFlag          bool // false = MORF OFF, true = MORF ON
	Harmonics         HarmonicParameters
	Formant           FormantParameters
	LowHarmonics      [64]byte
	HighHarmonics     [64]byte
	FormantFilterData [128]byte
	HarmonicEnvelopes [numHarmonics]HarmonicEnvelope
}

func newAdditiveKit(d []byte) AdditiveKit {
	kit := AdditiveKit{
		MorfFlag: d[0] == 1,
		Harmonics: HarmonicParameters{
			TotalGain:        int(d[1]),
			HarmonicGroup:    int(d[2]),
			KeyScalingToGain: int(d[3]) - 64,
			VelocityCurve:    int(d[4]) + 1, // 1...12
			VelocityDepth:    int(d[5]),     // 0...127
			HarmonicCopy1: HarmonicCopyParameters{
				PatchNumber:  int(d[6]),
				SourceNumber: int(d[7]),
			},
			HarmonicCopy2: HarmonicCopyParameters{
				PatchNumber:  int(d[8]),
				SourceNumber: int(d[9]),
			},
			HarmonicCopy3: HarmonicCopyParameters{
				PatchNumber:  int(d[10]),
				SourceNumber: int(d[11]),
			},
			HarmonicCopy4: HarmonicCopyParameters{
				PatchNumber:  int(d[12]),
				SourceNumber: int(d[13]),
			},
			Envelope: MORFHarmonicEnvelope{
				Time1:    int(d[14]),
				Time2:    int(d[15]),
				Time3:    int(d[16]),
				Time4:    int(d[17]),
				LoopType: int(d[18]), // 0 = OFF, 1 = LP1, 2 = LP2
			},
		},
		Formant: FormantParameters{
			Bias:          int(d[19]) - 64,
			EnvLFOSel:     int(d[20]) == 1,
			EnvelopeDepth: int(d[21]) - 64,
			Envelope: LoopingEnvelope{
				Attack: EnvelopeSegment{
					Rate:  int(d[22]),
					Level: int(d[23]) - 64,
				},
				Decay1: EnvelopeSegment{
					Rate:  int(d[24]),
					Level: int(d[25]) - 64,
				},
				Decay2: EnvelopeSegment{
					Rate:  int(d[26]),
					Level: int(d[27]) - 64,
				},
				Release: EnvelopeSegment{
					Rate:  int(d[28]),
					Level: int(d[29]) - 64,
				},
				LoopType: int(d[30]),
			},
			VelocitySensitivity: int(d[31]) - 64,
			KeyScaling:          int(d[32]) - 64,
			LFO: LFOParameters{
				Speed: int(d[33]),
				Shape: int(d[34]), // 0=TRI, 1=SAW, 2 = RND
				Depth: int(d[35]),
			},
		},
	}

	// Need to separately copy slices into arrays
	copy(kit.LowHarmonics[:], d[36:100])
	copy(kit.HighHarmonics[:], d[100:164])
	copy(kit.FormantFilterData[:], d[164:292])

	offset := 292
	for i := 0; i < numHarmonics; i++ {
		segments := [4]EnvelopeSegment{
			EnvelopeSegment{
				Rate:  int(d[offset]),
				Level: int(d[offset+1]),
			},
			EnvelopeSegment{
				Rate:  int(d[offset+2]),
				Level: int(d[offset+3] & 0x3F),
			},
			EnvelopeSegment{
				Rate:  int(d[offset+4]),
				Level: int(d[offset+5] & 0x3F),
			},
			EnvelopeSegment{
				Rate:  int(d[offset+6]),
				Level: int(d[offset+7]),
			},
		}

		kit.HarmonicEnvelopes[i] = HarmonicEnvelope{
			Segments: segments,
			SRSFlag:  ((d[offset+3] >> 6) & 0x01) == 1,
			TRTFlag:  ((d[offset+5] >> 6) & 0x01) == 1,
		}
	}

	return kit
}

func (k AdditiveKit) String() string {
	return fmt.Sprintf("MORF = %t\nFormant parameters: %s\nHarmonic parameters: %s\nLow harmonics = %s\nHigh harmonics: %s",
		k.MorfFlag, k.Formant, k.Harmonics, printable64ByteArray(k.LowHarmonics), printable64ByteArray(k.HighHarmonics))
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
	Oscillator        OscillatorSettings
	Filter            FilterSettings
	Amplifier         AmplifierSettings
	LFO               LFOSettings
}

func newSource(d []byte) Source {
	waveKitMSB := int(d[28])
	waveKitLSB := int(d[29])
	waveKitNumber := waveKitMSB*128 + waveKitLSB
	return Source{
		ZoneLow:           int(d[0]),
		ZoneHigh:          int(d[1]),
		VelocitySwitching: int(d[2]), // TODO: parse velocity switching value
		EffectPath:        int(d[3]),
		Volume:            int(d[4]),
		BenderPitch:       int(d[5]),
		BenderCutoff:      int(d[6]),
		Pressure: ModulationTarget{
			Target1: Modulation{
				Destination: int(d[7]),
				Depth:       int(d[8]) - 32, // (-31)33 ~ (+31)95
			},
			Target2: Modulation{
				Destination: int(d[9]),
				Depth:       int(d[10]) - 32,
			},
		},
		Wheel: ModulationTarget{
			Target1: Modulation{
				Destination: int(d[11]),
				Depth:       int(d[12]) - 32,
			},
			Target2: Modulation{
				Destination: int(d[13]),
				Depth:       int(d[14]) - 32,
			},
		},
		Expression: ModulationTarget{
			Target1: Modulation{
				Destination: int(d[15]),
				Depth:       int(d[16]) - 32,
			},
			Target2: Modulation{
				Destination: int(d[17]),
				Depth:       int(d[18]) - 32,
			},
		},
		Assignable1: AssignableModulationTarget{
			Source: int(d[19]),
			Modulation: Modulation{
				Destination: int(d[20]),
				Depth:       int(d[21]) - 32,
			},
		},
		Assignable2: AssignableModulationTarget{
			Source: int(d[22]),
			Modulation: Modulation{
				Destination: int(d[23]),
				Depth:       int(d[24]) - 32,
			},
		},
		KeyOnDelay: int(d[25]),
		Pan: PanSettings{
			PanType:  int(d[26]),
			PanValue: int(d[27]) - 64,
		},
		Oscillator: OscillatorSettings{
			WaveKit:  waveKitNumber + 1,
			Coarse:   int(d[30]) - 25, // (-24)40 ... +24(88)
			Fine:     int(d[31]) - 64,
			FixedKey: int(d[32]), // 0=OFF, 21~108=ON(A-1 ~ C7)
			KSPitch:  int(d[33]), // 0=0cent, 1=25cent,2=33cent, 3=50cent
			PitchEnvelope: PitchEnvelopeSettings{
				StartLevel:  int(d[34]) - 64,
				AttackTime:  int(d[35]),
				AttackLevel: int(d[36]) - 64,
				DecayTime:   int(d[37]),
			},
			VelocityToTime:  int(d[38]) - 64,
			VelocityToLevel: int(d[39]) - 64,
		},
		Filter: FilterSettings{
			Bypassed:              int(d[40]) == 1,
			Mode:                  int(d[41]),     // 0 = LP, 1 = HP
			VelocityCurve:         int(d[42]) + 1, // 0~11 (1~12)
			Resonance:             int(d[43]),
			Level:                 int(d[44]), // check the 0...7 (7...0) thing
			Cutoff:                int(d[45]),
			CutoffKeyScalingDepth: int(d[46]) - 64,
			CutoffVelocityDepth:   int(d[47]) - 64,
			EnvelopeDepth:         int(d[48]) - 64,
			Envelope: EnvelopeSettings{
				AttackTime:  int(d[49]),
				Decay1Time:  int(d[50]),
				Decay1Level: int(d[51]) - 64,
				Decay2Time:  int(d[52]),
				Decay2Level: int(d[53]) - 64,
				ReleaseTime: int(d[54]),
			},
			KeyScalingToEnvelope: FilterKeyScalingToEnvelope{
				AttackTime: int(d[55]) - 64,
				Decay1Time: int(d[56]) - 64,
			},
			VelocityToEnvelope: FilterVelocityToEnvelope{
				EnvelopeDepth: int(d[57]) - 64,
				AttackTime:    int(d[58]) - 64,
				Decay1Time:    int(d[59]) - 64,
			},
		},
		Amplifier: AmplifierSettings{
			VelocityCurve: int(d[60]) + 1, // 0...11 (1~12)
			Envelope: EnvelopeSettings{
				AttackTime:  int(d[61]),
				Decay1Time:  int(d[62]),
				Decay1Level: int(d[63]),
				Decay2Time:  int(d[64]),
				Decay2Level: int(d[65]),
				ReleaseTime: int(d[66]),
			},
			KeyScalingToEnvelope: AmplifierKeyScalingToEnvelope{
				Level:       int(d[67]) - 64,
				AttackTime:  int(d[68]) - 64,
				Decay1Time:  int(d[69]) - 64,
				ReleaseTime: int(d[70]) - 64,
			},
			VelocitySensitivity: AmplifierVelocitySensitivity{
				Level:       int(d[71]),
				AttackTime:  int(d[72]) - 64,
				Decay1Time:  int(d[73]) - 64,
				ReleaseTime: int(d[74]) - 64,
			},
		},
		LFO: LFOSettings{
			Waveform:   int(d[75]),
			Speed:      int(d[76]),
			DelayOnset: int(d[77]),
			FadeIn: LFOFadeInSettings{
				Time:    int(d[78]),
				ToSpeed: int(d[79]),
			},
			Vibrato: LFOModulationSettings{
				Depth:      int(d[80]),
				KeyScaling: int(d[81]) - 64,
			},
			Growl: LFOModulationSettings{
				Depth:      int(d[82]),
				KeyScaling: int(d[83]) - 64,
			},
			Tremolo: LFOModulationSettings{
				Depth:      int(d[84]),
				KeyScaling: int(d[85]) - 64,
			},
		},
	}
}

// Macro controller names
var macroControllerNames = []string{
	/*  0 */ "Pitch offset",
	/*  1 */ "Cutoff offset",
	/*  2 */ "Level",
	/*  3 */ "Vibrato depth offset",
	/*  4 */ "Growl depth offset",
	/*  5 */ "Tremolo depth offset",
	/*  6 */ "LFO speed offset",
	/*  7 */ "Attack time offset",
	/*  8 */ "Decay1 time offset",
	/*  9 */ "Release time offset",
	/* 10 */ "Velocity offset",
	/* 11 */ "Resonance offset",
	/* 12 */ "Panpot offset",
	/* 13 */ "FF bias offset",
	/* 14 */ "FF ENV/LFO depth offset",
	/* 15 */ "FF ENV/LFO speed offset",
	/* 16 */ "Harmonic lo offset",
	/* 17 */ "Harmonic hi offset",
	/* 18 */ "Harmonic even offset",
	/* 19 */ "Harmonic odd offset",
}

type MacroController struct {
	Param1 Modulation
	Param2 Modulation
}

var switchNames = []string{
	/*  0 */ "OFF",
	/*  1 */ "Harm Max",
	/*  2 */ "Harm Bright",
	/*  3 */ "Harm Dark",
	/*  4 */ "Harm Saw",
	/*  5 */ "Select Loud",
	/*  6 */ "Add Loud",
	/*  7 */ "Add 5th",
	/*  8 */ "Add Odd",
	/*  9 */ "Add Even",
	/* 10 */ "HE #1",
	/* 11 */ "HE #2",
	/* 12 */ "HE Loop",
	/* 13 */ "FF max",
	/* 14 */ "FF Comb",
	/* 15 */ "FF hicut",
	/* 16 */ "FF Comb2",
}

var effectDestinatioNames = []string{
	/* 0 */ "Effect1 Dry/Wet",
	/* 1 */ "Effect1 Para",
	/* 2 */ "Effect2 Dry/Wet",
	/* 3 */ "Effect2 Para",
	/* 4 */ "Effect3 Dry/Wet",
	/* 5 */ "Effect3 Para",
	/* 6 */ "Effect4 Dry/Wet",
	/* 7 */ "Effect4 Para",
	/* 8 */ "Reverb Dry/Wet1",
	/* 9 */ "Reverb Dry/Wet2",
}

var controlSourceNames = []string{
	/*  0 */ "Bender",
	/*  1 */ "Channel Pressure",
	/*  2 */ "Wheel",
	/*  3 */ "Expression",
	/*  4 */ "MIDI Volume",
	/*  5 */ "Panpot",
	/*  6 */ "General Controller 1",
	/*  7 */ "General Controller 2",
	/*  8 */ "General Controller 3",
	/*  9 */ "General Controller 4",
	/* 10 */ "General Controller 5",
	/* 11 */ "General Controller 6",
	/* 12 */ "General Controller 7",
	/* 13 */ "General Controller 8",
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
	SourceMutes map[int]bool // "Go does not provide a set type, but since the keys of a map are distinct, a map can serve this pur pose." TGPL p. 96

	// AM: Selects sources for Amplitude Modulation. One source can be set to modulate an adjacent source, i.e., 1>2.
	AmplitudeModulation int

	EffectControl1 AssignableModulationTarget
	EffectControl2 AssignableModulationTarget

	PortamentoEnabled bool
	PortamentoSpeed   int

	MacroController1 MacroController
	MacroController2 MacroController
	MacroController3 MacroController
	MacroController4 MacroController

	SW1  int
	SW2  int
	FSW1 int
	FSW2 int
}

func (c Common) String() string {
	return fmt.Sprintf("Volume: %3d  Polyphony: %d  Sources: %d  Effect algorithm: %d", c.Volume, c.Polyphony, c.SourceCount, c.EffectAlgorithm)
}

// Patch represents the parameters of a sound.
type Patch struct {
	Common
	Sources [numSources]Source
	Kits    []AdditiveKit
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
	numGEQBands         = 7
	numHarmonics        = 64
	numEffects          = 4

	// Common data offsets:
	commonChecksumOffset      = 0
	effectAlgorithmOffset     = 1
	reverbOffset              = 2
	effect1Offset             = 8
	effect2Offset             = 14
	effect3Offset             = 20
	effect4Offset             = 26
	gEQOffset                 = 32
	nameOffset                = 40
	volumeOffset              = 48
	polyphonyOffset           = 49
	sourceCountOffset         = 51
	sourceMutesOffset         = 52
	amplitudeModulationOffset = 53
	effectControlOffset       = 54
	portamentoOffset          = 60
	macroOffset               = 62
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

		effectAlgorithm := int(d[effectAlgorithmOffset]) + 1 // value is 0...3, scale to 1...4
		reverb := newReverb(d[reverbOffset : reverbOffset+6])
		effect1 := newEffect(d[effect1Offset : effect1Offset+6])
		effect2 := newEffect(d[effect2Offset : effect2Offset+6])
		effect3 := newEffect(d[effect3Offset : effect3Offset+6])
		effect4 := newEffect(d[effect4Offset : effect4Offset+6])
		geq := newGEQ(d[gEQOffset : gEQOffset+7])

		// Note: we skipped the "drum mark" at offset 39; it is always zero.

		nameStart := nameOffset
		nameEnd := nameStart + nameSize
		nameData := d[nameStart:nameEnd]
		name := string(bytes.TrimRight(nameData, "\x00"))

		volume := int(d[volumeOffset])
		polyphony := int(d[polyphonyOffset])

		// There is an unused byte at offset 50.
		// We needed source count earlier, so it's done already.

		// Source mutes are in the lowest four bits of the byte:
		mutes := d[sourceMutesOffset]
		mutesString := fmt.Sprintf("%08b", mutes)
		sm := make(map[int]bool)
		sm[6] = string(mutesString[5]) == "0"
		sm[5] = string(mutesString[4]) == "0"
		sm[4] = string(mutesString[3]) == "0"
		sm[3] = string(mutesString[2]) == "0"
		sm[2] = string(mutesString[1]) == "0"
		sm[1] = string(mutesString[0]) == "0"
		// TODO: Rewrite the source mutes parsing using logical bit operations.

		// Amplitude modulation: indicates which source modulates source 1
		am := int(d[amplitudeModulationOffset]) + 1

		offset := effectControlOffset

		// Effect control 1
		ec1 := AssignableModulationTarget{
			Source: int(d[offset]),
			Modulation: Modulation{
				Destination: int(d[offset+1]),
				Depth:       int(d[offset+2]),
			},
		}

		ec2 := AssignableModulationTarget{
			Source: int(d[offset+3]),
			Modulation: Modulation{
				Destination: int(d[offset+4]),
				Depth:       int(d[offset+5]),
			},
		}

		portamentoFlag := int(d[portamentoOffset]) == 1
		portamentoSpeed := int(d[portamentoOffset+1])

		offset = macroOffset
		mc1 := MacroController{
			Param1: Modulation{
				Destination: int(d[offset]),
				Depth:       int(d[offset+8]) - 32,
			},
			Param2: Modulation{
				Destination: int(d[offset+1]),
				Depth:       int(d[offset+9]) - 32,
			},
		}
		mc2 := MacroController{
			Param1: Modulation{
				Destination: int(d[offset+2]),
				Depth:       int(d[offset+10]) - 32,
			},
			Param2: Modulation{
				Destination: int(d[offset+3]),
				Depth:       int(d[offset+11]) - 32,
			},
		}
		mc3 := MacroController{
			Param1: Modulation{
				Destination: int(d[offset+4]),
				Depth:       int(d[offset+12]) - 32,
			},
			Param2: Modulation{
				Destination: int(d[offset+5]),
				Depth:       int(d[offset+13]) - 32,
			},
		}
		mc4 := MacroController{
			Param1: Modulation{
				Destination: int(d[offset+6]),
				Depth:       int(d[offset+14]) - 32,
			},
			Param2: Modulation{
				Destination: int(d[offset+7]),
				Depth:       int(d[offset+15]) - 32,
			},
		}

		sw1 := int(d[offset+16])
		sw2 := int(d[offset+17])
		fsw1 := int(d[offset+18])
		fsw2 := int(d[offset+19])

		sourceOffset := commonDataSize // source data starts after common data
		var sources [numSources]Source
		for sourceIndex := 0; sourceIndex < numSources; sourceIndex++ {
			if sourceIndex < sourceCount {
				sources[sourceIndex] = newSource(d[sourceOffset : sourceOffset+sourceDataSize])
				sourceOffset += sourceDataSize
			}
		}

		// With struct embedding, the literal must follow the shape of the type declaration. (TGPL, p. 106)
		c := Common{
			Name:                name,
			SourceCount:         sourceCount,
			Reverb:              reverb,
			Effect1:             effect1,
			Effect2:             effect2,
			Effect3:             effect3,
			Effect4:             effect4,
			Volume:              volume,
			Polyphony:           polyphony,
			EffectAlgorithm:     effectAlgorithm,
			GEQ:                 geq,
			PortamentoEnabled:   portamentoFlag,
			PortamentoSpeed:     portamentoSpeed,
			SourceMutes:         sm,
			AmplitudeModulation: am,
			EffectControl1:      ec1,
			EffectControl2:      ec2,
			MacroController1:    mc1,
			MacroController2:    mc2,
			MacroController3:    mc3,
			MacroController4:    mc4,
			SW1:                 sw1,
			SW2:                 sw2,
			FSW1:                fsw1,
			FSW2:                fsw2,
		}

		patch := Patch{Common: c, Sources: sources}

		// Now sourceOffset should be at the start of the additive wave kits
		for i := 0; i < pp.additiveWaveKitCount(); i++ {
			//additiveChecksum := d[sourceOffset]
			sourceOffset++
			additiveData := d[sourceOffset : sourceOffset+additiveWaveKitSize]
			kit := newAdditiveKit(additiveData)
			patch.Kits = append(patch.Kits, kit)
			sourceOffset += additiveWaveKitSize
		}

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
