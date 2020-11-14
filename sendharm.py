import sys
import os

from harmonics import get_harmonic_levels

def send_h(device_name, channel, levels, group_num=1, dry_run=False):
    for i, h in enumerate(levels):
        cmd = 'sendmidi dev "{}" hex syx'.format(device_name)

        hc = 0x40 + group_num

        msg = [
            0x40, # Kawai ID
            channel,
            0x10, # function number
            0x00, # group number
            0x0a, # machine number
            0x02,
            hc,
            source_number, i, 0, 0, h
        ]
        for b in msg:
            cmd += ' {:02x}'.format(b)

        if dry_run:
            print(cmd)
        else:
            os.system(cmd)

if __name__ == '__main__':
    waveform = sys.argv[1]
    levels = get_harmonic_levels(waveform)

    device_name = sys.argv[2]
    channel = int(sys.argv[3]) - 1
    source_number = int(sys.argv[4]) + 1
    dry_run = (sys.argv[5] == 'dry')

    print('Sending SOFT harmonics')
    send_h(device_name, channel, levels, 1, dry_run)

    print('Sending LOUD harmonics')
    send_h(device_name, channel, levels, 2, dry_run)
