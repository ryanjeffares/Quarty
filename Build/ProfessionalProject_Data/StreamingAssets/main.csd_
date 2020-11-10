<CsoundSynthesizer>
<CsoundOptions>
-odac
</CsoundOptions>
<CsInstruments>
sr = 44100
0dbfs = 1
nchnls = 2
ksmps = 32

instr ExamplePlayer
kEnv madsr 0.01, 0.1, 0.1, 0.5
kFreq = cpsmidinn(p4)
aSig pluck kEnv, kFreq, i(kFreq), 0, 6
aSig clfilt aSig, 800, 0, 2
aRevL, aRevR freeverb aSig, aSig, 0.6, 0.9
outs aSig + (aRevL * 0.3), aSig + (aRevR * 0.3)
endin

</CsInstruments>
<CsScore>

</CsScore>
</CsoundSynthesizer>
