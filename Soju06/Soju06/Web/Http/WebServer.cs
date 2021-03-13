/* ========= Soju06 Web Http Utility =========
 * NAMESPACE: Soju06.Web.Http
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 Web Http Utility ========= */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Soju06.Web.Http {
    public static class Http {
        public const string HttpVersion = "HTTP/1.1";
        public const string ServerName = "Soju's simple web server";
        public const string DefaultContentType = "text/html";
        public const string DefaultCharSet = "UTF-8";
    }

    public class HttpRequest {
        public string Protocol { set; get; }
        public Uri Uri { set; get; }
        public string Method { set; get; }
        public string Url { get { return Uri.LocalPath; } }
        public string UserAgent { set; get; }
        public string ContentType { set; get; }
        public Hashtable Headers { get; set; } = new Hashtable();
        public HttpQuery GetParameters { get; set; }
        public HttpQuery PostParameters { get; set; }
        public string Json { set; get; }
    }

    public class HttpRequestEventArgs : EventArgs {
        public HttpRequest Request { protected set; get; }
        public HttpRequestEventArgs(HttpRequest request) : base() => Request = request;
    }
    public delegate void HttpReqeustEventHandler(object sender, HttpRequestEventArgs e);

    public abstract class HttpServer : IDisposable {
        public event HttpReqeustEventHandler Reqeust;

        public IPEndPoint Address { get; set; }

        public bool IsActive {
            get => isActive && Listener?.Server?.IsBound == true;
        }

        public bool IsPause { get; private set; } = false;

        protected TcpListener Listener { get; set; }
        protected bool isActive = true;

        public Task Start(IPEndPoint address) {
            IsPause = false;
            isActive = false;
            Address = address;
            Listener = new TcpListener(Address);
            Listener?.Stop();
            Listener.Start();
            isActive = true;
            return Task.Run(AsyncAccept);
        }

        protected async void AsyncAccept() {
            while (isActive && Listener != null) {
                if (IsPause) { 
                    await Task.Delay(30); continue; 
                }
                var client = await Listener.AcceptTcpClientAsync();
                if (IsPause) continue;
                else if (!IsActive) return;
                var processor = new HttpProcessor(client, this);
                new Thread(new ThreadStart(processor.Process)) {
                    IsBackground = true
                }.Start();
            }
        }

        public void Pause() => IsPause = true;

        public void Stop() {
            isActive = false;
            IsPause = false;
            Listener?.Stop();
        }

        public void HandleRequest(HttpRequest request, ref HttpResponse response) {
            if (request.Url.ToUpper().Equals("/favicon.ico".ToUpper())) return;
            OnReqeust(new HttpRequestEventArgs(request));
            response.StatusCode = 200;
            response.Contents = Encoding.UTF8.GetBytes(OnResponse(request));
        }

        protected abstract string OnResponse(HttpRequest request);

        protected void OnReqeust(HttpRequestEventArgs e) {
            Reqeust?.Invoke(this, e);
        }

        public void Dispose() {
            isActive = false;
            IsPause = false;
            Listener?.Stop();
        }

        public object Tag { get; set; }
    }

    public class HttpResponse {
        private int _statusCode = 200;

        public int StatusCode {
            set {
                if (value == 404 || value == 500)
                    Connection = "close";
                _statusCode = value;
            }
            get => _statusCode;
        }

        public string StatusString {
            get {
                switch (StatusCode) {
                    case 100: return "Continue"; case 101: return "Switching Protocols";
                    case 102: return "Processing (WebDAV; RFC 2518)"; case 200: return "OK";
                    case 201: return "Created"; case 202: return "Accepted";
                    case 203: return "Non-Authoritative Information (since HTTP/1.1)";
                    case 204: return "No Content"; case 205: return "Reset Content";
                    case 206: return "Partial Content (RFC 7233)"; case 207: return "Multi-Status (WebDAV; RFC 4918)";
                    case 208: return "Already Reported (WebDAV; RFC 5842)";
                    case 226: return "IM Used (RFC 3229)"; case 300: return "Multiple Choices";
                    case 301: return "Moved Permanently"; case 302: return "Found";
                    case 303: return "See Other (since HTTP/1.1)"; case 304: return "Not Modified (RFC 7232)";
                    case 305: return "Use Proxy (since HTTP/1.1)"; case 306: return "Switch Proxy";
                    case 307: return "Temporary Redirect (since HTTP/1.1)"; case 308: return "Permanent Redirect (RFC 7538)";
                    case 400: return "Bad Request"; case 401: return "Unauthorized (RFC 7235)"; case 402: return "Payment Required";
                    case 403: return "Forbidden"; case 404: return "Not Found"; case 405: return "Method Not Allowed";
                    case 406: return "Not Acceptable"; case 407: return "Proxy Authentication Required (RFC 7235)";
                    case 408: return "Request Timeout"; case 409: return "Conflict"; case 410: return "Gone"; case 411: return "Length Required";
                    case 412: return "Precondition Failed (RFC 7232)"; case 413: return "Payload Too Large (RFC 7231)";
                    case 414: return "Request-URI Too Long"; case 415: return "Unsupported Media Type";
                    case 416: return "Requested Range Not Satisfiable (RFC 7233)"; case 417: return "Expectation Failed";
                    case 418: return "I'm a teapot (RFC 2324)"; case 419: return "Authentication Timeout (not in RFC 2616)";
                    case 420: return "Method Failure (Spring Framework)"; case 421: return "Misdirected Request (RFC 7540)";
                    case 422: return "Unprocessable Entity (WebDAV; RFC 4918)"; case 423: return "Locked (WebDAV; RFC 4918)";
                    case 424: return "Failed Dependency (WebDAV; RFC 4918)"; case 426: return "Upgrade Required";
                    case 428: return "Precondition Required (RFC 6585)"; case 429: return "Too Many Requests (RFC 6585)";
                    case 431: return "Request Header Fields Too Large (RFC 6585)";
                    case 440: return "Login Timeout (Microsoft)"; case 444: return "No Response (Nginx)";
                    case 449: return "Retry With (Microsoft)"; case 450: return "Blocked by Windows Parental Controls (Microsoft)";
                    case 451: return "Redirect (Microsoft)"; case 494: return "Request Header Too Large (Nginx)";
                    case 495: return "Cert Error (Nginx)"; case 496: return "No Cert (Nginx)";
                    case 497: return "HTTP to HTTPS (Nginx)"; case 498: return "Token expired/invalid (Esri)";
                    case 499: return "Client Closed Request (Nginx)"; case 500: return "Internal Server Error";
                    case 501: return "Not Implemented"; case 502: return "Bad Gateway";
                    case 503: return "Service Unavailable"; case 504: return "Gateway Timeout";
                    case 505: return "HTTP Version Not Supported"; case 506: return "Variant Also Negotiates (RFC 2295)";
                    case 507: return "Insufficient Storage (WebDAV; RFC 4918)"; case 508: return "Loop Detected (WebDAV; RFC 5842)";
                    case 509: return "Bandwidth Limit Exceeded (Apache bw/limited extension)[83]"; case 510: return "Not Extended (RFC 2774)";
                    case 511: return "Network Authentication Required (RFC 6585)"; case 520: return "Unknown Error";
                    case 522: return "Origin Connection Time-out"; case 598: return "Network read timeout error (Unknown)";
                    case 599: return "Network connect timeout error (Unknown)";
                    default: return string.Empty;
                }
            }
        }

        public List<Cookie> Cookies = new List<Cookie>();

        public string Connection { set; get; }
        public DateTime LastModified { set; get; }
        public string ContentType { set; get; }
        public string CharacterSet { set; get; }
        public byte[] Contents { set; get; }

        public int ContentLength { get { return (Contents != null) ? Contents.Length : 0; } }

        public HttpResponse() {
            ContentType = Http.DefaultContentType;
            CharacterSet = Http.DefaultCharSet;
            LastModified = DateTime.Now;
            Connection = "keep-alive";
        }
    }

    internal class HttpProcessor {
        public TcpClient socket;
        public HttpServer srv;

        private const int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
        private const int BUF_SIZE = 4096;

        public HttpProcessor(TcpClient s, HttpServer srv) {
            socket = s; this.srv = srv;
        }

        private string StreamReadLine(Stream inputStream) {
            int next_char;
            string data = "";
            while (true) {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') break;
                else if (next_char == '\r' || next_char == -1) continue;
                data += Convert.ToChar(next_char);
            } return data;
        }

        public void Process() {
            using (Stream inputStream = new BufferedStream(socket.GetStream())) {
                HttpRequest request = new HttpRequest();
                HttpResponse response = new HttpResponse();
                try {
                    ParseRequest(inputStream, ref request);
                    ReadHeaders(inputStream, ref request);
                    ReadGetRequestParameter(ref request);
                    if (request.Method.Equals("POST"))
                        ReadPostRequestParameter(inputStream, ref request);
                    response = HandleRequest(request);
                } catch (Exception ex) {
                    response.StatusCode = 500;
                    response.Contents = Encoding.UTF8.GetBytes(ex.ToString().Replace("\n", "<br />"));
                }
                SendResponse(socket.Client, response);
                socket.Close();
            }
        }

        protected void ParseRequest(Stream stream, ref HttpRequest request) {
            string req = StreamReadLine(stream);
            string[] tokens = req.Split(' ');
            if (tokens.Length != 3)
                throw new Exception("invalid http request line");
            request.Method = tokens[0].ToUpper();
            request.Uri = new Uri("http://127.0.0.1" + tokens[1]);
            request.Protocol = tokens[2];
        }

        protected void ReadHeaders(Stream stream, ref HttpRequest request) {
            string line;
            while ((line = StreamReadLine(stream)) != null) {
                if (line.Equals("")) return;

                var separator = line.IndexOf(':');
                if (separator == -1)
                    throw new Exception("invalid http header line: " + line);
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' ')) pos++;
                string value = line.Substring(pos, line.Length - pos);
                request.Headers[name] = value;
                if (name.Equals("Content-Type"))
                    request.ContentType = value;
                else if (name.Equals("UserAgent"))
                    request.UserAgent = value;
            }
        }

        protected void ReadGetRequestParameter(ref HttpRequest request) {
            var ub = new UriBuilder(request.Uri);
            request.GetParameters = HttpQuery.ParseQueryString(ub.Query);
        }

        protected void ReadPostRequestParameter(Stream Stream, ref HttpRequest request) {
            using (var ms = new MemoryStream()) {
                if (request.Headers.ContainsKey("Content-Length")) {
                    int content_len;
                    content_len = Convert.ToInt32(request.Headers["Content-Length"]);
                    if (content_len > MAX_POST_SIZE)
                        throw new Exception(string.Format("POST Content-Length({0}) too big for this simple server", content_len));
                    var buf = new byte[BUF_SIZE];
                    var to_read = content_len;
                    while (to_read > 0) {
                        int numread = Stream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                        if (numread == 0) {
                            if (to_read == 0) break;
                            else throw new Exception("client disconnected during post");
                        }
                        to_read -= numread;
                        ms.Write(buf, 0, numread);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                }
                using (var streamReader = new StreamReader(ms)) {
                    string data = streamReader.ReadToEnd();
                    if (request.ContentType.IndexOf("/json") >= 0) request.Json = data;
                    else request.PostParameters = HttpQuery.ParseQueryString(data);
                }
            }
        }

        protected HttpResponse HandleRequest(HttpRequest request) {
            HttpResponse response = new HttpResponse();
            srv.HandleRequest(request, ref response);
            return response;
        }

        protected static void SendResponse(Socket socket, HttpResponse response) {
            var sb = new StringBuilder();
            sb.Append(string.Format("{0} {1} {2}\r\n", Http.HttpVersion, response.StatusCode, response.StatusString));
            sb.Append(string.Format("Date: {0:r}\r\n", DateTime.Now));
            sb.Append(string.Format("Server: {0}\r\n", Http.ServerName));
            foreach (Cookie c in response.Cookies)
                sb.Append(string.Format("Set-Cookie: {0}={1}; path={2}; expires={3}; domain={4}\r\n", 
                    c.Name, c.Value, c.Path, c.Expires, c.Domain));
            sb.Append(string.Format("Connection: {0}\r\n", response.Connection));
            sb.Append(string.Format("Last-Modified: {0:r}\r\n", response.LastModified.ToUniversalTime()));
            sb.Append(string.Format("Content-Type: {0}; charset={1}\r\n", response.ContentType, response.CharacterSet));
            sb.Append(string.Format("Content-Length: {0}\r\n", response.ContentLength));
            sb.Append(string.Format("\r\n"));

            try {
                socket.Send(Encoding.UTF8.GetBytes(sb.ToString()));
                if (response.StatusCode != 200) {
                    string errorMessage = string.Format("<h1>{0} : {1} </h1>", response.StatusCode, response.StatusString);
                    socket.Send(Encoding.UTF8.GetBytes(errorMessage));
                }
                if (response.Contents != null)
                    socket.Send(response.Contents);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
