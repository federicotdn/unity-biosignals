import csv
import numpy as np
import matplotlib.pyplot as plt
import sys

def main():
	basename = sys.argv[1]
	rawBpmsFile = open("../DataSets/SPO2/" + basename + "-RawBpm.csv")
	procBpmsFile = open("../DataSets/SPO2/" + basename + "-ProcBpm.csv")
	peaksFile = open("../DataSets/SPO2/" + basename + "-Peaks.csv")

	rawBpmsReader = csv.reader(rawBpmsFile)
	procBpmsReader = csv.reader(procBpmsFile)
	peaksReader = csv.reader(peaksFile)

	rawBpmsLines = []
	for line in rawBpmsReader:
		rawBpmsLines.append(line)

	procBpmsLines = []
	for line in procBpmsReader:
		procBpmsLines.append(line)

	#BPMS
	
	if int(rawBpmsLines[0][1]) < int(procBpmsLines[0][1]):
		offset = int(rawBpmsLines[0][1])
	else:
		offset = int(procBpmsLines[0][1])

	l1, = plot_bpms(rawBpmsLines, offset, "BPMs brutos")
	l2, = plot_bpms(procBpmsLines, offset, "BPMs procesados")

	plt.legend(handles=[l1, l2])
	plt.xlabel('Segundos')
	plt.ylabel('BPMs')
	plt.show()

def plot_bpms(lines, offset, title):
	bpms_x = []
	bpms_y = []
	lastBpm = -1

	avg = 0

	for line in lines:
		bpm = int(line[0])
		timestamp = int(line[1])
		avg += bpm

		if True:
			bpms_x.append((timestamp - offset) / 10000000)
			bpms_y.append(bpm)
			lastBpm = bpm

	avg /= len(lines)
	print(title + ' average: ' + str(avg))

	return plt.plot(bpms_x, bpms_y, 'p', label=title, marker='.', mew=0.05)

if __name__ == "__main__":
    main()