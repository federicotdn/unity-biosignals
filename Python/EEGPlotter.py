import csv
import numpy as np
import matplotlib.pyplot as plt

def main():
  with open('/Users/hobbit/projects/ProyectoFinal/DataSets/EEG/test/Subject-1-1.csv', 'r') as csvfile:
    reader = csv.reader(csvfile)
    lines = []
    for row in reader:
      lines.append(row)
      # if firstline:    #skip first line
      #     firstline = False
      #     continue

  x_o = []
  x_c = []
  y_o = []
  y_c = []

  colors = []
  firstline = True

  boxplot_data = [[]]
  previous = int(lines[0][2])
  for line in lines:
    if (previous != int(line[2])):
      boxplot_data.append([])
    o = int(line[2])
    if (o == 0):
      colors.append('r')
      x_c.append(float(line[0]))
      y_c.append(float(line[1]))
    else:
      colors.append('b')
      x_o.append(float(line[0]))
      y_o.append(float(line[1]))
    boxplot_data[-1].append(float(line[0]))
    boxplot_data[-1].append(float(line[1]))
    previous = int(line[2])

  fig = plt.figure()
  ax1 = fig.add_subplot(111)

  ax1.scatter(x_o, y_o, c='b', marker="x", label='Open')
  ax1.scatter(x_c, y_c, c='r', marker="o", label='Closed')
  plt.legend(loc='upper left')

  # ax1.boxplot(boxplot_data)
  # ax1.yaxis.grid(True, linestyle='-', which='major', color='lightgrey',
  #              alpha=0.5)
  # ax1.set_axisbelow(True)
  # i = 0 
  # ticks = []
  # eyes_closed = True
  # while (i < len(boxplot_data)):
  #   if eyes_closed:
  #     ticks.append('Closed')
  #   else:
  #     ticks.append('Open')
  #   eyes_closed = not eyes_closed
  #   i+=1

  # plt.xticks(np.arange(1,len(ticks)), ticks)
  plt.show()



if __name__ == "__main__":
    main()