from http.server import HTTPServer, BaseHTTPRequestHandler
from urllib.parse import urlparse, parse_qs
import csv
import json
import os
import sys

LOG_DIR = './participant_logs'

class LogServerRequestHandler(BaseHTTPRequestHandler):
	def end_headers (self):
		self.send_header('Access-Control-Allow-Origin', '*')
		BaseHTTPRequestHandler.end_headers(self)

	def do_GET(self):
		query = parse_qs(urlparse(self.path).query)
		print(query)

		self.send_response(200)
		self.end_headers()
		self.wfile.write(bytes("Hello", 'utf-8'))

	def do_POST(self):
		try:
			content_length = int(self.headers['Content-Length'])
			raw_data = self.rfile.read(content_length)
			data = json.loads(str(raw_data, 'utf-8'))
			self.save_logfile(data['log_filename'], data['log_data'])
			self.send_response(200)
			self.end_headers()
			self.wfile.write(bytes('Successfully saved log', 'utf-8'))
		except Exception as e:
			print(e)
			self.send_response(500)
			self.end_headers()
			self.wfile.write(bytes('Failed to save log', 'utf-8'))

	def save_logfile(self, filename, log):
		desired_pathname = os.path.join(LOG_DIR, filename)
		log_pathname = desired_pathname + '.txt'

		counter = 0
		while os.path.isfile(log_pathname):
			counter += 1
			log_pathname = "%s-%d.txt" % (desired_pathname, counter)

		with open(log_pathname, 'w') as wfile:
			wfile.write(log)

if __name__ == "__main__":
	if not os.path.isdir(LOG_DIR):
		print("{} directory needs to be created first!".format(LOG_DIR))
		sys.exit(1)

	PORT = 8080
	if len(sys.argv) == 2:
		PORT = int(sys.argv[1])

	handler = LogServerRequestHandler
	server_address = ('', PORT)
	httpd = HTTPServer(server_address, handler)
	print("Serving at port", PORT)
	httpd.serve_forever()
