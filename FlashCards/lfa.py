import pandas
import numpy as np
import json

df = pandas.read_csv('lf_fc_logs.csv')

my_ids = set(np.unique(df[['UserID']]))
with open('ids.json', 'r') as rfile:
  all_ids = set(json.load(rfile))

diff_ids = all_ids - my_ids


start_df = df[df.Action == 'start']
clicked_df = start_df[['UserID', 'Word', 'Action']].groupby(['UserID', 'Word']).agg(['count'])

rdf = df.copy()
rdf.loc[:, 'is_reclick'] = False
for i in range(1, len(rdf)):
  if (rdf.loc[i, 'Word'] == rdf.loc[i - 1, 'Word'] and
      rdf.loc[i, 'Action'] == 'start' and
      rdf.loc[i - 1, 'Action'] == 'stop'):
    rdf.loc[i, 'is_reclick'] = True
rdf = rdf[rdf.is_reclick == True]
reclicks = rdf[['UserID', 'Word', 'is_reclick']].groupby(['UserID', 'Word'])['is_reclick'].count().reset_index(name="reclick_count")
reclicks.to_csv('reclicks.csv', index=False)

max_re = reclicks.groupby(['UserID'])['reclick_count'].max().reset_index(name="mre")
max_re.to_csv('max_reclicks.csv', index=False)
st_thresh = 5.0
st = max_re[max_re.mre >= st_thresh]
no_st = max_re[max_re.mre < st_thresh]

mrpu = reclicks.groupby(['Word'])['reclick_count'].mean().reset_index(name='mean_reclick_per_user')
mrpu.to_csv('mean_reclick_per_user.csv', index=False)
