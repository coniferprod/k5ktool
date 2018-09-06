NUM_PATCHES = 128
NUM_SOURCES = 6

def read_bank(filename):
    print('Reading sound data from bank file %s' % filename)

    with open(filename, 'rb') as f:
        patch_count = 0
        for patch_index in range(0, NUM_PATCHES):
            print("Patch {0}".format(patch_index))
            tone_pointer = int.from_bytes(f.read(4), byteorder='big')
            print("tone = {0:x}".format(tone_pointer))
            source_pointers = []
            for source_index in range(0, NUM_SOURCES):
                source_pointer = int.from_bytes(f.read(4), byteorder='big')
                print("source {0} = {1:x}".format(source_index, source_pointer))
                source_pointers.append(source_pointer)
