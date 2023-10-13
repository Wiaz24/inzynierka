import pandas as pd

df = pd.read_csv('2022.csv')

df['datetime'] = df.apply(lambda row: pd.to_datetime(row['Date'] + ' ' + row['Hour'], format='%Y-%m-%d %H:%M:%S'), axis=1)

df = df.drop(['Date', 'Hour'], axis=1)

df.to_csv('c2022.csv', index=False)