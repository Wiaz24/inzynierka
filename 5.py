import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_csv('cc2022.csv')

plt.plot(df['datetime'], df['Actual Total Load'])

plt.show()