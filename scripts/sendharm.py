import sys
import os

from harmonics import get_harmonic_levels, get_custom_harmonic_levels

def send_harmonics(device_name, channel, levels, group_num=1, dry_run=False):
    for i, h in enumerate(levels):
        cmd = 'sendmidi dev "{}" hex syx'.format(device_name)

        hc = 0x40 + group_num

        msg = [
            0x40, # Kawai ID
            channel,
            0x10, # function number
            0x00, # group number
            0x0a, # machine number
            0x02, # "Single Tone ADD Wave Parameter"
            hc,
            source_number, # 00h ... 05h
            i, 0, 0, h
        ]
        for b in msg:
            cmd += ' {:02x}'.format(b)

        print('Sending harmonic {}/64, command = "{}"'.format(i + 1, cmd))
        if not dry_run:
            os.system(cmd)

known_waveforms = ['saw', 'sqr', 'tri', 'sin']

def usage():
    print('usage: python3 sendharm.py device_name channel source_number waveform_name')
    print('waveform is one of:', ' '.join(known_waveforms))
    print('or "custom" with a list of comma-separated parameters')

if __name__ == '__main__':
    if len(sys.argv) < 5:
        usage()
        sys.exit(-1)

    levels = []
    waveform_name = sys.argv[4]
    waveform_params = None
    if waveform_name == 'custom':
        if len(sys.argv) < 6:  # custom specified, but no parameters supplied
            usage()
            sys.exit(-1)
        else:
            waveform_param_string = sys.argv[5]
            waveform_value_strings = waveform_param_string.split(',')
            waveform_values = [float(x) for x in waveform_value_strings]
            waveform_params = tuple(waveform_values)
            levels = get_custom_harmonic_levels(waveform_params)
    else:
        levels = get_harmonic_levels(waveform_name)

    device_name = sys.argv[1]
    channel = int(sys.argv[2]) - 1  # adjust channel 1...16 to 0...15
    source_number = int(sys.argv[3]) - 1  # adjust source 1...6 to 0...5
    #dry_run = (sys.argv[4] == 'dry')
    dry_run = False

    print('Sending SOFT harmonics')
    send_harmonics(device_name, channel, levels, 1, dry_run)

    print('Sending LOUD harmonics')
    send_harmonics(device_name, channel, levels, 2, dry_run)
