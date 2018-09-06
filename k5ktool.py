#!/usr/local/bin/python

import sys
import os
import argparse

from engine import read_bank
from engine import POOL_SIZE

def list(filename):
    (bank, free_memory) = read_bank(filename)
    free_percentage = free_memory * 100.0 / float(POOL_SIZE)

    print('"{0}" contains {1} patches. {2} bytes ({3:.1f}% of memory) free.'.format(os.path.basename(filename), bank.patch_count(), free_memory, free_percentage))
    print('number name  	sources  size')
    for p in bank.patches:
        print('{0:4}  {1}  {2}  {3}'.format(p.patch_index, p.patch_name, p.source_types, p.size))

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Kawai K5000 helper')
    parser.add_argument('command')
    parser.add_argument('-f', '--filename', help='Name of sound bank file')
    args = parser.parse_args()

    if args.filename == None:
        print('Need a file name')
        sys.exit(-1)

    if args.command == 'list':
        list(args.filename)
    else:
        print('unknown command %s' % args.command)
