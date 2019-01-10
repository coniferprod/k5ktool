import Foundation
import Files

let fileSize = 134660
let poolSize = 0x20000
let maxSourceCount = 6
let maxPatchCount = 128

struct Source {
    var isAdditive: Bool
    var additiveKitPointer: UInt32
}

struct Patch {
    var index: UInt
    var tonePointer: UInt32
    var isUsed: Bool
    var sources: [Source] // dynamic; see https://oleb.net/blog/2017/12/fixed-size-arrays/
    
    
}

struct Bank {
    static let maxPatchCount = 128
    static let maxSourceCount = 6
    static let poolSize = 0x20000
    
    var dataPool: [UInt8]
    var patches: [Patch]
    var patchCount: Int
    var base: UInt32
    
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

typealias Pointer = UInt32

// Lots of help from https://appventure.me/2016/07/15/swift3-nsdata-data/

public final class K5KTool {
    private let arguments: [String]

    public init(arguments: [String] = CommandLine.arguments) {
        self.arguments = arguments
    }

    public func run() throws {
        guard arguments.count > 1 else {
            throw Error.missingFileName
        }
        // The first argument is the execution path
        let fileName = arguments[1]
        
        let fileURL = URL.init(fileURLWithPath: fileName)
        // Read the whole file into memory at one (max 128 kB)
        let data = try Data(contentsOf: fileURL, options: .alwaysMapped)

        assert(data.count == fileSize, "Data size mismatch")
        print("Read \(data.count) bytes from file")
        
        let bank = getBank(data: data)
        
        
        
    }
    
    func getBank(data: Data) -> Bank? {
        var offset = 0
        var patchCount = 0
        let pointerWidth = UInt32.byteWidth
        while patchCount < Bank.maxPatchCount {
            let originalOffset = offset
            let tonePointer: UInt32 = data.scanValue(start: 0, length: pointerWidth)
            offset += pointerWidth
            var sourceCount = 0
            var sourcePointers = [UInt32]()
            while sourceCount < Bank.maxSourceCount {
                let sourcePointer: UInt32 = data.scanValue(start: offset, length: pointerWidth)
                sourcePointers.append(sourcePointer.bigEndian)
                sourceCount += 1
                offset += pointerWidth
            }
            print(String(format: "%08X: %03d - %08X", originalOffset, patchCount + 1, tonePointer.bigEndian))
            print("  " + sourcePointers.compactMap { String(format: "%08X", $0) }.joined(separator: " "))
            print()
            patchCount += 1
        }
        
        return nil
    }
}

extension Data {
    func scanValue<T>(start: Int, length: Int) -> T {
        return self.subdata(in: start..<start+length).withUnsafeBytes { $0.pointee }
    }
}

public extension K5KTool {
    enum Error: Swift.Error {
        case missingFileName
        case failedToCreateFile
    }
}

