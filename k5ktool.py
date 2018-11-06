#!/usr/local/bin/python

import sys
import os
import argparse

from engine import read_bank
from engine import POOL_SIZE
from engine import sysex_message

def list(filename: str):
    """List the contents of the bank with the given filename.
    
    :param filename: The name of the bank file
    :type filename: str
    """

    (bank, free_memory) = read_bank(filename)
    free_percentage = free_memory * 100.0 / float(POOL_SIZE)

    print('"{0}" contains {1} patches. {2} bytes ({3:.1f}% of memory) free.'.format(os.path.basename(filename), bank.patch_count(), free_memory, free_percentage))
    print('number name  	sources  size')
    for p in bank.patches:
        print('{0:4}  {1}  {2}  {3}'.format(p.patch_index, p.patch_name, p.source_types, p.size))

def make_sysex(filename: str, channel: int):
    print('Making SysEx from {0}'.format(filename))
    (bank, free_memory) = read_bank(filename)
    msg = sysex_message('one_single', channel, bytes([0x00]))
    print(msg)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Kawai K5000 helper')
    parser.add_argument('command')
    parser.add_argument('-f', '--filename', help='Name of sound bank file')
    parser.add_argument('-c', '--channel', help='MIDI channel to use (1...16)', type=int)
    parser.add_argument('-p', '--patch', help='Patch number in bank (1...128)', type=int)
    args = parser.parse_args()

    if args.filename == None:
        print('Need a file name')
        sys.exit(-1)

    cmd = args.command
    if cmd == 'list':
        list(args.filename)
    elif cmd == 'sysex':
        ch = 1
        if args.channel == None:
            print('No MIDI channel specified, using 1 as default')
        elif 1 <= args.channel <= 16:
            ch = args.channel 
            make_sysex(args.filename, ch)
        else:
            print('MIDI channel must be in the range 1...16')
            sys.exit(-1)
    else:
        print('unknown command %s' % args.command)
