// A playground for testing the various functions for reading a K5000 bank.
// It is easier and faster to test here than repeatedly compile the tool.

// Create the ~/Documents/Shared Playground Data directory if necessary
// and put the bank file(s) there.
// See https://medium.com/@vhart/read-write-and-delete-file-handling-from-xcode-playground-abf57e445b4

import Foundation
import Cocoa
import PlaygroundSupport

// https://appventure.me/2016/07/15/swift3-nsdata-data/

extension Data {
    func scanValue<T>(start: Int, length: Int) -> T {
        return self.subdata(in: start..<start+length).withUnsafeBytes { $0.pointee }
    }
}

let bankFileURL = playgroundSharedDataDirectory.appendingPathComponent("EAJ-#01.KAA")

let data = try Data(contentsOf: bankFileURL)

enum Bank {
    static let maxPatchCount = 128
    static let maxSourceCount = 6
    static let poolSize = 0x20000
    static let commonDataSize = 82
    static let sourceDataSize = 86
    static let sourceCountOffset = 51
    static let additiveWaveKitSize = 806
    
    static let commonChecksumOffset = 0
    static let effectAlgorithmOffset = 1
    static let reverbTypeOffset = 2
    static let nameOffset = 40
    static let nameSize = 8
    static let volumeOffset = 48
    static let polyOffset = 49
    static let effect1Offset = 8
    static let effect2Offset = 14
    static let effect3Offset = 20
    static let effect4Offset = 26
    static let maxEffects = 4
    static let GEQOffset = 32
}

let reverbNames = ["Hall 1", "Hall 2", "Hall 3", "Room 1", "Room 2", "Room 3", "Plate 1", "Plate 2", "Plate 3", "Reverse", "Long Delay"]

struct Reverb {
    let type: Int  // 0...10
    let dryWet: Int
    let param1: Int
    let param2: Int
    let param3: Int
    let param4: Int
    
    var description: String {
        return "\(reverbNames[type]): dry/wet = \(dryWet), param1 = \(param1), param2 = \(param2), param3 = \(param3), param4 = \(param4)"
    }
}

// There seems to be a conflict in the manual: there are 37 effect names,
// but the number of effects is reported to be 36. Cross-check this with
// the actual synth.
let effectNames = ["Early Reflection 1", "Early Reflection 2", "Tap Delay 1", "Tap Delay 2", "Single Delay", "Dual Delay", "Stereo Delay", "Cross Delay", "Auto Pan", "Auto Pan & Delay", "Chorus 1", "Chorus 2", "Chorus 1 & Delay", "Chorus 2 & Delay", "Flanger 1", "Flanger 2", "Flanger 1 & Delay", "Flanger 2 & Delay", "Ensemble", "Ensemble & Delay", "Celeste", "Celeste & Delay", "Tremolo", "Tremolo & Delay", "Phaser 1", "Phaser 2", "Phaser 1 & Delay", "Phaser 2 & Delay", "Rotary", "Autowah", "Bandpass", "Exciter", "Enhancer", "Overdrive", "Distortion", "Overdrive & Delay", "Distortion & Delay"]

struct Effect {
    let type: Int
    let depth: Int
    let param1: Int
    let param2: Int
    let param3: Int
    let param4: Int
    
    var description: String {
        print("effect type = \(type)")
        // Hmm, seems like some patches have the effect type value of zero,
        // even when it is illegal (should be 11...47)
        let effectIndex = type == 0 ? type : type - 11  // effects are numbered from 11 to 47
        
        return "\(effectNames[effectIndex]): depth = \(depth), param1 = \(param1), param2 = \(param2), param3 = \(param3), param4 = \(param4)"
    }
}

struct GEQ {
    let freq1: Int
    let freq2: Int
    let freq3: Int
    let freq4: Int
    let freq5: Int
    let freq6: Int
    let freq7: Int
    
    var description: String {
        return "\(freq1) \(freq2) \(freq3) \(freq4) \(freq5) \(freq6) \(freq7)"
    }
}


// https://stackoverflow.com/a/47221437/1016326
extension FixedWidthInteger {
    var byteWidth: Int {
        return self.bitWidth / UInt8.bitWidth
    }
    static var byteWidth: Int {
        return Self.bitWidth / UInt8.bitWidth
    }
}

typealias TonePointer = UInt32

struct PatchPointer {
    var index: Int
    var tonePointer: TonePointer
    var sourcePointers: [TonePointer]
    
    var additiveWaveKitCount: Int {
        // The source pointer for any additive wave kit is non-zero.
        // See https://www.hackingwithswift.com/example-code/language/how-to-count-matching-items-in-an-array
        return sourcePointers.filter { $0 != 0 	}.count
    }
    
    func isAdditive(sourceIndex: Int) -> Bool {
        return sourcePointers[sourceIndex] != 0
    }
}

var patches = [PatchPointer]()

var offset = 0
var patchCount = 0
let pointerWidth = TonePointer.byteWidth
while patchCount < Bank.maxPatchCount {
    let originalOffset = offset
    let tonePointer: TonePointer = data.scanValue(start: originalOffset, length: pointerWidth)
    offset += pointerWidth
    var sourceCount = 0
    var sourcePointers = [TonePointer]()
    while sourceCount < Bank.maxSourceCount {
        let sourcePointer: TonePointer = data.scanValue(start: offset, length: pointerWidth)
        sourcePointers.append(sourcePointer.bigEndian)
        sourceCount += 1
        offset += pointerWidth
    }
    print(String(format: "%08X: %03d - %08X", originalOffset, patchCount + 1, tonePointer.bigEndian))
    print("  " + sourcePointers.compactMap { String(format: "%08X", $0) }.joined(separator: " "))
    print()
    patchCount += 1
    
    patches.append(PatchPointer(index: patchCount, tonePointer: tonePointer.bigEndian, sourcePointers: sourcePointers))
}

let highMemoryPointer: TonePointer = data.scanValue(start: offset, length: pointerWidth)
print(String(format: "high memory pointer = %08X", highMemoryPointer.bigEndian))
offset += pointerWidth

let dataPool = data.subdata(in: offset..<offset+Bank.poolSize)

// Collect the non-zero tone pointers and the high memory pointer
var tonePointers = patches.map { $0.tonePointer }.filter { $0 != 0 }

// Find out which of the non-zero pointers is lowest
let base = tonePointers.sorted()[0]
print(String(format: "base = %08X", base))

print("patch count = \(patches.count)")

// Adjust every pointer down by the base amount
var adjustedPatches = [PatchPointer]()

var patchIndex = 0
for patch in patches {
    var adjustedTonePointer = patch.tonePointer
    if patch.tonePointer != 0 {
        adjustedTonePointer -= base
    }
    
    var adjustedSourcePointers = [TonePointer]()
    for sourcePointer in patch.sourcePointers {
        var adjustedSourcePointer = sourcePointer
        if sourcePointer != 0 {
            adjustedSourcePointer -= base
        }
        adjustedSourcePointers.append(adjustedSourcePointer)
    }

    let adjustedPatch = PatchPointer(index: patchIndex, tonePointer: adjustedTonePointer, sourcePointers: adjustedSourcePointers)
    adjustedPatches.append(adjustedPatch)
    patchIndex += 1
}

// Now we have all the adjusted data pointers, so we can start picking up
// chunks of data from the big pool based on them.

enum Polyphony: Int {
    case poly = 0
    case solo1 = 1
    case solo2 = 2
    
    var description: String {
        let names = ["POLY1", "SOLO1", "SOLO2"]
        return names[self.rawValue]
    }
}



func getByteAsInt(data: Data, start: Int) -> Int {
    let rawValue: UInt8 = data.scanValue(start: start, length: UInt8.byteWidth)
    return Int(rawValue)
}

func getEffect(number: Int, data: Data) -> Effect {
    var start = 0
    switch number {
    case 1: start = Bank.effect1Offset
    case 2: start = Bank.effect2Offset
    case 3: start = Bank.effect3Offset
    case 4: start = Bank.effect4Offset
    default: start = 0
    }
    
    return Effect(
        type: getByteAsInt(data: data, start: start),
        depth: getByteAsInt(data: data, start: start + 1),
        param1: getByteAsInt(data: data, start: start + 2),
        param2: getByteAsInt(data: data, start: start + 3),
        param3: getByteAsInt(data: data, start: start + 4),
        param4: getByteAsInt(data: data, start: start + 5))
}

for (index, patch) in adjustedPatches.enumerated() {
    /*
    print(String(format: "%03d - %08X", p.index, p.tonePointer))
    print("  " + p.sourcePointers.compactMap { String(format: "%08X", $0) }.joined(separator: " "))
    */
    
    let dataStart = Int(patch.tonePointer)
    let sourceCount: UInt8 = dataPool.scanValue(start: dataStart + Bank.sourceCountOffset, length: UInt8.byteWidth)
    let dataSize = Bank.commonDataSize + Bank.sourceDataSize * Int(sourceCount) + Bank.additiveWaveKitSize * patch.additiveWaveKitCount
    let dataEnd = dataStart + dataSize
    let data = dataPool.subdata(in: dataStart..<dataEnd)
    print("patch data size = \(data.count)")
    
    let nameStart = Bank.nameOffset
    let nameEnd = nameStart + Bank.nameSize
    let nameData = data.subdata(in: nameStart..<nameEnd)

    let name = String(data: nameData, encoding: .utf8) ?? ""
    
    let volume = getByteAsInt(data: data, start: Bank.volumeOffset)
    let polyRaw = getByteAsInt(data: data, start: Bank.polyOffset)
    let poly = Polyphony(rawValue: polyRaw)
    print(String(format: "%03d", index), "\(name), volume = \(volume), poly = \(poly!.description), sources = \(sourceCount)")

    let effectAlgorithm = getByteAsInt(data: data, start: Bank.effectAlgorithmOffset)
    print("Effect algorithm = \(effectAlgorithm + 1)")

    let reverb = Reverb(
        type: getByteAsInt(data: data, start: Bank.reverbTypeOffset),
        dryWet: getByteAsInt(data: data, start: Bank.reverbTypeOffset + 1),
        param1: getByteAsInt(data: data, start: Bank.reverbTypeOffset + 2),
        param2: getByteAsInt(data: data, start: Bank.reverbTypeOffset + 3),
        param3: getByteAsInt(data: data, start: Bank.reverbTypeOffset + 4),
        param4: getByteAsInt(data: data, start: Bank.reverbTypeOffset + 5))
    print("Reverb: \(reverb.description)")

    for effectNumber in 1...Bank.maxEffects {
        let effect = getEffect(number: effectNumber, data: data)
        print("effect #\(effectNumber) = \(effect.description)")
    }
    
    let geq = GEQ(
        freq1: getByteAsInt(data: data, start: Bank.GEQOffset) - 64,
        freq2: getByteAsInt(data: data, start: Bank.GEQOffset + 1) - 64,
        freq3: getByteAsInt(data: data, start: Bank.GEQOffset + 2) - 64,
        freq4: getByteAsInt(data: data, start: Bank.GEQOffset + 3) - 64,
        freq5: getByteAsInt(data: data, start: Bank.GEQOffset + 4) - 64,
        freq6: getByteAsInt(data: data, start: Bank.GEQOffset + 5) - 64,
        freq7: getByteAsInt(data: data, start: Bank.GEQOffset + 6) - 64)
    print("GEQ = \(geq.description)")
    print("")
}

