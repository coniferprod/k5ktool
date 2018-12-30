# k5ktool

Patch management utilities for the 
[Kawai K5000](https://en.wikipedia.org/wiki/Kawai_K5000)
Advanced Additive Synthesizer (1996).

_(work in progress)_

Requires Python 3.6 or later.

## Programming languages

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
even though there is a certain verbosity to Go that does not really feel
too comfortable.

Apart from learning, my earlier language choices reflected the desire to
run the application on both macOS and Windows, and possibly also Linux
(on the Raspberry Pi). This ruled out Swift, which would have been my
first choice in other circumsances. However, as I was writing an iOS 
application in Swift at the same time, it suddently struck me: maybe it
is not so important to be able to run this on many platforms, after all.
What if I just used Swift and got it over with. I would end up with a 
command-line utility for macOS (or, in a pinch, Linux), but I don't think
I care that much, since I will be making most use of it on the Mac anyway.

So, the fourth (and hopefully final) language for this utility is Swift, 
version 4.2. The learning purpose has been fulfilled; I am now better
versed in C#, Python and Go than when I started.

For the Swift version I have been heavily influenced by John Sundell's
article [Building a command line tool using the Swift Package Manager](https://www.swiftbysundell.com/posts/building-a-command-line-tool-using-the-swift-package-manager).

## Usage (Python)

Create a Python virtual environment with `python3 -mvenv venv` and activate it
with `source venv/bin/activate`. Then run the program with `python3 k5ktool.py cmd [options]`.

## The KAA and KA1 file formats

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

### KAA patch bank files

Each bank has a maximum of 128 patches. Each patch can have up to
six sources, which can be either additive or PCM (see the K5000 manual
for details).

At the start of the file is a table of offsets for each patch. Some of the
patches in a bank may be unused, and in that case the offset is zero.
Otherwise it indicates the location of the patch data inside the file,
with an applied displacement.

| Offset | Data |
| ------ | ---- |
| 000000 | Offsets to patch data |

_(to be continued)_
