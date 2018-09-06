NUM_PATCHES = 128
NUM_SOURCES = 6

POOL_SIZE = 0x20000

class Bank:
    def __init__(self):
        self.patches = []
        self.data = None

    def add_patch(self, patch):
        self.patches.append(patch)

    def patch_count(self):
        return len(self.patches)

class Patch:
    def __init__(self):
        self.is_used = False
        self.additive_kit_count = 0
        self.sources = [None] * NUM_SOURCES
        self.tone_pointer = 0
    
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
    print('Reading sound data from bank file %s' % filename)

    bank = Bank()

    with open(filename, 'rb') as f:
        for patch_index in range(0, NUM_PATCHES):
            patch = Patch()
            tone_pointer = int.from_bytes(f.read(4), byteorder='big')
            patch.tone_pointer = tone_pointer
            patch.is_used = tone_pointer != 0
            source_pointers = []
            for source_index in range(0, NUM_SOURCES):
                source_pointer = int.from_bytes(f.read(4), byteorder='big')
                source = Source(pointer=source_pointer)
                if source.is_additive:
                    patch.additive_kit_count += 1
                source_pointers.append(source_pointer)
                patch.add_source(source_index, source)
            if patch.is_used:
                #print('{0}: {1}'.format(patch_index, patch))
                bank.add_patch(patch)

        displacement = int.from_bytes(f.read(4), byteorder='big')
        print("displacement = {0:X}".format(displacement))

        print("bank has {0} patches".format(bank.patch_count()))

        bank.data = f.read(POOL_SIZE)

        tone_pointers = [p.tone_pointer for p in bank.patches]
        tone_pointers.append(displacement)
        print('got {0} tone pointers'.format(len(tone_pointers)))
        sorted_tone_pointers = sorted(tone_pointers)
        print('[{}]'.format(', '.join(hex(x) for x in sorted_tone_pointers)))
        base = sorted_tone_pointers[0]
        print('base = {0:X}'.format(base))

    for p in range(0, bank.patch_count()):
        patch.adjust_pointer(base)
        for s in range(0, len(patch.sources)):
            patch.sources[s].adjust_pointer(base)
        print(patch)
