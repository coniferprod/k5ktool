#!/usr/local/bin/python

import sys
import argparse

from engine import read_bank

def list(filename):
    read_bank(filename)

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Kawai K5000 helper')
    parser.add_argument('command')
    parser.add_argument('-f', '--filename', help='Name of sound bank file')
    args = parser.parse_args()
    print('command = %s' % args.command)

    if args.filename == None:
        print('Need a file name')
        sys.exit(-1)
    else:
        print(args.filename)

    if args.command == 'list':
        list(args.filename)
    else:
        print('unknown command %s' % args.command)
