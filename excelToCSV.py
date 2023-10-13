import pandas as pd

filename = '10_koniec.csv'

df = pd.read_csv(filename)

df['Date'] = pd.to_datetime(df['Date'], format='%Y%m%d')

df.to_csv('c'+filename, index=False)
