import sys
import math
import matplotlib.pyplot as plt

import levels

known_waveforms = ['saw', 'sqr', 'tri', 'sin']
harmonic_count = 64
max_level = 127

def get_level(amplitude):
    """Convert amplitude to synth harmonic level setting."""
    return math.floor(max_level + (8 * math.log2(abs(amplitude))))

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
        print(n, a)
        level = get_level(a)
        levels.append(level)
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
        levels.append(level)
    return levels

def get_custom_levels():
    pass

levels_function_table = {
    'sin': get_sine_levels,
    'saw': get_saw_levels,
    'sqr': get_sqr_levels,
    'tri': get_tri_levels
}

def get_harmonic_levels(waveform):
    func = levels_function_table[waveform]
    return func()

def compute(n, waveform_params):
    (a, b, c, xp, d, e, yp) = waveform_params

    x = n * math.pi * xp
    y = n * math.pi * yp

    module1 = 1.0 / math.pow(n, a)
    module2 = math.pow(math.sin(x), b) * math.pow(math.cos(x), c)
    module3 = math.pow(math.sin(y), d) * math.pow(math.cos(y), e)

    # Should we take absolute values of the sinusoids or not?
    result = module1 * module2 * module3
    #print('    compute = {}'.format(result))
    return result

def get_custom_level(harmonic_number, waveform_params, max_level=127):
    #a_max = compute(1, waveform)
    a_max = 1.0
    a = compute(harmonic_number, waveform_params)
    v = math.log(math.fabs(a / a_max), 2.0)
    #print('a_max = {}, v = {}'.format(a_max, v))
    level = max_level + 8 * v
    if level < 0:
        return 0
    else:
        return math.floor(level)

def get_custom_harmonic_levels(params):
    levels = []
    for h in range(harmonic_count):
        n = h + 1  # harmonic numbers start at 1
        level = get_custom_level(n, params)
        levels.append(level)
    return levels

def main(waveform_name, waveform_params=None):
    levels = []
    if waveform_name == 'custom':
        if waveform_params == None:
            return []
        levels = get_custom_harmonic_levels(waveform_params)
    else:
        levels = get_harmonic_levels(waveform_name)
    for i in range(len(levels)):
        n = i + 1
        print(f'{n:2}: {levels[i]:3}')

    plt.bar(range(64), levels)
    plt.show()

def usage():
    print('usage: python3 harmonics.py waveform')
    print('waveform is one of:', ' '.join(known_waveforms))
    print('or "custom" with a list of comma-separated parameters')

if __name__ == '__main__':
    if len(sys.argv) < 2:
        usage()
        sys.exit(-1)
    else:
        waveform_name = sys.argv[1]
        print('waveform = {}'.format(waveform_name))
        waveform_params = None
        if waveform_name == 'custom':
            if len(sys.argv) < 3:  # custom specified, but no parameters supplied
                usage()
                sys.exit(-1)
            else:
                waveform_param_string = sys.argv[2]
                print('param string = {}'.format(waveform_param_string))
                waveform_value_strings = waveform_param_string.split(',')
                print('values = {}'.format(waveform_value_strings))
                waveform_values = [float(x) for x in waveform_value_strings]
                waveform_params = tuple(waveform_values)
                print('params = {}'.format(waveform_params))
        main(waveform_name, waveform_params)
