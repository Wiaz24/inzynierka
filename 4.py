import pandas as pd

df = pd.read_csv('c2022.csv')

df['datetime'] = pd.to_datetime(df['datetime'])

for index, row in df.iterrows():
    if(row['datetime'].hour == 00):
        row['datetime'] = row['datetime'] + pd.Timedelta(days=1)

df.to_csv('cc2022.csv', index=False)