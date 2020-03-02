import math

# waveform: 0 = saw, 1 = square, 2 = triangle

def get_level(harmonic_number, waveform):
    #a_max = leiter(1, waveform)
    a_max = 1.0
    a = leiter(harmonic_number, waveform)
    v = math.log(math.fabs(a / a_max), 2.0)
    #print('a_max = {}, v = {}'.format(a_max, v))
    level = 127 + 8 * v
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
    #print('    leiter = {}'.format(result))
    return result

waveform_names = ['Sawtooth', 'Square', 'Triangle', 'Analog style square']
for w in range(4):
    print(waveform_names[w])
    for n in range(32):
        level = get_level(n + 1, w)
        print(n + 1, math.floor(level), level)
    print('----')
