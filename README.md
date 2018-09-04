# k5ktool

Patch management utilities for the Kawai K5000 Advanced Additive Synthesizer (1996).

## Usage

Requires .NET Core 2.1 or later. To run, issue the following command:

`dotnet run list --filename somebank.kaa`

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
