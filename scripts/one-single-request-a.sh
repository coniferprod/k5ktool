sendmidi dev $MIDI_PORT_NAME hex syx 40 00 00 00 0a 00 00 00
# 40 = 
# 00 = MIDI channel 1
# 00 = one single dump request from A bank
# 00 00 0a 00 00 = command to request one single dump
# 00 = tone no. (00h - 7fh)
# Response is something like: 40 00 20 00 0A 00 00 <tone> <data>
