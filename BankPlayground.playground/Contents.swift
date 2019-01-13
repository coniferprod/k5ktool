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

let bankFileURL = playgroundSharedDataDirectory.appendingPathComponent("WIZOO.KAA")

let data = try Data(contentsOf: bankFileURL)

enum Bank {
    static let maxPatchCount = 128
    static let maxSourceCount = 6
    static let poolSize = 0x20000
    static let commonDataSize = 82
    static let sourceDataSize = 86
    static let sourceCountOffset = 51
    static let additiveWaveKitSize = 806
    static let nameOffset = 40
    static let nameSize = 8
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
    
    let nameStart = Bank.nameOffset
    let nameEnd = nameStart + Bank.nameSize
    let nameData = data.subdata(in: nameStart..<nameEnd)

    var name = ""
    // ASCII is probably the closest encoding
    if let nameString = String(bytes: nameData, encoding: .ascii) {
        // Copy the characters except the NUL bytes
        for ch in nameString {
            if ch != "\0" {
                name.append(ch)
            }
        }
    }
    //print(String(format: "%03d %s", index, name))
    print(index, name)
    
    
}

