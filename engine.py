NUM_PATCHES = 128
NUM_SOURCES = 6

POOL_SIZE = 0x20000
SOURCE_COUNT_OFFSET = 51
TONE_COMMON_DATA_SIZE = 82
SOURCE_DATA_SIZE = 86
ADDITIVE_WAVE_KIT_SIZE = 806
NAME_OFFSET = 40
NAME_SIZE = 8

SYSEX_HEADER = [0xF0, 0x40]
SYSEX_FOOTER = 0xF7

class Bank:
    def __init__(self):
        self.patches = []
        self.data = None
        self.base = 0

    def add_patch(self, patch):
        self.patches.append(patch)

    def patch_count(self):
        return len(self.patches)

    def get_patch_data(self, patch_index):
        if self.data == None:
            return None
        patch = self.patches[patch_index]
        patch_start_offset = patch.tone_pointer
        patch_end_offset = patch_start_offset + patch.size
        print('data for patch {0} = [{1} : {2}]'.format(patch_index, patch_start_offset, patch_end_offset))
        return self.data[patch_start_offset : patch_end_offset]

class Patch:
    def __init__(self):
        self.is_used = False
        self.additive_kit_count = 0
        self.sources = [None] * NUM_SOURCES
        self.tone_pointer = 0
        self.source_count = 0
        self.source_types = '-' * NUM_SOURCES
        self.size = 0
        self.patch_name = ''
        self.patch_index = 0
    
    def add_source(self, index, source):
        self.sources[index] = source

    def adjust_pointer(self, displacement: int):
        self.tone_pointer -= displacement

    def __str__(self):
        return 'is_used={0} tone_pointer={1:X}'.format(self.is_used, self.tone_pointer)

class Source:
    def __init__(self, pointer: int):
        self.additive_kit_pointer = pointer
        self.is_additive = self.additive_kit_pointer != 0

    def adjust_pointer(self, displacement: int):
        self.additive_kit_pointer -= displacement

    def __str__(self):
        return 'is_additive={0} additive_kit_pointer={1:X}'.format(self.is_additive, self.additive_kit_pointer)

def read_bank(filename):
    bank = Bank()

    with open(filename, 'rb') as f:
        for patch_index in range(0, NUM_PATCHES):
            patch = Patch()
            tone_pointer = int.from_bytes(f.read(4), byteorder='big')
            patch.tone_pointer = tone_pointer
            patch.is_used = tone_pointer != 0
            patch.patch_index = patch_index + 1
            source_pointers = []
            for source_index in range(0, NUM_SOURCES):
                source_pointer = int.from_bytes(f.read(4), byteorder='big')
                source = Source(pointer=source_pointer)
                if source.is_additive:
                    patch.additive_kit_count += 1
                source_pointers.append(source_pointer)
                patch.add_source(source_index, source)
            if patch.is_used:
                bank.add_patch(patch)

        displacement = int.from_bytes(f.read(4), byteorder='big')

        bank.data = f.read(POOL_SIZE)

        tone_pointers = [p.tone_pointer for p in bank.patches]
        tone_pointers.append(displacement)
        sorted_tone_pointers = sorted(tone_pointers)
        base = sorted_tone_pointers[0]

    for p in range(0, bank.patch_count()):
        patch = bank.patches[p]
        patch.adjust_pointer(base)
        for s in range(0, len(patch.sources)):
            source = patch.sources[s]
            if source.is_additive:
                source.adjust_pointer(base)
    displacement -= base

    for p in range(0, bank.patch_count()):
        patch = bank.patches[p]
        patch.source_count = bank.data[patch.tone_pointer + SOURCE_COUNT_OFFSET]
        patch.size = TONE_COMMON_DATA_SIZE + SOURCE_DATA_SIZE * patch.source_count + ADDITIVE_WAVE_KIT_SIZE * patch.additive_kit_count
        name_offset = patch.tone_pointer + NAME_OFFSET
        name_data = bank.data[name_offset : name_offset + NAME_SIZE]
        patch.patch_name = name_data.decode('utf-8')

    for p in range(0, bank.patch_count()):
        patch = bank.patches[p]
        source_types = ''
        for s in range(0, patch.source_count):
            source = patch.sources[s]
            if source.is_additive:
                source_types += 'A'
            else:
                source_types += 'P'
        while s < NUM_SOURCES:
            source_types += '-'
            s += 1
        patch.source_types = source_types

    return (bank, POOL_SIZE - displacement)

dump_commands = {'one_single': [0x20, 0x00, 0x0A, 0x00, 0x00]}

def sysex_message(command, channel: int, data) -> bytearray:
    msg = bytearray(SYSEX_HEADER)
    msg.append(channel)
    msg.extend(dump_commands[command])
    msg.extend(data)
    msg.append(SYSEX_FOOTER)
    return msg
