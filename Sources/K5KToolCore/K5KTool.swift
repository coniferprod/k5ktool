import Foundation
import Files
import Bitter

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
    var effectAlgorithm: Int
    var reverb: Reverb
    var effects: [Effect]  // 4 values
    // drumMark, always zero for normal patches
    var name: String  // max 8 characters, ASCII
    var volume: Int
    var polyphony: Polyphony
    // byte 50 is unused
    var sourceCount: Int  // "No. of sources: 2~6" - maybe should be 1...6 ?
    var sourceMutes: [Bool]  // lowest six bits indicate sources, bit is 0 if muted but this value is true if source is muted; total six values
    var geq: GEQ
    
}

typealias EnvelopeSegment = (rate: Int, level: Int)
typealias LoopingEnvelopeSegment = (rate: Int, level: Int, loop: Bool)

struct HarmonicEnvelope {
    var segment0: EnvelopeSegment
    var segment1: LoopingEnvelopeSegment
    var segment2: LoopingEnvelopeSegment
    var segment3: EnvelopeSegment
}

typealias HarmonicCopy = (patch: Int, source: Int)

struct HarmonicParameters {
    var totalGain: Int
    
    // Non-MORF paramaters
    var harmonicGroup: Bool  // false = low, true = high
    var keyScalingToGain: Int  // (-63)1 ... (+63)127
    var velocityCurve: Int
    var velocityDepth: Int

    // MORF parameters
    // Harmonic Copy
    var harmonicCopy1: HarmonicCopy
    var harmonicCopy2: HarmonicCopy
    var harmonicCopy3: HarmonicCopy
    var harmonicCopy4: HarmonicCopy
    
    // Harmonic Envelope
    var envelope: EnvelopeSegment
    var loopType: Int  // 0 = off, 1 = LP1, 2 = LP2
}

typealias LFOParameters = (speed: Int, shape: Int, depth: Int)

struct FormantParameters {
    var bias: Int // (-63)1 ... (+63)127
    var envLFOSel: Bool // false = env, true = LFO
    
    // Envelope paramaters
    var envelopeDepth: Int  // (-63)1 ... (+63)127
    var attack: EnvelopeSegment  // rate = 0...127, level = (-63)1 ... (+63)127
    var decay1: EnvelopeSegment
    var decay2: EnvelopeSegment
    var release: EnvelopeSegment
    var loopType: Int // 0 = off, 1 = LP1, 2 = LP2
    var velocitySensitivity: Int // (-63)1 ... (+63)127
    var keyScaling: Int // (-63)1 ... (+63)127
    
    var LFO: LFOParameters  // speed = 0...127; shape = 0/TRI, 1=SAW, 2=RND; depth = 0...63
}

struct AdditiveKit {
    var checksum: Byte
    var morfFlag: Bool  // false = MORF OFF, true = MORF ON
    var harmonics: HarmonicParameters
    var formant: FormantParameters
    var lowHarmonics: [Byte]  // 64 values
    var highHarmonics: [Byte]  // 64 values
    var formantFilterData: [Byte]  // 128 values
    var harmonicEnvelopes: [HarmonicEnvelope]  // 64 values
}

typealias Modulation = (destination: Int, depth: Int)

struct ModulationTarget {
    var target1: Modulation
    var target2: Modulation
}

struct AssignableModulationTarget {
    var source: Int
    var target1: Modulation
    var target2: Modulation
}

struct Envelope {
    var attackTime: Int
    var decay1Time: Int
    var decay1Level: Int
    var decay2Time: Int
    var decay2Level: Int
    var releaseTime: Int
}

typealias FilterKeyScalingToEnvelope = (attackTime: Int, decay1Time: Int)
typealias FilterVelocityToEnvelope = (envelopeDepth: Int, attackTime: Int, decay1Time: Int)

struct Filter {
    var bypass: Bool
    var mode: Int  // 0=low pass, 1=high pass
    var velocityCurve: Int // 0...11 (1...12)
    var resonance: Int // 0...7
    var level: Int // 0...7 (7...0)
    var cutoff: Int // 0...127
    var cutoffKeyScalingDepth: Int // (-63)1 ... (+63)
    var cutoffVelocityDepth: Int // (-63)1 ... (+63)
    var envelopeDepth: Int
    var envelope: Envelope
    var keyScalingToEnvelope: FilterKeyScalingToEnvelope
    var velocityToEnvelope: FilterVelocityToEnvelope
}

typealias AmplifierKeyScalingToEnvelope = (level: Int, attackTime: Int, decay1Time: Int, releaseTime: Int)
typealias AmplifierVelocitySensitivity = (level: Int, attackTime: Int, decay1Time: Int, releaseTime: Int)

struct Amplifier {
    var velocityCurve: Int // 0...11
    var envelope: Envelope
    var keyScalingToEnvelope: AmplifierKeyScalingToEnvelope
    var velocitySensitivity: AmplifierVelocitySensitivity
}

typealias LFOFadeIn = (time: Int, toSpeed: Int)

typealias LFOModulation = (depth: Int, keyScaling: Int)

struct LFO {
    var waveform: Int // 0 = Tri, 1 = Sqr, 2 = Saw, 3 = Sin, 4 = Random
    var speed: Int
    var delayOnset: Int
    var fadeIn: LFOFadeIn
    var vibrato: LFOModulation
    var growl: LFOModulation
    var tremolo: LFOModulation
}

struct Source {
    var zoneLow: Int
    var zoneHigh: Int
    var velocitySwitching: Int // bits 5-6: 0=off, 1=loud, 2=soft. bits 0-4: velo 0=4 ... 31=127 (?)
    var effectPath: Int
    var volume: Int
    var benderPitch: Int
    var benderCutoff: Int
    var pressure: ModulationTarget
    var wheel: ModulationTarget
    var expression: ModulationTarget
    var assignable1: AssignableModulationTarget
    var assignable2: AssignableModulationTarget
    var keyOnDelay: Int
    var panType: Int // 0=normal, 1=KS, 2=-KS, 3=random
    var panValue: Int // (63L)1 ... (+63)127
    
    var filter: Filter
    var amplifier: Amplifier
    var lfo: LFO
}

struct Patch {
    var common: Common
    var sources: [Source]   // six values
    var additiveKit: AdditiveKit
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
    static let sourceMutesOffset = 52
    
    static let harmonicEnvelopeCount = 64
    static let formantFilterBandCount = 128
    
    var dataPool: [Byte]
    
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
        
        for (_, pp) in adjustedPatchPointers.enumerated() {
            let dataStart = Int(pp.tonePointer)
            
            let sourceCount = data.getByteAsInt(start: dataStart + sourceCountOffset)
            let dataSize = commonDataSize + sourceDataSize * sourceCount + additiveWaveKitSize * pp.additiveWaveKitCount
            let dataEnd = dataStart + dataSize
            let data = dataPool.subdata(in: dataStart..<dataEnd)
            
            let nameStart = nameOffset
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
            
            var sourceMutes = [Bool]()
            let sourceMutesValue = data.getByteAsInt(start: sourceMutesOffset)
            // Find out which bits are zero and turn them into Bool/trues.
            // Note we do this starting from the lowest bit.
            sourceMutes.append(sourceMutesValue.b0 == 0)
            sourceMutes.append(sourceMutesValue.b1 == 0)
            sourceMutes.append(sourceMutesValue.b2 == 0)
            sourceMutes.append(sourceMutesValue.b3 == 0)
            sourceMutes.append(sourceMutesValue.b4 == 0)
            sourceMutes.append(sourceMutesValue.b5 == 0)

            let geq = GEQ(
                freq1: data.getByteAsInt(start: GEQOffset) - 64,
                freq2: data.getByteAsInt(start: GEQOffset + 1) - 64,
                freq3: data.getByteAsInt(start: GEQOffset + 2) - 64,
                freq4: data.getByteAsInt(start: GEQOffset + 3) - 64,
                freq5: data.getByteAsInt(start: GEQOffset + 4) - 64,
                freq6: data.getByteAsInt(start: GEQOffset + 5) - 64,
                freq7: data.getByteAsInt(start: GEQOffset + 6) - 64)
            
            let common = Common(effectAlgorithm: effectAlgorithm, reverb: reverb, effects: effects, name: name, volume: volume, polyphony: polyphony!, sourceCount: sourceCount, sourceMutes: sourceMutes, geq: geq)
            
            var sources = [Source]()
            var additiveKit = AdditiveKit(checksum: <#T##Byte#>, morfFlag: <#T##Bool#>, harmonics: <#T##HarmonicParameters#>, formant: <#T##FormantParameters#>, lowHarmonics: <#T##[Byte]#>, highHarmonics: <#T##[Byte]#>, formantFilterData: <#T##[Byte]#>, harmonicEnvelopes: <#T##[HarmonicEnvelope]#>)
            let patch = Patch(common: common, sources: sources, additiveKit: additiveKit)
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
