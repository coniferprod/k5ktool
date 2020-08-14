import math

# waveform: 0 = saw, 1 = square, 2 = triangle

def get_level(harmonic_number, waveform):
    a = leiter(harmonic_number, waveform)
    v = math.log(math.fabs(a), 2.0)
    level = 127 + 8 * v
    #print(f'n = {harmonic_number} a = {a} v = {v} level = {level}')
    if level < 0:
        return 0
    else:
        return level

def leiter(n, waveform):
    values = [
        (1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),  # saw
        (1.0, 1.0, 0.0, 0.5, 0.0, 0.0, 0.0),  # square
        (2.0, 1.0, 0.0, 0.5, 0.0, 0.0, 0.0),  # triangle
        (3.0, 1.0, 0.0, 0.48, 2.0, 0.0, 0.035) # analog-style square 
    ]

    (a, b, c, xp, d, e, yp) = values[waveform]

    x = n * math.pi * xp
    y = n * math.pi * yp

    module1 = 1.0 / math.pow(n, a)
    module2 = math.pow(math.sin(x), b) * math.pow(math.cos(x), c)
    module3 = math.pow(math.sin(y), d) * math.pow(math.cos(y), e)

    # Should we take absolute values of the sinusoids or not?
    result = module1 * module2 * module3
    return result

def chunked(lst, n):
    for i in range(0, len(lst), n):
        yield lst[i:i + n]

waveform_names = ['Sawtooth', 'Square', 'Triangle', 'Analog style square']
for w in range(4):
    print(waveform_names[w])
    levels = []
    for n in range(64):
        harmonic_number = n + 1
        levels.append(get_level(harmonic_number, w))
        #print('{: >2} {: >3}'.format(harmonic_number, math.floor(level)))

    groups = chunked(levels, 16)
    group_index = 0
    for group in groups:
        index_line = ''
        value_line = ''
        for index, value in enumerate(group):
            harmonic_number = group_index * 16 + index + 1
            index_line += '{: >3} '.format(harmonic_number)
            value_line += '{: >3} '.format(math.floor(value))
        print(index_line)
        print(value_line)
        print()
        group_index += 1

    print('----')
    print()
    
