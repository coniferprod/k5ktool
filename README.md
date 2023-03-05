# k5ktool

Patch management utilities for the
[Kawai K5000](https://en.wikipedia.org/wiki/Kawai_K5000)
Advanced Additive Synthesizer (1996).

Uses C# 10.0. Requires .NET 6 or later.

## Command-line arguments with dotnet

To pass command-line arguments to the tool instead of having the `dotnet`
tool intercept them, use the `--` argument, like this:

    dotnet run -- --help

## File formats

The Kawai K5000 patches are usually stored in two binary file formats:

- KAA files are sound banks
- KA1 files are individual sounds

Some utilities can be found online which convert between these formats
and System Exclusive (SysEx), but they are written for MS-DOS or Windows
in the late 1990s and early 2000s, and are often not usable in modern
environments. The source code is available for some of these, and it has
been a huge help in making sense of the KAA and KA1 formats.

The Kawai K5000 MIDI implementation documents the structure of the
single and multi patches and data dumps which appear inside the files,
but does not go into detail about the KAA and KA1 files themselves.

This utility does not deal with the native Kawai K5000 patch formats.
Those formats are effectively a dump of the K5000 internal memory, and
contain pointers to different parts of the data files. Since we can
just as well transfer patches back and forth using MIDI System Exclusive
messages, either individually or as bulk dumps, we can ignore the native
KAA and KA1 formats.

If you need to convert from native formats to System Exclusive,
use [k5ktools](https://github.com/coniferprod/k5ktools).

## Working with MIDI System Exclusive messages

Using the [SendMIDI](https://github.com/gbevin/SendMIDI)
and [ReceiveMIDI](https://github.com/gbevin/ReceiveMIDI) utilities
by [Geert Bevin](https://github.com/gbevin). Refer to the instructions
of these programs for details.

Open two Terminal windows, one for sending and the other for receiving MIDI messages.
You can list the MIDI ports on your system with `sendmidi list`. You should get back something
like this, depending on what you have connected:

    Network Session 1
    Q25
    Steinberg UR22mkII  Port1
    E-MU XMidi2X2 Midi Out 1
    E-MU XMidi2X2 Midi Out 2

Save the name of the port that is connected to your synth in an environment variable available
for shell scripts, for example:

    export MIDI_PORT_NAME="E-MU XMidi2X2 Midi Out 1"

Start receiving from the MIDI port:

    receivemidi dev $MIDI_PORT_NAME

Note that the Kawai K5000 features active sensing, so you will start to see `active-sensing`
messages arriving from the unit.

In the second terminal, send a System Exclusive message with an ID request to the unit:

    sendmidi dev $MIDI_PORT_NAME hex syx 40 00 60

If you're not using MIDI channel 1, replace the `00` with the actual channel (01h/2 ... 0fh/16).

You should see the response from the unit in the output of the `receivemidi` command:

    system-exclusive hex 40 00 61 00 0A 02 dec

This means an ID acknowledge (61h) on channel 1 (00h) from a Kawai K5000S (02h).

## Python scripts

In addition to the C# command-line tool there are some Python scripts that generate harmonic
levels and send commands to the synth using the `sendmidi` utility.

For example, the `sendharm.py` script sends the harmonics for either a pre-defined waveform name
or a custom waveform specified as parameters:

    python3 sendharm.py "U-44 ZOOM U-44 MIDI I/O Port" 1 1 sqr
    python3 sendharm.py "U-44 ZOOM U-44 MIDI I/O Port" 1 1 custom 2.0,2.0,0.0,0.1,0.0,0.0,0.0

If you just want to generate the harmonic levels and inspect them visually, the `harmonics.py`
script does that for both predefined and custom levels, and shows a bar chart using Matplotlib.

The harmonic level computation is adapted from ["Method for Additive Synthesis of
Sound"](https://patents.google.com/patent/US6143974A/en) by Philip Y. Dahl (U.S.
patent 6,143,974; expired in 2019), which is directly related to the Kawai K5000.

## History

I started this utility in C# and .NET Core, mostly to get back on track
with C# and see what .NET Core development with Visual Studio Code would
be like. I soon realized that I wanted to progress with the utility more
than I wanted to (re-)learn C#, and I was also a bit frustrated at how
tall and wide C# actually is. It gains height from the brace
conventions, and width from the nested namespaces and classes.

My second attempt was in Python 3, and it is still present in this
repository. I've never been too comfortable with mid-to-low level
plumbing using Python libraries beyond slurping in files, so it was a
bit of a challenge to pick out individual bytes and words from the binary
source file. I can use Python on a higher level just fine, though.

The third attempt was to learn the Go programming language, since it has
gained a lot of popularity recently (2016-2018), and seemed to fit well
to the task of reading a binary file and processing the information. This
was my second serious attempt at Go, and I got a little further this time.
I was starting to feel almost comfortable with the language constructs,
even though there is a certain C-like terseness to Go that does not really
feel too comfortable, as it results in lots of code lines. This is something
that I really need to think about, as there are many things I like about Go.

Apart from learning, my earlier language choices reflected the desire to
run the application on both macOS and Windows, and possibly also Linux
(on the Raspberry Pi). This ruled out Swift, which would have been my
first choice in other circumstances. However, as I was writing an iOS
application in Swift at the same time, it suddently struck me: maybe it
is not so important to be able to run this on many platforms, after all.
What if I just used Swift and got it over with? I would end up with a
command-line utility for macOS (or, in a pinch, Linux), but I don't think
I care that much, since I will be making most use of it on the Mac anyway.

So, the fourth language for this utility was Swift version 4.2, developed
in Xcode 10. The learning purpose has been fulfilled; I am now better versed
in all of these languages than I was when I started.

For the Swift version I was heavily influenced by John Sundell's
article [Building a command line tool using the Swift Package Manager](https://www.swiftbysundell.com/posts/building-a-command-line-tool-using-the-swift-package-manager).

For reasons I have actually forgotten, I made the decision to go back to
C# for the fifth and final interation. I think it may have had something
to do with the desire to run this utility in Windows, and hopefully develop
a GUI for a K5000 patch editor for Windows 10 as a UWP application.

The actual patch data model has now been moved to a separate
repository, containing a .NET library packaged with NuGet.
