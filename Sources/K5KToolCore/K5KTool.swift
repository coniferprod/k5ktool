import Foundation
import Files

let fileSize = 134660

struct Reverb {
    let type: Int  // 0...10
    let dryWet: Int
    let param1: Int
    let param2: Int
    let param3: Int
    let param4: Int
    
    var description: String {
        return "\(Reverb.reverbNames[type]): dry/wet = \(dryWet), param1 = \(param1), param2 = \(param2), param3 = \(param3), param4 = \(param4)"
    }
    
    static let reverbNames = ["Hall 1", "Hall 2", "Hall 3", "Room 1", "Room 2", "Room 3", "Plate 1", "Plate 2", "Plate 3", "Reverse", "Long Delay"]
}

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
        
        return "\(Effect.effectNames[effectIndex]): depth = \(depth), param1 = \(param1), param2 = \(param2), param3 = \(param3), param4 = \(param4)"
    }
    
    // There seems to be a conflict in the manual: there are 37 effect names,
    // but the number of effects is reported to be 36. Cross-check this with
    // the actual synth.
    static let effectNames = ["Early Reflection 1", "Early Reflection 2", "Tap Delay 1", "Tap Delay 2", "Single Delay", "Dual Delay", "Stereo Delay", "Cross Delay", "Auto Pan", "Auto Pan & Delay", "Chorus 1", "Chorus 2", "Chorus 1 & Delay", "Chorus 2 & Delay", "Flanger 1", "Flanger 2", "Flanger 1 & Delay", "Flanger 2 & Delay", "Ensemble", "Ensemble & Delay", "Celeste", "Celeste & Delay", "Tremolo", "Tremolo & Delay", "Phaser 1", "Phaser 2", "Phaser 1 & Delay", "Phaser 2 & Delay", "Rotary", "Autowah", "Bandpass", "Exciter", "Enhancer", "Overdrive", "Distortion", "Overdrive & Delay", "Distortion & Delay"]
}

enum Polyphony: Int {
    case poly = 0
    case solo1 = 1
    case solo2 = 2
    
    var description: String {
        let names = ["POLY1", "SOLO1", "SOLO2"]
        return names[self.rawValue]
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

struct Common {
    var name: String
    var sourceCount: Int
    var reverb: Reverb
    var volume: Int
    var polyphony: Polyphony
    var effectAlgorithm: Int
    var effects: [Effect]
    var geq: GEQ
    
}

struct Patch {
    var common: Common
}

struct Bank {
    var patches: [Patch]
}

typealias Byte = UInt8

struct BankData {
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
    static let polyphonyOffset = 49
    static let effect1Offset = 8
    static let effect2Offset = 14
    static let effect3Offset = 20
    static let effect4Offset = 26
    static let maxEffects = 4
    static let GEQOffset = 32
    
    var dataPool: [Byte]
    
    static func parse(from data: Data) -> Bank {
        func getEffect(number: Int, data: Data) -> Effect {
            var start = 0
            switch number {
            case 1: start = effect1Offset
            case 2: start = effect2Offset
            case 3: start = effect3Offset
            case 4: start = effect4Offset
            default: start = 0
            }
            
            return Effect(
                type: data.getByteAsInt(start: start),
                depth: data.getByteAsInt(start: start + 1),
                param1: data.getByteAsInt(start: start + 2),
                param2: data.getByteAsInt(start: start + 3),
                param3: data.getByteAsInt(start: start + 4),
                param4: data.getByteAsInt(start: start + 5))
        }
        
        var patchPointers = [PatchPointer]()
        var offset = 0
        var patchCount = 0
        let pointerWidth = TonePointer.byteWidth
        while patchCount < maxPatchCount {
            let originalOffset = offset
            let tonePointer: TonePointer = data.scanValue(start: 0, length: pointerWidth)
            offset += pointerWidth
            var sourceCount = 0
            var sourcePointers = [TonePointer]()
            while sourceCount < maxSourceCount {
                let sourcePointer: TonePointer = data.scanValue(start: offset, length: pointerWidth)
                sourcePointers.append(sourcePointer.bigEndian)
                sourceCount += 1
                offset += pointerWidth
            }
            patchCount += 1
            
            patchPointers.append(PatchPointer(index: patchCount, tonePointer: tonePointer.bigEndian, sourcePointers: sourcePointers))
        }
        
        let highMemoryPointer: TonePointer = data.scanValue(start: offset, length: pointerWidth)
        offset += pointerWidth
        
        let dataPool = data.subdata(in: offset ..< offset + poolSize)
        
        // Collect the non-zero tone pointers and the high memory pointer
        var tonePointers = patchPointers.map { $0.tonePointer }.filter { $0 != 0 }
        tonePointers.append(highMemoryPointer)
        
        // Find out which of the non-zero pointers is lowest
        let base = tonePointers.sorted()[0]
        
        // Adjust every pointer down by the base amount
        var adjustedPatchPointers = [PatchPointer]()
        
        var patchIndex = 0
        for patch in patchPointers {
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
            adjustedPatchPointers.append(adjustedPatch)
            patchIndex += 1
        }
        
        // Now we have all the adjusted data pointers, so we can start picking up
        // chunks of data from the big pool based on them.
        
        var patches = [Patch]()
        
        for (index, ptr) in adjustedPatchPointers.enumerated() {
            let dataStart = Int(ptr.tonePointer)
            let sourceCount = data.getByteAsInt(start: dataStart + sourceCountOffset)
            let dataSize = commonDataSize + sourceDataSize * sourceCount + additiveWaveKitSize * ptr.additiveWaveKitCount
            let dataEnd = dataStart + dataSize
            let data = dataPool.subdata(in: dataStart..<dataEnd)
            
            let nameStart = dataStart + nameOffset
            let nameEnd = nameStart + nameSize
            let nameData = data.subdata(in: nameStart..<nameEnd)
            
            let name = String(data: nameData, encoding: .utf8) ?? ""
            
            let volume = data.getByteAsInt(start: volumeOffset)
            let polyphonyRaw = data.getByteAsInt(start: polyphonyOffset)
            let polyphony = Polyphony(rawValue: polyphonyRaw)
            let effectAlgorithm = data.getByteAsInt(start: effectAlgorithmOffset)
            
            let reverb = Reverb(
                type: data.getByteAsInt(start: reverbTypeOffset),
                dryWet: data.getByteAsInt(start: reverbTypeOffset + 1),
                param1: data.getByteAsInt(start: reverbTypeOffset + 2),
                param2: data.getByteAsInt(start: reverbTypeOffset + 3),
                param3: data.getByteAsInt(start: reverbTypeOffset + 4),
                param4: data.getByteAsInt(start: reverbTypeOffset + 5))
            
            var effects = [Effect]()
            for effectNumber in 1...maxEffects {
                let effect = getEffect(number: effectNumber, data: data)
                effects.append(effect)
            }
            
            let geq = GEQ(
                freq1: data.getByteAsInt(start: GEQOffset) - 64,
                freq2: data.getByteAsInt(start: GEQOffset + 1) - 64,
                freq3: data.getByteAsInt(start: GEQOffset + 2) - 64,
                freq4: data.getByteAsInt(start: GEQOffset + 3) - 64,
                freq5: data.getByteAsInt(start: GEQOffset + 4) - 64,
                freq6: data.getByteAsInt(start: GEQOffset + 5) - 64,
                freq7: data.getByteAsInt(start: GEQOffset + 6) - 64)
            
            let common = Common(name: name, sourceCount: sourceCount, reverb: reverb, volume: volume, polyphony: polyphony!, effectAlgorithm: effectAlgorithm, effects: effects, geq: geq)
            
            let patch = Patch(common: common)
            patches.append(patch)
        }
        
        return Bank(patches: patches)
    }
}


// From https://stackoverflow.com/a/47221437/1016326
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
        return sourcePointers.filter { $0 != 0 }.count
    }
    
    func isAdditive(sourceIndex: Int) -> Bool {
        return sourcePointers[sourceIndex] != 0
    }
}


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
        
        let bank = BankData.parse(from: data)
        for (index, patch) in bank.patches.enumerated() {
            let common = patch.common
            print(String(format: "%03d", index + 1), "\(common.name), volume = \(common.volume), polyphony = \(common.polyphony.description), sources = \(common.sourceCount)")
            print("Effect algorithm = \(common.effectAlgorithm + 1)")
            print("Reverb: \(common.reverb.description)")
            
            for i in 0..<common.effects.count {
                let effect = common.effects[i]
                print("Effect #\(i) = \(effect.description)")
            }
            
            print()
        }
    }
}

// From https://appventure.me/2016/07/15/swift3-nsdata-data/
extension Data {
    func scanValue<T>(start: Int, length: Int) -> T {
        return self.subdata(in: start..<start+length).withUnsafeBytes { $0.pointee }
    }
}

// Convenience to get a single byte value as integer
extension Data {
    func getByteAsInt(start: Int) -> Int {
        let rawValue: Byte = self.scanValue(start: start, length: Byte.byteWidth)
        return Int(rawValue)
    }
}

public extension K5KTool {
    enum Error: Swift.Error {
        case missingFileName
        case failedToCreateFile
    }
}
