import math

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

def get_level(harmonic_number, waveform_params, max_level=127):
    #a_max = compute(1, waveform)
    a_max = 1.0
    a = compute(harmonic_number, waveform_params)
    v = math.log(math.fabs(a / a_max), 2.0)
    #print('a_max = {}, v = {}'.format(a_max, v))
    level = max_level + 8 * v
    if level < 0:
        return 0
    else:
        return level
