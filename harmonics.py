import sys
import math

known_waveforms = ['saw', 'sqr', 'tri', 'sin']
harmonic_count = 64
max_level = 127

def get_level(amplitude):
    """ Convert amplitude to synth harmonic level setting."""
    return max_level + (8 * math.log2(abs(amplitude)))

def get_sine_levels():
    """Get list of levels with the first harmonic set to full and the rest to zero."""
    levels = [0] * harmonic_count
    levels[0] = max_level
    return levels

def get_saw_levels():
    """Get level for 1/n for each harmonic."""
    levels = []
    for i in range(harmonic_count):
        n = i + 1  # harmonic numbers start at 1
        a = 1.0 / float(n)
        level = get_level(a)
        levels.append(math.floor(level))
    return levels

def get_sqr_levels():
    """Get the sawtooth levels and take out the even harmonics to get square levels."""
    saw_levels = get_saw_levels()
    levels = []
    for i in range(len(saw_levels)):
        n = i + 1
        level = saw_levels[n] if n % 2 != 0 else 0
        levels.append(level)
    return levels

def get_tri_levels():
    """Get levels for amplitude 1/n^2 for each harmonic n."""
    levels = []
    negative = False  # is current harmonic negative?
    for h in range(harmonic_count):
        n = h + 1  # harmonic numbers start at 1
        level = 0
        if n % 2 != 0: # using only odd harmonics
            a = 1.0 / float(n * n)
            if negative:
                a = -a
                negative = not negative
            level = get_level(a)
        levels.append(math.floor(level))

    return levels

levels_function_table = {
    'sin': get_sine_levels,
    'saw': get_saw_levels,
    'sqr': get_sqr_levels,
    'tri': get_tri_levels
}

def get_harmonic_levels(waveform):
    func = levels_function_table[waveform]
    return func()

def main(waveform):
    levels = get_harmonic_levels(waveform)
    for i in range(len(levels)):
        n = i + 1
        print(f'{n:2}: {levels[i]:3}')

def usage():
    print('usage: python3 harmonics.py waveform')
    print('waveform is one of:', ' '.join(known_waveforms))

if __name__ == '__main__':
    if len(sys.argv) < 2:
        usage()
        sys.exit(-1)
    else:
        waveform = sys.argv[1]
        if not waveform in known_waveforms:
            usage()
            sys.exit(-1)
        main(waveform)
