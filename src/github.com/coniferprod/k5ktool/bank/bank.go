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
	return fmt.Sprintf("Atk T = %d, Dcy1 T = %d, Dcy1 L = %d, Dcy2 T = %d, Dcy2 L = %d, Rels T = %d",
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

type PanSettings struct {
	PanType  int
	PanValue int
}

type PitchEnvelopeSettings struct {
	StartLevel  int
	AttackTime  int
	AttackLevel int
	DecayTime   int
}

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

	filterMode := "lowpass"
	if f.Mode == 1 {
		filterMode = "highpass"
	}
	return fmt.Sprintf("state = %s, mode = %s, level = %d, cutoff = %d, velocity curve = %d, resonance = %d, KS to Cut = %d, Velo to Cut = %d",
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
	Rate    int
	Level   int
	Looping bool
}

type HarmonicEnvelope struct {
	Segment1 EnvelopeSegment
	Segment2 EnvelopeSegment
	Segment3 EnvelopeSegment
	Segment4 EnvelopeSegment
}

type HarmonicCopyParameters struct {
	PatchNumber  int
	SourceNumber int
}

type HarmonicParameters struct {
	TotalGain int

	// Non-MORF parameters
	HarmonicGroup    bool
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
	Envelope EnvelopeSegment
	LoopType int
}

type LFOParameters struct {
	Speed int
	Shape int
	Depth int
}

type FormantParameters struct {
	Bias      int  // (-63)1 ... (+63)127
	EnvLFOSel bool // false = env, true = LFO

	// Envelope parameters
	EnvelopeDepth       int             // (-63)1 ... (+63)127
	Attack              EnvelopeSegment // rate = 0...127, level = (-63)1 ... (+63)127
	Decay1              EnvelopeSegment
	Decay2              EnvelopeSegment
	Release             EnvelopeSegment
	LoopType            int // 0 = off, 1 = LP1, 2 = LP2
	VelocitySensitivity int // (-63)1 ... (+63)127
	KeyScaling          int // (-63)1 ... (+63)127

	LFO LFOParameters // speed = 0...127; shape = 0/TRI, 1=SAW, 2=RND; depth = 0...63
}

type AdditiveKit struct {
	MorfFlag          bool // false = MORF OFF, true = MORF ON
	Harmonics         HarmonicParameters
	Formant           FormantParameters
	LowHarmonics      [64]byte
	HighHarmonics     [64]byte
	FormantFilterData [128]byte
	HarmonicEnvelopes [64]HarmonicEnvelope
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
		VelocitySwitching: int(d[2]),
		EffectPath:        int(d[3]),
		Volume:            int(d[4]),
		BenderPitch:       int(d[5]),
		BenderCutoff:      int(d[6]),
		Pressure: ModulationTarget{
			Target1: Modulation{
				Destination: int(d[7]),
				Depth:       int(d[8]),
			},
			Target2: Modulation{
				Destination: int(d[9]),
				Depth:       int(d[10]),
			},
		},
		Wheel: ModulationTarget{
			Target1: Modulation{
				Destination: int(d[11]),
				Depth:       int(d[12]),
			},
			Target2: Modulation{
				Destination: int(d[13]),
				Depth:       int(d[14]),
			},
		},
		Expression: ModulationTarget{
			Target1: Modulation{
				Destination: int(d[15]),
				Depth:       int(d[16]),
			},
			Target2: Modulation{
				Destination: int(d[17]),
				Depth:       int(d[18]),
			},
		},
		Assignable1: AssignableModulationTarget{
			Source: int(d[19]),
			Modulation: Modulation{
				Destination: int(d[20]),
				Depth:       int(d[21]),
			},
		},
		Assignable2: AssignableModulationTarget{
			Source: int(d[22]),
			Modulation: Modulation{
				Destination: int(d[23]),
				Depth:       int(d[24]),
			},
		},
		KeyOnDelay: int(d[25]),
		Pan: PanSettings{
			PanType:  int(d[26]),
			PanValue: int(d[27]),
		},
		Oscillator: OscillatorSettings{
			WaveKit:  waveKitNumber + 1,
			Coarse:   int(d[30]), // TODO: handle (-24)40 ... +24(88)
			Fine:     int(d[31]) - 64,
			FixedKey: int(d[32]),
			KSPitch:  int(d[33]),
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
			Mode:                  int(d[41]),
			VelocityCurve:         int(d[42]) + 1, // make it 1...12
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
			VelocityCurve: int(d[60]), // zero- or one-based?
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

		reverb := newReverb(d[reverbOffset : reverbOffset+6])

		effect1 := newEffect(d[effect1Offset : effect1Offset+6])
		effect2 := newEffect(d[effect2Offset : effect2Offset+6])
		effect3 := newEffect(d[effect3Offset : effect3Offset+6])
		effect4 := newEffect(d[effect4Offset : effect4Offset+6])

		geq := newGEQ(d[gEQOffset : gEQOffset+7])

		portamentoFlag := int(d[portamentoOffset]) == 1
		portamentoSpeed := int(d[portamentoOffset+1])

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
