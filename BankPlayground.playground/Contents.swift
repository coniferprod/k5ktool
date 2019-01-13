// A playground for testing the various functions for reading a K5000 bank.
// It is easier and faster to test here than repeatedly compile the tool.

// Create the ~/Documents/Shared Playground Data directory if necessary
// and put the bank file(s) there.
// See https://medium.com/@vhart/read-write-and-delete-file-handling-from-xcode-playground-abf57e445b4

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

let tonePointers = [UInt32]()

enum Bank {
    static let maxPatchCount = 128
    static let maxSourceCount = 6
    static let poolSize = 0x20000
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
}

let highMemoryPointer: TonePointer = data.scanValue(start: offset, length: pointerWidth)
print(String(format: "high memory pointer = %08X", highMemoryPointer.bigEndian))
offset += pointerWidth

let dataPool = data.subdata(in: offset..<offset+Bank.poolSize)
