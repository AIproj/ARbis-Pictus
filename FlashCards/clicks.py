import pandas
import numpy as np
import json
import sys


df = pandas.read_csv('lf_fc_logs.csv')

my_ids = set(np.unique(df[['UserID']]))

start_df = df[df.Action == 'start']
clicks = start_df[['UserID', 'Word', 'Action']].groupby(['UserID', 'Word'])['Action'].count().reset_index(name='clicks')
clicks.to_csv('clicks_per_user_per_word.csv', index=False)

cperu = clicks.groupby(['UserID']).describe()
cperu.columns = cperu.columns.droplevel(0)
cperu.to_csv('clicks_per_user.csv')

cperw = clicks.groupby(['Word']).describe()
cperw.columns = cperw.columns.droplevel(0)
cperw.to_csv('clicks_per_word.csv')


##########################

start = df[df.Action == 'start']
start = start[['UserID', 'Session', 'Time']]
start = start.groupby(['UserID', 'Session'])['Time'].min().reset_index(name='sess_start')

stop = df[df.Action == 'stop']
stop = stop[['UserID', 'Session', 'Time']]
stop = stop.groupby(['UserID', 'Session'])['Time'].max().reset_index(name='sess_stop')

times = pandas.merge(start, stop, on=['UserID', 'Session'])
times.loc[:, 'phase_duration'] = np.minimum((times.sess_stop - times.sess_start) // 3, 30000)

dft = pandas.merge(df, times, on=['UserID', 'Session'])
dft = dft[dft.Action == 'start']
dft.loc[:, 'sess_phase'] = np.minimum((dft.Time - dft.sess_start) // dft.phase_duration, 2)
dft.loc[:, 'all_phase'] = dft.sess_phase + (3 * (dft.Session - 1))

phtimes = dft[['UserID', 'Action', 'Time', 'Session', 'Word', 'sess_phase', 'all_phase']]
phtimes.to_csv('clicks_with_phases.csv', index=False)

phcl = dft[['UserID', 'sess_phase', 'all_phase']]
sess_phcl = phcl.groupby(['UserID', 'sess_phase'])['all_phase'].count().reset_index(name='num_clicks')
avg_sess_phcl = sess_phcl.groupby(['sess_phase'])['num_clicks'].mean().reset_index(name='avg_clicks')
avg_sess_phcl.to_csv('avg_clicks_per_phase.csv', index=False)
all_phcl = phcl.groupby(['UserID', 'all_phase'])['sess_phase'].count().reset_index(name='num_clicks')
avg_all_phcl = all_phcl.groupby(['all_phase'])['num_clicks'].mean().reset_index(name='avg_clicks')
avg_all_phcl.to_csv('avg_clicks_per_phase_per_session.csv', index=False)

###########################

clks = df[df.Action == 'start']
clks = clks[['UserID', 'Session', 'Word']]
cpsess = clks.groupby(['UserID', 'Session'])['Word'].count().reset_index(name='nc')
avgnc = cpsess.groupby(['Session'])['nc'].mean().reset_index(name='avg_clicks_per_session')
avgnc.to_csv('avg_clicks_per_session.csv', index=False)
