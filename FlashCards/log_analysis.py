import csv
import datetime
import os

LOGDIR = os.path.join(os.path.dirname(os.path.realpath(__file__)), 'participant_logs')

EXCLUDE = ['000A', '000B', '001A', '001B', '002A', '002B', '1004B', '1009B', '1013B', 'README.md']
EXCLUDE = [ex.lower() for ex in EXCLUDE]

def split_row_dict(row):
  start_row = row.copy()
  stop_row = row.copy()

  start_row['Action'] = 'start'
  stop_row['Action'] = 'stop'

  start_row['Time'] = int(row['LookStart'])
  stop_row['Time'] = int(row['LookStop'])

  def time_to_str(time):
    return datetime.datetime.fromtimestamp(int(time) / 1e3).isoformat(' ')

  start_row['TimeReadable'] = time_to_str(start_row['Time'])
  stop_row['TimeReadable'] = time_to_str(stop_row['Time'])

  start_row.pop('LookStart')
  start_row.pop('LookStop')
  start_row.pop('LookDuration')
  stop_row.pop('LookStart')
  stop_row.pop('LookStop')
  stop_row.pop('LookDuration')
  return start_row, stop_row

def read_log(fpath):
  with open(fpath, 'r') as rfile:
    reader = csv.DictReader(rfile)
    rows = []
    for row in reader:
      row['UserID'] = row['UserID'][:-1]
      start_row, stop_row = split_row_dict(row)
      rows.extend([start_row, stop_row])
  return rows

def get_lf_rows():
  lf_rows = []
  for fname in os.listdir(LOGDIR):
    name = fname.lower()
    if [ex for ex in EXCLUDE if ex in name]:
      continue
    fpath = os.path.join(LOGDIR, fname)
    lf_rows.extend(read_log(fpath))
  return lf_rows

def write_as_csv(lf_rows):
  # out_fields = ['UserID', 'Session', 'Group', 'Word', 'LookStart', 'LookStop', 'LookDuration']
  out_fields = ['UserID', 'Session', 'Group', 'Word', 'Action', 'Time', 'TimeReadable']
  with open('lf_fc_logs.csv', 'w') as wfile:
    writer = csv.DictWriter(wfile, fieldnames=out_fields)
    writer.writeheader()
    [writer.writerow(row) for row in lf_rows]

lf_rows = get_lf_rows()
sorted_lf_rows = sorted(lf_rows, key=lambda x: x['Time'])
write_as_csv(sorted_lf_rows)
