import pandas as pd

df1 = pd.read_csv('c01_04.csv')
df2 = pd.read_csv('c04_07.csv')
df3 = pd.read_csv('c07_10.csv')
df4 = pd.read_csv('c10_koniec.csv')

df = pd.concat([df1, df2, df3, df4])

df.to_csv('2022.csv', index=False)