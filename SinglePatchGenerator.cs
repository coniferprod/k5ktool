using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using KSynthLib.K5000;

namespace K5KTool
{
    public class SinglePatchGenerator
    {
        static Dictionary<string, (byte[], byte[])> HarmonicLevelTemplates = new Dictionary<string, (byte[], byte[])>()
        {
            {
                "Init",
                (new byte[]
                {
                    127, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                })
            },
            {
                "Saw soft",
                (new byte[]
                {
                    127, 124, 121, 118, 115, 112, 109, 106,  // 1 ... 8
                    103, 100, 97, 94, 91, 88, 85, 82,        // 9 ... 16
                    79, 76, 73, 70, 67, 64, 61, 58,          // 17 ... 24
                    55, 52, 49, 46, 43, 40, 37, 34,          // 25 ... 32
                    31, 28, 25, 22, 19, 16, 13, 10,          // 33 ... 40
                    7, 4, 1, 0, 0, 0, 0, 0,                  // 41 ... 48
                    0, 0, 0, 0, 0, 0, 0, 0,                  // 49 ... 56
                    0, 0, 0, 0, 0, 0, 0, 0                   // 57 ... 64
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            {
                "Saw bright",
                (new byte[]
                {
                    127, 121, 117, 113, 110, 106, 102, 101,  // 1 ... 8
                    100, 99, 98, 97, 96, 95, 94, 93,         // 9 ... 16
                    92, 91, 90, 89, 88, 87, 86, 85,          // 17 ... 24
                    84, 83, 82, 81, 80, 79, 78, 78,          // 25 ... 32
                    78, 77, 77, 77, 76, 76, 76, 75,          // 33 ... 40
                    75, 75, 74, 73, 72, 71, 70, 69,          // 41 ... 48
                    68, 67, 66, 65, 64, 63, 62, 61,          // 49 ... 56
                    60, 59, 58, 57, 56, 55, 54, 53           // 57 ... 64
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            {
                "8+4",
                (new byte[]
                {
                    61, 73, 12, 77, 12, 81, 12, 90,  // 1 ... 8
                    12, 100, 12, 106, 12, 109, 12, 118,         // 9 ... 16
                    12, 12, 12, 118, 12, 12, 12, 114,          // 17 ... 24
                    12, 12, 12, 113, 12, 12, 12, 115,          // 25 ... 32
                    12, 12, 12, 112, 12, 111, 12, 112,          // 33 ... 40
                    12, 106, 12, 109, 12, 109, 12, 105,          // 41 ... 48
                    12, 106, 12, 104, 12, 99, 12, 102,          // 49 ... 56
                    12, 103, 12, 99, 12, 96, 12, 96           // 57 ... 64
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            {
                "Pulse 1",
                (new byte[]
                {
                    127, 124, 120, 0, 119, 117, 115, 0,  // 1 ... 8
                    111, 109, 107, 0, 103, 101, 100, 0,         // 9 ... 16
                    98, 97, 96, 0, 94, 93, 91, 0,          // 17 ... 24
                    89, 88, 87, 0, 85, 84, 83, 0,          // 25 ... 32
                    81, 80, 79, 0, 77, 76, 75, 0,          // 33 ... 40
                    73, 72, 71, 0, 69, 68, 67, 0,          // 41 ... 48
                    65, 64, 63, 0, 61, 60, 59, 0,          // 49 ... 56
                    56, 54, 52, 0, 48, 46, 44, 0           // 57 ... 64
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            {
                "Pulse 2",
                (new byte[]
                {
                    127, 124, 0, 121, 119, 0, 115, 113,  // 1 ... 8
                    0, 109, 107, 0, 103, 101, 0, 99,         // 9 ... 16
                    98, 0, 96, 95, 0, 93, 91, 0,          // 17 ... 24
                    89, 88, 0, 86, 85, 0, 83, 82,          // 25 ... 32
                    0, 80, 79, 0, 77, 76, 0, 74,          // 33 ... 40
                    73, 0, 71, 70, 0, 68, 67, 0,          // 41 ... 48
                    65, 64, 0, 62, 61, 0, 59, 58,          // 49 ... 56
                    0, 54, 52, 0, 48, 46, 0, 42           // 57 ... 64
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                })
            },
            {
                "Triangle",
                (new byte[]
                {
                    127, 0, 85, 0, 47, 0, 20, 0,  // 1 ... 8
                    4, 0, 0, 0, 0, 0, 0, 0,         // 9 ... 16
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            {
                "FM Piano",
                (new byte[]
                {
                    124, 119, 108, 101, 74, 60, 46, 42,  // 1 ... 8
                    0, 0, 0, 0, 0, 111, 0, 117,          // 9 ... 16
                    0, 0, 0, 0, 0, 0, 0, 0,              // 17 ... 24
                    0, 0, 0, 0, 98, 0, 110, 0,           // 25 ... 32
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            {
                "Stage Piano",
                (new byte[]
                {
                    115, 127, 120, 100, 98, 84, 81, 74,  // 1 ... 8
                    55, 0, 0, 0, 0, 0, 0, 0,          // 9 ... 16
                    0, 0, 0, 85, 72, 29, 71, 0,              // 17 ... 24
                    0, 0, 0, 0, 0, 0, 0, 0,           // 25 ... 32
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            {
                "Vibes",
                (new byte[]
                {
                    127, 0, 0, 115, 0, 0, 0, 0,  // 1 ... 8
                    0, 0, 103, 0, 0, 0, 0, 0,          // 9 ... 16
                    0, 0, 106, 0, 0, 0, 0, 0,              // 17 ... 24
                    0, 106, 0, 0, 0, 0, 0, 0,           // 25 ... 32
                    0, 0, 0, 0, 0, 0, 0, 0,             // 33 ... 40
                    100, 0, 0, 0, 0, 0, 0, 0,           // 41 ... 48
                    72, 0, 0, 0, 0, 0, 0, 60,            // 49 ... 56
                    0, 0, 0, 0, 0, 0, 0, 31             // 57 ... 64
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
            // TODO: Insert "Clavi", "Bells 1", "Bells 2", "Tubular", "SlapBass" here
            {
                "SynthBass",
                (new byte[]
                {
                    127, 121, 116, 90, 85, 103, 92, 96,  // 1 ... 8
                    0, 89, 86, 93, 85, 0, 91, 73,          // 9 ... 16
                    90, 0, 51, 78, 92, 81, 0, 83,              // 17 ... 24
                    90, 88, 0, 0, 0, 0, 0, 0,           // 25 ... 32
                    0, 0, 0, 0, 0, 0, 0, 0,             // 33 ... 40
                    0, 0, 0, 0, 0, 0, 0, 0,           // 41 ... 48
                    0, 0, 0, 0, 0, 0, 0, 0,            // 49 ... 56
                    0, 0, 0, 0, 0, 0, 0, 0             // 57 ... 64
                },
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                })
            },
        };

        // These are essentially the SoundDiver harmonic envelope templates
        static Dictionary<string, HarmonicEnvelope> HarmonicEnvelopeTemplates = new Dictionary<string, HarmonicEnvelope>()
        {
            {
                "Init",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Piano",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 125,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 92,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 49,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 39,
                        Level = 49
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "E-Piano",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 81,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 15,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Pluck",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 118,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 79,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Pad fast",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 83,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 63,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 64,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 52,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Pad slow",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 67,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 63,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 64,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Brass",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 106,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 115,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 86,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Perc. Organ",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 100,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 80,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 21,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Dark>Bright",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 52,
                        Level = 24
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 50,
                        Level = 0
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 21,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Even > Odd",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 57,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 48,
                        Level = 0
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 29,
                        Level = 63
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Formants",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 71,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 55,
                        Level = 0
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 43,
                        Level = 0
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "OrganFade",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 62,
                        Level = 0
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 46,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Octaver",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 62,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 48,
                        Level = 0
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 50,
                        Level = 35
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Sync",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 86,
                        Level = 42
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 66,
                        Level = 0
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 50,
                        Level = 35
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Attack",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 105,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 99,
                        Level = 39
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 50,
                        Level = 35
                    },
                    Segment1Loop = false,
                    Segment2Loop = false
                }
            },
            {
                "Repeat",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 91,
                        Level = 47
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 50,
                        Level = 35
                    },
                    Segment1Loop = false,
                    Segment2Loop = true
                }
            },
            {
                "E/O Loop",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 80,
                        Level = 36
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 88,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1Loop = false,
                    Segment2Loop = true
                }
            },
            {
                "Metal Loop",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 65,
                        Level = 0
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 89,
                        Level = 0
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = true,
                    Segment2Loop = false
                }
            },
            {
                "Organ Loop",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 89,
                        Level = 0
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 90,
                        Level = 63
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 0,
                        Level = 0
                    },
                    Segment1Loop = false,
                    Segment2Loop = true
                }
            },
            {
                "Mod Loop",
                new HarmonicEnvelope()
                {
                    Segment0 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 63
                    },
                    Segment1 = new EnvelopeSegment()
                    {
                        Rate = 89,
                        Level = 38
                    },
                    Segment2 = new EnvelopeSegment()
                    {
                        Rate = 105,
                        Level = 50
                    },
                    Segment3 = new EnvelopeSegment()
                    {
                        Rate = 127,
                        Level = 35
                    },
                    Segment1Loop = false,
                    Segment2Loop = true
                }
            }
        };

        static Dictionary<string, byte[]> FormantFilterTemplates = new Dictionary<string, byte[]>()
        {
            {
                "Init",
                new byte[]
                { 
                    127, 127, 127, 127, 127, 127, 127, 127, // 8
                    127, 127, 127, 127, 127, 127, 127, 127, // 16
                    127, 127, 127, 127, 127, 127, 127, 127, // 24
                    127, 127, 127, 127, 127, 127, 127, 127, // 32
                    127, 127, 127, 127, 127, 127, 127, 127, // 40
                    127, 127, 127, 127, 127, 127, 127, 127, // 48
                    127, 127, 127, 127, 127, 127, 127, 127, // 56
                    127, 127, 127, 127, 127, 127, 127, 127, // 64

                    127, 127, 127, 127, 127, 127, 127, 127, // 72
                    127, 127, 127, 127, 127, 127, 127, 127, // 80
                    127, 127, 127, 127, 127, 127, 127, 127, // 88
                    127, 127, 127, 127, 127, 127, 127, 127, // 96
                    127, 127, 127, 127, 127, 127, 127, 127, // 104
                    127, 127, 127, 127, 127, 127, 127, 127, // 112
                    127, 127, 127, 127, 127, 127, 127, 127, // 120
                    127, 127, 118, 105, 90, 60, 30, 1, // 128 
                }
            },
            {
                "LoPass",
                new byte[]
                {
                    127, 127, 127, 127, 127, 127, 127, 127, // 8
                    127, 127, 127, 127, 127, 127, 127, 127, // 16
                    127, 127, 127, 127, 127, 127, 127, 127, // 24
                    127, 127, 127, 127, 127, 127, 127, 127, // 32
                    127, 127, 127, 127, 127, 127, 127, 127, // 40
                    127, 127, 127, 127, 127, 127, 127, 127, // 48
                    127, 127, 127, 127, 127, 127, 127, 127, // 56
                    127, 126, 127, 127, 127, 127, 127, 126, // 64
                    
                    126, 126, 124, 124, 124, 123, 122, 121, // 72
                    120, 120, 118, 118, 117, 116, 115, 114, // 80
                    113, 110, 109, 108, 107, 106, 105, 104, // 88
                    103, 101, 100, 98, 96, 95, 92, 91,      // 96
                    88, 86, 84, 82, 80, 78, 76, 74,         // 104 
                    72, 68, 66, 63, 61, 57, 56, 54,         // 112
                    50, 48, 46, 42, 39, 35, 31, 29,         // 120
                    26, 23, 18, 14, 11, 5, 2, 0             // 128
                }
            },
            {
                "LoPass Reso",
                new byte[]
                {
                    105, 105, 105, 105, 105, 105, 105, 105, // 8
                    105, 105, 105, 105, 105, 105, 105, 105, // 16
                    105, 105, 105, 105, 105, 105, 105, 105, // 24
                    105, 105, 105, 105, 105, 105, 105, 105, // 32
                    105, 105, 105, 105, 105, 105, 105, 105, // 40
                    105, 105, 105, 105, 105, 105, 105, 105, // 48
                    105, 105, 105, 105, 105, 105, 105, 105, // 56
                    105, 105, 105, 105, 105, 105, 105, 105, // 64
                    105, 105, 105, 105, 105, 107, 111, 113, // 72
                    119, 122, 127, 127, 126, 123, 117, 107, // 80
                    97, 89, 80, 73, 67, 63, 58, 54,         // 88
                    53, 51, 48, 45, 43, 41, 39, 37,         // 96
                    36, 33, 32, 30, 28, 26, 24, 23,         // 104
                    21, 20, 18, 17, 15, 14, 13, 12,         // 112
                    12, 11, 10, 8, 8, 8, 7, 6,              // 120
                    6, 5, 4, 4, 3, 2, 2, 0                  // 128
                }
            },
            {
                "BP smooth",
                new byte[]
                {
                    0, 0, 2, 5, 7, 10, 13, 15,              // 8
                    17, 19, 21, 24, 26, 28, 31, 33,         // 16
                    35, 37, 39, 42, 43, 45, 47, 49,         // 24
                    51, 52, 54, 55, 57, 58, 61, 62,         // 32
                    65, 66, 68, 69, 71, 72, 74, 76,         // 40
                    78, 79, 82, 84, 85, 87, 89, 90,         // 48
                    91, 94, 95, 97, 98, 100, 101, 103,      // 56
                    104, 106, 108, 109, 110, 112, 114, 115, // 64
                    116, 117, 118, 119, 120, 120, 121, 122, // 72
                    123, 124, 125, 126, 127, 126, 125, 124, // 80
                    123, 122, 121, 121, 120, 119, 117, 115, // 88
                    113, 111, 109, 107, 105, 103, 101, 99,  // 96
                    97, 94, 92, 89, 86, 83, 80, 78,         // 104
                    76, 73, 71, 68, 66, 63, 60, 57,         // 112
                    54, 50, 47, 44, 41, 37, 35, 32,         // 120
                    30, 27, 22, 19, 14, 11, 6, 2            // 128
                }
            },
            {
                "BP sh.reso",
                new byte[]
                {
                    0, 0, 2, 4, 8, 11, 15, 18,              // 8
                    22, 24, 29, 32, 35, 38, 43, 45,         // 16
                    45, 47, 48, 50, 51, 53, 54, 55,         // 24
                    56, 59, 59, 61, 63, 64, 65, 67,         // 32
                    67, 67, 68, 69, 69, 70, 71, 71,         // 40
                    72, 73, 74, 74, 75, 76, 76, 77,         // 48
                    77, 80, 83, 86, 89, 92, 97, 100,        // 56
                    103, 106, 109, 114, 117, 120, 123, 124, // 64
                    124, 123, 120, 117, 114, 111, 108, 105, // 72
                    102, 99, 94, 91, 88, 85, 82, 79,        // 80
                    79, 78, 77, 76, 75, 74, 73, 72,         // 88
                    71, 68, 67, 66, 65, 64, 63, 62,         // 96
                    62, 58, 53, 48, 43, 38, 34, 29,         // 104
                    24, 19, 14, 10, 5, 0, 0, 0,             // 112
                    0, 0, 0, 0, 0, 0, 0, 0,                 // 120
                    0, 0, 0, 0, 0, 0, 0, 0                  // 128
                }
            },
            {
                "HiPass",
                new byte[] 
                {
                    0, 5, 11, 14, 18, 21, 24, 27,           // 8
                    29, 30, 32, 34, 36, 39, 40, 42,         // 16
                    44, 45, 47, 48, 50, 52, 54, 55,         // 24
                    57, 59, 61, 62, 63, 64, 65, 66,         // 32
                    67, 68, 70, 71, 72, 73, 75, 76,         // 40
                    78, 79, 80, 81, 82, 83, 85, 86,         // 48
                    87, 88, 89, 90, 91, 93, 93, 94,         // 56
                    95, 96, 97, 97, 98, 99, 100, 101,       // 64
                    102, 103, 104, 105, 106, 107, 108, 109, // 72
                    110, 111, 112, 113, 114, 114, 115, 115, // 80
                    116, 116, 117, 117, 118, 118, 119, 119, // 88
                    120, 121, 121, 122, 123, 123, 124, 124, // 96
                    125, 125, 126, 126, 126, 127, 127, 127, // 104
                    127, 127, 127, 127, 127, 127, 127, 127, // 112
                    127, 127, 127, 127, 127, 127, 127, 127, // 120
                    127, 127, 127, 127, 127, 127, 127, 127, // 128
                }
            },
            {
                "HiPass Reso",
                new byte[]
                {
                    0, 0, 0, 1, 2, 3, 3, 3,                 // 8
                    4, 4, 5, 6, 7, 7, 8, 9,                 // 16
                    10, 11, 11, 12, 13, 14, 14, 15,         // 24
                    16, 17, 17, 18, 19, 20, 20, 21,         // 32
                    22, 22, 24, 25, 26, 27, 28, 28,         // 40
                    30, 31, 32, 33, 34, 35, 36, 37,         // 48
                    38, 39, 41, 41, 42, 43, 45, 46,         // 56
                    47, 49, 51, 52, 53, 54, 56, 57,         // 64
                    60, 61, 63, 66, 69, 70, 73, 76,         // 72
                    80, 83, 87, 89, 93, 98, 102, 105,       // 80
                    109, 114, 119, 123, 126, 127, 127,      // 88
                    127, 126, 123, 119, 111, 106, 102, 98,  // 96
                    94, 92, 90, 89, 88, 87, 87, 87,         // 104
                    87, 87, 87, 87, 87, 87, 87, 87,         // 112
                    87, 87, 87, 87, 87, 87, 87, 87,         // 120
                    87, 87, 87, 87, 87, 87, 87, 87          // 128
                }
            },
            {
                "3 Reso",
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0,                 // 8
                    0, 0, 1, 6, 13, 19, 26, 32,             // 16
                    32, 38, 44, 50, 56, 62, 68, 74,         // 24
                    81, 87, 93, 99, 105, 107, 113, 127,     // 32
                    117, 108, 100, 94, 85, 76, 66, 57,      // 40
                    47, 37, 28, 18, 9, 6, 6, 6,             // 48
                    6, 6, 6, 9, 18, 28, 38, 48,             // 56
                    56, 65, 75, 85, 94, 100, 114, 124,      // 64

                    127, 118, 100, 95, 85, 74, 65, 55,      // 72
                    46, 37, 26, 17, 7, 6, 6, 6,             // 80
                    6, 6, 6, 7, 18, 27, 36, 45,             // 88
                    56, 66, 75, 84, 95, 100, 110, 119,      // 96
                    125, 127, 115, 105, 98, 93, 87, 81,     // 104
                    74, 68, 63, 57, 50, 44, 38, 32,         // 112
                    29, 26, 23, 19, 16, 14, 11, 7,          // 120
                    6, 6, 6, 6, 6, 6, 6, 6                  // 128
                }
            },
            {
                "4 Reso",
                new byte[]
                {
                    0, 9, 18, 26, 35, 44, 52, 61,           // 8
                    69, 78, 87, 95, 104, 113, 121, 127,     // 16
                    127, 121, 113, 104, 95, 87, 78, 69,     // 24
                    61, 52, 44, 35, 26, 18, 9, 0,           // 32
                    0, 9, 17, 27, 35, 44, 53, 62,           // 40
                    71, 79, 88, 97, 106, 114, 121, 124,     // 48
                    127, 121, 114, 106, 97, 88, 79, 71,     // 56
                    62, 53, 44, 35, 27, 17, 9, 0,           // 64

                    0, 9, 18, 26, 35, 43, 53, 61,           // 72
                    70, 79, 87, 97, 105, 114, 122, 124,     // 80
                    124, 122, 114, 105, 97, 87, 79, 70,     // 88
                    61, 53, 43, 35, 26, 18, 9, 0,           // 96
                    0, 8, 17, 26, 35, 44, 52, 61,           // 104
                    70, 79, 88, 96, 105, 114, 123, 126,     // 112
                    126, 122, 112, 103, 94, 84, 75, 65,     // 120
                    56, 47, 37, 28, 18, 9, 0, 0             // 128
                }
            },
            {
                "Phaser",
                new byte[]
                {
                    70, 70, 70, 70, 70, 70, 70, 70,         // 8
                    70, 70, 70, 70, 70, 70, 70, 70,         // 16
                    70, 70, 70, 70, 70, 70, 70, 70,         // 24
                    70, 70, 70, 70, 70, 71, 71, 72,         // 32
                    75, 78, 80, 83, 87, 94, 99, 109,        // 40
                    114, 119, 122, 124, 124, 127, 127, 127, // 48
                    127, 127, 126, 124, 122, 119, 117, 114, // 56
                    110, 103, 97, 89, 82, 74, 67, 60,       // 64
                    53, 47, 43, 40, 37, 35, 34, 33,         // 72
                    33, 33, 33, 34, 37, 40, 43, 47,         // 80
                    51, 56, 64, 72, 80, 90, 101, 108,       // 88
                    114, 119, 122, 124, 125, 126, 127, 127, // 96
                    126, 125, 124, 122, 120, 116, 115, 108, // 104
                    102, 97, 89, 80, 72, 63, 57, 49,        // 112
                    44, 38, 34, 30, 26, 21, 18, 14,         // 120
                    11, 8, 6, 4, 3, 0, 0, 0                 // 128
                }
            }
            // NOTE: Some formant filter templates are still missing
        };

        public SinglePatchDescriptor Descriptor;

        public SinglePatchGenerator(SinglePatchDescriptor descriptor)
        {
            this.Descriptor = descriptor;

            using (StreamReader sr = new StreamReader(@"Templates.json"))
            using (JsonTextReader reader = new JsonTextReader(sr))
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    //Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                }
                else
                {
                    //Console.WriteLine("Token: {0}", reader.TokenType);
                }
            }
        }

        public SinglePatch Generate()
        {
            SinglePatch single = new SinglePatch();

            single.Common.Name = Descriptor.Name;
            single.Common.Volume = 115;
            single.SingleCommon.NumSources = Descriptor.Sources.Count;
            single.SingleCommon.IsPortamentoEnabled = false;
            single.SingleCommon.PortamentoSpeed = 0;

            single.Sources = new Source[single.SingleCommon.NumSources];

            for (int i = 0; i < single.SingleCommon.NumSources; i++)
            {
                single.Sources[i] = GenerateSource(Descriptor.Sources[i]);
            }

            return single;
        }

        private Source GenerateSource(SourceDescriptor descriptor)
        {
            if (descriptor.WaveNumber == AdditiveKit.WaveNumber)
            {
                return GenerateAdditiveSource(
                    descriptor.WaveformTemplateName, 
                    descriptor.HarmonicLevelTemplateName, 
                    descriptor.HarmonicEnvelopeTemplateName,
                    descriptor.FormantFilterTemplateName);
            }
            else
            {
                return GeneratePCMSource(descriptor.WaveNumber);
            }
        }

        private Source GenerateAdditiveSource(string waveformTemplateName, string harmonicLevelTemplateName, string harmonicEnvelopeTemplateName, string formantFilterTemplateName)
        {
            Source source = new Source();
            source.ZoneLow = 0;
            source.ZoneHigh = 127;
            VelocitySwitchSettings vel = new VelocitySwitchSettings();
            vel.Type = VelocitySwitchType.Off;
            vel.Velocity = 68;
            source.VelocitySwitch = vel;

            source.Volume = 120;
            source.KeyOnDelay = 0;
            source.EffectPath = 1;
            source.BenderCutoff = 12;
            source.BenderPitch = 2;
            source.Pan = PanType.Normal;
            source.NormalPanValue = 0;

            // DCO settings for additive source
            source.DCO.WaveNumber = AdditiveKit.WaveNumber;
            source.DCO.Coarse = 0;
            source.DCO.Fine = 0;
            source.DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            source.DCO.FixedKey = 0; // OFF

            PitchEnvelope pitchEnv = new PitchEnvelope();
            pitchEnv.StartLevel = 0;
            pitchEnv.AttackTime = 4;
            pitchEnv.AttackLevel = 0;
            pitchEnv.DecayTime = 64;
            pitchEnv.LevelVelocitySensitivity = 0;
            pitchEnv.TimeVelocitySensitivity = 0;
            source.DCO.Envelope = pitchEnv;

            // DCF
            source.DCF.IsActive = true;
            source.DCF.Cutoff = 55;
            source.DCF.Resonance = 0;
            source.DCF.Level = 7;
            source.DCF.Mode = FilterMode.LowPass;
            source.DCF.VelocityCurve = 5;

            // DCF Envelope
            FilterEnvelope filterEnv = new FilterEnvelope();
            source.DCF.EnvelopeDepth = 25;
            filterEnv.AttackTime = 0;
            filterEnv.Decay1Time = 120;
            filterEnv.Decay1Level = 63;
            filterEnv.Decay2Time = 80;
            filterEnv.Decay2Level = 63;
            filterEnv.ReleaseTime = 20;
            source.DCF.Envelope = filterEnv;
            // DCF Modulation:
            source.DCF.KSToEnvAttackTime = 0;
            source.DCF.KSToEnvDecay1Time = 0;
            source.DCF.VelocityToEnvDepth = 30;
            source.DCF.VelocityToEnvAttackTime = 0;
            source.DCF.VelocityToEnvDecay1Time = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv = new AmplifierEnvelope();
            ampEnv.AttackTime = 1;
            ampEnv.Decay1Time = 94;
            ampEnv.Decay1Level = 127;
            ampEnv.Decay2Time = 80;
            ampEnv.Decay2Level = 63;
            ampEnv.ReleaseTime = 20;
            source.DCA.Envelope = ampEnv;

            // DCA Modulation
            source.DCA.KeyScaling.Level = 0;
            source.DCA.KeyScaling.AttackTime = 0;
            source.DCA.KeyScaling.Decay1Time = 0;
            source.DCA.KeyScaling.ReleaseTime = 0;

            source.DCA.VelocitySensitivity.Level = 20;
            source.DCA.VelocitySensitivity.AttackTime = 20;
            source.DCA.VelocitySensitivity.Decay1Time = 20;
            source.DCA.VelocitySensitivity.ReleaseTime = 20;

            // Harmonic levels
            if (!string.IsNullOrEmpty(waveformTemplateName))
            {
                int numHarmonics = 64;
                byte[] levels = LeiterEngine.GetHarmonicLevels(waveformTemplateName, numHarmonics, 127);  // levels are 0...127
                source.ADD.SoftHarmonics = levels;

                Console.WriteLine(String.Format("waveform template = {0}", waveformTemplateName));

                /*
                Console.WriteLine(String.Format("{0}, {1} harmonics:", waveformName, numHarmonics));
                for (int i = 0; i < levels.Length; i++)
                {
                    Console.WriteLine(String.Format("{0} = {1}", i + 1, levels[i]));
                }
                */
            }
            else if (!string.IsNullOrEmpty(harmonicLevelTemplateName))
            {
                Console.WriteLine(String.Format("harmonic template = {0}", harmonicLevelTemplateName));

                byte[] softLevels = HarmonicLevelTemplates[harmonicLevelTemplateName].Item1;
                source.ADD.SoftHarmonics = softLevels;

                byte[] loudLevels = HarmonicLevelTemplates[harmonicLevelTemplateName].Item2;
                source.ADD.LoudHarmonics = loudLevels;                
            }
            else
            {
                Console.WriteLine("No template specified for waveform or harmonic levels, using defaults");

                byte[] softLevels = HarmonicLevelTemplates["Init"].Item1;
                source.ADD.SoftHarmonics = softLevels;

                byte[] loudLevels = HarmonicLevelTemplates["Init"].Item2;
                source.ADD.LoudHarmonics = loudLevels;                
            }

            // Harmonic envelopes. Initially assign the same envelope for each harmonic.
            string harmEnvName = harmonicEnvelopeTemplateName;
            if (string.IsNullOrEmpty(harmonicEnvelopeTemplateName))
            {
                harmEnvName = "Init";
            }
            Console.WriteLine(String.Format("harmonic envelope template = {0}", harmEnvName));
            HarmonicEnvelope harmEnv = HarmonicEnvelopeTemplates[harmEnvName];
            for (int i = 0; i < AdditiveKit.NumHarmonics; i++)
            {
                source.ADD.HarmonicEnvelopes[i] = harmEnv;
            }
            
            // Formant filter
            string formantName = formantFilterTemplateName;
            if (string.IsNullOrEmpty(formantFilterTemplateName))
            {
                formantName = "Init";
            }
            Console.WriteLine(String.Format("formant filter template = {0}", formantName));
            source.ADD.FormantFilter = FormantFilterTemplates[formantName];

            return source;
        }

        private Source GeneratePCMSource(int waveNumber)
        {
            Source source = new Source();

            VelocitySwitchSettings vel = new VelocitySwitchSettings();
            vel.Type = VelocitySwitchType.Off;
            vel.Velocity = 68;
            source.VelocitySwitch = vel;

            source.Volume = 120;
            source.KeyOnDelay = 0;
            source.EffectPath = 1;
            source.BenderCutoff = 12;
            source.BenderPitch = 2;
            source.Pan = PanType.Normal;
            source.NormalPanValue = 0;

            // DCO
            source.DCO.WaveNumber = waveNumber;
            source.DCO.Coarse = 0;
            source.DCO.Fine = 0;
            source.DCO.KSPitch = KeyScalingToPitch.ZeroCent;
            source.DCO.FixedKey = 0; // OFF

            PitchEnvelope pitchEnv = new PitchEnvelope();
            pitchEnv.StartLevel = 0;
            pitchEnv.AttackTime = 4;
            pitchEnv.AttackLevel = 0;
            pitchEnv.DecayTime = 64;
            pitchEnv.LevelVelocitySensitivity = 0;
            pitchEnv.TimeVelocitySensitivity = 0;
            source.DCO.Envelope = pitchEnv;

            // DCF
            source.DCF.IsActive = true;
            source.DCF.Cutoff = 55;
            source.DCF.Resonance = 0;
            source.DCF.Level = 7;
            source.DCF.Mode = FilterMode.LowPass;
            source.DCF.VelocityCurve = 5;

            // DCF Envelope
            FilterEnvelope filterEnv = new FilterEnvelope();
            source.DCF.EnvelopeDepth = 25;
            filterEnv.AttackTime = 0;
            filterEnv.Decay1Time = 120;
            filterEnv.Decay1Level = 63;
            filterEnv.Decay2Time = 80;
            filterEnv.Decay2Level = 63;
            filterEnv.ReleaseTime = 20;
            source.DCF.Envelope = filterEnv;

            // DCF Modulation:
            source.DCF.KSToEnvAttackTime = 0;
            source.DCF.KSToEnvDecay1Time = 0;
            source.DCF.VelocityToEnvDepth = 30;
            source.DCF.VelocityToEnvAttackTime = 0;
            source.DCF.VelocityToEnvDecay1Time = 0;

            // DCA Envelope
            AmplifierEnvelope ampEnv = new AmplifierEnvelope();
            ampEnv.AttackTime = 1;
            ampEnv.Decay1Time = 94;
            ampEnv.Decay1Level = 127;
            ampEnv.Decay2Time = 80;
            ampEnv.Decay2Level = 63;
            ampEnv.ReleaseTime = 15;
            source.DCA.Envelope = ampEnv;

            // DCA Modulation
            source.DCA.KeyScaling.Level = 0;
            source.DCA.KeyScaling.AttackTime = 0;
            source.DCA.KeyScaling.Decay1Time = 0;
            source.DCA.KeyScaling.ReleaseTime = 0;

            source.DCA.VelocitySensitivity.Level = 20;
            source.DCA.VelocitySensitivity.AttackTime = 0;
            source.DCA.VelocitySensitivity.Decay1Time = 0;
            source.DCA.VelocitySensitivity.ReleaseTime = 0;

            return source;
        }
    }
}