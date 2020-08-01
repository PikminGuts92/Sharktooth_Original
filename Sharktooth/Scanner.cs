using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;

using PacketDotNet;
using PacketDotNet.Connections;
using PacketDotNet.Connections.Http;
using SharpPcap;

using Newtonsoft.Json;
using SharpPcap.LibPcap;

namespace Sharktooth
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Scanner : IDisposable
    {
        public string ManifestPath { get; set; }
        public string OutputDirectory { get; set; }
        [JsonProperty]
        public List<EclipseFileRequest> EclipseRequests { get; set; } = new List<EclipseFileRequest>();
        [JsonProperty]
        public List<FileRequest> Requests { get; set; } = new List<FileRequest>();
        public ICaptureDevice SelectedDevice { get; set; }
        public ReadOnlyCollection<ICaptureDevice> Devices { get; }
        private TcpConnectionManager ConnectionManager { get; }

        private const int READ_TIMEOUT_MS = 1000;

        public Scanner()
        {
            Devices = CaptureDeviceList.Instance;
            ConnectionManager = new TcpConnectionManager();
        }

        public void AddDevice(string path)
        {
            throw new NotImplementedException();

            /*
            OfflinePacketDevice device = new OfflinePacketDevice(path);
            if (device == null) return;

            this.Devices.Add(device);*/
        }
        
        public void ReadDumpFile(string path)
        {
            throw new NotImplementedException();

            /* // Opens device
            OfflinePacketDevice device = new OfflinePacketDevice(path);
            
            using (PacketCommunicator communicator = device.Open(0x10000, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                //communicator.SetFilter("ip and tcp");
                //communicator.ReceivePackets(0, DispatcherHandler);

                Packet packet;
                communicator.ReceivePacket(out packet);
                
                while (packet != null)
                {
                    if (IsHttpRequest(packet))
                    {
                        HttpRequestDatagram request = packet?.Ethernet?.Ip?.Tcp?.Http as HttpRequestDatagram;
                        ParseHttpRequest(request);
                    }

                    // Gets next packet
                    communicator.ReceivePacket(out packet);
                }
            } */
        }

        public void Start()
        {
            // If no device selected, don't do anything
            if (SelectedDevice == null)
                return;

            SelectedDevice = new CaptureFileReaderDevice(@"F:\Temp\GHTV_Trying.pcap");

            // Start scan
            //Communicator = SelectedDevice.Open(0x10000, PacketDeviceOpenAttributes.Promiscuous, 1000);
            //Communicator.ReceivePackets(-1, ProcessPacket); // Get packets indefinitely

            ConnectionManager.OnConnectionFound += ConnectionManager_OnConnectionFound;
            SelectedDevice.OnPacketArrival += new PacketArrivalEventHandler(SelectedDevice_OnPacketArrival);

            //SelectedDevice.Open(DeviceMode.Promiscuous, READ_TIMEOUT_MS);
            SelectedDevice.Open();
            SelectedDevice.Filter = "ip and tcp"; // Must be set AFTER opening

            //SelectedDevice.StartCapture();

            SelectedDevice.Capture();
        }

        private void ConnectionManager_OnConnectionFound(TcpConnection c)
        {
            var httpSessionWatcher = new HttpSessionWatcher(c, OnHttpRequestFound, OnHttpStatusFound, OnHttpWatcherError);

        }

        private void OnHttpRequestFound(HttpSessionWatcherRequestEventArgs e)
        {
            // only display compressed messages
            if ((e.Request.ContentEncoding == HttpMessage.ContentEncodings.Deflate) ||
               (e.Request.ContentEncoding == HttpMessage.ContentEncodings.Gzip))
            {
                //               log.Info(e.Request.ToString());
                Console.WriteLine(e.Request.ToString());
            }

            // NOTE: Regex on the url can be performed here on
            // e.Request.Url
        }

        private void OnHttpStatusFound(HttpSessionWatcherStatusEventArgs e)
        {
            //Dns.GetHostEntry("");

            // only display compressed messages
            if ((e.Status.ContentEncoding == HttpMessage.ContentEncodings.Deflate) ||
               (e.Status.ContentEncoding == HttpMessage.ContentEncodings.Gzip))
            {
                //                log.Info(e.Status.ToString());
                Console.WriteLine(e.Status.ToString());
            }
        }

        private void OnHttpWatcherError(string errorString)
        {
            // TODO: Handle somehow
        }

        private void SelectedDevice_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = Packet.ParsePacket(e.Device.LinkType, e.Packet.Data);
            var tcpPacket = packet.Extract<TcpPacket>();

            if (tcpPacket is null)
                return;

            ConnectionManager.ProcessPacket(e.Packet.Timeval, tcpPacket);
        }

        public Task StartAsync() => Task.Run(() => Start());

        public void Stop()
        {
            throw new NotImplementedException();

            /*// If already null, do nothing
            if (Communicator is null)
                return;

            // Stop scan
            Communicator.Break();
            Communicator.Dispose();
            Communicator = null;*/
        }

        public Task StopAsync() => Task.Run(() => Stop());

        /*private void ProcessPacket(Packet packet)
        {
            if (packet == null || !IsHttpRequest(packet)) return;
            
            HttpRequestDatagram request = packet?.Ethernet?.Ip?.Tcp?.Http as HttpRequestDatagram;
            ParseHttpRequest(request);
        }

        private bool IsHttpRequest(Packet packet)
        {
            HttpRequestDatagram request = packet?.Ethernet?.Ip?.Tcp?.Http as HttpRequestDatagram;

            return request != null;
        }

        private void ParseHttpRequest(HttpRequestDatagram request)
        {
            if (request.Uri == null || request.Header == null) return;

            FileRequest fileRequest;
            if (request.Uri.Contains('?'))
            {
                string[] pathQuery = request.Uri.Split('?');
                fileRequest = CreateRequest(request.Header["Host"].ValueString, pathQuery[0], pathQuery[1]);
            }
            else
            {
                fileRequest = CreateRequest(request.Header["Host"].ValueString, request.Uri, "");
            }
            
            // Downloads file
            string localPath = GetLocalPath(fileRequest, this.OutputDirectory);

            if (!File.Exists(localPath))
                DownloadFile(fileRequest.FullRequest, localPath);

            UpdateManifest();
        }*/

        private FileRequest CreateRequest(string host, string path, string query)
        {
            NameValueCollection parameters = HttpUtility.ParseQueryString(query);
            FileRequest request;

            if (parameters["eclipseps3"] != null)
            {
                string[] pair = parameters["eclipseps3"].Split('_'); // Time + hash pair

                TokenPair token = new TokenPair(new DateTime(Convert.ToInt64(pair[0])), pair[1]);
                request = EclipseRequests.FirstOrDefault(x => x.Host == host && x.Uri == path);

                if (request == null)
                {
                    request = new EclipseFileRequest(host, path, token);
                    EclipseRequests.Add((EclipseFileRequest)request);
                }
                else
                {
                    TokenPair firstPair = ((EclipseFileRequest)request).Tokens.FirstOrDefault(x => x.TimeStamp == token.TimeStamp && x.Token == token.Token);

                    if (firstPair == null)
                        ((EclipseFileRequest)request).Tokens.Add(token);
                }
            }
            else
            {
                request = Requests.FirstOrDefault(x => x.Host == host && x.Uri == path);

                if (request == null)
                {
                    request = new FileRequest(host, path, query);
                    Requests.Add(request);
                }
            }

            return request;
        }

        private void DownloadFile(string url, string savePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent responseContent = response.Content;
                        byte[] data = responseContent.ReadAsByteArrayAsync().Result;

                        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                        File.WriteAllBytes(savePath, data);
                    }
                }
            }
            catch
            {

            }
        }

        private string GetLocalPath(FileRequest request, string localDir)
        {
            return Path.Combine(localDir, request.Host + request.Uri);
        }

        private void UpdateManifest()
        {
            if (ManifestPath == null) return;

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            try
            {
                File.WriteAllText(ManifestPath, json);
            }
            catch
            {

            }
        }

        private Regex regex = new Regex(@"Host: [^\s]+", RegexOptions.IgnoreCase);
        private string ParseHeader(string tostring)
        {
            // Gets host name from header info
            MatchCollection matches = regex.Matches(tostring);
            return matches[0].Value.Substring(6);
        }

        public void Dispose()
        {
            //Communicator?.Dispose();
        }
    }
}
