sendmidi dev $MIDI_PORT_NAME hex syx 40 00 01 00 0a 00 00
# 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 03
# 40 = 
# 00 = MIDI channel 1
# 01 00 0a 00 00 = command to request block single dump from bank A
# last 19 bytes = tone map, with bits set to 1 for each tone you want (seven bits per byte, because of SysEx limitations)
# all tones = 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 7f 03 (last byte has lowest two bits for tones 127 and 128)
# Response is something like: 40 00 20 00 0A 00 00 <tone> <data>
